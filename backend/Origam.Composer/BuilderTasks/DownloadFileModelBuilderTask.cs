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
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;

namespace Origam.Composer.BuilderTasks;

public class DownloadFileModelBuilderTask(IFileSystemService fileSystemService)
    : IDownloadFileModelBuilderTask
{
    public string Name => "Download ORIGAM model-root from repository";
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    private string? RepositoryZipPath;

    public void Execute(Project project)
    {
        RepositoryZipPath = Path.Combine(project.ProjectFolder, "master.zip");

        CreateSourceFolder(project.ProjectFolder);
        DownloadModelFromRepository(project.OrigamRepositoryUrl);
        UnzipDefaultModelAndCopy(project.ProjectFolder);
        CreateCustomAssetsFolder(project.ProjectFolder);
    }

    private void CreateSourceFolder(string projectFolder)
    {
        var dir = new DirectoryInfo(projectFolder);
        if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
        {
            throw new Exception(
                string.Format(Strings.Sources_folder_already_exists, projectFolder)
            );
        }
        dir.Create();
    }

    private void DownloadModelFromRepository(string origamRepositoryUrl)
    {
        if (RepositoryZipPath == null)
        {
            throw new Exception(Strings.RepositoryZipPath_not_set);
        }

        using var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync(origamRepositoryUrl).Result;
        response.EnsureSuccessStatusCode();

        using var fs = new FileStream(RepositoryZipPath, FileMode.Create, FileAccess.Write);
        response.Content.CopyToAsync(fs).Wait();
    }

    private void UnzipDefaultModelAndCopy(string projectFolder)
    {
        if (RepositoryZipPath == null)
        {
            throw new Exception("RepositoryZipPath is not set.");
        }

        var tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(RepositoryZipPath, tempExtractPath);

        var modelRootPath = Path.Combine(tempExtractPath, "origam-master", "model-root");

        if (Directory.Exists(modelRootPath))
        {
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
        if (File.Exists(RepositoryZipPath))
        {
            File.Delete(RepositoryZipPath);
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

    public void Rollback(Project project)
    {
        if (Directory.Exists(project.ProjectFolder))
        {
            fileSystemService.DeleteDirectory(project.ProjectFolder);
        }
    }
}
