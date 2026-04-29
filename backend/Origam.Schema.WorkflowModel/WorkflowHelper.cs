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
using System.Linq;
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;

public static class WorkflowHelper
{
    public static ServiceMethodCallTask CreateServiceMethodCallTask(
        ContextStore contextStore,
        string serviceName,
        string methodName,
        bool persist
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var serviceSchemaItemProvider = schemaService.GetProvider<ServiceSchemaItemProvider>();
        if (
            !(
                serviceSchemaItemProvider.GetChildByName(
                    name: serviceName,
                    itemType: Service.CategoryConst
                )
                is Service service
            )
        )
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(serviceName),
                actualValue: serviceName,
                message: @"Service not found."
            );
        }
        if (
            !(
                service.GetChildByName(name: methodName, itemType: ServiceMethod.CategoryConst)
                is ServiceMethod serviceMethod
            )
        )
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(methodName),
                actualValue: methodName,
                message: $@"Method not found for service {serviceName}."
            );
        }
        var serviceMethodCallTask = contextStore.ParentItem.NewItem<ServiceMethodCallTask>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        serviceMethodCallTask.Name = methodName + "_" + contextStore.Name;
        serviceMethodCallTask.OutputContextStore = contextStore;
        serviceMethodCallTask.Service = service;
        serviceMethodCallTask.ServiceMethod = serviceMethod;
        if (persist)
        {
            serviceMethodCallTask.Persist();
        }
        return serviceMethodCallTask;
    }

    public static ContextReference CreateContextReference(
        ISchemaItem parentItem,
        ContextStore contextStore,
        string xpath,
        bool persist
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var contextReference = parentItem.NewItem<ContextReference>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        contextReference.ContextStore = contextStore;
        contextReference.XPath = xpath;
        if (persist)
        {
            contextReference.Persist();
        }
        return contextReference;
    }

    public static WorkflowTaskDependency CreateTaskDependency(
        IWorkflowStep step,
        IWorkflowStep dependencyStep,
        bool persist
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var workflowTaskDependency = step.NewItem<WorkflowTaskDependency>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        workflowTaskDependency.Task = dependencyStep;
        if (persist)
        {
            workflowTaskDependency.Persist();
        }
        return workflowTaskDependency;
    }

    public static ServiceMethodCallTask CreateDataServiceLoadDataTask(
        ContextStore contextStore,
        bool persist
    )
    {
        if (!(contextStore.Structure is DataStructure dataStructure))
        {
            throw new ArgumentOutOfRangeException(
                paramName: @"Structure",
                actualValue: contextStore.Structure,
                message: @"Context structure must be of type DataStructure."
            );
        }
        var serviceMethodCallTask = CreateServiceMethodCallTask(
            contextStore: contextStore,
            serviceName: "DataService",
            methodName: "LoadData",
            persist: persist
        );
        var serviceMethodCallParameterDataStructure =
            serviceMethodCallTask.GetChildByName(
                name: "DataStructure",
                itemType: ServiceMethodCallParameter.CategoryConst
            ) as ServiceMethodCallParameter;
        EntityHelper.CreateDataStructureReference(
            parentItem: serviceMethodCallParameterDataStructure,
            structure: dataStructure,
            method: null,
            sortSet: null,
            persist: persist
        );
        return serviceMethodCallTask;
    }

    public static ServiceMethodCallTask CreateDataServiceStoreDataTask(
        ContextStore contextStore,
        bool persist
    )
    {
        if (!(contextStore.Structure is DataStructure structure))
        {
            throw new ArgumentOutOfRangeException(
                paramName: @"Structure",
                actualValue: contextStore.Structure,
                message: @"Context structure must be of type DataStructure."
            );
        }
        var serviceMethodCallTask = CreateServiceMethodCallTask(
            contextStore: contextStore,
            serviceName: "DataService",
            methodName: "StoreData",
            persist: persist
        );
        var serviceMethodCallParameterDataStructure =
            serviceMethodCallTask.GetChildByName(
                name: "DataStructure",
                itemType: ServiceMethodCallParameter.CategoryConst
            ) as ServiceMethodCallParameter;
        EntityHelper.CreateDataStructureReference(
            parentItem: serviceMethodCallParameterDataStructure,
            structure: structure,
            method: null,
            sortSet: null,
            persist: persist
        );
        var serviceMethodCallParameterData =
            serviceMethodCallTask.GetChildByName(
                name: "Data",
                itemType: ServiceMethodCallParameter.CategoryConst
            ) as ServiceMethodCallParameter;
        var contextReference = CreateContextReference(
            parentItem: serviceMethodCallParameterData,
            contextStore: contextStore,
            xpath: "/",
            persist: false
        );
        contextReference.Name = contextReference.ContextStore.Name;
        if (persist)
        {
            contextReference.Persist();
        }
        return serviceMethodCallTask;
    }

    public static ServiceMethodCallTask CreateDataTransformationServiceTransformTask(
        ContextStore contextStore,
        bool persist
    )
    {
        var serviceMethodCallTask = CreateServiceMethodCallTask(
            contextStore: contextStore,
            serviceName: "DataTransformationService",
            methodName: "Transform",
            persist: persist
        );
        var serviceMethodCallParameterData =
            serviceMethodCallTask.GetChildByName(
                name: "Data",
                itemType: ServiceMethodCallParameter.CategoryConst
            ) as ServiceMethodCallParameter;
        var contextReference = CreateContextReference(
            parentItem: serviceMethodCallParameterData,
            contextStore: contextStore,
            xpath: "/",
            persist: false
        );
        contextReference.Name = contextReference.ContextStore.Name;
        if (persist)
        {
            contextReference.Persist();
        }
        return serviceMethodCallTask;
    }

    public static WorkQueueClass CreateWorkQueueClass(
        IDataEntity entity,
        ICollection fields,
        IList<ISchemaItem> generatedElements
    )
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var workQueueProvider =
            schemaService.GetProvider<WorkQueue.WorkQueueClassSchemaItemProvider>();
        var workQueueClass = workQueueProvider.NewItem<WorkQueueClass>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        workQueueClass.Name = entity.Name;
        workQueueClass.Entity = entity;
        workQueueClass.EntityStructure = EntityHelper.CreateDataStructure(
            entity: entity,
            name: entity.Name,
            persist: true
        );
        workQueueClass.EntityStructurePrimaryKeyMethod = EntityHelper.CreateFilterSet(
            dataStructure: workQueueClass.EntityStructure,
            name: "GetId",
            persist: true
        );
        var dataStructureEntity =
            workQueueClass.EntityStructure.Entities[index: 0] as DataStructureEntity;
        var primaryKeyFilter = entity.EntityFilters.FirstOrDefault(predicate: filter =>
            filter.Name == "GetId"
        );
        EntityHelper.CreateFilterSetFilter(
            dataStructureFilterSet: (DataStructureFilterSet)
                workQueueClass.EntityStructurePrimaryKeyMethod,
            dataStructureEntity: dataStructureEntity,
            filter: primaryKeyFilter,
            persist: true
        );
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var countLookup =
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(
                    id: new Guid(g: "2a953c7d-0276-42c4-99b9-aa484808bbcb")
                )
            ) as IDataLookup;
        workQueueClass.WorkQueueItemCountLookup = countLookup;
        // load wq_template data structure
        var workQueueTemplateStructure =
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(
                    id: new Guid(g: "e9d8e455-02de-4ae7-914e-9a3064e52bd6")
                )
            ) as DataStructure;
        // get a clone and save
        var workQueueStructure = workQueueTemplateStructure.Clone() as DataStructure;
        workQueueStructure.Name = "WQ_" + entity.Name;
        workQueueStructure.Group = EntityHelper.GetDataStructureGroup(name: entity.Group.Name);
        ;
        workQueueStructure.SetExtensionRecursive(extension: schemaService.ActiveExtension);
        workQueueStructure.ClearCacheOnPersist = false;
        workQueueStructure.ThrowEventOnPersist = false;
        workQueueStructure.Persist();
        workQueueStructure.UpdateReferences();
        workQueueStructure.ClearCacheOnPersist = true;
        DataStructureSortSet sortSet = EntityHelper.CreateSortSet(
            dataStructure: workQueueStructure,
            name: "Default",
            persist: true
        );
        EntityHelper.CreateSortSetItem(
            sortSet: sortSet,
            entity: workQueueStructure.Entities[index: 0] as DataStructureEntity,
            fieldName: "RecordCreated",
            direction: DataStructureColumnSortDirection.Ascending,
            persist: true
        );
        workQueueStructure.ThrowEventOnPersist = true;
        workQueueStructure.Persist();
        var workQueueEntity = (
            (DataStructureEntity)workQueueStructure.Entities[index: 0]
        ).EntityDefinition;
        var workQueueFieldTypes = new Hashtable();
        // add fields to the structure
        foreach (IDataEntityColumn column in fields)
        {
            if (!workQueueFieldTypes.Contains(key: column.DataType))
            {
                workQueueFieldTypes.Add(key: column.DataType, value: 0);
            }
            var fieldCount = (int)workQueueFieldTypes[key: column.DataType];
            fieldCount++;
            workQueueFieldTypes[key: column.DataType] = fieldCount;
            var fieldName = DecodeWorkQueueFieldName(dataType: column.DataType) + fieldCount;
            // find wq field (eg. "g1" for the first guid field)
            var workQueueField =
                workQueueEntity.GetChildByName(
                    name: fieldName,
                    itemType: AbstractDataEntityColumn.CategoryConst
                ) as IDataEntityColumn;
            var workQueueDataStructureField = EntityHelper.CreateDataStructureField(
                dataStructureEntity: (DataStructureEntity)workQueueStructure.Entities[index: 0],
                field: workQueueField,
                persist: false
            );
            // finally rename the field e.g. "g1" to "refSomethingId"
            workQueueDataStructureField.Name = column.Name;
            workQueueDataStructureField.Caption = column.Caption;
            workQueueDataStructureField.Persist();
        }
        workQueueClass.WorkQueueStructure = workQueueStructure;
        workQueueClass.WorkQueueStructureUserListMethod =
            workQueueStructure.GetChildByName(
                name: "GetByQueueId",
                itemType: DataStructureMethod.CategoryConst
            ) as DataStructureMethod;
        workQueueClass.WorkQueueStructureSortSet = sortSet;
        workQueueClass.Persist();
        GenerateWorkQueueClassEntityMappings(workQueueClass: workQueueClass);
        if (generatedElements == null)
        {
            return workQueueClass;
        }
        generatedElements.Add(item: workQueueClass);
        generatedElements.Add(item: workQueueClass.EntityStructure);
        generatedElements.Add(item: workQueueStructure);
        return workQueueClass;
    }

    private static string DecodeWorkQueueFieldName(OrigamDataType dataType)
    {
        switch (dataType)
        {
            case OrigamDataType.Blob:
            {
                return "blob";
            }
            case OrigamDataType.Boolean:
            {
                return "b";
            }
            case OrigamDataType.Currency:
            {
                return "c";
            }
            case OrigamDataType.Date:
            {
                return "d";
            }
            case OrigamDataType.Float:
            {
                return "f";
            }
            case OrigamDataType.Integer:
            {
                return "i";
            }
            case OrigamDataType.Long:
            {
                return "i";
            }
            case OrigamDataType.Memo:
            {
                return "m";
            }
            case OrigamDataType.String:
            {
                return "s";
            }
            case OrigamDataType.UniqueIdentifier:
            {
                return "g";
            }
        }
        throw new ArgumentOutOfRangeException(
            paramName: nameof(dataType),
            actualValue: dataType,
            message: ResourceUtils.GetString(key: "ErrorWorkQueueWizardUnknownType")
        );
    }

    public static void GenerateWorkQueueClassEntityMappings(WorkQueueClass workQueueClass)
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        foreach (
            WorkQueueClassEntityMapping workQueueClassEntityMapping in workQueueClass.EntityMappings
        )
        {
            workQueueClassEntityMapping.IsDeleted = true;
            workQueueClassEntityMapping.Persist();
        }
        var dataStructureEntity = (DataStructureEntity)
            workQueueClass.WorkQueueStructure.Entities[index: 0];
        foreach (var column in dataStructureEntity.Columns)
        {
            var newMapping = workQueueClass.NewItem<WorkQueueClassEntityMapping>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
            if (column.Name == "refId")
            {
                newMapping.Name = column.Name;
                newMapping.XPath = "/row/@Id";
                newMapping.Persist();
            }
            else
            {
                foreach (IDataEntityColumn entityColumn in workQueueClass.Entity.EntityColumns)
                {
                    if (
                        (entityColumn.Name == column.Name)
                        && (column.Name != "IsLocked")
                        && (column.Name != "RecordCreated")
                        && (column.Name != "RecordUpdated")
                        && (column.Name != "RecordCreatedBy")
                        && (column.Name != "RecordUpdatedBy")
                        && (column.Name != "Id")
                        && (column.Name != "refLockedByBusinessPartnerId")
                        && (column.Name != "refWorkQueueId")
                        && (column.Name != "AttemptCount")
                        && (column.Name != "InRetry")
                        && (column.Name != "LastAttemptTime")
                        && (column.Name != "NextAttemptTime")
                    )
                    {
                        newMapping.Name = column.Name;
                        newMapping.XPath =
                            "/row/"
                            + (
                                entityColumn.XmlMappingType == EntityColumnXmlMapping.Attribute
                                    ? "@"
                                    : ""
                            )
                            + entityColumn.Name;
                        newMapping.Persist();
                        break;
                    }
                }
            }
        }
    }
}
