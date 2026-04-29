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
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MoreLinq;
using Origam.DA;
using Origam.Extensions;
using Origam.Gui;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Common;
using Origam.Server.Controller;
using Origam.Server.Model.UIService;
using Origam.Service.Core;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

// ReSharper disable once InconsistentNaming
public class ServerCoreUIService : IBasicUIService
{
    private const int INITIAL_PAGE_NUMBER_OF_RECORDS = 50;

    // ReSharper disable once InconsistentNaming
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private readonly UIManager uiManager;
    private readonly SessionManager sessionManager;
    private readonly SessionHelper sessionHelper;
    private readonly IReportManager reportManager;

    public ServerCoreUIService(UIManager uiManager, SessionManager sessionManager)
    {
        this.uiManager = uiManager;
        this.sessionManager = sessionManager;
        sessionHelper = new SessionHelper(sessionManager: sessionManager);
        reportManager = new ServerCoreReportManager(sessionManager: sessionManager);
    }

    public string GetReportStandalone(
        string reportId,
        Hashtable parameters,
        DataReportExportFormatType dataReportExportFormatType
    )
    {
        return reportManager.GetReportStandalone(
            reportId: reportId,
            parameters: parameters,
            dataReportExportFormatType: dataReportExportFormatType
        );
    }

    public UIResult InitUI(UIRequest request)
    {
        return uiManager.InitUI(
            request: request,
            addChildSession: false,
            parentSession: null,
            basicUIService: this
        );
    }

    public PortalResult InitPortal(int maxRequestLength)
    {
        OrigamUserContext.Reset();
        var profile = SecurityTools.CurrentUserProfile();
        var result = new PortalResult(menu: MenuXmlBuilder.GetMenu());
        var settings = ConfigurationManager.GetActiveConfiguration();
        if (settings != null)
        {
            result.WorkQueueListRefreshInterval = settings.WorkQueueListRefreshPeriod * 1000;
            result.Slogan = settings.Slogan;
            result.HelpUrl = settings.HelpUrl;
        }
        result.MaxRequestLength = maxRequestLength;
        var logoNotificationBox = LogoNotificationBox();
        if (logoNotificationBox != null)
        {
            result.NotificationBoxRefreshInterval = logoNotificationBox.RefreshInterval * 1000;
        }
        // load favorites
        var favorites = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "e564c554-ca83-47eb-980d-95b4faba8fb8"),
            methodId: new Guid(g: "e468076e-a641-4b7d-b9b4-7d80ff312b1c"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "OrigamFavoritesUserConfig_parBusinessPartnerId",
            paramValue1: profile.Id
        );
        if (favorites.Tables[name: "OrigamFavoritesUserConfig"].Rows.Count > 0)
        {
            result.Favorites = (string)
                favorites.Tables[name: "OrigamFavoritesUserConfig"].Rows[index: 0][
                    columnName: "ConfigXml"
                ];
        }
        sessionManager.AddOrUpdatePortalSession(
            id: profile.Id,
            addSession: id => new PortalSessionStore(profileId: profile.Id),
            updateSession: (storeId, portalSessionStore) =>
            {
                var clearAll = portalSessionStore.ShouldBeCleared();
                // running session, we get all the form sessions
                var sessionsToDestroy = new List<Guid>();
                foreach (var mainSessionStore in portalSessionStore.FormSessions)
                {
                    if (clearAll)
                    {
                        sessionsToDestroy.Add(item: mainSessionStore.Id);
                    }
                    else if (sessionManager.HasFormSession(id: mainSessionStore.Id))
                    {
                        var sessionStore = mainSessionStore.ActiveSession ?? mainSessionStore;
                        if (
                            sessionStore is SelectionDialogSessionStore
                            || sessionStore.IsModalDialog
                        )
                        {
                            sessionsToDestroy.Add(item: sessionStore.Id);
                        }
                        else
                        {
                            var askWorkflowClose = false;
                            if (sessionStore is WorkflowSessionStore workflowSessionStore)
                            {
                                askWorkflowClose = workflowSessionStore.AskWorkflowClose;
                            }
                            var hasChanges = HasChanges(sessionStore: sessionStore);
                            result.Sessions.Add(
                                item: new PortalResultSession(
                                    formSessionId: sessionStore.Id,
                                    objectId: sessionStore.Request.ObjectId,
                                    isDirty: hasChanges,
                                    type: sessionStore.Request.Type,
                                    caption: sessionStore.Request.Caption,
                                    icon: sessionStore.Request.Icon,
                                    askWorkflowClose: askWorkflowClose
                                )
                            );
                        }
                    }
                    else
                    {
                        // session is registered in the user's portal,
                        // but not in the UIService anymore,
                        // we have to destroy it
                        sessionsToDestroy.Add(item: mainSessionStore.Id);
                    }
                }
                foreach (Guid id in sessionsToDestroy)
                {
                    try
                    {
                        DestroyUI(sessionFormIdentifier: id);
                    }
                    catch (Exception ex)
                    {
                        if (log.IsFatalEnabled)
                        {
                            log.LogOrigamError(
                                message: "Failed to destroy session " + id.ToString() + ".",
                                ex: ex
                            );
                        }
                    }
                }
                if (clearAll)
                {
                    portalSessionStore.ResetSessionStart();
                }
                return portalSessionStore;
            }
        );
        result.UserName = profile.FullName;
        result.UserId = profile.Id;
        result.Tooltip = ToolTipTools.NextTooltip();
        result.Title = ConfigurationManager.GetActiveConfiguration().TitleText;
        result.InitialScreenId = GetInitialScreenId();
        CreateUpdateOrigamOnlineUser();
        return result;
    }

    private static string GetInitialScreenId()
    {
        MenuSchemaItemProvider menuProvider = ServiceManager
            .Services.GetService<SchemaService>()
            .GetProvider<MenuSchemaItemProvider>();
        return menuProvider
            .ChildItemsRecursive.OfType<AbstractMenuItem>()
            .Where(predicate: x => x is ReportReferenceMenuItem || x is FormReferenceMenuItem)
            .FirstOrDefault(predicate: FormTools.IsFormMenuInitialScreen)
            ?.Id.ToString();
    }

    public void Logout()
    {
        PortalSessionStore pss;
        try
        {
            pss = sessionManager.GetPortalSession();
        }
        catch (SessionExpiredException)
        {
            return;
        }
        if (pss == null)
        {
            return;
        }
        while (pss.FormSessions.Count > 0)
        {
            DestroyUI(sessionFormIdentifier: pss.FormSessions[index: 0].Id);
        }
        Analytics.Instance.Log(message: "UI_LOGOUT");
        sessionManager.RemovePortalSession(id: SecurityTools.CurrentUserProfile().Id);
        Task.Run(action: () =>
            SecurityTools.RemoveOrigamOnlineUser(
                username: SecurityManager.CurrentPrincipal.Identity.Name
            )
        );
        OrigamUserContext.Reset();
    }

    // ReSharper disable once InconsistentNaming
    public void DestroyUI(Guid sessionFormIdentifier)
    {
        sessionHelper.DeleteSession(sessionFormIdentifier: sessionFormIdentifier);
        CreateUpdateOrigamOnlineUser();
    }

    public void DestroyUI(IEnumerable<Guid> sessionFormIdentifiers)
    {
        if (sessionFormIdentifiers == null)
        {
            return;
        }
        foreach (var sessionFormIdentifier in sessionFormIdentifiers)
        {
            sessionHelper.DeleteSession(sessionFormIdentifier: sessionFormIdentifier);
        }
        CreateUpdateOrigamOnlineUser();
    }

    public IDictionary<string, object> RefreshData(
        Guid sessionFormIdentifier,
        IStringLocalizer<SharedResources> localizer
    )
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier);
        var result = sessionStore.ExecuteAction(actionId: SessionStore.ACTION_REFRESH);
        CreateUpdateOrigamOnlineUser();
        if (!(result is DataSet))
        {
            throw new Exception(
                message: localizer[
                    name: "ErrorRefreshReturnInvalid",
                    arguments: [sessionStore.GetType().Name, SessionStore.ACTION_REFRESH]
                ]
            );
        }
        IList<string> columns = null;
        if (sessionStore.IsPagedLoading)
        {
            // for lazily-loaded data we provide all the preloaded columns
            // (primary keys + all the initial sort columns)
            columns = sessionStore.DataListLoadedColumns;
        }
        return DataTools.DatasetToDictionary(
            data: result as DataSet,
            columns: columns,
            firstPageRecords: INITIAL_PAGE_NUMBER_OF_RECORDS,
            firstRecordId: sessionStore.CurrentRecordId,
            dataListEntity: sessionStore.DataListEntity,
            ss: sessionStore
        );
    }

    public List<ChangeInfo> RestoreData(RestoreDataInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        return sessionStore.RestoreData(parentId: input.ObjectId);
    }

    public RuleExceptionDataCollection SaveDataQuery(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier);
        if (sessionStore.Data.HasErrors)
        {
            throw new RuleException(message: Origam.Server.Resources.ErrorInForm);
        }
        if ((sessionStore.ConfirmationRule == null) || !sessionStore.Data.HasChanges())
        {
            return new RuleExceptionDataCollection();
        }
        var hasRows = false;
        var clone = DatasetTools.CloneDataSet(dataset: sessionStore.Data);
        var rootTables = sessionStore
            .Data.Tables.Cast<DataTable>()
            .Where(predicate: table => table.ParentRelations.Count == 0)
            .ToList();
        foreach (var rootTable in rootTables)
        {
            foreach (DataRow row in rootTable.Rows)
            {
                if ((row.RowState == DataRowState.Deleted) || !IsRowDirty(row: row))
                {
                    continue;
                }
                DatasetTools.GetDataSlice(target: clone, rows: new List<DataRow> { row });
                hasRows = true;
            }
        }
        if (!hasRows)
        {
            return new RuleExceptionDataCollection();
        }
        var xmlDoc = DataDocumentFactory.New(dataSet: clone);
        return sessionStore.RuleEngine.EvaluateEndRule(
            rule: sessionStore.ConfirmationRule,
            data: xmlDoc
        );
    }

    public IList SaveData(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier);
        var output = (IList)sessionStore.ExecuteAction(actionId: SessionStore.ACTION_SAVE);
        CreateUpdateOrigamOnlineUser();
        return output;
    }

    public IList CreateObject(CreateObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        // todo: propagate requesting grid as guid?
        IList output = sessionStore.CreateObject(
            entity: input.Entity,
            values: input.Values,
            parameters: input.Parameters,
            requestingGrid: input.RequestingGridId.ToString()
        );
        CreateUpdateOrigamOnlineUser();
        return output;
    }

    public IList CopyObject(CopyObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        IList output = sessionStore.CopyObject(
            entity: input.Entity,
            originalId: input.OriginalId,
            requestingGrid: input.RequestingGridId.ToString(),
            entities: input.Entities,
            forcedValues: input.ForcedValues
        );
        CreateUpdateOrigamOnlineUser();
        return output;
    }

    public IList UpdateObject(UpdateObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        IList output = sessionStore.UpdateObjectBatch(
            entity: input.Entity,
            updateDataArray: input.UpdateData
        );
        CreateUpdateOrigamOnlineUser();
        return output;
    }

    public IList DeleteObject(DeleteObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        IList output = sessionStore.DeleteObject(entity: input.Entity, id: input.Id);
        CreateUpdateOrigamOnlineUser();
        return output;
    }

    public IList DeleteObjectInOrderedList(DeleteObjectInOrderedListInput input)
    {
        SessionStore ss = sessionManager.GetSession(
            sessionFormIdentifier: new Guid(g: input.SessionFormIdentifier)
        );
        List<ChangeInfo> changes = ss.UpdateObjectBatch(
            entity: input.Entity,
            property: input.OrderProperty,
            values: input.UpdatedOrderValues
        );
        changes.AddRange(collection: ss.DeleteObject(entity: input.Entity, id: input.Id));
        Task.Run(action: () =>
            SecurityTools.CreateUpdateOrigamOnlineUser(
                username: SecurityManager.CurrentPrincipal.Identity.Name,
                stats: sessionManager.GetSessionStats()
            )
        );
        return changes;
    }

    public List<ChangeInfo> GetRowData(MasterRecordInput input)
    {
        SessionStore sessionStore = GetSessionStore(sessionId: input.SessionFormIdentifier);
        if (sessionStore == null)
        {
            return new List<ChangeInfo>();
        }
        return sessionStore.GetRowData(
            entity: input.Entity,
            id: input.RowId,
            ignoreDirtyState: false
        );
    }

    public ChangeInfo GetRow(MasterRecordInput input)
    {
        return GetSessionStore(sessionId: input.SessionFormIdentifier)
            ?.GetRow(entity: input.Entity, id: input.RowId);
    }

    public IDictionary GetParameters(Guid sessionFormIdentifier)
    {
        if (sessionFormIdentifier == Guid.Empty)
        {
            return new Hashtable();
        }
        SessionStore sessionStore = GetSessionStore(sessionId: sessionFormIdentifier);
        return sessionStore == null ? new Hashtable() : sessionStore.Request.Parameters;
    }

    public List<List<object>> GetData(GetDataInput input)
    {
        SessionStore sessionStore = GetSessionStore(sessionId: input.SessionFormIdentifier);
        if (sessionStore == null)
        {
            return new List<List<object>>();
        }
        return sessionStore.GetData(
            childEntity: input.ChildEntity,
            parentRecordId: input.ParentRecordId,
            rootRecordId: input.RootRecordId
        );
    }

    private SessionStore GetSessionStore(Guid sessionId)
    {
        try
        {
            return sessionManager.GetSession(sessionFormIdentifier: sessionId);
        }
        catch (Exception ex)
        {
            log.Warn(message: ex.Message, exception: ex);
            return null;
        }
    }

    public IList RowStates(RowStatesInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        return sessionStore.RowStates(entity: input.Entity, ids: input.Ids);
    }

    public RuleExceptionDataCollection ExecuteActionQuery(ExecuteActionQueryInput input)
    {
        EntityUIAction action = GetAction(actionId: input.ActionId);
        // work queue commands are treated as actions,
        // but they're not part of the model,
        // so the GetAction can return null
        // the subsequent code needs to be able to handle it
        if (action is EntityMenuAction menuAction)
        {
            bool isAuthorized = SecurityManager
                .GetAuthorizationProvider()
                .Authorize(
                    principal: SecurityManager.CurrentPrincipal,
                    context: menuAction.Menu.Roles
                );
            if (!isAuthorized)
            {
                return new RuleExceptionDataCollection(
                    value: new[]
                    {
                        new RuleExceptionData(
                            message: string.Format(
                                format: Origam.Server.Resources.MenuNotAuthorized,
                                arg0: menuAction.Menu.NodeText
                            )
                        ),
                    }
                );
            }
        }
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        // check if action is on data list entity
        // we can't execute multiple checkboxes action on list entity,
        // but it is OK to execute them on details
        if (
            sessionStore.IsDelayedLoading
            && (action is EntityWorkflowAction workflowAction)
            && (action.Mode == PanelActionMode.MultipleCheckboxes)
            && (sessionStore.DataListEntity == input.Entity)
            && (workflowAction.MergeType != ServiceOutputMethod.Ignore)
        )
        {
            throw new Exception(
                message: "Only actions with merge type Ignore can be invoked in lazily loaded screens."
            );
        }
        if (action?.ConfirmationRule == null)
        {
            return new RuleExceptionDataCollection();
        }
        List<DataRow> rows = sessionStore.GetRows(entity: input.Entity, ids: input.SelectedIds);
        IXmlContainer xml = DatasetTools.GetRowXml(rows: rows, version: DataRowVersion.Default);
        var result = sessionStore.RuleEngine.EvaluateEndRule(
            rule: action.ConfirmationRule,
            data: xml
        );
        return result ?? new RuleExceptionDataCollection();
    }

    public IList ExecuteAction(ExecuteActionInput input)
    {
        EntityUIAction action = GetAction(actionId: input.ActionId);
        if (action is EntityMenuAction menuAction)
        {
            bool isAuthorized = SecurityManager
                .GetAuthorizationProvider()
                .Authorize(
                    principal: SecurityManager.CurrentPrincipal,
                    context: menuAction.Menu.Roles
                );
            if (!isAuthorized)
            {
                throw new OrigamSecurityException(
                    message: string.Format(
                        format: Resources.MenuNotAuthorized,
                        arg0: menuAction.Menu.NodeText
                    )
                );
            }
        }
        var actionRunnerClient = new ServerEntityUIActionRunnerClient(
            sessionManager: sessionManager,
            sessionFormIdentifier: input.SessionFormIdentifier.ToString()
        );
        var actionRunner = new ServerCoreEntityUIActionRunner(
            actionRunnerClient: actionRunnerClient,
            uiManager: uiManager,
            sessionManager: sessionManager,
            basicUIService: this,
            reportManager: reportManager
        );
        return actionRunner.ExecuteAction(
            sessionFormIdentifier: input.SessionFormIdentifier.ToString(),
            requestingGrid: input.RequestingGrid.ToString(),
            entity: input.Entity,
            actionType: input.ActionType,
            actionId: input.ActionId,
            parameterMappings: input.ParameterMappings,
            selectedIds: input.SelectedIds,
            inputParameters: input.InputParameters
        );
    }

    private static EntityUIAction GetAction(string actionId)
    {
        try
        {
            return UIActionTools.GetAction(action: actionId);
        }
        catch
        {
            // ignored
        }
        return null;
    }

    public Result<RowData, IActionResult> GetRow(
        Guid sessionFormIdentifier,
        string entity,
        DataStructureEntity dataStructureEntity,
        Guid rowId
    )
    {
        SessionStore sessionStore = GetSessionStore(sessionId: sessionFormIdentifier);
        switch (sessionStore)
        {
            case null:
            {
                return Result.Success<RowData, IActionResult>(
                    value: new RowData { Row = null, Entity = null }
                );
            }
            default:
            {
                var row = sessionStore.GetSessionRow(entity: entity, id: rowId);
                return Result.Success<RowData, IActionResult>(
                    value: new RowData { Row = row, Entity = dataStructureEntity }
                );
            }
        }
    }

    public Result<Guid, IActionResult> GetEntityId(Guid sessionFormIdentifier, string entity)
    {
        SessionStore sessionStore = GetSessionStore(sessionId: sessionFormIdentifier);
        switch (sessionStore)
        {
            case null:
            {
                return Result.Success<Guid, IActionResult>(value: Guid.Empty);
            }
            default:
            {
                var table = sessionStore.GetDataTable(entity: entity, data: sessionStore.Data);
                var entityId = Guid.Empty;
                if (table.ExtendedProperties.Contains(key: "EntityId"))
                {
                    entityId = (Guid)table.ExtendedProperties[key: "EntityId"];
                }
                return Result.Success<Guid, IActionResult>(value: entityId);
            }
        }
    }

    public UIResult WorkflowNext(WorkflowNextInput workflowNextInput)
    {
        if (
            sessionManager.GetSession(
                sessionFormIdentifier: workflowNextInput.SessionFormIdentifier
            )
            is WorkflowSessionStore workflowSessionStore
        )
        {
            return (UIResult)workflowSessionStore.ExecuteAction(actionId: SessionStore.ACTION_NEXT);
        }
        return null;
    }

    public RuleExceptionDataCollection WorkflowNextQuery(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier);
        return (RuleExceptionDataCollection)
            sessionStore.ExecuteAction(actionId: SessionStore.ACTION_QUERYNEXT);
    }

    public UIResult WorkflowAbort(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier);
        return (UIResult)sessionStore.ExecuteAction(actionId: SessionStore.ACTION_ABORT);
    }
#pragma warning disable 1998
    public async Task<UIResult> WorkflowRepeat(
#pragma warning restore 1998
        Guid sessionFormIdentifier,
        IStringLocalizer<SharedResources> localizer
    )
    {
        if (
            !(
                sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier)
                is WorkflowSessionStore workflowSessionStore
            )
        )
        {
            throw new Exception(message: localizer[name: "ErrorWorkflowSessionInvalid"]);
        }
        var request = new UIRequest
        {
            FormSessionId = null,
            IsStandalone = workflowSessionStore.Request.IsStandalone,
            ObjectId = workflowSessionStore.Request.ObjectId,
            Type = workflowSessionStore.Request.Type,
            Icon = workflowSessionStore.Request.Icon,
            Caption = workflowSessionStore.Request.Caption,
            Parameters = workflowSessionStore.Request.Parameters,
        };
        DestroyUI(sessionFormIdentifier: sessionFormIdentifier);
        return InitUI(request: request);
    }

    public int AttachmentCount(AttachmentCountInput input)
    {
        SecurityTools.CurrentUserProfile();
        SessionStore sessionStore = GetSessionStore(sessionId: input.SessionFormIdentifier);
        if (sessionStore == null)
        {
            return 0;
        }
        var result = 0;
        var idList = new List<object>();
        // We catch any problems with reading record ids
        // (they could have been unloaded by another request
        // and we don't want to hear messages about this).
        try
        {
            ChildrenRecordsIds(
                list: idList,
                row: sessionStore.GetSessionRow(entity: input.Entity, id: input.Id)
            );
        }
        catch
        {
            return 0;
        }
        if (idList.Count > 500)
        {
            return -1;
        }
        foreach (var recordId in idList)
        {
            var lookupService = ServiceManager.Services.GetService<IDataLookupService>();
            var oneRecordCount = (int)
                lookupService.GetDisplayText(
                    lookupId: new Guid(g: "fbf2cadd-e529-401d-80ce-d68de0a89f13"),
                    lookupValue: recordId,
                    useCache: false,
                    returnMessageIfNull: false,
                    transactionId: null
                );
            result += oneRecordCount;
        }
        return result;
    }

    public IList<Attachment> AttachmentList(AttachmentListInput input)
    {
        SecurityTools.CurrentUserProfile();
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        var result = new List<Attachment>();
        var idList = new List<object>();
        ChildrenRecordsIds(
            list: idList,
            row: sessionStore.GetSessionRow(entity: input.Entity, id: input.Id)
        );
        foreach (var recordId in idList)
        {
            var oneRecordList = CoreServices.DataService.Instance.LoadData(
                dataStructureId: new Guid(g: "44a25061-750f-4b42-a6de-09f3363f8621"),
                methodId: new Guid(g: "0fda540f-e5de-4ab6-93d2-76b0abe6fd77"),
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId: null,
                paramName1: "Attachment_parRefParentRecordId",
                paramValue1: recordId
            );
            foreach (DataRow row in oneRecordList.Tables[index: 0].Rows)
            {
                var user = "";
                if (!row.IsNull(columnName: "RecordCreatedBy"))
                {
                    try
                    {
                        var profileProvider = SecurityManager.GetProfileProvider();
                        var profile =
                            profileProvider.GetProfile(
                                profileId: (Guid)row[columnName: "RecordCreatedBy"]
                            ) as UserProfile;
                        // ReSharper disable once PossibleNullReferenceException
                        user = profile.FullName;
                    }
                    catch (Exception ex)
                    {
                        user = ex.Message;
                    }
                }
                var attachment = new Attachment
                {
                    Id = row[columnName: "Id"].ToString(),
                    CreatorName = user,
                    DateCreated = (DateTime)row[columnName: "RecordCreated"],
                    FileName = (string)row[columnName: "FileName"],
                };
                if (!row.IsNull(columnName: "Note"))
                {
                    attachment.Description = (string)row[columnName: "Note"];
                }
                var extension = Path.GetExtension(path: attachment.FileName).ToLower();
                attachment.Icon = "extensions/" + extension + ".png";
                result.Add(item: attachment);
            }
        }
        return result;
    }

    public static IList<WorkQueueInfo> WorkQueueList(IStringLocalizer<SharedResources> localizer)
    {
        try
        {
            IList<WorkQueueInfo> result = new List<WorkQueueInfo>();
            // if the user is not logged on,
            // we will gracefully finish by returning an empty list
            try
            {
                SecurityTools.CurrentUserProfile();
            }
            catch
            {
                return result;
            }
            var workQueueService = ServiceManager.Services.GetService<IWorkQueueService>();
            var lookupService = ServiceManager.Services.GetService<IDataLookupService>();
            var data = workQueueService.UserQueueList();
            var queueList = data.Tables[name: "WorkQueue"];
            foreach (DataRow row in queueList.Rows)
            {
                var workQueueId = row[columnName: "Id"];
                var workQueueClassName = (string)row[columnName: "WorkQueueClass"];
                long cnt = 0;
                if ((bool)row[columnName: "IsMessageCountDisplayed"])
                {
                    if (
                        workQueueService.WQClass(name: workQueueClassName)
                        is WorkQueueClass workQueueClass
                    )
                    {
                        cnt = (long)
                            lookupService.GetDisplayText(
                                lookupId: workQueueClass.WorkQueueItemCountLookupId,
                                lookupValue: workQueueId,
                                useCache: false,
                                returnMessageIfNull: false,
                                transactionId: null
                            );
                    }
                }
                var workQueueInfo = new WorkQueueInfo(
                    id: workQueueId.ToString(),
                    name: (string)row[columnName: "Name"],
                    countTotal: cnt
                );
                result.Add(item: workQueueInfo);
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: localizer[name: "ErrorLoadingWorkQueueList"],
                innerException: ex
            );
        }
    }

    public void ResetScreenColumnConfiguration(ResetScreenColumnConfigurationInput input)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;

        IPersistenceService persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var item = persistence.SchemaProvider.RetrieveInstance<FormReferenceMenuItem>(
            instanceId: input.ObjectInstanceId
        );
        item.Screen.ChildrenRecursive.OfType<ControlSetItem>()
            .Where(predicate: controlSetItem => controlSetItem.ControlItem.IsComplexType)
            .Select(selector: controlSetItem => controlSetItem.Id)
            .ForEach(action: screenSectionId =>
                ResetConfiguration(screenSectionId: screenSectionId, profileId: profileId)
            );
    }

    private void ResetConfiguration(Guid screenSectionId, Guid profileId)
    {
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            panelInstanceId: screenSectionId,
            workflowId: Guid.Empty,
            profileId: profileId
        );
        if (userConfig.Tables[name: "OrigamFormPanelConfig"].Rows.Count == 0)
        {
            return;
        }
        object data = userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
            columnName: "SettingsData"
        ];
        if (!(data is string strData))
        {
            OrigamPanelConfigDA.DeleteUserConfig(
                screenSectionId: screenSectionId,
                workflowId: Guid.Empty,
                profileId: profileId
            );
            return;
        }
        var xDocument = new XmlDocument();
        xDocument.LoadXml(xml: strData);
        var allNodes = xDocument.GetAllNodes();
        if (allNodes.Where(predicate: node => node.Name == "TableConfiguration").Count() < 2)
        {
            OrigamPanelConfigDA.DeleteUserConfig(
                screenSectionId: screenSectionId,
                workflowId: Guid.Empty,
                profileId: profileId
            );
            return;
        }
        var configurationNodes = allNodes.FirstOrDefault(predicate: node =>
            node.Name == "tableConfigurations"
        );
        configurationNodes
            ?.ChildNodes.Cast<XmlNode>()
            .Where(predicate: configNode =>
                configNode?.Attributes?[name: "isActive"]?.Value == "true"
                || configNode?.Attributes?[name: "name"]?.Value == ""
            )
            .ToList()
            .ForEach(action: nodeToRemove =>
                configurationNodes.RemoveChild(oldChild: nodeToRemove)
            );
        userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
            columnName: "SettingsData"
        ] = xDocument.OuterXml;
        OrigamPanelConfigDA.SaveUserConfig(
            userConfig: userConfig,
            panelInstanceId: screenSectionId,
            workflowId: Guid.Empty,
            profileId: profileId
        );
    }

    public void SaveObjectConfig(SaveObjectConfigInput input)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        WorkflowSessionStore workflowSessionStore = null;
        if (input.SessionFormIdentifier != Guid.Empty)
        {
            workflowSessionStore =
                sessionManager.GetSession(sessionFormIdentifier: input.SessionFormIdentifier)
                as WorkflowSessionStore;
        }
        var workflowId = workflowSessionStore?.WorkflowId ?? Guid.Empty;
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            panelInstanceId: input.ObjectInstanceId,
            workflowId: workflowId,
            profileId: profileId
        );
        if (userConfig.Tables[name: "OrigamFormPanelConfig"].Rows.Count == 0)
        {
            OrigamPanelConfigDA.CreatePanelConfigRow(
                configTable: userConfig.Tables[name: "OrigamFormPanelConfig"],
                panelInstanceId: input.ObjectInstanceId,
                workflowId: workflowId,
                profileId: profileId,
                defaultView: OrigamPanelViewMode.Grid
            );
        }
        var currentSettings = new XmlDocument();
        var newSettings = currentSettings.CreateDocumentFragment();
        foreach (var sectionNameAndData in input.SectionNameAndData)
        {
            var newSettingsNode = currentSettings.CreateElement(name: sectionNameAndData.Key);
            newSettingsNode.InnerXml = sectionNameAndData.Value;
            newSettings.AppendChild(newChild: newSettingsNode);
            XmlNode configNode;
            var settingsRow = userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0];
            if (settingsRow.IsNull(columnName: "SettingsData"))
            {
                configNode = currentSettings.CreateElement(name: "Configuration");
                currentSettings.AppendChild(newChild: configNode);
            }
            else
            {
                currentSettings.LoadXml(xml: (string)settingsRow[columnName: "SettingsData"]);
                configNode = currentSettings.FirstChild;
            }
            foreach (XmlNode node in configNode.SelectNodes(xpath: sectionNameAndData.Key))
            {
                node.ParentNode.RemoveChild(oldChild: node);
            }
            configNode.AppendChild(newChild: newSettings);
            settingsRow[columnName: "SettingsData"] = currentSettings.OuterXml;
            OrigamPanelConfigDA.SaveUserConfig(
                userConfig: userConfig,
                panelInstanceId: input.ObjectInstanceId,
                workflowId: workflowId,
                profileId: profileId
            );
        }
    }

    public static void SaveSplitPanelConfig(SaveSplitPanelConfigInput input)
    {
        SecurityTools.CurrentUserProfile();
        OrigamPanelColumnConfigDA.PersistColumnConfig(
            panelId: input.InstanceId,
            columnName: "splitPanel",
            position: 0,
            width: input.Position,
            hidden: false
        );
    }

    public static void SaveFavorites(SaveFavoritesInput input)
    {
        var profile = SecurityTools.CurrentUserProfile();
        // save favorites
        var favorites = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "e564c554-ca83-47eb-980d-95b4faba8fb8"),
            methodId: new Guid(g: "e468076e-a641-4b7d-b9b4-7d80ff312b1c"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "OrigamFavoritesUserConfig_parBusinessPartnerId",
            paramValue1: profile.Id
        );
        if (favorites.Tables[name: "OrigamFavoritesUserConfig"].Rows.Count > 0)
        {
            var row = favorites.Tables[name: "OrigamFavoritesUserConfig"].Rows[index: 0];
            row[columnName: "ConfigXml"] = input.ConfigXml;
            row[columnName: "RecordUpdated"] = DateTime.Now;
            row[columnName: "RecordUpdatedBy"] = profile.Id;
            row[columnName: "refBusinessPartnerId"] = profile.Id;
        }
        else
        {
            var row = favorites.Tables[name: "OrigamFavoritesUserConfig"].NewRow();
            row[columnName: "Id"] = Guid.NewGuid();
            row[columnName: "RecordCreated"] = DateTime.Now;
            row[columnName: "RecordCreatedBy"] = profile.Id;
            row[columnName: "ConfigXml"] = input.ConfigXml;
            row[columnName: "refBusinessPartnerId"] = profile.Id;
            favorites.Tables[name: "OrigamFavoritesUserConfig"].Rows.Add(row: row);
        }
        CoreServices.DataService.Instance.StoreData(
            dataStructureId: new Guid(g: "e564c554-ca83-47eb-980d-95b4faba8fb8"),
            data: favorites,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }

    public List<ChangeInfo> GetPendingChanges(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier: sessionFormIdentifier);
        var changes = sessionStore.PendingChanges;
        sessionStore.PendingChanges = null;
        return changes ?? new List<ChangeInfo>();
    }

    public List<ChangeInfo> GetChanges(ChangesInput input)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        var hasErrors = sessionStore.Data.HasErrors;
        var hasChanges = sessionStore.Data.HasChanges();
        return sessionStore.GetChanges(
            entity: input.Entity,
            id: input.RowId,
            operation: 0,
            hasErrors: hasErrors,
            hasChanges: hasChanges
        );
    }

    public static Result<Guid, IActionResult> SaveFilter(
        DataStructureEntity entity,
        SaveFilterInput input
    )
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        var storedFilter = new OrigamPanelFilter();
        var filterRow = storedFilter.PanelFilter.NewPanelFilterRow();
        filterRow.Id = Guid.NewGuid();
        filterRow.Name = input.Filter.Name;
        filterRow.IsGlobal = input.Filter.IsGlobal;
        filterRow.IsDefault = input.IsDefault;
        filterRow.PanelId = input.PanelId;
        filterRow.ProfileId = profileId;
        filterRow.RecordCreated = DateTime.Now;
        filterRow.RecordCreatedBy = profileId;
        storedFilter.PanelFilter.Rows.Add(row: filterRow);
        foreach (var filterDetail in input.Filter.Details)
        {
            if (entity.Column(name: filterDetail.Property) == null)
            {
                continue;
            }
            ConvertValues(entity: entity, filterDetail: filterDetail);
            OrigamPanelFilterDA.AddPanelFilterDetailRow(
                filterDS: storedFilter,
                profileId: profileId,
                filterId: filterRow.Id,
                columnName: filterDetail.Property,
                oper: filterDetail.Operator,
                value1: filterDetail.Value1,
                value2: filterDetail.Value2
            );
        }
        OrigamPanelFilterDA.PersistFilter(filter: storedFilter);
        return Result.Success<Guid, IActionResult>(value: filterRow.Id);
    }

    private static void ConvertValues(
        DataStructureEntity entity,
        UIGridFilterFieldConfiguration filterDetail
    )
    {
        OrigamDataType dataType = entity.Column(name: filterDetail.Property).DataType;
        switch (dataType)
        {
            case OrigamDataType.UniqueIdentifier:
            {
                var filterOperator = (FilterOperator)filterDetail.Operator;
                if (filterDetail.Value1 is string value1)
                {
                    if (Guid.TryParse(input: value1, result: out var parsedValue))
                    {
                        filterDetail.Value1 = parsedValue;
                    }
                }
                if (
                    (filterDetail.Value2 != null)
                    && (filterDetail.Value1 is string)
                    && (filterOperator != FilterOperator.Equals)
                    && (filterOperator != FilterOperator.NotEquals)
                )
                {
                    if (filterDetail.Value2 is string value2)
                    {
                        if (Guid.TryParse(input: value2, result: out var parsedValue))
                        {
                            filterDetail.Value2 = parsedValue;
                        }
                    }
                }
                break;
            }
            case OrigamDataType.Float:
            {
                if (
                    float.TryParse(
                        s: filterDetail.Value1 as string,
                        style: NumberStyles.Number | NumberStyles.Float,
                        provider: CultureInfo.InvariantCulture,
                        result: out var value1Parsed
                    )
                )
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (
                    float.TryParse(
                        s: filterDetail.Value2 as string,
                        style: NumberStyles.Number | NumberStyles.Float,
                        provider: CultureInfo.InvariantCulture,
                        result: out var value2Parsed
                    )
                )
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
            case OrigamDataType.Integer:
            {
                if (int.TryParse(s: filterDetail.Value1 as string, result: out var value1Parsed))
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (int.TryParse(s: filterDetail.Value2 as string, result: out var value2Parsed))
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
            case OrigamDataType.Long:
            {
                if (long.TryParse(s: filterDetail.Value1 as string, result: out var value1Parsed))
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (long.TryParse(s: filterDetail.Value2 as string, result: out var value2Parsed))
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
            case OrigamDataType.Currency:
            {
                if (
                    decimal.TryParse(
                        s: filterDetail.Value1 as string,
                        style: NumberStyles.Number | NumberStyles.Float,
                        provider: CultureInfo.InvariantCulture,
                        result: out var value1Parsed
                    )
                )
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (
                    decimal.TryParse(
                        s: filterDetail.Value2 as string,
                        style: NumberStyles.Number | NumberStyles.Float,
                        provider: CultureInfo.InvariantCulture,
                        result: out var value2Parsed
                    )
                )
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
        }
    }

    public static void DeleteFilter(Guid filterId)
    {
        var filter = OrigamPanelFilterDA.LoadFilter(id: filterId);
        filter.PanelFilter.Rows[index: 0].Delete();
        OrigamPanelFilterDA.PersistFilter(filter: filter);
    }

    public void SetDefaultFilter(SetDefaultFilterInput input, DataStructureEntity entity)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        Guid workflowId;
        if (
            !(
                sessionManager.GetSession(sessionFormIdentifier: input.SessionFormIdentifier)
                is WorkflowSessionStore workflowSessionStore
            )
        )
        {
            workflowId = Guid.Empty;
        }
        else
        {
            workflowId = workflowSessionStore.WorkflowId;
        }
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            panelInstanceId: input.PanelInstanceId,
            workflowId: workflowId,
            profileId: profileId
        );
        if (userConfig.Tables[name: "OrigamFormPanelConfig"].Rows.Count == 0)
        {
            OrigamPanelConfigDA.CreatePanelConfigRow(
                configTable: userConfig.Tables[name: "OrigamFormPanelConfig"],
                panelInstanceId: input.PanelInstanceId,
                workflowId: workflowId,
                profileId: profileId,
                defaultView: OrigamPanelViewMode.Form
            );
        }
        var shouldDeleteFilter = false;
        var oldFilterId = Guid.Empty;
        if (
            userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
                columnName: "refOrigamPanelFilterId"
            ] != DBNull.Value
        )
        {
            shouldDeleteFilter = true;
            oldFilterId = (Guid)
                userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
                    columnName: "refOrigamPanelFilterId"
                ];
        }
        var filterId = SaveFilter(entity: entity, input: input).Value;
        userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
            columnName: "refOrigamPanelFilterId"
        ] = filterId;
        OrigamPanelConfigDA.SaveUserConfig(
            userConfig: userConfig,
            panelInstanceId: input.PanelInstanceId,
            workflowId: workflowId,
            profileId: profileId
        );
        if (shouldDeleteFilter)
        {
            DeleteFilter(filterId: oldFilterId);
        }
    }

    public void ResetDefaultFilter(ResetDefaultFilterInput input)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        Guid workflowId;
        if (
            !(
                sessionManager.GetSession(sessionFormIdentifier: input.SessionFormIdentifier)
                is WorkflowSessionStore workflowSessionStore
            )
        )
        {
            workflowId = Guid.Empty;
        }
        else
        {
            workflowId = workflowSessionStore.WorkflowId;
        }
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            panelInstanceId: input.PanelInstanceId,
            workflowId: workflowId,
            profileId: profileId
        );
        if (
            (userConfig.Tables[name: "OrigamFormPanelConfig"].Rows.Count == 0)
            || (
                userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
                    columnName: "refOrigamPanelFilterId"
                ] == DBNull.Value
            )
        )
        {
            return;
        }
        var filterId = (Guid)
            userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
                columnName: "refOrigamPanelFilterId"
            ];
        userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
            columnName: "refOrigamPanelFilterId"
        ] = DBNull.Value;
        OrigamPanelConfigDA.SaveUserConfig(
            userConfig: userConfig,
            panelInstanceId: input.PanelInstanceId,
            workflowId: workflowId,
            profileId: profileId
        );
        DeleteFilter(filterId: filterId);
    }

    public string ReportFromMenu(Guid menuId)
    {
        return reportManager.GetReportFromMenu(menuId: menuId);
    }

    private static bool IsRowDirty(DataRow row)
    {
        if (row.RowState != DataRowState.Unchanged)
        {
            return true;
        }
        foreach (DataRelation childRelation in row.Table.ChildRelations)
        {
            if (row.GetChildRows(relation: childRelation).Any(predicate: IsRowDirty))
            {
                return true;
            }
            // look for deleted children. They aren't returned by
            // previous ChetChildRows call.
            if (
                row.GetChildRows(relation: childRelation, version: DataRowVersion.Original)
                    .Any(predicate: childRow => childRow.RowState == DataRowState.Deleted)
            )
            {
                return true;
            }
        }
        return false;
    }

    private static NotificationBox LogoNotificationBox()
    {
        var schema = ServiceManager.Services.GetService<SchemaService>();
        var provider = schema.GetProvider<NotificationBoxSchemaItemProvider>();
        NotificationBox logoNotificationBox = null;
        foreach (var abstractSchemaItem in provider.ChildItems)
        {
            var box = (NotificationBox)abstractSchemaItem;
            if (box.Type == NotificationBoxType.Logo)
            {
                logoNotificationBox = box;
            }
        }
        return logoNotificationBox;
    }

    private static bool HasChanges(SessionStore sessionStore)
    {
        var hasChanges =
            (
                (sessionStore is FormSessionStore formSessionStore)
                && !formSessionStore.MenuItem.ReadOnlyAccess
                && (sessionStore.Data != null)
                && sessionStore.Data.HasChanges()
            )
            || (
                (sessionStore is WorkflowSessionStore workflowSessionStore)
                && workflowSessionStore.AllowSave
                && (sessionStore.Data != null)
                && sessionStore.Data.HasChanges()
            );
        return hasChanges;
    }

    private void CreateUpdateOrigamOnlineUser()
    {
        var principal = SecurityManager.CurrentPrincipal;
        Task.Run(action: () =>
        {
            Thread.CurrentPrincipal = principal;
            SecurityTools.CreateUpdateOrigamOnlineUser(
                username: SecurityManager.CurrentPrincipal.Identity.Name,
                stats: sessionManager.GetSessionStats()
            );
        });
    }

    private static void ChildrenRecordsIds(List<object> list, DataRow row)
    {
        if (
            (row.Table.PrimaryKey.Length != 1) || (row.Table.PrimaryKey[0].DataType != typeof(Guid))
        )
        {
            return;
        }

        list.Add(item: row[column: row.Table.PrimaryKey[0]]);
        foreach (DataRelation childRelation in row.Table.ChildRelations)
        {
            foreach (var childRow in row.GetChildRows(relation: childRelation))
            {
                ChildrenRecordsIds(list: list, row: childRow);
            }
        }
    }

    public static XmlDocument DataRowToRecordTooltip(
        DataRow row,
        CultureInfo cultureInfo,
        IStringLocalizer<SharedResources> localizer
    )
    {
        var xmlDocument = new XmlDocument();
        var tooltipElement = xmlDocument.CreateElement(name: "tooltip");
        tooltipElement.SetAttribute(name: "title", value: row.Table.DisplayExpression);
        xmlDocument.AppendChild(newChild: tooltipElement);
        var y = 1;
        if (row.Table.Columns.Contains(name: "Id"))
        {
            CreateGenericRecordTooltipCell(
                xmlDocument: xmlDocument,
                parentElement: tooltipElement,
                y: y,
                text: "Id " + row[columnName: "Id"]
            );
            y++;
        }
        var profileProvider = SecurityManager.GetProfileProvider();
        if (
            row.Table.Columns.Contains(name: "RecordCreated")
            && !row.IsNull(columnName: "RecordCreated")
        )
        {
            string profileName;
            try
            {
                profileName = (
                    (UserProfile)
                        profileProvider.GetProfile(
                            profileId: (Guid)row[columnName: "RecordCreatedBy"]
                        )
                ).FullName;
            }
            catch
            {
                profileName = row[columnName: "RecordCreatedBy"].ToString();
            }
            CreateGenericRecordTooltipCell(
                xmlDocument: xmlDocument,
                parentElement: tooltipElement,
                y: y,
                text: string.Format(
                    format: localizer[name: "DefaultTooltipRecordCreated"],
                    arg0: profileName,
                    arg1: ((DateTime)row[columnName: "RecordCreated"]).ToString(
                        provider: cultureInfo
                    )
                )
            );
            y++;
        }
        if (!row.Table.Columns.Contains(name: "RecordUpdated"))
        {
            return xmlDocument;
        }
        if (row.IsNull(columnName: "RecordUpdated"))
        {
            CreateGenericRecordTooltipCell(
                xmlDocument: xmlDocument,
                parentElement: tooltipElement,
                y: y,
                text: localizer[name: "DefaultTooltipNoChange"]
            );
        }
        else
        {
            string profileName;
            try
            {
                profileName = (
                    (UserProfile)
                        profileProvider.GetProfile(
                            profileId: (Guid)row[columnName: "RecordUpdatedBy"]
                        )
                ).FullName;
            }
            catch
            {
                profileName = row[columnName: "RecordUpdatedBy"].ToString();
            }
            CreateGenericRecordTooltipCell(
                xmlDocument: xmlDocument,
                parentElement: tooltipElement,
                y: y,
                text: string.Format(
                    format: localizer[name: "DefaultTooltipRecordUpdated"],
                    arg0: profileName,
                    arg1: ((DateTime)row[columnName: "RecordUpdated"]).ToString(
                        provider: cultureInfo
                    )
                )
            );
        }
        return xmlDocument;
    }

    private static void CreateGenericRecordTooltipCell(
        XmlDocument xmlDocument,
        XmlElement parentElement,
        int y,
        string text
    )
    {
        var gridElement = xmlDocument.CreateElement(name: "cell");
        gridElement.SetAttribute(name: "type", value: "text");
        gridElement.SetAttribute(name: "x", value: "0");
        gridElement.SetAttribute(name: "y", value: y.ToString());
        gridElement.SetAttribute(name: "height", value: "1");
        gridElement.SetAttribute(name: "width", value: "1");
        gridElement.InnerText = text;
        parentElement.AppendChild(newChild: gridElement);
    }

    public static XmlDocument NotificationBoxContent()
    {
        SecurityTools.CurrentUserProfile();
        XmlDocument doc = null;
        NotificationBox logoNotificationBox = LogoNotificationBox();
        if (logoNotificationBox != null)
        {
            List<DataServiceDataTooltip> tooltips =
                logoNotificationBox.ChildItemsByType<DataServiceDataTooltip>(
                    itemType: DataServiceDataTooltip.CategoryConst
                );
            doc = GetTooltip(id: null, tooltips: tooltips)?.Xml;
        }
        if (doc == null)
        {
            doc = DefaultNotificationBoxContent();
        }
        return doc;
    }

    private static IXmlContainer GetTooltip(object id, List<DataServiceDataTooltip> tooltips)
    {
        tooltips.Sort();
        DataServiceDataTooltip tooltip = null;
        foreach (DataServiceDataTooltip tt in tooltips)
        {
            if (
                FeatureTools.IsFeatureOn(featureCode: tt.Features)
                && SecurityTools.IsInRole(roleName: tt.Roles)
            )
            {
                tooltip = tt;
            }
        }
        if (tooltip == null)
        {
            return null;
        }

        QueryParameterCollection qparams = new QueryParameterCollection();
        if (id != null)
        {
            foreach (string paramName in tooltip.TooltipLoadMethod.ParameterReferences.Keys)
            {
                qparams.Add(value: new QueryParameter(_parameterName: paramName, value: id));
            }
        }
        DataSet data = CoreServices.DataService.Instance.LoadData(
            dataStructureId: tooltip.TooltipDataStructureId,
            methodId: tooltip.TooltipDataStructureMethodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: qparams
        );
        IPersistenceService persistence = ServiceManager.Services.GetService<IPersistenceService>();
        IXsltEngine transformer = new CompiledXsltEngine(persistence: persistence.SchemaProvider);
        IXmlContainer result = transformer.Transform(
            data: DataDocumentFactory.New(dataSet: data),
            transformationId: tooltip.TooltipTransformationId,
            parameters: new Hashtable(),
            transactionId: null,
            outputStructure: null,
            validateOnly: false
        );
        return result;
    }

    private static XmlDocument DefaultNotificationBoxContent()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml: "<div class=\"logo-left\"><img src=\"./img/origam-logo.svg\"/></div>");
        return doc;
    }

    public void RevertChanges(RevertChangesInput input)
    {
        sessionManager
            .GetSession(sessionFormIdentifier: input.SessionFormIdentifier)
            .RevertChanges();
    }
}
