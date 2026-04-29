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
using Origam.Server.Common;
using Origam.Server.Model.Session;

namespace Origam.Server.Controller;

[Authorize(Policy = "InternalApi")]
[ApiController]
[Route(template: "internalApi/[controller]")]
public class SessionController : AbstractController
{
    public SessionController(
        SessionObjects sessionObjects,
        ILogger<AbstractController> log,
        IWebHostEnvironment environment
    )
        : base(log: log, sessionObjects: sessionObjects, environment: environment) { }

    [HttpPost(template: "[action]")]
    public async Task<IActionResult> CreateSessionAsync([FromBody] CreateSessionData sessionData)
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        PortalSessionStore pss = new PortalSessionStore(profileId: profile.Id);
        sessionObjects.SessionManager.AddPortalSessionIfNotExist(
            id: profile.Id,
            createSession: id => new PortalSessionStore(profileId: id)
        );
        Guid newSessionId = Guid.NewGuid();
        UIRequest uiRequest = new UIRequest
        {
            FormSessionId = newSessionId.ToString(),
            ObjectId = sessionData.MenuId.ToString(),
            Parameters = sessionData.Parameters,
            RegisterSession = true,
        };
        UIResult uiResult = sessionObjects.UIManager.InitUI(
            request: uiRequest,
            addChildSession: false,
            parentSession: null,
            basicUIService: sessionObjects.UIService
        );
        await Task.CompletedTask; //CS1998
        return Ok(value: newSessionId);
    }

    [HttpPost(template: "[action]")]
    public IActionResult DeleteSession([FromBody] DeleteSessionData sessionData)
    {
        new SessionHelper(sessionManager: sessionObjects.SessionManager).DeleteSession(
            sessionFormIdentifier: sessionData.SessionId
        );
        CallOrigamUserUpdate();
        return Ok();
    }

    [HttpPost(template: "[action]")]
    public IActionResult DeleteRow([FromBody] DeleteRowData sessionData)
    {
        SessionStore ss = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: sessionData.SessionFormIdentifier
        );
        IList output = ss.DeleteObject(entity: sessionData.Entity, id: sessionData.RowId);
        CallOrigamUserUpdate();
        return Ok(value: output);
    }

    [HttpPost(template: "[action]")]
    public IActionResult ChangeMasterRecord([FromBody] ChangeMasterRecordData sessionData)
    {
        SessionStore ss = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: sessionData.SessionFormIdentifier
        );
        List<ChangeInfo> output = ss.GetRowData(
            entity: sessionData.Entity,
            id: sessionData.RowId,
            ignoreDirtyState: false
        );
        CallOrigamUserUpdate();
        return Ok(value: output);
    }

    [HttpGet(template: "[action]")]
    public IActionResult Rows(
        [FromQuery] [RequiredNonDefault] Guid sessionFormIdentifier,
        [FromQuery] [Required] string childEntity,
        [FromQuery] [Required] string parentRecordId,
        [FromQuery] [Required] string rootRecordId
    )
    {
        SessionStore ss = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: sessionFormIdentifier
        );
        IList output = ss.GetData(
            childEntity: childEntity,
            parentRecordId: parentRecordId,
            rootRecordId: rootRecordId
        );
        CallOrigamUserUpdate();
        return Ok(value: output);
    }

    [HttpPost(template: "[action]")]
    public IActionResult SaveData([FromBody] SaveDataData saveData)
    {
        SessionStore ss = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: saveData.SessionId
        );
        IList output = (IList)ss.ExecuteAction(actionId: SessionStore.ACTION_SAVE);
        CallOrigamUserUpdate();
        return Ok(value: output);
    }

    [HttpPost(template: "[action]")]
    public IActionResult CreateRow([FromBody] NewRowData newRowData)
    {
        SessionStore ss = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: newRowData.SessionFormIdentifier
        );
        IList output = ss.CreateObject(
            entity: newRowData.Entity,
            values: newRowData.Values,
            parameters: newRowData.Parameters,
            requestingGrid: newRowData.RequestingGridId.ToString()
        );
        CallOrigamUserUpdate();
        return Ok(value: output);
    }

    [HttpPost(template: "[action]")]
    public IActionResult UpdateRow([FromBody] UpdateRowData updateData)
    {
        SessionStore ss = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: updateData.SessionFormIdentifier
        );
        IEnumerable<ChangeInfo> output = ss.UpdateObject(
            entity: updateData.Entity,
            id: updateData.Id,
            property: updateData.Property,
            newValue: updateData.NewValue
        );
        CallOrigamUserUpdate();
        return Ok(value: output);
    }

    [HttpPost(template: "[action]")]
    public IActionResult CloseSession()
    {
        PortalSessionStore pss = sessionObjects.SessionManager.GetPortalSession();
        if (pss == null)
        {
            return BadRequest(error: "Portal session not found.");
        }
        SessionHelper sessionHelper = new SessionHelper(
            sessionManager: sessionObjects.SessionManager
        );
        while (pss.FormSessions.Count > 0)
        {
            sessionHelper.DeleteSession(sessionFormIdentifier: pss.FormSessions[index: 0].Id);
        }
        return Ok();
    }

    private void CallOrigamUserUpdate()
    {
        var principal = SecurityManager.CurrentPrincipal;
        Task.Run(action: () =>
        {
            Thread.CurrentPrincipal = principal;
            SecurityTools.CreateUpdateOrigamOnlineUser(
                username: SecurityManager.CurrentPrincipal.Identity.Name,
                stats: sessionObjects.SessionManager.GetSessionStats()
            );
        });
    }
}
