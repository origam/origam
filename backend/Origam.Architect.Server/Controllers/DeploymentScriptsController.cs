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
using Origam.Architect.Server.Models.Requests.DeploymentScripts;
using Origam.Architect.Server.Models.Responses.DeploymentScripts;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentScriptsController(
    IPersistenceService persistenceService,
    DeploymentScriptRunnerService deploymentScriptRunner,
    DeploymentVersionCurrentService deploymentVersionCurrentService
) : ControllerBase
{
    [HttpPost("SetVersionCurrent")]
    public IActionResult SetVersionCurrent(
        [Required] [FromBody] SetVersionCurrentRequestModel requestModel
    )
    {
        var targetVersion = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
            requestModel.SchemaItemId,
            useCache: false
        );

        if (targetVersion is not DeploymentVersion version)
        {
            throw new Exception(Strings.DeploymentScript_VersionNotFound);
        }

        deploymentVersionCurrentService.SetVersionCurrent(version);

        return Ok();
    }

    [HttpPost("Run")]
    public IActionResult Run([Required] [FromBody] RunRequestModel requestModel)
    {
        var updateScriptActivity = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
            requestModel.SchemaItemId,
            useCache: false
        );

        if (updateScriptActivity is not AbstractUpdateScriptActivity script)
        {
            throw new Exception(Strings.DeploymentScript_NotFound);
        }

        SecurityManager.SetServerIdentity();
        string output = deploymentScriptRunner.RunDeploymentScript(script);

        return Ok(new RunResponseModel { Output = output });
    }
}
