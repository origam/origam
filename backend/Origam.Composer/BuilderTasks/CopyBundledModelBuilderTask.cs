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

using System.Reflection;
using System.Text;
using System.Text.Json;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.Composer.BuilderTasks;

public class CopyBundledModelBuilderTask(IConnectionStringService connectionStringService)
    : ICopyBundledModelBuilderTask
{
    public string Name => "Copy bundled ORIGAM model-root";
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    public void Execute(Project project)
    {
        CopyBundledModelRoot(projectFolder: project.ProjectFolder);
        CreateCustomAssetsFolder(projectFolder: project.ProjectFolder);
        CreateProjectManifest(project);
        CreateEnvFile(project);
    }

    private void CopyBundledModelRoot(string projectFolder)
    {
        string modelRootPath = Path.Combine(AppContext.BaseDirectory, "model-root");
        CopyDirectory(modelRootPath, projectFolder);
    }

    private void CreateCustomAssetsFolder(string projectFolder)
    {
        var dir = new DirectoryInfo(Path.Combine(projectFolder, path2: "customAssets"));
        if (!dir.Exists)
        {
            dir.Create();
        }
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                string.Format(Strings.Source_directory_not_found, sourceDir)
            );
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            if (!File.Exists(targetFilePath))
            {
                file.CopyTo(targetFilePath);
            }
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    private void CreateEnvFile(Project project)
    {
        string dbType =
            project.DatabaseType == DatabaseType.PgSql
                ? "postgresql"
                : project.DatabaseType.ToString().ToLower();

        var sb = new StringBuilder();
        sb.AppendLine($"OrigamSettings__DefaultSchemaExtensionId={project.NewPackageId}");
        sb.AppendLine(
            $"OrigamSettings__DataConnectionString={connectionStringService.GetConnectionString(project)}"
        );

        sb.AppendLine($"OrigamSettings__Name={project.Name}");
        sb.AppendLine(
            $"CustomAssetsConfig__PathToCustomAssetsFolder={"/home/origam/projectData/customAssets"}"
        );
        sb.AppendLine($"CustomAssetsConfig__RouteToCustomAssetsFolder=/customAssets");
        sb.AppendLine($"DatabaseType={dbType}");
        sb.AppendLine($"ExternalDomain_SetOnStart={WebSiteUrl(project)}");
        sb.Append("TZ=Europe/Prague");

        File.WriteAllText(
            Path.Combine(project.ProjectFolder, $"{project.Name}_Environments.env"),
            sb.ToString()
        );
    }

    private void CreateProjectManifest(Project project)
    {
        string manifestPath = Path.Combine(project.ProjectFolder, "origam-project.json");
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
        Assembly composerAssembly = typeof(CopyBundledModelBuilderTask).Assembly;
        return composerAssembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "OrigamVersion")
                .Value
            ?? throw new InvalidOperationException("Composer ORIGAM version is not set.");
    }

    private string WebSiteUrl(Project project)
    {
        if (project.DockerPort == Common.Constants.DefaultHttpsPort)
        {
            return "https://localhost";
        }
        return $"https://localhost:{project.DockerPort}";
    }

    public void Rollback(Project project) { }
}
