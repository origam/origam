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
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntityColumnReference.
	/// </summary>
	[SchemaItemDescription("Field Reference", 1)]
    [HelpTopic("Field+Reference")]
	[XmlModelRoot(ItemTypeConst)]
	[DefaultProperty("Column")]
    public class EntityColumnReference : AbstractSchemaItem
	{
		public const string ItemTypeConst = "EntityColumnReference";

		public EntityColumnReference() : base() {}

		public EntityColumnReference(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public EntityColumnReference(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		public override string Icon
		{
			get
			{
				try
				{
					return this.Column.Icon;
				}
				catch
				{
					return "1";
				}
			}
		}

		public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
		{
			if(this.Column != null)
				base.GetParameterReferences(this.Column as AbstractSchemaItem, list);
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Column);

			base.GetExtraDependencies (dependencies);
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.Column.PrimaryKey))
					{
						this.Column = item as IDataEntityColumn;
						break;
					}
				}
			}

			base.UpdateReferences ();
		}

		public override SchemaItemCollection ChildItems
		{
			get
			{
				return new SchemaItemCollection();
			}
		}
		#endregion

		#region Properties
		[EntityColumn("G01")]  
		public Guid ColumnId;

		[Category("Reference")]
		[TypeConverter(typeof(EntityColumnReferenceConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("field", "ColumnId")]
		public IDataEntityColumn Column
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ColumnId;

				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as IDataEntityColumn;
			}
			set
			{
				this.ColumnId = (Guid)value.PrimaryKey["Id"];

				if(this.Name == null)
				{
					this.Name = this.Column.Name;
				}
			}
		}
		#endregion
	}
}
