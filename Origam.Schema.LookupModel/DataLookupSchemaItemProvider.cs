#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using Origam.Schema.EntityModel;

namespace Origam.Schema.LookupModel
{
	/// <summary>
	/// Summary description for DataLookupSchemaItemProvider.
	/// </summary>
	public class DataLookupSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory, IDataLookupSchemaItemProvider
	{
		public DataLookupSchemaItemProvider()
		{
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return AbstractDataLookup.ItemTypeConst;
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
				return "icon_11_lookups.png";
			}
		}

		public override string NodeText
		{
			get
			{	
				return "Lookups";
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
				return new Type[] {typeof(DataServiceDataLookup)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractDataLookup item;

			if(type == typeof(DataServiceDataLookup))
			{
				item = new DataServiceDataLookup(schemaExtensionId);
				item.Name = "NewDataServiceDataLookup";

			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDataLookupUknownType"));

			item.Group = group;
			item.RootProvider = this;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);

			return item;
		}

		#endregion
	}
}
