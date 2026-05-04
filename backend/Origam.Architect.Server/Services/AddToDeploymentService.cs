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
        Platform platformParsed = platformResolveService.Resolve(requestedPlatformName: platform);

        var targetVersion = persistenceService.SchemaProvider.RetrieveInstance<DeploymentVersion>(
            instanceId: deploymentVersionId,
            useCache: false
        );
        if (targetVersion is null)
        {
            throw new Exception(message: "DeploymentVersion is not found.");
        }

        var selectedResults = schemaDbCompareResultsService.GetByIds(
            schemaItemIds: schemaItemIdList,
            platform: platformParsed
        );
        ProcessAllDeploymentActivities(version: targetVersion, selectedResults: selectedResults);
    }

    private void ProcessAllDeploymentActivities(
        DeploymentVersion version,
        List<SchemaDbCompareResult> selectedResults
    )
    {
        IService dataService = schemaService
            .GetProvider<ServiceSchemaItemProvider>()
            .ChildItems.Cast<IService>()
            .FirstOrDefault(predicate: service => service.Name == "DataService");

        foreach (SchemaDbCompareResult result in selectedResults)
        {
            var scripts = new[] { result.Script, result.Script2 };
            foreach (var script in scripts)
            {
                if (string.IsNullOrEmpty(value: script))
                {
                    continue;
                }

                var dbType = (DatabaseType)
                    Enum.Parse(
                        enumType: typeof(DatabaseType),
                        value: result.Platform.GetParseEnum(
                            dataDataService: result.Platform.DataService
                        )
                    );

                CreateAndPersistActivity(
                    name: result.SchemaItem.ModelDescription() + "_" + result.ItemName,
                    command: script,
                    version: version,
                    dataService: dataService,
                    databaseType: dbType
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
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        activity.Name =
            activity.ActivityOrder.ToString(format: "00000")
            + "_"
            + name.Replace(oldValue: " ", newValue: "_");
        activity.Service = dataService;
        activity.CommandText = command;
        activity.DatabaseType = databaseType;
        activity.Persist();
    }
}
