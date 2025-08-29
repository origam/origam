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
using System.Windows.Forms;

namespace Origam.UI;
/// <summary>
/// Summary description for AsMenu.
/// </summary>
public class AsMenu : ToolStripMenuItem, IStatusUpdate
{
	private readonly object caller;
	public AsMenu(object caller, string text) : base (text)
	{
		this.caller = caller;
		this.Text = text;
        this.DropDownOpening += AsMenu_DropDownOpening;
	}
	public List<ToolStripItem> SubItems { get; } = new ();
	public void Clear()
	{
        ToolStripItem[] array = new ToolStripItem[this.SubItems.Count];
		this.SubItems.CopyTo(array, 0);
		this.SubItems.Clear();
        this.DropDownItems.Clear();
		foreach(ToolStripItem item in array)
		{
			if(item is IDisposable disposeable)
			{
				disposeable.Dispose();
			}
		}
	}
	#region IStatusUpdate Members
	public void UpdateItemsToDisplay()
	{
		MenuItemTools.UpdateMenuItems(
			itemsToUpdate: DropDownItems,
			itemsToAdd: SubItems,
			caller: caller);
		
        this.Enabled = (this.SubItems.Count != 0);
	}
	public void PopulateMenu()
	{
		UpdateItemsToDisplay();
	}
	#endregion
    void AsMenu_DropDownOpening(object sender, EventArgs e)
    {
		this.UpdateItemsToDisplay();
	}
}
