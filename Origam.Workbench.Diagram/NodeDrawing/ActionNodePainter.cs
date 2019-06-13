using System;
using System.Drawing;
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.Extensions;
using Origam.Workbench.Diagram.Extensions;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    class ActionNodePainter: INodeItemPainter
    {
        private readonly InternalPainter painter;
        private readonly int preferedTextWidth = 35;
        private readonly int imageSize = 24;
        private readonly int textSideMargin = 3;
        private readonly int imageTopMargin = 10;
        private readonly int imageTextGap = 3;
        private readonly int textBottomMargin = 3;

        public ActionNodePainter(InternalPainter internalPainter)
        {
            painter = internalPainter;
        }

        public ICurve GetBoundary(Node node)
        {
            INodeData nodeData = (INodeData) node.UserData;
            var borderSize = CalculateBorder(node);
            return CurveFactory.CreateRectangle(borderSize.Width + nodeData.LeftMargin,
                borderSize.Height, new Point());
        }
        
        private Size CalculateBorder(Node node)
        {
            var nodeData = (INodeData)node.UserData;

            int actualTextWidth = GetTextLines(nodeData)
                .Select(line=>line.Width(painter.Font))
                .Max();

            int totalWidth = textSideMargin + actualTextWidth + textSideMargin;
            int totalHeight = imageTopMargin + imageSize +
                              imageTextGap + painter.Font.Height * 2 +
                              textBottomMargin;

            return new Size(totalWidth, totalHeight);
        }

        private Tuple<int,int> CalculateLabelPointOffsets(int[] lineWidths)
        {
            int widthDifference = Math.Abs(lineWidths[0] - lineWidths[1]);
            int offset = widthDifference / 2;

            return lineWidths[0] > lineWidths[1]
                ? new Tuple<int, int>(0, offset) 
                : new Tuple<int, int>(offset, 0);
        }

        public bool Draw(Node node, object graphicsObj)
        {
            INodeData nodeData = (INodeData) node.UserData;
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = ImageResizer.Resize(nodeData.PrimaryImage, imageSize);

            var borderSize = CalculateBorder(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);
            
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) image.Width / 2),
                    borderCorner.Y+imageTopMargin);
            
            var lines = GetTextLines(nodeData);
            var lineWidths = lines
                .Select(line => line.Width(painter.Font))
                .ToArray();
            int actualTextWidth = lineWidths.Max();
            var (label1XOffset, label2XOffset) = CalculateLabelPointOffsets(lineWidths);

            float textXCoordinate = (float) node.GeometryNode.Center.X - (float) actualTextWidth / 2;
            PointF line1LabelPoint = new PointF(
                textXCoordinate + label1XOffset,
                (float) borderCorner.Y + imageTopMargin + image.Height+ imageTextGap);
    
            PointF line2LabelPoint = new PointF(
                textXCoordinate + label2XOffset, 
                line1LabelPoint.Y+painter.Font.Height);
            
            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.FillRectangle(painter.LightGreyBrush, border);
                    graphics.DrawString(lines[0], painter.Font,
                        painter.GetTextBrush(nodeData.IsFromActivePackage),
                        line1LabelPoint, painter.DrawFormat);
                    graphics.DrawString(lines[1], painter.Font,
                        painter.GetTextBrush(nodeData.IsFromActivePackage),
                        line2LabelPoint, painter.DrawFormat);
                    graphics.DrawImage(image, imagePoint);
                    if (Equals(painter.NodeSelector.Selected, node))
                    {
                        graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                    }
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }

        private string[] GetTextLines(INodeData nodeData)
        {
            return nodeData.Text
                .Wrap(preferedTextWidth, painter.Font)
                .Split("\n")
                .Select(x => x.Trim())
                .ToArray();
        }
    }
}