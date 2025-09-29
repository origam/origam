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
using Origam.Server;

namespace Origam.Server;

public class SessionObjects
{
    public SessionManager SessionManager { get; }
    public UIManager UIManager { get; }
    public ServerCoreUIService UIService { get; }

    public SessionObjects()
    {
        var analytics = Analytics.Instance;
        SessionManager = new SessionManager(
            portalSessions: new ConcurrentDictionary<Guid, PortalSessionStore>(),
            formSessions: new ConcurrentDictionary<Guid, SessionStore>(),
            analytics: analytics,
            reportRequests: new ConcurrentDictionary<Guid, ReportRequest>(),
            blobDownloadRequests: new ConcurrentDictionary<Guid, BlobDownloadRequest>(),
            blobUploadRequests: new ConcurrentDictionary<Guid, BlobUploadRequest>());
        UIManager = new UIManager(50, SessionManager, analytics);
        UIService = new ServerCoreUIService(UIManager, SessionManager);
    }
}
