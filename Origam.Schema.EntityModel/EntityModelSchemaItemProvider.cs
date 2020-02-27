#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntityModelSchemaItemProvider.
	/// </summary>
	public class EntityModelSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public EntityModelSchemaItemProvider()
		{
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return AbstractDataEntity.CategoryConst;
			}
		}
		public override bool AutoCreateFolder
		{
			get
			{
				return true;
			}
		}
		public override string Group
		{
			get
			{
				return "DATA";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "icon_09_entities.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Entities";
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
				return new Type[] {typeof(TableMappingItem), typeof(DetachedEntity)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(TableMappingItem))
			{
				item = new TableMappingItem(schemaExtensionId);
				item.Name = "NewTable";
			}
			else if(type == typeof(DetachedEntity))
			{
				item = new DetachedEntity(schemaExtensionId);
				item.Name = "NewEntity";
			}
			else
			{
				throw new ArgumentOutOfRangeException("type", type, "This type is not supported by EntityModel");
			}

			item.RootProvider = this;
			item.PersistenceProvider = this.PersistenceProvider;
			item.Group = group;
			this.ChildItems.Add(item);

			// add default ancestor to all database entities
			if(type == typeof(TableMappingItem))
			{
				EntityHelper.AddAncestor(item as IDataEntity, EntityHelper.DefaultAncestor, false);
			}

			return item;
		}

		#endregion
	}
}
