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
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentScriptController(
    IPersistenceService persistenceService,
    DeploymentScriptRunnerService deploymentScriptRunner,
    DeploymentVersionCurrentService deploymentVersionCurrentService,
    IWebHostEnvironment environment,
    ILogger<DeploymentScriptController> log
) : OrigamController(log, environment)
{
    [HttpPost("SetVersionCurrent")]
    public IActionResult SetVersionCurrent(
        [Required] [FromBody] SetVersionCurrentRequestModel requestModel
    )
    {
        return RunWithErrorHandler(() =>
        {
            var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
                requestModel.SchemaItemId,
                useCache: false
            );

            if (item is not DeploymentVersion version)
            {
                return BadRequest(Strings.DeploymentScripts_SelectItemIsNotDeploymentVersion);
            }

            deploymentVersionCurrentService.SetVersionCurrent(version);

            return Ok();
        });
    }

    [HttpPost("Run")]
    public IActionResult Run([Required] [FromBody] RunRequestModel requestModel)
    {
        return RunWithErrorHandler(() =>
        {
            var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
                requestModel.SchemaItemId,
                useCache: false
            );

            if (item is not AbstractUpdateScriptActivity script)
            {
                return BadRequest(Strings.DeploymentScripts_SelectItemIsNotDeploymentScript);
            }

            SecurityManager.SetServerIdentity();
            deploymentScriptRunner.RunDeploymentScript(script);

            return Ok();
        });
    }
}
