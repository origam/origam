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
/// Summary description for EntityRelationColumnPairItem.
/// </summary>
[SchemaItemDescription(name: "Key", icon: 3)]
[HelpTopic(topic: "Relationship+Key")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "BaseEntityField")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityRelationColumnPairItem : AbstractSchemaItem
{
    public const string CategoryConst = "EntityRelationColumnPair";

    public EntityRelationColumnPairItem()
        : base() { }

    public EntityRelationColumnPairItem(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public EntityRelationColumnPairItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid BaseEntityColumnId;

    [TypeConverter(type: typeof(RelationPrimaryKeyColumnConverter))]
    [NotNullModelElementRuleAttribute()]
    [XmlReference(attributeName: "baseEntityField", idField: "BaseEntityColumnId")]
    public IDataEntityColumn BaseEntityField
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.BaseEntityColumnId;
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.BaseEntityColumnId = Guid.Empty;
            }
            else
            {
                this.BaseEntityColumnId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    public Guid RelatedEntityColumnId;

    [TypeConverter(type: typeof(RelationForeignKeyColumnConverter))]
    [NotNullModelElementRuleAttribute()]
    [XmlReference(attributeName: "relatedEntityField", idField: "RelatedEntityColumnId")]
    public IDataEntityColumn RelatedEntityField
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.RelatedEntityColumnId;
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.RelatedEntityColumnId = Guid.Empty;
            }
            else
            {
                this.RelatedEntityColumnId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string Icon
    {
        get { return "3"; }
    }
    public override string ItemType
    {
        get { return EntityRelationColumnPairItem.CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        BaseEntityField.GetParameterReferences(parentItem: BaseEntityField, list: list);
        RelatedEntityField.GetParameterReferences(parentItem: RelatedEntityField, list: list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.BaseEntityField);
        dependencies.Add(item: this.RelatedEntityField);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.BaseEntityField.PrimaryKey))
                {
                    this.BaseEntityField = item as IDataEntityColumn;
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
}
