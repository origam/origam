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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origam.DA;
using Origam.Extensions;
using Origam.Gui;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Server.Common;
using Origam.Service.Core;
using Origam.Workbench;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class UIManager
{
    internal static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private readonly int initialPageNumberOfRecords;
    private readonly SessionManager sessionManager;
    private readonly Analytics analytics;

    public UIManager(
        int initialPageNumberOfRecords,
        SessionManager sessionManager,
        Analytics analytics
    )
    {
        this.analytics = analytics;
        this.initialPageNumberOfRecords = initialPageNumberOfRecords;
        this.sessionManager = sessionManager;
    }

    public UIResult InitUI(
        UIRequest request,
        bool addChildSession,
        SessionStore parentSession,
        IBasicUIService basicUIService
    )
    {
        Task getFormXmlTask = null;
        // Stack can't handle resending DataDocumentFx,
        // it needs to be converted to XmlDocument
        // and then converted back to DataDocumentFx
        foreach (object key in request.Parameters.Keys.CastToList<object>())
        {
            object value = request.Parameters[key: key];
            if (value is XmlDocument xmlDoc)
            {
                request.Parameters[key: key] = new XmlContainer(xmlDocument: xmlDoc);
            }
            if (value is JObject jobj)
            {
                request.Parameters[key: key] = new XmlContainer(
                    xmlDocument: JsonConvert.DeserializeXmlNode(value: jobj.ToString())
                );
            }
        }
        bool isExclusive = false;
        if (request.IsNewSession)
        {
            PortalSessionStore pss = sessionManager.GetPortalSession();
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            if (settings.MaxOpenTabs > 0)
            {
                if (pss.FormSessions.Count >= settings.MaxOpenTabs)
                {
                    throw new RuleException(message: Resources.ErrorTooManyTabsOpen);
                }
            }
            if (!request.ObjectId.Contains(value: "|"))
            {
                if (
                    request.Type != UIRequestType.Dashboard
                    && request.Type != UIRequestType.WorkQueue
                    && Guid.TryParse(input: request.ObjectId, result: out var objectId)
                )
                {
                    IPersistenceService persistence =
                        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                        as IPersistenceService;
                    AbstractMenuItem menu =
                        persistence.SchemaProvider.RetrieveInstance(
                            type: typeof(AbstractMenuItem),
                            primaryKey: new ModelElementKey(id: objectId)
                        ) as AbstractMenuItem;
                    if (menu != null) // can be a workflow
                    {
                        isExclusive = menu.OpenExclusively;
                        if (menu.OpenExclusively && pss.FormSessions.Count > 0)
                        {
                            throw new RuleException(
                                message: string.Format(
                                    format: Resources.ErrorCannotOpenFormExclusively,
                                    arg0: request.Caption
                                )
                            );
                        }
                    }
                }
            }
        }
        UserProfile profile = SecurityTools.CurrentUserProfile();
        if (request.Type == UIRequestType.Dashboard)
        {
            return InitDashboardView(request: request);
        }
        SessionStore ss;
        if (
            request.FormSessionId == null
            || request.IsNewSession
            || request.RegisterSession == false
        )
        {
            ss = sessionManager.CreateSessionStore(
                request: request,
                basicUIService: basicUIService
            );
            analytics.SetProperty(propertyName: "OrigamFormId", value: ss.FormId);
            analytics.SetProperty(propertyName: "OrigamFormName", value: ss.Name);
            analytics.Log(message: "UI_OPENFORM");
            if (ss.SupportsFormXmlAsync && IsFormXmlNotCachedOnClient(request: request, ss: ss))
            {
                getFormXmlTask = Task.Run(action: () => ss.PrepareFormXml());
            }
            RegisterSession(
                request: request,
                addChildSession: addChildSession,
                parentSession: parentSession,
                ss: ss
            );
            ss.Init();
            ss.IsExclusive = isExclusive;
        }
        else
        {
            // USE EXISTING SESSION
            ss = sessionManager.GetSession(
                sessionFormIdentifier: new Guid(g: request.FormSessionId)
            );
            if (ss.RefreshOnInitUI)
            {
                ss.ExecuteAction(actionId: SessionStore.ACTION_REFRESH);
            }
            RegisterSession(
                request: request,
                addChildSession: addChildSession,
                parentSession: parentSession,
                ss: ss
            );
        }
        // finalize
        FormSessionStore fss = ss as FormSessionStore;
        if (fss != null && fss.IsDelayedLoading && request.SupportsPagedData)
        {
            ss.IsPagedLoading = true;
        }
        IList<string> columns = null;
        if (ss.IsPagedLoading)
        {
            // for lazily-loaded data we provide all the preloaded columns
            // (primary keys + all the initial sort columns)
            columns = ss.DataListLoadedColumns;
        }
        DatasetTools.CheckRowErrorOfChangedRows(dataSet: ss.InitialData);
        UIResult result = new UIResult(
            sessionId: ss.Id,
            data: DataTools.DatasetToDictionary(
                data: ss.InitialData,
                columns: columns,
                firstPageRecords: initialPageNumberOfRecords,
                firstRecordId: ss.CurrentRecordId,
                dataListEntity: ss.DataListEntity,
                ss: ss
            ),
            variables: ss.Variables,
            isDirty: ss.HasChanges()
        );
        result.Notifications = ss.Notifications;
        result.HasPartialData = ss.IsPagedLoading;
        if (ss is WorkflowSessionStore)
        {
            result.WorkflowTaskId = (ss as WorkflowSessionStore).TaskId.ToString();
        }
        if (request.RequestCurrentRecordId)
        {
            result.CurrentRecordId =
                ss.CurrentRecordId == null ? null : ss.CurrentRecordId.ToString();
        }
        if (IsFormXmlNotCachedOnClient(request: request, ss: ss))
        {
            // wait for asynchronously loaded form xml
            if (getFormXmlTask != null)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: "Waiting for XML...");
                }
                Task.WaitAll(tasks: getFormXmlTask);
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: "Waiting for XML is over...");
                }
            }
            // FORM XML
            SetFormXml(result: result, profile: profile, ss: ss);
        }
        result.Tooltip = ToolTipTools.NextTooltip(formId: ss.HelpTooltipFormId);
        result.FormDefinitionId = ss.FormId.ToString();
        result.Title = ss.Title;
        return result;
    }

    private void RegisterSession(
        UIRequest request,
        bool addChildSession,
        SessionStore parentSession,
        SessionStore ss
    )
    {
        if (request.RegisterSession)
        {
            sessionManager.RegisterSession(ss: ss);
        }
        if (addChildSession)
        {
            ss.ParentSession = parentSession;
            parentSession.AddChildSession(ss: ss);
            parentSession.ActiveSession = ss;
        }
        if (request.RegisterSession)
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
        if (
            request.IsNewSession
            && request.RegisterSession
            && FeatureTools.IsFeatureOn(featureCode: OrigamEvent.OpenScreen.FeatureCode)
        )
        {
            OrigamEventTools.RecordOpenScreen(sessionStore: ss);
        }
    }

    private static void SetFormXml(UIResult result, UserProfile profile, SessionStore ss)
    {
        XmlDocument formXml;
        WorkflowSessionStore wss = ss as WorkflowSessionStore;
        formXml = ss.GetFormXml();
        if (wss == null)
        {
            FormXmlPostProcessing(
                result: result,
                data: ss.Data,
                formXml: formXml,
                profile: profile,
                workflowId: Guid.Empty,
                sortSet: ss.SortSet
            );
        }
        else
        {
            FormXmlPostProcessing(
                result: result,
                data: ss.Data,
                formXml: formXml,
                profile: profile,
                workflowId: wss.WorkflowId,
                sortSet: ss.SortSet
            );
        }
        System.Diagnostics.Debug.Assert(condition: result.FormDefinition != null);
    }

    private static bool IsFormXmlNotCachedOnClient(UIRequest request, SessionStore ss)
    {
        return !(request.IsDataOnly || request.CachedFormIds.Contains(item: ss.FormId.ToString()));
    }

    private static void FormXmlPostProcessing(
        UIResult result,
        DataSet data,
        XmlDocument formXml,
        UserProfile profile,
        Guid workflowId,
        DataStructureSortSet sortSet
    )
    {
        result.FormDefinition = formXml.OuterXml;

        XmlNodeList configurableNodes = formXml.SelectNodes(
            xpath: "//*[@HasPanelConfiguration='true']"
        );
        foreach (XmlElement element in configurableNodes)
        {
            UIPanel panel = new UIPanel();
            panel.Entity = element.GetAttribute(name: "Entity");
            panel.Id = XmlConvert.ToGuid(s: element.GetAttribute(name: "ModelId"));
            panel.InstanceId = XmlConvert.ToGuid(s: element.GetAttribute(name: "ModelInstanceId"));
            if (element.GetAttribute(name: "DefaultPanelView") != "") { }
            UIPanelConfig panelConfig = GetPanelConfig(
                workflowId: workflowId,
                profile: profile,
                data: data,
                panel: panel,
                grid: element
            );
            result.PanelConfigurations.Add(item: panelConfig);
            // default sort
            if (sortSet != null && element.AttributeIsFalseOrMissing(attributeName: "IsHeadless"))
            {
                if (element.GetAttribute(name: "DisableActionButtons") == "true")
                {
                    throw new Exception(message: Resources.ActionButtonsCannotBeDisabled);
                }
                List<DataStructureSortSetItem> sorts = new List<DataStructureSortSetItem>();
                foreach (
                    var item in sortSet.ChildItemsByType<DataStructureSortSetItem>(
                        itemType: DataStructureSortSetItem.CategoryConst
                    )
                )
                {
                    if (item.Entity.Name == panel.Entity)
                    {
                        sorts.Add(item: item);
                    }
                }
                sorts.Sort();
                foreach (DataStructureSortSetItem item in sorts)
                {
                    panelConfig.DefaultSort.Add(
                        item: new UIGridSortConfiguration(
                            field: item.FieldName,
                            direction: item.SortDirection.ToString()
                        )
                    );
                }
            }
        }
        // SPLITTER CONFIG
        XmlNodeList splitters = formXml.SelectNodes(xpath: "//*[@Type='VSplit' or @Type='HSplit']");
        foreach (XmlElement s in splitters)
        {
            Guid instanceId = XmlConvert.ToGuid(s: s.GetAttribute(name: "ModelInstanceId"));
            OrigamPanelColumnConfig loadedConfig = OrigamPanelColumnConfigDA.LoadUserConfig(
                panelId: instanceId,
                profileId: profile.Id
            );
            if (loadedConfig.PanelColumnConfig.Count > 0)
            {
                UISplitConfig splitConfig = new UISplitConfig();
                UIPanel panel = new UIPanel();
                panel.InstanceId = instanceId;
                splitConfig.Panel = panel;
                splitConfig.Position = loadedConfig.PanelColumnConfig[index: 0].ColumnWidth;
                result.PanelConfigurations.Add(item: splitConfig);
            }
        }
        // MENU MAPPINGS
        IDataLookupService ls =
            ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
            as IDataLookupService;
        XmlNodeList lookups = formXml.SelectNodes(xpath: "//*[string(@LookupId)]");
        foreach (XmlElement l in lookups)
        {
            Guid lookupId = XmlConvert.ToGuid(s: l.GetAttribute(name: "LookupId"));
            IMenuBindingResult binding = ls.GetMenuBinding(lookupId: lookupId, value: null);
            result.LookupMenuMappings.Add(
                item: new LookupConfig(
                    lookupId: lookupId,
                    menuId: binding.MenuId,
                    dependsOnValue: ls.HasMenuBindingWithSelection(lookupId: lookupId),
                    selectionPanelId: binding.PanelId
                )
            );
        }
    }

    private UIResult InitDashboardView(UIRequest request)
    {
        return InitDashboardView(objectId: request.ObjectId, viewId: null);
    }

    private UIResult InitDashboardView(string objectId, string viewId)
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        UIResult result = new UIResult(
            sessionId: Guid.Empty,
            data: null,
            variables: null,
            isDirty: false
        );
        XmlDocument dashboardViews = DashboardViews(menuId: objectId);
        XmlDocument formXml;
        if (dashboardViews.FirstChild.ChildNodes.Count > 0)
        {
            XmlNode viewNode;
            if (viewId == null)
            {
                viewNode = dashboardViews.FirstChild.FirstChild;
            }
            else
            {
                viewNode = dashboardViews.SelectSingleNode(
                    xpath: "/dashboardViews/view[@id='" + viewId + "']"
                );
            }
            if (viewNode == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "viewId",
                    actualValue: viewId,
                    message: Resources.ErrorDashboardViewNotFound
                );
            }
            string selectedViewId = viewNode.Attributes[name: "id"].Value;
            string selectedViewName = viewNode.Attributes[name: "name"].Value;
            string config = DashboardViewConfig(viewId: selectedViewId);
            formXml = FormXmlBuilder.GetXml(
                dashboardViewConfig: config,
                name: selectedViewName,
                menuId: new Guid(g: objectId),
                dashboardViews: dashboardViews
            );
        }
        else
        {
            formXml = FormXmlBuilder.GetXml(
                dashboardViewConfig: "<configuration/>",
                name: "",
                menuId: new Guid(g: objectId),
                dashboardViews: dashboardViews
            );
        }
        FormXmlPostProcessing(
            result: result,
            data: null,
            formXml: formXml,
            profile: profile,
            workflowId: Guid.Empty,
            sortSet: null
        );
        Task.Run(action: () =>
            SecurityTools.CreateUpdateOrigamOnlineUser(
                username: SecurityManager.CurrentPrincipal.Identity.Name,
                stats: sessionManager.GetSessionStats()
            )
        );
        return result;
    }

    private static UIPanelConfig GetPanelConfig(
        Guid workflowId,
        UserProfile profile,
        DataSet data,
        UIPanel panel,
        XmlElement grid
    )
    {
        UIPanelConfig panelConfig = new UIPanelConfig();
        panelConfig.Panel = panel;
        if (data != null)
        {
            // Check for a Create security state only for the root grid.
            // For child grids we test by adding an artificial child record when
            // getting security rule for the parent record. Since the root grid
            // has no parent record, we use an empty <ROOT/>.
            if (
                !grid.GetAttribute(name: "DataMember").Contains(value: ".")
                && grid.GetAttribute(name: "ShowAddButton") == "true"
            )
            {
                RuleEngine re = RuleEngine.Create(
                    contextStores: new Hashtable(),
                    transactionId: null
                );
                XmlContainer newRecordData = new XmlContainer();
                newRecordData.Xml.AppendChild(
                    newChild: newRecordData.Xml.CreateElement(name: "ROOT")
                );
                panelConfig.AllowCreate = re.EvaluateRowLevelSecurityState(
                    originalData: newRecordData,
                    actualData: newRecordData,
                    field: null,
                    type: CredentialType.Create,
                    entityId: (Guid)
                        data.Tables[name: panel.Entity].ExtendedProperties[key: "EntityId"],
                    fieldId: Guid.Empty,
                    isNewRow: true
                );
            }
            // filters
            foreach (
                UIGridFilterConfiguration uigfc in GetFilters(
                    panelId: panel.Id,
                    data: data,
                    entity: panel.Entity
                )
            )
            {
                panelConfig.Filters.Add(item: uigfc);
            }
            // default filter and grid visibility
            DataSet userConfig = OrigamPanelConfigDA.LoadConfigData(
                panelInstanceId: panel.InstanceId,
                workflowId: workflowId,
                profileId: profile.Id
            );
            if (userConfig.Tables[name: "OrigamFormPanelConfig"].Rows.Count == 0)
            {
                panelConfig.Panel.DefaultPanelView = panel.DefaultPanelView;
            }
            else
            {
                DataRow userConfigRow = userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[
                    index: 0
                ];
                panelConfig.Panel.DefaultPanelView = (OrigamPanelViewMode)
                    userConfigRow[columnName: "DefaultView"];
                if (userConfigRow[columnName: "refOrigamPanelFilterId"] != DBNull.Value)
                {
                    OrigamPanelFilter defaultFilter = OrigamPanelFilterDA.LoadFilter(
                        id: (Guid)userConfigRow[columnName: "refOrigamPanelFilterId"]
                    );
                    foreach (OrigamPanelFilter.PanelFilterRow pfr in defaultFilter.PanelFilter.Rows)
                    {
                        UIGridFilterConfiguration uigfc = GetFilter(
                            pfr: pfr,
                            data: data,
                            entity: panel.Entity
                        );
                        panelConfig.InitialFilter = uigfc;
                    }
                }
            }
        }
        return panelConfig;
    }

    private static IList<UIGridFilterConfiguration> GetFilters(
        Guid panelId,
        DataSet data,
        string entity
    )
    {
        List<UIGridFilterConfiguration> result = new List<UIGridFilterConfiguration>();
        OrigamPanelFilter pf = OrigamPanelFilterDA.LoadFilters(panelId: panelId);
        foreach (OrigamPanelFilter.PanelFilterRow pfr in pf.PanelFilter.Rows)
        {
            result.Add(item: GetFilter(pfr: pfr, data: data, entity: entity));
        }
        return result;
    }

    private static UIGridFilterConfiguration GetFilter(
        OrigamPanelFilter.PanelFilterRow pfr,
        DataSet data,
        string entity
    )
    {
        UIGridFilterConfiguration uigfc = new UIGridFilterConfiguration(
            id: pfr.Id,
            name: pfr.Name,
            isGlobal: pfr.IsGlobal
        );
        if (data.Tables.Contains(name: entity))
        {
            foreach (OrigamPanelFilter.PanelFilterDetailRow pfdr in pfr.GetPanelFilterDetailRows())
            {
                // check if the column stored in the filter was not removed from the entity, in that
                // case just ignore this field
                if (data.Tables[name: entity].Columns.Contains(name: pfdr.ColumnName))
                {
                    Type t = data.Tables[name: entity].Columns[name: pfdr.ColumnName].DataType;
                    FilterOperator op = (FilterOperator)pfdr.Operator;
                    if (
                        t == typeof(Guid)
                        && op != FilterOperator.Equals
                        && op != FilterOperator.NotEquals
                    )
                    {
                        t = typeof(string);
                    }
                    uigfc.Details.Add(
                        item: new UIGridFilterFieldConfiguration(
                            property: pfdr.ColumnName,
                            value1: OrigamPanelFilterDA.StoredFilterValue(
                                row: pfdr,
                                type: t,
                                valueNumber: 1
                            ),
                            value2: OrigamPanelFilterDA.StoredFilterValue(
                                row: pfdr,
                                type: t,
                                valueNumber: 2
                            ),
                            oper: pfdr.Operator
                        )
                    );
                }
            }
        }
        return uigfc;
    }

    private static string DashboardViewConfig(string viewId)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
            as IDataLookupService;
        return ls.GetDisplayText(
                lookupId: new Guid(g: "d27877bc-3fd5-4fe6-a5c9-b4119fb821b6"),
                lookupValue: viewId,
                useCache: false,
                returnMessageIfNull: false,
                transactionId: null
            )
            .ToString();
    }

    private static XmlDocument DashboardViews(string menuId)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement rootElement = doc.CreateElement(name: "dashboardViews");
        doc.AppendChild(newChild: rootElement);
        IPrincipal principal = SecurityManager.CurrentPrincipal;
        DataSet data = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "e6b7e890-032c-4837-b3e1-592f9d6f9d0f"),
            methodId: new Guid(g: "916f8028-9d89-49b2-bb66-97548bde8b7d"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "OrigamDashboardView_parMenuId",
            paramValue1: menuId
        );
        foreach (DataRow row in data.Tables[name: "OrigamDashboardView"].Rows)
        {
            IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
            if (auth.Authorize(principal: principal, context: (string)row[columnName: "Roles"]))
            {
                XmlElement viewElement = doc.CreateElement(name: "view");
                viewElement.SetAttribute(name: "id", value: row[columnName: "Id"].ToString());
                viewElement.SetAttribute(name: "name", value: (string)row[columnName: "Name"]);
                viewElement.SetAttribute(name: "roles", value: (string)row[columnName: "Roles"]);
                rootElement.AppendChild(newChild: viewElement);
            }
        }
        return doc;
    }
}
