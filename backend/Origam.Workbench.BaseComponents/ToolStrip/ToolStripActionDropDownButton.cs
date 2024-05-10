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

namespace Origam.Gui.UI
{
    
    public sealed class ToolStripActionDropDownButton : ToolStripDropDownButton
    {
        private readonly int imageArrowGap = 10;
        
        public List<ToolStripActionMenuItem> ToolStripMenuItems 
            => DropDownItems.Cast<ToolStripActionMenuItem>().ToList();

        /// <summary>
        /// This constructor should be used for dubugging only
        /// </summary>
        public ToolStripActionDropDownButton()
        {
            ToolStripButtonTools.InitBigButton(this);
            Padding = new Padding(
                left: 0,
                top: 0,
                right: 5,
                bottom: 0);
        }

        public ToolStripActionDropDownButton(EntityDropdownAction dropdownAction)
        {
            AddActionItems(dropdownAction);
            ToolStripButtonTools.InitBigButton(this);
            ToolStripButtonTools.InitActionButton(this, dropdownAction);
        }

        private void AddActionItems(EntityDropdownAction dropdownAction)
        {
            foreach (var item in dropdownAction.ChildItems)
            {
                if (item is EntityUIAction action)
                {
                    DropDownItems.Add(new ToolStripActionMenuItem(action));
                }
            }
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value.Wrap(Width, Font);
                if (!base.Text.Contains(Environment.NewLine))
                {
                    base.Text += Environment.NewLine;
                }
                if (base.Text.EndsWith(Environment.NewLine))
                {
                    base.Text += " ";
                }
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            PaintButtonBackground(e);
            ToolStripButtonTools.PaintImage(this,e);
            this.PaintText(e);
            PaintDropDownArrow(e);
        }

        private void PaintButtonBackground(PaintEventArgs e)
        {
            var teventArgs = new ToolStripItemRenderEventArgs(e.Graphics, this);
            Owner
                .Renderer
                .DrawDropDownButtonBackground(teventArgs);
        }

        private void PaintDropDownArrow(PaintEventArgs e)
        {
            var arrowRectangle = GetArrowRectangle();
            var renderer = this.Owner.Renderer;
            var graphics = e.Graphics;
            
            var arrowColor = this.Enabled ? 
                SystemColors.ControlText :
                SystemColors.ControlDark;
            
            var eventArgs = new ToolStripArrowRenderEventArgs(
                g: graphics,
                toolStripItem: this, 
                arrowRectangle: arrowRectangle,
                arrowColor: arrowColor,
                arrowDirection: ArrowDirection.Down);
            
            renderer.DrawArrow(eventArgs);
        }

        private Rectangle GetArrowRectangle()
        {
            var imageRectangle =
                ToolStripButtonTools.GetImageRectangle(this);
            
            var yCoord = imageRectangle.Y + imageRectangle.Height/ 2;
            var xCoord = imageRectangle.X + imageRectangle.Width + imageArrowGap;

            return  new Rectangle(
                new Point(xCoord, yCoord),
                new Size(5, 5)); // looks like the rectangle size has nothing to to do with the arrow size
        }
    }
    
    public class ToolStripActionMenuItem:ToolStripMenuItem, IActionContainer
    {

        private readonly EntityUIAction action;

        public ToolStripActionMenuItem(EntityUIAction action) 
            : base(action.Caption)
        {
            this.action = action;
        }

        public EntityUIAction GetAction()
        {
            return action;
        }
    }
}