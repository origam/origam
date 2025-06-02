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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Gui;
using Origam.Server.Attributes;
using Origam.Server.Model.Session;

namespace Origam.Server.Controller;
[Authorize]
[ApiController]
[Route("internalApi/[controller]")]
public class SessionController : AbstractController
{
    
    public SessionController(SessionObjects sessionObjects, 
        ILogger<AbstractController> log, IWebHostEnvironment environment) 
        : base(log, sessionObjects, environment)
    {
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> CreateSessionAsync([FromBody]CreateSessionData sessionData)
    {
        return await RunWithErrorHandlerAsync(async () =>
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();
            PortalSessionStore pss = new PortalSessionStore(profile.Id);
            sessionObjects.SessionManager.AddPortalSessionIfNotExist(
                id: profile.Id,
                createSession: id => new PortalSessionStore(id)
            );
            Guid newSessionId = Guid.NewGuid();
            UIRequest uiRequest = new UIRequest
            {
                FormSessionId = newSessionId.ToString(),
                ObjectId = sessionData.MenuId.ToString(),
                Parameters = sessionData.Parameters,
                RegisterSession = true
            };
            UIResult uiResult = sessionObjects.UIManager.InitUI(
                request: uiRequest,
                addChildSession: false,
                parentSession: null,
                basicUIService: sessionObjects.UIService);
            await Task.CompletedTask; //CS1998
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
            List<ChangeInfo> output = ss.GetRowData(
                sessionData.Entity,
                sessionData.RowId,
                false);
            CallOrigamUserUpdate();
            return Ok(output);
        });
    }
    [HttpGet("[action]")]
    public IActionResult Rows([FromQuery][RequiredNonDefault] Guid sessionFormIdentifier, 
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
    public IActionResult SaveData([FromBody]SaveDataData saveData)
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
    public IActionResult UpdateRow([FromBody]UpdateRowData updateData)
    {
        return RunWithErrorHandler(() =>
        {
            SessionStore ss = sessionObjects.SessionManager.GetSession(updateData.SessionFormIdentifier);
            IEnumerable<ChangeInfo> output = ss.UpdateObject(
                entity: updateData.Entity,
                id: updateData.Id,
                property: updateData.Property,
                newValue: updateData.NewValue);
            CallOrigamUserUpdate();
            return Ok(output);
        });
    }
    [HttpPost("[action]")]
    public IActionResult CloseSession()
    {
        PortalSessionStore pss = 
            sessionObjects.SessionManager.GetPortalSession();
        if (pss == null)
        {
            return BadRequest("Portal session not found.");
        }
        SessionHelper sessionHelper = new SessionHelper(
            sessionObjects.SessionManager);
        while(pss.FormSessions.Count > 0)
        {
            sessionHelper.DeleteSession(pss.FormSessions[0].Id);
        }
        return Ok();
    }
    private void CallOrigamUserUpdate()
    {
        var principal = SecurityManager.CurrentPrincipal;
        Task.Run(() =>
        {
            Thread.CurrentPrincipal = principal;
            SecurityTools.CreateUpdateOrigamOnlineUser(
                SecurityManager.CurrentPrincipal.Identity.Name,
                sessionObjects.SessionManager.GetSessionStats());
        });
    }
}
