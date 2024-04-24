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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel
{
	[SchemaItemDescription("Sort Set", "Sort Sets", "icon_sort-set.png")]
    [HelpTopic("Sort+Sets")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DataStructureSortSet : AbstractSchemaItem
	{
		public const string CategoryConst = "DataStructureSortSet";

		public DataStructureSortSet() {}

		public DataStructureSortSet(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureSortSet(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		public override string ItemType => CategoryConst;

		public override bool UseFolders => false;

		public override bool CanMove(UI.IBrowserNode2 newNode)
		{
			return ((ISchemaItem)newNode).PrimaryKey.Equals(
				ParentItem.PrimaryKey);
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes => new[]
		{
			typeof(DataStructureSortSetItem)
		};

		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			return base.NewItem<T>(schemaExtensionId, group, 
				typeof(T) == typeof(DataStructureSortSetItem) ?
				"NewSortSetItem" : null);
		}

		#endregion
	}
}
