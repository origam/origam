using System;
using System.Collections;
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
        private readonly SessionObjects sessionObjects;

        public SessionsController(SessionObjects sessionObjects)
        {
            this.sessionObjects = sessionObjects;
        }

        [HttpPost("[action]")]
        public IActionResult New(Guid menuId)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();

            if (!sessionObjects.SessionManager.HasPortalSession(profile.Id))
            {
                PortalSessionStore pss = new PortalSessionStore(profile.Id);
                sessionObjects.SessionManager.AddPortalSession(profile.Id, pss);
            }

            Guid newSessionId = Guid.NewGuid();
            UIRequest uiRequest = new UIRequest
            {
                FormSessionId = newSessionId.ToString(),
                ObjectId = menuId.ToString()
            };
            UIResult uiResult = sessionObjects.UiManager.InitUI(
                request: uiRequest,
                registerSession: true,
                addChildSession: false,
                parentSession: null,
                basicUiService: sessionObjects.UiService);
            return Ok(newSessionId);
        }

        public IList UpdateObject(Guid sessionFormIdentifier, string entity, object id, string property, object newValue)
        {
            SessionStore ss = sessionObjects.SessionManager.GetSession(sessionFormIdentifier);
            IList output = ss.UpdateObject(entity, id, property, newValue);
            Task.Run(() => SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionObjects.SessionManager.GetSessionStats()));
            return output;
        }
    }
}
