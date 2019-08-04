using Microsoft.Extensions.Localization;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Server;
using Origam.ServerCommon;
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
                foreach(SessionStore mainSS in portalSessionStore.FormSessions)
                {
                    if(clearAll)
                    {
                        sessionsToDestroy.Add(mainSS.Id);
                    }
                    else if(sessionManager.HasFormSession(mainSS.Id))
                    {
                        SessionStore ss = mainSS.ActiveSession ?? mainSS;
                        if((ss is SelectionDialogSessionStore)
                            || ss.IsModalDialog)
                        {
                            sessionsToDestroy.Add(ss.Id);
                        }
                        else
                        {
                            bool askWorkflowClose = false;
                            if(ss is WorkflowSessionStore wss)
                            {
                                askWorkflowClose = wss.AskWorkflowClose;
                            }
                            bool hasChanges = HasChanges(ss);
                            result.Sessions.Add(
                                new PortalResultSession(
                                    ss.Id, 
                                    ss.Request.ObjectId, 
                                    hasChanges, 
                                    ss.Request.Type, 
                                    ss.Request.Caption, 
                                    ss.Request.Icon, 
                                    askWorkflowClose));
                        }
                    }
                    else
                    {
                        // session is registered in the user's portal, 
                        // but not in the UIService anymore,
                        // we have to destroy it
                        sessionsToDestroy.Add(mainSS.Id);
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
                PortalSessionStore pss = new PortalSessionStore(profile.Id);
                sessionManager.AddPortalSession(profile.Id, pss);
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
        private static bool HasChanges(SessionStore ss)
        {
            bool hasChanges = false;
            if((ss is FormSessionStore) 
                && (ss.Data != null) 
                && ss.Data.HasChanges())
            {
                hasChanges = true;
            }
            if((ss is WorkflowSessionStore wss)
                && wss.AllowSave &&
                (ss.Data != null)
                && ss.Data.HasChanges())
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
