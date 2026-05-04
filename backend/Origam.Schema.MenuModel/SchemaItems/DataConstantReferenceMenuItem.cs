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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for DataConstantReferenceMenuItem.
/// </summary>
[SchemaItemDescription(name: "Data Constant Reference", iconName: "menu_parameter.png")]
[HelpTopic(topic: "Data+Constant+Menu+Item")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataConstantReferenceMenuItem : AbstractMenuItem
{
    public DataConstantReferenceMenuItem()
        : base() { }

    public DataConstantReferenceMenuItem(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public DataConstantReferenceMenuItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Constant);
        if (DataLookup != null)
        {
            dependencies.Add(item: DataLookup);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #region Properties
    public Guid DataConstantId;

    [Category(category: "Data Constant Reference")]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "constant", idField: "DataConstantId")]
    public DataConstant Constant
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataConstantId;
            return (DataConstant)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.DataConstantId = Guid.Empty;
            }
            else
            {
                this.DataConstantId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    public Guid DataLookupId;

    [Category(category: "User Interface")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [Description(
        description: "Optional data lookup which will be used to display the drop-down box. If not specified, the lookup defined in DataConstant will be used."
    )]
    [XmlReference(attributeName: "dataLookup", idField: "DataLookupId")]
    public IDataLookup DataLookup
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataLookupId;
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.DataLookupId = Guid.Empty;
            }
            else
            {
                this.DataLookupId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [Browsable(browsable: false)]
    public IDataLookup FinalLookup
    {
        get { return DataLookup ?? Constant.DataLookup; }
    }
    private bool _refreshPortalAfterSave = false;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "refreshPortalAfterSave")]
    [Description(description: "If true, the client will refresh its menu after saving data.")]
    public bool RefreshPortalAfterSave
    {
        get { return _refreshPortalAfterSave; }
        set { _refreshPortalAfterSave = value; }
    }
    #endregion
}
