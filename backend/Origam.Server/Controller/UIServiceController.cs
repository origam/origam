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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Server.Common;
using Origam.Server.Configuration;
using Origam.Server.Model.Search;
using Origam.Server.Model.UIService;
using Origam.Service.Core;
using Origam.Services;
using Origam.Workbench;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Controller;

[Authorize(Policy = "InternalApi")]
[ApiController]
[Route(template: "internalApi/[controller]")]
public class UIServiceController : AbstractController
{
    private readonly IStringLocalizer<SharedResources> localizer;
    private readonly ClientFilteringConfig clientFilteringConfig;
    private readonly IDataLookupService lookupService;
    private readonly RequestLocalizationOptions localizationOptions;
    private readonly CustomAssetsConfig customAssetsConfig;
    private readonly HtmlClientConfig htmlClientConfig;
    private readonly ChatConfig chatConfig;

    public UIServiceController(
        SessionObjects sessionObjects,
        IStringLocalizer<SharedResources> localizer,
        ILogger<AbstractController> log,
        IOptions<RequestLocalizationOptions> localizationOptions,
        IOptions<CustomAssetsConfig> customAssetsOptions,
        IOptions<ClientFilteringConfig> filteringConfig,
        IOptions<HtmlClientConfig> htmlClientConfigOptions,
        IOptions<ChatConfig> chatConfigOptions,
        IWebHostEnvironment environment
    )
        : base(log: log, sessionObjects: sessionObjects, environment: environment)
    {
        this.localizer = localizer;
        this.clientFilteringConfig = filteringConfig.Value;
        this.localizationOptions = localizationOptions.Value;
        customAssetsConfig = customAssetsOptions.Value;
        htmlClientConfig = htmlClientConfigOptions.Value;
        lookupService = ServiceManager.Services.GetService<IDataLookupService>();
        chatConfig = chatConfigOptions.Value;
    }

    #region Endpoints
    [HttpGet(template: "[action]")]
    // ReSharper disable once UnusedParameter.Global
    public IActionResult InitPortal()
    {
        Analytics.Instance.Log(message: "UI_INIT");
        PortalResult result = sessionObjects.UIService.InitPortal(maxRequestLength: 4);
        AddConfigData(result: result);
        return Ok(value: result);
    }

    [AllowAnonymous]
    [HttpGet(template: "[action]")]
    public IActionResult DefaultLocalizationCookie()
    {
        var cultureProvider = localizationOptions
            .RequestCultureProviders.OfType<OrigamCookieRequestCultureProvider>()
            .First();
        string localizationCookie = cultureProvider.MakeCookieValue(
            requestCulture: localizationOptions.DefaultRequestCulture
        );
        return Ok(value: localizationCookie);
    }

    [HttpPost(template: "[action]")]
    public IActionResult InitUI([FromBody] UIRequest request)
    {
        return Ok(
            value: sessionObjects.UIManager.InitUI(
                request: request,
                addChildSession: false,
                parentSession: null,
                basicUIService: sessionObjects.UIService
            )
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult DestroyUI([FromBody] DestroyInput input)
    {
        sessionObjects.UIService.DestroyUI(sessionFormIdentifier: input.FormSessionId);
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult DestroyManyUI([FromBody] DestroyManyInput input)
    {
        sessionObjects.UIService.DestroyUI(
            sessionFormIdentifiers: input.FormSessionIds ?? Array.Empty<Guid>()
        );
        return Ok();
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier:guid}")]
    public IActionResult RefreshData(Guid sessionFormIdentifier)
    {
        return Ok(
            value: sessionObjects.UIService.RefreshData(
                sessionFormIdentifier: sessionFormIdentifier,
                localizer: localizer
            )
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult RestoreData([FromBody] RestoreDataInput input)
    {
        return Ok(value: sessionObjects.UIService.RestoreData(input: input));
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier:guid}")]
    public IActionResult SaveDataQuery(Guid sessionFormIdentifier)
    {
        return Ok(
            value: sessionObjects.UIService.SaveDataQuery(
                sessionFormIdentifier: sessionFormIdentifier
            )
        );
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier:guid}")]
    public IActionResult SaveData(Guid sessionFormIdentifier)
    {
        var ruleExceptionData = sessionObjects.UIService.SaveDataQuery(
            sessionFormIdentifier: sessionFormIdentifier
        );
        var errors = ruleExceptionData
            .Cast<RuleExceptionData>()
            .Where(predicate: data => data.Severity == RuleExceptionSeverity.High)
            .Select(selector: data =>
                $"Entity: {data.EntityName}, Field: {data.FieldName}, Message: {data.Message}, Severity: {data.Severity}"
            )
            .ToList();
        if (errors.Count > 0)
        {
            return StatusCode(statusCode: 409, value: string.Join(separator: "\n", values: errors));
        }
        return Ok(
            value: sessionObjects.UIService.SaveData(sessionFormIdentifier: sessionFormIdentifier)
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult RevertChanges([FromBody] RevertChangesInput input)
    {
        sessionObjects.UIService.RevertChanges(input: input);
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult MasterRecord([FromBody] MasterRecordInput input)
    {
        return Ok(value: sessionObjects.UIService.GetRowData(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetData([FromBody] GetDataInput input)
    {
        return Ok(value: sessionObjects.UIService.GetData(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult RowStates([FromBody] RowStatesInput input)
    {
        return Ok(value: sessionObjects.UIService.RowStates(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult CreateObject([FromBody] [Required] CreateObjectInput input)
    {
        return Ok(value: sessionObjects.UIService.CreateObject(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult CopyObject([FromBody] [Required] CopyObjectInput input)
    {
        return Ok(value: sessionObjects.UIService.CopyObject(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult UpdateObject([FromBody] [Required] UpdateObjectInput input)
    {
        return Ok(value: sessionObjects.UIService.UpdateObject(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult DeleteObject([FromBody] [Required] DeleteObjectInput input)
    {
        //todo: handle deleting non existing objects
        return Ok(value: sessionObjects.UIService.DeleteObject(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult DeleteObjectInOrderedList(
        [FromBody] [Required] DeleteObjectInOrderedListInput input
    )
    {
        //todo: handle deleting non existing objects
        return Ok(value: sessionObjects.UIService.DeleteObjectInOrderedList(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult ExecuteActionQuery([FromBody] [Required] ExecuteActionQueryInput input)
    {
        return Ok(value: sessionObjects.UIService.ExecuteActionQuery(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult ExecuteAction([FromBody] [Required] ExecuteActionInput input)
    {
        return Ok(value: sessionObjects.UIService.ExecuteAction(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetLookupLabels([FromBody] [Required] LookupLabelsInput input)
    {
        // todo: unify approach
        var checkResult = CheckLookup(input: input);
        if (checkResult != null)
        {
            return checkResult;
        }
        var labelDictionary = GetLookupLabelsInternal(input: input);
        return Ok(value: labelDictionary);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetLookupCacheDependencies(
        [FromBody] [Required] GetLookupCacheDependenciesInput input
    )
    {
        var dependencies = new Dictionary<string, object>(capacity: input.LookupIds.Length);
        foreach (var lookupId in input.LookupIds)
        {
            dependencies.Add(
                key: (string)lookupId,
                value: GetLookupCacheDependencies(lookupId: (string)lookupId)
            );
        }
        return Ok(value: dependencies);
    }

    [HttpGet(template: "[action]")]
    public IActionResult WorkQueueList()
    {
        return Ok(value: ServerCoreUIService.WorkQueueList(localizer: localizer));
    }

    [HttpPost(template: "[action]")]
    public IActionResult ResetScreenColumnConfiguration(
        [FromBody] [Required] ResetScreenColumnConfigurationInput input
    )
    {
        sessionObjects.UIService.ResetScreenColumnConfiguration(input: input);
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult SaveObjectConfig([FromBody] [Required] SaveObjectConfigInput input)
    {
        sessionObjects.UIService.SaveObjectConfig(input: input);
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult SaveSplitPanelConfig([FromBody] [Required] SaveSplitPanelConfigInput input)
    {
        ServerCoreUIService.SaveSplitPanelConfig(input: input);
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetLookupLabelsEx([FromBody] LookupLabelsInput[] inputs)
    {
        var result = new Dictionary<Guid, Dictionary<object, string>>();
        foreach (var input in inputs)
        {
            var checkResult = CheckLookup(input: input);
            if (checkResult != null)
            {
                return checkResult;
            }
            var labelDictionary = GetLookupLabelsInternal(input: input);
            result.Add(key: input.LookupId, value: labelDictionary);
        }
        return Ok(value: result);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetLookupList([FromBody] LookupListInput input)
    {
        //todo: implement GetFilterLookupList
        return LookupInputToRowData(input: input)
            .Bind(func: rowData => GetLookupRows(input: input, rowData: rowData))
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetLookupNewRecordInitialValues(
        [FromBody] LookupNewRecordInitialValuesInput input
    )
    {
        return LookupInputToRowData(input: input)
            .Bind(func: rowData => RowDataToNewRecordInitialValues(input: input, rowData: rowData))
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetRows([FromBody] GetRowsInput input)
    {
        var sessionStore = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        if (sessionStore is WorkQueueSessionStore workQueueSessionStore)
        {
            return WorkQueueGetRowsGetRowsQuery(input: input, sessionStore: workQueueSessionStore)
                .Bind(func: dataStructureQuery =>
                    ExecuteDataReader(
                        dataStructureQuery: dataStructureQuery,
                        methodId: input.MenuId
                    )
                )
                .Map(func: ToActionResult)
                .Finally(func: UnwrapReturnValue);
        }
        return EntityIdentificationToEntityData(input: input)
            .Bind(func: entityData => GetRowsGetQuery(input: input, entityData: entityData))
            .Bind(func: dataStructureQuery =>
                ExecuteDataReader(dataStructureQuery: dataStructureQuery, methodId: input.MenuId)
            )
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetRow([FromBody] MasterRecordInput input)
    {
        return Ok(value: sessionObjects.UIService.GetRow(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetAggregations([FromBody] GetGroupsAggregations input)
    {
        var sessionStore = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        if (sessionStore is WorkQueueSessionStore workQueueSessionStore)
        {
            return WorkQueueGetRowsGetAggregationQuery(
                    input: input,
                    sessionStore: workQueueSessionStore
                )
                .Bind(func: ExecuteDataReaderGetPairs)
                .Bind(func: ExtractAggregationList)
                .Map(func: ToActionResult)
                .Finally(func: UnwrapReturnValue);
        }
        return EntityIdentificationToEntityData(input: input)
            .Bind(func: entityData =>
                GetRowsGetAggregationQuery(input: input, entityData: entityData)
            )
            .Bind(func: ExecuteDataReaderGetPairs)
            .Bind(func: ExtractAggregationList)
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetGroups([FromBody] GetGroupsInput input)
    {
        return EntityIdentificationToEntityData(input: input)
            .Bind(func: entityData => GetRowsGetGroupQuery(input: input, entityData: entityData))
            .Bind(func: ExecuteDataReaderGetPairs)
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPut(template: "[action]")]
    public IActionResult Row([FromBody] UpdateRowInput input)
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
            .Bind(func: entityData =>
                GetRow(
                    dataService: dataService,
                    entity: entityData.Entity,
                    dataStructureEntityId: input.DataStructureEntityId,
                    methodId: Guid.Empty,
                    rowId: input.RowId
                )
            )
            .Tap(action: rowData => FillRow(rowData: rowData, newValues: input.NewValues))
            .Map(func: rowData => SubmitChange(rowData: rowData, operation: Operation.Update))
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult Row([FromBody] NewRowInput input)
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
            .Map(func: entityData => MakeEmptyRow(entity: entityData.Entity))
            .Tap(action: rowData => FillRow(input: input, rowData: rowData))
            .Map(func: rowData => SubmitChange(rowData: rowData, operation: Operation.Create))
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult NewEmptyRow([FromBody] NewEmptyRowInput input)
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
            .Map(func: entityData => MakeEmptyRow(entity: entityData.Entity))
            .Map(func: PrepareNewRow)
            .Map(func: rowData => SubmitChange(rowData: rowData, operation: Operation.Create))
            .Finally(func: UnwrapReturnValue);
    }

    [HttpDelete(template: "[action]")]
    public IActionResult Row([FromBody] DeleteRowInput input)
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
            .Bind(func: entityData =>
                GetRow(
                    dataService: dataService,
                    entity: entityData.Entity,
                    dataStructureEntityId: input.DataStructureEntityId,
                    methodId: Guid.Empty,
                    rowId: input.RowIdToDelete
                )
            )
            .Map(func: rowData =>
            {
                rowData.Row.Delete();
                return SubmitDelete(rowData: rowData);
            })
            .Map(func: ThrowAwayReturnData)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier}")]
    public IActionResult WorkflowNextQuery(Guid sessionFormIdentifier)
    {
        return Ok(
            value: sessionObjects.UIService.WorkflowNextQuery(
                sessionFormIdentifier: sessionFormIdentifier
            )
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult WorkflowNext([FromBody] [Required] WorkflowNextInput input)
    {
        return Ok(value: sessionObjects.UIService.WorkflowNext(workflowNextInput: input));
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier}")]
    public IActionResult WorkflowAbort(Guid sessionFormIdentifier)
    {
        return Ok(
            value: sessionObjects.UIService.WorkflowAbort(
                sessionFormIdentifier: sessionFormIdentifier
            )
        );
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier}")]
    public IActionResult WorkflowRepeat(Guid sessionFormIdentifier)
    {
        return Ok(
            value: sessionObjects
                .UIService.WorkflowRepeat(
                    sessionFormIdentifier: sessionFormIdentifier,
                    localizer: localizer
                )
                .Result
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult AttachmentCount([FromBody] [Required] AttachmentCountInput input)
    {
        return Ok(value: sessionObjects.UIService.AttachmentCount(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult AttachmentList([FromBody] [Required] AttachmentListInput input)
    {
        return Ok(value: sessionObjects.UIService.AttachmentList(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetRecordTooltip([FromBody] GetRecordTooltipInput input)
    {
        return AmbiguousInputToRowData(input: input, dataService: dataService)
            .Map(func: RowDataToRecordTooltip)
            .Finally(func: result =>
            {
                if (result.IsSuccess)
                {
                    return result.Value;
                }
                if (result.Error is NotFoundObjectResult notFoundResult)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(
                        xml: "<tooltip title=\"'Error'\">"
                            + $"<cell type=\"text\" x=\"0\" y=\"1\" height=\"1\" width=\"1\">Error</cell>"
                            + $"<cell type=\"text\" x=\"0\" y=\"2\" height=\"1\" width=\"1\">{notFoundResult.Value}</cell>"
                            + "</tooltip>"
                    );
                    return Ok(value: doc);
                }
                return result.Error;
            });
    }

    [HttpGet(template: "[action]")]
    public IActionResult GetNotificationBoxContent()
    {
        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
        {
            Indent = true,
            NewLineOnAttributes = true,
        };
        return Ok(
            value: ServerCoreUIService
                .NotificationBoxContent()
                .ToBeautifulString(xmlWriterSettings: xmlWriterSettings)
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetAudit([FromBody] GetAuditInput input)
    {
        return AmbiguousInputToEntityId(input: input)
            .Map(func: entityId => GetAuditLog(entityId: entityId, id: input.RowId))
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult SaveFavorites([FromBody] [Required] SaveFavoritesInput input)
    {
        ServerCoreUIService.SaveFavorites(input: input);
        return Ok();
    }

    [HttpGet(template: "[action]/{sessionFormIdentifier:guid}")]
    public IActionResult PendingChanges(Guid sessionFormIdentifier)
    {
        return Ok(
            value: sessionObjects.UIService.GetPendingChanges(
                sessionFormIdentifier: sessionFormIdentifier
            )
        );
    }

    [HttpPost(template: "[action]")]
    public IActionResult Changes([FromBody] ChangesInput input)
    {
        return Ok(value: sessionObjects.UIService.GetChanges(input: input));
    }

    [HttpPost(template: "[action]")]
    public IActionResult SaveFilter([FromBody] SaveFilterInput input)
    {
        return FindEntity(id: input.DataStructureEntityId)
            .Bind(func: dataStructureEntity =>
                ServerCoreUIService.SaveFilter(entity: dataStructureEntity, input: input)
            )
            .Map(func: filterId => ToActionResult(obj: filterId))
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult DeleteFilter([FromBody] DeleteFilterInput deleteFilterInput)
    {
        ServerCoreUIService.DeleteFilter(filterId: deleteFilterInput.FilterId);
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult SetDefaultFilter([FromBody] SetDefaultFilterInput input)
    {
        return FindEntity(id: input.DataStructureEntityId)
            .Tap(action: dataStructureEntity =>
                sessionObjects.UIService.SetDefaultFilter(input: input, entity: dataStructureEntity)
            )
            .Map(func: ToActionResult)
            .Map(func: ThrowAwayReturnData)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult ResetDefaultFilter([FromBody] ResetDefaultFilterInput input)
    {
        sessionObjects.UIService.ResetDefaultFilter(input: input);
        return Ok();
    }

    [HttpGet(template: "[action]/{menuId}")]
    public IActionResult ReportFromMenu(Guid menuId)
    {
        return FindItem<ReportReferenceMenuItem>(id: menuId)
            .Bind(func: Authorize)
            .Map(func: menuItem => sessionObjects.UIService.ReportFromMenu(menuId: menuItem.Id))
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetMenuId([FromBody] GetMenuInput input)
    {
        return Ok(value: GetMenuId(lookupId: input.LookupId, referenceId: input.ReferenceId));
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetFilterListValues([FromBody] GetFilterListValuesInput input)
    {
        var sessionStore = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        if (sessionStore is WorkQueueSessionStore workQueueSessionStore)
        {
            return GetFilterListValuesQuery(
                    input: input,
                    entityData: GetWorkQueueEntityData(workQueueSessionStore: workQueueSessionStore)
                )
                .Map(func: queryData =>
                {
                    var query = queryData.DataStructureQuery;
                    query.MethodId = workQueueSessionStore
                        .WorkQueueClass
                        .WorkQueueStructureUserListMethodId;
                    query.SortSetId = workQueueSessionStore
                        .WorkQueueClass
                        .WorkQueueStructureSortSetId;
                    query.DataSourceId = workQueueSessionStore.WorkQueueClass.WorkQueueStructureId;
                    query.Parameters.Add(
                        value: new QueryParameter(
                            _parameterName: "WorkQueueEntry_parWorkQueueId",
                            value: sessionStore.Request.ObjectId
                        )
                    );
                    return query;
                })
                .Bind(func: ExecuteDataReaderGetPairs)
                .Bind(func: StreamlineFilterListValues)
                .Map(func: ToActionResult)
                .Finally(func: UnwrapReturnValue);
        }
        return EntityIdentificationToEntityData(input: input)
            .Bind(func: entityData =>
                GetFilterListValuesQuery(input: input, entityData: entityData)
            )
            .Bind(func: queryData =>
                AddMethodAndSource(
                    sessionFormIdentifier: queryData.SessionFormIdentifier,
                    masterRowId: Guid.Empty,
                    entityData: queryData.EntityData,
                    query: queryData.DataStructureQuery
                )
            )
            .Bind(func: ExecuteDataReaderGetPairs)
            .Bind(func: StreamlineFilterListValues)
            .Map(func: ToActionResult)
            .Finally(func: UnwrapReturnValue);
    }
    #endregion

    private EntityData GetWorkQueueEntityData(WorkQueueSessionStore workQueueSessionStore)
    {
        List<DataStructureEntity> entities = workQueueSessionStore
            .WorkQueueClass
            .WorkQueueStructure
            .Entities;
        var structureEntity = entities
            .Cast<DataStructureEntity>()
            .FirstOrDefault(predicate: entity => entity.Name == workQueueEntity);
        if (entities.Count != 1 || structureEntity == null)
        {
            throw new ArgumentException(
                message: $"WorkQueueStructure {workQueueSessionStore.WorkQueueClass.WorkQueueStructure.Id} must contain exactly one {nameof(DataStructureEntity)} called \"{workQueueEntity}\""
            );
        }
        return new EntityData { Entity = structureEntity, MenuItem = null };
    }

    private string GetMenuId(Guid lookupId, Guid referenceId)
    {
        return ServiceManager
            .Services.GetService<IDataLookupService>()
            .GetMenuBinding(lookupId: lookupId, value: referenceId)
            .MenuId;
    }

    private Result<object, IActionResult> ExtractAggregationList(
        IEnumerable<Dictionary<string, object>> readerResult
    )
    {
        var rowList = readerResult?.ToList();
        if (rowList == null || rowList.Count == 0)
        {
            return Result.Success<object, IActionResult>(
                value: new List<Dictionary<string, object>>()
            );
        }
        var firstRow = rowList[index: 0];
        if (firstRow == null || !firstRow.ContainsKey(key: "aggregations"))
        {
            return Result.Success<object, IActionResult>(
                value: new List<Dictionary<string, object>>()
            );
        }
        return Result.Success<object, IActionResult>(value: firstRow[key: "aggregations"]);
    }

    private Result<IEnumerable<object>, IActionResult> StreamlineFilterListValues(
        IEnumerable<IEnumerable<KeyValuePair<string, object>>> fullReaderResult
    )
    {
        var streamlinedList = new List<object>();
        foreach (var entry in fullReaderResult)
        {
            streamlinedList.Add(item: entry.First().Value);
        }
        return Result.Success<IEnumerable<object>, IActionResult>(value: streamlinedList);
    }

    private Dictionary<object, string> GetLookupLabelsInternal(LookupLabelsInput input)
    {
        var labelDictionary = input.LabelIds.ToDictionary(
            keySelector: id => id,
            elementSelector: id =>
            {
                object lookupResult = lookupService.GetDisplayText(
                    lookupId: input.LookupId,
                    lookupValue: id,
                    useCache: false,
                    returnMessageIfNull: true,
                    transactionId: null
                );
                return lookupResult is decimal result
                    ? result.ToString(format: "0.#")
                    : lookupResult.ToString();
            }
        );
        return labelDictionary;
    }

    private IList<string> GetLookupCacheDependencies(string lookupId)
    {
        // TODO: Later we can implement multiple dependencies e.g. for entities based on views where we
        // could name all the source entities/tables on which the view is dependent so even view based
        // lookups could be handled correctly and reset when source table change.
        var result = new List<string>();
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var dataServiceDataLookup =
            persistenceService.SchemaProvider.RetrieveInstance<DataServiceDataLookup>(
                instanceId: new Guid(g: lookupId)
            );
        var datasetGenerator = new DatasetGenerator(userDefinedParameters: true);
        var comboListDataset = datasetGenerator.CreateDataSet(
            ds: dataServiceDataLookup.ListDataStructure
        );
        var comboListTable = comboListDataset.Tables[
            name: dataServiceDataLookup
                .ListDataStructure.ChildItemsByType<DataStructureEntity>(
                    itemType: DataStructureEntity.CategoryConst
                )[index: 0]
                .Name
        ];
        var tableName = FormXmlBuilder.DatabaseTableName(table: comboListTable);
        if (tableName != null)
        {
            result.Add(item: tableName);
        }
        return result;
    }

    private IActionResult CheckLookup(LookupLabelsInput input)
    {
        if (input.MenuId == Guid.Empty)
        {
            SecurityTools.CurrentUserProfile();
        }
        else
        {
            var menuResult = FindItem<FormReferenceMenuItem>(id: input.MenuId)
                .Bind(func: Authorize)
                .Bind(func: menuItem =>
                    CheckLookupIsAllowedInMenu(
                        menuItem: (FormReferenceMenuItem)menuItem,
                        lookupId: input.LookupId
                    )
                );
            if (menuResult.IsFailure)
            {
                return menuResult.Error;
            }
        }
        return null;
    }

    private Result<FormReferenceMenuItem, IActionResult> CheckLookupIsAllowedInMenu(
        FormReferenceMenuItem menuItem,
        Guid lookupId
    )
    {
        if (!MenuLookupIndex.HasDataFor(menuItemId: menuItem.Id))
        {
            var xmlOutput = FormXmlBuilder.GetXml(menuId: menuItem.Id);
            MenuLookupIndex.AddIfNotPresent(
                menuId: menuItem.Id,
                containedLookups: xmlOutput.ContainedLookups
            );
        }
        return MenuLookupIndex.IsAllowed(menuItemId: menuItem.Id, lookupId: lookupId)
            ? Result.Success<FormReferenceMenuItem, IActionResult>(value: menuItem)
            : Result.Failure<FormReferenceMenuItem, IActionResult>(
                error: BadRequest(error: "Lookup is not referenced in any entity in the Menu item")
            );
    }

    private IEnumerable<object[]> GetRowData(LookupListInput input, DataTable dataTable)
    {
        var lookup = FindItem<DataServiceDataLookup>(id: input.LookupId).Value;
        if (lookup.IsFilteredServerside || string.IsNullOrEmpty(value: input.SearchText))
        {
            return dataTable
                .Rows.Cast<DataRow>()
                .Select(selector: row => GetColumnValues(row: row, columnNames: input.ColumnNames));
        }
        var columnNamesWithoutPrimaryKey = FilterOutPrimaryKey(
            columnNames: input.ColumnNames,
            primaryKey: dataTable.PrimaryKey
        );
        return dataTable
            .Rows.Cast<DataRow>()
            .Where(predicate: row =>
                Filter(
                    row: row,
                    columnNames: columnNamesWithoutPrimaryKey,
                    likeParameter: input.SearchText
                )
            )
            .Select(selector: row => GetColumnValues(row: row, columnNames: input.ColumnNames));
    }

    private static object[] GetColumnValues(DataRow row, IEnumerable<string> columnNames)
    {
        return columnNames.Select(selector: colName => row[columnName: colName]).ToArray();
    }

    private static string[] FilterOutPrimaryKey(
        IEnumerable<string> columnNames,
        DataColumn[] primaryKey
    )
    {
        return columnNames
            .Where(predicate: columnName =>
                primaryKey.All(predicate: dataColumn => dataColumn.ColumnName != columnName)
            )
            .ToArray();
    }

    private static bool Filter(DataRow row, IEnumerable<string> columnNames, string likeParameter)
    {
        return columnNames
            .Select(selector: colName => row[columnName: colName])
            .Any(predicate: colValue =>
                colValue
                    .ToString()
                    .Contains(
                        value: likeParameter,
                        comparisonType: StringComparison.InvariantCultureIgnoreCase
                    )
            );
    }

    private Result<IEnumerable<object[]>, IActionResult> GetLookupRows(
        LookupListInput input,
        RowData rowData
    )
    {
        var internalRequest = new LookupListRequest
        {
            LookupId = input.LookupId,
            FieldName = input.Property,
            CurrentRow = rowData.Row,
            ShowUniqueValues = input.ShowUniqueValues,
            SearchText = input.SearchText,
            PageSize = input.PageSize,
            PageNumber = input.PageNumber,
            ParameterMappings = DictionaryToHashtable(source: input.Parameters),
        };
        var dataTable = lookupService.GetList(request: internalRequest);
        return AreColumnNamesValid(input: input, dataTable: dataTable)
            ? Result.Success<IEnumerable<object[]>, IActionResult>(
                value: GetRowData(input: input, dataTable: dataTable)
            )
            : Result.Failure<IEnumerable<object[]>, IActionResult>(
                error: BadRequest(error: "Some of the supplied column names are not in the table.")
            );
    }

    private Result<Dictionary<string, string>, IActionResult> RowDataToNewRecordInitialValues(
        LookupNewRecordInitialValuesInput input,
        RowData rowData
    )
    {
        var initialValues = new Dictionary<string, string>();
        // key parameter name, value target column name
        foreach (KeyValuePair<string, string> parameterMapping in input.ParameterMappings)
        {
            if (
                parameterMapping.Key.Equals(
                    value: "SearchText",
                    comparisonType: StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                initialValues.Add(
                    // target column name
                    key: parameterMapping.Value,
                    // target column value
                    value: input.SearchText
                );
            }
            else if (
                rowData.Row.Table.Columns.Contains(
                    name: (string)input.Parameters[key: parameterMapping.Key]
                )
            )
            {
                initialValues.Add(
                    // target column name
                    key: parameterMapping.Value,
                    // target column value
                    value: rowData
                        .Row[columnName: (string)input.Parameters[key: parameterMapping.Key]]
                        .ToString()
                );
            }
            else
            {
                throw new ArgumentException(
                    message: $"Parameter '{parameterMapping.Key}' maps to not available source column '{input.Parameters[key: parameterMapping.Key]}'."
                );
            }
        }
        return initialValues;
    }

    private Result<RowData, IActionResult> LookupInputToRowData(AbstractLookupRowDataInput input)
    {
        if (input.SessionFormIdentifier == Guid.Empty)
        {
            return FindItem<FormReferenceMenuItem>(id: input.MenuId)
                .Bind(func: Authorize)
                .Bind(func: menuItem =>
                    CheckLookupIsAllowedInMenu(
                        menuItem: (FormReferenceMenuItem)menuItem,
                        lookupId: input.LookupId
                    )
                )
                .Bind(func: menuItem =>
                    GetEntityData(
                        dataStructureEntityId: input.DataStructureEntityId,
                        menuItem: menuItem
                    )
                )
                .Bind(func: CheckEntityBelongsToMenu)
                .Bind(func: entityData =>
                    GetRow(
                        dataService: dataService,
                        entity: entityData.Entity,
                        dataStructureEntityId: input.DataStructureEntityId,
                        methodId: Guid.Empty,
                        rowId: input.Id
                    )
                );
        }
        if (input.DataStructureEntityId == Guid.Empty)
        {
            return sessionObjects.UIService.GetRow(
                sessionFormIdentifier: input.SessionFormIdentifier,
                entity: input.Entity,
                dataStructureEntity: null,
                rowId: input.Id
            );
        }
        return FindEntity(id: input.DataStructureEntityId)
            .Bind(func: dataStructureEntity =>
                sessionObjects.UIService.GetRow(
                    sessionFormIdentifier: input.SessionFormIdentifier,
                    entity: input.Entity,
                    dataStructureEntity: dataStructureEntity,
                    rowId: input.Id
                )
            );
    }

    private static Hashtable DictionaryToHashtable(IDictionary<string, object> source)
    {
        var result = new Hashtable(capacity: source.Count);
        foreach (var (key, value) in source)
        {
            result.Add(key: key, value: value);
        }
        return result;
    }

    private static bool AreColumnNamesValid(LookupListInput input, DataTable dataTable)
    {
        var actualColumnNames = dataTable
            .Columns.Cast<DataColumn>()
            .Select(selector: x => x.ColumnName)
            .ToArray();
        return input.ColumnNames.All(predicate: colName =>
            actualColumnNames.Contains(value: colName)
        );
    }

    private Result<DataStructureQuery, IActionResult> WorkQueueGetRowsGetAggregationQuery(
        GetGroupsAggregations input,
        WorkQueueSessionStore sessionStore
    )
    {
        var query = new DataStructureQuery
        {
            Entity = workQueueEntity,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>(),
            },
            ColumnsInfo = new ColumnsInfo(
                columns: new List<ColumnData>(),
                renderSqlForDetachedFields: true
            ),
            ForceDatabaseCalculation = true,
            AggregatedColumns = input.AggregatedColumns,
            MethodId = sessionStore.WorkQueueClass.WorkQueueStructureUserListMethodId,
            SortSetId = sessionStore.WorkQueueClass.WorkQueueStructureSortSetId,
            DataSourceId = sessionStore.WorkQueueClass.WorkQueueStructureId,
        };
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

    private Result<DataStructureQuery, IActionResult> GetRowsGetAggregationQuery(
        GetGroupsAggregations input,
        EntityData entityData
    )
    {
        var query = new DataStructureQuery
        {
            Entity = entityData.Entity.Name,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>(),
            },
            ColumnsInfo = new ColumnsInfo(
                columns: new List<ColumnData>(),
                renderSqlForDetachedFields: true
            ),
            ForceDatabaseCalculation = true,
            AggregatedColumns = input.AggregatedColumns,
        };
        return AddMethodAndSource(
            sessionFormIdentifier: input.SessionFormIdentifier,
            masterRowId: input.MasterRowId,
            entityData: entityData,
            query: query
        );
    }

    private Result<QueryData, IActionResult> GetFilterListValuesQuery(
        GetFilterListValuesInput input,
        EntityData entityData
    )
    {
        var column = entityData.Entity.Column(name: input.Property);
        if (column == null)
        {
            return Result.Failure<QueryData, IActionResult>(
                error: BadRequest(
                    error: $"Cannot get values for \"{input.Property}\" because the column does not exist."
                )
            );
        }
        var field = column.Field;
        ColumnData columnData;
        string entity;
        if ((field is DetachedField detachedField) && (detachedField.ArrayRelation != null))
        {
            columnData = new ColumnData(
                name: detachedField.ArrayValueField.Name,
                isVirtual: (detachedField.ArrayValueField is DetachedField),
                defaultValue: (detachedField.ArrayValueField as DetachedField)?.DefaultValue?.Value,
                hasRelation: (detachedField.ArrayValueField as DetachedField)?.ArrayRelation != null
            );
            entity = detachedField.ArrayRelation.AssociatedEntity.Name;
        }
        else
        {
            columnData = new ColumnData(
                name: input.Property,
                isVirtual: (field is DetachedField),
                defaultValue: (field as DetachedField)?.DefaultValue?.Value,
                hasRelation: (field as DetachedField)?.ArrayRelation != null
            );
            entity = entityData.Entity.Name;
        }
        var columns = new List<ColumnData> { columnData };
        var query = new DataStructureQuery
        {
            Entity = entity,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>(),
            },
            CustomOrderings = new CustomOrderings(orderings: new List<Ordering>()),
            ColumnsInfo = new ColumnsInfo(columns: columns, renderSqlForDetachedFields: true),
            Distinct = true,
            ForceDatabaseCalculation = true,
            AggregatedColumns = new List<Aggregation>(),
        };
        return new QueryData
        {
            SessionFormIdentifier = input.SessionFormIdentifier,
            DataStructureQuery = query,
            EntityData = entityData,
        };
    }

    private class QueryData
    {
        public Guid SessionFormIdentifier { get; set; }
        public DataStructureQuery DataStructureQuery { get; set; }
        public EntityData EntityData { get; set; }
    }

    private Result<DataStructureQuery, IActionResult> GetRowsGetGroupQuery(
        GetGroupsInput input,
        EntityData entityData
    )
    {
        var customOrdering = GetOrderings(orderingList: input.OrderingList);
        DataStructureColumn column = entityData.Entity.Column(name: input.GroupBy);
        if (column == null)
        {
            return Result.Failure<DataStructureQuery, IActionResult>(
                error: BadRequest(
                    error: $"Cannot group by \"{input.GroupBy}\" because the column does not exist."
                )
            );
        }
        var field = column.Field;
        var columnData = new ColumnData(
            name: input.GroupBy,
            isVirtual: (field is DetachedField),
            defaultValue: (field as DetachedField)?.DefaultValue?.Value,
            hasRelation: (field as DetachedField)?.ArrayRelation != null
        );
        List<ColumnData> columns = new List<ColumnData>
        {
            columnData,
            ColumnData.GroupByCountColumn,
        };
        if (input.GroupByLookupId != Guid.Empty)
        {
            columns.Add(item: ColumnData.GroupByCaptionColumn);
        }
        var query = new DataStructureQuery
        {
            Entity = entityData.Entity.Name,
            CustomFilters = new CustomFilters
            {
                Filters = input.Filter,
                FilterLookups = input.FilterLookups ?? new Dictionary<string, Guid>(),
            },
            CustomOrderings = customOrdering,
            RowLimit = input.RowLimit,
            ColumnsInfo = new ColumnsInfo(columns: columns, renderSqlForDetachedFields: true),
            ForceDatabaseCalculation = true,
            CustomGrouping = new Grouping(
                groupBy: input.GroupBy,
                lookupId: input.GroupByLookupId,
                groupingUnit: input.GroupingUnit
            ),
            AggregatedColumns = input.AggregatedColumns,
        };
        return AddMethodAndSource(
            sessionFormIdentifier: input.SessionFormIdentifier,
            masterRowId: input.MasterRowId,
            entityData: entityData,
            query: query
        );
    }

    private static void FillRow(RowData rowData, Dictionary<string, string> newValues)
    {
        foreach (var (key, value) in newValues)
        {
            var dataType = rowData.Row.Table.Columns[name: key].DataType;
            rowData.Row[columnName: key] = DatasetTools.ConvertValue(
                value: value,
                targetType: dataType
            );
        }
    }

    private RowData MakeEmptyRow(DataStructureEntity entity)
    {
        var dataSet = dataService.GetEmptyDataSet(
            dataStructureId: entity.RootEntity.ParentItemId,
            culture: CultureInfo.InvariantCulture
        );
        var table = dataSet.Tables[name: entity.Name];
        var row = table.NewRow();
        return new RowData { Entity = entity, Row = row };
    }

    private static RowData FillRow(NewRowInput input, RowData rowData)
    {
        DatasetTools.ApplyPrimaryKey(row: rowData.Row);
        DatasetTools.UpdateOrigamSystemColumns(
            row: rowData.Row,
            isNew: true,
            profileId: SecurityManager.CurrentUserProfile().Id
        );
        FillRow(rowData: rowData, newValues: input.NewValues);
        rowData.Row.Table.Rows.Add(row: rowData.Row);
        return rowData;
    }

    private static RowData PrepareNewRow(RowData rowData)
    {
        DatasetTools.ApplyPrimaryKey(row: rowData.Row);
        DatasetTools.UpdateOrigamSystemColumns(
            row: rowData.Row,
            isNew: true,
            profileId: SecurityManager.CurrentUserProfile().Id
        );
        rowData.Row.Table.NewRow();
        return rowData;
    }

    private IActionResult SubmitDelete(RowData rowData)
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
            value: new ChangeInfo
            {
                Entity = rowData.Row.Table.TableName,
                Operation = Operation.Delete,
            }
        );
    }

    private IActionResult ThrowAwayReturnData(IActionResult arg)
    {
        return Ok();
    }

    private IActionResult RowDataToRecordTooltip(RowData rowData)
    {
        var requestCultureFeature = Request.HttpContext.Features.Get<IRequestCultureFeature>();
        var cultureInfo = requestCultureFeature.RequestCulture.Culture;
        return Ok(
            value: ServerCoreUIService.DataRowToRecordTooltip(
                row: rowData.Row,
                cultureInfo: cultureInfo,
                localizer: localizer
            )
        );
    }

    private IActionResult GetAuditLog(Guid entityId, object id)
    {
        var auditLog = AuditLogDA.RetrieveLogTransformed(entityId: entityId, recordId: id);
        if (log != null)
        {
            return Ok(
                value: DataTools.DatatableToDictionary(
                    t: auditLog.Tables[index: 0],
                    includeColumnNames: false
                )
            );
        }
        return Ok();
    }

    private void AddConfigData(PortalResult result)
    {
        result.LogoUrl = string.IsNullOrWhiteSpace(value: customAssetsConfig.Html5ClientLogoUrl)
            ? "./img/origam-logo.svg"
            : customAssetsConfig.Html5ClientLogoUrl;
        result.ChatRefreshInterval = string.IsNullOrEmpty(value: chatConfig.PathToChatApp)
            ? 0
            : chatConfig.ChatRefreshInterval;
        result.CustomAssetsRoute = customAssetsConfig.RouteToCustomAssetsFolder;
        result.ShowToolTipsForMemoFieldsOnly = htmlClientConfig.ShowToolTipsForMemoFieldsOnly;
        result.RowStatesDebouncingDelayMilliseconds =
            htmlClientConfig.RowStatesDebouncingDelayMilliseconds;
        result.DropDownTypingDebouncingDelayMilliseconds =
            htmlClientConfig.DropDownTypingDebouncingDelayMilliseconds;
        result.GetLookupLabelExDebouncingDelayMilliseconds =
            htmlClientConfig.GetLookupLabelExDebouncingDelayMilliseconds;
        result.FilteringConfig = clientFilteringConfig;
    }
}
