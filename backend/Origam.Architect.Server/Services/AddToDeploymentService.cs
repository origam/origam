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

using Origam.Architect.Server.Interfaces.Services;
using Origam.DA;
using Origam.DA.Common.DatabasePlatform;
using Origam.Schema.DeploymentModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class AddToDeploymentService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    IPlatformResolveService platformResolveService,
    ISchemaDbCompareResultsService schemaDbCompareResultsService
) : IAddToDeploymentService
{
    public void Process(string platform, Guid deploymentVersionId, List<Guid> schemaItemIdList)
    {
        Platform platformParsed = platformResolveService.Resolve(platform);

        var targetVersion = persistenceService.SchemaProvider.RetrieveInstance<DeploymentVersion>(
            deploymentVersionId,
            useCache: false
        );
        if (targetVersion is null)
        {
            throw new Exception("DeploymentVersion is not found.");
        }

        var selectedResults = schemaDbCompareResultsService.GetByIds(
            schemaItemIdList,
            platformParsed
        );
        ProcessAllDeploymentActivities(targetVersion, selectedResults);
    }

    private void ProcessAllDeploymentActivities(
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
}
