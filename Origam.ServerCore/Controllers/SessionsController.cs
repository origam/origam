using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
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
            return RunWithErrorHandler(() =>
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
                    InitializeStructure = sessionData.InitializeStructure,
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
            });
        }


        [HttpPost("[action]")]
        public IActionResult Delete([FromBody]DeleteSessionData sessionData)
        {
            return RunWithErrorHandler(() =>
            {
                new SessionHelper(sessionObjects.SessionManager)
                    .DeleteSession(sessionData.SessionId);
                CallOrigamUserUpdate();
                return Ok();
            });
        }

        [HttpPost("[action]")]
        public IActionResult Save([FromBody]SaveSessionData saveData)
        {
            return RunWithErrorHandler(() =>
            {
                SessionStore ss = sessionObjects.SessionManager.GetSession(saveData.SessionId);
                IList output = (IList)ss.ExecuteAction(SessionStore.ACTION_SAVE);
                CallOrigamUserUpdate();
                return Ok(output);
            });
        }

        [HttpPost("[action]")]
        public IActionResult CreateRow([FromBody] NewRowData newRowData)
        {
            return RunWithErrorHandler(() =>
            {
                SessionStore ss = sessionObjects.SessionManager.GetSession(newRowData.SessionFormIdentifier);
                IList output = ss.CreateObject(
                    newRowData.Entity,
                    newRowData.Values,
                    newRowData.Parameters,
                    newRowData.RequestingGridId.ToString());
                CallOrigamUserUpdate();
                return Ok(output);
            });
        }

        [HttpPost("[action]")]
        public IActionResult UpdateRow([FromBody]UpdateRowData updateData )
        {
            return RunWithErrorHandler(() =>
            {
                SessionStore ss = sessionObjects.SessionManager.GetSession(updateData.SessionFormIdentifier);
                IList output = ss.UpdateObject(
                    entity: updateData.Entity,
                    id: updateData.Id,
                    property: updateData.Property,
                    newValue: updateData.NewValue);
                CallOrigamUserUpdate();
                return Ok(output);
            });
        }


        private IActionResult RunWithErrorHandler(Func<IActionResult> func)
        {
            try
            {
                return func();
            }
            catch (UIException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private void CallOrigamUserUpdate()
        {
            var principal = Thread.CurrentPrincipal;
            Task.Run(() =>
            {
                Thread.CurrentPrincipal = principal;
                SecurityTools.CreateUpdateOrigamOnlineUser(
                    SecurityManager.CurrentPrincipal.Identity.Name,
                    sessionObjects.SessionManager.GetSessionStats());
            });
        }
    }
}
