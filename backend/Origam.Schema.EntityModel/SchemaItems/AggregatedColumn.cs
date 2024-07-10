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

namespace Origam.Schema.EntityModel;
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
[SchemaItemDescription("Aggregated Field", "Fields", "icon_agregated-field.png")]
[HelpTopic("Aggregated+Field")]
[DefaultProperty("Relation")]
[ClassMetaVersion("6.0.0")]
public class AggregatedColumn : AbstractDataEntityColumn, IRelationReference
{
	public AggregatedColumn() {}
	public AggregatedColumn(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AggregatedColumn(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractDataEntityColumn Members
	public override string FieldType => "AggregatedColumn";
	public override bool ReadOnly => true;
	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
		dependencies.Add(Field);
		dependencies.Add(Relation);
		base.GetExtraDependencies (dependencies);
	}
    public override void UpdateReferences()
    {
        if(Relation != null)
        {
            foreach(ISchemaItem item in RootItem.ChildItemsRecursive)
            {
                if(item.OldPrimaryKey?.Equals(Relation.PrimaryKey) == true)
                {
	                // store the old field because setting an entity will reset the field
	                var oldField = Field;
	                Relation = item as IAssociation;
	                Field = oldField;
	                break;
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
		get => _aggregationType;
		set => _aggregationType = value;
	}
	public Guid RelationId;
	[Category("Aggregation")]
	[TypeConverter(typeof(EntityRelationConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
    [XmlReference("relation", "RelationId")]
	public IAssociation Relation
	{
		get => (AbstractSchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), new ModelElementKey(RelationId)) 
			as IAssociation;
		set
		{
			RelationId = (Guid)value.PrimaryKey["Id"];
            Field = null;
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
		get => (AbstractSchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), new ModelElementKey(ColumnId)) 
			as IDataEntityColumn;
		set
		{
			ColumnId = (value == null) 
				? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			if(Field == null)
			{
				return;
			}
			DataType = Field.DataType;
            DataLength = Field.DataLength;
            Name = AggregationType + Field.Name;
            Caption = Field.Caption;
		}
	}
	#endregion
	#region Convert
	public override bool CanConvertTo(Type type)
	{
		return (type == typeof(FieldMappingItem) 
		        || type == typeof(DetachedField)) 
		       && (ParentItem is IDataEntity);
	}
	protected override ISchemaItem ConvertTo<T>()
	{
		var converted = ParentItem.NewItem<T>(SchemaExtensionId, Group);
		if(converted is AbstractDataEntityColumn abstractDataEntityColumn)
		{
			CopyFieldMembers(this, abstractDataEntityColumn);
		}
		if(converted is FieldMappingItem fieldMappingItem)
		{
			fieldMappingItem.MappedColumnName = Name;
		}
		else if(typeof(T) == typeof(DetachedField))
		{
		}
		else
		{
			return base.ConvertTo<T>();
		}
		// does the common conversion tasks and persists both this and converted objects
		FinishConversion(this, converted);
		return converted;
	}
	#endregion
}
