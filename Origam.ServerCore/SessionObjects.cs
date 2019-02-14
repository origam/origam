using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Origam.Server;
using Origam.ServerCommon;

namespace Origam.ServerCore
{
    public class SessionObjects
    {
        public SessionManager SessionManager { get; }
        public UIManager UiManager { get; }
        public BasicUiService UiService { get; }

        public SessionObjects()
        {
            var analytics = Analytics.Instance;
            SessionManager = new SessionManager(
                portalSessions: new Dictionary<Guid, PortalSessionStore>(),
                formSessions: new Dictionary<Guid, SessionStore>(),
                analytics: analytics,
                runsOnCore: true);
            UiManager = new UIManager(50, SessionManager, analytics);
            UiService = new BasicUiService();
        }
    }
}
