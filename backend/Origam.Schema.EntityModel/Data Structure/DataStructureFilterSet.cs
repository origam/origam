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
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DataQuery.
	/// </summary>
	[SchemaItemDescription("Filter Set", "Filter Sets", "icon_filter-set.png")]
    [HelpTopic("Filter+Sets")]
    [ClassMetaVersion("6.0.0")]
    public class DataStructureFilterSet : DataStructureMethod
	{
		public DataStructureFilterSet() {}

		public DataStructureFilterSet(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureFilterSet(Key primaryKey) : base(primaryKey)	{}
	
		#region Properties
		private bool _isDynamic = false;
		[DefaultValue(false)]
        [XmlAttribute("dynamic")]
        [DynamicModelElementRule]
        public bool IsDynamic
		{
			get => _isDynamic;
			set => _isDynamic = value;
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes => new[]
		{
			typeof(DataStructureFilterSetFilter)
		};

		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			string itemName = null;
			if(typeof(T) == typeof(DataStructureFilterSetFilter))
			{
				itemName = "NewDataStructureFilterSetFilter";
			}
			else if(typeof(T) == typeof(DataStructureFilterSetOrExpression))
			{
				itemName = "OR";
			}
			return base.NewItem<T>(schemaExtensionId, group, itemName);
		}

		#endregion
	}
}
