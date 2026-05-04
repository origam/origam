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
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Maps physical table to an entity.
/// </summary>
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractDataEntity : AbstractSchemaItem, IDataEntity, ISchemaItemFactory
{
    public AbstractDataEntity()
        : base()
    {
        Init();
    }

    public AbstractDataEntity(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractDataEntity(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    public const string CategoryConst = "DataEntity";

    private void Init()
    {
        this.ChildItemTypes.InsertRange(
            index: 0,
            collection: new Type[]
            {
                typeof(FieldMappingItem),
                typeof(DetachedField),
                typeof(FunctionCall),
                typeof(AggregatedColumn),
                typeof(LookupField),
                typeof(EntityRelationItem),
                typeof(DatabaseParameter),
                typeof(EntityFilter),
                typeof(DataEntityIndex),
                typeof(EntitySecurityFilterReference),
                typeof(EntitySecurityRule),
                typeof(EntityConditionalFormatting),
            }
        );
    }

    #region IDataEntity Members
    [Browsable(browsable: false)]
    public List<SchemaItemParameter> EntityParameters =>
        ChildItemsByType<SchemaItemParameter>(itemType: SchemaItemParameter.CategoryConst);

    [Browsable(browsable: false)]
    public virtual List<IDataEntityColumn> EntityPrimaryKey
    {
        get
        {
            var list = new List<IDataEntityColumn>();
            foreach (IDataEntityColumn column in this.EntityColumns)
            {
                if (column.IsPrimaryKey)
                {
                    list.Add(item: column);
                }
            }
            return list;
        }
    }
    public Guid DescribingFieldId;

    [TypeConverter(type: typeof(EntityColumnReferenceConverter))]
    [XmlReference(attributeName: "describingField", idField: "DescribingFieldId")]
    public IDataEntityColumn DescribingField
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.DescribingFieldId)
                    ) as IDataEntityColumn;
        }
        set => DescribingFieldId = (Guid?)value?.PrimaryKey[key: "Id"] ?? Guid.Empty;
    }
    bool _entityIsReadOnly = false;

    [Category(category: "Entity"), DefaultValue(value: false)]
    [Browsable(browsable: false)]
    [XmlAttribute(attributeName: "readOnly")]
    public bool EntityIsReadOnly
    {
        get { return _entityIsReadOnly; }
        set { _entityIsReadOnly = value; }
    }
    EntityAuditingType _auditingType = EntityAuditingType.None;

    [Category(category: "Entity"), DefaultValue(value: EntityAuditingType.None)]
    [Description(
        description: "Indicates if audit trail will be recorded for changes in this entity. If set to All, every change (create/update/delete) will be recorded in the audit log that users can browse in the UI. If set UpdatesAndDeletes only update and delete changes will be recorded."
    )]
    [XmlAttribute(attributeName: "audit")]
    public EntityAuditingType AuditingType
    {
        get { return _auditingType; }
        set { _auditingType = value; }
    }
    public Guid AuditingSecondReferenceKeyColumnId;

    [TypeConverter(type: typeof(EntityColumnReferenceConverter))]
    [Category(category: "Entity")]
    [Description(
        description: "If auditing is enabled and this value is filled, system will store value of designated column to audit log when recording delete operation."
    )]
    [XmlReference(
        attributeName: "auditingSecondReferenceKeyColumn",
        idField: "AuditingSecondReferenceKeyColumnId"
    )]
    public IDataEntityColumn AuditingSecondReferenceKeyColumn
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: AuditingSecondReferenceKeyColumnId)
                ) as IDataEntityColumn;
        set =>
            AuditingSecondReferenceKeyColumnId = (Guid?)value?.PrimaryKey[key: "Id"] ?? Guid.Empty;
    }
    private string _caption = "";

    [Category(category: "Entity")]
    [Localizable(isLocalizable: true)]
    [Description(
        description: "User interface label for this entity. It is used e.g. for generic error messages about the entity ('Error occured in Invoice' instead of 'Error occured in InvDocRec')."
    )]
    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
#if ORIGAM_CLIENT
    private bool _columnsPopulated = false;
    private List<IDataEntityColumn> _columns;
#endif

    [Browsable(browsable: false)]
    public List<IDataEntityColumn> EntityColumns
    {
        get
        {
#if ORIGAM_CLIENT
            if (!_columnsPopulated)
            {
                lock (Lock)
                {
                    if (!_columnsPopulated)
                    {
                        _columns = ChildItemsByType<IDataEntityColumn>(
                            itemType: AbstractDataEntityColumn.CategoryConst
                        );
                        _columnsPopulated = true;
                    }
                }
            }
            return _columns;
#else
            return this.ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst);
#endif
        }
    }

    [Browsable(browsable: false)]
    public List<EntityRelationItem> EntityRelations =>
        ChildItemsByType<EntityRelationItem>(itemType: EntityRelationItem.CategoryConst);

    [Browsable(browsable: false)]
    public List<IDataEntity> ChildEntities
    {
        get
        {
            var result = new List<IDataEntity>();
            foreach (EntityRelationItem relation in this.EntityRelations)
            {
                if (relation.IsParentChild)
                {
                    result.Add(item: relation.RelatedEntity);
                }
            }
            return result;
        }
    }

    [Browsable(browsable: false)]
    public List<IDataEntity> ChildEntitiesRecursive
    {
        get
        {
            var result = new List<IDataEntity>();
            foreach (IDataEntity entity in this.ChildEntities)
            {
                result.Add(item: entity);
                result.AddRange(collection: entity.ChildEntitiesRecursive);
            }
            return result;
        }
    }

    [Browsable(browsable: false)]
    public List<EntityFilter> EntityFilters =>
        ChildItemsByType<EntityFilter>(itemType: EntityFilter.CategoryConst);

    [Browsable(browsable: false)]
    public List<DataEntityIndex> EntityIndexes =>
        ChildItemsByType<DataEntityIndex>(itemType: DataEntityIndex.CategoryConst);

    [Browsable(browsable: false)]
    public List<AbstractEntitySecurityRule> RowLevelSecurityRules =>
        ChildItemsByType<AbstractEntitySecurityRule>(
            itemType: AbstractEntitySecurityRule.CategoryConst
        );

    [Browsable(browsable: false)]
    public List<EntityConditionalFormatting> ConditionalFormattingRules =>
        ChildItemsByType<EntityConditionalFormatting>(
            itemType: EntityConditionalFormatting.CategoryConst
        );

    [Browsable(browsable: false)]
    public List<DataEntityConstraint> Constraints
    {
        get
        {
            var result = new List<DataEntityConstraint>();
            DataEntityConstraint pk = new DataEntityConstraint(type: ConstraintType.PrimaryKey);
            foreach (IDataEntityColumn column in this.EntityColumns)
            {
                if (column.IsPrimaryKey)
                {
                    pk.Fields.Add(item: column);
                }
                DataEntityConstraint foreignKey = column.ForeignKeyConstraint;
                if (foreignKey != null)
                {
                    result.Add(item: foreignKey);
                }
            }
            if (pk.Fields.Count > 0)
            {
                result.Add(item: pk);
            }

            return result;
        }
    }

    [Browsable(browsable: false)]
    public bool HasEntityAFieldDenyReadRule()
    {
        if (
            ChildItemsByTypeRecursive(itemType: EntityFieldSecurityRule.CategoryConst)
                .ToArray()
                .Cast<EntityFieldSecurityRule>()
                .Where(predicate: rule => rule.Type == PermissionType.Deny && rule.ReadCredential)
                .Count() > 0
        )
        {
            return true;
        }

        return false;
    }
    #endregion
    #region Overriden ISchemaItem Methods
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.DescribingField != null)
        {
            dependencies.Add(item: this.DescribingField);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
