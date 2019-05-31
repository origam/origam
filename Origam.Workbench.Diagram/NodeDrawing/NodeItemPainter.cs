using System.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    class NodeItemPainter : INodeItemPainter
    {
        private readonly InternalPainter painter;

        public NodeItemPainter(InternalPainter internalPainter)
        {
            painter = internalPainter;
        }

        public ICurve GetBoundary(Node node)
        {
            INodeData nodeData = (INodeData) node.UserData;
            var borderSize = painter.CalculateMinHeaderBorder(node);
            return CurveFactory.CreateRectangle(borderSize.Width + nodeData.LeftMargin,
                borderSize.Height, new Point());
        }

        public bool Draw(Node node, object graphicsObj)
        {
            INodeData nodeData = (INodeData) node.UserData;
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = painter.GetImages(node).Primary;
            
            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, painter.Font);

            var borderSize = painter.CalculateMinHeaderBorder(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                painter.NodeHeaderHeight +  nodeData.LeftMargin,
                (float) node.GeometryNode.Center.Y -
                (int) stringSize.Height / 2);

            var imageHorizontalBorder =
                (painter.NodeHeaderHeight - image.Width) / 2;
            var imageVerticalBorder =
                (painter.NodeHeaderHeight - image.Height) / 2;
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
                         imageHorizontalBorder) +  nodeData.LeftMargin,
                (float) (node.GeometryNode.Center.Y -
                         (float) border.Height / 2 + imageVerticalBorder));

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, painter.Font,
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