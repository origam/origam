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
using System.Windows.Forms;
using Origam.Extensions;
using Origam.Schema.GuiModel;
using Origam.Workbench.BaseComponents;

namespace Origam.Gui.UI;

internal static class ToolStripButtonTools
{
    private const int ImageTextGap = 0;
    public static readonly Size BUTTON_SIZE = new Size(24, 95);
    private static readonly int defaultImageHeight = 24;

    public static void InitBigButton(ToolStripItem actionButton)
    {
        actionButton.TextAlign = ContentAlignment.BottomCenter;
        actionButton.ImageAlign = ContentAlignment.MiddleCenter;
        actionButton.TextImageRelation = TextImageRelation.ImageAboveText;
        actionButton.AutoSize = true;
        actionButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        actionButton.Margin = new Padding(left: 5, top: 5, right: 5, bottom: 20);
        actionButton.Size = BUTTON_SIZE;
    }

    public static void InitActionButton(ToolStripItem actionButton, EntityUIAction action)
    {
        actionButton.Text = action.Caption;
        actionButton.Image = action.NodeImage.ToBitmap();
    }

    public static void PaintText(this ToolStripItem actionButton, PaintEventArgs e)
    {
        if (actionButton.Owner == null)
            return;

        var renderer = actionButton.Owner.Renderer;

        bool sholudPaintText =
            (actionButton.DisplayStyle & ToolStripItemDisplayStyle.Text)
            == ToolStripItemDisplayStyle.Text;

        if (sholudPaintText)
        {
            renderer.DrawItemText(
                new ToolStripItemTextRenderEventArgs(
                    e.Graphics,
                    actionButton,
                    actionButton.Text,
                    GetTextRectangle(actionButton),
                    actionButton.ForeColor,
                    actionButton.Font,
                    TextFormatFlags.HorizontalCenter
                )
            );
        }
    }

    public static void PaintImage(ToolStripItem actionButton, PaintEventArgs e)
    {
        if (actionButton.Owner == null)
            return;

        var renderer = actionButton.Owner.Renderer;
        actionButton.Image = GetImage(actionButton);

        bool shouldPaintImage =
            (actionButton.DisplayStyle & ToolStripItemDisplayStyle.Image)
            == ToolStripItemDisplayStyle.Image;

        if (shouldPaintImage)
        {
            renderer.DrawItemImage(
                new ToolStripItemImageRenderEventArgs(
                    e.Graphics,
                    actionButton,
                    GetImageRectangle(actionButton)
                )
            );
        }
    }

    private static Image GetImage(ToolStripItem actionButton)
    {
        return actionButton.Image ?? ImageRes.UnknownIcon;
    }

    public static Rectangle GetImageRectangle(ToolStripItem actionButton)
    {
        Image image = GetImage(actionButton);
        var xCoord = (actionButton.Width - image.Size.Width) / 2;
        var yCoord = actionButton.Margin.Top + (defaultImageHeight - image.Height) / 2;
        return new Rectangle(new Point(xCoord, yCoord), image.Size);
    }

    private static Rectangle GetTextRectangle(this ToolStripItem actionButton)
    {
        var textHeight = actionButton.Text.Height(actionButton.Font);
        var textWidth = actionButton.Text.Width(actionButton.Font);

        var yCoord = actionButton.Margin.Top + ImageTextGap + textHeight;
        var xCoord = (actionButton.Width - textWidth) / 2;
        return new Rectangle(xCoord, yCoord, textWidth, textHeight);
    }
}
