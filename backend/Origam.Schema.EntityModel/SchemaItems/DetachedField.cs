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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using Origam.DA.EntityModel;

namespace Origam.Schema.EntityModel;
[SchemaItemDescription("Virtual Field", "Fields", "icon_virtual-field.png")]
[HelpTopic("Virtual+Field")]
[ClassMetaVersion("6.0.0")]
public class DetachedField : AbstractDataEntityColumn, IRelationReference
{
	public DetachedField() {}
	public DetachedField(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DetachedField(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	public Guid ArrayRelationId;
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
	
	[Category("Array")]
	[TypeConverter(typeof(EntityRelationConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("arrayRelation", "ArrayRelationId")]
	public IAssociation ArrayRelation
	{
		get => (AbstractSchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), new ModelElementKey(ArrayRelationId)) 
			as IAssociation;
		set
		{
			ArrayRelationId = (value == null) 
				? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			ArrayValueField = null;
		}
	}
	[Browsable(false)]
		// only for IReference needs, for public access we have ArrayRelation
	public IAssociation Relation
	{
		get => ArrayRelation;
		set
		{
			ArrayRelation = value;
            ArrayValueField = null;
		}
	}
	public Guid ArrayValueFieldId;
	[Category("Array")]
	[TypeConverter(typeof(EntityRelationColumnsConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("arrayValueField", "ArrayValueFieldId")]
	public IDataEntityColumn ArrayValueField
	{
		get => (AbstractSchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(ArrayValueFieldId)) as IDataEntityColumn;
		set
		{
			ArrayValueFieldId = (value == null) 
				? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			if(value == null)
			{
				return;
			}
			DataType = OrigamDataType.Array;
			DefaultLookup = ArrayValueField.DefaultLookup;
			Caption = ArrayValueField.Caption;
		}
	}
	#endregion
	#region Overriden AbstractDataEntityColumn Members
	public override string FieldType => "DetachedField";
	public override bool ReadOnly => false;
	public override void GetParameterReferences(
		AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
	}
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if(ArrayRelation != null)
        {
            dependencies.Add(ArrayRelation);
        }
        if(ArrayValueField !=null)
        {
            dependencies.Add(ArrayValueField);
        }
        base.GetExtraDependencies(dependencies);
    }
    public override void UpdateReferences()
    {
        if(ArrayRelation != null)
        {
            foreach(ISchemaItem item in RootItem.ChildItemsRecursive)
            {
                if(item.OldPrimaryKey?.Equals(ArrayRelation.PrimaryKey) 
                   == true)
                {
	                // store the old field because setting an entity will reset the field
	                var oldField = ArrayValueField;
	                ArrayRelation = item as IAssociation;
	                ArrayValueField = oldField;
	                break;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
	#region Convert
	public override bool CanConvertTo(Type type)
	{
		return (type == typeof(FieldMappingItem)) 
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
