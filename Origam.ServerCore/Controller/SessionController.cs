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

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Origam.Security.Identity;
using Origam.Server;
using Origam.ServerCommon;
using Origam.ServerCore.Model;
using Origam.ServerCore.Model.Session;

namespace Origam.ServerCore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("internalApi/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly SessionObjects sessionObjects;

        public SessionController(SessionObjects sessionObjects, IServiceProvider serviceProvider)
        {
            this.sessionObjects = sessionObjects;
            IdentityServiceAgent.ServiceProvider = serviceProvider;
        }

        [HttpPost("[action]")]
        public IActionResult CreateSession([FromBody]CreateSessionData sessionData)
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
        public IActionResult DeleteSession([FromBody]DeleteSessionData sessionData)
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
        public IActionResult DeleteRow([FromBody]DeleteRowData sessionData)
        {
            return RunWithErrorHandler(() =>
            {
                SessionStore ss = sessionObjects.SessionManager.GetSession(sessionData.SessionFormIdentifier);
                IList output = ss.DeleteObject(
                    sessionData.Entity,
                    sessionData.RowId);
                CallOrigamUserUpdate();
                return Ok(output);
            });
        }

        [HttpPost("[action]")]
        public IActionResult ChangeMasterRecord([FromBody]ChangeMasterRecordData sessionData)
        {
            return RunWithErrorHandler(() =>
            {
                SessionStore ss = sessionObjects.SessionManager.GetSession(sessionData.SessionFormIdentifier);
                IList output = ss.GetRowData(
                    sessionData.Entity,
                    sessionData.RowId,
                    false);
                CallOrigamUserUpdate();
                return Ok(output);
            });
        }

        [HttpGet("[action]")]
        public IActionResult Rows([FromQuery][RequireNonDefault] Guid sessionFormIdentifier, 
            [FromQuery][Required] string childEntity, [FromQuery][Required] string parentRecordId,
            [FromQuery][Required] string rootRecordId)
        {
            return RunWithErrorHandler(() =>
            {
                SessionStore ss = sessionObjects.SessionManager.GetSession(sessionFormIdentifier);
                IList output = ss.GetData(childEntity, parentRecordId,  rootRecordId);
                CallOrigamUserUpdate();
                return Ok(output);
            });
        }
        
        [HttpPost("[action]")]
        public IActionResult SaveSession([FromBody]SaveSessionData saveData)
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
        public IActionResult CreateRow([FromBody]NewRowData newRowData)
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
