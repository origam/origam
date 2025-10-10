#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Drawing;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.NodeDrawing;

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
        SizeF stringSize = editorGraphics.MeasureString(node.LabelText, painter.Font);

        Rectangle imageBackground = new Rectangle(
            border.Location,
            new Size(painter.NodeHeaderHeight, painter.NodeHeaderHeight)
        );
        Point headerCenter = border.GetCenter();
        var labelPoint = new PointF(
            headerCenter.X
                - (float)border.Width / 2
                + painter.NodeHeaderHeight
                + painter.TextSideMargin
                + (nodeData.SecondaryImage == null ? 0 : imageBackground.Width - 5),
            (float)headerCenter.Y - (int)stringSize.Height / 2
        );
        var imageBorder = new Size(
            (imageBackground.Width - nodeData.PrimaryImage.Width) / 2,
            (imageBackground.Height - nodeData.PrimaryImage.Height) / 2
        );
        var primaryImagePoint = new PointF(
            headerCenter.X - (float)border.Width / 2 + imageBorder.Width,
            headerCenter.Y - (float)border.Height / 2 + imageBorder.Height
        );

        var secondaryImagePoint = new PointF(
            headerCenter.X - (float)border.Width / 2 + imageBorder.Width + imageBackground.Width,
            headerCenter.Y - (float)border.Height / 2 + imageBorder.Height
        );
        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.DrawString(
                    node.LabelText,
                    painter.Font,
                    painter.GetTextBrush(nodeData.IsFromActivePackage),
                    labelPoint,
                    painter.DrawFormat
                );
                graphics.FillRectangle(painter.LightGreyBrush, imageBackground);
                graphics.DrawImage(nodeData.PrimaryImage, primaryImagePoint);
                if (nodeData.SecondaryImage != null)
                {
                    graphics.DrawImage(nodeData.SecondaryImage, secondaryImagePoint);
                }
            },
            yAxisCoordinate: headerCenter.Y
        );
    }
}
