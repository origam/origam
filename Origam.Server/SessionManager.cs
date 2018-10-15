using System;
using System.Collections;
using System.Collections.Generic;
using Origam;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.Gui;

namespace Origam.Server
{
    class SessionManager
    {
        private readonly Dictionary<Guid, PortalSessionStore> portalSessions;
        private readonly Dictionary<Guid, SessionStore> formSessions;

        public SessionManager()
        {    
            portalSessions = (Dictionary<Guid, PortalSessionStore>)FluorineFx.Context.FluorineContext.Current.ApplicationState["portals"];
            if (portalSessions == null)
            {
                portalSessions = new Dictionary<Guid, PortalSessionStore>();
                FluorineFx.Context.FluorineContext.Current.ApplicationState["portals"] = portalSessions;
            }

            formSessions = (Dictionary<Guid, SessionStore>)FluorineFx.Context.FluorineContext.Current.ApplicationState["forms"];
            if (formSessions == null)
            {
                formSessions = new Dictionary<Guid, SessionStore>();
                FluorineFx.Context.FluorineContext.Current.ApplicationState["forms"] = formSessions;
            }
        }
        
        public int PortalSessionCount => portalSessions.Count;
        
        public void RemoveFormSession(Guid sessionFormIdentifier)
        {
            lock (((IDictionary)formSessions).SyncRoot)
            {
                formSessions.Remove(sessionFormIdentifier);
            }
        }

        public void RemovePortalSession(Guid id)
        {
            portalSessions.Remove(id);
        }

        public void AddPortalSession(Guid id,PortalSessionStore portalSessionStore )
        {
            portalSessions.Add(id, portalSessionStore);
        }


        public PortalSessionStore GetPortalSession(Guid id)
        {
            return portalSessions[id];
        }

        public PortalSessionStore GetPortalSession()
        {
            Guid profileId = SecurityTools.CurrentUserProfile().Id;

            if (!portalSessions.ContainsKey(profileId))
            {
                throw new SessionExpiredException();
            } 
            else
            {
                return portalSessions[profileId];
            }
        }

        public SessionStore GetSession(Guid sessionFormIdentifier)
        {
            return GetSession(sessionFormIdentifier, false);
        }

        public SessionStore GetSession(ExecuteActionProcessData processData)
        {
            return GetSession(new Guid(processData.SessionFormIdentifier));
        }

        public bool SessionExists(Guid sessionFormIdentifier)
        {
            lock (((IDictionary)formSessions).SyncRoot)
            {
                return formSessions.ContainsKey(sessionFormIdentifier);
            }
        }

        public SessionStore GetSession(Guid sessionFormIdentifier, bool rootSession)
        {
            SecurityTools.CurrentUserProfile();

            SessionStore ss;

            try
            {
                lock (((IDictionary)formSessions).SyncRoot)
                {
                    ss = formSessions[sessionFormIdentifier];
                }
            }
            catch (KeyNotFoundException)
            {
                throw new SessionExpiredException();
            }

            if (ss == null)
            {
                throw new Exception(string.Format(Properties.Resources.ErrorSessionNotFound, sessionFormIdentifier.ToString()));
            }

            ss.CacheExpiration.AddMinutes(5);

            // check for an active sub-session - return it
            if (! rootSession && ss.ActiveSession != null)
            {
                return ss.ActiveSession;
            }

            return ss;
        }
        
        public void RegisterSession(SessionStore ss)
        {
            lock (((IDictionary)formSessions).SyncRoot)
            {
                PortalSessionStore pss = GetPortalSession();
                if (pss.IsExclusiveScreenOpen
                    && ! pss.ExclusiveSession.Equals(ss))
                {
                    throw new RuleException(string.Format(Properties.Resources.ErrorCannotOpenScreenWhenExclusiveIsOpen,
                        pss.ExclusiveSession.Title));
                }
                formSessions[ss.Id] = ss;
                if (!ss.Request.IsStandalone)
                {
                    if (!pss.FormSessions.Contains(ss))
                    {
                        pss.IsExclusiveScreenOpen = ss.IsExclusive;
                        pss.FormSessions.Add(ss);
                    }
                }
            }
        }
        public SessionStats GetSessionStats()
        {
            int dirtyScreens = 0;
            int runningWorkflows = 0;
            UserProfile profile = SecurityTools.CurrentUserProfile();
            foreach (SessionStore mainSessionStore
                in portalSessions[profile.Id].FormSessions)
            {
                if (formSessions.ContainsKey(mainSessionStore.Id))
                {
                    SessionStore sessionStore 
                        = (mainSessionStore.ActiveSession == null 
                            ? mainSessionStore 
                            : mainSessionStore.ActiveSession);
                    if (sessionStore is FormSessionStore) {
                        if (sessionStore.HasChanges())
                        {
                            dirtyScreens++;
                        }
                    } 
                    else if (sessionStore is WorkflowSessionStore) 
                    {
                        if (sessionStore.HasChanges())
                        {
                            dirtyScreens++;
                        }
                        if (!(sessionStore as WorkflowSessionStore).IsFinalForm)
                        {
                            runningWorkflows++;
                        }
                    }
                }
            }
            return new SessionStats(dirtyScreens, runningWorkflows);
        }

        public bool HasPortalSession(Guid id)
        {
            return portalSessions.ContainsKey(id);
        }
        public bool HasFormSession(Guid id)
        {
            return formSessions.ContainsKey(id);
        }
        public SessionStore CreateSessionStore(UIRequest request, UIService uiService)
        {
            if (request.FormSessionId != null && SessionExists(new Guid(request.FormSessionId)))
            {
                throw new Exception("Session already exists. Cannot create new session.");
            }
            SessionStore ss = null;
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            AbstractMenuItem menuItem;
            UIRequestType type = request.Type;
            if (type == UIRequestType.FormReferenceMenuItem_WithSelection && request.IsSingleRecordEdit)
            {
                type = UIRequestType.FormReferenceMenuItem;
            }
            if (request.ObjectId.Contains("|"))
            {
                string[] objectId = request.ObjectId.Split("|".ToCharArray());
                string[] classPath = objectId[1].Split(",".ToCharArray());
                IDynamicSessionStoreProvider dynamicProvider =
                    Reflector.InvokeObject(classPath[0], classPath[1]) 
                    as IDynamicSessionStoreProvider;
                ss = dynamicProvider.GetSessionStore(uiService, request);
            }
            else
            {
                switch (type)
                {
                    case UIRequestType.FormReferenceMenuItem_WithSelection:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        FormReferenceMenuItem formMenuItem = menuItem as FormReferenceMenuItem;

                        ss = new SelectionDialogSessionStore(uiService, request, formMenuItem.SelectionDialogPanel.DataSourceId,
                            formMenuItem.SelectionPanelBeforeTransformationId, formMenuItem.SelectionPanelAfterTransformationId,
                            formMenuItem.SelectionPanelId, menuItem.Name, formMenuItem.SelectionDialogEndRule);
                        break;

                    case UIRequestType.DataConstantReferenceMenuItem:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        DataConstantReferenceMenuItem parMenuItem = menuItem as DataConstantReferenceMenuItem;
                        // PARAM
                        ss = new ParameterSessionStore(uiService, request, parMenuItem.Constant, parMenuItem.FinalLookup,
                            parMenuItem.DisplayName, parMenuItem.Name, parMenuItem.RefreshPortalAfterSave);
                        break;

                    case UIRequestType.FormReferenceMenuItem:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        formMenuItem = menuItem as FormReferenceMenuItem;
                        // FORM
                        ss = new FormSessionStore(uiService, request, menuItem.Name);
                        break;

                    case UIRequestType.WorkflowReferenceMenuItem:
                        AbstractSchemaItem item = ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractSchemaItem;
                        WorkflowReferenceMenuItem wfMenuItem = item as WorkflowReferenceMenuItem;
                        IWorkflow wf = item as IWorkflow;
                        if (wfMenuItem != null)
                        {
                            wf = wfMenuItem.Workflow;
                            // set default workflow menu item parameters (assigned constants to the menu item)
                            RuleEngine ruleEngine = new RuleEngine(null, null);
                            foreach (AbstractSchemaItem parameter in wfMenuItem.ChildItems)
                            {
                                if (parameter != null)
                                {
                                    if (!request.Parameters.Contains(parameter.Name))
                                    {
                                        request.Parameters.Add(parameter.Name, ruleEngine.Evaluate(parameter));
                                    }
                                }
                            }
                        }
                        ss = new WorkflowSessionStore(uiService, request, (Guid)wf.PrimaryKey["Id"], item.Name);
                        break;

                    case UIRequestType.ReportReferenceMenuItem:
                        break;

                    case UIRequestType.ReportReferenceMenuItem_WithSelection:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        ReportReferenceMenuItem reportMenuItem = menuItem as ReportReferenceMenuItem;
                        ss = new SelectionDialogSessionStore(uiService, request, reportMenuItem.SelectionDialogPanel.DataSourceId,
                            reportMenuItem.SelectionPanelBeforeTransformationId, reportMenuItem.SelectionPanelAfterTransformationId,
                            reportMenuItem.SelectionPanelId, menuItem.Name, reportMenuItem.SelectionDialogEndRule);
                        break;

                    case UIRequestType.WorkQueue:
                        // WORK QUEUE OK
                        ss = new WorkQueueSessionStore(uiService, request, Properties.Resources.WorkQueueTitle + " " + request.Caption);
                        break;
                }
            }
            if (ss == null) throw new Exception(Properties.Resources.ErrorUnknownSessionType);
            return ss;
        }
        
    }

    internal struct SessionStats
    {
        public readonly int DirtyScreens;
        public readonly int RunningWorkflows;

        public SessionStats(int dirtyScreens, int runningWorkflows)
        {
            DirtyScreens = dirtyScreens;
            RunningWorkflows = runningWorkflows;
        }
    }
}