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

using Microsoft.Extensions.Localization;
using Origam.DA;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Server;
using Origam.ServerCommon;
using Origam.ServerCore.Model.UIService;
using Origam.Workbench.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public class ServerCoreUIService : IBasicUIService
    {
        private const int INITIAL_PAGE_NUMBER_OF_RECORDS = 50;

        private readonly UIManager uiManager;
        private readonly SessionManager sessionManager;
        private readonly SessionHelper sessionHelper;

        public ServerCoreUIService(
            UIManager uiManager, SessionManager sessionManager)
        {
            this.uiManager = uiManager;
            this.sessionManager = sessionManager;
            sessionHelper = new SessionHelper(sessionManager);
        }

        public string GetReportStandalone(
            string reportId, 
            Hashtable parameters, 
            DataReportExportFormatType dataReportExportFormatType)
        {
            throw new NotImplementedException();
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
            UserProfile profile = SecurityTools.CurrentUserProfile();
            PortalResult result = new PortalResult(MenuXmlBuilder.GetMenu());
            OrigamSettings settings 
                = ConfigurationManager.GetActiveConfiguration();
            if(settings != null)
            {
                result.WorkQueueListRefreshInterval 
                    = settings.WorkQueueListRefreshPeriod * 1000;
                result.Slogan = settings.Slogan;
                result.HelpUrl = settings.HelpUrl;
            }
            result.MaxRequestLength = maxRequestLength;
            NotificationBox logoNotificationBox = LogoNotificationBox();
            if(logoNotificationBox != null)
            {
                result.NotificationBoxRefreshInterval 
                    = logoNotificationBox.RefreshInterval * 1000;
            }
            // load favorites
            DataSet favorites = core.DataService.LoadData(
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
            if(sessionManager.HasPortalSession(profile.Id))
            {
                var portalSessionStore = sessionManager.GetPortalSession(
                    profile.Id);
                bool clearAll = portalSessionStore.ShouldBeCleared();
                // running session, we get all the form sessions
                ArrayList sessionsToDestroy = new ArrayList();
                foreach(SessionStore mainSessionStore 
                    in portalSessionStore.FormSessions)
                {
                    if(clearAll)
                    {
                        sessionsToDestroy.Add(mainSessionStore.Id);
                    }
                    else if(sessionManager.HasFormSession(mainSessionStore.Id))
                    {
                        SessionStore sessionStore 
                            = mainSessionStore.ActiveSession ?? mainSessionStore;
                        if((sessionStore is SelectionDialogSessionStore)
                            || sessionStore.IsModalDialog)
                        {
                            sessionsToDestroy.Add(sessionStore.Id);
                        }
                        else
                        {
                            bool askWorkflowClose = false;
                            if(sessionStore 
                                is WorkflowSessionStore workflowSessionStore)
                            {
                                askWorkflowClose 
                                    = workflowSessionStore.AskWorkflowClose;
                            }
                            bool hasChanges = HasChanges(sessionStore);
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
                    DestroyUI(id);
                }
                if(clearAll)
                {
                    portalSessionStore.ResetSessionStart();
                }
            }
            else
            {
                // new session
                PortalSessionStore portalSessionStore 
                    = new PortalSessionStore(profile.Id);
                sessionManager.AddPortalSession(
                    profile.Id, portalSessionStore);
            }
            result.UserName = profile.FullName 
                + " (" + sessionManager.PortalSessionCount + ")";
            result.Tooltip = ToolTipTools.NextTooltip();
            CreateUpdateOrigamOnlineUser();
            return result;
        }
        public void DestroyUI(Guid sessionFormIdentifier)
        {
            sessionHelper.DeleteSession(sessionFormIdentifier);
            CreateUpdateOrigamOnlineUser();
        }
        public IDictionary<string, object> RefreshData(
            Guid sessionFormIdentifier,
            IStringLocalizer<SharedResources> localizer)
        {
            SessionStore sessionStore 
                = sessionManager.GetSession(sessionFormIdentifier);
            object result = sessionStore.ExecuteAction(
                SessionStore.ACTION_REFRESH);
            CreateUpdateOrigamOnlineUser();
            if (result is DataSet)
            {
                IList<string> columns = null;
                if (sessionStore.IsPagedLoading)
                {
                    // for lazily-loaded data we provide all the preloaded columns
                    // (primary keys + all the initial sort columns)
                    columns = sessionStore.DataListLoadedColumns;
                }
                return DataTools.DatasetToHashtable(result as DataSet, columns, 
                    INITIAL_PAGE_NUMBER_OF_RECORDS, sessionStore.CurrentRecordId, 
                    sessionStore.DataListEntity, sessionStore);
            }
            throw new Exception(localizer["ErrorRefreshReturnInvalid", 
                sessionStore.GetType().Name, SessionStore.ACTION_REFRESH]);

        }
        public RuleExceptionDataCollection SaveDataQuery(Guid sessionFormIdentifier)
        {
            SessionStore sessionStore = sessionManager.GetSession(
                sessionFormIdentifier);
            if((sessionStore.ConfirmationRule != null) 
                && sessionStore.Data.HasChanges())
            {
                bool hasRows = false;
                DataSet clone = DatasetTools.CloneDataSet(sessionStore.Data);
                List<DataTable> rootTables = new List<DataTable>();
                foreach(DataTable table in sessionStore.Data.Tables)
                {
                    if(table.ParentRelations.Count == 0)
                    {
                        rootTables.Add(table);
                    }
                }
                foreach(DataTable rootTable in rootTables)
                {
                    foreach(DataRow row in rootTable.Rows)
                    {
                        if((row.RowState != DataRowState.Deleted) 
                            && IsRowDirty(row))
                        {
                            DatasetTools.GetDataSlice(
                                clone, new List<DataRow>{row});
                            hasRows = true;
                        }
                    }
                }
                if(hasRows)
                {
                    IDataDocument xmlDoc = DataDocumentFactory.New(clone);
                    return sessionStore.RuleEngine.EvaluateEndRule(
                        sessionStore.ConfirmationRule, xmlDoc);
                }
                return new RuleExceptionDataCollection();
            }
            return new RuleExceptionDataCollection();
        }
        public IList SaveData(Guid sessionFormIdentifier)
        {
            SessionStore sessionStore = sessionManager.GetSession(
                sessionFormIdentifier);
            IList output = (IList)sessionStore.ExecuteAction(
                SessionStore.ACTION_SAVE);
            CreateUpdateOrigamOnlineUser();
            return output;
        }
        public IList CreateObject(CreateObjectData data)
        {
            SessionStore sessionStore 
                = sessionManager.GetSession(data.SessionFormIdentifier);
            // todo: propagate requesting grid as guid?
            IList output = sessionStore.CreateObject(
                data.Entity, data.Values, data.Parameters, 
                data.RequestingGridId.ToString());
            CreateUpdateOrigamOnlineUser();
            return output;
        }
        private bool IsRowDirty(DataRow row)
        {
            if(row.RowState != DataRowState.Unchanged)
            {
                return true;
            }
            foreach(DataRelation childRelation in row.Table.ChildRelations)
            {
                foreach(DataRow childRow in row.GetChildRows(childRelation))
                {
                    if(IsRowDirty(childRow))
                    {
                        return true;
                    }
                }
				// look for deleted children. They aren't returned by
				// previous ChetChildRows call. 
				foreach(DataRow childRow in row.GetChildRows(childRelation,
					DataRowVersion.Original))
				{
					if(childRow.RowState == DataRowState.Deleted)
					{
						return true;
					}
				}
			}
            return false;
        }
        private static NotificationBox LogoNotificationBox()
        {
            SchemaService schema 
                = ServiceManager.Services.GetService<SchemaService>();
            NotificationBoxSchemaItemProvider provider 
                = schema.GetProvider<NotificationBoxSchemaItemProvider>();
            NotificationBox logoNotificationBox = null;
            foreach(NotificationBox box in provider.ChildItems)
            {
                if(box.Type == NotificationBoxType.Logo)
                {
                    logoNotificationBox = box;
                }
            }
            return logoNotificationBox;
        }
        private static bool HasChanges(SessionStore sessionStore)
        {
            bool hasChanges = false;
            if((sessionStore is FormSessionStore) 
                && (sessionStore.Data != null) 
                && sessionStore.Data.HasChanges())
            {
                hasChanges = true;
            }
            if((sessionStore is WorkflowSessionStore workflowSessionStore)
                && workflowSessionStore.AllowSave &&
                (sessionStore.Data != null)
                && sessionStore.Data.HasChanges())
            {
                hasChanges = true;
            }
            return hasChanges;
        }
        private void CreateUpdateOrigamOnlineUser()
        {
            var principal = Thread.CurrentPrincipal;
            Task.Run(() =>
            {
                Thread.CurrentPrincipal = principal;
                SecurityTools.CreateUpdateOrigamOnlineUser(
                    SecurityManager.CurrentPrincipal.Identity.Name,
                    sessionManager.GetSessionStats());
            });
        }
    }
}
