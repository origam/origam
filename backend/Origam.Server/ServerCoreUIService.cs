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

using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Origam.DA;
using Origam.Gui;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Server;
using Origam.Workbench.Services;
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
using MoreLinq;
using Origam.Extensions;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Controller;
using Origam.Server.Model.UIService;
using Origam.Service.Core;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;
// ReSharper disable once InconsistentNaming
public class ServerCoreUIService : IBasicUIService
{
    private const int INITIAL_PAGE_NUMBER_OF_RECORDS = 50;
    // ReSharper disable once InconsistentNaming
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly UIManager uiManager;
    private readonly SessionManager sessionManager;
    private readonly SessionHelper sessionHelper;
    private readonly IReportManager reportManager;
    public ServerCoreUIService(
        UIManager uiManager, SessionManager sessionManager)
    {
        this.uiManager = uiManager;
        this.sessionManager = sessionManager;
        sessionHelper = new SessionHelper(sessionManager);
        reportManager = new ServerCoreReportManager(sessionManager);
    }
    public string GetReportStandalone(
        string reportId, 
        Hashtable parameters, 
        DataReportExportFormatType dataReportExportFormatType)
    {
        return reportManager.GetReportStandalone(
            reportId, parameters, dataReportExportFormatType);
    }
    public UIResult InitUI(UIRequest request)
    {
        return uiManager.InitUI(
            request: request,
            addChildSession: false,
            parentSession: null,
            basicUIService: this);
    }
    public PortalResult InitPortal(int maxRequestLength)
    {
        OrigamUserContext.Reset();
        var profile = SecurityTools.CurrentUserProfile();
        var result = new PortalResult(MenuXmlBuilder.GetMenu());
        var settings = ConfigurationManager.GetActiveConfiguration();
        if(settings != null)
        {
            result.WorkQueueListRefreshInterval 
                = settings.WorkQueueListRefreshPeriod * 1000;
            result.Slogan = settings.Slogan;
            result.HelpUrl = settings.HelpUrl;
        }
        result.MaxRequestLength = maxRequestLength;
        var logoNotificationBox = LogoNotificationBox();
        if(logoNotificationBox != null)
        {
            result.NotificationBoxRefreshInterval 
                = logoNotificationBox.RefreshInterval * 1000;
        }
        // load favorites
        var favorites = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid("e564c554-ca83-47eb-980d-95b4faba8fb8"), 
            methodId: new Guid("e468076e-a641-4b7d-b9b4-7d80ff312b1c"), 
            defaultSetId: Guid.Empty, 
            sortSetId: Guid.Empty, 
            transactionId: null, 
            paramName1: "OrigamFavoritesUserConfig_parBusinessPartnerId", 
            paramValue1: profile.Id);
        if(favorites.Tables["OrigamFavoritesUserConfig"].Rows.Count > 0)
        {
            result.Favorites = (string)favorites
                .Tables["OrigamFavoritesUserConfig"].Rows[0]["ConfigXml"];
        }
        sessionManager.AddOrUpdatePortalSession(
            id: profile.Id, 
            addSession: id => new PortalSessionStore(profile.Id),
            updateSession: (storeId, portalSessionStore) =>    
            {
                var clearAll = portalSessionStore.ShouldBeCleared();
            // running session, we get all the form sessions
            var sessionsToDestroy = new List<Guid>();
            foreach(var mainSessionStore in portalSessionStore.FormSessions)
            {
                if(clearAll)
                {
                    sessionsToDestroy.Add(mainSessionStore.Id);
                }
                else if(sessionManager.HasFormSession(mainSessionStore.Id))
                {
                    var sessionStore = mainSessionStore.ActiveSession 
                        ?? mainSessionStore;
                    if(sessionStore is SelectionDialogSessionStore
                        || sessionStore.IsModalDialog)
                    {
                        sessionsToDestroy.Add(sessionStore.Id);
                    }
                    else
                    {
                        var askWorkflowClose = false;
                        if(sessionStore 
                            is WorkflowSessionStore workflowSessionStore)
                        {
                            askWorkflowClose 
                                = workflowSessionStore.AskWorkflowClose;
                        }
                        var hasChanges = HasChanges(sessionStore);
                        result.Sessions.Add(
                            new PortalResultSession(
                                sessionStore.Id, 
                                sessionStore.Request.ObjectId, 
                                hasChanges, 
                                sessionStore.Request.Type, 
                                sessionStore.Request.Caption, 
                                sessionStore.Request.Icon, 
                                askWorkflowClose));
                    }
                }
                else
                {
                    // session is registered in the user's portal, 
                    // but not in the UIService anymore,
                    // we have to destroy it
                    sessionsToDestroy.Add(mainSessionStore.Id);
                }
            }
            foreach(Guid id in sessionsToDestroy)
            {
                try
                {
                    DestroyUI(id);
                }
                catch(Exception ex)
                {
                    if(log.IsFatalEnabled)
                    {
                        log.LogOrigamError(
                            "Failed to destroy session " + id.ToString()
                            + ".", ex);
                    }
                }
            }
            if(clearAll)
            {
                portalSessionStore.ResetSessionStart();
            }
            return portalSessionStore;
        });
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
        MenuSchemaItemProvider menuProvider = ServiceManager.Services
            .GetService<SchemaService>()
            .GetProvider<MenuSchemaItemProvider>();
        return menuProvider.ChildItemsRecursive
            .OfType<AbstractMenuItem>()
            .Where(x => 
                x is ReportReferenceMenuItem || x is FormReferenceMenuItem)
            .FirstOrDefault(FormTools.IsFormMenuInitialScreen)
            ?.Id.ToString();
    }
    public void Logout()
    {
        PortalSessionStore pss;
        try
        {
            pss = sessionManager.GetPortalSession();
        }
        catch(SessionExpiredException)
        {
            return;
        }
        if(pss == null)
        {
            return;
        }
        while(pss.FormSessions.Count > 0)
        {
            DestroyUI(pss.FormSessions[0].Id);
        }
        Analytics.Instance.Log("UI_LOGOUT");
        sessionManager.RemovePortalSession(SecurityTools.CurrentUserProfile().Id);
        Task.Run(() => SecurityTools.RemoveOrigamOnlineUser(
            SecurityManager.CurrentPrincipal.Identity.Name));
        OrigamUserContext.Reset();
    }
    // ReSharper disable once InconsistentNaming
    public void DestroyUI(Guid sessionFormIdentifier)
    {
        sessionHelper.DeleteSession(sessionFormIdentifier);
        CreateUpdateOrigamOnlineUser();
    }
    public IDictionary<string, object> RefreshData(
        Guid sessionFormIdentifier,
        IStringLocalizer<SharedResources> localizer)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier);
        var result = sessionStore.ExecuteAction(
            SessionStore.ACTION_REFRESH);
        CreateUpdateOrigamOnlineUser();
        if(!(result is DataSet))
        {
            throw new Exception(localizer["ErrorRefreshReturnInvalid",
                sessionStore.GetType().Name, SessionStore.ACTION_REFRESH]);
        }
        IList<string> columns = null;
        if(sessionStore.IsPagedLoading)
        {
            // for lazily-loaded data we provide all the preloaded columns
            // (primary keys + all the initial sort columns)
            columns = sessionStore.DataListLoadedColumns;
        }
        return DataTools.DatasetToDictionary(result as DataSet, columns, 
            INITIAL_PAGE_NUMBER_OF_RECORDS, sessionStore.CurrentRecordId, 
            sessionStore.DataListEntity, sessionStore);
    }
    public List<ChangeInfo> RestoreData(RestoreDataInput input)
    {
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        return sessionStore.RestoreData(input.ObjectId);
    }
    public RuleExceptionDataCollection SaveDataQuery(
        Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier);
        if (sessionStore.Data.HasErrors)
        {
            throw new RuleException(Origam.Server.Resources.ErrorInForm);
        }
        if((sessionStore.ConfirmationRule == null) ||
           !sessionStore.Data.HasChanges())
        {
            return new RuleExceptionDataCollection();
        }
        var hasRows = false;
        var clone = DatasetTools.CloneDataSet(sessionStore.Data);
        var rootTables = sessionStore.Data.Tables
            .Cast<DataTable>()
            .Where(table => table.ParentRelations.Count == 0).ToList();
        foreach(var rootTable in rootTables)
        {
            foreach(DataRow row in rootTable.Rows)
            {
                if((row.RowState == DataRowState.Deleted) ||
                   !IsRowDirty(row))
                {
                    continue;
                }
                DatasetTools.GetDataSlice(
                    clone, new List<DataRow>{row});
                hasRows = true;
            }
        }
        if(!hasRows)
        {
            return new RuleExceptionDataCollection();
        }
        var xmlDoc = DataDocumentFactory.New(clone);
        return sessionStore.RuleEngine.EvaluateEndRule(
            sessionStore.ConfirmationRule, xmlDoc);
    }
    public IList SaveData(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier);
        var output = (IList)sessionStore.ExecuteAction(
            SessionStore.ACTION_SAVE);
        CreateUpdateOrigamOnlineUser();
        return output;
    }
    public IList CreateObject(CreateObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        // todo: propagate requesting grid as guid?
        IList output = sessionStore.CreateObject(
            input.Entity, input.Values, input.Parameters, 
            input.RequestingGridId.ToString());
        CreateUpdateOrigamOnlineUser();
        return output;
    }
    public IList CopyObject(CopyObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        IList output = sessionStore.CopyObject(
            input.Entity, input.OriginalId, 
            input.RequestingGridId.ToString(), input.Entities, 
            input.ForcedValues);
        CreateUpdateOrigamOnlineUser();
        return output;
    }
    public IList UpdateObject(UpdateObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        IList output = sessionStore.UpdateObjectBatch(
            input.Entity, input.UpdateData);
        CreateUpdateOrigamOnlineUser();
        return output;
    }
    public IList DeleteObject(DeleteObjectInput input)
    {
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        IList output = sessionStore.DeleteObject(input.Entity, input.Id);
        CreateUpdateOrigamOnlineUser();
        return output;
    }
    
    public IList DeleteObjectInOrderedList(DeleteObjectInOrderedListInput input)
    {
        SessionStore ss = sessionManager.GetSession(new Guid(input.SessionFormIdentifier));
        List<ChangeInfo> changes = ss.UpdateObjectBatch(input.Entity, input.OrderProperty, input.UpdatedOrderValues);
        changes.AddRange(ss.DeleteObject(input.Entity, input.Id));
        Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
            SecurityManager.CurrentPrincipal.Identity.Name,
            sessionManager.GetSessionStats()));
        return changes;
    }
    public List<ChangeInfo> GetRowData(MasterRecordInput input)
    {
        SessionStore sessionStore = GetSessionStore(input.SessionFormIdentifier);
        if(sessionStore == null)
        {
            return new List<ChangeInfo>();
        }
        return sessionStore.GetRowData(
            input.Entity, input.RowId, false);
    } 
    public ChangeInfo GetRow(MasterRecordInput input)
    {
        return 
            GetSessionStore(input.SessionFormIdentifier)
            ?.GetRow(input.Entity, input.RowId);
    }
    public IDictionary GetParameters(Guid sessionFormIdentifier)
    {
        if(sessionFormIdentifier == Guid.Empty)
        {
            return new Hashtable();
        }
        SessionStore sessionStore = GetSessionStore(sessionFormIdentifier);
        return sessionStore == null 
            ? new Hashtable() : sessionStore.Request.Parameters;
    }
    public List<List<object>> GetData(GetDataInput input)
    {
        SessionStore sessionStore = GetSessionStore(input.SessionFormIdentifier);
        if(sessionStore == null)
        {
            return new List<List<object>>();
        }
        return sessionStore.GetData(
            input.ChildEntity, 
            input.ParentRecordId, 
            input.RootRecordId);
    }
    private SessionStore GetSessionStore(Guid sessionId)
    {
        try
        {
            return sessionManager.GetSession(sessionId);
        }
        catch (Exception ex)
        {
            log.Warn(ex.Message, ex);
            return null;
        }
    }
    public IList RowStates(RowStatesInput input)
    {
	    var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        return sessionStore.RowStates(input.Entity, input.Ids);
    }
    public RuleExceptionDataCollection ExecuteActionQuery(
        ExecuteActionQueryInput input)
    {
        EntityUIAction action = GetAction(input.ActionId);
        // work queue commands are treated as actions,
        // but they're not part of the model,
        // so the GetAction can return null
        // the subsequent code needs to be able to handle it
        if (action is EntityMenuAction menuAction)
        {
            bool isAuthorized = SecurityManager.GetAuthorizationProvider().Authorize(
                SecurityManager.CurrentPrincipal, menuAction.Menu.Roles);
            if (!isAuthorized)
            {
                return new RuleExceptionDataCollection(new []
                {
                    new RuleExceptionData(string.Format(
                        Origam.Server.Resources.MenuNotAuthorized, menuAction.Menu.NodeText)
                    )
                });
            }
        }
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        // check if action is on data list entity
        // we can't execute multiple checkboxes action on list entity,
        // but it is OK to execute them on details
        if (sessionStore.IsDelayedLoading 
            && (action is EntityWorkflowAction workflowAction)
            && (action.Mode == PanelActionMode.MultipleCheckboxes)
            && (sessionStore.DataListEntity == input.Entity)
            && (workflowAction.MergeType != ServiceOutputMethod.Ignore))
        {
            throw new Exception("Only actions with merge type Ignore can be invoked in lazily loaded screens.");
        }
        if(action?.ConfirmationRule == null)
        {
            return new RuleExceptionDataCollection();
        }
        List<DataRow> rows = sessionStore.GetRows(input.Entity, input.SelectedIds);
        IXmlContainer xml 
            = DatasetTools.GetRowXml(rows, DataRowVersion.Default);
        var result = sessionStore.RuleEngine.EvaluateEndRule(
                action.ConfirmationRule, xml);
        return result ?? new RuleExceptionDataCollection();
    }
    public IList ExecuteAction(ExecuteActionInput input)
    {
        EntityUIAction action = GetAction(input.ActionId);
        if (action is EntityMenuAction menuAction)
        {
            bool isAuthorized = SecurityManager.GetAuthorizationProvider().Authorize(
                SecurityManager.CurrentPrincipal, menuAction.Menu.Roles);
            if (!isAuthorized)
            {
                throw new OrigamSecurityException(string.Format(
                    Resources.MenuNotAuthorized,
                    menuAction.Menu.NodeText)
                );
            }
        }
        var actionRunnerClient 
            = new ServerEntityUIActionRunnerClient(
                sessionManager, input.SessionFormIdentifier.ToString());
        var actionRunner = new ServerCoreEntityUIActionRunner( 
            actionRunnerClient: actionRunnerClient,
            uiManager: uiManager,  
            sessionManager: sessionManager,
            basicUIService: this,
            reportManager: reportManager);
        return actionRunner.ExecuteAction(
            input.SessionFormIdentifier.ToString(), 
            input.RequestingGrid.ToString(), 
            input.Entity,
            input.ActionType,
            input.ActionId, 
            input.ParameterMappings,
            input.SelectedIds, 
            input.InputParameters);
    }
    private static EntityUIAction GetAction(string actionId)
    {
        try
        {
           return UIActionTools.GetAction(actionId);
        }
        catch
        {
            // ignored
        }
        return null;
    }
    public Result<RowData, IActionResult> GetRow(
        Guid sessionFormIdentifier, string entity, 
        DataStructureEntity dataStructureEntity, Guid rowId)
    {
        SessionStore sessionStore = GetSessionStore(sessionFormIdentifier);
        switch(sessionStore)
        {
            case null:
                return Result.Success<RowData, IActionResult>(
                    new RowData{Row = null, Entity = null});
            default:
            {
                var row = sessionStore.GetSessionRow(entity, rowId);
                return Result.Success<RowData, IActionResult>(
                    new RowData{Row = row, Entity = dataStructureEntity});
            }
        }
    }
    public Result<Guid, IActionResult> GetEntityId(
        Guid sessionFormIdentifier, string entity)
    {
        SessionStore sessionStore = GetSessionStore(sessionFormIdentifier);
        switch(sessionStore)
        {
            case null:
                return Result.Success<Guid, IActionResult>(Guid.Empty);
            default:
            {
                var table = sessionStore.GetDataTable(entity, sessionStore.Data);
                var entityId = Guid.Empty;
                if(table.ExtendedProperties.Contains("EntityId"))
                {
                    entityId = (Guid)table.ExtendedProperties["EntityId"];
                }
                return Result.Success<Guid, IActionResult>(entityId);
            }
        }
    }
    public UIResult WorkflowNext(WorkflowNextInput workflowNextInput)
    {
        if(sessionManager.GetSession(
                workflowNextInput.SessionFormIdentifier)
            is WorkflowSessionStore workflowSessionStore)
        {
            return (UIResult) workflowSessionStore.ExecuteAction(
                SessionStore.ACTION_NEXT);
        }
        return null;
    }
    public RuleExceptionDataCollection WorkflowNextQuery(
        Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier);
        return (RuleExceptionDataCollection)sessionStore.ExecuteAction(
            SessionStore.ACTION_QUERYNEXT);
    }
    public UIResult WorkflowAbort(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(
            sessionFormIdentifier);
        return (UIResult)sessionStore.ExecuteAction(
            SessionStore.ACTION_ABORT); 
    }
#pragma warning disable 1998
    public async Task<UIResult> WorkflowRepeat(
#pragma warning restore 1998
        Guid sessionFormIdentifier,
        IStringLocalizer<SharedResources> localizer)
    {
        if(!(sessionManager.GetSession(sessionFormIdentifier) 
            is WorkflowSessionStore workflowSessionStore))
        {
            throw new Exception(localizer["ErrorWorkflowSessionInvalid"]);
        }
        var request = new UIRequest
        {
            FormSessionId = null,
            IsStandalone = workflowSessionStore.Request.IsStandalone,
            ObjectId = workflowSessionStore.Request.ObjectId,
            Type = workflowSessionStore.Request.Type,
            Icon = workflowSessionStore.Request.Icon,
            Caption = workflowSessionStore.Request.Caption,
            Parameters = workflowSessionStore.Request.Parameters
        };
        DestroyUI(sessionFormIdentifier);
        return InitUI(request);
    }
    public int AttachmentCount(AttachmentCountInput input)
    {
        SecurityTools.CurrentUserProfile();
        SessionStore sessionStore = GetSessionStore(input.SessionFormIdentifier);
        if(sessionStore == null)
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
            ChildrenRecordsIds(idList, sessionStore.GetSessionRow(
                input.Entity, input.Id));
        }
        catch
        {
            return 0;
        }
        if(idList.Count > 500)
        {
            return -1;
        }
        foreach(var recordId in idList)
        {
            var lookupService 
                = ServiceManager.Services.GetService<IDataLookupService>();
            var oneRecordCount = (int)lookupService.GetDisplayText(
                lookupId: new Guid("fbf2cadd-e529-401d-80ce-d68de0a89f13"), 
                lookupValue: recordId, 
                useCache: false, 
                returnMessageIfNull: false, 
                transactionId: null);
            result += oneRecordCount;
        }
        return result;
    }
    public IList<Attachment> AttachmentList(AttachmentListInput input)
    {
        SecurityTools.CurrentUserProfile();
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        var result = new List<Attachment>();
        var idList = new List<object>();
        ChildrenRecordsIds(idList, sessionStore.GetSessionRow(
            input.Entity, input.Id));
        foreach(var recordId in idList)
        {
            var oneRecordList = CoreServices.DataService.Instance.LoadData(
                dataStructureId: new Guid("44a25061-750f-4b42-a6de-09f3363f8621"), 
                methodId: new Guid("0fda540f-e5de-4ab6-93d2-76b0abe6fd77"), 
                defaultSetId: Guid.Empty, 
                sortSetId: Guid.Empty, 
                transactionId: null, 
                paramName1: "Attachment_parRefParentRecordId", 
                paramValue1: recordId);
            foreach(DataRow row in oneRecordList.Tables[0].Rows)
            {
                var user = "";
                if(!row.IsNull("RecordCreatedBy"))
                {
                    try
                    {
                        var profileProvider 
                            = SecurityManager.GetProfileProvider();
                        var profile = profileProvider.GetProfile(
                            (Guid)row["RecordCreatedBy"]) as UserProfile;
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
                    Id = row["Id"].ToString(),
                    CreatorName = user,
                    DateCreated = (DateTime)row["RecordCreated"],
                    FileName = (string)row["FileName"]
                };
                if(!row.IsNull("Note"))
                {
                    attachment.Description = (string)row["Note"];
                }
                var extension 
                    = Path.GetExtension(attachment.FileName).ToLower();
                attachment.Icon = "extensions/" + extension + ".png";
                result.Add(attachment);
            }
        }
        return result;
    }
    public static IList<WorkQueueInfo> WorkQueueList(
        IStringLocalizer<SharedResources> localizer)
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
            var workQueueService 
                = ServiceManager.Services.GetService<IWorkQueueService>();
            var lookupService 
                = ServiceManager.Services.GetService<IDataLookupService>();
            var data = workQueueService.UserQueueList();
            var queueList = data.Tables["WorkQueue"];
            foreach(DataRow row in queueList.Rows)
            {
                var workQueueId = row["Id"];
                var workQueueClassName = (string)row["WorkQueueClass"];
                long cnt = 0;
                if((bool)row["IsMessageCountDisplayed"])
                {
                    if(workQueueService.WQClass(workQueueClassName) 
                        is WorkQueueClass workQueueClass)
                        cnt = (long) lookupService.GetDisplayText(
                            workQueueClass.WorkQueueItemCountLookupId,
                            workQueueId, false, false, null);
                }
                var workQueueInfo = new WorkQueueInfo(
                    workQueueId.ToString(), (string)row["Name"], cnt);
                result.Add(workQueueInfo);
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(localizer["ErrorLoadingWorkQueueList"], ex);
        }
    }
    public void ResetScreenColumnConfiguration(ResetScreenColumnConfigurationInput input)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        
        IPersistenceService persistence = 
            ServiceManager.Services.GetService<IPersistenceService>();
        var item = persistence.SchemaProvider.RetrieveInstance<FormReferenceMenuItem>(input.ObjectInstanceId);
        item.Screen.ChildrenRecursive
            .OfType<ControlSetItem>()
            .Where(controlSetItem => controlSetItem.ControlItem.IsComplexType)
            .Select(controlSetItem => controlSetItem.Id)
            .ForEach(screenSectionId => ResetConfiguration(screenSectionId, profileId));
    }
    private void ResetConfiguration(Guid screenSectionId, Guid profileId)
    {
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            screenSectionId, Guid.Empty, profileId);
        if(userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
        {
            return;
        }
        object data = userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["SettingsData"];
        if (!(data is string strData))
        {
            OrigamPanelConfigDA.DeleteUserConfig(screenSectionId, Guid.Empty, profileId);
            return;
        }
        var xDocument = new XmlDocument();
        xDocument.LoadXml(strData);
        var allNodes = xDocument
            .GetAllNodes();
        if (allNodes.Where(node =>
                node.Name == "TableConfiguration").Count() < 2)
        {
            OrigamPanelConfigDA.DeleteUserConfig(screenSectionId, Guid.Empty, profileId);
            return;
        }
        var configurationNodes = allNodes
            .FirstOrDefault(node =>
                node.Name == "tableConfigurations");
        configurationNodes?.ChildNodes
            .Cast<XmlNode>()
            .Where(configNode =>
                configNode?.Attributes?["isActive"]?.Value == "true" ||
                configNode?.Attributes?["name"]?.Value == ""
            )
            .ToList()
            .ForEach(nodeToRemove => configurationNodes.RemoveChild(nodeToRemove));
        userConfig.Tables["OrigamFormPanelConfig"].Rows[0][
            "SettingsData"] = xDocument.OuterXml;
        OrigamPanelConfigDA.SaveUserConfig(
            userConfig, screenSectionId, Guid.Empty, profileId);
    }
    public void SaveObjectConfig(SaveObjectConfigInput input)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        WorkflowSessionStore workflowSessionStore = null;
        if(input.SessionFormIdentifier != Guid.Empty)
        {
            workflowSessionStore = sessionManager.GetSession(
                input.SessionFormIdentifier) as WorkflowSessionStore;
        }
        var workflowId = workflowSessionStore?.WorkflowId ?? Guid.Empty;
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            input.ObjectInstanceId, workflowId, profileId);
        if(userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
        {
            OrigamPanelConfigDA.CreatePanelConfigRow(
                userConfig.Tables["OrigamFormPanelConfig"], 
                input.ObjectInstanceId, workflowId, profileId, 
                OrigamPanelViewMode.Grid);
        }
        var currentSettings = new XmlDocument();
        var newSettings = currentSettings.CreateDocumentFragment();
        foreach (var sectionNameAndData in input.SectionNameAndData)
        {
            var newSettingsNode = currentSettings.CreateElement(
                sectionNameAndData.Key);
            newSettingsNode.InnerXml = sectionNameAndData.Value;
            newSettings.AppendChild(newSettingsNode);
            XmlNode configNode;
            var settingsRow 
                = userConfig.Tables["OrigamFormPanelConfig"].Rows[0];
            if(settingsRow.IsNull("SettingsData"))
            {
                configNode = currentSettings.CreateElement("Configuration");
                currentSettings.AppendChild(configNode);
            }
            else
            {
                currentSettings.LoadXml((string)settingsRow["SettingsData"]);
                configNode = currentSettings.FirstChild;
            }
            foreach(XmlNode node in configNode.SelectNodes(sectionNameAndData.Key))
            {
                node.ParentNode.RemoveChild(node);
            }
            configNode.AppendChild(newSettings);
            settingsRow["SettingsData"] = currentSettings.OuterXml;
            OrigamPanelConfigDA.SaveUserConfig(
                userConfig, input.ObjectInstanceId, workflowId, profileId);
        }
    }
    public static void SaveSplitPanelConfig(SaveSplitPanelConfigInput input)
    {
        SecurityTools.CurrentUserProfile();
        OrigamPanelColumnConfigDA.PersistColumnConfig(
            input.InstanceId, "splitPanel", 0, 
            input.Position, false);
    }
    public static void SaveFavorites(SaveFavoritesInput input)
    {
        var profile = SecurityTools.CurrentUserProfile();
        // save favorites
        var favorites = CoreServices.DataService.Instance.LoadData(
            new Guid("e564c554-ca83-47eb-980d-95b4faba8fb8"), 
            new Guid("e468076e-a641-4b7d-b9b4-7d80ff312b1c"), 
            Guid.Empty, 
            Guid.Empty, 
            null, 
            "OrigamFavoritesUserConfig_parBusinessPartnerId", 
            profile.Id);
        if(favorites.Tables["OrigamFavoritesUserConfig"].Rows.Count > 0)
        {
            var row = favorites.Tables["OrigamFavoritesUserConfig"].Rows[0];
            row["ConfigXml"] = input.ConfigXml;
            row["RecordUpdated"] = DateTime.Now;
            row["RecordUpdatedBy"] = profile.Id;
            row["refBusinessPartnerId"] = profile.Id;
        }
        else
        {
            var row = favorites.Tables["OrigamFavoritesUserConfig"]
                .NewRow();
            row["Id"] = Guid.NewGuid();
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = profile.Id;
            row["ConfigXml"] = input.ConfigXml;
            row["refBusinessPartnerId"] = profile.Id;
            favorites.Tables["OrigamFavoritesUserConfig"].Rows.Add(row);
        }
        CoreServices.DataService.Instance.StoreData(
            new Guid("e564c554-ca83-47eb-980d-95b4faba8fb8"), 
            favorites, false, null);
    }
    public List<ChangeInfo> GetPendingChanges(Guid sessionFormIdentifier)
    {
        var sessionStore = sessionManager.GetSession(sessionFormIdentifier);
        var changes = sessionStore.PendingChanges;
        sessionStore.PendingChanges = null;
        return changes ?? new List<ChangeInfo>();
    }
    public List<ChangeInfo> GetChanges(ChangesInput input)
    {
        var sessionStore = sessionManager.GetSession(
            input.SessionFormIdentifier);
        var hasErrors = sessionStore.Data.HasErrors;
        var hasChanges = sessionStore.Data.HasChanges();
        return sessionStore.GetChanges(
            input.Entity, input.RowId, 0, hasErrors, hasChanges);
    }
    public static Result<Guid, IActionResult> SaveFilter(
        DataStructureEntity entity, SaveFilterInput input)
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
        storedFilter.PanelFilter.Rows.Add(filterRow);
        foreach(var filterDetail in input.Filter.Details)
        {
            if(entity.Column(filterDetail.Property) == null)
            {
                continue;
            }
            ConvertValues(entity, filterDetail);
            OrigamPanelFilterDA.AddPanelFilterDetailRow(
                storedFilter, profileId, filterRow.Id, 
                filterDetail.Property, filterDetail.Operator, 
                filterDetail.Value1, filterDetail.Value2);
        }
        OrigamPanelFilterDA.PersistFilter(storedFilter);
        return Result.Success<Guid, IActionResult>(filterRow.Id);
    }
    private static void ConvertValues(DataStructureEntity entity,
        UIGridFilterFieldConfiguration filterDetail)
    {
        OrigamDataType dataType = entity.Column(filterDetail.Property).DataType;
        switch (dataType)
        {
            case OrigamDataType.UniqueIdentifier:
            {
                var filterOperator = (FilterOperator)filterDetail.Operator;
                if (filterDetail.Value1 is string value1)
                {
                    if (Guid.TryParse(value1, out var parsedValue))
                    {
                        filterDetail.Value1 = parsedValue;
                    }
                }
                if ((filterDetail.Value2 != null)
                    && (filterDetail.Value1 is string)
                    && (filterOperator != FilterOperator.Equals)
                    && (filterOperator != FilterOperator.NotEquals))
                {
                    if (filterDetail.Value2 is string value2)
                    {
                        if (Guid.TryParse(value2, out var parsedValue))
                        {
                            filterDetail.Value2 = parsedValue;
                        }
                    }
                }
                break;
            }
            case OrigamDataType.Float:
            {
                if (float.TryParse(filterDetail.Value1 as string,
                    NumberStyles.Number | NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var value1Parsed ))
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (float.TryParse(filterDetail.Value2 as string,
                    NumberStyles.Number | NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var value2Parsed ))
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
            case OrigamDataType.Integer:
            {
                if (int.TryParse(filterDetail.Value1 as string,
                    out var value1Parsed))
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (int.TryParse(filterDetail.Value2 as string,
                    out var value2Parsed))
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
            case OrigamDataType.Long:
            {
                if (long.TryParse(filterDetail.Value1 as string,
                    out var value1Parsed))
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (long.TryParse(filterDetail.Value2 as string,
                    out var value2Parsed))
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
            case OrigamDataType.Currency:
            {
                if (decimal.TryParse(filterDetail.Value1 as string,
                    NumberStyles.Number | NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value1Parsed))
                {
                    filterDetail.Value1 = value1Parsed;
                }
                if (decimal.TryParse(filterDetail.Value2 as string,
                    NumberStyles.Number | NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value2Parsed))
                {
                    filterDetail.Value2 = value2Parsed;
                }
                break;
            }
        }
    }
    public static void DeleteFilter(Guid filterId)
    {
        var filter = OrigamPanelFilterDA.LoadFilter(filterId);
        filter.PanelFilter.Rows[0].Delete();
        OrigamPanelFilterDA.PersistFilter(filter);
    }
    public void SetDefaultFilter(
        SetDefaultFilterInput input, DataStructureEntity entity)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        Guid workflowId;
        if(!(sessionManager.GetSession(input.SessionFormIdentifier) 
            is WorkflowSessionStore workflowSessionStore))
        {
            workflowId = Guid.Empty;
        }
        else
        {
            workflowId = workflowSessionStore.WorkflowId;
        }
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            input.PanelInstanceId, workflowId, profileId);
        if(userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
        {
            OrigamPanelConfigDA.CreatePanelConfigRow(
                userConfig.Tables["OrigamFormPanelConfig"], 
                input.PanelInstanceId, 
                workflowId, 
                profileId, 
                OrigamPanelViewMode.Form);
        }
        var shouldDeleteFilter = false;
        var oldFilterId = Guid.Empty;
        if(userConfig.Tables["OrigamFormPanelConfig"]
            .Rows[0]["refOrigamPanelFilterId"] 
        != DBNull.Value)
        {
            shouldDeleteFilter = true;
            oldFilterId = (Guid)userConfig.Tables["OrigamFormPanelConfig"]
                .Rows[0]["refOrigamPanelFilterId"];
        }
        var filterId = SaveFilter(entity, input).Value;
        userConfig.Tables["OrigamFormPanelConfig"]
            .Rows[0]["refOrigamPanelFilterId"] = filterId;
        OrigamPanelConfigDA.SaveUserConfig(
            userConfig, input.PanelInstanceId, workflowId, profileId);
        if(shouldDeleteFilter)
        {
            DeleteFilter(oldFilterId);
        }
    }
    public void ResetDefaultFilter(ResetDefaultFilterInput input)
    {
        var profileId = SecurityTools.CurrentUserProfile().Id;
        Guid workflowId;
        if(!(sessionManager.GetSession(input.SessionFormIdentifier) 
            is WorkflowSessionStore workflowSessionStore))
        {
            workflowId = Guid.Empty;
        }
        else
        {
            workflowId = workflowSessionStore.WorkflowId;
        }
        var userConfig = OrigamPanelConfigDA.LoadConfigData(
            input.PanelInstanceId, workflowId, profileId);
        if((userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
        || (userConfig.Tables["OrigamFormPanelConfig"]
            .Rows[0]["refOrigamPanelFilterId"] 
        == DBNull.Value))
        {
            return;
        }
        var filterId = (Guid)userConfig.Tables["OrigamFormPanelConfig"]
            .Rows[0]["refOrigamPanelFilterId"];
        userConfig.Tables["OrigamFormPanelConfig"]
            .Rows[0]["refOrigamPanelFilterId"] = DBNull.Value;
        OrigamPanelConfigDA.SaveUserConfig(
            userConfig, input.PanelInstanceId, workflowId, profileId);
        DeleteFilter(filterId);
    }
    public string ReportFromMenu(Guid menuId)
    {
        return reportManager.GetReportFromMenu(menuId);
    }
    private static bool IsRowDirty(DataRow row)
    {
        if(row.RowState != DataRowState.Unchanged)
        {
            return true;
        }
        foreach(DataRelation childRelation in row.Table.ChildRelations)
        {
            if(row.GetChildRows(childRelation).Any(IsRowDirty))
            {
                return true;
            }
            // look for deleted children. They aren't returned by
			// previous ChetChildRows call. 
            if(row.GetChildRows(childRelation,
                DataRowVersion.Original).Any(
                childRow => childRow.RowState == DataRowState.Deleted))
            {
                return true;
            }
        }
        return false;
    }
    private static NotificationBox LogoNotificationBox()
    {
        var schema = ServiceManager.Services.GetService<SchemaService>();
        var provider 
            = schema.GetProvider<NotificationBoxSchemaItemProvider>();
        NotificationBox logoNotificationBox = null;
        foreach(var abstractSchemaItem in provider.ChildItems)
        {
            var box = (NotificationBox) abstractSchemaItem;
            if(box.Type == NotificationBoxType.Logo)
            {
                logoNotificationBox = box;
            }
        }
        return logoNotificationBox;
    }
    private static bool HasChanges(SessionStore sessionStore)
    {
        var hasChanges 
            = ((sessionStore is FormSessionStore formSessionStore) 
            && !formSessionStore.MenuItem.ReadOnlyAccess && (sessionStore.Data != null) 
            && sessionStore.Data.HasChanges()) 
            || ((sessionStore is WorkflowSessionStore workflowSessionStore)
            && workflowSessionStore.AllowSave 
            && (sessionStore.Data != null) 
            && sessionStore.Data.HasChanges());
        return hasChanges;
    }
    private void CreateUpdateOrigamOnlineUser()
    {
        var principal = SecurityManager.CurrentPrincipal;
        Task.Run(() =>
        {
            Thread.CurrentPrincipal = principal;
            SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats());
        });
    }
    private static void ChildrenRecordsIds(List<object> list, DataRow row)
    {
        if((row.Table.PrimaryKey.Length != 1) ||
            (row.Table.PrimaryKey[0].DataType != typeof(Guid))) return;
        list.Add(row[row.Table.PrimaryKey[0]]);
        foreach(DataRelation childRelation in row.Table.ChildRelations)
        {
            foreach(var childRow in row.GetChildRows(childRelation))
            {
                ChildrenRecordsIds(list, childRow);
            }
        }
    } 
    public static XmlDocument DataRowToRecordTooltip(
        DataRow row, 
        CultureInfo cultureInfo,
        IStringLocalizer<SharedResources> localizer)
    {
        var xmlDocument = new XmlDocument();
        var tooltipElement = xmlDocument.CreateElement("tooltip");
        tooltipElement.SetAttribute("title", row.Table.DisplayExpression);
        xmlDocument.AppendChild(tooltipElement);
        var y = 1;
        if(row.Table.Columns.Contains("Id"))
        {
            CreateGenericRecordTooltipCell(
                xmlDocument, tooltipElement, y, "Id " + row["Id"]);
            y++;
        }
        var profileProvider = SecurityManager.GetProfileProvider();
        if(row.Table.Columns.Contains("RecordCreated") 
            && !row.IsNull("RecordCreated"))
        {
            string profileName;
            try
            {
                profileName = ((UserProfile)profileProvider.GetProfile(
                        (Guid)row["RecordCreatedBy"])).FullName;
            }
            catch
            {
                profileName = row["RecordCreatedBy"].ToString();
            }
            CreateGenericRecordTooltipCell(
                xmlDocument, tooltipElement, y, 
                string.Format(
                    localizer["DefaultTooltipRecordCreated"], profileName, 
                    ((DateTime)row["RecordCreated"]).ToString(
                        cultureInfo)));
            y++;
        }
        if(!row.Table.Columns.Contains("RecordUpdated"))
        {
            return xmlDocument;
        }
        if(row.IsNull("RecordUpdated"))
        {
            CreateGenericRecordTooltipCell(
                xmlDocument, tooltipElement, y, 
                localizer["DefaultTooltipNoChange"]);
        }
        else
        {
            string profileName;
            try
            {
                profileName = ((UserProfile)profileProvider.GetProfile(
                    (Guid)row["RecordUpdatedBy"])).FullName;
            }
            catch
            {
                profileName = row["RecordUpdatedBy"].ToString();
            }
            CreateGenericRecordTooltipCell(
                xmlDocument, tooltipElement, y, 
                string.Format(localizer["DefaultTooltipRecordUpdated"], 
                profileName, ((DateTime)row["RecordUpdated"]).ToString(
                    cultureInfo)));
        }
        return xmlDocument;
    }
    private static void CreateGenericRecordTooltipCell(
        XmlDocument xmlDocument, XmlElement parentElement, 
        int y, string text)
    {
        var gridElement = xmlDocument.CreateElement("cell");
        gridElement.SetAttribute("type", "text");
        gridElement.SetAttribute("x", "0");
        gridElement.SetAttribute("y", y.ToString());
        gridElement.SetAttribute("height", "1");
        gridElement.SetAttribute("width", "1");
        gridElement.InnerText = text;
        parentElement.AppendChild(gridElement);
    }
    public static XmlDocument NotificationBoxContent()
    {
        SecurityTools.CurrentUserProfile();
        XmlDocument doc = null;
        NotificationBox logoNotificationBox = LogoNotificationBox();
        if (logoNotificationBox != null)
        {
            List<DataServiceDataTooltip> tooltips = logoNotificationBox.ChildItemsByType<DataServiceDataTooltip>(
                DataServiceDataTooltip.CategoryConst);
            doc = GetTooltip(null, tooltips)?.Xml;
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
            if (IsFeatureOn(tt.Features) 
                && IsInRole(tt.Roles))
            {
                tooltip = tt;
            }
        }
        if (tooltip == null) return null;
        QueryParameterCollection qparams = new QueryParameterCollection();
        if (id != null)
        {
            foreach (string paramName in
                tooltip.TooltipLoadMethod.ParameterReferences.Keys)
            {
                qparams.Add(new QueryParameter(paramName, id));
            }
        }
        DataSet data = CoreServices.DataService.Instance.LoadData(
            tooltip.TooltipDataStructureId, 
            tooltip.TooltipDataStructureMethodId, 
            Guid.Empty, Guid.Empty, null, qparams);
        IPersistenceService persistence = 
            ServiceManager.Services.GetService<IPersistenceService>();
        IXsltEngine transformer = new CompiledXsltEngine(persistence.SchemaProvider);
        IXmlContainer result = transformer.Transform(
            DataDocumentFactory.New(data), 
            tooltip.TooltipTransformationId, 
            new Hashtable(), null,
            null, false);
        return result;
    }
    
    private static bool IsFeatureOn(string featureCode)
    {
        return ServiceManager.Services
            .GetService<IParameterService>()
            .IsFeatureOn(featureCode);
    }
    private static bool IsInRole(string roleName)
    {
        return SecurityManager
            .GetAuthorizationProvider()
            .Authorize(SecurityManager.CurrentPrincipal, roleName);
    }
    private static XmlDocument DefaultNotificationBoxContent()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml("<div class=\"logo-left\"><img src=\"./img/origam-logo.svg\"/></div>");
        return doc;
    }
    public void RevertChanges(RevertChangesInput input)
    {
        sessionManager
            .GetSession(input.SessionFormIdentifier)
            .RevertChanges();
    }
}
