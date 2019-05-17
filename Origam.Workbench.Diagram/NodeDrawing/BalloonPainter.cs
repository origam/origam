using System.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    internal class BalloonPainter: INodeItemPainter
    {
        private readonly InternalPainter painter;
        private readonly SolidBrush balloonBrush;
        private readonly int balloonRadius = 20;

        public BalloonPainter(InternalPainter painter, SolidBrush balloonBrush)
        {
            this.painter = painter;
            this.balloonBrush = balloonBrush;
        }

        public ICurve GetBoundary(Node node)
        {
            return new Ellipse(balloonRadius,balloonRadius,new Point(0,0));
        }

        public bool Draw(Node node, object graphicsObj)
        {
            Graphics editorGraphics = (Graphics) graphicsObj;
            
            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, painter.Font);
            
            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - stringSize.Width / 2,
                (float) node.GeometryNode.Center.Y -
                (int) stringSize.Height / 2);
            var boundingRectangle = new Rectangle(
                (int)node.BoundingBox.LeftBottom.X, 
                (int)node.BoundingBox.LeftBottom.Y, 
                (int)node.BoundingBox.Size.Width, 
                (int)node.BoundingBox.Size.Height);
            
            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.FillEllipse(balloonBrush, boundingRectangle);
                    graphics.DrawString(node.LabelText, painter.Font, painter.WhiteBrush,
                        labelPoint, painter.DrawFormat);
                    graphics.DrawEllipse(painter.BlackPen, boundingRectangle);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
    }
}