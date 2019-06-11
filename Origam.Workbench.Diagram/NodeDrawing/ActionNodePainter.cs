using System.Drawing;
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
            
            string wrapedText = nodeData.Text.Wrap(preferedTextWidth, painter.Font);
            int actualTextWidth = wrapedText.Width(painter.Font);

            int totalWidth = textSideMargin + actualTextWidth + textSideMargin;
            int totalHeight = imageTopMargin + imageSize +
                              imageTextGap + painter.Font.Height * 2 +
                              textBottomMargin;

            return new Size(totalWidth, totalHeight);
        }

        public bool Draw(Node node, object graphicsObj)
        {
            INodeData nodeData = (INodeData) node.UserData;
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = ImageResizer.Resize(nodeData.PrimaryImage, imageSize);
            
            string wrapedText = nodeData.Text.Wrap(preferedTextWidth, painter.Font);
            int actualTextWidth = wrapedText.Width(painter.Font);


            var borderSize = CalculateBorder(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) actualTextWidth / 2,
                (float) borderCorner.Y + imageTopMargin + image.Height+ imageTextGap);

            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) image.Width / 2),
                    borderCorner.Y+imageTopMargin);

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.FillRectangle(painter.LightGreyBrush, border);
                    graphics.DrawString(wrapedText, painter.Font,
                        painter.GetTextBrush(nodeData.IsFromActivePackage),
                        labelPoint, painter.DrawFormat);
                    graphics.DrawImage(image, imagePoint);
                    if (Equals(painter.NodeSelector.Selected, node))
                    {
                        graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                    }
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
    }
}