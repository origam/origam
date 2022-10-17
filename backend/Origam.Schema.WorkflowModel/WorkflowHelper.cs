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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowHelper.
	/// </summary>
	public class WorkflowHelper
	{
		public static ServiceMethodCallTask CreateServiceMethodCallTask(ContextStore contextStore, string serviceName, string methodName, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			ServiceSchemaItemProvider ssip = schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;
			Service svc = ssip.GetChildByName(serviceName, Service.CategoryConst) as Service;

			if(svc == null)
			{
				throw new ArgumentOutOfRangeException("serviceName", serviceName, "Service not found.");
			}

			ServiceMethod method = svc.GetChildByName(methodName, ServiceMethod.CategoryConst) as ServiceMethod;

			if(svc == null)
			{
				throw new ArgumentOutOfRangeException("methodName", methodName, "Method not found for service " + serviceName + ".");
			}

			ServiceMethodCallTask t = contextStore.ParentItem.NewItem(typeof(ServiceMethodCallTask), schema.ActiveSchemaExtensionId, null) as ServiceMethodCallTask;
			t.Name = methodName + "_" + contextStore.Name;
			t.OutputContextStore = contextStore;
			t.Service = svc;
			t.ServiceMethod = method;

			if(persist) t.Persist();

			return t;
		}

		public static ContextReference CreateContextReference(AbstractSchemaItem parentItem, ContextStore context, string xpath, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			ContextReference cr = parentItem.NewItem(typeof(ContextReference), schema.ActiveSchemaExtensionId, null) as ContextReference;
			cr.ContextStore = context;
			cr.XPath = xpath;

			if(persist) cr.Persist();

			return cr;
		}

		public static WorkflowTaskDependency CreateTaskDependency(IWorkflowStep step, IWorkflowStep dependencyStep, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			WorkflowTaskDependency dep = step.NewItem(typeof(WorkflowTaskDependency), schema.ActiveSchemaExtensionId, null) as WorkflowTaskDependency;
			dep.Task = dependencyStep;

			if(persist) dep.Persist();

			return dep;
		}

		public static ServiceMethodCallTask CreateDataServiceLoadDataTask(ContextStore context, bool persist)
		{
			if(! (context.Structure is DataStructure))
			{
				throw new ArgumentOutOfRangeException("Structure", context.Structure, "Context structure must be of type DataStructure.");
			}

			ServiceMethodCallTask task = WorkflowHelper.CreateServiceMethodCallTask(context, "DataService", "LoadData", persist);

			ServiceMethodCallParameter p1 = task.GetChildByName("DataStructure", ServiceMethodCallParameter.CategoryConst) as ServiceMethodCallParameter;
			EntityHelper.CreateDataStructureReference(p1, context.Structure as DataStructure, null, null, persist);

			return task;
		}

		public static ServiceMethodCallTask CreateDataServiceStoreDataTask(ContextStore context, bool persist)
		{
			if(! (context.Structure is DataStructure))
			{
				throw new ArgumentOutOfRangeException("Structure", context.Structure, "Context structure must be of type DataStructure.");
			}

			ServiceMethodCallTask task = WorkflowHelper.CreateServiceMethodCallTask(context, "DataService", "StoreData", persist);

			ServiceMethodCallParameter p1 = task.GetChildByName("DataStructure", ServiceMethodCallParameter.CategoryConst) as ServiceMethodCallParameter;
			EntityHelper.CreateDataStructureReference(p1, context.Structure as DataStructure, null, null, persist);

			ServiceMethodCallParameter p2 = task.GetChildByName("Data", ServiceMethodCallParameter.CategoryConst) as ServiceMethodCallParameter;
			ContextReference cr = WorkflowHelper.CreateContextReference(p2, context, "/", false);
			cr.Name = cr.ContextStore.Name;
			if(persist) cr.Persist();

			return task;
		}

		public static ServiceMethodCallTask CreateDataTransformationServiceTransformTask(ContextStore context, bool persist)
		{
			ServiceMethodCallTask task = WorkflowHelper.CreateServiceMethodCallTask(context, "DataTransformationService", "Transform", persist);

			ServiceMethodCallParameter p1 = task.GetChildByName("Data", ServiceMethodCallParameter.CategoryConst) as ServiceMethodCallParameter;
			ContextReference cr = WorkflowHelper.CreateContextReference(p1, context, "/", false);
			cr.Name = cr.ContextStore.Name;
			if(persist) cr.Persist();

			return task;
		}

		public static WorkQueueClass CreateWorkQueueClass(
            IDataEntity entity, ICollection fields, IList<AbstractSchemaItem> generatedElements)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			WorkQueue.WorkQueueClassSchemaItemProvider wqProvider =
                schema.GetProvider(typeof(WorkQueue.WorkQueueClassSchemaItemProvider)) 
                as WorkQueue.WorkQueueClassSchemaItemProvider;
			WorkQueueClass  wqc = wqProvider.NewItem(typeof(WorkQueueClass), schema.ActiveSchemaExtensionId, null) as WorkQueueClass;
			wqc.Name = entity.Name;
			wqc.Entity = entity;
			wqc.EntityStructure = EntityHelper.CreateDataStructure(entity, entity.Name, true);
			wqc.EntityStructurePrimaryKeyMethod = EntityHelper.CreateFilterSet(wqc.EntityStructure, "GetId", true);
            DataStructureEntity dsEntity = wqc.EntityStructure.Entities[0] as DataStructureEntity;
			EntityFilter primaryKeyFilter = null;
			foreach(EntityFilter filter in entity.EntityFilters)
			{
				if(filter.Name == "GetId")
				{
					primaryKeyFilter = filter;
					break;
				}
			}

			EntityHelper.CreateFilterSetFilter(
				wqc.EntityStructurePrimaryKeyMethod as DataStructureFilterSet,
				dsEntity, primaryKeyFilter, true);

			IPersistenceService persistence = 
				ServiceManager.Services.GetService(typeof(IPersistenceService)) 
				as IPersistenceService;

			IDataLookup countLookup = persistence.SchemaProvider.RetrieveInstance(
				typeof(AbstractSchemaItem),
				new ModelElementKey(new Guid("2a953c7d-0276-42c4-99b9-aa484808bbcb"))
				) as IDataLookup;
			wqc.WorkQueueItemCountLookup = countLookup;

			// load wq_template data structure
			DataStructure wqTemplateStructure = persistence.SchemaProvider.RetrieveInstance(
				typeof(AbstractSchemaItem),
				new ModelElementKey(new Guid("e9d8e455-02de-4ae7-914e-9a3064e52bd6"))
				) as DataStructure;

			// get a clone and save
			DataStructure wqStructure = wqTemplateStructure.Clone() as DataStructure;
			wqStructure.Name = "WQ_" + entity.Name;
			wqStructure.Group = EntityHelper.GetDataStructureGroup(entity.Group.Name);;
			wqStructure.SetExtensionRecursive(schema.ActiveExtension);
			wqStructure.ClearCacheOnPersist = false;
			wqStructure.ThrowEventOnPersist = false;
			wqStructure.Persist();
			wqStructure.UpdateReferences();
			wqStructure.ClearCacheOnPersist = true;
			DataStructureSortSet sortSet = EntityHelper.CreateSortSet(
				wqStructure, "Default", true);
			EntityHelper.CreateSortSetItem(sortSet,
				wqStructure.Entities[0] as DataStructureEntity, "RecordCreated",
				DataStructureColumnSortDirection.Ascending, true);
			wqStructure.ThrowEventOnPersist = true;
			wqStructure.Persist();

			IDataEntity wqEntity = (wqStructure.Entities[0] as DataStructureEntity).EntityDefinition;
			Hashtable wqFieldTypes = new Hashtable();
			// add fields to the structure
			foreach(IDataEntityColumn column in fields)
			{
				if(! wqFieldTypes.Contains(column.DataType))
				{
					wqFieldTypes.Add(column.DataType, 0);
				}
				int fieldCount = (int)wqFieldTypes[column.DataType];
				fieldCount++;
				wqFieldTypes[column.DataType] = fieldCount;
				string fieldName = DecodeWorkQueueFieldName(column.DataType) + fieldCount.ToString();
				// find wq field (eg. "g1" for the first guid field)
				IDataEntityColumn wqField = wqEntity.GetChildByName(fieldName, AbstractDataEntityColumn.CategoryConst) as IDataEntityColumn;
				DataStructureColumn wqDsField = EntityHelper.CreateDataStructureField(wqStructure.Entities[0] as DataStructureEntity, wqField, false);
				// finally rename the field e.g. "g1" to "refSomethingId"
				wqDsField.Name = column.Name;
				wqDsField.Caption = column.Caption;
				wqDsField.Persist();
			}

			wqc.WorkQueueStructure = wqStructure; 
			wqc.WorkQueueStructureUserListMethod = wqStructure.GetChildByName(
				"GetByQueueId", DataStructureMethod.CategoryConst) 
				as DataStructureMethod;
			wqc.WorkQueueStructureSortSet = sortSet;
			wqc.Persist();

			GenerateWorkQueueClassEntityMappings(wqc);

            if (generatedElements != null)
            {
                generatedElements.Add(wqc);
                generatedElements.Add(wqc.EntityStructure);
                generatedElements.Add(wqStructure);
            }
            return wqc;
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

			throw new ArgumentOutOfRangeException("dataType", dataType, ResourceUtils.GetString("ErrorWorkQueueWizardUnknownType"));
		}

		public static void GenerateWorkQueueClassEntityMappings(WorkQueueClass wqc)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			foreach(WorkQueueClassEntityMapping m in wqc.EntityMappings)
			{
				m.IsDeleted = true;
				m.Persist();
			}

			DataStructureEntity entity = (DataStructureEntity)wqc.WorkQueueStructure.Entities[0];

			foreach(DataStructureColumn column in entity.Columns)
			{
				WorkQueueClassEntityMapping newMapping = wqc.NewItem(typeof(WorkQueueClassEntityMapping), schema.ActiveSchemaExtensionId, null) as WorkQueueClassEntityMapping;

				if(column.Name == "refId")
				{
					newMapping.Name = column.Name;
					newMapping.XPath = "/row/@Id";
					newMapping.Persist();
				}
				else
				{
					foreach(IDataEntityColumn entityColumn in wqc.Entity.EntityColumns)
					{
						if(entityColumn.Name == column.Name & column.Name != "IsLocked" & column.Name != "RecordCreated" & column.Name != "RecordUpdated" & column.Name != "RecordCreatedBy" & column.Name != "RecordUpdatedBy" & column.Name != "Id" & column.Name != "refLockedByBusinessPartnerId" & column.Name != "refWorkQueueId")
						{
							newMapping.Name = column.Name;

							newMapping.XPath = "/row/" + (entityColumn.XmlMappingType == EntityColumnXmlMapping.Attribute ? "@" : "") + entityColumn.Name;
							newMapping.Persist();

							break;
						}
					}
				}

			}
		}
	}
}
