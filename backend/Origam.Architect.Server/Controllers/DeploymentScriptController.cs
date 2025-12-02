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
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentScriptController(
    IPersistenceService persistenceService,
    SchemaService schemaService,
    DeploymentScriptRunnerService deploymentScriptRunner,
    DeploymentVersionCurrentService deploymentVersionCurrentService,
    ILogger<DeploymentScriptController> log
) : OrigamController(log)
{
    [HttpPost("SetVersionCurrent")]
    public IActionResult SetVersionCurrent(
        [Required] [FromBody] SetVersionCurrentRequestModel requestModel
    )
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
    }

    [HttpPost("Run")]
    public IActionResult Run([Required] [FromBody] RunRequestModel requestModel)
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
    }

    [HttpPost("List")]
    public IActionResult List([Required] [FromBody] ListRequestModel requestModel)
    {
        return RunWithErrorHandler(() =>
        {
            if (
                schemaService.ActiveExtension == null
                || schemaService.ActiveExtension.Id == Guid.Empty
            )
            {
                throw new InvalidOperationException(
                    "Active extension (package) is not set (activeExtensionId missing)."
                );
            }

            SecurityManager.SetServerIdentity();

            var da = (AbstractSqlDataService)DataServiceFactory.GetDataService();
            da.PersistenceProvider = persistenceService.SchemaProvider;

            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            var platforms = settings.GetAllPlatforms();

            Platform platform;
            if (string.IsNullOrWhiteSpace(requestModel.Platform))
            {
                platform = platforms.FirstOrDefault();
                if (platform == null)
                {
                    throw new InvalidOperationException("No platforms are configured.");
                }
            }
            else
            {
                var requestedPlatformName = requestModel.Platform.Trim();
                platform = platforms.FirstOrDefault(p =>
                    string.Equals(p.Name, requestedPlatformName, StringComparison.OrdinalIgnoreCase)
                );

                if (platform == null)
                {
                    var available = string.Join(", ", platforms.Select(p => p.Name));
                    throw new InvalidOperationException(
                        $"Unknown platform '{requestedPlatformName}'. Available platforms: {available}."
                    );
                }
            }

            var DaPlatform = (AbstractSqlDataService)DataServiceFactory.GetDataService(platform);
            DaPlatform.PersistenceProvider = persistenceService.SchemaProvider;
            var dbCompareResults = DaPlatform.CompareSchema(persistenceService.SchemaProvider);
            foreach (SchemaDbCompareResult dbCompareResult in dbCompareResults)
            {
                dbCompareResult.Platform = platform;
            }

            var deploymentVersions = schemaService
                .GetProvider<DeploymentSchemaItemProvider>()
                .ChildItems.Cast<DeploymentVersion>()
                .OrderBy(deploymentVersion => deploymentVersion.Version);

            List<SchemaDeploymentVersionDto> possibleDeploymentVersions = [];
            DeploymentVersion currentVersion = null;
            foreach (DeploymentVersion version in deploymentVersions)
            {
                if (version.Package.PrimaryKey.Equals(schemaService.ActiveExtension.PrimaryKey))
                {
                    if (version.IsCurrentVersion)
                    {
                        currentVersion = version;
                    }

                    possibleDeploymentVersions.Add(
                        new SchemaDeploymentVersionDto { Id = version.Id, Name = version.Name }
                    );
                }
            }

            var response = new ListResponseModel
            {
                CurrentDeploymentVersionId = currentVersion != null ? currentVersion.Id : null,
                DeploymentVersions = possibleDeploymentVersions,
                Results = dbCompareResults
                    .Select(result => new SchemaDbCompareResultDto
                    {
                        SchemaItemId = result.SchemaItem?.Id.ToString() ?? string.Empty,
                        SchemaItemType = result.SchemaItemType?.Name ?? string.Empty,
                        ResultType = result.ResultType.ToString(),
                        ItemName = result.ItemName ?? string.Empty,
                        Remark = result.Remark ?? string.Empty,
                        Script = result.Script ?? string.Empty,
                        Script2 = result.Script2 ?? string.Empty,
                        PlatformName = result.Platform?.Name ?? string.Empty,
                    })
                    .ToList(),
            };

            return Ok(response);
        });
    }
}
