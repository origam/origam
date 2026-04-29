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
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Extensions;
using Origam.Server.Model;
using Origam.Server.Model.UIService;
using Origam.Server.Session_Stores;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Controller;

[Authorize(Policy = "InternalApi")]
[ApiController]
[Route(template: "internalApi/[controller]")]
public abstract class AbstractController : ControllerBase
{
    protected readonly SessionObjects sessionObjects;
    private readonly IWebHostEnvironment environment;
    protected readonly IDataService dataService;
    protected readonly string workQueueEntity = "WorkQueueEntry";

    protected class EntityData
    {
        public FormReferenceMenuItem MenuItem { get; set; }
        public DataStructureEntity Entity { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    protected readonly ILogger<AbstractController> log;

    protected AbstractController(
        ILogger<AbstractController> log,
        SessionObjects sessionObjects,
        IWebHostEnvironment environment
    )
    {
        this.log = log;
        this.sessionObjects = sessionObjects;
        this.environment = environment;
        dataService = DataServiceFactory.GetDataService();
    }

    protected static MenuLookupIndex MenuLookupIndex
    {
        get
        {
            const string allowedLookups = "AllowedLookups";
            if (!OrigamUserContext.Context.ContainsKey(key: allowedLookups))
            {
                OrigamUserContext.Context.Add(key: allowedLookups, value: new MenuLookupIndex());
            }
            var lookupIndex = (MenuLookupIndex)OrigamUserContext.Context[key: allowedLookups];
            return lookupIndex;
        }
    }

    protected Result<AbstractMenuItem, IActionResult> Authorize(AbstractMenuItem menuItem)
    {
        return SecurityManager
            .GetAuthorizationProvider()
            .Authorize(principal: User, context: menuItem.Roles)
            ? Result.Success<AbstractMenuItem, IActionResult>(value: menuItem)
            : Result.Failure<AbstractMenuItem, IActionResult>(error: Forbid());
    }

    protected Result<T, IActionResult> FindItem<T>(Guid id)
        where T : class
    {
        return !(
            ServiceManager
                .Services.GetService<IPersistenceService>()
                .SchemaProvider.RetrieveInstance(
                    type: typeof(T),
                    primaryKey: new Key(id: id),
                    useCache: true,
                    throwNotFoundException: false
                )
            is T instance
        )
            ? Result.Failure<T, IActionResult>(
                error: NotFound(value: "Object with requested id not found.")
            )
            : Result.Success<T, IActionResult>(value: instance);
    }

    protected Result<EntityData, IActionResult> GetEntityData(
        Guid dataStructureEntityId,
        FormReferenceMenuItem menuItem
    )
    {
        return FindEntity(id: dataStructureEntityId)
            .Map(func: entity => new EntityData { MenuItem = menuItem, Entity = entity });
    }

    protected Result<EntityData, IActionResult> CheckEntityBelongsToMenu(EntityData entityData)
    {
        return (
            entityData.MenuItem.Screen.DataStructure.Id == entityData.Entity.RootEntity.ParentItemId
        )
            ? Result.Success<EntityData, IActionResult>(value: entityData)
            : Result.Failure<EntityData, IActionResult>(
                error: BadRequest(error: "The requested Entity does not belong to the menu.")
            );
    }

    protected Result<RowData, IActionResult> GetRow(
        IDataService dataService,
        DataStructureEntity entity,
        Guid dataStructureEntityId,
        Guid methodId,
        Guid rowId
    )
    {
        var rows = SessionStore.LoadRows(
            dataService: dataService,
            entity: entity,
            dataStructureEntityId: dataStructureEntityId,
            methodId: methodId,
            rowIds: new List<Guid> { rowId }
        );
        if (rows.Count == 0)
        {
            return Result.Failure<RowData, IActionResult>(
                error: NotFound(value: "Requested data row was not found.")
            );
        }
        return Result.Success<RowData, IActionResult>(
            value: new RowData { Row = rows[index: 0], Entity = entity }
        );
    }

    protected IActionResult UnwrapReturnValue(Result<IActionResult, IActionResult> result)
    {
        return result.IsSuccess ? result.Value : result.Error;
    }

    protected Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
    {
        return FindItem<DataStructureEntity>(id: id)
            .OnFailureCompensate(func: error =>
                Result.Failure<DataStructureEntity, IActionResult>(
                    error: NotFound(
                        value: "Requested DataStructureEntity not found. " + error.GetMessage()
                    )
                )
            );
    }

    protected IActionResult SubmitChange(RowData rowData, Operation operation)
    {
        try
        {
            DataService.Instance.StoreData(
                dataStructureId: rowData.Entity.RootEntity.ParentItemId,
                data: rowData.Row.Table.DataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: null
            );
        }
        catch (DBConcurrencyException ex)
        {
            if (string.IsNullOrEmpty(value: ex.Message) && (ex.InnerException != null))
            {
                return Conflict(error: ex.InnerException.Message);
            }
            return Conflict(error: ex.Message);
        }
        return Ok(
            value: SessionStore.GetChangeInfo(
                requestingGrid: null,
                row: rowData.Row,
                operation: operation,
                rowStateProcessor: null
            )
        );
    }

    protected Result<RowData, IActionResult> AmbiguousInputToRowData(
        AmbiguousInput input,
        IDataService dataService
    )
    {
        if (input.SessionFormIdentifier == Guid.Empty)
        {
            return FindItem<FormReferenceMenuItem>(id: input.MenuId)
                .BindSuccessFailure(
                    onSuccess: item =>
                        Authorize(menuItem: item)
                            .Bind(func: menuItem =>
                                GetEntityData(
                                    dataStructureEntityId: input.DataStructureEntityId,
                                    menuItem: (FormReferenceMenuItem)menuItem
                                )
                            )
                            .Bind(func: CheckEntityBelongsToMenu)
                            .Bind(func: entityData =>
                                GetRow(
                                    dataService: dataService,
                                    entity: entityData.Entity,
                                    dataStructureEntityId: input.DataStructureEntityId,
                                    methodId: Guid.Empty,
                                    rowId: input.RowId
                                )
                            ),
                    onFailure: () =>
                        AuthorizeQueueItem(menuId: input.MenuId)
                            .Bind(func: _ => FindEntity(id: input.DataStructureEntityId))
                            .Bind(func: entity =>
                                GetRow(
                                    dataService: dataService,
                                    entity: entity,
                                    dataStructureEntityId: input.DataStructureEntityId,
                                    methodId: Guid.Empty,
                                    rowId: input.RowId
                                )
                            )
                );
        }

        return FindEntity(id: input.DataStructureEntityId)
            .Bind(func: dataStructureEntity =>
                sessionObjects.UIService.GetRow(
                    sessionFormIdentifier: input.SessionFormIdentifier,
                    entity: dataStructureEntity.Name,
                    dataStructureEntity: dataStructureEntity,
                    rowId: input.RowId
                )
            );
    }

    private Result<Guid, IActionResult> AuthorizeQueueItem(Guid menuId)
    {
        DataTable workQueues = ServiceManager
            .Services.GetService<IWorkQueueService>()
            .UserQueueList()
            .Tables[index: 0];
        bool menuIdBelongToReachableQueue = workQueues
            .Rows.Cast<DataRow>()
            .Any(predicate: row => (Guid)row[columnName: "Id"] == menuId);
        return menuIdBelongToReachableQueue
            ? Result.Success<Guid, IActionResult>(value: menuId)
            : Result.Failure<Guid, IActionResult>(
                error: NotFound(value: "Object with requested id not found.")
            );
    }

    protected Result<Guid, IActionResult> AmbiguousInputToEntityId(AmbiguousInput input)
    {
        if (input.SessionFormIdentifier == Guid.Empty)
        {
            return FindItem<FormReferenceMenuItem>(id: input.MenuId)
                .Bind(func: Authorize)
                .Bind(func: menuItem =>
                    GetEntityData(
                        dataStructureEntityId: input.DataStructureEntityId,
                        menuItem: (FormReferenceMenuItem)menuItem
                    )
                )
                .Bind(func: CheckEntityBelongsToMenu)
                .Bind(func: EntityDataToEntityId);
        }

        return sessionObjects.UIService.GetEntityId(
            sessionFormIdentifier: input.SessionFormIdentifier,
            entity: input.Entity
        );
    }

    protected Result<EntityData, IActionResult> EntityIdentificationToEntityData(
        IEntityIdentification input
    )
    {
        if (input.SessionFormIdentifier == Guid.Empty)
        {
            return FindItem<FormReferenceMenuItem>(id: input.MenuId)
                .Bind(func: Authorize)
                .Bind(func: menuItem =>
                    GetEntityData(
                        dataStructureEntityId: input.DataStructureEntityId,
                        menuItem: (FormReferenceMenuItem)menuItem
                    )
                )
                .Bind(func: CheckEntityBelongsToMenu);
        }

        return FindItem<FormReferenceMenuItem>(id: input.MenuId)
            .Bind(func: menuItem =>
                GetEntityData(
                    dataStructureEntityId: input.DataStructureEntityId,
                    menuItem: menuItem
                )
            );
    }

    protected Result<Guid, IActionResult> EntityDataToEntityId(EntityData entityData)
    {
        return Result.Success<Guid, IActionResult>(value: entityData.Entity.EntityId);
    }

    protected IActionResult ToActionResult(object obj)
    {
        return Ok(value: obj);
    }

    protected Result<DataStructureQuery, IActionResult> AddMethodAndSource(
        Guid sessionFormIdentifier,
        Guid masterRowId,
        EntityData entityData,
        DataStructureQuery query
    )
    {
        bool isLazyLoaded = entityData.MenuItem.ListDataStructure != null;
        if (isLazyLoaded)
        {
            if (entityData.MenuItem.ListEntity.Name == entityData.Entity.Name)
            {
                query.MethodId = entityData.MenuItem.ListMethodId;
                query.SortSetId = entityData.MenuItem.ListSortSetId;
                query.DataSourceId = entityData.MenuItem.ListDataStructure.Id;
                // get parameters from session store
                var parameters = sessionObjects.UIService.GetParameters(
                    sessionFormIdentifier: sessionFormIdentifier
                );
                foreach (var key in parameters.Keys)
                {
                    query.Parameters.Add(
                        value: new QueryParameter(
                            _parameterName: key.ToString(),
                            value: parameters[key: key]
                        )
                    );
                }
            }
            else
            {
                return FindItem<DataStructureMethod>(id: entityData.MenuItem.MethodId)
                    .Map(func: CustomParameterService.GetFirstNonCustomParameter)
                    .Bind(func: parameterName =>
                    {
                        query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
                        query.Parameters.Add(
                            value: new QueryParameter(
                                _parameterName: parameterName,
                                value: masterRowId
                            )
                        );
                        if (masterRowId == Guid.Empty)
                        {
                            return Result.Failure<DataStructureQuery, IActionResult>(
                                error: BadRequest(error: "MasterRowId cannot be empty")
                            );
                        }
                        query.MethodId = entityData.MenuItem.MethodId;
                        query.SortSetId = entityData.MenuItem.SortSetId;
                        return Result.Success<DataStructureQuery, IActionResult>(value: query);
                    });
            }
        }
        else
        {
            query.MethodId = entityData.MenuItem.MethodId;
            query.SortSetId = entityData.MenuItem.SortSetId;
            query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
        }
        return Result.Success<DataStructureQuery, IActionResult>(value: query);
    }

    protected CustomOrderings GetOrderings(List<IRowOrdering> orderingList)
    {
        var orderings = orderingList
            .Select(
                selector: (inputOrdering, i) =>
                    new Ordering(
                        columnName: inputOrdering.ColumnId,
                        direction: inputOrdering.Direction,
                        lookupId: inputOrdering.LookupId,
                        sortOrder: i + 1000
                    )
            )
            .ToList();
        return new CustomOrderings(orderings: orderings);
    }

    protected Result<DataStructureQuery, IActionResult> GetRowsGetQuery(
        ILazyRowLoadInput input,
        EntityData entityData
    )
    {
        var customOrderings = GetOrderings(orderingList: input.OrderingList);
        if (input.RowOffset != 0 && customOrderings.IsEmpty)
        {
            return Result.Failure<DataStructureQuery, IActionResult>(
                error: BadRequest(
                    error: $"Ordering must be specified if \"{nameof(input.RowOffset)}\" is specified"
                )
            );
        }
        var query = new DataStructureQuery
        {
            Entity = entityData.Entity.Name,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>(),
            },
            CustomOrderings = customOrderings,
            RowLimit = input.RowLimit,
            RowOffset = input.RowOffset,
            ColumnsInfo = new ColumnsInfo(
                columns: input
                    .ColumnNames.Select(selector: colName =>
                    {
                        var column = entityData.Entity.Column(name: colName);
                        var field = column.Field;
                        return new ColumnData(
                            name: colName,
                            isVirtual: (field is DetachedField || column.IsWriteOnly),
                            defaultValue: (field as DetachedField)?.DefaultValue?.Value,
                            hasRelation: (field as DetachedField)?.ArrayRelation != null
                        );
                    })
                    .ToList(),
                renderSqlForDetachedFields: true
            ),
            ForceDatabaseCalculation = true,
        };

        if (input.Parameters != null)
        {
            foreach (var pair in input.Parameters)
            {
                query.Parameters.Add(
                    value: new QueryParameter(_parameterName: pair.Key, value: pair.Value)
                );
            }
        }
        return AddMethodAndSource(
            sessionFormIdentifier: input.SessionFormIdentifier,
            masterRowId: input.MasterRowId,
            entityData: entityData,
            query: query
        );
    }

    protected Result<
        IEnumerable<Dictionary<string, object>>,
        IActionResult
    > ExecuteDataReaderGetPairs(DataStructureQuery dataStructureQuery)
    {
        var linesAsPairs = dataService.ExecuteDataReaderReturnPairs(query: dataStructureQuery);
        return Result.Success<IEnumerable<Dictionary<string, object>>, IActionResult>(
            value: linesAsPairs
        );
    }

    protected Result<IEnumerable<object>, IActionResult> ExecuteDataReader(
        DataStructureQuery dataStructureQuery,
        Guid methodId
    )
    {
        Result<DataStructureMethod, IActionResult> method = FindItem<DataStructureMethod>(
            id: dataStructureQuery.MethodId
        );
        if (method.IsSuccess)
        {
            var structureMethod = method.Value;
            if (structureMethod is DataStructureWorkflowMethod)
            {
                var menuItem = FindItem<FormReferenceMenuItem>(id: methodId).Value;
                IEnumerable<object> result = LoadData(
                        menuItem: menuItem,
                        dataStructureQuery: dataStructureQuery
                    )
                    .ToList();
                return Result.Success<IEnumerable<object>, IActionResult>(value: result);
            }
        }
        var linesAsArrays = dataService
            .ExecuteDataReader(dataStructureQuery: dataStructureQuery)
            .ToList();
        return Result.Success<IEnumerable<object>, IActionResult>(value: linesAsArrays);
    }

    private IEnumerable<object> LoadData(
        FormReferenceMenuItem menuItem,
        DataStructureQuery dataStructureQuery
    )
    {
        var datasetBuilder = new DataSetBuilder();
        var data = datasetBuilder.InitializeFullStructure(
            id: menuItem.ListDataStructureId,
            defaultSet: menuItem.DefaultSet
        );
        var listData = datasetBuilder.InitializeListStructure(
            data: data,
            listEntity: menuItem.ListEntity.Name,
            isDbSource: false
        );
        return TransformData(
            dataSet: datasetBuilder.LoadListData(
                dataListLoadedColumns: new List<string>(),
                data: listData,
                listEntity: menuItem.ListEntity.Name,
                sortSet: menuItem.ListSortSet,
                _menuItem: menuItem
            ),
            query: dataStructureQuery
        );
    }

    private IEnumerable<object> TransformData(DataSet dataSet, DataStructureQuery query)
    {
        var table = dataSet.Tables[index: 0];
        foreach (DataRow dataRow in table.Rows)
        {
            var values = new object[query.ColumnsInfo.Count];
            for (var i = 0; i < query.ColumnsInfo.Count; i++)
            {
                values[i] = dataRow.Field<object>(
                    columnName: query.ColumnsInfo.Columns[index: i].Name
                );
            }
            yield return ProcessReaderOutput(values: values, columnsInfo: query.ColumnsInfo);
        }
    }

    private List<object> ProcessReaderOutput(object[] values, ColumnsInfo columnsInfo)
    {
        if (columnsInfo == null)
        {
            throw new ArgumentNullException(paramName: nameof(columnsInfo));
        }
        var updatedValues = new List<object>();
        for (var i = 0; i < columnsInfo.Count; i++)
        {
            updatedValues.Add(item: values[i]);
        }
        return updatedValues;
    }

    protected Result<DataStructureQuery, IActionResult> WorkQueueGetRowsGetRowsQuery(
        ILazyRowLoadInput input,
        WorkQueueSessionStore sessionStore
    )
    {
        var customOrderings = GetOrderings(orderingList: input.OrderingList);
        if (input.RowOffset != 0 && customOrderings.IsEmpty)
        {
            return Result.Failure<DataStructureQuery, IActionResult>(
                error: BadRequest(
                    error: $"Ordering must be specified if \"{nameof(input.RowOffset)}\" is specified"
                )
            );
        }
        var query = new DataStructureQuery
        {
            Entity = workQueueEntity,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>(),
            },
            CustomOrderings = customOrderings,
            RowLimit = input.RowLimit,
            RowOffset = input.RowOffset,
            ColumnsInfo = new ColumnsInfo(
                columns: input
                    .ColumnNames.Select(selector: colName =>
                    {
                        return new ColumnData(
                            name: colName,
                            isVirtual: false,
                            defaultValue: null,
                            hasRelation: false
                        );
                    })
                    .ToList(),
                renderSqlForDetachedFields: true
            ),
            ForceDatabaseCalculation = true,
            MethodId = sessionStore.WorkQueueClass.WorkQueueStructureUserListMethodId,
            SortSetId = sessionStore.WorkQueueClass.WorkQueueStructureSortSetId,
            DataSourceId = sessionStore.WorkQueueClass.WorkQueueStructureId,
        };
        if (input.Parameters != null)
        {
            foreach (var pair in input.Parameters)
            {
                query.Parameters.Add(
                    value: new QueryParameter(_parameterName: pair.Key, value: pair.Value)
                );
            }
        }
        var parameters = sessionObjects.UIService.GetParameters(
            sessionFormIdentifier: sessionStore.Id
        );
        foreach (var key in parameters.Keys)
        {
            query.Parameters.Add(
                value: new QueryParameter(
                    _parameterName: key.ToString(),
                    value: parameters[key: key]
                )
            );
        }
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "WorkQueueEntry_parWorkQueueId",
                value: sessionStore.Request.ObjectId
            )
        );
        return query;
    }
}
