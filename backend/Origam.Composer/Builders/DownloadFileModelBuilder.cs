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
using Origam.Composer.Services;
using Origam.Git;
using Spectre.Console;

namespace Origam.Composer.Builders;

public class DownloadFileModelBuilder : AbstractBuilder
{
    public override string Name => "Download ORIGAM model-root from repository";

    private string SourcesFolder;
    private string RepositoryZipPath;

    public override void Execute(Project project)
    {
        AnsiConsole.MarkupLine($"[orange1][bold]Executing:[/][/] {Name}");

        SourcesFolder = project.ProjectFolder;
        RepositoryZipPath = Path.Combine(project.ProjectFolder, "master.zip");

        CreateSourceFolder();
        DownloadModelFromRepository();
        UnzipDefaultModelAndCopy();
        CreateCustomAssetsFolder();
    }

    private void CreateSourceFolder()
    {
        var dir = new DirectoryInfo(SourcesFolder);
        if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
        {
            throw new Exception($"Sources folder {SourcesFolder} already exists and is not empty.");
        }
        dir.Create();
    }

    private void DownloadModelFromRepository()
    {
        const string url = "https://github.com/origam/origam/archive/master.zip"; // TODO: Make configurable

        using var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();

        using var fs = new FileStream(RepositoryZipPath, FileMode.Create, FileAccess.Write);
        response.Content.CopyToAsync(fs).Wait();
    }

    private void UnzipDefaultModelAndCopy()
    {
        var tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(RepositoryZipPath, tempExtractPath);

        var modelRootPath = Path.Combine(tempExtractPath, "origam-master", "model-root");

        if (Directory.Exists(modelRootPath))
        {
            CopyDirectory(modelRootPath, SourcesFolder);
        }
        else
        {
            throw new DirectoryNotFoundException(
                $"Model root directory not found at: {modelRootPath}"
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

    private void CreateCustomAssetsFolder()
    {
        var dir = new DirectoryInfo(Path.Combine(SourcesFolder, "customAssets"));
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
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
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

    public override void Rollback()
    {
        if (Directory.Exists(SourcesFolder))
        {
            GitManager.DeleteDirectory(SourcesFolder); // TODO: Is GitManager necessary here?
        }
    }
}
