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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntityColumnReference.
/// </summary>
[SchemaItemDescription(
    name: "Data Constant Reference",
    folderName: "Parameters",
    iconName: "icon_data-constant-reference.png"
)]
[HelpTopic(topic: "Data+Constant+Reference")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "DataConstant")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataConstantReference : AbstractSchemaItem
{
    public const string CategoryConst = "DataConstantReference";

    public DataConstantReference()
        : base() { }

    public DataConstantReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataConstantReference(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.DataConstant != null)
        {
            base.GetParameterReferences(parentItem: DataConstant, list: list);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.DataConstant);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    public Guid DataConstantId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "constant", idField: "DataConstantId")]
    public DataConstant DataConstant
    {
        get
        {
            try
            {
                return (ISchemaItem)
                        this.PersistenceProvider.RetrieveInstance(
                            type: typeof(ISchemaItem),
                            primaryKey: new ModelElementKey(id: this.DataConstantId)
                        ) as DataConstant;
            }
            catch
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorDataConstantNotFound",
                        args: this.Name
                    )
                );
            }
        }
        set
        {
            DataConstantId = (Guid)value.PrimaryKey[key: "Id"];
            if (Name == null)
            {
                Name = value.Name;
            }
        }
    }
    #endregion
}
