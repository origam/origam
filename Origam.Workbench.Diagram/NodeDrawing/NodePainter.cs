using System.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    internal class NodePainter : INodeItemPainter
    {
        private readonly InternalPainter painter;

        public NodePainter(InternalPainter painter)
        {
            this.painter = painter;
        }
        
        public ICurve GetBoundary(Node node)
        {
            var borderSize = painter.CalculateBorderSize(node);
            return CurveFactory.CreateRectangle(borderSize.Width,
                borderSize.Height, new Point());
        }

        public bool Draw(Node node, object graphicsObj)
        {
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = painter.GetImage(node);

            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, painter.Font);

            var borderSize = painter.CalculateBorderSize(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);
            Rectangle imageBackground = new Rectangle(borderCorner,
                new Size(painter.NodeHeight, painter.NodeHeight));

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                painter.NodeHeight + painter.TextSideMargin,
                (float) node.GeometryNode.Center.Y -
                (int) stringSize.Height / 2);

            var imageHorizontalBorder =
                (imageBackground.Width - image.Width) / 2;
            var imageVerticalBorder =
                (imageBackground.Height - image.Height) / 2;
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
                         imageHorizontalBorder),
                (float) (node.GeometryNode.Center.Y -
                         (float) border.Height / 2 + imageVerticalBorder));

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, painter.Font, painter.BlackBrush,
                        labelPoint, painter.DrawFormat);
                    graphics.FillRectangle(painter.GreyBrush, imageBackground);
                    graphics.DrawImage(image, imagePoint);
                    graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
    }
}