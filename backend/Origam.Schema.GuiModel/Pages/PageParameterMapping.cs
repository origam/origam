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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(
    name: "Parameter Mapping",
    folderName: "Parameter Mappings",
    iconName: "file-mapping.png"
)]
[HelpTopic(topic: "Parameter+Mapping")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.1")]
public class PageParameterMapping : AbstractSchemaItem
{
    public const string CategoryConst = "PageParameterMapping";

    public PageParameterMapping()
        : base()
    {
        Init();
    }

    public PageParameterMapping(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public PageParameterMapping(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (DefaultValue != null)
        {
            dependencies.Add(item: DefaultValue);
        }
    }

    #region Properties
    [Category(category: "Mapping")]
    [Description(
        description: "Name of url query string parameter, e.g. in case http://my-api/my-page?searchstring=value the mapped parametr should be 'searchstring'"
    )]
    [XmlAttribute(attributeName: "mappedParameter")]
    public string MappedParameter { get; set; } = "";

    [Category(category: "Mapping")]
    [Description(
        description: "Name of a datastructure entity in a mapped context "
            + "store's datastructure. If defined, the incoming json body is "
            + "wrapped by a new object with the given name. So that way, the"
            + " original input json object is expected as just datastructure "
            + "entity object itself or a list of datastructure entity objects"
            + " on json top level."
    )]
    [DefaultValue(value: "")]
    [XmlAttribute(attributeName: "entityName")]
    public string DatastructureEntityName { get; set; } = "";
    public Guid DataConstantId;

    [Category(category: "Mapping")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "defaultValue", idField: "DataConstantId")]
    public DataConstant DefaultValue
    {
        get =>
            (ISchemaItem)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DataConstantId)
                ) as DataConstant;
        set
        {
            this.DataConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }

    [Category(category: "Lists")]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isList")]
    public bool IsList { get; set; } = false;
    public Guid SeparatorDataConstantId;

    [Category(category: "Lists")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "listSeparator", idField: "SeparatorDataConstantId")]
    public DataConstant ListSeparator
    {
        get =>
            (ISchemaItem)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.SeparatorDataConstantId)
                ) as DataConstant;
        set
        {
            this.SeparatorDataConstantId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public override string ItemType => CategoryConst;
    #endregion
}
