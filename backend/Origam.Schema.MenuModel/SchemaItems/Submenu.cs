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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for Submenu.
	/// </summary>
	[SchemaItemDescription("Submenu", "menu_folder.png")]
    [HelpTopic("Submenu")]
    [ClassMetaVersion("6.0.0")]
	public class Submenu : AbstractMenuItem, ISchemaItemFactory
	{
		public Submenu() : base() {}

		public Submenu(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public Submenu(Key primaryKey) : base(primaryKey)	{}

		[Browsable(false)]
		public override string Roles
		{
			get
			{
				ArrayList children = this.ChildItemsRecursive;
				ArrayList roles = new ArrayList();

				foreach(object child in children)
				{
					AbstractMenuItem menuItem = child as AbstractMenuItem;

					if(menuItem != null & !(menuItem is Submenu))
					{
						if(menuItem.Roles == "*") return "*";

						string[] childRoles = menuItem.Roles.Split(";".ToCharArray());

						foreach(string role in childRoles)
						{
							if(! roles.Contains(role)) roles.Add(role);
						}
					}
				}

				StringBuilder result = new StringBuilder();
				foreach(string role in roles)
				{
					if(result.Length > 0) result.Append(";");
					result.Append(role);
				}

				return result.ToString();
			}
			set
			{
				base.Roles = value;
			}
		}

		[Browsable(false)]
		public override string Features
		{
			get
			{
				ArrayList children = this.ChildItemsRecursive;
				ArrayList features = new ArrayList();

				foreach(object child in children)
				{
					AbstractMenuItem menuItem = child as AbstractMenuItem;

					if(menuItem != null & !(menuItem is Submenu))
					{
						if(menuItem.Features == "" | menuItem.Features == null) return "";

						string[] childFeatures = menuItem.Features.Split(";".ToCharArray());

						foreach(string feature in childFeatures)
						{
							if(! features.Contains(feature)) features.Add(feature);
						}
					}
				}

				StringBuilder result = new StringBuilder();
				foreach(string feature in features)
				{
					if(result.Length > 0) result.Append(";");
					result.Append(feature);
				}

				return result.ToString();
			}
			set
			{
				base.Features = value;
			}
		}

		[DefaultValue(false)]
		[XmlAttribute("isHidden")]
		public bool IsHidden { get; set; } 

		[Browsable(false)]
        public new bool OpenExclusively
        {
            get
            {
                return base.OpenExclusively;
            }
            set
            {
                base.OpenExclusively = value;
            }
        }

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

		public override int CompareTo(object obj)
		{
			AbstractMenuItem item = obj as AbstractMenuItem;
			Submenu submenu = obj as Submenu;

			if(submenu != null)
			{
				return this.DisplayName.CompareTo(item.DisplayName);
			}
			else if(item != null)
			{
				return -1;
			}
			else
			{
				throw new InvalidCastException();
			}
		}


		#endregion

	}
}
