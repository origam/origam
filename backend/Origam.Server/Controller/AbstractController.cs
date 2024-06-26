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

using IdentityServer4;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Rule;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server;
using Origam.Server.Model;
using Origam.Server.Model.UIService;
using Origam.Server.Session_Stores;
using Origam.Server.Extensions;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Controller;
[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
[ApiController]
[Route("internalApi/[controller]")]
public abstract class AbstractController: ControllerBase
{
    protected readonly SessionObjects sessionObjects;
    protected readonly IDataService dataService;
    protected readonly string workQueueEntity = "WorkQueueEntry";
    protected class EntityData
    {
        public FormReferenceMenuItem MenuItem { get; set; }
        public DataStructureEntity Entity { get; set; }
    }
    // ReSharper disable once InconsistentNaming
    protected readonly ILogger<AbstractController> log;
    protected AbstractController(ILogger<AbstractController> log, SessionObjects sessionObjects)
    {
        this.log = log;
        this.sessionObjects = sessionObjects;
        dataService = DataServiceFactory.GetDataService();
    }
    protected static MenuLookupIndex MenuLookupIndex {
        get
        {
            const string allowedLookups = "AllowedLookups";
            if(!OrigamUserContext.Context.ContainsKey(allowedLookups))
            {
                OrigamUserContext.Context.Add(
                    allowedLookups, new MenuLookupIndex());
            }
            var lookupIndex = (MenuLookupIndex)OrigamUserContext.Context[
                allowedLookups];
            return lookupIndex;
        }
    }
    protected IActionResult RunWithErrorHandler(Func<IActionResult> func)
    {
        try
        {
            return func();
        }
        catch (SessionExpiredException ex)
        {
            return NotFound(ex);
        }
        catch (RowNotFoundException ex)
        {
            return NotFound(ex);
        }
        catch (DBConcurrencyException ex)
        {
            log.LogError(ex, ex.Message);
            return StatusCode(409, ex);
        }
        catch (ServerObjectDisposedException ex)
        {
            return StatusCode(474, ex); // Suggests to the client that this error could be ignored
        }
        catch (UIException ex)
        {
            return StatusCode(422, ex);
        }
        catch (Exception ex)
        {
            if (ex is IUserException)
            {
                return StatusCode(420, ex);
            }
            log.LogOrigamError(ex, ex.Message);
            return StatusCode(500, ex);
        }
    }
    protected Result<AbstractMenuItem, IActionResult> Authorize(
        AbstractMenuItem menuItem)
    {
        return SecurityManager.GetAuthorizationProvider().Authorize(
            User, menuItem.Roles)
            ? Result.Success<AbstractMenuItem, IActionResult>(menuItem)
            : Result.Failure<AbstractMenuItem, IActionResult>(Forbid());
    }
    protected Result<T,IActionResult> FindItem<T>(Guid id) where T : class
    {
        return !(ServiceManager.Services
            .GetService<IPersistenceService>()
            .SchemaProvider
            .RetrieveInstance(typeof(T), new Key(id), true, false) is T instance)
            ? Result.Failure<T, IActionResult>(
                NotFound("Object with requested id not found."))
            : Result.Success<T, IActionResult>(instance);
    }
    protected Result<EntityData, IActionResult> GetEntityData(
        Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
    {
        return FindEntity(dataStructureEntityId)
            .Map(entity => 
                new EntityData {MenuItem = menuItem, Entity = entity});
    }
    protected Result<EntityData, IActionResult> CheckEntityBelongsToMenu(
        EntityData entityData)
    {
        return (entityData.MenuItem.Screen.DataStructure.Id 
            == entityData.Entity.RootEntity.ParentItemId)
            ? Result.Success<EntityData, IActionResult>(entityData)
            : Result.Failure<EntityData, IActionResult>(
                BadRequest("The requested Entity does not belong to the menu."));
    }
    protected Result<RowData, IActionResult> GetRow(
        IDataService dataService,
        DataStructureEntity entity, Guid dataStructureEntityId,
        Guid methodId, Guid rowId)
    {
        var rows = SessionStore.LoadRows(dataService, entity, 
            dataStructureEntityId, methodId, new List<Guid> { rowId });
        if(rows.Count == 0)
        {
            return Result.Failure<RowData, IActionResult>(
                NotFound("Requested data row was not found."));
        }
        return Result.Success<RowData, IActionResult>(
            new RowData{Row = rows[0], Entity = entity});
    }
    protected IActionResult UnwrapReturnValue(
        Result<IActionResult, IActionResult> result)
    {
        return result.IsSuccess ? result.Value : result.Error;
    }
    protected Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
    {
        return FindItem<DataStructureEntity>(id)
            .OnFailureCompensate(error =>
                Result.Failure<DataStructureEntity, IActionResult>(
                    NotFound("Requested DataStructureEntity not found. " 
                    + error.GetMessage())));
    }
    protected IActionResult SubmitChange(
        RowData rowData, Operation operation)
    {
        try
        {
            DataService.Instance.StoreData(
                dataStructureId: rowData.Entity.RootEntity.ParentItemId,
                data: rowData.Row.Table.DataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: null);
        }
        catch(DBConcurrencyException ex)
        {
            if(string.IsNullOrEmpty(ex.Message) 
                && (ex.InnerException != null))
            {
                return Conflict(ex.InnerException.Message);
            }
            return Conflict(ex.Message);
        }
        return Ok(SessionStore.GetChangeInfo(
            requestingGrid: null, 
            row: rowData.Row, 
            operation: operation, 
            rowStateProcessor: null));
    }
    protected Result<RowData, IActionResult> AmbiguousInputToRowData(
        AmbiguousInput input, IDataService dataService)
    {
        if(input.SessionFormIdentifier == Guid.Empty)
        {
            return 
                FindItem<FormReferenceMenuItem>(input.MenuId)
                .BindSuccessFailure(
                    onSuccess: item =>  
                        Authorize(item)
                        .Bind(menuItem => GetEntityData(
                            input.DataStructureEntityId,
                            (FormReferenceMenuItem) menuItem))
                        .Bind(CheckEntityBelongsToMenu)
                        .Bind(entityData => GetRow(
                            dataService,
                            entityData.Entity,
                            input.DataStructureEntityId,
                            Guid.Empty,
                            input.RowId)),
                    onFailure: () => 
                        AuthorizeQueueItem(input.MenuId)
                        .Bind(_ => FindEntity(input.DataStructureEntityId))
                        .Bind(entity => GetRow(
                            dataService,
                            entity,
                            input.DataStructureEntityId,
                            Guid.Empty,
                            input.RowId))
                );
        }
        else
        {
            return
                FindEntity(input.DataStructureEntityId)
                    .Bind(dataStructureEntity =>
                        sessionObjects.UIService.GetRow(
                            input.SessionFormIdentifier, dataStructureEntity.Name,
                            dataStructureEntity, input.RowId));
        }
    }
    private Result<Guid, IActionResult> AuthorizeQueueItem(Guid menuId)
    {
        DataTable workQueues = ServiceManager.Services
            .GetService<IWorkQueueService>()
            .UserQueueList()
            .Tables[0];
        bool menuIdBelongToReachableQueue = workQueues.Rows
            .Cast<DataRow>()
            .Any(row => (Guid)row["Id"] == menuId);
        return menuIdBelongToReachableQueue
            ? Result.Success<Guid, IActionResult>(menuId)
            : Result.Failure<Guid, IActionResult>(
                NotFound("Object with requested id not found."));
    }
    protected Result<Guid, IActionResult> AmbiguousInputToEntityId(
        AmbiguousInput input)
    {
        if(input.SessionFormIdentifier == Guid.Empty)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .Bind(Authorize)
                .Bind(menuItem => GetEntityData(
                    input.DataStructureEntityId, 
                    (FormReferenceMenuItem)menuItem))
                .Bind(CheckEntityBelongsToMenu)
                .Bind(EntityDataToEntityId);
        }
        else
        {
            return sessionObjects.UIService.GetEntityId(
                input.SessionFormIdentifier, input.Entity);
        }
    } 
    protected Result<EntityData, IActionResult> EntityIdentificationToEntityData(
        IEntityIdentification input)
    {
        if(input.SessionFormIdentifier == Guid.Empty)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .Bind(Authorize)
                .Bind(menuItem => GetEntityData(
                    input.DataStructureEntityId, 
                    (FormReferenceMenuItem)menuItem))
                .Bind(CheckEntityBelongsToMenu);
        }
        else
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .Bind(menuItem => GetEntityData(
                    input.DataStructureEntityId, menuItem));
        }
    }
    protected Result<Guid, IActionResult> EntityDataToEntityId(
        EntityData entityData)
    {
        return Result.Ok<Guid, IActionResult>(entityData.Entity.EntityId);
    }
    protected IActionResult ToActionResult(object obj)
    {
        return Ok(obj);
    }
    
    protected Result<DataStructureQuery, IActionResult> AddMethodAndSource(
        Guid sessionFormIdentifier, Guid masterRowId, EntityData entityData,
        DataStructureQuery query)
    {
        bool isLazyLoaded = entityData.MenuItem.ListDataStructure != null;
        if(isLazyLoaded)
        {
            if(entityData.MenuItem.ListEntity.Name == entityData.Entity.Name)
            {
                query.MethodId = entityData.MenuItem.ListMethodId;
                query.SortSetId = entityData.MenuItem.ListSortSetId;
                query.DataSourceId
                    = entityData.MenuItem.ListDataStructure.Id;
                // get parameters from session store
                var parameters = sessionObjects.UIService.GetParameters(
                    sessionFormIdentifier);
                foreach(var key in parameters.Keys)
                {
                    query.Parameters.Add(
                        new QueryParameter(key.ToString(),
                            parameters[key]));
                }
            }
            else
            {
                return FindItem<DataStructureMethod>(entityData.MenuItem.MethodId)
                    .Map(CustomParameterService.GetFirstNonCustomParameter)
                    .Bind(parameterName =>
                    {
                        query.DataSourceId
                            = entityData.Entity.RootEntity.ParentItemId;
                        query.Parameters.Add(new QueryParameter(
                            parameterName, masterRowId));
                        if(masterRowId == Guid.Empty)
                        {
                            return Result
                                .Failure<DataStructureQuery, IActionResult>(
                                    BadRequest("MasterRowId cannot be empty"));
                        }
                        query.MethodId = entityData.MenuItem.MethodId;
                        query.SortSetId = entityData.MenuItem.SortSetId;
                        return Result
                            .Success<DataStructureQuery, IActionResult>(query);
                    });
            }
        }
        else
        {
            query.MethodId = entityData.MenuItem.MethodId;
            query.SortSetId = entityData.MenuItem.SortSetId;
            query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
        }
        return Result.Ok<DataStructureQuery, IActionResult>(query);
    }
    protected CustomOrderings GetOrderings(List<IRowOrdering> orderingList)
    {
        var orderings = orderingList
            .Select((inputOrdering, i) => 
                new Ordering(
                    columnName: inputOrdering.ColumnId, 
                    direction: inputOrdering.Direction, 
                    lookupId: inputOrdering.LookupId, 
                    sortOrder: i + 1000)
            ).ToList();
        return new CustomOrderings(orderings);
    }
    
    protected Result<DataStructureQuery, IActionResult> GetRowsGetQuery(
        ILazyRowLoadInput input, EntityData entityData)
    {
        var customOrderings = GetOrderings(input.OrderingList);
        if(input.RowOffset != 0 && customOrderings.IsEmpty)
        {
            return Result.Failure<DataStructureQuery, IActionResult>(BadRequest( $"Ordering must be specified if \"{nameof(input.RowOffset)}\" is specified"));
        }
        var query = new DataStructureQuery
        {
            Entity = entityData.Entity.Name,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>()
            },
            CustomOrderings = customOrderings,
            RowLimit = input.RowLimit,
            RowOffset = input.RowOffset,
            ColumnsInfo = new ColumnsInfo(input.ColumnNames
                    .Select(colName =>
                    {
                        var column = entityData.Entity.Column(colName);
                        var field = column.Field;
                        return new ColumnData(
                            name: colName,
                            isVirtual: (field is DetachedField
                            || column.IsWriteOnly),
                            defaultValue: (field as DetachedField)
                            ?.DefaultValue?.Value,
                            hasRelation: (field as DetachedField)
                            ?.ArrayRelation != null);
                    })
                    .ToList(),
                renderSqlForDetachedFields: true),
            ForceDatabaseCalculation = true,
        };
        
        if (input.Parameters != null)
        {
            foreach (var pair in input.Parameters)
            {
                query.Parameters.Add(new QueryParameter(
                    pair.Key, pair.Value));
            } 
        }
        return AddMethodAndSource(
            input.SessionFormIdentifier, input.MasterRowId, entityData, query);
    }
    protected Result<IEnumerable<Dictionary<string, object>>, IActionResult> 
        ExecuteDataReaderGetPairs(
        DataStructureQuery dataStructureQuery)
    {
        var linesAsPairs = dataService
            .ExecuteDataReaderReturnPairs(dataStructureQuery);
        return Result.Ok<IEnumerable<Dictionary<string, object>>, IActionResult>(linesAsPairs);
    }
    protected Result<IEnumerable<object>, IActionResult> ExecuteDataReader(
        DataStructureQuery dataStructureQuery, Guid methodId)
    {
        Result<DataStructureMethod, IActionResult> method 
            = FindItem<DataStructureMethod>(dataStructureQuery.MethodId);
        if(method.IsSuccess)
        {
            var structureMethod = method.Value;
            if(structureMethod is DataStructureWorkflowMethod)
            {
                var menuItem = FindItem<FormReferenceMenuItem>(methodId)
                    .Value;
                IEnumerable<object> result = LoadData(
                    menuItem,dataStructureQuery).ToList();
                return Result.Ok<IEnumerable<object>, IActionResult>(result);
            }
        }
        var linesAsArrays = dataService
            .ExecuteDataReader(dataStructureQuery)
            .ToList();
        return Result.Ok<IEnumerable<object>, IActionResult>(linesAsArrays);
    }
    
    private IEnumerable<object> LoadData(
        FormReferenceMenuItem menuItem, 
        DataStructureQuery dataStructureQuery)
    {
        var datasetBuilder = new DataSetBuilder();
        var data = datasetBuilder.InitializeFullStructure(
            menuItem.ListDataStructureId,menuItem.DefaultSet);
        var listData = datasetBuilder.InitializeListStructure(
            data,menuItem.ListEntity.Name,false);
        return TransformData(datasetBuilder.LoadListData(
                new List<string>(), listData, 
                menuItem.ListEntity.Name, menuItem.ListSortSet,menuItem), 
            dataStructureQuery);
    }
    
    private IEnumerable<object> TransformData(
        DataSet dataSet, DataStructureQuery query)
    {
        var table = dataSet.Tables[0];
        foreach(DataRow dataRow in table.Rows)
        {
            var values = new object[query.ColumnsInfo.Count];
            for(var i = 0; i < query.ColumnsInfo.Count; i++)
            {
                values[i] = dataRow.Field<object>(query.ColumnsInfo.Columns[i].Name);
            }
            yield return ProcessReaderOutput(
                values, query.ColumnsInfo);
        }
    }
    
    private List<object> ProcessReaderOutput(object[] values, ColumnsInfo columnsInfo)
    {
        if(columnsInfo == null)
        {
            throw new ArgumentNullException(nameof(columnsInfo));
        }
        var updatedValues = new List<object>();
        for(var i = 0; i < columnsInfo.Count; i++)
        {
            updatedValues.Add(values[i]);
        }
        return updatedValues;
    }
    protected Result<DataStructureQuery, IActionResult> WorkQueueGetRowsGetRowsQuery(
        ILazyRowLoadInput input, WorkQueueSessionStore sessionStore)
    {
        var customOrderings = GetOrderings(input.OrderingList);
        if (input.RowOffset != 0 && customOrderings.IsEmpty)
        {
            return Result.Failure<DataStructureQuery, IActionResult>(BadRequest($"Ordering must be specified if \"{nameof(input.RowOffset)}\" is specified"));
        }
        var query = new DataStructureQuery
        {
            Entity = workQueueEntity,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>()
            },
            CustomOrderings = customOrderings,
            RowLimit = input.RowLimit,
            RowOffset = input.RowOffset,
            ColumnsInfo = new ColumnsInfo(input.ColumnNames
                    .Select(colName =>
                    {
                        return new ColumnData(
                            name: colName,
                            isVirtual: false,
                            defaultValue: null,
                        hasRelation: false);
                    })
                    .ToList(),
                renderSqlForDetachedFields: true),
            ForceDatabaseCalculation = true,
            MethodId = sessionStore.WQClass.WorkQueueStructureUserListMethodId,
            SortSetId = sessionStore.WQClass.WorkQueueStructureSortSetId,
            DataSourceId = sessionStore.WQClass.WorkQueueStructureId
        };
        if (input.Parameters != null)
        {
            foreach (var pair in input.Parameters)
            {
                query.Parameters.Add(new QueryParameter(
                    pair.Key, pair.Value));
            }
        }
        var parameters = sessionObjects.UIService.GetParameters(
            sessionStore.Id);
        foreach (var key in parameters.Keys)
        {
            query.Parameters.Add(
                new QueryParameter(key.ToString(),
                    parameters[key]));
        }
        query.Parameters.Add(new QueryParameter(
            "WorkQueueEntry_parWorkQueueId", sessionStore.Request.ObjectId));
        return query;
    }
}
