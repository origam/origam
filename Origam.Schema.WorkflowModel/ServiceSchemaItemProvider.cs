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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ServiceSchemaItemProvider.
	/// </summary>
	public class ServiceSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public const string ItemTypeConst = "Service";
		
		public ServiceSchemaItemProvider()
		{
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return Service.ItemTypeConst;
			}
		}
		public override string Group
		{
			get
			{
				return "BL";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "icon_31_services.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Services";
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

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(Service)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(Service))
			{
				item = new Service(schemaExtensionId);
				item.Name = "NewService";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorServiceModelUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			item.RootProvider = this;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
