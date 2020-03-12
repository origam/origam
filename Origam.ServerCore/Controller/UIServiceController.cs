#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server;
using Origam.ServerCore.Model.UIService;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using IdentityServer4;
using Microsoft.AspNetCore.Localization;
using Origam.Workbench;
using Origam.ServerCommon.Session_Stores;
using Origam.ServerCore.Resources;

namespace Origam.ServerCore.Controller
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("internalApi/[controller]")]
    public class UIServiceController : AbstractController
    {
        private readonly SessionObjects sessionObjects;
        private readonly IStringLocalizer<SharedResources> localizer;
        private readonly IDataLookupService lookupService;
        private readonly IDataService dataService;

        #region Endpoints
        public UIServiceController(
            SessionObjects sessionObjects,
            IStringLocalizer<SharedResources> localizer,
            ILogger<AbstractController> log) : base(log)
        {
            this.sessionObjects = sessionObjects;
            this.localizer = localizer;
            lookupService
                = ServiceManager.Services.GetService<IDataLookupService>();
            dataService = DataService.GetDataService();
        }
        [HttpGet("[action]/{locale}")]
        // ReSharper disable once UnusedParameter.Global
        public IActionResult InitPortal(string locale)
        {
            Analytics.Instance.Log("UI_INIT");
            //TODO: find out how to setup locale cookies and incorporate
            // locale resolver
            /*// set locale
            locale = locale.Replace("_", "-");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
            // set locale to the cookie
            Response.Cookies.Append(
                ORIGAMLocaleResolver.ORIGAM_CURRENT_LOCALE, locale);*/
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.InitPortal(4)));
        }
        [HttpPost("[action]")]
        public IActionResult InitUI([FromBody]UIRequest request)
        {
            return RunWithErrorHandler(() => Ok(
                // registerSession is important for session less handling
                sessionObjects.UIManager.InitUI(
                    request: request,
                    addChildSession: false,
                    parentSession: null,
                    basicUIService: sessionObjects.UIService)
            ));
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult DestroyUI(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                sessionObjects.UIService.DestroyUI(sessionFormIdentifier);
                return Ok();
            });
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult RefreshData(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.RefreshData(
                    sessionFormIdentifier, localizer)));
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult SaveDataQuery(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.SaveDataQuery(
                    sessionFormIdentifier)));
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult SaveData(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.SaveData(
                    sessionFormIdentifier)));
        }
        [HttpPost("[action]")]
        public IActionResult MasterRecord([FromBody]MasterRecordInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.GetRowData(input)));
        }
        [HttpPost("[action]")]
        public IActionResult GetData([FromBody]GetDataInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.GetData(input)));
        }
        [HttpPost("[action]")]
        public IActionResult RowStates([FromBody]RowStatesInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.RowStates(input)));
        }
        [HttpPost("[action]")]
        public IActionResult CreateObject(
            [FromBody][Required]CreateObjectInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.CreateObject(input)));
        }
        [HttpPost("[action]")]
        public IActionResult UpdateObject(
            [FromBody][Required]UpdateObjectInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.UpdateObject(input)));
        }
        [HttpPost("[action]")]
        public IActionResult DeleteObject(
            [FromBody][Required]DeleteObjectInput input)
        {
            //todo: handle deleting non existing objects
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.DeleteObject(input)));
        }
        [HttpPost("[action]")]
        public IActionResult ExecuteActionQuery(
            [FromBody][Required]ExecuteActionQueryInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.ExecuteActionQuery(input)));
        }
        [HttpPost("[action]")]
        public IActionResult ExecuteAction(
            [FromBody][Required]ExecuteActionInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.ExecuteAction(input)));
        }
        [HttpPost("[action]")]
        public IActionResult GetLookupLabels(
            [FromBody][Required]LookupLabelsInput input)
        {
            return RunWithErrorHandler(() =>
            {
                // todo: unify approach
                var checkResult = CheckLookup(input);
                if (checkResult != null)
                {
                    return checkResult;
                }
                var labelDictionary = GetLookupLabelsInternal(input);
                return Ok(labelDictionary);
            });
        }
        [HttpGet("[action]")]
        public IActionResult WorkQueueList()
        {
            return RunWithErrorHandler(() => 
                Ok(ServerCoreUIService.WorkQueueList(localizer)));
        }
        [HttpPost("[action]")]
        public IActionResult SaveObjectConfig(
            [FromBody][Required]SaveObjectConfigInput input)
        {
            return RunWithErrorHandler(() =>
            {
                sessionObjects.UIService.SaveObjectConfig(input);
                return Ok();
            });
        }
        [HttpPost("[action]")]
        public IActionResult SaveSplitPanelConfig(
            [FromBody][Required]SaveSplitPanelConfigInput input)
        {
            return RunWithErrorHandler(() =>
            {
                ServerCoreUIService.SaveSplitPanelConfig(input);
                return Ok();
            });
        }
        [HttpPost("[action]")]
        public IActionResult GetLookupLabelsEx([FromBody]LookupLabelsInput[] inputs)
        {
            return RunWithErrorHandler(() =>
            {
                var result = new Dictionary<Guid, Dictionary<object, string>>();
                foreach (var input in inputs)
                {
                    var checkResult = CheckLookup(input);
                    if (checkResult != null)
                    {
                        return checkResult;
                    }
                    var labelDictionary = GetLookupLabelsInternal(input);
                    result.Add(input.LookupId, labelDictionary);
                }
                return Ok(result);
            });
        }
        [HttpPost("[action]")]
        public IActionResult GetLookupList([FromBody]LookupListInput input)
        {
            //todo: implement GetFilterLookupList
            return LookupListInputToRowData(input)
                .OnSuccess(rowData => GetLookupRows(input, rowData))
                .OnSuccess(ToActionResult)
                .OnBoth<IActionResult,IActionResult>(UnwrapReturnValue);
        }
        [HttpPost("[action]")]
        public IActionResult GetRows([FromBody]GetRowsInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return FindItem<FormReferenceMenuItem>(input.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem => GetEntityData(
                        input.DataStructureEntityId, menuItem))
                    .OnSuccess(CheckEntityBelongsToMenu)
                    .OnSuccess(entityData => GetRowsGetQuery(input, entityData))
                    .OnSuccess(datastructureQuery=>ExecuteDataReader(
                        datastructureQuery, input))
                    .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
            });
        }
        [HttpPut("[action]")]
        public IActionResult Row([FromBody]UpdateRowInput input)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(
                    input.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => 
                    GetRow(
                        dataService,
                        entityData.Entity, 
                        input.DataStructureEntityId,
                        Guid.Empty,
                        input.RowId))
                .OnSuccess(rowData => FillRow(rowData, input.NewValues))
                .OnSuccess(rowData => SubmitChange(rowData, Operation.Update))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpPost("[action]")]
        public IActionResult Row([FromBody]NewRowInput input)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(
                    input.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => MakeEmptyRow(entityData.Entity))
                .OnSuccess(rowData => FillRow(input, rowData))
                .OnSuccess(rowData => SubmitChange(rowData, Operation.Create))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpPost("[action]")]
        public IActionResult NewEmptyRow([FromBody]NewEmptyRowInput input)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem =>
                    GetEntityData(input.DataStructureEntityId,
                        menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => MakeEmptyRow(entityData.Entity))
                .OnSuccess(PrepareNewRow)
                .OnSuccess(rowData => SubmitChange(rowData, Operation.Create))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpDelete("[action]")]
        public IActionResult Row([FromBody]DeleteRowInput input)
        {
            return
                FindItem<FormReferenceMenuItem>(input.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem => GetEntityData(
                        input.DataStructureEntityId, menuItem))
                    .OnSuccess(CheckEntityBelongsToMenu)
                    .OnSuccess(entityData => GetRow(
                        dataService,
                        entityData.Entity,
                        input.DataStructureEntityId,
                        Guid.Empty,
                        input.RowIdToDelete))
                    .OnSuccess(rowData =>
                    {
                        rowData.Row.Delete();
                        return SubmitDelete(rowData);
                    })
                    .OnSuccess(ThrowAwayReturnData)
                    .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpGet("[action]/{sessionFormIdentifier}")]
        public IActionResult WorkflowNextQuery(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.WorkflowNextQuery(
                    sessionFormIdentifier)));
        }
        [HttpPost("[action]")]
        public IActionResult WorkflowNext(
            [FromBody][Required]WorkflowNextInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.WorkflowNext(input)));
        }
        [HttpGet("[action]/{sessionFormIdentifier}")]
        public IActionResult WorkflowAbort(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.WorkflowAbort(
                    sessionFormIdentifier)));
        }
        [HttpGet("[action]/{sessionFormIdentifier}")]
        public IActionResult WorkflowRepeat(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.WorkflowRepeat(
                    sessionFormIdentifier, localizer)));
        }
        [HttpPost("[action]")]
        public IActionResult AttachmentCount(
            [FromBody][Required]AttachmentCountInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.AttachmentCount(input)));
        }
        [HttpPost("[action]")]
        public IActionResult AttachmentList(
            [FromBody][Required]AttachmentListInput input)
        {
            return RunWithErrorHandler(() 
                => Ok(sessionObjects.UIService.AttachmentList(input)));
        }
        [HttpPost("[action]")]
        public IActionResult GetRecordTooltip(
            [FromBody]GetRecordTooltipInput input)
        {
            return AmbiguousInputToRowData(input, dataService, sessionObjects)
                .OnSuccess(RowDataToRecordTooltip)
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpPost("[action]")]
        public IActionResult GetAudit([FromBody]GetAuditInput input)
        {
            return AmbiguousInputToEntityId(input, dataService, sessionObjects)
                .OnSuccess(entityId => 
                    GetAuditLog(entityId, input.RowId))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpPost("[action]")]
        public IActionResult SaveFavorites(
            [FromBody][Required]SaveFavoritesInput input)
        {
            return RunWithErrorHandler(() =>
            {
                ServerCoreUIService.SaveFavorites(input);
                return Ok();
            });
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult PendingChanges(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() => Ok(
                sessionObjects.UIService.GetPendingChanges(
                    sessionFormIdentifier)));
        }
        #endregion
        private Dictionary<object, string> GetLookupLabelsInternal(
            LookupLabelsInput input)
        {
            var labelDictionary = input.LabelIds.ToDictionary(
                    id => id,
                    id =>
                    {
                        object lookupResult
                            = lookupService.GetDisplayText(
                                input.LookupId, id, false, true, null);
                        return lookupResult is decimal result
                            ? result.ToString("0.#")
                            : lookupResult.ToString();
                    });
            return labelDictionary;
        }
        private IActionResult CheckLookup(LookupLabelsInput input)
        {
            if (input.MenuId == Guid.Empty)
            {
                SecurityTools.CurrentUserProfile();
            }
            else
            {
                var menuResult = FindItem<FormReferenceMenuItem>(
                    input.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem
                        => CheckLookupIsAllowedInMenu(
                            menuItem, input.LookupId));
                if (menuResult.IsFailure)
                {
                    return menuResult.Error;
                }
            }
            return null;
        }
        private Result<IActionResult, IActionResult> ExecuteDataReader(
            DataStructureQuery dataStructureQuery, GetRowsInput input)
        {
            Result<DataStructureMethod, IActionResult> method 
                = FindItem<DataStructureMethod>(dataStructureQuery.MethodId);
            if (method.IsSuccess)
            {
                var structureMethod = method.Value;
                if (structureMethod is DataStructureWorkflowMethod)
                {
                    var menuItem = FindItem<FormReferenceMenuItem>(input.MenuId)
                        .Value;
                    IEnumerable<object> result2 = LoadData(
                        menuItem,dataStructureQuery).ToList();
                    return Result.Ok<IActionResult, IActionResult>(Ok(result2));
                }
            }
            IEnumerable<object> result = dataService.ExecuteDataReader(
                dataStructureQuery).ToList();
            return Result.Ok<IActionResult, IActionResult>(Ok(result));
        }
        private Result<FormReferenceMenuItem, IActionResult> CheckLookupIsAllowedInMenu(
            FormReferenceMenuItem menuItem, Guid lookupId)
        {
            if(!MenuLookupIndex.HasDataFor(menuItem.Id))
            {
                var xmlOutput = FormXmlBuilder.GetXml(menuItem.Id);
                MenuLookupIndex.AddIfNotPresent(
                    menuItem.Id, xmlOutput.ContainedLookups);
            }
            return MenuLookupIndex.IsAllowed(menuItem.Id, lookupId)
                ? Result.Ok<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Fail<FormReferenceMenuItem, IActionResult>(
                    BadRequest("Lookup is not referenced in any entity in the Menu item"));
        }
        private IEnumerable<object[]> GetRowData(
            LookupListInput input, DataTable dataTable)
        {
            var lookup = FindItem<DataServiceDataLookup>(input.LookupId).Value;
            if(lookup.IsFilteredServerside || string.IsNullOrEmpty(input.SearchText))
            {
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Select(row => GetColumnValues(row, input.ColumnNames));
            }
            var columnNamesWithoutPrimaryKey = FilterOutPrimaryKey(
                input.ColumnNames, dataTable.PrimaryKey);
            return dataTable.Rows
                .Cast<DataRow>()
                .Where(row => Filter(row, columnNamesWithoutPrimaryKey, 
                    input.SearchText))
                .Select(row => GetColumnValues(row, input.ColumnNames));
        }
        private static object[] GetColumnValues(
            DataRow row, IEnumerable<string> columnNames)
        {
            return columnNames.Select(colName => row[colName]).ToArray();
        }
        private static string[] FilterOutPrimaryKey(
            IEnumerable<string> columnNames, DataColumn[] primaryKey)
        {
            return columnNames
                .Where(columnName => primaryKey.All(
                    dataColumn => dataColumn.ColumnName != columnName))
                .ToArray();
        }
        private static bool Filter(
            DataRow row, IEnumerable<string> columnNames, string likeParameter)
        {
            return columnNames
                .Select(colName => row[colName])
                .Any(colValue => 
                    colValue.ToString().Contains(
                        likeParameter, 
                        StringComparison.InvariantCultureIgnoreCase));
        }
        private Result<IEnumerable<object[]>,IActionResult> GetLookupRows(
            LookupListInput input, RowData rowData)
        {
            var internalRequest = new LookupListRequest
            {
                LookupId = input.LookupId,
                FieldName = input.Property,
                CurrentRow = rowData.Row,
                ShowUniqueValues = input.ShowUniqueValues,
                SearchText = "%" + input.SearchText + "%",
                PageSize = input.PageSize,
                PageNumber = input.PageNumber,
                ParameterMappings = DictionaryToHashtable(input.Parameters)
            };
            var dataTable = lookupService.GetList(internalRequest);
            return AreColumnNamesValid(input, dataTable)
                ? Result.Ok<IEnumerable<object[]>, IActionResult>(
                    GetRowData(input, dataTable))
                : Result.Fail<IEnumerable<object[]>, IActionResult>(
                    BadRequest("Some of the supplied column names are not in the table."));
        }
        private Result<RowData, IActionResult> LookupListInputToRowData(
            LookupListInput input)
        {
            if(input.SessionFormIdentifier == Guid.Empty)
            {
                return FindItem<FormReferenceMenuItem>(input.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem => CheckLookupIsAllowedInMenu(
                        menuItem, input.LookupId))
                    .OnSuccess(menuItem => GetEntityData(
                        input.DataStructureEntityId, menuItem))
                    .OnSuccess(CheckEntityBelongsToMenu)
                    .OnSuccess(entityData => GetRow(
                        dataService,
                        entityData.Entity, input.DataStructureEntityId,
                        Guid.Empty, input.Id));
            }
            else
            {
                return sessionObjects.UIService.GetRow(
                    input.SessionFormIdentifier, input.Entity, input.Id);
            }
        }
        private static Hashtable DictionaryToHashtable(IDictionary<string, object> source)
        {
            var result = new Hashtable(source.Count);
            foreach(var(key, value) in source)
            {
                result.Add(key, value);
            }
            return result;
        }
        private static bool AreColumnNamesValid(
            LookupListInput input, DataTable dataTable)
        {
            var actualColumnNames = dataTable.Columns
                .Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();
            return input.ColumnNames
                .All(colName => actualColumnNames.Contains(colName));
        }
        private IActionResult ToActionResult(object obj)
        {
            return Ok(obj);
        }
        private Result<DataStructureQuery, IActionResult> GetRowsGetQuery(
            GetRowsInput input, EntityData entityData)
        {
            var query = new DataStructureQuery
            {
                Entity = entityData.Entity.Name,
                CustomFilters = string.IsNullOrWhiteSpace(input.Filter)
                    ? null
                    : input.Filter,
                CustomOrdering = input.OrderingAsTuples,
                RowLimit = input.RowLimit,
                ColumnsInfo = new ColumnsInfo(input.ColumnNames
                    .Select(colName =>
                    {
                        var field = entityData.Entity.Column(colName).Field;
                        return new ColumnData(
                            name: colName,
                            isVirtual: (field is DetachedField),
                            defaultValue: (field as DetachedField)
                                ?.DefaultValue?.Value,
                            hasRelation: (field as DetachedField)
                                ?.ArrayRelation != null);
                    })
                    .ToList(),
                    renderSqlForDetachedFields: true),
                ForceDatabaseCalculation = true,
            };
            if(entityData.MenuItem.ListDataStructure != null)
            {
                if(entityData.MenuItem.ListEntity.Name 
                    == entityData.Entity.Name)
                {
                    query.MethodId = entityData.MenuItem.ListMethodId;
                    query.DataSourceId 
                        = entityData.MenuItem.ListDataStructure.Id;
                    // get parameters from session store
                    var parameters = sessionObjects.UIService.GetParameters(
                        input.SessionFormIdentifier);
                    foreach (var key in parameters.Keys)
                    {
                        query.Parameters.Add(
                            new QueryParameter(key.ToString(),
                            parameters[key]));
                    }
                }
                else
                {
                    return FindItem<DataStructureMethod>(
                            entityData.MenuItem.MethodId)
                        .OnSuccess(
                            CustomParameterService.GetFirstNonCustomParameter)
                        .OnSuccess(parameterName =>
                        {
                            query.DataSourceId 
                                = entityData.Entity.RootEntity.ParentItemId;
                            query.Parameters.Add(new QueryParameter(
                                parameterName, input.MasterRowId));
                            if (input.MasterRowId == Guid.Empty)
                            {
                                return Result
                                    .Fail<DataStructureQuery, IActionResult>(
                                    BadRequest("MasterRowId cannot be empty"));
                            }
                            query.MethodId = entityData.MenuItem.MethodId;
                            return Result
                                .Ok<DataStructureQuery, IActionResult>(query);
                        });
                }
            }
            else
            {
                query.MethodId = entityData.MenuItem.MethodId;
                query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
            }
            return Result.Ok<DataStructureQuery, IActionResult>(query);
        }
        private static void FillRow(
            RowData rowData, Dictionary<string, string> newValues)
        {
            foreach(var(key, value) in newValues)
            {
                var dataType = rowData.Row.Table.Columns[key].DataType;
                rowData.Row[key] = DatasetTools.ConvertValue(value, dataType);
            }
        }
        private RowData MakeEmptyRow(DataStructureEntity entity)
        {
            var dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            var table = dataSet.Tables[entity.Name];
            var row = table.NewRow();
            return new RowData { Entity = entity, Row = row };
        }
        private static RowData FillRow(NewRowInput input, RowData rowData)
        {
            DatasetTools.ApplyPrimaryKey(rowData.Row);
            DatasetTools.UpdateOrigamSystemColumns(
                rowData.Row, true, SecurityManager.CurrentUserProfile().Id);
            FillRow(rowData, input.NewValues);
            rowData.Row.Table.Rows.Add(rowData.Row);
            return rowData;
        }
        private static RowData PrepareNewRow(RowData rowData)
        {
            DatasetTools.ApplyPrimaryKey(rowData.Row);
            DatasetTools.UpdateOrigamSystemColumns(
                rowData.Row, true, SecurityManager.CurrentUserProfile().Id);
            rowData.Row.Table.NewRow();
            return rowData;
        }
        private IActionResult SubmitDelete(RowData rowData)
        {
            try
            {
                DataService.StoreData(
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
            return Ok(SessionStore.GetDeleteInfo(
                                requestingGrid: null,
                                tableName: rowData.Row.Table.TableName,
                                objectId: null));
        }
        private IActionResult ThrowAwayReturnData(IActionResult arg)
        {
            return Ok();
        }
        private IActionResult RowDataToRecordTooltip(RowData rowData)
        {
            var requestCultureFeature = Request.HttpContext.Features
                .Get<IRequestCultureFeature>();
            var cultureInfo = requestCultureFeature.RequestCulture.Culture;
            return Ok(ServerCoreUIService.DataRowToRecordTooltip(
                rowData.Row, 
                cultureInfo,
                localizer));
        }
        private IActionResult GetAuditLog(Guid entityId, object id)
        {
            var auditLog = AuditLogDA.RetrieveLogTransformed(
                entityId, id);
            if(log != null)
            {
                return Ok(DataTools.DatatableToHashtable(
                    auditLog.Tables[0], false));
            }
            return Ok();
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
            foreach (DataRow dataRow in table.Rows)
            {
                var values = new object[query.ColumnsInfo.Count];
                for (var i = 0; i < query.ColumnsInfo.Count; i++)
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
    }
}
