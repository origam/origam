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
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(name: "Work Queue Class", iconName: "work-queue-class.png")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkQueueClass : AbstractSchemaItem
{
    public const string CategoryConst = "WorkQueueClass";

    public WorkQueueClass()
        : base()
    {
        Init();
    }

    public WorkQueueClass(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public WorkQueueClass(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    public WorkQueueWorkflowCommand GetCommand(string name)
    {
        WorkQueueWorkflowCommand cmd =
            this.GetChildByName(name: name, itemType: WorkQueueWorkflowCommand.CategoryConst)
            as WorkQueueWorkflowCommand;
        if (cmd == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "name",
                actualValue: name,
                message: ResourceUtils.GetString(key: "ErrorUknownWorkQueueCommand")
            );
        }

        return cmd;
    }

    public WorkqueueLoader GetLoader(string name)
    {
        WorkqueueLoader loader =
            this.GetChildByName(name: name, itemType: WorkqueueLoader.CategoryConst)
            as WorkqueueLoader;
        if (loader == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "name",
                actualValue: name,
                message: ResourceUtils.GetString(key: "ErrorUknownWorkQueueLoader")
            );
        }

        return loader;
    }

    private void Init()
    {
        ChildItemTypes.Add(item: typeof(WorkQueueClassEntityMapping));
        ChildItemTypes.Add(item: typeof(WorkQueueWorkflowCommand));
        ChildItemTypes.Add(item: typeof(WorkqueueLoader));
        ChildItemTypes.Add(item: typeof(WorkQueueCustomScreen));
    }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.EntityStructure != null)
        {
            dependencies.Add(item: this.EntityStructure);
        }

        if (this.EntityStructurePrimaryKeyMethod != null)
        {
            dependencies.Add(item: this.EntityStructurePrimaryKeyMethod);
        }

        if (this.Entity != null)
        {
            dependencies.Add(item: this.Entity);
        }

        if (this.ConditionFilter != null)
        {
            dependencies.Add(item: this.ConditionFilter);
        }

        if (this.RelatedEntity1 != null)
        {
            dependencies.Add(item: this.RelatedEntity1);
        }

        if (this.RelatedEntity2 != null)
        {
            dependencies.Add(item: this.RelatedEntity2);
        }

        if (this.RelatedEntity3 != null)
        {
            dependencies.Add(item: this.RelatedEntity3);
        }

        if (this.RelatedEntity4 != null)
        {
            dependencies.Add(item: this.RelatedEntity4);
        }

        if (this.RelatedEntity5 != null)
        {
            dependencies.Add(item: this.RelatedEntity5);
        }

        if (this.RelatedEntity6 != null)
        {
            dependencies.Add(item: this.RelatedEntity6);
        }

        if (this.RelatedEntity7 != null)
        {
            dependencies.Add(item: this.RelatedEntity7);
        }

        if (this.WorkQueueStructure != null)
        {
            dependencies.Add(item: this.WorkQueueStructure);
        }

        if (this.WorkQueueStructureUserListMethod != null)
        {
            dependencies.Add(item: this.WorkQueueStructureUserListMethod);
        }

        if (this.NotificationLoadMethod != null)
        {
            dependencies.Add(item: this.NotificationLoadMethod);
        }

        if (this.NotificationStructure != null)
        {
            dependencies.Add(item: this.NotificationStructure);
        }

        if (this.WorkQueueItemCountLookup != null)
        {
            dependencies.Add(item: this.WorkQueueItemCountLookup);
        }

        if (this.WorkQueueStructureSortSet != null)
        {
            dependencies.Add(item: this.WorkQueueStructureSortSet);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    [Browsable(browsable: false)]
    public List<WorkQueueClassEntityMapping> EntityMappings =>
        this.ChildItemsByType<WorkQueueClassEntityMapping>(
            itemType: WorkQueueClassEntityMapping.CategoryConst
        );
    public Guid EntityId;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "entity", idField: "EntityId")]
    public IDataEntity Entity
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: EntityId)
                );
        set
        {
            this.EntityId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
            this.ConditionFilter = null;
        }
    }
    public Guid EntityConditionFilterId;

    [TypeConverter(type: typeof(WorkQueueClassFilterConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "conditionFilter", idField: "EntityConditionFilterId")]
    public EntityFilter ConditionFilter
    {
        get =>
            (EntityFilter)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: EntityConditionFilterId)
                );
        set
        {
            this.EntityConditionFilterId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid WorkQueueStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [StructureMustHaveGetByIdFilterRule]
    [XmlReference(attributeName: "workQueueStructure", idField: "WorkQueueStructureId")]
    public DataStructure WorkQueueStructure
    {
        get =>
            (DataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: WorkQueueStructureId)
                );
        set
        {
            this.WorkQueueStructureId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            this.WorkQueueStructureSortSet = null;
            this.WorkQueueStructureUserListMethod = null;
        }
    }

    public Guid RelatedEntity1Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity1", idField: "RelatedEntity1Id")]
    public IDataEntity RelatedEntity1
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity1Id)
                );
        set
        {
            this.RelatedEntity1Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid RelatedEntity2Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity2", idField: "RelatedEntity2Id")]
    public IDataEntity RelatedEntity2
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity2Id)
                );
        set
        {
            this.RelatedEntity2Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid RelatedEntity3Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity3", idField: "RelatedEntity3Id")]
    public IDataEntity RelatedEntity3
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity3Id)
                );
        set
        {
            this.RelatedEntity3Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid RelatedEntity4Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity4", idField: "RelatedEntity4Id")]
    public IDataEntity RelatedEntity4
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity4Id)
                );
        set
        {
            this.RelatedEntity4Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid RelatedEntity5Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity5", idField: "RelatedEntity5Id")]
    public IDataEntity RelatedEntity5
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity5Id)
                );
        set
        {
            this.RelatedEntity5Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid RelatedEntity6Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity6", idField: "RelatedEntity6Id")]
    public IDataEntity RelatedEntity6
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity6Id)
                );
        set
        {
            this.RelatedEntity6Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid RelatedEntity7Id;

    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "relatedEntity7", idField: "RelatedEntity7Id")]
    public IDataEntity RelatedEntity7
    {
        get =>
            (IDataEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RelatedEntity7Id)
                );
        set
        {
            this.RelatedEntity7Id = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid EntityStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule(conditionField: "Entity")]
    [XmlReference(attributeName: "entityStructure", idField: "EntityStructureId")]
    public DataStructure EntityStructure
    {
        get =>
            (DataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: EntityStructureId)
                );
        set
        {
            this.EntityStructureId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            this.EntityStructurePrimaryKeyMethod = null;
        }
    }

    public Guid EntityStructurePkMethodId;

    [TypeConverter(type: typeof(WorkQueueClassEntityStructureFilterConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule(conditionField: "EntityStructure")]
    [XmlReference(
        attributeName: "entityStructurePrimaryKeyMethod",
        idField: "EntityStructurePkMethodId"
    )]
    public DataStructureMethod EntityStructurePrimaryKeyMethod
    {
        get =>
            (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: EntityStructurePkMethodId)
                );
        set
        {
            this.EntityStructurePkMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid WorkQueueStructureUserListMethodId;

    [TypeConverter(type: typeof(WorkQueueClassWQDataStructureFilterConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(
        attributeName: "workQueueStructureUserListMethod",
        idField: "WorkQueueStructureUserListMethodId"
    )]
    public DataStructureMethod WorkQueueStructureUserListMethod
    {
        get =>
            (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: WorkQueueStructureUserListMethodId)
                );
        set
        {
            this.WorkQueueStructureUserListMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid WorkQueueStructureSortSetId;

    [TypeConverter(type: typeof(WorkQueueClassWQDataStructureSortSetConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule]
    [XmlReference(
        attributeName: "workQueueStructureSortSet",
        idField: "WorkQueueStructureSortSetId"
    )]
    public DataStructureSortSet WorkQueueStructureSortSet
    {
        get =>
            (DataStructureSortSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: WorkQueueStructureSortSetId)
                );
        set
        {
            this.WorkQueueStructureSortSetId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid WorkQueueItemCountLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "workQueueItemCountLookup", idField: "WorkQueueItemCountLookupId")]
    public IDataLookup WorkQueueItemCountLookup
    {
        get =>
            (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: WorkQueueItemCountLookupId)
                );
        set
        {
            this.WorkQueueItemCountLookupId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid NotificationStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint), Category(category: "Notification")]
    [XmlReference(attributeName: "notificationStructure", idField: "NotificationStructureId")]
    public DataStructure NotificationStructure
    {
        get =>
            (DataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: NotificationStructureId)
                );
        set
        {
            this.NotificationStructureId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            this.NotificationLoadMethod = null;
        }
    }

    public Guid NotificationLoadMethodId;

    [TypeConverter(type: typeof(WorkQueueClassNotificationStructureFilterConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint), Category(category: "Notification")]
    [XmlReference(attributeName: "notificationLoadMethod", idField: "NotificationLoadMethodId")]
    public DataStructureMethod NotificationLoadMethod
    {
        get =>
            (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: NotificationLoadMethodId)
                );
        set
        {
            this.NotificationLoadMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    [Category(category: "Notification")]
    [XmlAttribute(attributeName: "notificationFilterPkParameter")]
    public string NotificationFilterPkParameter { get; set; } = "";

    [Category(category: "UI")]
    [XmlAttribute(attributeName: "defaultPanelConfiguration")]
    public string DefaultPanelConfiguration { get; set; } = "";
    #endregion
}

[AttributeUsage(validOn: AttributeTargets.Property)]
public class StructureMustHaveGetByIdFilterRule : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (!(instance is WorkQueueClass workQueueClass))
        {
            throw new Exception(
                message: $"{nameof(StructureMustHaveGetByIdFilterRule)} can be only applied to type {nameof(WorkQueueClass)}"
            );
        }
        if (workQueueClass.WorkQueueStructure == null)
        {
            return null;
        }
        DataStructureFilterSet getByIdFilterSet = workQueueClass
            .WorkQueueStructure.ChildItems.OfType<DataStructureFilterSet>()
            .FirstOrDefault(predicate: filterSet => filterSet.Name == "GetById");
        return getByIdFilterSet == null
            ? new Exception(
                message: $"The {nameof(workQueueClass.WorkQueueStructure)} of "
                    + $"{nameof(WorkQueueClass)} {workQueueClass.Name}, "
                    + $"Id: {workQueueClass.Id} does not have filter set named GetById which is required."
            )
            : null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance: instance);
    }
}
