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

using System.IO.Compression;
using System.Text;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.Composer.BuilderTasks;

public class DownloadFileModelBuilderTask(
    IFileSystemService fileSystemService,
    IConnectionStringService connectionStringService
) : IDownloadFileModelBuilderTask
{
    public string Name => "Download ORIGAM model-root from repository";
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    private string repositoryZipPath;

    public void Execute(Project project)
    {
        repositoryZipPath = Path.Combine(project.ProjectFolder, "master.zip");
        
        DownloadModelFromRepository(origamRepositoryUrl: project.OrigamRepositoryUrl);
        UnzipDefaultModelAndCopy(projectFolder: project.ProjectFolder);
        CreateCustomAssetsFolder(projectFolder: project.ProjectFolder);
        CreateEnvFile(project);
    }

    private void CleanupUnnecessaryFiles(string projectFolder)
    {
        var buildPath = Path.Combine(projectFolder, "build");
        fileSystemService.DeleteDirectory(buildPath);

        DeleteFileIfExists(Path.Combine(projectFolder, "LICENSE"));
        DeleteFileIfExists(Path.Combine(projectFolder, ".gitignore"));
    }

    private static void DeleteFileIfExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        File.SetAttributes(filePath, FileAttributes.Normal);
        File.Delete(filePath);
    }

    private void DownloadModelFromRepository(string origamRepositoryUrl)
    {
        if (repositoryZipPath == null)
        {
            throw new Exception(Strings.RepositoryZipPath_not_set);
        }

        using var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync(origamRepositoryUrl).Result;
        response.EnsureSuccessStatusCode();

        using var fs = new FileStream(repositoryZipPath, FileMode.Create, FileAccess.Write);
        response.Content.CopyToAsync(fs).Wait();
    }

    private void UnzipDefaultModelAndCopy(string projectFolder)
    {
        if (repositoryZipPath == null)
        {
            throw new Exception("RepositoryZipPath is not set.");
        }

        var tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(repositoryZipPath, tempExtractPath);

        var modelRootPath = Path.Combine(tempExtractPath, "origam-master", "model-root");

        if (Directory.Exists(modelRootPath))
        {
            CleanupUnnecessaryFiles(modelRootPath);
            CopyDirectory(modelRootPath, projectFolder);
        }
        else
        {
            throw new DirectoryNotFoundException(
                string.Format(Strings.Model_root_directory_not_found, modelRootPath)
            );
        }

        if (Directory.Exists(tempExtractPath))
        {
            Directory.Delete(tempExtractPath, true);
        }
        if (File.Exists(repositoryZipPath))
        {
            File.Delete(repositoryZipPath);
        }
    }

    private void CreateCustomAssetsFolder(string projectFolder)
    {
        var dir = new DirectoryInfo(Path.Combine(projectFolder, "customAssets"));
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
            file.CopyTo(targetFilePath);
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

    private string WebSiteUrl(Project project)
    {
        if (project.DockerPort == Common.Constants.DefaultHttpsPort)
        {
            return "https://localhost";
        }
        return "https://localhost:" + project.DockerPort;
    }

    public void Rollback(Project project) { }
}
