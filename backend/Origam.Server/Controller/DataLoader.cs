// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using CSharpFunctionalExtensions;
// using Microsoft.AspNetCore.Mvc;
// using Origam.DA;
// using Origam.DA.Service;
// using Origam.Extensions;
// using Origam.Schema.EntityModel;
// using Origam.Schema.MenuModel;
// using Origam.Schema.WorkflowModel;
// using Origam.Server.Model;
// using Origam.Server.Model.UIService;
// using Origam.Server.Session_Stores;
// using Origam.Workbench.Services;
// using Origam.Workbench.Services.CoreServices;
//
// namespace Origam.Server.Controller;
//
// public class DataLoader
// {
//     private readonly SessionObjects sessionObjects;
//     private readonly IDataService dataService;
//     private readonly string workQueueEntity = "WorkQueueEntry";
//
//     public DataLoader(SessionObjects sessionObjects, IDataService dataService)
//     {
//         this.sessionObjects = sessionObjects;
//         this.dataService = dataService;
//     }
//
//     public Result<DataStructureQuery, Error> GetRowsGetGroupQuery(
//         GetGroupsInput input, EntityData entityData)
//     {
//         var customOrdering = GetOrderings(input.OrderingList);
//
//         DataStructureColumn column = entityData.Entity.Column(input.GroupBy);
//         if (column == null)
//         {
//             return Result.Failure<DataStructureQuery, Error>(Error.BadRequest($"Cannot group by \"{input.GroupBy}\" because the column does not exist."));
//         }
//
//         var field = column.Field;
//         var columnData = new ColumnData(
//             name: input.GroupBy,
//             isVirtual: (field is DetachedField),
//             defaultValue: (field as DetachedField)
//             ?.DefaultValue?.Value,
//             hasRelation: (field as DetachedField)
//             ?.ArrayRelation != null);
//
//         List<ColumnData> columns = new List<ColumnData>
//             {columnData, ColumnData.GroupByCountColumn};
//
//         if(input.GroupByLookupId != Guid.Empty)
//         {
//             columns.Add(ColumnData.GroupByCaptionColumn);
//         }
//
//         var query = new DataStructureQuery
//         {
//             Entity = entityData.Entity.Name,
//             CustomFilters = new CustomFilters
//             {
//                 Filters = input.Filter,
//                 FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>()
//             },
//             CustomOrderings = customOrdering,
//             RowLimit = input.RowLimit,
//             ColumnsInfo = new ColumnsInfo(
//                 columns: columns, 
//                 renderSqlForDetachedFields: true),
//             ForceDatabaseCalculation = true,
//             CustomGrouping= new Grouping(
//                 input.GroupBy, input.GroupByLookupId, input.GroupingUnit),
//             AggregatedColumns = input.AggregatedColumns 
//         };
//         return AddMethodAndSource(
//             input.SessionFormIdentifier, input.MasterRowId, entityData, query);
//     }
//     
//     protected Result<T,Error> FindItem<T>(Guid id) where T : class
//     {
//         return !(ServiceManager.Services
//             .GetService<IPersistenceService>()
//             .SchemaProvider
//             .RetrieveInstance(typeof(T), new Key(id), true, false) is T instance)
//             ? Result.Failure<T, Error>(
//                 Error.NotFound("Object with requested id not found."))
//             : Result.Success<T, Error>(instance);
//     }
//     
//    protected Result<DataStructureQuery, Error> AddMethodAndSource(
//         Guid sessionFormIdentifier, Guid masterRowId, EntityData entityData,
//         DataStructureQuery query)
//     {
//         bool isLazyLoaded = entityData.MenuItem.ListDataStructure != null;
//         if(isLazyLoaded)
//         {
//             if(entityData.MenuItem.ListEntity.Name == entityData.Entity.Name)
//             {
//                 query.MethodId = entityData.MenuItem.ListMethodId;
//                 query.SortSetId = entityData.MenuItem.ListSortSetId;
//                 query.DataSourceId
//                     = entityData.MenuItem.ListDataStructure.Id;
//                 // get parameters from session store
//                 var parameters = sessionObjects.UIService.GetParameters(
//                     sessionFormIdentifier);
//                 foreach(var key in parameters.Keys)
//                 {
//                     query.Parameters.Add(
//                         new QueryParameter(key.ToString(),
//                             parameters[key]));
//                 }
//             }
//             else
//             {
//                 return FindItem<DataStructureMethod>(entityData.MenuItem.MethodId)
//                     .Map(CustomParameterService.GetFirstNonCustomParameter)
//                     .Bind(parameterName =>
//                     {
//                         query.DataSourceId
//                             = entityData.Entity.RootEntity.ParentItemId;
//                         query.Parameters.Add(new QueryParameter(
//                             parameterName, masterRowId));
//                         if(masterRowId == Guid.Empty)
//                         {
//                             return Result
//                                 .Failure<DataStructureQuery, Error>(
//                                     Error.BadRequest("MasterRowId cannot be empty"));
//                         }
//
//                         query.MethodId = entityData.MenuItem.MethodId;
//                         query.SortSetId = entityData.MenuItem.SortSetId;
//                         return Result.Success<DataStructureQuery, Error>(query);
//                     });
//             }
//         }
//         else
//         {
//             query.MethodId = entityData.MenuItem.MethodId;
//             query.SortSetId = entityData.MenuItem.SortSetId;
//             query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
//         }
//
//         return Result.Ok<DataStructureQuery, Error>(query);
//     }
//
//   
//     public CustomOrderings GetOrderings(List<IRowOrdering> orderingList)
//     {
//         var orderings = orderingList
//             .Select((inputOrdering, i) => 
//                 new Ordering(
//                     columnName: inputOrdering.ColumnId, 
//                     direction: inputOrdering.Direction, 
//                     lookupId: inputOrdering.LookupId, 
//                     sortOrder: i + 1000)
//             ).ToList();
//         return new CustomOrderings(orderings);
//     }
//     protected Result<AbstractMenuItem, Error> Authorize(
//         AbstractMenuItem menuItem)
//     {
//         return SecurityManager.GetAuthorizationProvider().Authorize(
//             User, menuItem.Roles)
//             ? Result.Success<AbstractMenuItem, Error>(menuItem)
//             : Result.Failure<AbstractMenuItem, Error>(Error.Forbid());
//     }
//
//     protected Result<EntityData, Error> GetEntityData(
//         Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
//     {
//         return FindEntity(dataStructureEntityId)
//             .Map(entity => 
//                 new EntityData {MenuItem = menuItem, Entity = entity});
//     }
//     protected Result<EntityData, Error> CheckEntityBelongsToMenu(
//         EntityData entityData)
//     {
//         return (entityData.MenuItem.Screen.DataStructure.Id 
//             == entityData.Entity.RootEntity.ParentItemId)
//             ? Result.Success<EntityData, Error>(entityData)
//             : Result.Failure<EntityData, Error>(
//                 Error.BadRequest("The requested Entity does not belong to the menu."));
//     }
//     protected Result<RowData, Error> GetRow(
//         IDataService dataService,
//         DataStructureEntity entity, Guid dataStructureEntityId,
//         Guid methodId, Guid rowId)
//     {
//         var rows = SessionStore.LoadRows(dataService, entity, 
//             dataStructureEntityId, methodId, new List<Guid> { rowId });
//         if(rows.Count == 0)
//         {
//             return Result.Failure<RowData, Error>(
//                 Error.NotFound("Requested data row was not found."));
//         }
//         return Result.Success<RowData, Error>(
//             new RowData{Row = rows[0], Entity = entity});
//     }
//     protected Error UnwrapReturnValue(
//         Result<Error, Error> result)
//     {
//         return result.IsSuccess ? result.Value : result.Error;
//     }
//     protected Result<DataStructureEntity, Error> FindEntity(Guid id)
//     {
//         return FindItem<DataStructureEntity>(id)
//             .OnFailureCompensate(error =>
//                 Result.Failure<DataStructureEntity, Error>(
//                     Error.NotFound("Requested DataStructureEntity not found. " 
//                                    + error.Message)));
//     }
//     // protected ChangeInfo SubmitChange(
//     //     RowData rowData, Operation operation)
//     // {
//     //     try
//     //     {
//     //         DataService.Instance.StoreData(
//     //             dataStructureId: rowData.Entity.RootEntity.ParentItemId,
//     //             data: rowData.Row.Table.DataSet,
//     //             loadActualValuesAfterUpdate: false,
//     //             transactionId: null);
//     //     }
//     //     catch(DBConcurrencyException ex)
//     //     {
//     //         if(string.IsNullOrEmpty(ex.Message) 
//     //             && (ex.InnerException != null))
//     //         {
//     //             return Error.Conflict(ex.InnerException.Message);
//     //         }
//     //         return Conflict(ex.Message);
//     //     }
//     //     return SessionStore.GetChangeInfo(
//     //         requestingGrid: null, 
//     //         row: rowData.Row, 
//     //         operation: operation, 
//     //         RowStateProcessor: null);
//     // }
//     protected Result<RowData, Error> AmbiguousInputToRowData(
//         AmbiguousInput input, IDataService dataService)
//     {
//         if(input.SessionFormIdentifier == Guid.Empty)
//         {
//             return 
//                 FindItem<FormReferenceMenuItem>(input.MenuId)
//                 .BindSuccessFailure(
//                     onSuccess: item =>  
//                         Authorize(item)
//                         .Bind(menuItem => GetEntityData(
//                             input.DataStructureEntityId,
//                             (FormReferenceMenuItem) menuItem))
//                         .Bind(CheckEntityBelongsToMenu)
//                         .Bind(entityData => GetRow(
//                             dataService,
//                             entityData.Entity,
//                             input.DataStructureEntityId,
//                             Guid.Empty,
//                             input.RowId)),
//                     onFailure: () => 
//                         AuthorizeQueueItem(input.MenuId)
//                         .Bind(_ => FindEntity(input.DataStructureEntityId))
//                         .Bind(entity => GetRow(
//                             dataService,
//                             entity,
//                             input.DataStructureEntityId,
//                             Guid.Empty,
//                             input.RowId))
//                 );
//         }
//         else
//         {
//             return
//                 FindEntity(input.DataStructureEntityId)
//                     .Map(dataStructureEntity =>
//                         sessionObjects.UIService.GetRow(
//                             input.SessionFormIdentifier, dataStructureEntity.Name,
//                             dataStructureEntity, input.RowId));
//         }
//     }
//
//     private Result<Guid, Error> AuthorizeQueueItem(Guid menuId)
//     {
//         DataTable workQueues = ServiceManager.Services
//             .GetService<IWorkQueueService>()
//             .UserQueueList()
//             .Tables[0];
//         bool menuIdBelongToReachableQueue = workQueues.Rows
//             .Cast<DataRow>()
//             .Any(row => (Guid)row["Id"] == menuId);
//
//         return menuIdBelongToReachableQueue
//             ? Result.Success<Guid, Error>(menuId)
//             : Result.Failure<Guid, Error>(
//                 Error.NotFound("Object with requested id not found."));
//     }
//
//     protected Result<Guid, Error> AmbiguousInputToEntityId(
//         AmbiguousInput input)
//     {
//         if(input.SessionFormIdentifier == Guid.Empty)
//         {
//             return FindItem<FormReferenceMenuItem>(input.MenuId)
//                 .Bind(Authorize)
//                 .Bind(menuItem => GetEntityData(
//                     input.DataStructureEntityId, 
//                     (FormReferenceMenuItem)menuItem))
//                 .Bind(CheckEntityBelongsToMenu)
//                 .Bind(EntityDataToEntityId);
//         }
//         else
//         {
//             return sessionObjects.UIService.GetEntityId(
//                 input.SessionFormIdentifier, input.Entity);
//         }
//     } 
//     protected Result<EntityData, Error> EntityIdentificationToEntityData(
//         IEntityIdentification input)
//     {
//         if(input.SessionFormIdentifier == Guid.Empty)
//         {
//             return FindItem<FormReferenceMenuItem>(input.MenuId)
//                 .Bind(Authorize)
//                 .Bind(menuItem => GetEntityData(
//                     input.DataStructureEntityId, 
//                     (FormReferenceMenuItem)menuItem))
//                 .Bind(CheckEntityBelongsToMenu);
//         }
//         else
//         {
//             return FindItem<FormReferenceMenuItem>(input.MenuId)
//                 .Bind(menuItem => GetEntityData(
//                     input.DataStructureEntityId, menuItem));
//         }
//     }
//     protected Result<Guid, Error> EntityDataToEntityId(
//         EntityData entityData)
//     {
//         return Result.Ok<Guid, Error>(entityData.Entity.EntityId);
//     }
//
//     // protected Result<DataStructureQuery, Error> AddMethodAndSource(
//     //     Guid sessionFormIdentifier, Guid masterRowId, EntityData entityData,
//     //     DataStructureQuery query)
//     // {
//     //     bool isLazyLoaded = entityData.MenuItem.ListDataStructure != null;
//     //     if(isLazyLoaded)
//     //     {
//     //         if(entityData.MenuItem.ListEntity.Name == entityData.Entity.Name)
//     //         {
//     //             query.MethodId = entityData.MenuItem.ListMethodId;
//     //             query.SortSetId = entityData.MenuItem.ListSortSetId;
//     //             query.DataSourceId
//     //                 = entityData.MenuItem.ListDataStructure.Id;
//     //             // get parameters from session store
//     //             var parameters = sessionObjects.UIService.GetParameters(
//     //                 sessionFormIdentifier);
//     //             foreach(var key in parameters.Keys)
//     //             {
//     //                 query.Parameters.Add(
//     //                     new QueryParameter(key.ToString(),
//     //                         parameters[key]));
//     //             }
//     //         }
//     //         else
//     //         {
//     //             return FindItem<DataStructureMethod>(entityData.MenuItem.MethodId)
//     //                 .Map(CustomParameterService.GetFirstNonCustomParameter)
//     //                 .Bind(parameterName =>
//     //                 {
//     //                     query.DataSourceId
//     //                         = entityData.Entity.RootEntity.ParentItemId;
//     //                     query.Parameters.Add(new QueryParameter(
//     //                         parameterName, masterRowId));
//     //                     if(masterRowId == Guid.Empty)
//     //                     {
//     //                         return Result
//     //                             .Failure<DataStructureQuery, Error>(
//     //                                 BadRequest("MasterRowId cannot be empty"));
//     //                     }
//     //
//     //                     query.MethodId = entityData.MenuItem.MethodId;
//     //                     query.SortSetId = entityData.MenuItem.SortSetId;
//     //                     return Result
//     //                         .Success<DataStructureQuery, Error>(query);
//     //                 });
//     //         }
//     //     }
//     //     else
//     //     {
//     //         query.MethodId = entityData.MenuItem.MethodId;
//     //         query.SortSetId = entityData.MenuItem.SortSetId;
//     //         query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
//     //     }
//     //
//     //     return Result.Ok<DataStructureQuery, Error>(query);
//     // }
//
//     // protected CustomOrderings GetOrderings(List<IRowOrdering> orderingList)
//     // {
//     //     var orderings = orderingList
//     //         .Select((inputOrdering, i) => 
//     //             new Ordering(
//     //                 columnName: inputOrdering.ColumnId, 
//     //                 direction: inputOrdering.Direction, 
//     //                 lookupId: inputOrdering.LookupId, 
//     //                 sortOrder: i + 1000)
//     //         ).ToList();
//     //     return new CustomOrderings(orderings);
//     // }
//     
//     protected Result<DataStructureQuery, Error> GetRowsGetQuery(
//         ILazyRowLoadInput input, EntityData entityData)
//     {
//         var customOrderings = GetOrderings(input.OrderingList);
//
//         if(input.RowOffset != 0 && customOrderings.IsEmpty)
//         {
//             return Result.Failure<DataStructureQuery, Error>(Error.BadRequest( $"Ordering must be specified if \"{nameof(input.RowOffset)}\" is specified"));
//         }
//         var query = new DataStructureQuery
//         {
//             Entity = entityData.Entity.Name,
//             CustomFilters = new CustomFilters
//             {
//                 Filters = input.Filter,
//                 FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>()
//             },
//             CustomOrderings = customOrderings,
//             RowLimit = input.RowLimit,
//             RowOffset = input.RowOffset,
//             ColumnsInfo = new ColumnsInfo(input.ColumnNames
//                     .Select(colName =>
//                     {
//                         var column = entityData.Entity.Column(colName);
//                         var field = column.Field;
//                         return new ColumnData(
//                             name: colName,
//                             isVirtual: (field is DetachedField
//                             || column.IsWriteOnly),
//                             defaultValue: (field as DetachedField)
//                             ?.DefaultValue?.Value,
//                             hasRelation: (field as DetachedField)
//                             ?.ArrayRelation != null);
//                     })
//                     .ToList(),
//                 renderSqlForDetachedFields: true),
//             ForceDatabaseCalculation = true,
//         };
//         
//         if (input.Parameters != null)
//         {
//             foreach (var pair in input.Parameters)
//             {
//                 query.Parameters.Add(new QueryParameter(
//                     pair.Key, pair.Value));
//             } 
//         }
//         return AddMethodAndSource(
//             input.SessionFormIdentifier, input.MasterRowId, entityData, query);
//     }
//
//
//     protected Result<IEnumerable<Dictionary<string, object>>, Error> 
//         ExecuteDataReaderGetPairs(
//         DataStructureQuery dataStructureQuery)
//     {
//         var linesAsPairs = dataService
//             .ExecuteDataReaderReturnPairs(dataStructureQuery);
//         return Result.Ok<IEnumerable<Dictionary<string, object>>, Error>(linesAsPairs);
//     }
//
//     protected Result<IEnumerable<object>, Error> ExecuteDataReader(
//         DataStructureQuery dataStructureQuery, Guid methodId)
//     {
//         Result<DataStructureMethod, Error> method 
//             = FindItem<DataStructureMethod>(dataStructureQuery.MethodId);
//         if(method.IsSuccess)
//         {
//             var structureMethod = method.Value;
//             if(structureMethod is DataStructureWorkflowMethod)
//             {
//                 var menuItem = FindItem<FormReferenceMenuItem>(methodId)
//                     .Value;
//                 IEnumerable<object> result = LoadData(
//                     menuItem,dataStructureQuery).ToList();
//                 return Result.Ok<IEnumerable<object>, Error>(result);
//             }
//         }
//         var linesAsArrays = dataService
//             .ExecuteDataReader(dataStructureQuery)
//             .ToList();
//         return Result.Ok<IEnumerable<object>, Error>(linesAsArrays);
//     }
//     
//     private IEnumerable<object> LoadData(
//         FormReferenceMenuItem menuItem, 
//         DataStructureQuery dataStructureQuery)
//     {
//         var datasetBuilder = new DataSetBuilder();
//         var data = datasetBuilder.InitializeFullStructure(
//             menuItem.ListDataStructureId,menuItem.DefaultSet);
//         var listData = datasetBuilder.InitializeListStructure(
//             data,menuItem.ListEntity.Name,false);
//         return TransformData(datasetBuilder.LoadListData(
//                 new List<string>(), listData, 
//                 menuItem.ListEntity.Name, menuItem.ListSortSet,menuItem), 
//             dataStructureQuery);
//     }
//     
//     private IEnumerable<object> TransformData(
//         DataSet dataSet, DataStructureQuery query)
//     {
//         var table = dataSet.Tables[0];
//         foreach(DataRow dataRow in table.Rows)
//         {
//             var values = new object[query.ColumnsInfo.Count];
//             for(var i = 0; i < query.ColumnsInfo.Count; i++)
//             {
//                 values[i] = dataRow.Field<object>(query.ColumnsInfo.Columns[i].Name);
//             }
//             yield return ProcessReaderOutput(
//                 values, query.ColumnsInfo);
//         }
//     }
//     
//     private List<object> ProcessReaderOutput(object[] values, ColumnsInfo columnsInfo)
//     {
//         if(columnsInfo == null)
//         {
//             throw new ArgumentNullException(nameof(columnsInfo));
//         }
//         var updatedValues = new List<object>();
//         for(var i = 0; i < columnsInfo.Count; i++)
//         {
//             updatedValues.Add(values[i]);
//         }
//         return updatedValues;
//     }
//     protected Result<DataStructureQuery, Error> WorkQueueGetRowsGetRowsQuery(
//         ILazyRowLoadInput input, WorkQueueSessionStore sessionStore)
//     {
//         var customOrderings = GetOrderings(input.OrderingList);
//         if (input.RowOffset != 0 && customOrderings.IsEmpty)
//         {
//             return Result.Failure<DataStructureQuery, Error>(Error.BadRequest($"Ordering must be specified if \"{nameof(input.RowOffset)}\" is specified"));
//         }
//         var query = new DataStructureQuery
//         {
//             Entity = workQueueEntity,
//             CustomFilters = new CustomFilters
//             {
//                 Filters = input.Filter,
//                 FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>()
//             },
//             CustomOrderings = customOrderings,
//             RowLimit = input.RowLimit,
//             RowOffset = input.RowOffset,
//             ColumnsInfo = new ColumnsInfo(input.ColumnNames
//                     .Select(colName =>
//                     {
//                         return new ColumnData(
//                             name: colName,
//                             isVirtual: false,
//                             defaultValue: null,
//                         hasRelation: false);
//                     })
//                     .ToList(),
//                 renderSqlForDetachedFields: true),
//             ForceDatabaseCalculation = true,
//             MethodId = sessionStore.WQClass.WorkQueueStructureUserListMethodId,
//             SortSetId = sessionStore.WQClass.WorkQueueStructureSortSetId,
//             DataSourceId = sessionStore.WQClass.WorkQueueStructureId
//         };
//         if (input.Parameters != null)
//         {
//             foreach (var pair in input.Parameters)
//             {
//                 query.Parameters.Add(new QueryParameter(
//                     pair.Key, pair.Value));
//             }
//         }
//         var parameters = sessionObjects.UIService.GetParameters(
//             sessionStore.Id);
//         foreach (var key in parameters.Keys)
//         {
//             query.Parameters.Add(
//                 new QueryParameter(key.ToString(),
//                     parameters[key]));
//         }
//         query.Parameters.Add(new QueryParameter(
//             "WorkQueueEntry_parWorkQueueId", sessionStore.Request.ObjectId));
//         return query;
//     }
// }
//
// public class EntityData
// {
//     public FormReferenceMenuItem MenuItem { get; set; }
//     public DataStructureEntity Entity { get; set; }
// }
//
// public class Error
// {
//     private Error(string message, ErrorType type)
//     {
//         Message = message;
//         Type = type;
//     }
//
//     public string Message { get; }
//     public ErrorType Type { get;}
//
//     public static Error NotFound(string message)
//     {
//         return new Error(message, ErrorType.NotFound);
//     }
//     public static Error BadRequest(string message)
//     {
//         return new Error(message, ErrorType.BadRequest);
//     }
//     public static Error InternalServerError(string message)
//     {
//         return new Error(message, ErrorType.InternalServerError);
//     }
//     public static Error Conflict(string message)
//     {
//         return new Error(message, ErrorType.Conflict);
//     }   
//     public static Error Forbid(string message=null)
//     {
//         return new Error(message, ErrorType.Forbid);
//     }
// }
//
// public enum ErrorType
// {
//     NotFound, BadRequest, InternalServerError, Conflict, Forbid
// }