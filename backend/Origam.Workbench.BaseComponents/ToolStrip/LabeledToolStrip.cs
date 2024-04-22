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

namespace Origam.Gui.UI;

public class LabeledToolStrip : ToolStrip
{
    private const int BottomTextMarin = 3;
    private readonly SolidBrush foreColorBrush;
    public IToolStripContainer Owner { get; }

    public LabeledToolStrip(IToolStripContainer owner)
    {
            MinimumSize = new Size(0, ToolStripButtonTools.BUTTON_SIZE.Height);
            foreColorBrush = new SolidBrush(ForeColor);
            Renderer = new SideBorderOnlyStripRenderer();
            Visible = false;
            Owner = owner;
        }

    protected override void OnItemAdded(ToolStripItemEventArgs e)
    {
            base.OnItemAdded(e);
            Visible = true;
        }

    protected override void OnItemRemoved(ToolStripItemEventArgs e)
    {
            base.OnItemRemoved(e);
            if (Items.Count == 0)
            {
                Visible = false;
            }
        }

    protected override void OnPaint(PaintEventArgs e)
    {
            base.OnPaint(e);

            int textX = (Size.Width - Text.Width(Font)) / 2;
            int textY = Size.Height - Text.Height(Font) - BottomTextMarin;

            var LabelFont = new Font(Font.Name, 8, FontStyle.Bold);
            e.Graphics.DrawString(Text, LabelFont, foreColorBrush, textX, textY);
        }
}

internal class SideBorderOnlyStripRenderer : ToolStripProfessionalRenderer 
{
    protected override void OnRenderToolStripBorder(
        ToolStripRenderEventArgs e)
    {
            base.OnRenderToolStripBorder(e);
            var rectangle = new Rectangle(
                e.AffectedBounds.Location.X-5,
                e.AffectedBounds.Location.Y - 5,
                e.AffectedBounds.Width + 5,
                e.AffectedBounds.Height + 10);
            ControlPaint.DrawBorder(e.Graphics,
                bounds: rectangle,
                leftColor: SystemColors.ControlDarkDark,
                leftWidth: 2,
                leftStyle: ButtonBorderStyle.Solid,
                topColor: SystemColors.ControlDarkDark,
                topWidth: 2,
                topStyle: ButtonBorderStyle.Solid,
                rightColor: SystemColors.ControlDark,
                rightWidth: 1,
                rightStyle: ButtonBorderStyle.Solid,
                bottomColor: SystemColors.ControlDarkDark,
                bottomWidth: 2,
                bottomStyle: ButtonBorderStyle.Solid);
           
        }

    protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
    {
            // we want no grip
        }
}