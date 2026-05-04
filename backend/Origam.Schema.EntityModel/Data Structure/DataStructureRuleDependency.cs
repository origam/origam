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
/// Summary description for DataStructureRuleDependency.
/// </summary>
[SchemaItemDescription(
    name: "Dependency",
    folderName: "Dependencies",
    iconName: "icon_rule-dependency.png"
)]
[HelpTopic(topic: "Rule+Set+Rule+Dependency")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Entity")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureRuleDependency : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureRuleDependency";

    public DataStructureRuleDependency()
        : base() { }

    public DataStructureRuleDependency(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureRuleDependency(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
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
            this.DataStructureEntityId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            this.Field = null;
        }
    }

    public Guid FieldId;

    [TypeConverter(type: typeof(DataStructureEntityFieldConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "field", idField: "FieldId")]
    public IDataEntityColumn Field
    {
        get
        {
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.FieldId)
                );
        }
        set
        {
            this.FieldId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
            UpdateName();
        }
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
        dependencies.Add(item: this.Field);
        base.GetExtraDependencies(dependencies: dependencies);
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
    #endregion
    #region Private Methods
    private void UpdateName()
    {
        if (this.Entity != null && this.Field != null)
        {
            this.Name = this.Entity.Name + "_" + this.Field.Name;
        }
    }
    #endregion
}
