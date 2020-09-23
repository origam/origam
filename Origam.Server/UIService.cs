#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using Origam.hosting.utils;
using Origam;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.DA;
using Origam.DA.Service;
using Origam.Gui.Win;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using Origam.Workbench;
using Origam.Workbench.Services;
using FluorineFx;
using FluorineFx.Json;
using Origam.Gui;
using Origam.Server.Properties;
using Origam.Server.Search;
using Origam.ServerCommon;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Server
{
    [RemotingService]
    public class UIService: IBasicUIService
    {
        #region Private Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SessionManager sessionManager;
        private const int INITIAL_PAGE_NUMBER_OF_RECORDS = 50;
        private readonly UIManager uiManager;
        private readonly ReportManager reportManager;
        private readonly SessionHelper sessionHelper;

        #endregion

        #region Constructors
        public UIService()
        {
            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
            parameterService.SetFeatureStatus("FLASH", true);

            var portalSessions = (Dictionary<Guid, PortalSessionStore>)FluorineFx.Context.FluorineContext.Current.ApplicationState["portals"];
            if (portalSessions == null)
            {
                portalSessions = new Dictionary<Guid, PortalSessionStore>();
                FluorineFx.Context.FluorineContext.Current.ApplicationState["portals"] = portalSessions;
            }

            var formSessions = (Dictionary<Guid, SessionStore>)FluorineFx.Context.FluorineContext.Current.ApplicationState["forms"];
            if (formSessions == null)
            {
                formSessions = new Dictionary<Guid, SessionStore>();
                FluorineFx.Context.FluorineContext.Current.ApplicationState["forms"] = formSessions;
            }

            sessionManager = new SessionManager(
                portalSessions: portalSessions,
                formSessions: formSessions,
                reportRequests: null,
                blobDownloadRequests: null,
                blobUploadRequests: null,
                analytics: Analytics.Instance);
            uiManager = new UIManager(INITIAL_PAGE_NUMBER_OF_RECORDS,sessionManager, Analytics.Instance);
            reportManager = new ReportManager(sessionManager);
            sessionHelper = new SessionHelper(sessionManager);
        }

        #endregion

        #region IUIService Members
        [JsonRpcMethod]
        public PortalResult InitPortal(string locale)
        {
            Analytics.Instance.Log("UI_INIT");

            // set locale
            locale = locale.Replace("_", "-");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);

            // set locale to the cookie
            HttpCookie localeCookie = new HttpCookie(ORIGAMLocaleResolver.ORIGAM_CURRENT_LOCALE);
            localeCookie.Value = locale;
            HttpContext.Current.Response.Cookies.Add(localeCookie);

            UserProfile profile = SecurityTools.CurrentUserProfile();

            PortalResult result = new PortalResult(GetMenu());

            OrigamSettings settings = (OrigamSettings)ConfigurationManager.GetActiveConfiguration();

            if (settings != null)
            {
                result.WorkQueueListRefreshInterval = settings.WorkQueueListRefreshPeriod * 1000;
                result.Slogan = settings.Slogan;
                result.HelpUrl = settings.HelpUrl;
            }

            HttpRuntimeSection httpRuntimeSection 
                = System.Configuration.ConfigurationManager.GetSection(
                "system.web/httpRuntime") as HttpRuntimeSection;
            result.MaxRequestLength = httpRuntimeSection.MaxRequestLength;

            NotificationBox logoNotificationBox = LogoNotificationBox();

            if (logoNotificationBox != null)
            {
                result.NotificationBoxRefreshInterval = logoNotificationBox.RefreshInterval * 1000;
            }

            // load favorites
            DataSet favorites = core.DataService.LoadData(new Guid("e564c554-ca83-47eb-980d-95b4faba8fb8"), new Guid("e468076e-a641-4b7d-b9b4-7d80ff312b1c"), Guid.Empty, Guid.Empty, null, "OrigamFavoritesUserConfig_parBusinessPartnerId", profile.Id);
            if (favorites.Tables["OrigamFavoritesUserConfig"].Rows.Count > 0)
            {
                result.Favorites = (string)favorites.Tables["OrigamFavoritesUserConfig"].Rows[0]["ConfigXml"];
            }

            if (sessionManager.HasPortalSession(profile.Id))
            {
                var portalSessionStore = sessionManager.GetPortalSession(profile.Id);
                bool clearAll = portalSessionStore.ShouldBeCleared();
                // running session, we get all the form sessions

                ArrayList sessionsToDestroy = new ArrayList();

                foreach (SessionStore mainSS in portalSessionStore.FormSessions)
                {
                    if (clearAll)
                    {
                        sessionsToDestroy.Add(mainSS.Id);
                    }
                    else if (sessionManager.HasFormSession(mainSS.Id))
                    {
                        SessionStore ss = (mainSS.ActiveSession == null ? mainSS : mainSS.ActiveSession);

                        if (ss is SelectionDialogSessionStore || ss.IsModalDialog)
                        {
                            sessionsToDestroy.Add(ss.Id);
                        }
                        else
                        {
                            WorkflowSessionStore wss = ss as WorkflowSessionStore;
                            bool askWorkflowClose = false;
                            if (wss != null) askWorkflowClose = wss.AskWorkflowClose;
                            
                            bool hasChanges = HasChanges(ss);

                            result.Sessions.Add(new PortalResultSession(ss.Id, ss.Request.ObjectId, hasChanges, ss.Request.Type, ss.Request.Caption, ss.Request.Icon, askWorkflowClose));
                        }
                    }
                    else
                    {
                        // session is registered in the user's portal, but not in the UIService anymore,
                        // we have to destroy it
                        sessionsToDestroy.Add(mainSS.Id);
                    }
                }

                foreach (Guid id in sessionsToDestroy)
                {
                    try
                    {
                        DestroyUI(id);
                    }
                    catch(Exception ex)
                    {
                        if(log.IsFatalEnabled)
                        {
                            log.Error(
                                "Failed to destroy session " + id.ToString()
                                + ".", ex);
                        }
                    }
                }
                if (clearAll)
                {
                    portalSessionStore.ResetSessionStart();
                }
            }
            else
            {
                // new session
                PortalSessionStore pss = new PortalSessionStore(profile.Id);
                sessionManager.AddPortalSession(profile.Id, pss);
            }

            result.UserName = profile.FullName;
            result.UserId = profile.Id;
            result.Tooltip = ToolTipTools.NextTooltip();
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return result;
        }

        private static bool HasChanges(SessionStore ss)
        {
            bool hasChanges = false;
            WorkflowSessionStore wss = ss as WorkflowSessionStore;
            if (ss is FormSessionStore && ss.Data != null && ss.Data.HasChanges()) hasChanges = true;
            if (wss != null && wss.AllowSave  && ss.Data != null && ss.Data.HasChanges()) hasChanges = true;
            return hasChanges;
        }

        [JsonRpcMethod]
        public void SavePanelVariables(string sessionId, string variableKey, IDictionary variables)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionId));
            if (ss != null)
            {
                ss.Variables[variableKey] = variables;
            }
        }

        [JsonRpcMethod]
        public void Logout()
        {
            PortalSessionStore pss = sessionManager.GetPortalSession();

            if (pss == null)
            {
                throw new Exception(Resources.ErrorLogOut);
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

        [JsonRpcMethod]
        public string GetMenu()
        {
            return MenuXmlBuilder.GetMenu();
        }

        [JsonRpcMethod]
        public UIResult InitUI(UIRequest request)
        {
            return uiManager.InitUI(
                request: request,
                addChildSession: false,
                parentSession: null,
                basicUIService: this);
        }

        [JsonRpcMethod]
        public bool IsDirty(string formSessionId)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(formSessionId));
            return ss.Data.HasChanges();
        }

        [JsonRpcMethod]
        public RuleExceptionDataCollection ExecuteActionQuery(string sessionFormIdentifier, string entity, string actionType, string actionId, Hashtable parameterMappings, IList selectedItems, Hashtable inputParameters)
        {
            EntityUIAction action = null;
            try
            {
                action = UIActionTools.GetAction(actionId);
            }
            catch
            {
            }
            if (action != null && action.ConfirmationRule != null)
            {
                SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
                DataTable table = ss.GetTable(entity, ss.Data);
                DataRow[] rows = new DataRow[selectedItems.Count];
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    rows[i] = ss.GetSessionRow(entity, selectedItems[i]);
                }
                IXmlContainer xml 
                    = DatasetTools.GetRowXml(rows, DataRowVersion.Default);
                RuleExceptionDataCollection result 
                    = ss.RuleEngine.EvaluateEndRule(
                    action.ConfirmationRule, xml);
                return result ?? new RuleExceptionDataCollection();
            }
            return new RuleExceptionDataCollection();
        }


        [JsonRpcMethod]
        public IList ExecuteAction(string sessionFormIdentifier, string requestingGrid, string entity, string actionType, string actionId, Hashtable parameterMappings, IList selectedItems)
        {
            return ExecuteAction(sessionFormIdentifier, requestingGrid, entity, actionType, actionId, parameterMappings, selectedItems, new Hashtable());
        }

        public IList ExecuteAction(string sessionFormIdentifier, string requestingGrid, 
            string entity, string actionType, string actionId,
            Hashtable parameterMappings, IList selectedItems, Hashtable inputParameters)
        {
            var actionRunnerClient 
                = new ServerEntityUIActionRunnerClient(sessionManager,sessionFormIdentifier);

            var actionRunner = new ServerEntityUIActionRunner( 
                actionRunnerClient: actionRunnerClient,
                uiManager: uiManager,  
                sessionManager: sessionManager,
                basicUIService: this,
                reportManager: reportManager);

            return actionRunner.ExecuteAction( sessionFormIdentifier, 
                requestingGrid, entity,  actionType,  actionId, parameterMappings,
                selectedItems, inputParameters);
        }

        public UIResult WorkflowNext(Guid sessionFormIdentifier, List<string> cachedFormIds)
        {
            WorkflowSessionStore ss = sessionManager.GetSession(sessionFormIdentifier) as WorkflowSessionStore;
            return (UIResult)ss.ExecuteAction(SessionStore.ACTION_NEXT, cachedFormIds);
        }

        [JsonRpcMethod]
        public RuleExceptionDataCollection WorkflowNextQuery(string sessionFormIdentifier)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            return (RuleExceptionDataCollection)ss.ExecuteAction(SessionStore.ACTION_QUERYNEXT);
        }

        public UIResult WorkflowAbort(Guid sessionFormIdentifier)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            return (UIResult)ss.ExecuteAction(SessionStore.ACTION_ABORT); 
        }

        public UIResult WorkflowRepeat(Guid sessionFormIdentifier)
        {
            WorkflowSessionStore wss = sessionManager.GetSession(sessionFormIdentifier) as WorkflowSessionStore;
            if (wss == null) throw new Exception(Resources.ErrorWorkflowSessionInvalid);
            UserProfile profile = SecurityTools.CurrentUserProfile();

            UIRequest request = new UIRequest();
            request.FormSessionId = null;
            request.IsStandalone = wss.Request.IsStandalone;
            request.ObjectId = wss.Request.ObjectId;
            request.Type = wss.Request.Type;
            request.Icon = wss.Request.Icon;
            request.Caption = wss.Request.Caption;

            DestroyUI(sessionFormIdentifier);

            return InitUI(request);
        }

        public void DestroyUI(Guid sessionFormIdentifier)
        {
            sessionHelper.DeleteSession(sessionFormIdentifier);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
        }

        public IList SaveData(Guid sessionFormIdentifier)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            IList output = (IList)ss.ExecuteAction(SessionStore.ACTION_SAVE);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        /// <summary>
        /// Evaluates an end rule on form-data (only dirty rows will get evaluated).
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public RuleExceptionDataCollection SaveDataQuery(string sessionFormIdentifier)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            if (ss.ConfirmationRule != null && ss.Data.HasChanges())
            {
                bool hasRows = false;
                DataSet clone = DatasetTools.CloneDataSet(ss.Data);
                List<DataTable> rootTables = new List<DataTable>();
                foreach (DataTable table in ss.Data.Tables)
                {
                    if (table.ParentRelations.Count == 0)
                    {
                        rootTables.Add(table);
                    }
                }

                foreach (DataTable rootTable in rootTables)
                {
                    foreach (DataRow row in rootTable.Rows)
                    {
                        if (row.RowState != DataRowState.Deleted && IsRowDirty(row))
                        {
                            DatasetTools.GetDataSlice(clone, new List<DataRow>{row});
                            hasRows = true;
                        }
                    }
                }

                if (hasRows)
                {
                    IDataDocument xmlDoc = DataDocumentFactory.New(clone);
                    return ss.RuleEngine.EvaluateEndRule(ss.ConfirmationRule, xmlDoc);
                }
                return new RuleExceptionDataCollection();
            }

            return new RuleExceptionDataCollection();
        }

        private bool IsRowDirty(DataRow row)
        {
            if (row.RowState != DataRowState.Unchanged)
            {
                return true;
            }

            foreach (DataRelation childRelation in row.Table.ChildRelations)
            {
                foreach (DataRow childRow in row.GetChildRows(childRelation))
                {
                    if (IsRowDirty(childRow))
                    {
                        return true;
                    }
                }
				// look for deleted children. They aren't returned by
				// previous ChetChildRows call. 
				foreach (DataRow childRow in row.GetChildRows(childRelation,
					DataRowVersion.Original))
				{
					if (childRow.RowState == DataRowState.Deleted)
					{
						return true;
					}
				}
			}

            return false;
        }

        /// <summary>
        /// Gets data by parent record ID (only for delayed data loading).
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ArrayList GetData(Guid sessionFormIdentifier, string childEntity, object parentRecordId, object rootRecordId)
        {
            SessionStore ss = null;

            try
            {
                ss = sessionManager.GetSession(sessionFormIdentifier);
            }
            catch
            {
            }

            if (ss == null)
            {
                return new ArrayList();
            }

            return ss.GetData(childEntity, parentRecordId, rootRecordId);
        }

        [JsonRpcMethod]
        public IList RestoreData(string sessionFormIdentifier, string entity, object parentId)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            return ss.RestoreData(parentId);
        }


        /// <summary>
        /// Returns whole all rows for the specified columns (preloads columns).
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public IList<ArrayList> GetDataForColumns(string sessionFormIdentifier, string entity, string[] columns)
        {
            List<ArrayList> result = new List<ArrayList>();

            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            ss.LoadColumns(new List<string>(columns));
            DataTable table = ss.GetTable(entity, ss.DataList);
            foreach (DataRow row in table.Rows)
            {
                result.Add(SessionStore.GetRowData(row, columns, false));
            }
            return result;
        }

        /// <summary>
        /// Returns a single row object.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public ArrayList GetDataForRow(string sessionFormIdentifier, string entity, object id)
        {
            List<ArrayList> result = new List<ArrayList>();
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            DataRow row = ss.GetListRow(entity, id);
            return SessionStore.GetRowData(row, SessionStore.GetColumnNames(row.Table));
        }

        /// <summary>
        /// Returns a matrix of requested rows/columns.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public IList<ArrayList> GetDataForMatrix(string sessionFormIdentifier, string entity, string[] rows, string[] columns)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            return ss.GetDataForMatrix(entity, rows, columns);
        }

        [JsonRpcMethod]
        public ArrayList GetPendingChanges(string sessionFormIdentifier)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            ArrayList changes = ss.PendingChanges;
            ss.PendingChanges = null;

            if (changes == null)
            {
                return new ArrayList();
            }
            return changes;
        }

        [JsonRpcMethod]
        public ArrayList GetChanges(string sessionFormIdentifier, string entity, object id)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            bool hasErrors = ss.Data.HasErrors;
            bool hasChanges = ss.Data.HasChanges();
            return ss.GetChanges(entity, id, 0, hasErrors, hasChanges);
        }

        [JsonRpcMethod]
        public ArrayList GetRowData(string sessionFormIdentifier, string entity, object id)
        {
            SessionStore ss = null;
            try
            {
                ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            }
            catch
            {
            }

            if (ss == null)
            {
                return new ArrayList();
            }

            return ss.GetRowData(entity, id, false);
        }

        public IDictionary<string, object> GetSessionData(Guid sessionFormIdentifier)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);

            return DataTools.DatasetToHashtable(ss.Data);
        }

        public IDictionary<string, object> RefreshData(Guid sessionFormIdentifier)
        {
            return RefreshData(sessionFormIdentifier, new Hashtable());
        }

        public IDictionary<string, object> RefreshData(Guid sessionFormIdentifier, Hashtable parameters)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            object result = ss.ExecuteAction(SessionStore.ACTION_REFRESH);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            if (result is DataSet)
            {
                IList<string> columns = null;
                if (ss.IsPagedLoading)
                {
                    // for lazily-loaded data we provide all the preloaded columns
                    // (primary keys + all the initial sort columns)
                    columns = ss.DataListLoadedColumns;
                }
                return DataTools.DatasetToHashtable(result as DataSet, columns, 
                    INITIAL_PAGE_NUMBER_OF_RECORDS, ss.CurrentRecordId, ss.DataListEntity,
                    ss);
            }
            throw new Exception(String.Format(Resources.ErrorRefreshReturnInvalid, ss.GetType().Name, SessionStore.ACTION_REFRESH));
        }

        [JsonRpcMethod]
        public ArrayList LoadData(string dataSourceId, string filterId, string sortId, 
            IDictionary<string, object> parameters)
        {
            QueryParameterCollection qparams = new QueryParameterCollection();
            foreach (KeyValuePair<string, object> entry in parameters)
            {
                qparams.Add(new QueryParameter(entry.Key, entry.Value));
            }
            DataTable table = core.DataService.LoadData(
                new Guid(dataSourceId), new Guid(filterId), Guid.Empty, 
                new Guid(sortId), null, qparams).Tables[0];
            return DataTools.DataTableToArrayList(table, SessionStore.GetColumnNames(table));
        }

        public IList CreateObject(Guid sessionFormIdentifier, string entity, IDictionary<string, object> parameters, string requestingGrid)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            IList output = ss.CreateObject(entity, null, parameters, requestingGrid);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        [JsonRpcMethod]
        public IList CreateObjectEx(string sessionFormIdentifier, string entity, IDictionary<string, object> values,
            IDictionary<string, object> parameters, string requestingGrid)
        {
            return CreateObjectEx(new Guid(sessionFormIdentifier), entity, values, parameters, requestingGrid);
        }

        public IList CreateObjectEx(Guid sessionFormIdentifier, string entity, IDictionary<string, object> values,
            IDictionary<string, object> parameters, string requestingGrid)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            IList output = ss.CreateObject(entity, values, parameters, requestingGrid);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        [JsonRpcMethod]
        public IList CopyObject(string sessionFormIdentifier, string entity, object originalId, string requestingGrid, ArrayList entities)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            IList output = ss.CopyObject(entity, originalId, requestingGrid, entities, 
                null);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        [JsonRpcMethod]
        public IList CopyObjectEx(string sessionFormIdentifier, string entity, 
            object originalId, string requestingGrid, ArrayList entities, 
            IDictionary<string, object> forcedValues)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            IList output = ss.CopyObject(entity, originalId, requestingGrid, entities, 
                forcedValues);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        /// <summary>
        /// Updates one row/field/value.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="property"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public IList UpdateObject(Guid sessionFormIdentifier, string entity, object id, string property, object newValue)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            IList output = ss.UpdateObject(entity, id, property, newValue);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        /// <summary>
        /// Updates multiple rows - one field, different values in one batch.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public IList UpdateObjectBatch(string sessionFormIdentifier, string entity, string property, Hashtable values)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            IList output = ss.UpdateObjectBatch(entity, property, values);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        /// <summary>
        /// Updates one row - multiple fields in one batch.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public IList UpdateObjectEx(string sessionFormIdentifier, string entity, object id, Hashtable values)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            IList output = ss.UpdateObjectEx(entity, id, values);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        public IList DeleteObject(Guid sessionFormIdentifier, string entity, object id)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            IList output = ss.DeleteObject(entity, id);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return output;
        }

        [JsonRpcMethod]
        public IList DeleteObjectInOrderedList(string sessionFormIdentifier, string entity, 
            object id, string orderProperty, Hashtable updatedOrderValues)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            ArrayList changes = ss.UpdateObjectBatch(entity, orderProperty, updatedOrderValues);
            changes.AddRange(ss.DeleteObject(entity, id));
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionManager.GetSessionStats()));
            return changes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionFormIdentifier">Form session id</param>
        /// <param name="entity">data entity</param>
        /// <param name="id">current record's ID in the 'entity'</param>
        /// <param name="reportId">ReportId from the form XML</param>
        /// <param name="parameters">parameters from the form XML</param>
        /// <returns>URL of the report</returns>
        [JsonRpcMethod]
        public string GetReport(string sessionFormIdentifier, string entity, 
            object id, string reportId, Hashtable parameterMappings)
        {
            return reportManager.GetReport(sessionFormIdentifier, entity,
                id, reportId, parameterMappings);
        }

        [JsonRpcMethod]
        public string GetReportStandalone(string reportId, Hashtable parameters,
			DataReportExportFormatType dataReportExportFormatType)
        {
            return reportManager.GetReportStandalone(
                reportId,  parameters,dataReportExportFormatType);
        }

        [JsonRpcMethod]
        public string GetReportFromMenu(string menuId)
        {
            return reportManager.GetReportFromMenu(new Guid(menuId));
        }

        [JsonRpcMethod]
        public string GetAttachmentDownloadUrl(object[] ids, bool isPreview)
        {
            string key = Guid.NewGuid().ToString();

            HttpContext.Current.Application[key] = new AttachmentRequest(ids, isPreview);

            return "Attachment.aspx?id=" + key;
        }

        [JsonRpcMethod]
        public string GetExport(EntityExportInfo info)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(info.SessionFormIdentifier));
            info.Store = ss;
            string key = Guid.NewGuid().ToString();
            HttpContext.Current.Application[key] = info;
            return "Export.aspx?id=" + key;
        }

        [JsonRpcMethod]
        public string GetSearchUrl(string searchString)
        {
            string key = Guid.NewGuid().ToString();

            HttpContext.Current.Application[key] = new SearchRequest(searchString, SecurityManager.CurrentPrincipal);

            return "Search.aspx?id=" + key;
        }

        [JsonRpcMethod]
        public IDictionary<string, ArrayList> GetLookupListForFilter(
            string sessionFormIdentifier, string entity, string property, 
            string lookupId)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            IDataLookupService ls = ServiceManager.Services.GetService(
                typeof(IDataLookupService)) as IDataLookupService;
            bool isLazyLoaded = ss.IsLazyLoadedEntity(entity);
            if (isLazyLoaded)
            {
                List<string> list = new List<string>();
                list.Add(property);
                // in case column was not loaded yet we load it before getting
                // the list of unique ids
                ss.LoadColumns(list);
            }
            DataTable table = ss.GetTable(entity, 
                (isLazyLoaded ? ss.DataList : ss.Data));
            DataColumn column = table.Columns[property];
            if (SessionStore.IsColumnArray(column))
            {
                entity = (string)column.ExtendedProperties[Const.ArrayRelation];
                table = table.ChildRelations[entity].ChildTable;
                property = (string)column.ExtendedProperties[Const.ArrayRelationField];
            }
            DataRowCollection rows = table.Rows;
            Hashtable keys = new Hashtable();
            foreach (DataRow r in rows)
            {
                if (r.RowState != DataRowState.Deleted)
                {
                    object pk = r[property];
                    if (pk != DBNull.Value)
                    {
                        if (!keys.Contains(pk))
                        {
                            keys.Add(pk, null);
                        }
                    }
                }
            }
            Hashtable distinctValues = ls.GetAllValuesDistinct(new Guid(lookupId), keys);
            IDictionary<string, ArrayList> resultTable = new Dictionary<string, ArrayList>(2);
            ArrayList data = new ArrayList();
            foreach (DictionaryEntry entry in distinctValues)
            {
                ArrayList test = (ArrayList)entry.Value;
                test.Add(test[0]);
                data.Add(new[] {entry.Value, entry.Key});
            }
            resultTable.Add("data", data);
            return resultTable;
        }

        [JsonRpcMethod]
        public IDictionary<string, ArrayList> GetLookupListEx(LookupListExRequest request)
        {
            if (request.SessionFormIdentifier == null)
            {
                throw new Exception("sessionFormIdentifier cannot be null");
            }
            if (request.LookupId == null)
            {
                throw new Exception("lookupId cannot be null");
            }
            SessionStore ss = sessionManager.GetSession(new Guid(request.SessionFormIdentifier));
            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) 
                as IDataLookupService;
            if (request.Id == null || String.Empty.Equals(request.Id))
            {
                // parametrized lookup in the filter - we load all the labels in the data table
                DataRowCollection rows = ss.GetTable(request.Entity, 
                    (ss.IsLazyLoadedEntity(request.Entity) ? ss.DataList : ss.Data)).Rows;
                Hashtable keys = new Hashtable();
                foreach(DataRow r in rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                    {
                        object pk = r[request.Property];
                        if (pk != DBNull.Value)
                        {
                            if (!keys.Contains(pk))
                            {
                                keys.Add(pk, null);
                            }
                        }
                    }
                }
                DataTable result = ls.GetAllValues(new Guid(request.LookupId), keys);
                return DataTools.DatatableToHashtable(result, false);
            }
            // get the list from the database
            DataRow row = ss.GetSessionRow(request.Entity, request.Id);
            Hashtable p = DictionaryToHashtable(request.Parameters);
            LookupListRequest internalRequest = new LookupListRequest();
            internalRequest.LookupId = new Guid(request.LookupId);
            internalRequest.FieldName = request.Property;
            internalRequest.ParameterMappings = p;
            internalRequest.CurrentRow = row;
            internalRequest.ShowUniqueValues = request.ShowUniqueValues;
            internalRequest.SearchText = request.SearchText;
            internalRequest.PageSize = request.PageSize;
            internalRequest.PageNumber = request.PageNumber;
            return DataTools.DatatableToHashtable(ls.GetList(internalRequest), false);
        }

        [JsonRpcMethod]
        public IDictionary<string, ArrayList> GetLookupList(string lookupId)
        {
            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

            return DataTools.DatatableToHashtable(ls.GetList(new Guid(lookupId), null).ToTable(), true);
        }

        /// <summary>
        /// Provides an array of keys on which the lookup is dependent. These keys are also provided in screen's
        /// data sources. When data are saved in the screen, lookup caches should immediately expire.
        /// </summary>
        /// <param name="lookupId"></param>
        /// <returns></returns>
        [JsonRpcMethod]
        public IList<string> GetLookupCacheDependencies(string lookupId)
        {
            // TODO: Later we can implement multiple dependencies e.g. for entities based on views where we
            // could name all the source entities/tables on which the view is dependent so even view based
            // lookups could be handled correctly and reset when source table change.
            List<string> result = new List<string>();
            IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataServiceDataLookup lookup = persistence.SchemaProvider.RetrieveInstance(typeof(DataServiceDataLookup), new ModelElementKey(new Guid(lookupId))) as DataServiceDataLookup;
            DatasetGenerator gen = new DatasetGenerator(true);
            DataSet comboListDataset = gen.CreateDataSet(lookup.ListDataStructure);
            DataTable comboListTable = comboListDataset.Tables[(lookup.ListDataStructure.ChildItemsByType(DataStructureEntity.ItemTypeConst)[0] as DataStructureEntity).Name];
            string tableName = FormXmlBuilder.DatabaseTableName(comboListTable);
            if (tableName != null)
            {
                result.Add(tableName);
            }
            return result;
        }

        /// <summary>
        /// Batch request functionality for @GetLookupCacheDependencies
        /// </summary>
        /// <param name="lookupIds">Array of ids of requsted lookups</param>
        /// <returns></returns>
        [JsonRpcMethod]
        public IDictionary<string, object> GetLookupCacheDependenciesEx(object[] lookupIds)
        {
            IDictionary<string, object> dependencies 
                = new Dictionary<string, object>(lookupIds.Length);
            foreach (object lookupId in lookupIds)
            {
                dependencies.Add((string)lookupId, 
                    GetLookupCacheDependencies((string)lookupId));
            }
            return dependencies;
        }

        [JsonRpcMethod]
        public Hashtable GetLookupLabelsEx(Hashtable request)
        {
            SecurityTools.CurrentUserProfile();
            Hashtable result = new Hashtable();
            IDataLookupService lookupService = ServiceManager.Services
                .GetService(typeof(IDataLookupService)) as IDataLookupService;
            foreach (DictionaryEntry entry in request)
            {
                Guid lookupId = new Guid((string)entry.Key);
                object[] requestedIds = entry.Value as object[];
                ArrayList subResult = new ArrayList();
                foreach (object requestedId in requestedIds)
                {
                    LookupLabelResult labelResult = new LookupLabelResult();
                    labelResult.Id = requestedId;
                    object lookupResult = lookupService.GetDisplayText(
                        lookupId, requestedId, false, true, null);
                    if (lookupResult is decimal)
                    {
                        labelResult.Value = ((decimal)lookupResult).ToString("0.#");
                    }
                    else
                    {
                        labelResult.Value = lookupResult.ToString();
                    }
                    subResult.Add(labelResult);
                }
                result.Add(lookupId, subResult);
            }
            return result;
        }

        public IList GetLookupLabels(Guid lookupId, object[] ids)
        {
            SecurityTools.CurrentUserProfile();

            ArrayList result = new ArrayList();

            foreach (object id in ids)
            {
                IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

                LookupLabelResult r = new LookupLabelResult();
                r.Id = id;
                object lookupResult = ls.GetDisplayText(lookupId, id, false, true, null);
                if (lookupResult is decimal)
                {
                    r.Value = ((decimal)lookupResult).ToString("0.#");
                }
                else
                {
                    r.Value = lookupResult.ToString();
                }
                result.Add(r);
            }

            return result;
        }

        [JsonRpcMethod]
        public XmlDocument GetLookupTooltip(string lookupId, object id)
        {
            SecurityTools.CurrentUserProfile();

            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataServiceDataLookup lookup = ps.SchemaProvider.RetrieveInstance(typeof(DataStructure), new ModelElementKey(new Guid(lookupId))) as DataServiceDataLookup;

            ArrayList tooltips = lookup.Tooltips;
            return GetTooltip(id, tooltips).Xml;
        }

        private static IXmlContainer GetTooltip(object id, ArrayList tooltips)
        {
            tooltips.Sort();

            DataServiceDataTooltip tooltip = null;
            foreach (DataServiceDataTooltip tt in tooltips)
            {
                if (RuleEngine.IsFeatureOn(tt.Features) && RuleEngine.IsInRole(tt.Roles))
                {
                    tooltip = tt;
                }
            }

            if (tooltip == null) return null;

            QueryParameterCollection qparams = new QueryParameterCollection();

            if (id != null)
            {
                foreach (string paramName in tooltip.TooltipLoadMethod.ParameterReferences.Keys)
                {
                    qparams.Add(new QueryParameter(paramName, id));
                }
            }

            DataSet data = core.DataService.LoadData(tooltip.TooltipDataStructureId, tooltip.TooltipDataStructureMethodId, Guid.Empty, Guid.Empty, null, qparams);

            IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            IXsltEngine transformer = AsTransform.GetXsltEngine(
                XsltEngineType.XslTransform, persistence.SchemaProvider);
            IXmlContainer result = transformer.Transform(DataDocumentFactory.New(data), tooltip.TooltipTransformationId, new Hashtable(), new RuleEngine(null, null), null, false);

            return result;
        }

        [JsonRpcMethod]
        public XmlDocument GetRecordTooltip(string sessionFormIdentifier, string entity, object id)
        {
            if (sessionFormIdentifier == null) return null;
            CultureInfo culture = CultureInfo.CurrentCulture;

            SecurityTools.CurrentUserProfile();

            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            DataRow row = ss.GetSessionRow(entity, id);

            XmlDocument doc = new XmlDocument();
            XmlElement tooltipElement = doc.CreateElement("tooltip");
            tooltipElement.SetAttribute("title", row.Table.DisplayExpression);
            doc.AppendChild(tooltipElement);

            int y = 1;

            if (row.Table.Columns.Contains("Id"))
            {
                CreateGenericRecordTooltipCell(doc, tooltipElement, y, "Id " + row["Id"]);
                y++;
            }

            IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();

            if (row.Table.Columns.Contains("RecordCreated") && ! row.IsNull("RecordCreated"))
            {
                string profileName;

                try
                {
                    profileName = ((UserProfile)profileProvider.GetProfile((Guid)row["RecordCreatedBy"])).FullName;
                }
                catch
                {
                    profileName = row["RecordCreatedBy"].ToString();
                }

                CreateGenericRecordTooltipCell(doc, tooltipElement, y, String.Format(Resources.DefaultTooltipRecordCreated, profileName, ((DateTime)row["RecordCreated"]).ToString(culture)));
                y++;
            }

            if (row.Table.Columns.Contains("RecordUpdated"))
            {
                if (row.IsNull("RecordUpdated"))
                {
                    CreateGenericRecordTooltipCell(doc, tooltipElement, y, Resources.DefaultTooltipNoChange);
                }
                else
                {
                    string profileName;

                    try
                    {
                        profileName = ((UserProfile)profileProvider.GetProfile((Guid)row["RecordUpdatedBy"])).FullName;
                    }
                    catch
                    {
                        profileName = row["RecordUpdatedBy"].ToString();
                    }

                    CreateGenericRecordTooltipCell(doc, tooltipElement, y, String.Format(Resources.DefaultTooltipRecordUpdated, profileName, ((DateTime)row["RecordUpdated"]).ToString(culture)));
                }
                y++;
            }

            return doc;
        }

        [JsonRpcMethod]
        public XmlDocument NotificationBoxContent()
        {
            SecurityTools.CurrentUserProfile();

            XmlDocument doc = null;

            NotificationBox logoNotificationBox = LogoNotificationBox();

            if (logoNotificationBox != null)
            {
                ArrayList tooltips = logoNotificationBox.ChildItemsByType(DataServiceDataTooltip.ItemTypeConst);

                doc = GetTooltip(null, tooltips).Xml;
            }

            if (doc == null)
            {
                doc = DefaultNotificationBoxContent();
            }

            return doc;
        }

        private static NotificationBox LogoNotificationBox()
        {
            SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            NotificationBoxSchemaItemProvider provider = schema.GetProvider(typeof(NotificationBoxSchemaItemProvider)) as NotificationBoxSchemaItemProvider;

            NotificationBox logoNotificationBox = null;

            foreach (NotificationBox box in provider.ChildItems)
            {
                if (box.Type == NotificationBoxType.Logo)
                {
                    logoNotificationBox = box;
                }
            }
            return logoNotificationBox;
        }

        private static XmlDocument DefaultNotificationBoxContent()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement tooltipElement = doc.CreateElement("tooltip");
            tooltipElement.SetAttribute("title", "Notification Box");
            doc.AppendChild(tooltipElement);

            XmlElement gridElement = doc.CreateElement("cell");
            gridElement.SetAttribute("type", "grid");
            gridElement.SetAttribute("x", "0");
            gridElement.SetAttribute("y", "1");
            gridElement.SetAttribute("height", "1");
            gridElement.SetAttribute("width", "1");
            tooltipElement.AppendChild(gridElement);

            XmlElement rowElement = doc.CreateElement("row");
            gridElement.AppendChild(rowElement);

            XmlElement colElement1 = doc.CreateElement("col");
            colElement1.SetAttribute("type", "image");
            colElement1.SetAttribute("src", "logo.png");
            colElement1.InnerText = "logo";
            rowElement.AppendChild(colElement1);
            return doc;
        }

        private void CreateGenericRecordTooltipCell(XmlDocument doc, XmlElement parentElement, int y, string text)
        {
            XmlElement gridElement = doc.CreateElement("cell");
            gridElement.SetAttribute("type", "text");
            gridElement.SetAttribute("x", "0");
            gridElement.SetAttribute("y", y.ToString());
            gridElement.SetAttribute("height", "1");
            gridElement.SetAttribute("width", "1");
            gridElement.InnerText = text;
            parentElement.AppendChild(gridElement);
        }

        [JsonRpcMethod]
        public IMenuBindingResult GetLookupEditMenu(string lookupId, object value)
        {
            SecurityTools.CurrentUserProfile();

            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

            return ls.GetMenuBinding(new Guid(lookupId), value);
        }

        [JsonRpcMethod]
        public string GetMenuId(string lookupId, object value)
        {
            SecurityTools.CurrentUserProfile();

            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

            return ls.GetMenuBinding(new Guid(lookupId), value).MenuId;
        }

        public void SaveColumnConfig(Guid formPanelId, IList columnConfigurations)
        {
            SecurityTools.CurrentUserProfile();

            for (int i = 0; i < columnConfigurations.Count; i++)
            {
                UIGridColumnConfiguration config = (UIGridColumnConfiguration)columnConfigurations[i];
                OrigamPanelColumnConfigDA.PersistColumnConfig(formPanelId, config.Property, i, config.Width, config.IsHidden);
            }
        }

        public void SaveSplitPanelConfig(Guid instanceId, int position)
        {
            SecurityTools.CurrentUserProfile();

            OrigamPanelColumnConfigDA.PersistColumnConfig(instanceId, "splitPanel", 0, position, false);
        }

        [JsonRpcMethod]
        public IList RowStates(string sessionFormIdentifier, string entity, object[] ids)
        {
            return RowStates(new Guid(sessionFormIdentifier), entity, ids);
        }

        public IList RowStates(Guid sessionFormIdentifier, string entity, object[] ids)
        {
            SessionStore ss = null;
			try
			{
				ss = sessionManager.GetSession(sessionFormIdentifier);
			}
			catch (SessionExpiredException)
			{ 
				return new ArrayList();
			}
            if (ss != null)
            {
                return ss.RowStates(entity, ids);
            }
            return new ArrayList();
        }

        public Guid SaveFilter(Guid sessionFormIdentifier, string entity, Guid panelId, UIGridFilterConfiguration filter)
        {
            return SaveFilter(sessionFormIdentifier, entity, panelId, filter, false);
        }

        public void DeleteFilter(Guid filterId)
        {
            SecurityTools.CurrentUserProfile();
            OrigamPanelFilter f = OrigamPanelFilterDA.LoadFilter(filterId);
            f.PanelFilter.Rows[0].Delete();

            OrigamPanelFilterDA.PersistFilter(f);
        }

        public void SetDefaultFilter(Guid sessionFormIdentifier, string entity, Guid panelInstanceId, Guid panelId, UIGridFilterConfiguration filter)
        {
            Guid profileId = SecurityTools.CurrentUserProfile().Id;

            Guid workflowId;
            WorkflowSessionStore wss = sessionManager.GetSession(sessionFormIdentifier) as WorkflowSessionStore;
            if (wss == null)
            {
                workflowId = Guid.Empty;
            }
            else
            {
                workflowId = wss.WorkflowId;
            }

            DataSet userConfig = OrigamPanelConfigDA.LoadConfigData(panelInstanceId, workflowId, profileId);

            if (userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
            {
                OrigamPanelConfigDA.CreatePanelConfigRow(userConfig.Tables["OrigamFormPanelConfig"], panelInstanceId, workflowId, profileId, OrigamPanelViewMode.Form);
            }

            bool shouldDeleteFilter = false;
            Guid oldFilter = Guid.Empty;

            if (userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] != DBNull.Value)
            {
                shouldDeleteFilter = true;
                oldFilter = (Guid)userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"];
            }

            Guid filterId = SaveFilter(sessionFormIdentifier, entity, panelId, filter, true);

            userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] = filterId;

            OrigamPanelConfigDA.SaveUserConfig(userConfig, panelInstanceId, workflowId, profileId);

            if (shouldDeleteFilter)
            {
                DeleteFilter(oldFilter);
            }
        }

        public void ResetDefaultFilter(Guid panelInstanceId, Guid sessionFormIdentifier)
        {
            Guid profileId = SecurityTools.CurrentUserProfile().Id;

            Guid workflowId;
            WorkflowSessionStore wss = sessionManager.GetSession(sessionFormIdentifier) as WorkflowSessionStore;
            if (wss == null)
            {
                workflowId = Guid.Empty;
            }
            else
            {
                workflowId = wss.WorkflowId;
            }

            DataSet userConfig = OrigamPanelConfigDA.LoadConfigData(panelInstanceId, workflowId, profileId);

            if (userConfig.Tables["OrigamFormPanelConfig"].Rows.Count != 0)
            {
                if (userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] != DBNull.Value)
                {
                    Guid id = (Guid)userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"];

                    userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] = DBNull.Value;
                    OrigamPanelConfigDA.SaveUserConfig(userConfig, panelInstanceId, workflowId, profileId);

                    DeleteFilter(id);
                }
            }
        }

        public void SavePanelState(UIPanel panel, Guid sessionFormIdentifier)
        {
            Guid profileId = SecurityTools.CurrentUserProfile().Id;

            Guid workflowId;
            WorkflowSessionStore wss = sessionManager.GetSession(sessionFormIdentifier) as WorkflowSessionStore;
            if (wss == null)
            {
                workflowId = Guid.Empty;
            }
            else
            {
                workflowId = wss.WorkflowId;
            }

            DataSet userConfig = OrigamPanelConfigDA.LoadConfigData(panel.InstanceId, workflowId, profileId);

            if (userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
            {
                OrigamPanelConfigDA.CreatePanelConfigRow(userConfig.Tables["OrigamFormPanelConfig"], panel.InstanceId, workflowId, profileId, panel.DefaultPanelView);
            }

            userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["DefaultView"] = panel.DefaultPanelView;

            OrigamPanelConfigDA.SaveUserConfig(userConfig, panel.InstanceId, workflowId, profileId);
        }

        [JsonRpcMethod]
        public void SaveObjectConfig(string objectInstanceId, string section, string settingsData, string sessionFormIdentifier)
        {
            Guid profileId = SecurityTools.CurrentUserProfile().Id;
            Guid objectId = new Guid(objectInstanceId);

            Guid workflowId;
            WorkflowSessionStore wss = null;

            if (sessionFormIdentifier != Guid.Empty.ToString())
            {
                wss = sessionManager.GetSession(new Guid(sessionFormIdentifier)) as WorkflowSessionStore;
            }

            if (wss == null)
            {
                workflowId = Guid.Empty;
            }
            else
            {
                workflowId = wss.WorkflowId;
            }

            DataSet userConfig = OrigamPanelConfigDA.LoadConfigData(objectId, workflowId, profileId);

            if (userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
            {
                OrigamPanelConfigDA.CreatePanelConfigRow(userConfig.Tables["OrigamFormPanelConfig"], objectId, workflowId, profileId, OrigamPanelViewMode.Grid);
            }

            XmlDocument currentSettings = new XmlDocument();
            XmlDocumentFragment newSettings = currentSettings.CreateDocumentFragment();
            XmlElement newSettingsNode = currentSettings.CreateElement(section);
            newSettingsNode.InnerXml = settingsData;
            newSettings.AppendChild(newSettingsNode);

            XmlNode configNode;

            DataRow settingsRow = userConfig.Tables["OrigamFormPanelConfig"].Rows[0];

            if (settingsRow.IsNull("SettingsData"))
            {
                configNode = currentSettings.CreateElement("Configuration");
                currentSettings.AppendChild(configNode);
            }
            else
            {
                currentSettings.LoadXml((string)settingsRow["SettingsData"]);
                configNode = currentSettings.FirstChild;
            }

            foreach(XmlNode n in configNode.SelectNodes(section))
            {
                n.ParentNode.RemoveChild(n);
            }

            configNode.AppendChild(newSettings);

            settingsRow["SettingsData"] = currentSettings.OuterXml;
            OrigamPanelConfigDA.SaveUserConfig(userConfig, objectId, workflowId, profileId);
        }


        public IDictionary<string, ArrayList> GetAudit(Guid sessionId, string entity, object id)
        {
            SecurityTools.CurrentUserProfile();

            SessionStore ss = sessionManager.GetSession(sessionId);
            DataTable t = ss.GetTable(entity, ss.Data);

            Guid entityId = Guid.Empty;

            if (t.ExtendedProperties.Contains("EntityId"))
            {
                entityId = (Guid)t.ExtendedProperties["EntityId"];
            }

            DataSet log = AuditLogDA.RetrieveLogTransformed(entityId, id);

            if (log != null)
            {
                return DataTools.DatatableToHashtable(log.Tables[0], false);
            }
            return null;
        }

        [JsonRpcMethod]
        public void RemoveAttachment(object id)
        {
            InternalHandlers.GetAttachmentHandler().DeleteFile(id);
        }

        [JsonRpcMethod]
        public void UpdateAttachmentInfo(Attachment attachmentInfo)
        {
            DataRow data = AttachmentUtils.LoadAttachmentInfo(attachmentInfo.Id);
            data["Note"] = attachmentInfo.Description;
            AttachmentUtils.SaveAttachmentInfo(data);
        }


        [JsonRpcMethod]
        public string[] GetAttachmentUploadUrls(string sessionId, object recordId, string entity, int fileCount)
        {
            SessionStore ss = sessionManager.GetSession(new Guid(sessionId));
            DataTable t = ss.GetSessionEntity(entity);

            if (t.ExtendedProperties.Contains("EntityId"))
            {
                Guid entityId = (Guid)t.ExtendedProperties["EntityId"];

                string[] keys = new string[fileCount];

                for (int i = 0; i < fileCount; i++)
                {
                    string key = Guid.NewGuid().ToString();

                    HttpContext.Current.Application[key] = new AttachmentUploadRequest(recordId, entityId, SecurityManager.CurrentPrincipal);

                    keys[i] = "UploadAttachment.aspx?id=" + key;
                }

                return keys;
            }
            throw new NotSupportedException(String.Format(Resources.ErrorAttachmentsNotSupported, entity));
        }

        [JsonRpcMethod]
        public string GetBlobUploadUrl(string sessionFormIdentifier, string entity, string property, string id, DateTime dateCreated, DateTime dateLastModified, IDictionary parameters)
        {
            string key = Guid.NewGuid().ToString();
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            DataRow row = ss.GetSessionRow(entity, id);

            HttpContext.Current.Application[key] = new BlobUploadRequest(row, SecurityManager.CurrentPrincipal, parameters, dateCreated, dateLastModified, property);

            return "UploadBlob.aspx?id=" + key;
        }

        [JsonRpcMethod]
        public string GetBlobDownloadUrl(string sessionFormIdentifier, string entity, string property, string id, bool isPreview, IDictionary parameters)
        {

            string key = Guid.NewGuid().ToString();
            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            DataRow row = ss.GetSessionRow(entity, id);

            HttpContext.Current.Application[key] = new BlobDownloadRequest(row, parameters, property, isPreview);

            return "DownloadBlob.aspx?id=" + key;
        }

        [JsonRpcMethod]
        public long AttachmentCount(string sessionId, string entity, object id)
        {
            SecurityTools.CurrentUserProfile();
            SessionStore ss = null;
            try
            {
                ss = sessionManager.GetSession(new Guid(sessionId));
            }
            catch
            {
            }
            if (ss == null)
            {
                return 0;
            }

            DataTable t = ss.GetTable(entity, ss.Data);
            Guid entityId = Guid.Empty;
            if (t.ExtendedProperties.Contains("EntityId"))
            {
                entityId = (Guid)t.ExtendedProperties["EntityId"];
            }
            long result = 0;
            List<object> idList = new List<object>();
            // We catch any problems with reading record ids (they could have been unloaded by another request
            // and we don't want to hear messages about this.
            try
            {
                ChildrenRecordsIds(idList, ss.GetSessionRow(entity, id));
            }
            catch {
                return 0;
            }
            if (idList.Count > 500) return -1;
            foreach (object recordId in idList)
            {
                IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
                long oneRecordCount = (long)ls.GetDisplayText(new Guid("fbf2cadd-e529-401d-80ce-d68de0a89f13"), recordId, false, false, null);

                result += oneRecordCount;
            }
            return result;
        }

        [JsonRpcMethod]
        public IList<Attachment> AttachmentList(string sessionId, string entity, object id)
        {
            SecurityTools.CurrentUserProfile();
            SessionStore ss = sessionManager.GetSession(new Guid(sessionId));
            DataTable t = ss.GetTable(entity, ss.Data);
            Guid entityId = Guid.Empty;
            if (t.ExtendedProperties.Contains("EntityId"))
            {
                entityId = (Guid)t.ExtendedProperties["EntityId"];
            }
            List<Attachment> result = new List<Attachment>();
            List<object> idList = new List<object>();
            ChildrenRecordsIds(idList, ss.GetSessionRow(entity, id));
            foreach (object recordId in idList)
            {
                DataSet oneRecordList = core.DataService.LoadData(new Guid("44a25061-750f-4b42-a6de-09f3363f8621"), new Guid("0fda540f-e5de-4ab6-93d2-76b0abe6fd77"), Guid.Empty, Guid.Empty, null, "Attachment_parRefParentRecordId", recordId);
                foreach (DataRow row in oneRecordList.Tables[0].Rows)
                {
                    string user = "";
                    if (! row.IsNull("RecordCreatedBy"))
                    {
                        try
                        {
                            IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
                            UserProfile profile = profileProvider.GetProfile((Guid)row["RecordCreatedBy"]) as UserProfile;
                            user = profile.FullName;
                        }
                        catch (Exception ex)
                        {
                            user = ex.Message;
                        }
                    }
                    Attachment att = new Attachment();
                    att.Id = row["Id"].ToString();
                    att.CreatorName = user;
                    att.DateCreated = (DateTime)row["RecordCreated"];
                    if (!row.IsNull("Note"))
                    {
                        att.Description = (string)row["Note"];
                    }
                    att.FileName = (string)row["FileName"];
                    string extension = Path.GetExtension(att.FileName).ToLower();
                    att.Icon = "extensions/" + extension + ".png";
                    result.Add(att);
                }
            }
            return result;
        }

        [JsonRpcMethod]
        public IList<WorkQueueInfo> WorkQueueList()
        {
            try
            {
                IList<WorkQueueInfo> result = new List<WorkQueueInfo>();

                // if the user is not logged on, we will gracefully finish by returning an empty list
                try
                {
                    SecurityTools.CurrentUserProfile();
                }
                catch
                {
                    return result;
                }


                IWorkQueueService wqs = ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
                IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

                DataSet data = wqs.UserQueueList();
                DataTable queueList = data.Tables["WorkQueue"];

                foreach (DataRow row in queueList.Rows)
                {
                    object workQueueId = row["Id"];
                    string wqClassName = (string)row["WorkQueueClass"];

                    long cnt = 0;

                    if ((bool)row["IsMessageCountDisplayed"])
                    {
                        WorkQueueClass wqc = wqs.WQClass(wqClassName) as WorkQueueClass;
                        cnt = (long)ls.GetDisplayText(wqc.WorkQueueItemCountLookupId, workQueueId, false, false, null);
                    }

                    WorkQueueInfo wqi = new WorkQueueInfo(workQueueId.ToString(), (string)row["Name"], cnt);

                    result.Add(wqi);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(Resources.ErrorLoadingWorkQueueList, ex);
            }
        }

        public static IList<DashboardToolboxItem> DashboardToolbox()
        {
            IList<DashboardToolboxItem> items = new List<DashboardToolboxItem>();

            SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            DashboardWidgetsSchemaItemProvider provider = schema.GetProvider(typeof(DashboardWidgetsSchemaItemProvider)) as DashboardWidgetsSchemaItemProvider;

            AddDashboardItems(provider, items);

            return items;
        }

        private static void AddDashboardItems(ISchemaItemProvider parent, IList<DashboardToolboxItem> items)
        {
            ArrayList widgets = parent.ChildItemsByType(AbstractDashboardWidget.ItemTypeConst);
            foreach (AbstractDashboardWidget widget in widgets)
            {
                DashboardToolboxItem item = new DashboardToolboxItem();
                item.Caption = widget.Caption;
                item.Id = widget.Id;

                if (widget is DashboardWidgetFolder)
                {
                    item.Type = DashboardToolboxItemType.Folder;
                    AddDashboardItems(widget, item.ChildWidgets);
                }
                else if (widget is HorizontalContainerDashboardWidget)
                {
                    item.Type = DashboardToolboxItemType.HBox;
                }
                else if (widget is VerticalContainerDashboardWidget)
                {
                    item.Type = DashboardToolboxItemType.VBox;
                }
                else
                {
                    // parameters
                    foreach (DashboardWidgetParameter param in widget.ChildItemsByType(DashboardWidgetParameter.ItemTypeConst))
                    {
                        DashboardToolboxItemParameter newParameter = new DashboardToolboxItemParameter(param.Name, param.Caption, param.DataType, param.LookupId);

                        if (param.Lookup != null)
                        {
                            XmlDocument doc = new XmlDocument();
                            XmlElement controlElement = doc.CreateElement("control");
                            doc.AppendChild(controlElement);

                            ComboBoxBuilder.Build(controlElement, param.LookupId, false, null, null);

                            newParameter.ControlDefinition = doc;
                        }

                        item.Parameters.Add(newParameter);
                    }

                    // properties
                    foreach (DashboardWidgetProperty prop in widget.Properties)
                    {
                        item.Properties.Add(prop);
                    }
                }

                items.Add(item);
            }
        }

        public static string SaveDashboardViewConfig(string viewId, string menuId, string name, string roles, string config)
        {
            Guid profileId = SecurityTools.CurrentUserProfile().Id;
            DataSet data;

            if (viewId == null)
            {
                DatasetGenerator dg = new DatasetGenerator(true);
                IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
                DataStructure ds = ps.SchemaProvider.RetrieveInstance(typeof(DataStructure), new ModelElementKey(new Guid("4c519d8d-445c-4cf6-9d40-2e75d9c095be"))) as DataStructure;
                data = dg.CreateDataSet(ds);
            }
            else
            {
                data = LoadDashboardView(viewId);
            }

            DataTable table = data.Tables["OrigamDashboardView"];
            DataRow row;

            if (viewId == null)
            {
                row = table.NewRow();
                row["Id"] = Guid.NewGuid();
                row["RecordCreated"] = DateTime.Now;
                row["RecordCreatedBy"] = profileId;
                row["MenuId"] = menuId;
            }
            else
            {
                row = table.Rows[0];
                row["RecordUpdated"] = DateTime.Now;
                row["RecordUpdatedBy"] = profileId;
            }

            row["Name"] = name;
            row["Roles"] = roles;
            row["ConfigXml"] = config;

            if (viewId == null)
            {
                table.Rows.Add(row);
            }

            SaveDashboardView(data);

            return ((Guid)row["Id"]).ToString();
        }

        public static void DeleteDashboardView(string viewId)
        {
            DataSet data = LoadDashboardView(viewId);

            DataTable table = data.Tables["OrigamDashboardView"];
            DataRow row = table.Rows[0];

            row.Delete();

            SaveDashboardView(data);
        }

        private static void SaveDashboardView(DataSet data)
        {
            core.DataService.StoreData(new Guid("4c519d8d-445c-4cf6-9d40-2e75d9c095be"), data, false, null);
        }

        private static DataSet LoadDashboardView(string viewId)
        {
            DataSet data;
            data = core.DataService.LoadData(new Guid("4c519d8d-445c-4cf6-9d40-2e75d9c095be"),
                new Guid("6f44f576-258f-4998-95ca-ecaaff9e7265"), Guid.Empty, Guid.Empty, null,
                "OrigamDashboardView_parId", viewId);
            return data;
        }

        private void ChildrenRecordsIds(List<object> list, DataRow row)
        {
            if (row.Table.PrimaryKey.Length == 1 && row.Table.PrimaryKey[0].DataType == typeof(Guid))
            {
                list.Add(row[row.Table.PrimaryKey[0]]);

                foreach (DataRelation childRelation in row.Table.ChildRelations)
                {
                    foreach (DataRow childRow in row.GetChildRows(childRelation))
                    {
                        ChildrenRecordsIds(list, childRow);
                    }
                }
            }
        }

        [JsonRpcMethod]
        public void SaveFavorites(string favoritesXml)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();

            // save favorites
            DataSet favorites = core.DataService.LoadData(new Guid("e564c554-ca83-47eb-980d-95b4faba8fb8"), new Guid("e468076e-a641-4b7d-b9b4-7d80ff312b1c"), Guid.Empty, Guid.Empty, null, "OrigamFavoritesUserConfig_parBusinessPartnerId", profile.Id);
            if (favorites.Tables["OrigamFavoritesUserConfig"].Rows.Count > 0)
            {
                DataRow row = favorites.Tables["OrigamFavoritesUserConfig"].Rows[0];
                row["ConfigXml"] = favoritesXml;
                row["RecordUpdated"] = DateTime.Now;
                row["RecordUpdatedBy"] = profile.Id;
                row["refBusinessPartnerId"] = profile.Id;
            }
            else
            {
                DataRow row = favorites.Tables["OrigamFavoritesUserConfig"].NewRow();
                row["Id"] = Guid.NewGuid();
                row["RecordCreated"] = DateTime.Now;
                row["RecordCreatedBy"] = profile.Id;
                row["ConfigXml"] = favoritesXml;
                row["refBusinessPartnerId"] = profile.Id;

                favorites.Tables["OrigamFavoritesUserConfig"].Rows.Add(row);
            }

            core.DataService.StoreData(new Guid("e564c554-ca83-47eb-980d-95b4faba8fb8"), favorites, false, null);
        }

        [JsonRpcMethod]
        public HelpTooltip SaveTooltipAndNext(string saveTooltipId)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();

            // save the old tooltip
            DatasetGenerator dg = new DatasetGenerator(true);
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure ds = ps.SchemaProvider.RetrieveInstance(typeof(DataStructure), new ModelElementKey(new Guid("b68edcfc-45de-48f6-b842-c0d8f8b4fc24"))) as DataStructure;
            DataSet data = dg.CreateDataSet(ds);

            DataTable table = data.Tables["OrigamTooltipHelpUsage"];
            DataRow row = table.NewRow();
            row["Id"] = Guid.NewGuid();
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = profile.Id;
            row["refBusinessPartnerId"] = profile.Id;
            row["refOrigamTooltipHelpId"] = new Guid(saveTooltipId);
            table.Rows.Add(row);

            core.DataService.StoreData(new Guid("b68edcfc-45de-48f6-b842-c0d8f8b4fc24"), data, false, null);

            // get the formId of the former tooltip (continue the tooltip workflow on the current form)
            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
            string formId = ls.GetDisplayText(new Guid("4883e3ff-3dee-4401-961c-70f3fe353998"), saveTooltipId, null).ToString();

            // get next tooltip
            return ToolTipTools.NextTooltip(formId);
        }

        #endregion

        #region Private Static Methods

        private Guid SaveFilter(Guid sessionFormIdentifier, string entity, Guid panelId, UIGridFilterConfiguration filter, bool isDefault)
        {
            SessionStore ss = sessionManager.GetSession(sessionFormIdentifier);
            DataTable table = ss.GetTable(entity, ss.Data);
            
            Guid profileId = SecurityTools.CurrentUserProfile().Id;

            OrigamPanelFilter storedFilter = new OrigamPanelFilter();

            OrigamPanelFilter.PanelFilterRow fr = storedFilter.PanelFilter.NewPanelFilterRow();
            fr.Id = Guid.NewGuid();
            fr.Name = filter.Name;
            fr.IsGlobal = filter.IsGlobal;
            fr.IsDefault = isDefault;
            fr.PanelId = panelId;
            fr.ProfileId = profileId;
            fr.RecordCreated = DateTime.Now;
            fr.RecordCreatedBy = profileId;

            storedFilter.PanelFilter.Rows.Add(fr);

            foreach (UIGridFilterFieldConfiguration filterDetail in filter.Details)
            {
                if (table.Columns.Contains(filterDetail.Property))
                {
                    if (table.Columns[filterDetail.Property].DataType == typeof(Guid))
                    {
                        FilterOperator op = (FilterOperator)filterDetail.Operator;

                        if (filterDetail.Value1 != null && filterDetail.Value1 is string)
                        {
                            try
                            {
                                filterDetail.Value1 = new Guid((string)filterDetail.Value1);
                            }
                            catch (FormatException)
                            { }
                        }

                        if (filterDetail.Value2 != null && filterDetail.Value1 is string && op != FilterOperator.Equals && op != FilterOperator.NotEquals)
                        {
                            try
                            {
                                filterDetail.Value2 = new Guid((string)filterDetail.Value2);
                            }
                            catch (FormatException)
                            {
                            }
                        }
                    }

                    OrigamPanelFilterDA.AddPanelFilterDetailRow(storedFilter, profileId, fr.Id, filterDetail.Property, filterDetail.Operator, filterDetail.Value1, filterDetail.Value2);
                }
            }

            OrigamPanelFilterDA.PersistFilter(storedFilter);

            return fr.Id;
        }

        #region IUIService Members


        [JsonRpcMethod]
        public void DestroyUI(string sessionFormIdentifier)
        {
            DestroyUI(new Guid(sessionFormIdentifier));
        }

        [JsonRpcMethod]
        public IList SaveData(string sessionFormIdentifier)
        {
            return SaveData(new Guid(sessionFormIdentifier));
        }

        [JsonRpcMethod]
        public ArrayList GetData(string sessionFormIdentifier, string childEntity, object parentRecordId, object rootRecordId)
        {
            return GetData(new Guid(sessionFormIdentifier), childEntity, parentRecordId, rootRecordId);
        }

        [JsonRpcMethod]
        public IDictionary<string, object> GetSessionData(string sessionFormIdentifier)
        {
            return GetSessionData(new Guid(sessionFormIdentifier));
        }

        [JsonRpcMethod]
        public IDictionary<string, object> RefreshData(string sessionFormIdentifier)
        {
            return RefreshData(new Guid(sessionFormIdentifier));
        }

        public IDictionary<string, object> RefreshData(string sessionFormIdentifier, Hashtable parameters)
        {
            return RefreshData(new Guid(sessionFormIdentifier), parameters);
        }

        [JsonRpcMethod]
        public IList CreateObject(string sessionFormIdentifier, string entity, IDictionary<string, object> parameters, string requestingGrid)
        {
            return CreateObject(new Guid(sessionFormIdentifier), entity, parameters, requestingGrid);
        }

        [JsonRpcMethod]
        public void UpdateTextField(string sessionFormIdentifier, string entity, object id, string property, object newValue)
        {
            UpdateObject(new Guid(sessionFormIdentifier), entity, id, property, newValue);
        }

        [JsonRpcMethod]
        public IList UpdateObject(string sessionFormIdentifier, string entity, object id, string property, object newValue)
        {
            return UpdateObject(new Guid(sessionFormIdentifier), entity, id, property, newValue);
        }

        [JsonRpcMethod]
        public IList DeleteObject(string sessionFormIdentifier, string entity, object id)
        {
            return DeleteObject(new Guid(sessionFormIdentifier), entity, id);
        }

        [JsonRpcMethod]
        public IList GetLookupLabels(string lookupId, object[] ids)
        {
            return GetLookupLabels(new Guid(lookupId), ids);
        }

        [JsonRpcMethod]
        public void SaveColumnConfig(string formPanelId, IList columnConfigurations)
        {
            SaveColumnConfig(new Guid(formPanelId), columnConfigurations);
        }

        [JsonRpcMethod]
        public void SaveSplitPanelConfig(string instanceId, int position)
        {
            SaveSplitPanelConfig(new Guid(instanceId), position);
        }

        [JsonRpcMethod]
        public Guid SaveFilter(string sessionFormIdentifier, string entity, string panelId, UIGridFilterConfiguration filter)
        {
            return SaveFilter(new Guid(sessionFormIdentifier), entity, new Guid(panelId), filter);
        }

        [JsonRpcMethod]
        public void DeleteFilter(string filterId)
        {
            DeleteFilter(new Guid(filterId));
        }

        [JsonRpcMethod]
        public void SetDefaultFilter(string sessionFormIdentifier, string entity, string panelInstanceId, string panelId, UIGridFilterConfiguration filter)
        {
            SetDefaultFilter(new Guid(sessionFormIdentifier), entity, new Guid(panelInstanceId), new Guid(panelId), filter);
        }

        [JsonRpcMethod]
        public void ResetDefaultFilter(string panelInstanceId, string sessionFormIdentifier)
        {
            ResetDefaultFilter(new Guid(panelInstanceId), new Guid(sessionFormIdentifier));
        }

        [JsonRpcMethod]
        public void SavePanelState(UIPanel panel, string sessionFormIdentifier)
        {
            SavePanelState(panel, new Guid(sessionFormIdentifier));
        }

        [JsonRpcMethod]
        public IDictionary<string, ArrayList> GetAudit(string sessionId, string entity, object id)
        {
            return GetAudit(new Guid(sessionId), entity, id);
        }

        [JsonRpcMethod]
        public UIResult WorkflowNext(string sessionFormIdentifier, List<string> cachedFormIds)
        {
            return WorkflowNext(new Guid(sessionFormIdentifier), cachedFormIds);
        }

        [JsonRpcMethod]
        public UIResult WorkflowAbort(string sessionFormIdentifier)
        {
            return WorkflowAbort(new Guid(sessionFormIdentifier));
        }

        [JsonRpcMethod]
        public UIResult WorkflowRepeat(string sessionFormIdentifier)
        {
            return WorkflowRepeat(new Guid(sessionFormIdentifier));
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
        #endregion
    }

    #endregion

}
