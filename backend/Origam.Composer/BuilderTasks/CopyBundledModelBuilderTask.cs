#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;

namespace Origam.Composer.BuilderTasks;

public class CopyBundledModelBuilderTask : ICopyBundledModelBuilderTask
{
    public string Name => "Copy bundled ORIGAM model-root";
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    public void Execute(Project project)
    {
        CopyBundledModelRoot(project.ProjectFolder);
        EnsureCustomAssetsFolderExists(project.ProjectFolder);
    }

    private static void CopyBundledModelRoot(string projectFolder)
    {
        string modelRootPath = Path.Join(AppContext.BaseDirectory, path2: "model-root");
        CopyDirectory(modelRootPath, projectFolder);
    }

    private static void EnsureCustomAssetsFolderExists(string projectFolder)
    {
        Directory.CreateDirectory(Path.Join(projectFolder, path2: "customAssets"));
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var sourceDirectory = new DirectoryInfo(sourceDir);
        if (!sourceDirectory.Exists)
        {
            throw new DirectoryNotFoundException(
                string.Format(Strings.Source_directory_not_found, sourceDir)
            );
        }

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in sourceDirectory.GetFiles())
        {
            string targetFilePath = Path.Join(destinationDir, file.Name);
            if (!File.Exists(targetFilePath))
            {
                file.CopyTo(targetFilePath);
            }
        }

        foreach (DirectoryInfo subdirectory in sourceDirectory.GetDirectories())
        {
            CopyDirectory(subdirectory.FullName, Path.Join(destinationDir, subdirectory.Name));
        }
    }

    public void Rollback(Project project) { }
}
