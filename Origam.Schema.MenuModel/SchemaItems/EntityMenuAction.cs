#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for EntitySecurityRule.
	/// </summary>
	[SchemaItemDescription("Menu Action", "UI Actions", 69)]
    [HelpTopic("Menu+Action")]
	public class EntityMenuAction : EntityUIAction
	{
		public EntityMenuAction() : base() {}

		public EntityMenuAction(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public EntityMenuAction(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		public override string Icon
		{
			get
			{
				return "69";
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Menu);

			base.GetExtraDependencies (dependencies);
		}

		public override string[] NewTypeNames
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
		[EntityColumn("G05")]  
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
}
