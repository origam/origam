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
using System.Collections.Concurrent;
using Origam.Gui;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server;
using Origam.Server.Common;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Server;

public class SessionManager
{
    private readonly ConcurrentDictionary<Guid, PortalSessionStore> portalSessions;
    private readonly ConcurrentDictionary<Guid, SessionStore> formSessions;
    private readonly ConcurrentDictionary<Guid, ReportRequest> reportRequests;
    private readonly ConcurrentDictionary<Guid, BlobDownloadRequest> blobDownloadRequests;
    private readonly ConcurrentDictionary<Guid, BlobUploadRequest> blobUploadRequests;
    private readonly Analytics analytics;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public SessionManager(
        ConcurrentDictionary<Guid, PortalSessionStore> portalSessions,
        ConcurrentDictionary<Guid, SessionStore> formSessions,
        Analytics analytics,
        ConcurrentDictionary<Guid, ReportRequest> reportRequests,
        ConcurrentDictionary<Guid, BlobDownloadRequest> blobDownloadRequests,
        ConcurrentDictionary<Guid, BlobUploadRequest> blobUploadRequests
    )
    {
        this.analytics = analytics;
        this.portalSessions = portalSessions;
        this.formSessions = formSessions;
        this.reportRequests = reportRequests;
        this.blobDownloadRequests = blobDownloadRequests;
        this.blobUploadRequests = blobUploadRequests;
    }

    public int PortalSessionCount => portalSessions.Count;

    public void RemoveFormSession(Guid sessionFormIdentifier)
    {
        if (!formSessions.TryRemove(sessionFormIdentifier, out _))
        {
            log.Warn(
                $"Form session with id: {sessionFormIdentifier} was not removed because it did not exist"
            );
        }
    }

    public void RemovePortalSession(Guid id)
    {
        if (!portalSessions.TryRemove(id, out _))
        {
            log.Warn($"Portal session with id: {id} was not removed because it did not exist");
        }
    }

    public void AddPortalSessionIfNotExist(Guid id, Func<Guid, PortalSessionStore> createSession)
    {
        portalSessions.GetOrAdd(id, createSession);
    }

    public PortalSessionStore GetPortalSession(Guid id)
    {
        portalSessions.TryGetValue(id, out var value);
        return value;
    }

    public PortalSessionStore GetPortalSession()
    {
        Guid profileId = SecurityTools.CurrentUserProfile().Id;
        return portalSessions.GetOrAdd(profileId, guid => throw new SessionExpiredException());
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
        return formSessions.TryGetValue(sessionFormIdentifier, out _);
    }

    public SessionStore GetSession(Guid sessionFormIdentifier, bool rootSession)
    {
        SecurityTools.CurrentUserProfile();
        SessionStore ss = formSessions.GetOrAdd(
            sessionFormIdentifier,
            guid => throw new SessionExpiredException()
        );

        if (ss == null)
        {
            throw new Exception(
                string.Format(Resources.ErrorSessionNotFound, sessionFormIdentifier.ToString())
            );
        }
        ss.CacheExpiration.AddMinutes(5);
        // check for an active sub-session - return it
        if (!rootSession && ss.ActiveSession != null)
        {
            return ss.ActiveSession;
        }
        return ss;
    }

    public void RegisterSession(SessionStore ss)
    {
        PortalSessionStore pss = GetPortalSession();
        if (pss.IsExclusiveScreenOpen && !pss.ExclusiveSession.Equals(ss))
        {
            throw new RuleException(
                string.Format(
                    Resources.ErrorCannotOpenScreenWhenExclusiveIsOpen,
                    pss.ExclusiveSession.Title
                )
            );
        }
        formSessions.AddOrUpdate(ss.Id, ss, (guid, store) => store);
        if (!ss.Request.IsStandalone)
        {
            if (!pss.FormSessions.Contains(ss))
            {
                pss.IsExclusiveScreenOpen = ss.IsExclusive;
                pss.FormSessions.Add(ss);
            }
        }
    }

    public SessionStats GetSessionStats()
    {
        int dirtyScreens = 0;
        int runningWorkflows = 0;
        UserProfile profile = SecurityTools.CurrentUserProfile();
        portalSessions.TryGetValue(profile.Id, out var portalSession);
        if (portalSession == null)
        {
            return new SessionStats(dirtyScreens, runningWorkflows);
        }
        foreach (SessionStore mainSessionStore in portalSession.FormSessions)
        {
            if (formSessions.ContainsKey(mainSessionStore.Id))
            {
                SessionStore sessionStore = (
                    mainSessionStore.ActiveSession == null
                        ? mainSessionStore
                        : mainSessionStore.ActiveSession
                );
                if (sessionStore is FormSessionStore)
                {
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

    public bool HasFormSession(Guid id)
    {
        return formSessions.ContainsKey(id);
    }

    public SessionStore CreateSessionStore(UIRequest request, IBasicUIService basicUIService)
    {
        if (request.FormSessionId != null && SessionExists(new Guid(request.FormSessionId)))
        {
            throw new Exception("Session already exists. Cannot create new session.");
        }
        SessionStore ss = null;
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
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
                Reflector.InvokeObject(classPath[0], classPath[1]) as IDynamicSessionStoreProvider;
            ss = dynamicProvider.GetSessionStore(basicUIService, request);
        }
        else
        {
            switch (type)
            {
                case UIRequestType.FormReferenceMenuItem_WithSelection:
                {
                    menuItem =
                        ps.SchemaProvider.RetrieveInstance(
                            typeof(AbstractMenuItem),
                            new ModelElementKey(new Guid(request.ObjectId))
                        ) as AbstractMenuItem;
                    FormReferenceMenuItem formMenuItem = menuItem as FormReferenceMenuItem;
                    ss = new SelectionDialogSessionStore(
                        basicUIService,
                        request,
                        formMenuItem.SelectionDialogPanel.DataSourceId,
                        formMenuItem.SelectionPanelBeforeTransformationId,
                        formMenuItem.SelectionPanelAfterTransformationId,
                        formMenuItem.SelectionPanelId,
                        menuItem.Name,
                        formMenuItem.SelectionDialogEndRule,
                        analytics
                    );
                    break;
                }

                case UIRequestType.DataConstantReferenceMenuItem:
                {
                    menuItem =
                        ps.SchemaProvider.RetrieveInstance(
                            typeof(AbstractMenuItem),
                            new ModelElementKey(new Guid(request.ObjectId))
                        ) as AbstractMenuItem;
                    DataConstantReferenceMenuItem parMenuItem =
                        menuItem as DataConstantReferenceMenuItem;
                    // PARAM
                    ss = new ParameterSessionStore(
                        basicUIService,
                        request,
                        parMenuItem.Constant,
                        parMenuItem.FinalLookup,
                        parMenuItem.DisplayName,
                        parMenuItem.Name,
                        parMenuItem.RefreshPortalAfterSave,
                        analytics
                    );
                    break;
                }

                case UIRequestType.FormReferenceMenuItem:
                {
                    menuItem =
                        ps.SchemaProvider.RetrieveInstance(
                            typeof(AbstractMenuItem),
                            new ModelElementKey(new Guid(request.ObjectId))
                        ) as AbstractMenuItem;
                    // FORM
                    ss =
                        request.NewRecordInitialValues != null
                            ? new NewRecordSessionStore(
                                basicUIService,
                                request,
                                menuItem.Name,
                                analytics
                            )
                            : new FormSessionStore(
                                basicUIService,
                                request,
                                menuItem.Name,
                                analytics
                            );
                    break;
                }

                case UIRequestType.WorkflowReferenceMenuItem:
                {
                    ISchemaItem item =
                        ps.SchemaProvider.RetrieveInstance(
                            typeof(ISchemaItem),
                            new ModelElementKey(new Guid(request.ObjectId))
                        ) as ISchemaItem;
                    WorkflowReferenceMenuItem wfMenuItem = item as WorkflowReferenceMenuItem;
                    IWorkflow wf = item as IWorkflow;
                    if (wfMenuItem != null)
                    {
                        wf = wfMenuItem.Workflow;
                        // set default workflow menu item parameters (assigned constants to the menu item)
                        RuleEngine ruleEngine = RuleEngine.Create(null, null);
                        foreach (ISchemaItem parameter in wfMenuItem.ChildItems)
                        {
                            if (parameter != null)
                            {
                                if (!request.Parameters.Contains(parameter.Name))
                                {
                                    request.Parameters.Add(
                                        parameter.Name,
                                        ruleEngine.Evaluate(parameter)
                                    );
                                }
                            }
                        }
                    }
                    ss = new WorkflowSessionStore(
                        basicUIService,
                        request,
                        (Guid)wf.PrimaryKey["Id"],
                        item.Name,
                        analytics
                    );
                    break;
                }

                case UIRequestType.ReportReferenceMenuItem:
                    break;
                case UIRequestType.ReportReferenceMenuItem_WithSelection:
                {
                    menuItem =
                        ps.SchemaProvider.RetrieveInstance(
                            typeof(AbstractMenuItem),
                            new ModelElementKey(new Guid(request.ObjectId))
                        ) as AbstractMenuItem;
                    ReportReferenceMenuItem reportMenuItem = menuItem as ReportReferenceMenuItem;
                    ss = new SelectionDialogSessionStore(
                        basicUIService,
                        request,
                        reportMenuItem.SelectionDialogPanel.DataSourceId,
                        reportMenuItem.SelectionPanelBeforeTransformationId,
                        reportMenuItem.SelectionPanelAfterTransformationId,
                        reportMenuItem.SelectionPanelId,
                        menuItem.Name,
                        reportMenuItem.SelectionDialogEndRule,
                        analytics
                    );
                    break;
                }

                case UIRequestType.WorkQueue:
                {
                    ss = new WorkQueueSessionStore(
                        basicUIService,
                        request,
                        Resources.WorkQueueTitle + " " + request.Caption,
                        analytics
                    );
                    break;
                }
            }
        }
        if (ss == null)
        {
            throw new Exception(Resources.ErrorUnknownSessionType);
        }

        return ss;
    }

    public void AddReportRequest(Guid key, ReportRequest request)
    {
        if (!reportRequests.TryAdd(key, request))
        {
            throw new ArgumentException(
                $"Report request could not be added because another one with id {key} already exists"
            );
        }
    }

    public ReportRequest GetReportRequest(Guid key)
    {
        reportRequests.TryGetValue(key, out var reportRequest);
        return reportRequest;
    }

    public void RemoveReportRequest(Guid key)
    {
        if (!reportRequests.TryRemove(key, out _))
        {
            log.Warn($"Report request with id: {key} was not removed because it did not exist");
        }
    }

    public void RemoveExcelFileRequest(Guid key)
    {
        if (!reportRequests.TryRemove(key, out _))
        {
            log.Warn($"Excel file request with id: {key} was not removed because it did not exist");
        }
    }

    public void AddBlobDownloadRequest(Guid key, BlobDownloadRequest request)
    {
        if (!blobDownloadRequests.TryAdd(key, request))
        {
            throw new ArgumentException(
                $"Blob download request could not be added because another session with id {key} already exists"
            );
        }
    }

    public BlobDownloadRequest GetBlobDownloadRequest(Guid key)
    {
        blobDownloadRequests.TryGetValue(key, out var request);
        return request;
    }

    public void RemoveBlobDownloadRequest(Guid key)
    {
        if (!blobDownloadRequests.TryRemove(key, out _))
        {
            log.Warn(
                $"Blob download request with id: {key} was not removed because it did not exist"
            );
        }
    }

    public void AddBlobUploadRequest(Guid key, BlobUploadRequest request)
    {
        if (!blobUploadRequests.TryAdd(key, request))
        {
            throw new ArgumentException(
                $"Blob upload request could not be added because another session with id {key} already exists"
            );
        }
    }

    public BlobUploadRequest GetBlobUploadRequest(Guid key)
    {
        blobUploadRequests.TryGetValue(key, out var request);
        return request;
    }

    public void RemoveBlobUploadRequest(Guid key)
    {
        if (!blobUploadRequests.TryRemove(key, out _))
        {
            log.Warn(
                $"Blob upload request with id: {key} was not removed because it did not exist"
            );
        }
    }

    public void AddOrUpdatePortalSession(
        Guid id,
        Func<Guid, PortalSessionStore> addSession,
        Func<Guid, PortalSessionStore, PortalSessionStore> updateSession
    )
    {
        portalSessions.AddOrUpdate(id, addSession, updateSession);
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
