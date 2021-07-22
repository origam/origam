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

using Origam.Schema.GuiModel;
using Origam.Schema.GuiModel.Designer;

namespace Origam.Gui.UI
{
    
    public sealed class ToolStripActionButton : BigToolStripButton,
        IActionContainer
    {
        private readonly EntityUIAction action;


        /// <summary>
        /// This constructor should be used for dubugging only
        /// </summary>
        public ToolStripActionButton()
        {         
        }

        public ToolStripActionButton(EntityUIAction action)
        {
            this.action = action;
            ToolStripButtonTools.InitActionButton(this, action);
        }

        public EntityUIAction GetAction()
        {
            return action;
        }
    }
}