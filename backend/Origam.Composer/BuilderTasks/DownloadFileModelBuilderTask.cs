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
        repositoryZipPath = Path.Combine(path1: project.ProjectFolder, path2: "master.zip");

        DownloadModelFromRepository(origamRepositoryUrl: project.OrigamRepositoryUrl);
        UnzipDefaultModelAndCopy(projectFolder: project.ProjectFolder);
        CreateCustomAssetsFolder(projectFolder: project.ProjectFolder);
        CreateEnvFile(project: project);
    }

    private void CleanupUnnecessaryFiles(string projectFolder)
    {
        var buildPath = Path.Combine(path1: projectFolder, path2: "build");
        fileSystemService.DeleteDirectory(directoryPath: buildPath);

        DeleteFileIfExists(filePath: Path.Combine(path1: projectFolder, path2: "LICENSE"));
        DeleteFileIfExists(filePath: Path.Combine(path1: projectFolder, path2: ".gitignore"));
    }

    private static void DeleteFileIfExists(string filePath)
    {
        if (!File.Exists(path: filePath))
        {
            return;
        }

        File.SetAttributes(path: filePath, fileAttributes: FileAttributes.Normal);
        File.Delete(path: filePath);
    }

    private void DownloadModelFromRepository(string origamRepositoryUrl)
    {
        if (repositoryZipPath == null)
        {
            throw new Exception(message: Strings.RepositoryZipPath_not_set);
        }

        using var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync(requestUri: origamRepositoryUrl).Result;
        response.EnsureSuccessStatusCode();

        using var fs = new FileStream(
            path: repositoryZipPath,
            mode: FileMode.Create,
            access: FileAccess.Write
        );
        response.Content.CopyToAsync(stream: fs).Wait();
    }

    private void UnzipDefaultModelAndCopy(string projectFolder)
    {
        if (repositoryZipPath == null)
        {
            throw new Exception(message: "RepositoryZipPath is not set.");
        }

        var tempExtractPath = Path.Combine(
            path1: Path.GetTempPath(),
            path2: Guid.NewGuid().ToString()
        );
        ZipFile.ExtractToDirectory(
            sourceArchiveFileName: repositoryZipPath,
            destinationDirectoryName: tempExtractPath
        );

        var modelRootPath = Path.Combine(
            path1: tempExtractPath,
            path2: "origam-master",
            path3: "model-root"
        );

        if (Directory.Exists(path: modelRootPath))
        {
            CleanupUnnecessaryFiles(projectFolder: modelRootPath);
            CopyDirectory(sourceDir: modelRootPath, destinationDir: projectFolder);
        }
        else
        {
            throw new DirectoryNotFoundException(
                message: string.Format(
                    format: Strings.Model_root_directory_not_found,
                    arg0: modelRootPath
                )
            );
        }

        if (Directory.Exists(path: tempExtractPath))
        {
            Directory.Delete(path: tempExtractPath, recursive: true);
        }
        if (File.Exists(path: repositoryZipPath))
        {
            File.Delete(path: repositoryZipPath);
        }
    }

    private void CreateCustomAssetsFolder(string projectFolder)
    {
        var dir = new DirectoryInfo(
            path: Path.Combine(path1: projectFolder, path2: "customAssets")
        );
        if (!dir.Exists)
        {
            dir.Create();
        }
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(path: sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                message: string.Format(format: Strings.Source_directory_not_found, arg0: sourceDir)
            );
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(path: destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(path1: destinationDir, path2: file.Name);
            file.CopyTo(destFileName: targetFilePath);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(path1: destinationDir, path2: subDir.Name);
            CopyDirectory(sourceDir: subDir.FullName, destinationDir: newDestinationDir);
        }
    }

    private void CreateEnvFile(Project project)
    {
        string dbType =
            project.DatabaseType == DatabaseType.PgSql
                ? "postgresql"
                : project.DatabaseType.ToString().ToLower();

        var sb = new StringBuilder();
        sb.AppendLine(handler: $"OrigamSettings__DefaultSchemaExtensionId={project.NewPackageId}");
        sb.AppendLine(
            handler: $"OrigamSettings__DataConnectionString={connectionStringService.GetConnectionString(project: project)}"
        );

        sb.AppendLine(handler: $"OrigamSettings__Name={project.Name}");
        sb.AppendLine(
            value: $"CustomAssetsConfig__PathToCustomAssetsFolder={"/home/origam/projectData/customAssets"}"
        );
        sb.AppendLine(value: $"CustomAssetsConfig__RouteToCustomAssetsFolder=/customAssets");
        sb.AppendLine(handler: $"DatabaseType={dbType}");
        sb.AppendLine(handler: $"ExternalDomain_SetOnStart={WebSiteUrl(project: project)}");
        sb.Append(value: "TZ=Europe/Prague");

        File.WriteAllText(
            path: Path.Combine(
                path1: project.ProjectFolder,
                path2: $"{project.Name}_Environments.env"
            ),
            contents: sb.ToString()
        );
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
