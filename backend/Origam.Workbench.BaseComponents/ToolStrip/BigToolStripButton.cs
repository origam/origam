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

using System;
using System.Drawing;
using System.Windows.Forms;
using Origam.Extensions;

namespace Origam.Gui.UI;

public class BigToolStripButton : ToolStripButton
{
    public BigToolStripButton()
    {
        Font = new Font(familyName: Font.Name, emSize: 8);
        ToolStripButtonTools.InitBigButton(actionButton: this);
    }

    public override string Text
    {
        get => base.Text;
        set => SetTextWithCorrectWidth(value: value);
    }

    private void SetTextWithCorrectWidth(string value)
    {
        string valueWithSpace =
            " "
            + (
                value.EndsWith(value: "...")
                    ? value.Substring(startIndex: 0, length: value.Length - 3)
                    : value
            );
        base.Text = valueWithSpace.Wrap(widthInPixels: Width, font: Font);
        if (!base.Text.Contains(value: Environment.NewLine))
        {
            base.Text += Environment.NewLine;
        }
        if (base.Text.EndsWith(value: Environment.NewLine))
        {
            base.Text += " ";
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        PaintButtonBackground(e: e);
        ToolStripButtonTools.PaintImage(actionButton: this, e: e);
        this.PaintText(e: e);
    }

    private void PaintButtonBackground(PaintEventArgs e)
    {
        var eventArgs = new ToolStripItemRenderEventArgs(g: e.Graphics, item: this);
        Owner.Renderer.DrawButtonBackground(e: eventArgs);
    }
}
