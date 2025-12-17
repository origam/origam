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
using Origam.DA;
using Origam.DA.Common.DatabasePlatform;
using Origam.DA.Service;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentScriptsGeneratorController(
    IPersistenceService persistenceService,
    SchemaService schemaService
) : ControllerBase
{
    [HttpPost("List")]
    public IActionResult List([Required] [FromBody] ListRequestModel requestModel)
    {
        GuardIsActiveExtensionSet();

        SecurityManager.SetServerIdentity();

        var platform = ResolvePlatform(requestModel.Platform);
        var dbCompareResults = GetCompareDbSchemaByPlatform(platform);

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
    }

    [HttpPost("AddToDeployment")]
    public IActionResult AddToDeployment(
        [Required] [FromBody] AddToDeploymentRequestModel requestModel
    )
    {
        GuardIsActiveExtensionSet();

        SecurityManager.SetServerIdentity();

        var platform = ResolvePlatform(requestModel.Platform);

        var requiredVersion = persistenceService.SchemaProvider.RetrieveInstance<DeploymentVersion>(
            requestModel.DeploymentVersionId,
            useCache: false
        );

        if (requiredVersion is null)
        {
            return BadRequest(Strings.DeploymentScripts_SelectItemIsNotDeploymentVersion);
        }

        var selectedResults = GetSchemaDbCompareResultsByIds(requestModel.SchemaItemIds, platform);
        RunAllDeploymentActivities(requiredVersion, selectedResults);

        return Ok();
    }

    [HttpPost("AddToModel")]
    public IActionResult AddToModel([Required] [FromBody] AddToModelRequestModel requestModel)
    {
        GuardIsActiveExtensionSet();

        SecurityManager.SetServerIdentity();

        Platform platform = ResolvePlatform(requestModel.Platform);

        var compareResults = GetSchemaDbCompareResultsByNames(
            requestModel.SchemaItemNames,
            platform
        );

        foreach (SchemaDbCompareResult result in compareResults)
        {
            if (result.ResultType == DbCompareResultType.MissingInSchema)
            {
                var schemaItem = result.SchemaItem;
                schemaItem.Group = schemaService
                    .GetProvider<EntityModelSchemaItemProvider>()
                    .GetGroup(schemaService.ActiveExtension.Name);
                schemaItem.RootProvider.ChildItems.Add(schemaItem);
                schemaItem.Persist();
            }
        }

        return Ok();
    }

    private List<SchemaDbCompareResult> GetSchemaDbCompareResultsByIds(
        List<string> schemaItemIds,
        Platform platform
    )
    {
        var dbCompareResults = GetCompareDbSchemaByPlatform(platform);

        var idSet = new HashSet<Guid>();
        if (schemaItemIds != null)
        {
            foreach (var idStr in schemaItemIds)
            {
                if (Guid.TryParse(idStr, out Guid id))
                {
                    idSet.Add(id);
                }
            }
        }

        var selectedResults = dbCompareResults
            .Where(r => r.SchemaItem != null && idSet.Contains(r.SchemaItem.Id))
            .ToList();

        return selectedResults;
    }

    private List<SchemaDbCompareResult> GetSchemaDbCompareResultsByNames(
        List<string> schemaItemNames,
        Platform platform
    )
    {
        var dbCompareResults = GetCompareDbSchemaByPlatform(platform);

        var selectedResults = dbCompareResults
            .Where(r => r.SchemaItem != null)
            .Where(r => schemaItemNames.Contains(r.SchemaItem.Name))
            .ToList();

        return selectedResults;
    }

    private void RunAllDeploymentActivities(
        DeploymentVersion version,
        List<SchemaDbCompareResult> selectedResults
    )
    {
        IService dataService = schemaService
            .GetProvider<ServiceSchemaItemProvider>()
            .ChildItems.Cast<IService>()
            .FirstOrDefault(service => service.Name == "DataService");

        foreach (SchemaDbCompareResult result in selectedResults)
        {
            var scripts = new[] { result.Script, result.Script2 };
            foreach (var script in scripts)
            {
                if (string.IsNullOrEmpty(script))
                {
                    continue;
                }

                var dbType = (DatabaseType)
                    Enum.Parse(
                        typeof(DatabaseType),
                        result.Platform.GetParseEnum(result.Platform.DataService)
                    );

                CreateAndPersistActivity(
                    result.SchemaItem.ModelDescription() + "_" + result.ItemName,
                    script,
                    version,
                    dataService,
                    dbType
                );
            }
        }
    }

    private void CreateAndPersistActivity(
        string name,
        string command,
        DeploymentVersion version,
        IService dataService,
        DatabaseType databaseType
    )
    {
        var activity = version.NewItem<ServiceCommandUpdateScriptActivity>(
            schemaService.ActiveSchemaExtensionId,
            null
        );
        activity.Name = activity.ActivityOrder.ToString("00000") + "_" + name.Replace(" ", "_");
        activity.Service = dataService;
        activity.CommandText = command;
        activity.DatabaseType = databaseType;
        activity.Persist();
    }

    private void GuardIsActiveExtensionSet()
    {
        if (schemaService.ActiveExtension == null || schemaService.ActiveExtension.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Active extension (package) is not set (activeExtensionId missing)."
            );
        }
    }

    private Platform ResolvePlatform(string requestedPlatformName)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var platforms = settings.GetAllPlatforms();

        if (string.IsNullOrWhiteSpace(requestedPlatformName))
        {
            var platform = platforms.FirstOrDefault();
            if (platform == null)
            {
                throw new InvalidOperationException("No platforms are configured.");
            }
            return platform;
        }

        var requested = requestedPlatformName.Trim();
        var match = platforms.FirstOrDefault(p =>
            string.Equals(p.Name, requested, StringComparison.OrdinalIgnoreCase)
        );

        if (match == null)
        {
            var available = string.Join(", ", platforms.Select(p => p.Name));
            throw new InvalidOperationException(
                $"Unknown platform '{requested}'. Available platforms: {available}."
            );
        }

        return match;
    }

    private List<SchemaDbCompareResult> GetCompareDbSchemaByPlatform(Platform platform)
    {
        var daPlatform = (AbstractSqlDataService)DataServiceFactory.GetDataService(platform);
        daPlatform.PersistenceProvider = persistenceService.SchemaProvider;
        var dbCompareResults = daPlatform.CompareSchema(persistenceService.SchemaProvider);
        foreach (SchemaDbCompareResult result in dbCompareResults)
        {
            result.Platform = platform;
        }
        return dbCompareResults;
    }
}
