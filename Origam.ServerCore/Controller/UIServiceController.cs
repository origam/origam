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
using Origam.Security.Identity;
using Origam.Server;
using Origam.ServerCore.Extensions;
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

namespace Origam.ServerCore.Controller
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
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
        internal object _lock = new object();

        #region Endpoints
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
        [HttpGet("[action]/{locale}")]
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
            return RunWithErrorHandler(() =>
            {
                //TODO: findout how to get request size limit
                return Ok(sessionObjects.UIService.InitPortal(4));
            });
        }
        [HttpPost("[action]")]
        public async System.Threading.Tasks.Task<IActionResult> InitUI([FromBody]UIRequest request)
        {
            return await RunWithErrorHandlerAsync(async () =>
            {
                return Ok(
                    // registerSession is important for sessionless handling

                    await sessionObjects.UIManager.InitUIAsync(
                        request: request,
                        addChildSession: false,
                        parentSession: null,
                        basicUIService: sessionObjects.UIService)
                );
            });
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
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.RefreshData(
                    sessionFormIdentifier, localizer));
            });
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult SaveDataQuery(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.SaveDataQuery(
                    sessionFormIdentifier));
            });
        }
        [HttpGet("[action]/{sessionFormIdentifier:guid}")]
        public IActionResult SaveData(Guid sessionFormIdentifier)
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
        public IActionResult RowStates([FromBody]RowStatesInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.RowStates(input));
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
                Ok(sessionObjects.UIService.WorkQueueList(localizer)));
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
                sessionObjects.UIService.SaveSplitPanelConfig(input);
                return Ok();
            });
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

        [HttpPost("[action]")]
        public IActionResult GetLookupLabelsEx([FromBody]LookupLabelsInput[] inputs)
        {
            return RunWithErrorHandler(() =>
            {
                Dictionary<Guid, Dictionary<Guid, string>> result
                    = new Dictionary<Guid, Dictionary<Guid, string>>();
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

        private Dictionary<Guid, string> GetLookupLabelsInternal(LookupLabelsInput input)
        {
            Dictionary<Guid, string> labelDictionary
                = input.LabelIds.ToDictionary(
                    id => id,
                    id =>
                    {
                        object lookupResult
                            = lookupService.GetDisplayText(
                                input.LookupId, id, false, true, null);
                        return lookupResult is decimal
                            ? ((decimal)lookupResult).ToString("0.#")
                            : lookupResult.ToString();
                    });
            return labelDictionary;
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
                        entityData.Entity, input.DataStructureEntityId,
                        Guid.Empty, input.Id))
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
            return RunWithErrorHandler(() =>
            {
                return FindItem<FormReferenceMenuItem>(input.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem => GetEntityData(
                        input.DataStructureEntityId, menuItem))
                    .OnSuccess(CheckEntityBelongsToMenu)
                    .OnSuccess(entityData => GetRowsGetQuery(input, entityData))
                    .OnSuccess(datastructureQuery=>ExecuteDataReader(datastructureQuery, input))
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
        [HttpGet("[action]/{sessionFormIdentifier}")]
        public IActionResult WorkflowAbort(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.WorkflowAbort(
                    sessionFormIdentifier));
            });
        }
        [HttpGet("[action]/{sessionFormIdentifier}")]
        public IActionResult WorkflowRepeat(Guid sessionFormIdentifier)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.WorkflowRepeat(
                    sessionFormIdentifier, localizer));
            });
        }
        [HttpPost("[action]")]
        public IActionResult AttachmentCount(
            [FromBody][Required]AttachmentCountInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.AttachmentCount(input));
            });
        }
        [HttpPost("[action]")]
        public IActionResult AttachmentList(
            [FromBody][Required]AttachmentListInput input)
        {
            return RunWithErrorHandler(() =>
            {
                return Ok(sessionObjects.UIService.AttachmentList(input));
            });
        }
        [HttpPost("[action]")]
        public IActionResult GetRecordTooltip(
            [FromBody]GetRecordTooltipInput input)
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
                        Guid.Empty,
                        input.RowId))
                .OnSuccess(RowDataToRecordTooltip)
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        [HttpPost("[action]")]
        public IActionResult GetAudit([FromBody]GetAuditInput input)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(
                    input.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => 
                    GetAuditLog(
                        entityData, 
                        input.RowId))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }
        #endregion
        private async System.Threading.Tasks.Task<IActionResult> RunWithErrorHandlerAsync(
            Func<System.Threading.Tasks.Task<IActionResult>> func)
        {
            try
            {
                return await func();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return NotFound(ex.ActualValue);
            }
            catch (SessionExpiredException ex)
            {
                return NotFound(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }


        private IActionResult RunWithErrorHandler(Func<IActionResult> func)
        {
            try
            {
                return func();
            }
            catch(ArgumentOutOfRangeException ex)
            {
                return NotFound(ex.ActualValue);
            }
            catch (SessionExpiredException ex)
            {
                return NotFound(ex);
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
        private Result<IActionResult, IActionResult> ExecuteDataReader(
            DataStructureQuery dataStructureQuery, GetRowsInput input)
        {
            Result<DataStructureMethod, IActionResult> method = FindItem<DataStructureMethod>(dataStructureQuery.MethodId);
            if (method.IsSuccess)
            {
                DataStructureMethod structureMethod = method.Value;
                if (structureMethod is DataStructureWorkflowMethod)
                {
                    FormReferenceMenuItem menuItem = FindItem<FormReferenceMenuItem>(input.MenuId).Value;
                     IEnumerable<object> result2 = LoadData(menuItem,dataStructureQuery).ToList();
                    return Result.Ok<IActionResult, IActionResult>(
                        Ok(result2));
                }
            }
            IEnumerable<object> result = dataService.ExecuteDataReader(dataStructureQuery).ToList();
            return Result.Ok<IActionResult, IActionResult>(
                Ok(result));
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
            DataStructureEntity entity, Guid dataStructureEntityId,
            Guid methodId, Guid rowId)
        {
            var rows = SessionStore.LoadRows(dataService, entity, 
                dataStructureEntityId, methodId, new List<Guid> { rowId });
            if(rows.Count == 0)
            {
                return Result.Fail<RowData, IActionResult>(
                    NotFound("Requested data row was not found."));
            }
            return Result.Ok<RowData, IActionResult>(
                new RowData{Row = rows[0], Entity = entity});
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
            internalRequest.ParameterMappings = DictionaryToHashtable(input.Parameters);
            DataTable dataTable = lookupService.GetList(internalRequest);
            return AreColumnNamesValid(input, dataTable)
                ? Result.Ok<IEnumerable<object[]>, IActionResult>(
                    GetRowData(input, dataTable))
                : Result.Fail<IEnumerable<object[]>, IActionResult>(
                    BadRequest("Some of the supplied column names are not in the table."));
        }

        private static Hashtable DictionaryToHashtable(IDictionary<string, object> source)
        {
            Hashtable result = new Hashtable(source.Count);
            foreach (KeyValuePair<string, object> kvp in source)
            {
                result.Add(kvp.Key, kvp.Value);
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

        private IActionResult UnwrapReturnValue(
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
        private IActionResult RowDataToRecordTooltip(RowData rowData)
        {
            var requestCultureFeature = Request.HttpContext.Features
                .Get<IRequestCultureFeature>();
            var cultureInfo = requestCultureFeature.RequestCulture.Culture;
            return Ok(sessionObjects.UIService.DataRowToRecordTooltip(
                rowData.Row, 
                cultureInfo,
                localizer));
        }
        private IActionResult GetAuditLog(EntityData entityData, object id)
        {
            var auditLog = AuditLogDA.RetrieveLogTransformed(
                entityData.Entity.EntityId, id);
            if(log != null)
            {
                return Ok(DataTools.DatatableToHashtable(
                    auditLog.Tables[0], false));
            }
            return Ok();
        }
        private IEnumerable<object> LoadData(FormReferenceMenuItem menuItem, DataStructureQuery dataStructureQuery)
        {
            DataSet data = InitializeFullStructure(menuItem);
            DataSet ListData = InitializeListStructure(data,menuItem.ListEntity.Name);
            return  TransformData(LoadListData(ListData, menuItem.ListEntity.Name, menuItem.ListSortSet,menuItem), dataStructureQuery);
        }

        private IEnumerable<object> TransformData(DataSet dataSet, DataStructureQuery query)
        {
            DataTable table = dataSet.Tables[0];
                foreach (DataRow dataRow in table.Rows)
                {
                    object[] values = new object[query.ColumnsInfo.Count];
                    for (int i = 0, index = 0; i < query.ColumnsInfo.Count; i++)
                    {
                        values[i] = dataRow.Field<object>(query.ColumnsInfo.Columns[i].Name);
                        index++;
                    }
                    yield return ProcessReaderOutput(
                        values, query.ColumnsInfo);
                }
            
        }
        private List<object> ProcessReaderOutput(object[] values, ColumnsInfo columnsInfo)
        {
            if (columnsInfo == null)
                throw new ArgumentNullException(nameof(columnsInfo));
            var updatedValues = new List<object>();
            for (int i = 0; i < columnsInfo.Count; i++)
            {
                updatedValues.Add(values[i]);
            }

            return updatedValues;
        }
        private DataSet LoadListData(DataSet data, string listEntity, DataStructureSortSet sortSet, FormReferenceMenuItem _menuItem)
        {
            List<string> DataListLoadedColumns = new List<string>();
            string initialColumns = "";
            initialColumns = ListPrimaryKeyColumns(data, listEntity);
            if (sortSet != null)
            {
                foreach (DataStructureSortSetItem sortItem in
                    sortSet.ChildItemsByType(DataStructureSortSetItem.ItemTypeConst))
                {
                    if (sortItem.Entity.Name == listEntity)
                    {
                        initialColumns += ";" + sortItem.FieldName;
                       
                    }
                }
            }
            // load list entity
            DataSet result = DataService.LoadData(_menuItem.ListDataStructureId, _menuItem.ListMethodId,
                Guid.Empty, _menuItem.ListSortSetId, null, null, data, listEntity, initialColumns);
            // load all array field child entities - there is no way how to read
            // only children of a specific record (inside LazyLoadListRowData) so
            // we preload all array fields here
            ArrayList arrayColumns = new ArrayList();
            foreach (DataColumn col in result.Tables[listEntity].Columns)
            {
                if (IsColumnArray(col))
                {
                    arrayColumns.Add(col.ColumnName);
                }
            }
            LoadArrayColumns(result, listEntity, null, arrayColumns, DataListLoadedColumns,_menuItem);
            return result;
        }
        private static bool IsColumnArray(DataColumn dataColumn)
        {
            if (dataColumn.ExtendedProperties.Contains(Const.OrigamDataType))
            {
                return ((Schema.OrigamDataType)dataColumn.ExtendedProperties[Const.OrigamDataType]) == Schema.OrigamDataType.Array;
            }
            else
            {
                return false;
            }
        }
        private void LoadArrayColumns(DataSet dataset, string entity,
            QueryParameterCollection qparams, ArrayList arrayColumns,List<string> DataListLoadedColumns, FormReferenceMenuItem _menuItem)
        {
            lock (_lock)
            {
                foreach (string column in arrayColumns)
                {
                    if (!DataListLoadedColumns.Contains(column))
                    {
                        DataColumn col = dataset.Tables[entity].Columns[column];
                        string relationName = (string)col.ExtendedProperties[Const.ArrayRelation];
                        DataService.LoadData(_menuItem.ListDataStructureId, _menuItem.ListMethodId,
                            Guid.Empty, _menuItem.ListSortSetId, null, qparams, dataset, relationName,
                            null);
                        DataListLoadedColumns.Add(column);
                    }
                }
            }
        }
        private static string ListPrimaryKeyColumns(DataSet data, string listEntity)
        {
            string initialColumns = "";
            foreach (var pkCol in data.Tables[listEntity].PrimaryKey)
            {
                if (initialColumns != "")
                {
                    initialColumns += ";";
                }
                initialColumns += pkCol.ColumnName;
            }
            return initialColumns;
        }
        private DataSet InitializeFullStructure(FormReferenceMenuItem menuItem)
        {
            return new DatasetGenerator(true).CreateDataSet(DataStructure(menuItem.ListDataStructureId), true, menuItem.DefaultSet);
        }
        private DataStructure DataStructure(Guid id)
        {
            return FindItem<DataStructure>(id).Value;
        }
        private static DataSet InitializeListStructure(DataSet data, string listEntity)
        {
            DataSet listData = DatasetTools.CloneDataSet(data);
            DataTable listTable = listData.Tables[listEntity];
            // turn off constraints checking because we will be loading only partial
            // data to the list
            listData.EnforceConstraints = false;
            // remove all computed columns - list will calculate values from the database,
            // by having aggregation expressions the values would be recalculated to zeros
            // becuase we would not have any child records
            // row-level computed columns cause performance problems with lots of data
            foreach (DataColumn col in listTable.Columns)
            {
                col.Expression = "";
                col.ReadOnly = false;
                // default values would distract when debugging a partly loaded list
                col.DefaultValue = null;
            }
            // we add a column that will identify if the record has been loaded from
            // the database
            //DataColumn loadedFlagColumn =
            //    listTable.Columns.Add("___ORIGAM_IsLoaded", typeof(bool));
            //loadedFlagColumn.AllowDBNull = false;
            //loadedFlagColumn.DefaultValue = false;
            // we cannot delete non-list tables because child entities will be 
            // neccessary for array-type fields (TagInput etc.)
            return listData;
        }
    }
}
