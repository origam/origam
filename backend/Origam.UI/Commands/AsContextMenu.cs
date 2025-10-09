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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Origam.UI;
/// <summary>
/// Summary description for AsMenu.
/// </summary>
public class AsContextMenu : ContextMenuStrip, IStatusUpdate
{
	private readonly object caller;
	private readonly List<ToolStripItem> subItems = new List<ToolStripItem>();
	public AsContextMenu(object caller)
	{
		this.caller = caller;
	}
	
	public void AddSubItem(ToolStripItem subItem)
	{
		subItems.Add(subItem);
		UpdateItemsToDisplay();
	}
	public void AddSubItems(IEnumerable<ToolStripItem> newItems)
	{
		subItems.AddRange(newItems);
		UpdateItemsToDisplay();
	}
	protected override void OnOpening(CancelEventArgs e)
    {
        UpdateItemsToDisplay();
        base.OnOpening(new CancelEventArgs(false));
    }
	#region IStatusUpdate Members
	public void UpdateItemsToDisplay()
	{
		MenuItemTools.UpdateMenuItems(
			itemsToUpdate: Items,
			itemsToAdd: subItems,
			caller: caller);
	}
	#endregion
}
