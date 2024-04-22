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

namespace Origam.Schema.EntityModel;

/// <summary>
/// Maps physical table to an entity.
/// </summary>
[SchemaItemDescription("Virtual Entity", "icon_virtual-entity.png")]
[HelpTopic("Entities")]
[ClassMetaVersion("6.0.0")]
public class DetachedEntity : AbstractDataEntity
{
	public DetachedEntity() : base() {}

	public DetachedEntity(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public DetachedEntity(Key primaryKey) : base(primaryKey)	{}

//		public override bool CanConvertTo(Type type)
//		{
//			return (type == typeof(TableMappingItem));
//		}
//
//		public override ISchemaItem ConvertTo(Type type)
//		{
//			if(type == typeof(TableMappingItem))
//			{
//				ArrayList ancestors = new ArrayList(this.Ancestors);
//
//				TableMappingItem converted = this.RootProvider.NewItem(type, this.SchemaExtensionId, this.Group) as TableMappingItem;
//
//				converted.PrimaryKey["Id"] = this.PrimaryKey["Id"];
//
//				converted.Name = this.Name;
//				converted.MappedObjectName = this.Name;
//				converted.IsAbstract = this.IsAbstract;
//
//				// we have to delete first (also from the cache)
//				this.DeleteChildItems = false;
//				this.PersistChildItems = false;
//				this.IsDeleted = true;
//				this.Persist();
//
//				converted.Persist();
//
//				foreach(SchemaItemAncestor ancestor in ancestors)
//				{
//					converted.Ancestors.Add(ancestor.Clone() as SchemaItemAncestor);
//				}
//
//				converted.Persist();
//
//				return converted;
//			}
//			else
//			{
//				return base.ConvertTo(type);
//			}
//		}
}