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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.EntityModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(
    name: "Virtual Field",
    folderName: "Fields",
    iconName: "icon_virtual-field.png"
)]
[HelpTopic(topic: "Virtual+Field")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DetachedField : AbstractDataEntityColumn, IRelationReference
{
    public DetachedField() { }

    public DetachedField(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public DetachedField(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid ArrayRelationId;

    [NoDuplicateNamesInParentRule]
    [Category(category: "(Schema Item)")]
    [StringNotEmptyModelElementRule]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "name")]
    public override string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    [Category(category: "Array")]
    [TypeConverter(type: typeof(EntityRelationConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "arrayRelation", idField: "ArrayRelationId")]
    public IAssociation ArrayRelation
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ArrayRelationId)
                ) as IAssociation;
        set
        {
            ArrayRelationId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            ArrayValueField = null;
        }
    }

    [Browsable(browsable: false)]
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

    [Category(category: "Array")]
    [TypeConverter(type: typeof(EntityRelationColumnsConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "arrayValueField", idField: "ArrayValueFieldId")]
    public IDataEntityColumn ArrayValueField
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ArrayValueFieldId)
                ) as IDataEntityColumn;
        set
        {
            ArrayValueFieldId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            if (value == null)
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
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    ) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (ArrayRelation != null)
        {
            dependencies.Add(item: ArrayRelation);
        }
        if (ArrayValueField != null)
        {
            dependencies.Add(item: ArrayValueField);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        if (ArrayRelation != null)
        {
            foreach (ISchemaItem item in RootItem.ChildItemsRecursive)
            {
                if (item.OldPrimaryKey?.Equals(obj: ArrayRelation.PrimaryKey) == true)
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
        return (type == typeof(FieldMappingItem)) && (ParentItem is IDataEntity);
    }

    protected override ISchemaItem ConvertTo<T>()
    {
        var converted = ParentItem.NewItem<T>(schemaExtensionId: SchemaExtensionId, group: Group);
        if (converted is AbstractDataEntityColumn abstractDataEntityColumn)
        {
            CopyFieldMembers(source: this, destination: abstractDataEntityColumn);
        }
        if (converted is FieldMappingItem fieldMappingItem)
        {
            fieldMappingItem.MappedColumnName = Name;
        }
        else
        {
            return base.ConvertTo<T>();
        }
        // does the common conversion tasks and persists both this and converted objects
        FinishConversion(source: this, converted: converted);
        return converted;
    }
    #endregion
}
