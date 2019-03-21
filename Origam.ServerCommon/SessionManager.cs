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
﻿using System;
using System.Collections;
using System.Collections.Generic;
using Origam;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.Gui;
using Origam.ServerCommon;

namespace Origam.Server
{
    public class SessionManager
    {
        private readonly Dictionary<Guid, PortalSessionStore> portalSessions;
        private readonly Dictionary<Guid, SessionStore> formSessions;
        private readonly Analytics analytics;
        private readonly bool runsOnCore;

        public SessionManager(Dictionary<Guid, PortalSessionStore> portalSessions,
            Dictionary<Guid, SessionStore> formSessions, Analytics analytics, bool runsOnCore)
        {
            this.runsOnCore = runsOnCore;
            this.analytics = analytics;
            this.portalSessions = portalSessions;
            this.formSessions = formSessions;
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
                throw new Exception(string.Format(Resources.ErrorSessionNotFound, sessionFormIdentifier.ToString()));
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
                    throw new RuleException(string.Format(Resources.ErrorCannotOpenScreenWhenExclusiveIsOpen,
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
        public SessionStore CreateSessionStore(UIRequest request, IBasicUIService basicUiService)
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
                ss = dynamicProvider.GetSessionStore(basicUiService, request);
            }
            else
            {
                switch (type)
                {
                    case UIRequestType.FormReferenceMenuItem_WithSelection:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        FormReferenceMenuItem formMenuItem = menuItem as FormReferenceMenuItem;

                        ss = new SelectionDialogSessionStore(basicUiService, request, formMenuItem.SelectionDialogPanel.DataSourceId,
                            formMenuItem.SelectionPanelBeforeTransformationId, formMenuItem.SelectionPanelAfterTransformationId,
                            formMenuItem.SelectionPanelId, menuItem.Name, formMenuItem.SelectionDialogEndRule, analytics);
                        break;

                    case UIRequestType.DataConstantReferenceMenuItem:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        DataConstantReferenceMenuItem parMenuItem = menuItem as DataConstantReferenceMenuItem;
                        // PARAM
                        ss = new ParameterSessionStore(basicUiService, request, parMenuItem.Constant, parMenuItem.FinalLookup,
                            parMenuItem.DisplayName, parMenuItem.Name, parMenuItem.RefreshPortalAfterSave, analytics);
                        break;

                    case UIRequestType.FormReferenceMenuItem:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        formMenuItem = menuItem as FormReferenceMenuItem;
                        // FORM
                        ss = new FormSessionStore(basicUiService, request, menuItem.Name, analytics, runsOnCore);
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
                        ss = new WorkflowSessionStore(basicUiService, request, (Guid)wf.PrimaryKey["Id"], item.Name, analytics);
                        break;

                    case UIRequestType.ReportReferenceMenuItem:
                        break;

                    case UIRequestType.ReportReferenceMenuItem_WithSelection:
                        menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(request.ObjectId))) as AbstractMenuItem;
                        ReportReferenceMenuItem reportMenuItem = menuItem as ReportReferenceMenuItem;
                        ss = new SelectionDialogSessionStore(basicUiService, request, reportMenuItem.SelectionDialogPanel.DataSourceId,
                            reportMenuItem.SelectionPanelBeforeTransformationId, reportMenuItem.SelectionPanelAfterTransformationId,
                            reportMenuItem.SelectionPanelId, menuItem.Name, reportMenuItem.SelectionDialogEndRule, analytics);
                        break;

                    case UIRequestType.WorkQueue:
                        // WORK QUEUE OK
                        ss = new WorkQueueSessionStore(basicUiService, request, Resources.WorkQueueTitle + " " + request.Caption, analytics);
                        break;
                }
            }
            if (ss == null) throw new Exception(Resources.ErrorUnknownSessionType);
            return ss;
        }
        
    }

    public struct SessionStats
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