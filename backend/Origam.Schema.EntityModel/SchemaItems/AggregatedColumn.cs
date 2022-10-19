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
using System.Xml.Serialization;
using Origam.DA.EntityModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel
{
	public enum AggregationType
	{
		None = 0,
		Count = 1,
		Sum = 2,
		Average = 3,
		Minimum = 4,
		Maximum = 5,
		CumulativeSum = 6
	}

	/// <summary>
	/// Summary description for AggregatedColumn.
	/// </summary>
	[SchemaItemDescription("Aggregated Field", "Fields", "icon_agregated-field.png")]
    [HelpTopic("Aggregated+Field")]
    [DefaultProperty("Relation")]
    [ClassMetaVersion("6.0.0")]
	public class AggregatedColumn : AbstractDataEntityColumn, IRelationReference
	{
		public AggregatedColumn() : base() {}

		public AggregatedColumn(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public AggregatedColumn(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractDataEntityColumn Members

		public override string FieldType { get; } = "AggregatedColumn";

		public override bool ReadOnly
		{
			get
			{
				return true;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Field);
			dependencies.Add(this.Relation);

			base.GetExtraDependencies (dependencies);
		}

        public override void UpdateReferences()
        {
            if (this.Relation != null)
            {
                foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
                {
                    if (item.OldPrimaryKey != null)
                    {
                        if (item.OldPrimaryKey.Equals(this.Relation.PrimaryKey))
                        {
                            // store the old field because setting an entity will reset the field
                            IDataEntityColumn oldField = this.Field;
                            this.Relation = item as IAssociation;
                            this.Field = oldField;
                            break;
                        }
                    }
                }
            }
            base.UpdateReferences();
        }
        #endregion

		#region Properties
		private AggregationType _aggregationType = AggregationType.Sum;

		
		[NoDuplicateNamesInParentRule]
		[Category("(Schema Item)")]
		[StringNotEmptyModelElementRule]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlAttribute("name")]
		public override string Name
		{
			get => base.Name; 
			set => base.Name = value;
		}
		
		[Category("Aggregation")]
		[NotNullModelElementRule()]
		[NoNestedCountAggregationsRule]
        [XmlAttribute("aggregationType")]
		public AggregationType AggregationType
		{
			get
			{
				return _aggregationType;
			}
			set
			{
				_aggregationType = value;
			}
		}

		public Guid RelationId;

		[Category("Aggregation")]
		[TypeConverter(typeof(EntityRelationConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("relation", "RelationId")]
		public IAssociation Relation
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RelationId)) as IAssociation;
			}
			set
			{
				this.RelationId = (Guid)value.PrimaryKey["Id"];
                this.Field = null;
			}
		}
		
		public Guid ColumnId;

		[Category("Aggregation")]
		[TypeConverter(typeof(EntityRelationColumnsConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("field", "ColumnId")]
        public IDataEntityColumn Field
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ColumnId)) as IDataEntityColumn;
			}
			set
			{
				this.ColumnId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
                if (Field != null)
                {
                    this.DataType = this.Field.DataType;
                    this.DataLength = this.Field.DataLength;
                    this.Name = this.AggregationType.ToString() + this.Field.Name;
                    this.Caption = this.Field.Caption;
                }
            }
		}
		#endregion

		#region Convert
		public override bool CanConvertTo(Type type)
		{
			return
				(
				(
				type == typeof(FieldMappingItem)
				| type == typeof(DetachedField)
				)
				&
				(
				this.ParentItem is IDataEntity
				)
				);
		}

		public override ISchemaItem ConvertTo(Type type)
		{
			AbstractSchemaItem converted = this.ParentItem.NewItem(type, this.SchemaExtensionId, this.Group) as AbstractSchemaItem;

			if(converted is AbstractDataEntityColumn)
			{
				AbstractDataEntityColumn.CopyFieldMembers(this, converted as AbstractDataEntityColumn);
			}

			if(converted is FieldMappingItem)
			{
				(converted as FieldMappingItem).MappedColumnName = this.Name;
			}
			else if(type == typeof(DetachedField))
			{
			}
			else
			{
				return base.ConvertTo(type);
			}

			// does the common conversion tasks and persists both this and converted objects
			AbstractSchemaItem.FinishConversion(this, converted);

			return converted;
		}
		#endregion

	}
}
