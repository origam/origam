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
using Origam.Security.Identity;
using Origam.Server;
using Origam.ServerCore.Extensions;
using Origam.ServerCore.Model.UIService;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Origam.ServerCore.Controller
{
    [Authorize]
    [ApiController]
    [Route("internalApi/[controller]")]
    public class UIServiceController : AbstractController
    {
        class EntityData
        {
            public FormReferenceMenuItem MenuItem { get; set; }
            public DataStructureEntity Entity { get; set; }
        }
        private readonly SessionObjects sessionObjects;
        private readonly IStringLocalizer<SharedResources> localizer;
        private readonly IDataLookupService lookupService;
        private readonly IDataService dataService;

        public UIServiceController(
            SessionObjects sessionObjects, 
            IServiceProvider serviceProvider,
            IStringLocalizer<SharedResources> localizer,
            ILogger<AbstractController> log) : base(log)
        {
            this.sessionObjects = sessionObjects;
            IdentityServiceAgent.ServiceProvider = serviceProvider;
            this.localizer = localizer;
            lookupService 
                = ServiceManager.Services.GetService<IDataLookupService>();
            dataService = DataService.GetDataService();
        }

        [HttpGet("[action]")]
        public IActionResult InitPortal([FromQuery][Required]string locale)
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
            return RunWithErrorHandler(() =>
            {
                //TODO: findout how to get request size limit
                return Ok(sessionObjects.UIService.InitPortal(4));
            });
        }

        [HttpPost("[action]")]
        public IActionResult InitUI([FromBody]UIRequest request)
        {
            // registerSession is important for sessionless handling
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIManager.InitUI(
                    request: request,
                    addChildSession: false,
                    parentSession: null,
                    basicUIService: sessionObjects.UIService));
            });
        }

        [HttpGet("[action]")]
        public IActionResult DestroyUI(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                sessionObjects.UIService.DestroyUI(sessionFormIdentifier);
                return Ok();
            });
        }

        [HttpGet("[action]")]
        public IActionResult RefreshData(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.RefreshData(
                    sessionFormIdentifier, localizer));
            });
        }

        [HttpGet("[action]")]
        public IActionResult SaveDataQuery(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.SaveDataQuery(
                    sessionFormIdentifier));
            });
        }

        [HttpGet("[action]")]
        public IActionResult SaveData(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.SaveData(
                    sessionFormIdentifier));
            });
        }

        [HttpPost("[action]")]
        public IActionResult MasterRecord([FromBody]MasterRecordInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.GetRowData(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult GetData([FromBody]GetDataInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.GetData(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult CreateObject(
            [FromBody][Required]CreateObjectInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.CreateObject(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult UpdateObject(
            [FromBody][Required]UpdateObjectInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.UpdateObject(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult DeleteObject(
            [FromBody][Required]DeleteObjectInput input)
        {
            return RunWithErrorHandler(() =>
            {
                //todo: handle deleting non existing objects
                return Ok(sessionObjects.UIService.DeleteObject(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult ExecuteActionQuery(
            [FromBody][Required]ExecuteActionQueryInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.ExecuteActionQuery(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult ExecuteAction(
            [FromBody][Required]ExecuteActionInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.ExecuteAction(input));
            });
        }

        [HttpPost("[action]")]
        public IActionResult GetLookupLabels([FromBody]LookupLabelsInput input)
        {
            return RunWithErrorHandler(() =>
            {
                //todo: what about workflows?
                var menuResult = FindItem<FormReferenceMenuItem>(
                    input.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem 
                        => CheckLookupIsAllowedInMenu(
                            menuItem, input.LookupId));
                if(menuResult.IsFailure)
                {
                    return menuResult.Error;
                }
                Dictionary<Guid, string> labelDictionary 
                    = input.LabelIds.ToDictionary(
                        id => id,
                        id =>
                        {
                            object lookupResult 
                                = lookupService.GetDisplayText(
                                    input.LookupId, id, false, true, null);
                            return lookupResult is decimal
                                ? ((decimal) lookupResult).ToString("0.#")
                                : lookupResult.ToString();
                        });
                return Ok(labelDictionary);
            });
        }

        [HttpPost("[action]")]
        public IActionResult GetLookupList([FromBody]LookupListInput input)
        {
            //todo: implement GetFilterLookupList
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
                        entityData.Entity, input.DataStructureEntityId, input.Id))
                    .OnSuccess(rowData => GetLookupRows(input, rowData))
                    .OnSuccess(ToActionResult)
                    .OnBoth<IActionResult,IActionResult>(UnwrapReturnValue);
            }
            else
            {
                return sessionObjects.UIService.GetRow(
                        input.SessionFormIdentifier, input.Entity, input.Id)
                    .OnSuccess(rowData => GetLookupRows(input, rowData))
                    .OnSuccess(ToActionResult)
                    .OnBoth<IActionResult,IActionResult>(UnwrapReturnValue);
            }
        }

        [HttpPost("[action]")]
        public IActionResult GetRows([FromBody]GetRowsInput input)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(
                    input.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => GetRowsGetQuery(input, entityData))
                .OnSuccess(dataService.ExecuteDataReader)
                .OnSuccess(ToActionResult)
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
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
                        entityData.Entity, 
                        input.DataStructureEntityId,
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
                .OnSuccess(rowData => PrepareNewRow(rowData))
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
                        entityData.Entity,
                        input.DataStructureEntityId,
                        input.RowIdToDelete))
                    .OnSuccess(rowData =>
                    {
                        rowData.Row.Delete();
                        return SubmitDelete(rowData);
                    })
                    .OnSuccess(ThrowAwayReturnData)
                    .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }

        [HttpGet("[action]")]
        public IActionResult WorkflowNextQuery(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.WorkflowNextQuery(
                    sessionFormIdentifier));
            });
        }

        [HttpPost("[action]")]
        public IActionResult WorkflowNext(
            [FromBody][Required]WorkflowNextInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.WorkflowNext(input));
            });
        }

        [HttpGet("[action]")]
        public IActionResult WorkflowAbort(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.WorkflowAbort(
                    sessionFormIdentifier));
            });
        }

        [HttpGet("[action]")]
        public IActionResult WorkflowRepeat(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.WorkflowRepeat(
                    sessionFormIdentifier, localizer));
            });
        }
        private IActionResult RunWithErrorHandler(Func<IActionResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        private Result<T,IActionResult> FindItem<T>(Guid id) where T : class
        {
            T instance = ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T), new Key(id)) 
                as T;
            return instance == null
                ? Result.Fail<T, IActionResult>(
                    NotFound("Object with requested id not found."))
                : Result.Ok<T, IActionResult>(instance);
        }
        private Result<FormReferenceMenuItem, IActionResult> Authorize(
            FormReferenceMenuItem menuItem)
        {
            return SecurityManager.GetAuthorizationProvider().Authorize(
                User, menuItem.Roles)
                ? Result.Ok<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Fail<FormReferenceMenuItem, IActionResult>(Forbid());
        }
        private Result<FormReferenceMenuItem, IActionResult> CheckLookupIsAllowedInMenu(
            FormReferenceMenuItem menuItem, Guid lookupId)
        {
            if(!MenuLookupIndex.HasDataFor(menuItem.Id))
            {
                XmlOutput xmlOutput = FormXmlBuilder.GetXml(menuItem.Id);
                MenuLookupIndex.AddIfNotPresent(
                    menuItem.Id, xmlOutput.ContainedLookups);
            }
            return MenuLookupIndex.IsAllowed(menuItem.Id, lookupId)
                ? Result.Ok<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Fail<FormReferenceMenuItem, IActionResult>(
                    BadRequest("Lookup is not referenced in any entity in the Menu item"));
        }
        private Result<EntityData, IActionResult> CheckEntityBelongsToMenu(
            EntityData entityData)
        {
            return (entityData.MenuItem.Screen.DataStructure.Id 
                == entityData.Entity.RootEntity.ParentItemId)
                ? Result.Ok<EntityData, IActionResult>(entityData)
                : Result.Fail<EntityData, IActionResult>(
                    BadRequest("The requested Entity does not belong to the menu."));
        }
        private Result<RowData, IActionResult> GetRow(
            DataStructureEntity entity, Guid dataStructureEntityId, Guid rowId)
        {
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = dataStructureEntityId,
                Entity = entity.Name,
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter("Id", rowId));
            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);
            DataTable dataSetTable = dataSet.Tables[entity.Name];
            if(dataSetTable.Rows.Count == 0)
            {
                return Result.Fail<RowData, IActionResult>(
                    NotFound("Requested data row was not found."));
            }
            return Result.Ok<RowData, IActionResult>(
                new RowData{Row =dataSetTable.Rows[0], Entity = entity});
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
            else
            {
                string[] columnNamesWithoutPrimaryKey = FilterOutPrimaryKey(
                    input.ColumnNames, dataTable.PrimaryKey);
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Where(row => Filter(row, columnNamesWithoutPrimaryKey, 
                        input.SearchText))
                    .Select(row => GetColumnValues(row, input.ColumnNames));
            }
        }
        private static object[] GetColumnValues(
            DataRow row, string[] columnNames)
        {
            return columnNames.Select(colName => row[colName]).ToArray();
        }
        private static string[] FilterOutPrimaryKey(
            string[] columnNames, DataColumn[] primaryKey)
        {
            return columnNames
                .Where(
                    columnName => !primaryKey.Any(
                        dataColumn => dataColumn.ColumnName == columnName))
                .ToArray();
        }
        private static bool Filter(
            DataRow row, string[] columnNames, string likeParameter)
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
            LookupListRequest internalRequest = new LookupListRequest();
            internalRequest.LookupId = input.LookupId;
            internalRequest.FieldName = input.Property;
            internalRequest.CurrentRow = rowData.Row;
            internalRequest.ShowUniqueValues = input.ShowUniqueValues;
            internalRequest.SearchText = "%" + input.SearchText + "%";
            internalRequest.PageSize = input.PageSize;
            internalRequest.PageNumber = input.PageNumber;
            DataTable dataTable = lookupService.GetList(internalRequest);
            return AreColumnNamesValid(input, dataTable)
                ? Result.Ok<IEnumerable<object[]>, IActionResult>(
                    GetRowData(input, dataTable))
                : Result.Fail<IEnumerable<object[]>, IActionResult>(
                    BadRequest("Some of the supplied column names are not in the table."));
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

        public IActionResult UnwrapReturnValue(
            Result<IActionResult, IActionResult> result)
        {
            return result.IsSuccess ? result.Value : result.Error;
        }
        private Result<DataStructureQuery, IActionResult> GetRowsGetQuery(
            GetRowsInput input, EntityData entityData)
        {
            DataStructureQuery query = new DataStructureQuery
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
                            isVirtual: field is DetachedField,
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
                }
                else
                {
                    return FindItem<DataStructureMethod>(entityData.MenuItem.MethodId)
                        .OnSuccess(CustomParameterService.GetFirstNonCustomParameter)
                        .OnSuccess(parameterName =>
                        {
                            query.DataSourceId 
                                = entityData.Entity.RootEntity.ParentItemId;
                            query.Parameters.Add(
                                new QueryParameter(parameterName, input.MasterRowId));
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
        private Result<EntityData, IActionResult> GetEntityData(
            Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
        {
            return FindEntity(dataStructureEntityId)
                .OnSuccess(entity 
                    => new EntityData {MenuItem = menuItem, Entity = entity});
        }
        private Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
        {
            return FindItem<DataStructureEntity>(id)
                .OnFailureCompensate(error =>
                    Result.Fail<DataStructureEntity, IActionResult>(
                        NotFound("Requested DataStructureEntity not found. " 
                        + error.GetMessage())));
        }
        private static void FillRow(
            RowData rowData, Dictionary<string, string> newValues)
        {
            foreach (var colNameValuePair in newValues)
            {
                Type dataType 
                    = rowData.Row.Table.Columns[colNameValuePair.Key].DataType;
                rowData.Row[colNameValuePair.Key] =
                    DatasetTools.ConvertValue(colNameValuePair.Value, dataType);
            }
        }
        private IActionResult SubmitChange(RowData rowData, Operation operation)
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
            return Ok(SessionStore.GetChangeInfo(
                requestingGrid: null, 
                row: rowData.Row, 
                operation: operation, 
                RowStateProcessor: null));
        }
        private RowData MakeEmptyRow(DataStructureEntity entity)
        {
            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            DataTable table = dataSet.Tables[entity.Name];
            DataRow row = table.NewRow();
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
    }
}
