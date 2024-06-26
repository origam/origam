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
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;
using System.Collections.Generic;
using System.Linq;

namespace Origam.Schema.WorkflowModel;
public static class WorkflowHelper
{
	public static ServiceMethodCallTask CreateServiceMethodCallTask(
		ContextStore contextStore, 
		string serviceName, 
		string methodName, 
		bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var serviceSchemaItemProvider 
			= schemaService.GetProvider<ServiceSchemaItemProvider>();
		if(!(serviceSchemaItemProvider.GetChildByName(
			    serviceName, Service.CategoryConst) is Service service))
		{
			throw new ArgumentOutOfRangeException(
				nameof(serviceName), serviceName, @"Service not found.");
		}
		if(!(service.GetChildByName(
			    methodName, ServiceMethod.CategoryConst) 
			    is ServiceMethod serviceMethod))
		{
			throw new ArgumentOutOfRangeException(
				nameof(methodName), methodName,
				$@"Method not found for service {serviceName}.");
		}
		var serviceMethodCallTask = contextStore.ParentItem
			.NewItem<ServiceMethodCallTask>(
				schemaService.ActiveSchemaExtensionId, null); 
		serviceMethodCallTask.Name = methodName + "_" + contextStore.Name;
		serviceMethodCallTask.OutputContextStore = contextStore;
		serviceMethodCallTask.Service = service;
		serviceMethodCallTask.ServiceMethod = serviceMethod;
		if(persist)
		{
			serviceMethodCallTask.Persist();
		}
		return serviceMethodCallTask;
	}
	public static ContextReference CreateContextReference(
		AbstractSchemaItem parentItem, 
		ContextStore contextStore, 
		string xpath, 
		bool persist)
	{
		var schemaService = ServiceManager.Services
			.GetService<ISchemaService>();
		var contextReference = parentItem.NewItem<ContextReference>(
				schemaService.ActiveSchemaExtensionId, null);
		contextReference.ContextStore = contextStore;
		contextReference.XPath = xpath;
		if(persist)
		{
			contextReference.Persist();
		}
		return contextReference;
	}
	public static WorkflowTaskDependency CreateTaskDependency(
		IWorkflowStep step, 
		IWorkflowStep dependencyStep, 
		bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var workflowTaskDependency = step.NewItem<WorkflowTaskDependency>(
				schemaService.ActiveSchemaExtensionId, null);
		workflowTaskDependency.Task = dependencyStep;
		if(persist)
		{
			workflowTaskDependency.Persist();
		}
		return workflowTaskDependency;
	}
	public static ServiceMethodCallTask CreateDataServiceLoadDataTask(
		ContextStore contextStore, bool persist)
	{
		if(!(contextStore.Structure is DataStructure dataStructure))
		{
			throw new ArgumentOutOfRangeException(@"Structure", 
				contextStore.Structure, 
				@"Context structure must be of type DataStructure.");
		}
		var serviceMethodCallTask = CreateServiceMethodCallTask(
			contextStore, "DataService", "LoadData", persist);
		var serviceMethodCallParameterDataStructure = serviceMethodCallTask
			.GetChildByName("DataStructure", 
				ServiceMethodCallParameter.CategoryConst) 
			as ServiceMethodCallParameter;
		EntityHelper.CreateDataStructureReference(
			serviceMethodCallParameterDataStructure, dataStructure, 
			null, null, persist);
		return serviceMethodCallTask;
	}
	public static ServiceMethodCallTask CreateDataServiceStoreDataTask(
		ContextStore contextStore, bool persist)
	{
		if(!(contextStore.Structure is DataStructure structure))
		{
			throw new ArgumentOutOfRangeException(
				@"Structure", contextStore.Structure, 
				@"Context structure must be of type DataStructure.");
		}
		var serviceMethodCallTask = CreateServiceMethodCallTask(
			contextStore, "DataService", "StoreData", persist);
		var serviceMethodCallParameterDataStructure = serviceMethodCallTask
			.GetChildByName("DataStructure", 
				ServiceMethodCallParameter.CategoryConst) 
			as ServiceMethodCallParameter;
		EntityHelper.CreateDataStructureReference(
			serviceMethodCallParameterDataStructure, 
			structure, null, null, persist);
		var serviceMethodCallParameterData = serviceMethodCallTask
			.GetChildByName("Data", 
				ServiceMethodCallParameter.CategoryConst) 
			as ServiceMethodCallParameter;
		var contextReference = CreateContextReference(
			serviceMethodCallParameterData, contextStore, "/", false);
		contextReference.Name = contextReference.ContextStore.Name;
		if (persist)
		{
			contextReference.Persist();
		}
		return serviceMethodCallTask;
	}
	public static ServiceMethodCallTask 
		CreateDataTransformationServiceTransformTask(
			ContextStore contextStore, bool persist)
	{
		var serviceMethodCallTask = CreateServiceMethodCallTask(
			contextStore, "DataTransformationService", "Transform", 
			persist);
		var serviceMethodCallParameterData = serviceMethodCallTask
			.GetChildByName("Data", 
				ServiceMethodCallParameter.CategoryConst) 
			as ServiceMethodCallParameter;
		var contextReference = CreateContextReference(
			serviceMethodCallParameterData, contextStore, "/", false);
		contextReference.Name = contextReference.ContextStore.Name;
		if(persist)
		{
			contextReference.Persist();
		}
		return serviceMethodCallTask;
	}
	public static WorkQueueClass CreateWorkQueueClass(
        IDataEntity entity, 
        ICollection fields, 
        IList<AbstractSchemaItem> generatedElements)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var workQueueProvider = schemaService
				.GetProvider<WorkQueue.WorkQueueClassSchemaItemProvider>();
		var workQueueClass = workQueueProvider.NewItem<WorkQueueClass>(
				schemaService.ActiveSchemaExtensionId, null);
		workQueueClass.Name = entity.Name;
		workQueueClass.Entity = entity;
		workQueueClass.EntityStructure = EntityHelper
			.CreateDataStructure(entity, entity.Name, true);
		workQueueClass.EntityStructurePrimaryKeyMethod = EntityHelper
			.CreateFilterSet(workQueueClass.EntityStructure, "GetId", true);
        var dataStructureEntity = workQueueClass.EntityStructure.Entities[0] 
            as DataStructureEntity;
		var primaryKeyFilter = entity.EntityFilters.Cast<EntityFilter>()
			.FirstOrDefault(filter => filter.Name == "GetId");
		EntityHelper.CreateFilterSetFilter(
			(DataStructureFilterSet)workQueueClass
				.EntityStructurePrimaryKeyMethod,
			dataStructureEntity, primaryKeyFilter, true);
		var persistenceService = ServiceManager.Services
			.GetService<IPersistenceService>();
		var countLookup = persistenceService.SchemaProvider.RetrieveInstance(
			typeof(AbstractSchemaItem),
			new ModelElementKey(
				new Guid("2a953c7d-0276-42c4-99b9-aa484808bbcb"))
			) as IDataLookup;
		workQueueClass.WorkQueueItemCountLookup = countLookup;
		// load wq_template data structure
		var workQueueTemplateStructure = persistenceService.SchemaProvider
			.RetrieveInstance(
				typeof(AbstractSchemaItem), 
				new ModelElementKey(
					new Guid("e9d8e455-02de-4ae7-914e-9a3064e52bd6"))
			) as DataStructure;
		// get a clone and save
		var workQueueStructure
			= workQueueTemplateStructure.Clone() as DataStructure;
		workQueueStructure.Name = "WQ_" + entity.Name;
		workQueueStructure.Group = EntityHelper.GetDataStructureGroup(
			entity.Group.Name);;
		workQueueStructure.SetExtensionRecursive(
			schemaService.ActiveExtension);
		workQueueStructure.ClearCacheOnPersist = false;
		workQueueStructure.ThrowEventOnPersist = false;
		workQueueStructure.Persist();
		workQueueStructure.UpdateReferences();
		workQueueStructure.ClearCacheOnPersist = true;
		DataStructureSortSet sortSet = EntityHelper.CreateSortSet(
			workQueueStructure, "Default", true);
		EntityHelper.CreateSortSetItem(sortSet,
			workQueueStructure.Entities[0] as DataStructureEntity,
			"RecordCreated",
			DataStructureColumnSortDirection.Ascending, true);
		workQueueStructure.ThrowEventOnPersist = true;
		workQueueStructure.Persist();
		var workQueueEntity 
			= ((DataStructureEntity)workQueueStructure.Entities[0])
			.EntityDefinition;
		var workQueueFieldTypes = new Hashtable();
		// add fields to the structure
		foreach(IDataEntityColumn column in fields)
		{
			if(!workQueueFieldTypes.Contains(column.DataType))
			{
				workQueueFieldTypes.Add(column.DataType, 0);
			}
			var fieldCount = (int)workQueueFieldTypes[column.DataType];
			fieldCount++;
			workQueueFieldTypes[column.DataType] = fieldCount;
			var fieldName 
				= DecodeWorkQueueFieldName(column.DataType) + fieldCount;
			// find wq field (eg. "g1" for the first guid field)
			var workQueueField = workQueueEntity.GetChildByName(
				fieldName, AbstractDataEntityColumn.CategoryConst) 
				as IDataEntityColumn;
			var workQueueDataStructureField = EntityHelper
				.CreateDataStructureField(
					(DataStructureEntity)workQueueStructure.Entities[0], 
					workQueueField, false);
			// finally rename the field e.g. "g1" to "refSomethingId"
			workQueueDataStructureField.Name = column.Name;
			workQueueDataStructureField.Caption = column.Caption;
			workQueueDataStructureField.Persist();
		}
		workQueueClass.WorkQueueStructure = workQueueStructure; 
		workQueueClass.WorkQueueStructureUserListMethod = workQueueStructure
			.GetChildByName("GetByQueueId", 
				DataStructureMethod.CategoryConst) as DataStructureMethod;
		workQueueClass.WorkQueueStructureSortSet = sortSet;
		workQueueClass.Persist();
		GenerateWorkQueueClassEntityMappings(workQueueClass);
		if(generatedElements == null)
		{
			return workQueueClass;
		}
		generatedElements.Add(workQueueClass);
        generatedElements.Add(workQueueClass.EntityStructure);
        generatedElements.Add(workQueueStructure);
        return workQueueClass;
	}
	private static string DecodeWorkQueueFieldName(OrigamDataType dataType)
	{
		switch(dataType)
		{
			case OrigamDataType.Blob:
				return "blob";
			case OrigamDataType.Boolean:
				return "b";
			case OrigamDataType.Currency:
				return "c";
			case OrigamDataType.Date:
				return "d";
			case OrigamDataType.Float:
				return "f";
			case OrigamDataType.Integer:
				return "i";
			case OrigamDataType.Long:
				return "i";
			case OrigamDataType.Memo:
				return "m";
			case OrigamDataType.String:
				return "s";
			case OrigamDataType.UniqueIdentifier:
				return "g";
		}
		throw new ArgumentOutOfRangeException(nameof(dataType), dataType, 
			ResourceUtils.GetString("ErrorWorkQueueWizardUnknownType"));
	}
	public static void GenerateWorkQueueClassEntityMappings(
		WorkQueueClass workQueueClass)
	{
		var schemaService = ServiceManager.Services
			.GetService<ISchemaService>();
		foreach(WorkQueueClassEntityMapping workQueueClassEntityMapping 
		        in workQueueClass.EntityMappings)
		{
			workQueueClassEntityMapping.IsDeleted = true;
			workQueueClassEntityMapping.Persist();
		}
		var dataStructureEntity = (DataStructureEntity)workQueueClass
			.WorkQueueStructure.Entities[0];
		foreach(var column in dataStructureEntity.Columns)
		{
			var newMapping = workQueueClass
				.NewItem<WorkQueueClassEntityMapping>(
					schemaService.ActiveSchemaExtensionId, null);
			if(column.Name == "refId")
			{
				newMapping.Name = column.Name;
				newMapping.XPath = "/row/@Id";
				newMapping.Persist();
			}
			else
			{
				foreach(IDataEntityColumn entityColumn
				        in workQueueClass.Entity.EntityColumns)
				{
					if((entityColumn.Name == column.Name)
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
					   && (column.Name != "NextAttemptTime"))
					{
						newMapping.Name = column.Name;
						newMapping.XPath 
							= "/row/" 
							  + (entityColumn.XmlMappingType 
							     == EntityColumnXmlMapping.Attribute 
								  ? "@" : "") 
							  + entityColumn.Name;
						newMapping.Persist();
						break;
					}
				}
			}
		}
	}
}
