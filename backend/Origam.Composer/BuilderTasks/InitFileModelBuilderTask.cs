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

using Origam.Composer.DTOs;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.DA.Common.DatabasePlatform;
using Origam.Workbench.Services;

namespace Origam.Composer.BuilderTasks;

public class InitFileModelBuilderTask : AbstractDatabaseBuilderTask, IInitFileModelBuilderTask
{
    public override string Name => "Initialize model (from files)";

    private SchemaService SchemaService;

    public override void Execute(Project project)
    {
        var settings = new OrigamSettings
        {
            DataConnectionString = BuildConnectionStringArchitect(project),
            ModelSourceControlLocation = project.ModelFolder,
            DataDataService = project.GetDataDataService,
        };
        ConfigurationManager.SetActiveConfiguration(settings);

        OrigamEngine.OrigamEngine.InitializeRuntimeServices();
        SchemaService = ServiceManager.Services.GetService<SchemaService>();
        SchemaService.SchemaLoaded += EventHandler_SchemaLoaded!;

        SchemaService.LoadSchema(
            schemaExtensionId: new Guid(project.BasePackageId),
            isInteractive: true
        );
    }

    public override void Rollback(Project project)
    {
        SchemaService?.UnloadSchema();
        OrigamEngine.OrigamEngine.UnloadConnectedServices();
    }

    private void EventHandler_SchemaLoaded(object sender, bool isInteractive)
    {
        OrigamEngine.OrigamEngine.InitializeSchemaItemProviders(SchemaService);
        var deployment = ServiceManager.Services.GetService<IDeploymentService>();
        var parameterService = ServiceManager.Services.GetService<IParameterService>();

        var isEmpty = deployment.IsEmptyDatabase();
        if (isEmpty)
        {
            deployment.Deploy();
        }

        parameterService.RefreshParameters();
    }

    private string BuildConnectionStringArchitect(Project project)
    {
        if (project.DatabaseType == DatabaseType.MsSql)
        {
            return DataService(project.DatabaseType)
                .BuildConnectionString(
                    serverName: project.DatabaseHost,
                    port: project.DatabasePort,
                    databaseName: project.DatabaseName,
                    userName: project.DatabaseUserName,
                    password: project.DatabasePassword,
                    integratedAuthentication: project.DatabaseIntegratedAuthentication,
                    pooling: false
                );
        }

        if (project.DatabaseType == DatabaseType.PgSql)
        {
            return DataService(project.DatabaseType)
                .BuildConnectionString(
                    serverName: project.DatabaseHost,
                    port: project.DatabasePort,
                    databaseName: project.DatabaseName,
                    userName: project.DatabaseInternalUserName,
                    password: project.DatabaseInternalUserPassword,
                    integratedAuthentication: project.DatabaseIntegratedAuthentication,
                    pooling: false
                );
        }

        return null;
    }
}
