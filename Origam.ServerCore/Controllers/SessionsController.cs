using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Server;
using Origam.ServerCommon;
using Origam.ServerCore.Models;

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
        public IActionResult New([FromBody]NewSessionData sessionData)
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
                ObjectId = sessionData.MenuId.ToString(),
                Parameters = sessionData.Parameters
            };
            UIResult uiResult = sessionObjects.UiManager.InitUI(
                request: uiRequest,
                registerSession: true,
                addChildSession: false,
                parentSession: null,
                basicUiService: sessionObjects.UiService);
            return Ok(newSessionId);
        }

        [HttpPost("[action]")]
        public IList UpdateObject([FromBody]UpdateObjectData updateData )
        {
            SessionStore ss = sessionObjects.SessionManager.GetSession(updateData.SessionFormIdentifier);
            IList output = ss.UpdateObject(
                entity: updateData.Entity, 
                id: updateData.Id, 
                property: updateData.Property, 
                newValue: updateData.NewValue);
            var principal = Thread.CurrentPrincipal;
            Task.Run(() =>
            {
                Thread.CurrentPrincipal = principal;
                SecurityTools.CreateUpdateOrigamOnlineUser(
                    SecurityManager.CurrentPrincipal.Identity.Name,
                    sessionObjects.SessionManager.GetSessionStats());
            });
            return output;
        }

        [HttpPost("[action]")]
        public IList SaveData([FromQuery]Guid sessionFormIdentifier)
        {
            SessionStore ss = sessionObjects.SessionManager.GetSession(sessionFormIdentifier);
            IList output = (IList)ss.ExecuteAction(SessionStore.ACTION_SAVE);
            var principal = Thread.CurrentPrincipal;
            Task.Run(() =>
            {
                Thread.CurrentPrincipal = principal;
                SecurityTools.CreateUpdateOrigamOnlineUser(
                    SecurityManager.CurrentPrincipal.Identity.Name,
                    sessionObjects.SessionManager.GetSessionStats());
            });
            return output;
        }
    }
}
