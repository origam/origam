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

using Origam.DA.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;

namespace Origam.Schema.MenuModel;
/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription("Menu Action", "UI Actions", "icon_menu-action.png")]
[HelpTopic("Menu+Action")]
[ClassMetaVersion("6.0.0")]
public class EntityMenuAction : EntityUIAction
{
	public EntityMenuAction() : base() {}
	public EntityMenuAction(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityMenuAction(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractDataEntityColumn Members
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Menu);
		base.GetExtraDependencies (dependencies);
	}
	public override IList<string> NewTypeNames
	{
		get
		{
			if(this.Menu == null)
			{
				return base.NewTypeNames;
			}
			else
			{
				return this.Menu.NewTypeNames;
			}
		}
	}
	#endregion
	#region Properties
	public Guid MenuId;
	[Category("References")]
	[TypeConverter(typeof(MenuModel.MenuItemConverter))]
    [XmlReference("menu", "MenuId")]
	public MenuModel.AbstractMenuItem Menu
	{
		get
		{
			return (MenuModel.AbstractMenuItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.MenuId));
		}
		set
		{
			this.MenuId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
		}
	}
	#endregion
}
