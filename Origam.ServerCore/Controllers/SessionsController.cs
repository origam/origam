using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Server;
using Origam.ServerCommon;

namespace Origam.ServerCore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        [HttpPost("[action]")]
        public IActionResult New()
        {
            var portalSessions = new Dictionary<Guid, PortalSessionStore>();
            var formSessions = new Dictionary<Guid, SessionStore>();
            var sessionManager = new SessionManager(portalSessions, formSessions);
            var uiManager = new UIManager(50, sessionManager);
            var uiService = new BasicUiService();

            Guid newSessionId = Guid.NewGuid();
            UIRequest uiRequest = new UIRequest{FormSessionId = newSessionId.ToString()};
            UIResult uiResult = uiManager.InitUI(
                request: uiRequest,
                registerSession: true,
                addChildSession: false,
                parentSession: null,
                basicUiService: uiService);
            return Ok(newSessionId);
        }
    }
}
