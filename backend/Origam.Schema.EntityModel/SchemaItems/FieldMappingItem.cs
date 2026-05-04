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
using Origam.Schema.EntityModel.Attributes;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(
    name: "Database Field",
    folderName: "Fields",
    iconName: "icon_database-field.png"
)]
[HelpTopic(topic: "Database+Field")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class FieldMappingItem : AbstractDataEntityColumn, IDatabaseDataTypeMapping
{
    public FieldMappingItem() { }

    public FieldMappingItem(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public FieldMappingItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties

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

    private string _sourceFieldName;

    [LengthLimit]
    [Category(category: "Mapping")]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "mappedColumnName")]
    [DisplayName(displayName: "Mapped Column Name")]
    public string MappedColumnName
    {
        get => _sourceFieldName?.Trim();
        set => _sourceFieldName = value;
    }
    public Guid dataTypeMappingId;

    [Category(category: "Mapping")]
    [TypeConverter(type: typeof(DataTypeMappingConverter))]
    [Description(
        description: "Optional specific data type. If not specified a default type will be assigned based on the main Data Type."
    )]
    [DisplayName(displayName: "Mapped Data Type")]
    [XmlReference(attributeName: "mappedDataType", idField: "dataTypeMappingId")]
    public DatabaseDataType MappedDataType
    {
        get =>
            (DatabaseDataType)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(DatabaseDataType),
                    primaryKey: new ModelElementKey(id: dataTypeMappingId)
                );
        set => dataTypeMappingId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
    #region Overriden AbstractDataEntityColumn Members
    protected bool _excludeFromAuditing = false;

    [Browsable(browsable: true)]
    [Category(category: "Entity Column"), DefaultValue(value: false)]
    [Description(
        description: "When turned on this field's changes will not be recorded in the audit log (e.g. password fields)."
    )]
    [XmlAttribute(attributeName: "excludeFromAuditing")]
    public override bool ExcludeFromAuditing
    {
        get => _excludeFromAuditing;
        set => _excludeFromAuditing = value;
    }
    public override string FieldType { get; } = "FieldMappingItem";

    [Browsable(browsable: false)]
    public override bool ReadOnly => false;

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    ) { }

    public override void OnNameChanged(string originalName)
    {
        if (string.IsNullOrEmpty(value: MappedColumnName) || MappedColumnName == originalName)
        {
            MappedColumnName = Name;
        }
    }

    public override void OnPropertyChanged(string propertyName)
    {
        if (propertyName == "DataType")
        {
            MappedDataType = null;
        }
        base.OnPropertyChanged(propertyName: propertyName);
    }
    #endregion
    #region Convert
    public override bool CanConvertTo(Type type)
    {
        return (type == typeof(DetachedField)) && (ParentItem is IDataEntity);
    }

    protected override ISchemaItem ConvertTo<T>()
    {
        var converted = ParentItem.NewItem<T>(schemaExtensionId: SchemaExtensionId, group: Group);
        if (converted is AbstractDataEntityColumn abstractDataEntityColumn)
        {
            CopyFieldMembers(source: this, destination: abstractDataEntityColumn);
        }
        if (typeof(T) == typeof(DetachedField)) { }
        else
        {
            return base.ConvertTo<T>();
        }
        // does the common conversion tasks and persists both this and converted objects
        FinishConversion(source: this, converted: converted);
        return converted;
    }
    #endregion
    public static IDataEntity GetLocalizationTable(TableMappingItem tableMappingItem)
    {
        return tableMappingItem?.LocalizationRelation?.AssociatedEntity;
    }

    [Browsable(browsable: false)]
    public FieldMappingItem GetLocalizationField(TableMappingItem tableMappingItem)
    {
        if ((DataType != OrigamDataType.String) && (DataType != OrigamDataType.Memo))
        {
            // non-string data types couldn't be localized
            return null;
        }
        var localizationTable = GetLocalizationTable(tableMappingItem: tableMappingItem);
        // find column in localization table
        return localizationTable?.GetChildByName(name: Name) as FieldMappingItem;
    }
}
