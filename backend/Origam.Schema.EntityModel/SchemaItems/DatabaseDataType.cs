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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(name: "Database Data Type", iconName: "icon_08_database-data-types.png")]
[HelpTopic(topic: "Database+Data+Type")]
[DefaultProperty(name: "DataType")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DatabaseDataType : AbstractSchemaItem
{
    public const string CategoryConst = "DatabaseDataType";

    public DatabaseDataType()
        : base() { }

    public DatabaseDataType(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DatabaseDataType(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    OrigamDataType _dataType = OrigamDataType.String;

    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [Category(category: "Mapping")]
    [DisplayName(displayName: "Data Type")]
    [Description(description: "Base ORIGAM data type to which the mapping is assigned.")]
    [XmlAttribute(attributeName: "dataType")]
    public OrigamDataType DataType
    {
        get { return _dataType; }
        set { _dataType = value; }
    }
    string _mappedDatabaseTypeName = "";

    [Category(category: "Mapping")]
    [Description(description: "Name of the data type as used by the current database engine.")]
    [DisplayName(displayName: "Database Specific Data Type")]
    [TypeConverter(type: typeof(DataTypeMappingAvailableTypesConverter))]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "mappedDatabaseTypeName")]
    public string MappedDatabaseTypeName
    {
        get { return _mappedDatabaseTypeName; }
        set
        {
            _mappedDatabaseTypeName = value;
            if (value != null)
            {
                this.Name = value;
            }
        }
    }
    #endregion
    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
}
