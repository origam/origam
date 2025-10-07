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
using System.Collections;
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
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractDataEntity : AbstractSchemaItem, IDataEntity, ISchemaItemFactory
{
    public AbstractDataEntity()
        : base()
    {
        Init();
    }

    public AbstractDataEntity(Guid schemaExtensionId)
        : base(schemaExtensionId)
    {
        Init();
    }

    public AbstractDataEntity(Key primaryKey)
        : base(primaryKey)
    {
        Init();
    }

    public const string CategoryConst = "DataEntity";

    private void Init()
    {
        this.ChildItemTypes.InsertRange(
            0,
            new Type[]
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
    [Browsable(false)]
    public List<SchemaItemParameter> EntityParameters =>
        ChildItemsByType<SchemaItemParameter>(SchemaItemParameter.CategoryConst);

    [Browsable(false)]
    public virtual List<IDataEntityColumn> EntityPrimaryKey
    {
        get
        {
            var list = new List<IDataEntityColumn>();
            foreach (IDataEntityColumn column in this.EntityColumns)
            {
                if (column.IsPrimaryKey)
                    list.Add(column);
            }
            return list;
        }
    }
    public Guid DescribingFieldId;

    [TypeConverter(typeof(EntityColumnReferenceConverter))]
    [XmlReference("describingField", "DescribingFieldId")]
    public IDataEntityColumn DescribingField
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(this.DescribingFieldId)
                    ) as IDataEntityColumn;
        }
        set => DescribingFieldId = (Guid?)value?.PrimaryKey["Id"] ?? Guid.Empty;
    }
    bool _entityIsReadOnly = false;

    [Category("Entity"), DefaultValue(false)]
    [Browsable(false)]
    [XmlAttribute("readOnly")]
    public bool EntityIsReadOnly
    {
        get { return _entityIsReadOnly; }
        set { _entityIsReadOnly = value; }
    }
    EntityAuditingType _auditingType = EntityAuditingType.None;

    [Category("Entity"), DefaultValue(EntityAuditingType.None)]
    [Description(
        "Indicates if audit trail will be recorded for changes in this entity. If set to All, every change (create/update/delete) will be recorded in the audit log that users can browse in the UI. If set UpdatesAndDeletes only update and delete changes will be recorded."
    )]
    [XmlAttribute("audit")]
    public EntityAuditingType AuditingType
    {
        get { return _auditingType; }
        set { _auditingType = value; }
    }
    public Guid AuditingSecondReferenceKeyColumnId;

    [TypeConverter(typeof(EntityColumnReferenceConverter))]
    [Category("Entity")]
    [Description(
        "If auditing is enabled and this value is filled, system will store value of designated column to audit log when recording delete operation."
    )]
    [XmlReference("auditingSecondReferenceKeyColumn", "AuditingSecondReferenceKeyColumnId")]
    public IDataEntityColumn AuditingSecondReferenceKeyColumn
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(AuditingSecondReferenceKeyColumnId)
                ) as IDataEntityColumn;
        set => AuditingSecondReferenceKeyColumnId = (Guid?)value?.PrimaryKey["Id"] ?? Guid.Empty;
    }
    private string _caption = "";

    [Category("Entity")]
    [Localizable(true)]
    [Description(
        "User interface label for this entity. It is used e.g. for generic error messages about the entity ('Error occured in Invoice' instead of 'Error occured in InvDocRec')."
    )]
    [XmlAttribute("label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
#if ORIGAM_CLIENT
    private bool _columnsPopulated = false;
    private List<IDataEntityColumn> _columns;
#endif

    [Browsable(false)]
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
                            AbstractDataEntityColumn.CategoryConst
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

    [Browsable(false)]
    public List<EntityRelationItem> EntityRelations =>
        ChildItemsByType<EntityRelationItem>(EntityRelationItem.CategoryConst);

    [Browsable(false)]
    public List<IDataEntity> ChildEntities
    {
        get
        {
            var result = new List<IDataEntity>();
            foreach (EntityRelationItem relation in this.EntityRelations)
            {
                if (relation.IsParentChild)
                    result.Add(relation.RelatedEntity);
            }
            return result;
        }
    }

    [Browsable(false)]
    public List<IDataEntity> ChildEntitiesRecursive
    {
        get
        {
            var result = new List<IDataEntity>();
            foreach (IDataEntity entity in this.ChildEntities)
            {
                result.Add(entity);
                result.AddRange(entity.ChildEntitiesRecursive);
            }
            return result;
        }
    }

    [Browsable(false)]
    public List<EntityFilter> EntityFilters =>
        ChildItemsByType<EntityFilter>(EntityFilter.CategoryConst);

    [Browsable(false)]
    public List<DataEntityIndex> EntityIndexes =>
        ChildItemsByType<DataEntityIndex>(DataEntityIndex.CategoryConst);

    [Browsable(false)]
    public List<AbstractEntitySecurityRule> RowLevelSecurityRules =>
        ChildItemsByType<AbstractEntitySecurityRule>(AbstractEntitySecurityRule.CategoryConst);

    [Browsable(false)]
    public List<EntityConditionalFormatting> ConditionalFormattingRules =>
        ChildItemsByType<EntityConditionalFormatting>(EntityConditionalFormatting.CategoryConst);

    [Browsable(false)]
    public List<DataEntityConstraint> Constraints
    {
        get
        {
            var result = new List<DataEntityConstraint>();
            DataEntityConstraint pk = new DataEntityConstraint(ConstraintType.PrimaryKey);
            foreach (IDataEntityColumn column in this.EntityColumns)
            {
                if (column.IsPrimaryKey)
                {
                    pk.Fields.Add(column);
                }
                DataEntityConstraint foreignKey = column.ForeignKeyConstraint;
                if (foreignKey != null)
                {
                    result.Add(foreignKey);
                }
            }
            if (pk.Fields.Count > 0)
                result.Add(pk);
            return result;
        }
    }

    [Browsable(false)]
    public bool HasEntityAFieldDenyReadRule()
    {
        if (
            ChildItemsByTypeRecursive(EntityFieldSecurityRule.CategoryConst)
                .ToArray()
                .Cast<EntityFieldSecurityRule>()
                .Where(rule => rule.Type == PermissionType.Deny && rule.ReadCredential)
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
            dependencies.Add(this.DescribingField);
        base.GetExtraDependencies(dependencies);
    }

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
