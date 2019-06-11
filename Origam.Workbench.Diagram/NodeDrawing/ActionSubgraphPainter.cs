using System.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Origam.Workbench.Diagram.Extensions;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;


namespace Origam.Workbench.Diagram.NodeDrawing
{
    internal class ActionSubgraphPainter: INodeItemPainter
    {
        private readonly InternalPainter painter;
        private readonly double labelLeftMargin = 10;

        public ActionSubgraphPainter(InternalPainter internalPainter)
        {
            painter = internalPainter;
        }

        public ICurve GetBoundary(Node node) 
        {
            var clusterBoundary = ((Cluster) node.GeometryNode).RectangularBoundary;

            var height = clusterBoundary.TopMargin;
            var labelWidth = painter.GetLabelWidth(node);

            var width = clusterBoundary.MinWidth > labelWidth
                ? clusterBoundary.MinWidth 
                : labelWidth + painter.LabelSideMargin * 2;

            return CurveFactory.CreateRectangle(width, height, new Point());
        }

        public bool Draw(Node node, object graphicsObj)
        {
            INodeData nodeData = (INodeData)node.UserData;
            var borderSize = new Size(
                (int)node.BoundingBox.Width,
                (int)node.BoundingBox.Height);

            Graphics editorGraphics = (Graphics)graphicsObj;

            double centerX = node.GeometryNode.Center.X;
            double centerY = node.GeometryNode.Center.Y;

            var labelPoint = new PointF(
                (float)(centerX - borderSize.Width / 2 + labelLeftMargin),
                (float)centerY - borderSize.Height / 2.0f + painter.LabelTopMargin);
            
            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(nodeData.Text, painter.Font,
                        painter.BlackBrush,
                        labelPoint, painter.DrawFormat);
                },
                yAxisCoordinate: (float)node.GeometryNode.Center.Y);

            return true;
        }
    }
}