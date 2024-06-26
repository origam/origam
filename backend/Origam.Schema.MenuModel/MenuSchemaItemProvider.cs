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
using System.Linq;

namespace Origam.Schema.MenuModel;
/// <summary>
/// Summary description for MenuSchemaItemProvider.
/// </summary>
public class MenuSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
{
	public MenuSchemaItemProvider() 
	{
		ChildItemTypes.Add(typeof(Menu));
		ChildItemTypes.Add(typeof(ContextMenu));
	}
	/// <summary>
	/// Returns the first child Menu, skipping all the ContextMenu types.
	/// </summary>
	public Menu MainMenu =>
		ChildItemsByType(Menu.CategoryConst)
			.OfType<Menu>()
			.FirstOrDefault();
	#region ISchemaItemProvider Members
	public override string RootItemType
	{
		get
		{
			return Menu.CategoryConst;
		}
	}
	public override string Group
	{
		get
		{
			return "UI";
		}
	}
	#endregion
	#region IBrowserNode Members
	public override string Icon
	{
		get
		{
			// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
			return "icon_18_menu.png";
		}
	}
	public override string NodeText
	{
		get
		{	
			return "Menu";
		}
		set
		{
			base.NodeText = value;
		}
	}
	public override string NodeToolTipText
	{
		get
		{
			// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
			return null;
		}
	}
	#endregion
}
