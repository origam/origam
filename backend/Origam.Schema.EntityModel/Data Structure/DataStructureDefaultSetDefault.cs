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
/// Summary description for DataStructureDefaultSetDefault.
/// </summary>
[SchemaItemDescription(name: "Default", iconName: "icon_default.png")]
[HelpTopic(topic: "Default+Sets")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureDefaultSetDefault : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureDefaultSetDefault";

    public DataStructureDefaultSetDefault()
        : base() { }

    public DataStructureDefaultSetDefault(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureDefaultSetDefault(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid DataConstantId;

    [TypeConverter(type: typeof(DataConstantConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "default", idField: "DataConstantId")]
    public DataConstant Default
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataConstantId;
            return (DataConstant)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataConstant),
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
            UpdateName();
        }
    }

    public Guid DataStructureEntityId;

    [TypeConverter(type: typeof(DataQueryEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            return (DataStructureEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new ModelElementKey(id: this.DataStructureEntityId)
                );
        }
        set
        {
            this.DataStructureEntityId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            this.Field = null;
            UpdateName();
        }
    }
    public Guid EntityFieldId;

    [TypeConverter(type: typeof(DataStructureEntityFieldConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "field", idField: "EntityFieldId")]
    public IDataEntityColumn Field
    {
        get
        {
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new ModelElementKey(id: this.EntityFieldId)
                );
        }
        set
        {
            this.EntityFieldId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            UpdateName();
        }
    }

    public Guid ParameterId;

    [TypeConverter(type: typeof(ParameterReferenceConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "parameter", idField: "ParameterId")]
    public SchemaItemParameter Parameter
    {
        get
        {
            return (SchemaItemParameter)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(SchemaItemParameter),
                        primaryKey: new ModelElementKey(id: this.ParameterId)
                    ) as SchemaItemParameter;
        }
        set { this.ParameterId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Entity);
        dependencies.Add(item: this.Default);
        dependencies.Add(item: this.Field);
        if (this.Parameter != null)
        {
            dependencies.Add(item: this.Parameter);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Parameter != null)
        {
            if (!list.ContainsKey(key: this.Parameter.Name))
            {
                ParameterReference pr = new ParameterReference();
                pr.PersistenceProvider = this.PersistenceProvider;
                pr.Parameter = this.Parameter;
                pr.Name = this.Parameter.Name;
                list.Add(key: this.Parameter.Name, value: pr);
            }
        }
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Entity.PrimaryKey))
                {
                    this.Entity = item as DataStructureEntity;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        ISchemaItem item = newNode as ISchemaItem;
        return item != null && item.PrimaryKey.Equals(obj: this.ParentItem.PrimaryKey);
    }
    #endregion
    #region Private Methods
    private void UpdateName()
    {
        string entity = this.Entity == null ? "" : this.Entity.Name;
        string field = this.Field == null ? "" : this.Field.Name;
        string def = this.Default == null ? "" : this.Default.Name;
        this.Name = entity + "_" + field + "_" + def;
    }
    #endregion
}
