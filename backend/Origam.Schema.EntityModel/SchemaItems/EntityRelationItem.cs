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

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(
    name: "Relationship",
    folderName: "Relationships",
    iconName: "icon_relationship.png"
)]
[HelpTopic(topic: "Relationships")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "RelatedEntity")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityRelationItem : AbstractSchemaItem, IAssociation
{
    public EntityRelationItem() { }

    public EntityRelationItem(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public EntityRelationItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public const string CategoryConst = "EntityRelation";
    #region Properties
    public Guid RelatedEntityId;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "relatedEntity", idField: "RelatedEntityId")]
    public IDataEntity RelatedEntity
    {
        get
        {
            var key = new ModelElementKey { Id = RelatedEntityId };
            return (IDataEntity)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                RelatedEntityId = Guid.Empty;
                Name = "";
            }
            else
            {
                RelatedEntityId = (Guid)value.PrimaryKey[key: "Id"];
                Name = RelatedEntity.Name;
            }
            // We have to delete all child items
            ChildItems.Clear();
        }
    }
    private bool _isParentChild = false;

    [XmlAttribute(attributeName: "parentChild")]
    public bool IsParentChild
    {
        get => _isParentChild;
        set => _isParentChild = value;
    }

    [SelfJoinSameBaseRule]
    [XmlAttribute(attributeName: "selfJoin")]
    public bool IsSelfJoin { get; set; }
    private bool _isOR = false;

    [XmlAttribute(attributeName: "or")]
    public bool IsOR
    {
        get => _isOR;
        set => _isOR = value;
    }
    #endregion
    #region Overriden ISchemaItem Members

    public override bool UseFolders => false;
    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        try
        {
            dependencies.Add(item: RelatedEntity);
        }
        catch
        {
            throw new ArgumentOutOfRangeException(
                paramName: "RelatedEntityId",
                actualValue: RelatedEntityId,
                message: ResourceUtils.GetString(
                    key: "ErrorRelatedEntity",
                    args: new object[] { Name, BaseEntity.Name }
                )
            );
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        var item = newNode as ISchemaItem;
        return (item != null) && item.PrimaryKey.Equals(obj: ParentItem.PrimaryKey);
    }
    #endregion
    #region IAssociation Members
    [Browsable(browsable: false)]
    public IDataEntity BaseEntity => ParentItem as IDataEntity;

    [Browsable(browsable: false)]
    public IDataEntity AssociatedEntity => RelatedEntity;
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(browsable: false)]
    public override Type[] NewItemTypes =>
        new[] { typeof(EntityRelationColumnPairItem), typeof(EntityRelationFilter) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(EntityRelationColumnPairItem))
        {
            itemName = Name + "Key" + (ChildItems.Count + 1);
        }
        else if (typeof(T) == typeof(EntityRelationFilter))
        {
            itemName = "NewEntityRelationFilter";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
