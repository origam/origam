#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Interfaces.Services;
using Origam.Architect.Server.Models.Requests.DeploymentScripts;
using Origam.Architect.Server.Models.Responses.DeploymentScripts;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentScriptsGeneratorController(
    SchemaService schemaService,
    ISchemaDbCompareResultsService schemaDbCompareResultsService,
    IAddToDeploymentService addToDeploymentService,
    IAddToModelService addToModelService
) : ControllerBase
{
    [HttpGet("List")]
    public IActionResult List([FromQuery] string platform)
    {
        ContextGuardAndResolver();

        ListResponseModel response = schemaDbCompareResultsService.PrepareListResponseModel(
            platform
        );

        return Ok(response);
    }

    [HttpPost("AddToDeployment")]
    public IActionResult AddToDeployment(
        [Required] [FromBody] AddToDeploymentRequestModel requestModel
    )
    {
        ContextGuardAndResolver();

        addToDeploymentService.Process(
            requestModel.Platform,
            requestModel.DeploymentVersionId,
            requestModel.SchemaItemIds
        );

        return Ok();
    }

    [HttpPost("AddToModel")]
    public IActionResult AddToModel([Required] [FromBody] AddToModelRequestModel requestModel)
    {
        ContextGuardAndResolver();

        addToModelService.Process(requestModel.Platform, requestModel.SchemaItemNames);

        return Ok();
    }

    private void ContextGuardAndResolver()
    {
        if (schemaService.ActiveExtension == null || schemaService.ActiveExtension.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Active extension (package) is not set (activeExtensionId missing)."
            );
        }

        SecurityManager.SetServerIdentity();
    }
}
