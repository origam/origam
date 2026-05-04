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
        SizeF stringSize = editorGraphics.MeasureString(text: node.LabelText, font: painter.Font);

        Rectangle imageBackground = new Rectangle(
            location: border.Location,
            size: new Size(width: painter.NodeHeaderHeight, height: painter.NodeHeaderHeight)
        );
        Point headerCenter = border.GetCenter();
        var labelPoint = new PointF(
            x: headerCenter.X
                - ((float)border.Width / 2)
                + painter.NodeHeaderHeight
                + painter.TextSideMargin
                + (nodeData.SecondaryImage == null ? 0 : imageBackground.Width - 5),
            y: (float)headerCenter.Y - ((int)stringSize.Height / 2)
        );
        var imageBorder = new Size(
            width: (imageBackground.Width - nodeData.PrimaryImage.Width) / 2,
            height: (imageBackground.Height - nodeData.PrimaryImage.Height) / 2
        );
        var primaryImagePoint = new PointF(
            x: headerCenter.X - ((float)border.Width / 2) + imageBorder.Width,
            y: headerCenter.Y - ((float)border.Height / 2) + imageBorder.Height
        );

        var secondaryImagePoint = new PointF(
            x: headerCenter.X
                - ((float)border.Width / 2)
                + imageBorder.Width
                + imageBackground.Width,
            y: headerCenter.Y - ((float)border.Height / 2) + imageBorder.Height
        );
        editorGraphics.DrawUpSideDown(
            drawAction: graphics =>
            {
                graphics.DrawString(
                    s: node.LabelText,
                    font: painter.Font,
                    brush: painter.GetTextBrush(isFromActivePackage: nodeData.IsFromActivePackage),
                    point: labelPoint,
                    format: painter.DrawFormat
                );
                graphics.FillRectangle(brush: painter.LightGreyBrush, rect: imageBackground);
                graphics.DrawImage(image: nodeData.PrimaryImage, point: primaryImagePoint);
                if (nodeData.SecondaryImage != null)
                {
                    graphics.DrawImage(image: nodeData.SecondaryImage, point: secondaryImagePoint);
                }
            },
            yAxisCoordinate: headerCenter.Y
        );
    }
}
