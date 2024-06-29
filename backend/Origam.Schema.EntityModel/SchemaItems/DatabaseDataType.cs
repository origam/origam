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
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;
[SchemaItemDescription("Database Data Type", "icon_08_database-data-types.png")]
[HelpTopic("Database+Data+Type")]
[DefaultProperty("DataType")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public  class DatabaseDataType : AbstractSchemaItem
{
	public const string CategoryConst = "DatabaseDataType";
	public DatabaseDataType() : base() {}
	public DatabaseDataType(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DatabaseDataType(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	OrigamDataType _dataType = OrigamDataType.String;
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[Category("Mapping")]
    [DisplayName("Data Type")]
    [Description("Base ORIGAM data type to which the mapping is assigned.")]
    [XmlAttribute("dataType")]
	public OrigamDataType DataType
	{
		get
		{
			return _dataType;
		}
		set
		{
			_dataType = value;
		}
	}
    string _mappedDatabaseTypeName = "";
    [Category("Mapping")]
    [Description("Name of the data type as used by the current database engine.")]
    [DisplayName("Database Specific Data Type")]
    [TypeConverter(typeof(DataTypeMappingAvailableTypesConverter))]
    [NotNullModelElementRule()]
    [XmlAttribute("mappedDatabaseTypeName")]
    public string MappedDatabaseTypeName
	{
		get
		{
			return _mappedDatabaseTypeName;
		}
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
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override ISchemaItemCollection ChildItems
	{
		get
		{
			return SchemaItemCollection.Create();
		}
	}
	#endregion
}
