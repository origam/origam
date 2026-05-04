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
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(name: "Parameter", folderName: "Parameters", icon: 29)]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DashboardWidgetParameter : AbstractSchemaItem
{
    public const string CategoryConst = "DashboardWidgetParameter";

    public DashboardWidgetParameter()
        : base()
    {
        Init();
    }

    public DashboardWidgetParameter(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public DashboardWidgetParameter(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    #region Properties
    private string _caption = "";

    [Category(category: "User Interface")]
    [StringNotEmptyModelElementRule()]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private OrigamDataType _dataType = OrigamDataType.String;

    [Category(category: "Mapping")]
    [XmlAttribute(attributeName: "dataType")]
    public OrigamDataType DataType
    {
        get { return _dataType; }
        set { _dataType = value; }
    }
    public Guid DataConstantId;

    [Category(category: "Mapping")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "defaultValue", idField: "DataConstantId")]
    public DataConstant DefaultValue
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.DataConstantId)
                    ) as DataConstant;
        }
        set
        {
            this.DataConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
    public Guid LookupId;

    [Category(category: "Mapping")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "lookup", idField: "LookupId")]
    public IDataLookup Lookup
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.LookupId)
                    ) as IDataLookup;
        }
        set { this.LookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    public override string Icon
    {
        get { return "29"; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
