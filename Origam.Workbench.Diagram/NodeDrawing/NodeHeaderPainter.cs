using System.Drawing;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    class NodeHeaderPainter
    {
        private readonly InternalPainter painter;

        public NodeHeaderPainter(InternalPainter painter)
        {
            this.painter = painter;
        }

        public void Draw(Node node, Graphics editorGraphics, Rectangle border)
        {
            INodeData nodeData = (INodeData)node.UserData;

            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, painter.Font);
            
            Rectangle imageBackground = new Rectangle(border.Location,
                new Size(painter.NodeHeaderHeight, painter.NodeHeaderHeight));

            Point headerCenter = border.GetCenter();
            var labelPoint = new PointF(
                headerCenter.X - (float) border.Width / 2 +
                painter.NodeHeaderHeight + painter.TextSideMargin + (nodeData.SecondaryImage == null ? 0 : imageBackground.Width - 5),
                (float) headerCenter.Y -
                (int) stringSize.Height / 2);

            var imageBorder = new Size(
                (imageBackground.Width - nodeData.PrimaryImage.Width) / 2,
                (imageBackground.Height - nodeData.PrimaryImage.Height) / 2);
            var primaryImagePoint = new PointF(
                headerCenter.X - (float) border.Width / 2 +
                imageBorder.Width,
                headerCenter.Y -
                (float) border.Height / 2 + imageBorder.Height);
            
            var secondaryImagePoint = new PointF(
                headerCenter.X - (float) border.Width / 2 +
                imageBorder.Width  + imageBackground.Width,
                headerCenter.Y -
                (float) border.Height / 2 + imageBorder.Height);

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, painter.Font, painter.GetTextBrush(nodeData.IsFromActivePackage),
                        labelPoint, painter.DrawFormat);
                    graphics.FillRectangle(painter.LightGreyBrush, imageBackground);
                    graphics.DrawImage( nodeData.PrimaryImage, primaryImagePoint);
                    if (nodeData.SecondaryImage != null)
                    {
                        graphics.DrawImage(nodeData.SecondaryImage, secondaryImagePoint);
                    }
                },
                yAxisCoordinate: headerCenter.Y);
        }
    }
}