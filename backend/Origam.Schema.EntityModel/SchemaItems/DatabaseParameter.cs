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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[ClassMetaVersion(versionStr: "6.0.0")]
public class DatabaseParameter : SchemaItemParameter, IDatabaseDataTypeMapping
{
    public DatabaseParameter()
        : base() { }

    public DatabaseParameter(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public DatabaseParameter(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public Guid dataTypeMappingId;

    [Category(category: "Database Mapping")]
    [TypeConverter(type: typeof(DataTypeMappingConverter))]
    [Description(description: "Database specific data type")]
    [DisplayName(displayName: "Data Type Mapping")]
    [XmlReference(attributeName: "mappedDataType", idField: "dataTypeMappingId")]
    public DatabaseDataType MappedDataType
    {
        get
        {
            return (DatabaseDataType)
                    PersistenceProvider.RetrieveInstance(
                        type: typeof(DatabaseDataType),
                        primaryKey: new ModelElementKey(id: dataTypeMappingId)
                    ) as DatabaseDataType;
        }
        set
        {
            dataTypeMappingId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
}
