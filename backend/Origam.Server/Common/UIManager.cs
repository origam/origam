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
using Origam.Server;
using Origam.Service.Core;
using Origam.Workbench;
using Origam.Workbench.Services;
using Origam.Workflow;
using core = Origam.Workbench.Services.CoreServices;
using Debug = System.Diagnostics.Debug;

namespace Origam.Server;
public class UIManager
{
    internal static readonly log4net.ILog log 
        = log4net.LogManager.GetLogger(System.Reflection.MethodBase
            .GetCurrentMethod().DeclaringType);
    private readonly int initialPageNumberOfRecords;
    private readonly SessionManager sessionManager;
    private readonly Analytics analytics;
    public UIManager(int initialPageNumberOfRecords,
        SessionManager sessionManager, Analytics analytics)
    {
        this.analytics = analytics;
        this.initialPageNumberOfRecords = initialPageNumberOfRecords;
        this.sessionManager = sessionManager;
    }
    public UIResult InitUI(UIRequest request, bool addChildSession, 
        SessionStore parentSession, IBasicUIService basicUIService)
    {
        Task getFormXmlTask = null;
        // Stack can't handle resending DataDocumentFx, 
        // it needs to be converted to XmlDocument 
        // and then converted back to DataDocumentFx
        foreach (object key in request.Parameters.Keys.CastToList<object>())
        {
            object value = request.Parameters[key];
            if (value is XmlDocument xmlDoc)
            {
                request.Parameters[key] = new XmlContainer(
                    xmlDoc);
            }
            if (value is JObject jobj)
            {
                request.Parameters[key] = new XmlContainer(
                    JsonConvert.DeserializeXmlNode(jobj.ToString())
                    );
            }
        }
        bool isExclusive = false;
        if(request.IsNewSession)
        {
            PortalSessionStore pss = sessionManager.GetPortalSession();
            OrigamSettings settings =  ConfigurationManager.GetActiveConfiguration();
            if (settings.MaxOpenTabs > 0)
            {
                if (pss.FormSessions.Count >= settings.MaxOpenTabs)
                {
                    throw new RuleException(Resources.ErrorTooManyTabsOpen);
                }
            }
            if(!request.ObjectId.Contains("|"))
            {
                if (request.Type != UIRequestType.Dashboard
                    && request.Type != UIRequestType.WorkQueue
                    && Guid.TryParse(request.ObjectId, out var objectId))
                {
                    IPersistenceService persistence =
                        ServiceManager.Services.GetService(
                                typeof(IPersistenceService)) as
                            IPersistenceService;
                    AbstractMenuItem menu =
                        persistence.SchemaProvider.RetrieveInstance(
                                typeof(AbstractMenuItem),
                                new ModelElementKey(objectId))
                            as AbstractMenuItem;
                    if(menu != null) // can be a workflow
                    {
                        isExclusive = menu.OpenExclusively;
                        if(menu.OpenExclusively &&
                            pss.FormSessions.Count > 0)
                        {
                            throw new RuleException(string.Format(
                                Resources.ErrorCannotOpenFormExclusively,
                                request.Caption));
                        }
                    }
                }
            }
        }
        UserProfile profile = SecurityTools.CurrentUserProfile();
        if(request.Type == UIRequestType.Dashboard)
        {
            return InitDashboardView(request);
        }
        SessionStore ss;
        if(request.FormSessionId == null || request.IsNewSession ||
            request.RegisterSession == false)
        {
            ss = sessionManager.CreateSessionStore(request, basicUIService);
            analytics.SetProperty("OrigamFormId", ss.FormId);
            analytics.SetProperty("OrigamFormName", ss.Name);
            analytics.Log("UI_OPENFORM");
            if (ss.SupportsFormXmlAsync 
            && IsFormXmlNotCachedOnClient(request, ss))
            {
                getFormXmlTask = Task.Run(() => ss.PrepareFormXml());
            }
            ss.Init();
            ss.IsExclusive = isExclusive;
        }
        else
        {
            // USE EXISTING SESSION
            ss = sessionManager.GetSession(new Guid(request.FormSessionId));
            if(ss.RefreshOnInitUI)
            {
                ss.ExecuteAction(SessionStore.ACTION_REFRESH);
            }
        }
        // finalize
        FormSessionStore fss = ss as FormSessionStore;
        if(fss != null && fss.IsDelayedLoading &&
            request.SupportsPagedData)
        {
            ss.IsPagedLoading = true;
        }
        IList<string> columns = null;
        if(ss.IsPagedLoading)
        {
            // for lazily-loaded data we provide all the preloaded columns
            // (primary keys + all the initial sort columns)
            columns = ss.DataListLoadedColumns;
        }
        DatasetTools.CheckRowErrorOfChangedRows(ss.InitialData);
        UIResult result = new UIResult(
            sessionId: ss.Id, 
            data: DataTools.DatasetToDictionary(
                ss.InitialData, columns, initialPageNumberOfRecords,
                ss.CurrentRecordId, ss.DataListEntity, ss), 
            variables: ss.Variables, 
            isDirty: ss.HasChanges());
        result.Notifications = ss.Notifications;
        result.HasPartialData = ss.IsPagedLoading;
        if(ss is WorkflowSessionStore)
        {
            result.WorkflowTaskId =
                (ss as WorkflowSessionStore).TaskId.ToString();
        }
        if(request.RequestCurrentRecordId)
        {
            result.CurrentRecordId = ss.CurrentRecordId == null
                ? null
                : ss.CurrentRecordId.ToString();
        }
        if(IsFormXmlNotCachedOnClient(request, ss))
        {
            // wait for asynchronously loaded form xml
            if(getFormXmlTask != null)
            {
                if(log.IsDebugEnabled)
                {
                    log.Debug("Waiting for XML...");
                }
                Task.WaitAll(getFormXmlTask);
                if(log.IsDebugEnabled)
                {
                    log.Debug("Waiting for XML is over...");
                }
            }
            // FORM XML
            SetFormXml(result, profile, ss);
        }
        result.Tooltip = ToolTipTools.NextTooltip(ss.HelpTooltipFormId);
        result.FormDefinitionId = ss.FormId.ToString();
        result.Title = ss.Title;
        if(request.RegisterSession)
        {
            sessionManager.RegisterSession(ss);
        }
        if(addChildSession)
        {
            ss.ParentSession = parentSession;
            parentSession.AddChildSession(ss);
            parentSession.ActiveSession = ss;
        }
        if(request.RegisterSession)
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
        return result;
    }
    private static void SetFormXml(UIResult result, UserProfile profile,
        SessionStore ss)
    {
        XmlDocument formXml;
        WorkflowSessionStore wss = ss as WorkflowSessionStore;
        formXml = ss.GetFormXml();
        if (wss == null)
        {
            FormXmlPostProcessing(result, ss.Data, formXml, profile,
                Guid.Empty, ss.SortSet);
        } else
        {
            FormXmlPostProcessing(result, ss.Data, formXml, profile,
                wss.WorkflowId, ss.SortSet);
        }
        System.Diagnostics.Debug.Assert(result.FormDefinition != null);
    }
    private static bool IsFormXmlNotCachedOnClient(UIRequest request,
        SessionStore ss)
    {
        return !(request.IsDataOnly ||
                 request.CachedFormIds.Contains(ss.FormId.ToString()));
    }
    private static void FormXmlPostProcessing(UIResult result, DataSet data,
        XmlDocument formXml, UserProfile profile, Guid workflowId,
        DataStructureSortSet sortSet)
    {
        result.FormDefinition = formXml.OuterXml;
        
        XmlNodeList configurableNodes = formXml.SelectNodes("//*[@HasPanelConfiguration='true']"); 
        foreach (XmlElement element in configurableNodes)
        {
            UIPanel panel = new UIPanel();
            panel.Entity = element.GetAttribute("Entity");
            panel.Id = XmlConvert.ToGuid(element.GetAttribute("ModelId"));
            panel.InstanceId =
                XmlConvert.ToGuid(element.GetAttribute("ModelInstanceId"));
            if (element.GetAttribute("DefaultPanelView") != "")
            {
            }
            UIPanelConfig panelConfig =
                GetPanelConfig(workflowId, profile, data, panel, element);
            result.PanelConfigurations.Add(panelConfig);
            // default sort
            if (sortSet != null &&
                element.AttributeIsFalseOrMissing("IsHeadless") &&
                element.AttributeIsFalseOrMissing("DisableActionButtons"))
            {
                List<DataStructureSortSetItem> sorts =
                    new List<DataStructureSortSetItem>();
                foreach (var item in sortSet
                    .ChildItemsByType<DataStructureSortSetItem>(
                        DataStructureSortSetItem.CategoryConst))
                {
                    if (item.Entity.Name == panel.Entity)
                    {
                        sorts.Add(item);
                    }
                }
                sorts.Sort();
                foreach (DataStructureSortSetItem item in sorts)
                {
                    panelConfig.DefaultSort.Add(
                        new UIGridSortConfiguration(item.FieldName,
                            item.SortDirection.ToString()));
                }
            }
        }
        // SPLITTER CONFIG
        XmlNodeList splitters =
            formXml.SelectNodes("//*[@Type='VSplit' or @Type='HSplit']");
        foreach (XmlElement s in splitters)
        {
            Guid instanceId =
                XmlConvert.ToGuid(s.GetAttribute("ModelInstanceId"));
            OrigamPanelColumnConfig loadedConfig =
                OrigamPanelColumnConfigDA.LoadUserConfig(instanceId,
                    profile.Id);
            if (loadedConfig.PanelColumnConfig.Count > 0)
            {
                UISplitConfig splitConfig = new UISplitConfig();
                UIPanel panel = new UIPanel();
                panel.InstanceId = instanceId;
                splitConfig.Panel = panel;
                splitConfig.Position =
                    loadedConfig.PanelColumnConfig[0].ColumnWidth;
                result.PanelConfigurations.Add(splitConfig);
            }
        }
        // MENU MAPPINGS
        IDataLookupService ls =
            ServiceManager.Services.GetService(typeof(IDataLookupService))
                as IDataLookupService;
        XmlNodeList lookups = formXml.SelectNodes("//*[string(@LookupId)]");
        foreach (XmlElement l in lookups)
        {
            Guid lookupId = XmlConvert.ToGuid(l.GetAttribute("LookupId"));
            IMenuBindingResult binding = ls.GetMenuBinding(lookupId, null);
            result.LookupMenuMappings.Add(new LookupConfig(lookupId,
                binding.MenuId, ls.HasMenuBindingWithSelection(lookupId),
                binding.PanelId));
        }
    }
    private UIResult InitDashboardView(UIRequest request)
    {
        return InitDashboardView(request.ObjectId, null);
    }
    private UIResult InitDashboardView(string objectId, string viewId)
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        UIResult result = new UIResult(Guid.Empty, null, null, false);
        XmlDocument dashboardViews = DashboardViews(objectId);
        XmlDocument formXml;
        if (dashboardViews.FirstChild.ChildNodes.Count > 0)
        {
            XmlNode viewNode;
            if (viewId == null)
            {
                viewNode = dashboardViews.FirstChild.FirstChild;
            } else
            {
                viewNode =
                    dashboardViews.SelectSingleNode(
                        "/dashboardViews/view[@id='" + viewId + "']");
            }
            if (viewNode == null)
            {
                throw new ArgumentOutOfRangeException("viewId", viewId,
                    Resources.ErrorDashboardViewNotFound);
            }
            string selectedViewId = viewNode.Attributes["id"].Value;
            string selectedViewName = viewNode.Attributes["name"].Value;
            string config = DashboardViewConfig(selectedViewId);
            formXml = FormXmlBuilder.GetXml(config, selectedViewName,
                new Guid(objectId), dashboardViews);
        } else
        {
            formXml = FormXmlBuilder.GetXml("<configuration/>", "",
                new Guid(objectId), dashboardViews);
        }
        FormXmlPostProcessing(result, null, formXml, profile, Guid.Empty,
            null);
        Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
            SecurityManager.CurrentPrincipal.Identity.Name,
            sessionManager.GetSessionStats()));
        return result;
    }
    private static UIPanelConfig GetPanelConfig(Guid workflowId,
        UserProfile profile, DataSet data, UIPanel panel, XmlElement grid)
    {
        UIPanelConfig panelConfig = new UIPanelConfig();
        panelConfig.Panel = panel;
        if (data != null)
        {
            // Check for a Create security state only for the root grid.
            // For child grids we test by adding an artificial child record when
            // getting security rule for the parent record. Since the root grid
            // has no parent record, we use an empty <ROOT/>.
            if (!grid.GetAttribute("DataMember").Contains(".") &&
                grid.GetAttribute("ShowAddButton") == "true")
            {
                RuleEngine re = RuleEngine.Create(new Hashtable(), null);
                XmlContainer newRecordData = new XmlContainer();
                newRecordData.Xml.AppendChild(
                    newRecordData.Xml.CreateElement("ROOT"));
                panelConfig.AllowCreate = re.EvaluateRowLevelSecurityState(
                    newRecordData, newRecordData, null,
                    CredentialType.Create,
                    (Guid) data.Tables[panel.Entity]
                        .ExtendedProperties["EntityId"], Guid.Empty, true);
            }
            // filters
            foreach (UIGridFilterConfiguration uigfc in GetFilters(panel.Id,
                data, panel.Entity))
            {
                panelConfig.Filters.Add(uigfc);
            }
            // default filter and grid visibility
            DataSet userConfig =
                OrigamPanelConfigDA.LoadConfigData(panel.InstanceId,
                    workflowId, profile.Id);
            if (userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
            {
                panelConfig.Panel.DefaultPanelView = panel.DefaultPanelView;
            } else
            {
                DataRow userConfigRow =
                    userConfig.Tables["OrigamFormPanelConfig"].Rows[0];
                panelConfig.Panel.DefaultPanelView =
                    (OrigamPanelViewMode) userConfigRow["DefaultView"];
                if (userConfigRow["refOrigamPanelFilterId"] != DBNull.Value)
                {
                    OrigamPanelFilter defaultFilter =
                        OrigamPanelFilterDA.LoadFilter(
                            (Guid) userConfigRow["refOrigamPanelFilterId"]);
                    foreach (OrigamPanelFilter.PanelFilterRow pfr in
                        defaultFilter.PanelFilter.Rows)
                    {
                        UIGridFilterConfiguration uigfc =
                            GetFilter(pfr, data, panel.Entity);
                        panelConfig.InitialFilter = uigfc;
                    }
                }
            }
        }
        return panelConfig;
    }
    private static IList<UIGridFilterConfiguration> GetFilters(Guid panelId,
        DataSet data, string entity)
    {
        List<UIGridFilterConfiguration> result =
            new List<UIGridFilterConfiguration>();
        OrigamPanelFilter pf = OrigamPanelFilterDA.LoadFilters(panelId);
        foreach (OrigamPanelFilter.PanelFilterRow pfr in pf.PanelFilter.Rows)
        {
            result.Add(GetFilter(pfr, data, entity));
        }
        return result;
    }
    private static UIGridFilterConfiguration GetFilter(
        OrigamPanelFilter.PanelFilterRow pfr, DataSet data, string entity)
    {
        UIGridFilterConfiguration uigfc =
            new UIGridFilterConfiguration(pfr.Id, pfr.Name, pfr.IsGlobal);
        if (data.Tables.Contains(entity))
        {
            foreach (OrigamPanelFilter.PanelFilterDetailRow pfdr in pfr
                .GetPanelFilterDetailRows())
            {
                // check if the column stored in the filter was not removed from the entity, in that
                // case just ignore this field
                if (data.Tables[entity].Columns.Contains(pfdr.ColumnName))
                {
                    Type t = data.Tables[entity].Columns[pfdr.ColumnName]
                        .DataType;
                    FilterOperator op = (FilterOperator) pfdr.Operator;
                    if (t == typeof(Guid) && op != FilterOperator.Equals &&
                        op != FilterOperator.NotEquals)
                    {
                        t = typeof(string);
                    }
                    uigfc.Details.Add(
                        new UIGridFilterFieldConfiguration(pfdr.ColumnName,
                            OrigamPanelFilterDA.StoredFilterValue(pfdr, t, 1),
                            OrigamPanelFilterDA.StoredFilterValue(pfdr, t, 2),
                            pfdr.Operator)
                    );
                }
            }
        }
        return uigfc;
    }
    private static string DashboardViewConfig(string viewId)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(typeof(IDataLookupService))
                as IDataLookupService;
        return ls
            .GetDisplayText(
                new Guid("d27877bc-3fd5-4fe6-a5c9-b4119fb821b6"), viewId,
                false, false, null).ToString();
    }
    private static XmlDocument DashboardViews(string menuId)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement rootElement = doc.CreateElement("dashboardViews");
        doc.AppendChild(rootElement);
        IPrincipal principal = SecurityManager.CurrentPrincipal;
        DataSet data = core.DataService.Instance.LoadData(
            new Guid("e6b7e890-032c-4837-b3e1-592f9d6f9d0f"),
            new Guid("916f8028-9d89-49b2-bb66-97548bde8b7d"), Guid.Empty,
            Guid.Empty, null,
            "OrigamDashboardView_parMenuId", menuId);
        foreach (DataRow row in data.Tables["OrigamDashboardView"].Rows)
        {
            IOrigamAuthorizationProvider auth =
                SecurityManager.GetAuthorizationProvider();
            if (auth.Authorize(principal, (string) row["Roles"]))
            {
                XmlElement viewElement = doc.CreateElement("view");
                viewElement.SetAttribute("id", row["Id"].ToString());
                viewElement.SetAttribute("name", (string) row["Name"]);
                viewElement.SetAttribute("roles", (string) row["Roles"]);
                rootElement.AppendChild(viewElement);
            }
        }
        return doc;
    }
}
