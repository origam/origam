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
/// Summary description for DataStructureTemplate.
/// </summary>
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class DataStructureTemplate : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureTemplate";

    public DataStructureTemplate()
        : base() { }

    public DataStructureTemplate(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureTemplate(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid DataStructureEntityId;

    [TypeConverter(type: typeof(DataQueryEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRuleAttribute()]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataStructureEntityId;
            return (DataStructureEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.DataStructureEntityId = Guid.Empty;
            }
            else
            {
                this.DataStructureEntityId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string Icon
    {
        get { return "5"; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Entity);
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
    #endregion
}
