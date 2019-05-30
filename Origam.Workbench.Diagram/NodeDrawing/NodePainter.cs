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
        private readonly NodeHeaderPainter nodeHeaderPainter;

        public NodePainter(InternalPainter painter, bool isFromActivePackage)
        {
            this.painter = painter;
            nodeHeaderPainter = new NodeHeaderPainter(painter,isFromActivePackage );
        }
        
        public ICurve GetBoundary(Node node)
        {
            var borderSize = painter.CalculateMinHeaderBorder(node);
            return CurveFactory.CreateRectangle(borderSize.Width,
                borderSize.Height, new Point());
        }

        public bool Draw(Node node, object graphicsObj)
        {
            Graphics editorGraphics = (Graphics) graphicsObj;
            var borderSize = painter.CalculateMinHeaderBorder(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);
          
            nodeHeaderPainter.Draw(node, editorGraphics, border);
            
            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
    }
}