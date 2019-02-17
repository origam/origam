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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for Menu.
	/// </summary>
	[SchemaItemDescription("Menu", "home.png")]
    [HelpTopic("Menu")]
	[XmlModelRoot(ItemTypeConst)]
    public class Menu : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string ItemTypeConst = "Menu";

		public Menu() : base() {}

		public Menu(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public Menu(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		[Browsable(false)]
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Properties

		[Category("Menu Item")]
		[EntityColumn("SS01")]
		[XmlAttribute("displayName")]
		public string DisplayName { get; set; }

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(Submenu),
									  typeof(FormReferenceMenuItem),
									  typeof(DataConstantReferenceMenuItem),
									  typeof(WorkflowReferenceMenuItem),
									  typeof(ReportReferenceMenuItem),
									  typeof(DashboardMenuItem),
                                      typeof(DynamicMenu)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(WorkflowReferenceMenuItem))
			{
				item = new WorkflowReferenceMenuItem(schemaExtensionId);
				item.Name = "<SequentialWorkflowReference_name>";
			}
			else if(type == typeof(FormReferenceMenuItem))
			{
				item = new FormReferenceMenuItem(schemaExtensionId);
				item.Name = "<ScreenReference_name>";
			}
			else if(type == typeof(ReportReferenceMenuItem))
			{
				item = new ReportReferenceMenuItem(schemaExtensionId);
				item.Name = "<ReportReference_name>";
			}
			else if(type == typeof(DataConstantReferenceMenuItem))
			{
				item = new DataConstantReferenceMenuItem(schemaExtensionId);
				item.Name = "<DataConstantReference_name>";
			}
			else if(type == typeof(Submenu))
			{
				item = new Submenu(schemaExtensionId);
				item.Name = "<Submenu_name>";
			}
			else if(type == typeof(DashboardMenuItem))
			{
				item = new DashboardMenuItem(schemaExtensionId);
				item.Name = "<Dashboard_name>";
			}
            else if (type == typeof(DynamicMenu))
            {
                item = new DynamicMenu(schemaExtensionId);
                item.Name = "<DynamicMenu_name>";
            }
            else
                throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorMenuUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
