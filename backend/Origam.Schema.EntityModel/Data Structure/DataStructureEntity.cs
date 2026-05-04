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
using Origam.DA.ObjectPersistence.Attributes;

namespace Origam.Schema.EntityModel;

public enum RelationType
{
    Normal,
    FilterParent,
    NotExists,
    LeftJoin,
    InnerJoin,
}

[SchemaItemDescription(name: "Entity", folderName: "Entities", iconName: "icon_entity.png")]
[HelpTopic(topic: "Entities")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Entity")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureEntity : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureEntity";

    public DataStructureEntity() { }

    public DataStructureEntity(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureEntity(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid EntityId = Guid.Empty;

    /// <summary>
    /// Can be IDataEntity (as a root entity of the data structure) or
    /// IAssociation (as a child entity of another entity in the data structure).
    /// </summary>
    [TypeConverter(type: typeof(DataStructureEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [RelationshipWithKeyRule()]
    [XmlReference(attributeName: "entity", idField: "EntityId")]
    public ISchemaItem Entity
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: EntityId)
                );
        set
        {
            if (value == null)
            {
                EntityId = Guid.Empty;
                Name = "";
            }
            else
            {
                if (!(value is IDataEntity || value is IAssociation))
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "Entity",
                        actualValue: value,
                        message: ResourceUtils.GetString(key: "ErrorNotIDataItem")
                    );
                }
                EntityId = (Guid)value.PrimaryKey[key: "Id"];
                Name = Entity.Name;
            }
        }
    }

    [Browsable(browsable: false)]
    public IDataEntity EntityDefinition
    {
        get
        {
            switch (Entity)
            {
                case IDataEntity _:
                {
                    return Entity as IDataEntity;
                }
                case IAssociation _:
                {
                    return ((IAssociation)Entity).AssociatedEntity;
                }
                default:
                {
                    if (Entity != null)
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "Entity",
                            actualValue: this.Entity,
                            message: ResourceUtils.GetString(key: "ErrorNotIDataEntity")
                        );
                    }
                    break;
                }
            }
            return null;
        }
    }

    [Browsable(browsable: false)]
    public DataStructureEntity RootEntity => GetRootEntity(parentEntity: this);

    private DataStructureEntity GetRootEntity(DataStructureEntity parentEntity)
    {
        return parentEntity.ParentItem is DataStructure
            ? parentEntity
            : GetRootEntity(parentEntity: parentEntity.ParentItem as DataStructureEntity);
    }

    private string _caption = "";

    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get => _caption;
        set => _caption = value;
    }
    private bool _allFields = true;

    [XmlAttribute(attributeName: "allFields")]
    public bool AllFields
    {
        get => _allFields;
        set
        {
            if (value == _allFields)
            {
                return;
            }
            if (value)
            {
                List<DataStructureColumn> list = ChildItemsByType<DataStructureColumn>(
                    itemType: DataStructureColumn.CategoryConst
                );
                foreach (DataStructureColumn column in list)
                {
                    if (
                        column.Entity == null
                        && !column.UseCopiedValue
                        && !column.UseLookupValue
                        && column.Field.Name == column.Name
                    )
                    {
                        column.IsDeleted = true;
                    }
                }
            }
            _allFields = value;
        }
    }
    private bool _ignoreImplicitFilters = false;

    [DefaultValue(value: false)]
    [Description(
        description: "Disables row level security filters for an entity. Row-level filters are defined under entitities."
    )]
    [XmlAttribute(attributeName: "ignoreImplicitFilters")]
    public bool IgnoreImplicitFilters
    {
        get => _ignoreImplicitFilters;
        set => _ignoreImplicitFilters = value;
    }
    private DataStructureIgnoreCondition _ignoreCondition = DataStructureIgnoreCondition.None;

    [DefaultValue(value: DataStructureIgnoreCondition.None)]
    [Description(
        description: "Specify the condition resulting in not adding the whole entity to data query. Value 'IgnoreWhenNoFilters' means that the entity is skipped when neither one filter would be constructed for that entity. Value 'IgnoreWhenNoExplicitFilters' means the same as 'IgnoreWhenNoFilters' but it doesn't count implicit filters (aka row level security filters), so only datastructure filters are examined. Note, that filters can be avoided from construction according to their ignore condition settings and provided the whole corresponding filterset is 'dynamic'"
    )]
    [XmlAttribute(attributeName: "ignoreCondition")]
    public DataStructureIgnoreCondition IgnoreCondition
    {
        get => _ignoreCondition;
        set => _ignoreCondition = value;
    }
    private DataStructureConcurrencyHandling _concurrencyHandling =
        DataStructureConcurrencyHandling.Standard;

    [DefaultValue(value: DataStructureConcurrencyHandling.Standard)]
    [Description(
        description: "Specify behaviour during cuncurrency handling. Standard - concurrency checks are performed; LastWins - no concurrency checks are performed."
    )]
    [XmlAttribute(attributeName: "concurrencyHandling")]
    public DataStructureConcurrencyHandling ConcurrencyHandling
    {
        get => _concurrencyHandling;
        set => _concurrencyHandling = value;
    }
    private RelationType _relationType = RelationType.Normal;

    [RelationTypeModelElementRule()]
    [XmlAttribute(attributeName: "relationType")]
    public RelationType RelationType
    {
        get => _relationType;
        set => _relationType = value;
    }
    public Guid ConditionEntityConstantId;

    [TypeConverter(type: typeof(DataConstantConverter))]
    [XmlReference(attributeName: "conditionEntityConstant", idField: "ConditionEntityConstantId")]
    public DataConstant ConditionEntityConstant
    {
        get =>
            (DataConstant)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(EntityFilter),
                    primaryKey: new ModelElementKey(id: ConditionEntityConstantId)
                );
        set =>
            ConditionEntityConstantId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    private string _conditionEntityParameterName;

    [Description(
        description: "When defined (e.g. Resource_parName) together with a value of 'ConditionEntityConstant' then a value of the parameter is tested whether equals to a value of 'ConditionalEntityConstant'. If equals then an entity is APPLIED to resulting data query, otherwise the entity is skipped. When not defined, entity is allways applied."
    )]
    [XmlAttribute(attributeName: "conditionEntityParameterName")]
    public string ConditionEntityParameterName
    {
        get => _conditionEntityParameterName;
        set => _conditionEntityParameterName = (value == "") ? null : value;
    }
    private bool _serializeAsSingleJsonObject = false;

    [DefaultValue(value: false)]
    [Description(
        description: "It controls JSON serialization to API. If false it always serializes as array of objects. If true it serializes as a single JSON object and return an error if there are more objects."
    )]
    [XmlAttribute(attributeName: "serializeAsSingleJsonObject")]
    public bool SerializeAsSingleJsonObject
    {
        get => _serializeAsSingleJsonObject;
        set => _serializeAsSingleJsonObject = value;
    }
    private bool _useUpsert = false;

    [Category(category: "Update"), DefaultValue(value: false)]
    [XmlAttribute(attributeName: "useUpsert")]
    public bool UseUpsert
    {
        get => _useUpsert;
        set => _useUpsert = value;
    }
#if ORIGAM_CLIENT
    private bool _columnsPopulated = false;
    private List<DataStructureColumn> _columns = new List<DataStructureColumn>();
#endif

    [Browsable(browsable: false)]
    public List<DataStructureColumn> Columns
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
                        _columns = GetColumns();
                        _columnsPopulated = true;
                    }
                }
            }
            return _columns;
#else
            return GetColumns();
#endif
        }
    }

    public DataStructureColumn Column(string name)
    {
        return Columns.FirstOrDefault(predicate: column => column.Name == name);
    }

    public List<DataStructureColumn> GetColumnsFromEntity()
    {
        var columns = new List<DataStructureColumn>();
        if (!(AllFields && EntityId != Guid.Empty))
        {
            return columns;
        }
        foreach (IDataEntityColumn column in EntityDefinition.EntityColumns)
        {
            if (column.ExcludeFromAllFields)
            {
                continue;
            }
            var newColumn = new DataStructureColumn(schemaExtensionId: SchemaExtensionId);
            newColumn.IsPersistable = false;
            newColumn.PersistenceProvider = PersistenceProvider;
            newColumn.Field = column;
            newColumn.Name = column.Name;
            newColumn.ParentItem = this;
            columns.Add(item: newColumn);
        }
        return columns;
    }

    public bool ExistsEntityFieldAsColumn(IDataEntityColumn entityField)
    {
        return Columns.Any(predicate: column =>
            column.Field.PrimaryKey.Equals(obj: entityField.PrimaryKey)
        );
    }

    private List<DataStructureColumn> GetColumns()
    {
        // columns from entity (AllFields=true)
        List<DataStructureColumn> columns = GetColumnsFromEntity();
        // add all extra columns specified
        columns.AddRange(
            collection: ChildItemsByType<DataStructureColumn>(
                itemType: DataStructureColumn.CategoryConst
            )
        );
        columns.Sort();
        return columns;
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        // relation has parameters (i.e. there are parameters in the JOIN clause
        if (Entity is IAssociation)
        {
            var childList = new Dictionary<string, ParameterReference>();
            Entity.GetParameterReferences(parentItem: Entity, list: childList);
            // If children had some parameter references, we rename them and add them to the final
            // collection.
            foreach (var entry in childList)
            {
                // we rename it using parent data structure entity name
                var name = ParentItem.Name + "_" + entry.Key;
                if (!list.ContainsKey(key: name))
                {
                    list.Add(key: name, value: entry.Value);
                }
            }
        }
        foreach (DataStructureColumn dataStructureColumn in Columns)
        {
            var childList = new Dictionary<string, ParameterReference>();
            dataStructureColumn.GetParameterReferences(
                parentItem: dataStructureColumn,
                list: childList
            );
            // If children had some parameter references,
            // we rename them and add them to the final collection.
            foreach (var entry in childList)
            {
                var name = Name + "_" + entry.Key;
                if (!list.ContainsKey(key: name))
                {
                    list.Add(key: name, value: entry.Value);
                }
            }
        }
    }

    public override string ItemType => CategoryConst;

    public override bool CanMove(UI.IBrowserNode2 newNode) =>
        // can move to the root only
        newNode.Equals(obj: RootItem);

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Entity);
        dependencies.Add(item: ConditionEntityConstant);
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(browsable: false)]
    public override Type[] NewItemTypes =>
        new[] { typeof(DataStructureEntity), typeof(DataStructureColumn) };
    #endregion
}
