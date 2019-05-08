using System.Drawing;
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    internal class SubgraphNodePainter : INodeItemPainter
    {
        private readonly InternalPainter painter;
        private readonly NodePainter nodePainter;

        public SubgraphNodePainter(InternalPainter internalPainter)
        {
            painter = internalPainter;
            nodePainter = new NodePainter(internalPainter);
        }
        
        public ICurve GetBoundary(Node node)
        {
            Subgraph subgraph = (Subgraph) node;
            if (!subgraph.Nodes.Any() && ! subgraph.Subgraphs.Any())
            {
                return nodePainter.GetBoundary(node);
            }

            var clusterBoundary =
                ((Cluster) node.GeometryNode).RectangularBoundary;

            var height = clusterBoundary.TopMargin;
            var labelWidth = GetLabelWidth(node);

            var width = clusterBoundary.MinWidth > labelWidth
                ? clusterBoundary.MinWidth
                : labelWidth + painter.LabelSideMargin * 2;

            return CurveFactory.CreateRectangle(width, height, new Point());
        }

        public bool Draw(Node node, object graphicsObj)
        {
            Subgraph subgraph = (Subgraph) node;
            if (!subgraph.Nodes.Any())
            {
                return nodePainter.Draw(node, graphicsObj);
            }

            var borderSize = new Size(
                (int) node.BoundingBox.Width,
                (int) node.BoundingBox.Height);

            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = painter.GetImage(node);

            double centerX = node.GeometryNode.Center.X;
            double centerY = node.GeometryNode.Center.Y;
            var borderCorner = new System.Drawing.Point(
                (int) centerX - borderSize.Width / 2,
                (int) centerY - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                painter.NodeHeight + painter.Margin + painter.TextSideMargin,
                (float) centerY - border.Height / 2.0f + painter.LabelTopMargin);

            Rectangle imageBackground = new Rectangle(borderCorner,
                new Size(painter.NodeHeight, painter.NodeHeight));
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
                    graphics.FillRectangle(painter.GreyBrush, imageBackground);
                    graphics.DrawString(node.LabelText, painter.Font, painter.DrawBrush,
                        labelPoint, painter.DrawFormat);
                    graphics.DrawRectangle(painter.GetActiveBorderPen(node), border);
                    graphics.DrawImage(image, imagePoint);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
        
        private float GetLabelWidth(Node node)
        {
            SizeF stringSize = painter.MeasureString(node.LabelText);
            var labelWidth = stringSize.Width + painter.NodeHeight + painter.Margin + painter.TextSideMargin;
            return labelWidth;
        }
    }
}