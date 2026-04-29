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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Origam.Extensions;
using Origam.Schema.GuiModel;
using Origam.Schema.GuiModel.Designer;

namespace Origam.Gui.UI;

public sealed class ToolStripActionDropDownButton : ToolStripDropDownButton
{
    private readonly int imageArrowGap = 10;

    public List<ToolStripActionMenuItem> ToolStripMenuItems =>
        DropDownItems.Cast<ToolStripActionMenuItem>().ToList();

    /// <summary>
    /// This constructor should be used for dubugging only
    /// </summary>
    public ToolStripActionDropDownButton()
    {
        ToolStripButtonTools.InitBigButton(actionButton: this);
        Padding = new Padding(left: 0, top: 0, right: 5, bottom: 0);
    }

    public ToolStripActionDropDownButton(EntityDropdownAction dropdownAction)
    {
        AddActionItems(dropdownAction: dropdownAction);
        ToolStripButtonTools.InitBigButton(actionButton: this);
        ToolStripButtonTools.InitActionButton(actionButton: this, action: dropdownAction);
    }

    private void AddActionItems(EntityDropdownAction dropdownAction)
    {
        foreach (var item in dropdownAction.ChildItems)
        {
            if (item is EntityUIAction action)
            {
                DropDownItems.Add(value: new ToolStripActionMenuItem(action: action));
            }
        }
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value.Wrap(widthInPixels: Width, font: Font);
            if (!base.Text.Contains(value: Environment.NewLine))
            {
                base.Text += Environment.NewLine;
            }
            if (base.Text.EndsWith(value: Environment.NewLine))
            {
                base.Text += " ";
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        PaintButtonBackground(e: e);
        ToolStripButtonTools.PaintImage(actionButton: this, e: e);
        this.PaintText(e: e);
        PaintDropDownArrow(e: e);
    }

    private void PaintButtonBackground(PaintEventArgs e)
    {
        var teventArgs = new ToolStripItemRenderEventArgs(g: e.Graphics, item: this);
        Owner.Renderer.DrawDropDownButtonBackground(e: teventArgs);
    }

    private void PaintDropDownArrow(PaintEventArgs e)
    {
        var arrowRectangle = GetArrowRectangle();
        var renderer = this.Owner.Renderer;
        var graphics = e.Graphics;

        var arrowColor = this.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;

        var eventArgs = new ToolStripArrowRenderEventArgs(
            g: graphics,
            toolStripItem: this,
            arrowRectangle: arrowRectangle,
            arrowColor: arrowColor,
            arrowDirection: ArrowDirection.Down
        );

        renderer.DrawArrow(e: eventArgs);
    }

    private Rectangle GetArrowRectangle()
    {
        var imageRectangle = ToolStripButtonTools.GetImageRectangle(actionButton: this);

        var yCoord = imageRectangle.Y + (imageRectangle.Height / 2);
        var xCoord = imageRectangle.X + imageRectangle.Width + imageArrowGap;
        return new Rectangle(
            location: new Point(x: xCoord, y: yCoord),
            size: new Size(width: 5, height: 5)
        ); // looks like the rectangle size has nothing to to do with the arrow size
    }
}

public class ToolStripActionMenuItem : ToolStripMenuItem, IActionContainer
{
    private readonly EntityUIAction action;

    public ToolStripActionMenuItem(EntityUIAction action)
        : base(text: action.Caption)
    {
        this.action = action;
    }

    public EntityUIAction GetAction()
    {
        return action;
    }
}
