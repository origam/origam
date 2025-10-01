#region license

/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Server.Controller;

[ApiController]
public class WorkQueueController : ControllerBase
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    [HttpPost]
    [Route("workQueue/{workQueueCode}/{commandText}")]
    public IActionResult CreateSessionAsync(
        string workQueueCode,
        string commandText,
        [FromQuery] [Required] Guid workQueueEntryId
    )
    {
        if (log.IsDebugEnabled)
        {
            log.Debug("Processing: " + HttpContext.Request.GetDisplayUrl());
        }
        HttpContext.Response.ContentType = "application/json";
        try
        {
            IWorkQueueService workQueueService =
                ServiceManager.Services.GetService<IWorkQueueService>();
            workQueueService.HandleAction(workQueueCode, commandText, workQueueEntryId);
            return Ok();
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(ex.Message, ex);
            }
            string output;
            if (ex is RuleException ruleException)
            {
                output = String.Format(
                    "{{\"Message\" : {0}, \"RuleResult\" : {1}}}",
                    JsonConvert.SerializeObject(ruleException.Message),
                    JsonConvert.SerializeObject(ruleException.RuleResult)
                );
            }
            else if (ex is ArgumentOutOfRangeException argumentException)
            {
                output = String.Format(
                    "{{\"Message\" : {0}, \"ParamName\" : {1}, \"ActualValue\" : {2}}}",
                    JsonConvert.SerializeObject(argumentException.Message),
                    JsonConvert.SerializeObject(argumentException.ParamName),
                    JsonConvert.SerializeObject(argumentException.ActualValue)
                );
            }
            else
            {
                output = JsonConvert.SerializeObject(ex);
            }
            return BadRequest(output);
        }
    }
}
