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
	/// Summary description for EntityFilter.
	/// </summary>
	[SchemaItemDescription("Filter", "Filters", "icon_filter.png")]
    [HelpTopic("Filters")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class EntityFilter : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string CategoryConst = "EntityFilter";

		public EntityFilter() : base() {}

		public EntityFilter(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public EntityFilter(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			return newNode is IDataEntity;
		}

		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(FunctionCall)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(FunctionCall))
			{
				FunctionCall item = new FunctionCall(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;

				item.Group = group;
				item.IsAbstract = this.IsAbstract;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorEntityFilterUnknownType"));
		}

		#endregion

	}
}
