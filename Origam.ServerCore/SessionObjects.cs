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
        internal SessionManager SessionManager { get; }
        internal UIManager UiManager { get; }
        internal BasicUiService UiService { get; }

        public SessionObjects()
        {
            SessionManager = new SessionManager(
                new Dictionary<Guid, PortalSessionStore>(),
                new Dictionary<Guid, SessionStore>());
            UiManager = new UIManager(50, SessionManager);
            UiService = new BasicUiService();
        }
    }
}
