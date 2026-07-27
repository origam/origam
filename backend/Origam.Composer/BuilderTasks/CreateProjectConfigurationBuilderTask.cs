#region license
/*
Copyright 2026 Advantage Solutions, s. r. o.

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

using System.Text;
using System.Text.Json;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Origam.DA.Common.DatabasePlatform;
using Origam.Extensions;

namespace Origam.Composer.BuilderTasks;

public class CreateProjectConfigurationBuilderTask(
    IConnectionStringService connectionStringService
) : ICreateProjectConfigurationBuilderTask
{
    public string Name => "Create project configuration";
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    public void Execute(Project project)
    {
        CreateProjectManifest(project);
        CreateEnvironmentFile(project);
    }

    private void CreateEnvironmentFile(Project project)
    {
        string databaseType =
            project.DatabaseType == DatabaseType.PgSql
                ? "postgresql"
                : project.DatabaseType.ToString().ToLower();

        var contents = new StringBuilder();
        contents.AppendLine($"OrigamSettings__DefaultSchemaExtensionId={project.NewPackageId}");
        contents.AppendLine(
            $"OrigamSettings__DataConnectionString={connectionStringService.GetConnectionString(project)}"
        );
        contents.AppendLine($"OrigamSettings__Name={project.Name}");
        contents.AppendLine(
            "CustomAssetsConfig__PathToCustomAssetsFolder=/home/origam/projectData/customAssets"
        );
        contents.AppendLine("CustomAssetsConfig__RouteToCustomAssetsFolder=/customAssets");
        contents.AppendLine($"DatabaseType={databaseType}");
        contents.AppendLine($"ExternalDomain_SetOnStart={GetWebsiteUrl(project)}");
        contents.Append("TZ=Europe/Prague");

        File.WriteAllText(
            Path.Join(project.ProjectFolder, $"{project.Name}_Environments.env"),
            contents.ToString()
        );
    }

    private static void CreateProjectManifest(Project project)
    {
        string manifestPath = Path.Join(project.ProjectFolder, path2: "origam-project.json");
        if (File.Exists(manifestPath))
        {
            return;
        }

        string databaseEngine =
            project.DatabaseType == DatabaseType.PgSql
                ? "postgresql"
                : project.DatabaseType.ToString().ToLower();
        var manifest = new
        {
            schemaVersion = 1,
            projectName = project.Name,
            origamVersion = GetOrigamVersion(),
            defaultSchemaExtensionId = project.NewPackageId,
            databaseEngine,
        };
        string json = JsonSerializer.Serialize(
            manifest,
            new JsonSerializerOptions { WriteIndented = true }
        );

        File.WriteAllText(manifestPath, json + Environment.NewLine);
    }

    private static string GetOrigamVersion()
    {
        return StringExtensions.GetAssemblyVersion(tag: "master");
    }

    private static string GetWebsiteUrl(Project project)
    {
        return project.DockerPort == Common.Constants.DefaultHttpsPort
            ? "https://localhost"
            : $"https://localhost:{project.DockerPort}";
    }

    public void Rollback(Project project) { }
}
