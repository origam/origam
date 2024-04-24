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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Origam.Schema.MenuModel;
using Origam.Gui;

namespace Origam.Server
{
    public class SessionHelper
    {
        private readonly SessionManager sessionManager;

        public SessionHelper(SessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
        }

        public void DeleteSession(Guid sessionFormIdentifier)
        {
            SessionStore ss = null;

            // if session not found, we just remove it from the list of sessions
            try
            {
                ss = sessionManager.GetSession(sessionFormIdentifier, true);

                // if the form was a modal dialog that needs to pass data to its parent,
                // we do it here
                if (ss.Request.ParentSessionId != null && ss.IsModalDialogCommited)
                {
                    SessionStore parentSession = sessionManager.GetSession(new Guid(ss.Request.ParentSessionId));
                    parentSession.PendingChanges = new ArrayList();
                    EntityWorkflowAction ewa = UIActionTools.GetAction(
                        ss.Request.SourceActionId) as EntityWorkflowAction;

                    var actionRunnerClient = new ServerEntityUIActionRunnerClient(
                        sessionManager, parentSession);

                    actionRunnerClient.ProcessWorkflowResults(
                        profile: SecurityTools.CurrentUserProfile(),
                        processData: null,
                        sourceData: ss.Data,
                        targetData: parentSession.Data,
                        entityWorkflowAction: ewa,
                        changes: parentSession.PendingChanges);

                    actionRunnerClient.PostProcessWorkflowAction(
                        data: parentSession.Data,
                        entityWorkflowAction: ewa,
                        changes: parentSession.PendingChanges);
                }

                ss.Dispose();
            }
            catch
            {
                if (ss != null && ss.IsModalDialog)
                {
                    ss.IsModalDialogCommited = false;
                    throw;
                }
            }

            sessionManager.RemoveFormSession(sessionFormIdentifier);

            if (ss != null && !ss.Request.IsStandalone)
            {
                PortalSessionStore pss = sessionManager.GetPortalSession();
                IList<SessionStore> toRemove = new List<SessionStore>();

                foreach (SessionStore childSS in pss.FormSessions)
                {
                    if (childSS.Id.Equals(ss.Id))
                    {
                        toRemove.Add(childSS);
                    }
                }

                foreach (SessionStore rem in toRemove)
                {
                    pss.FormSessions.Remove(rem);
                    pss.IsExclusiveScreenOpen = false;
                }
            }
        }
    }
}
