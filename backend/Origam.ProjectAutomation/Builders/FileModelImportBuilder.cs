#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using Origam.Git;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Origam.ProjectAutomation.Builders;

namespace Origam.ProjectAutomation;
public class FileModelImportBuilder: AbstractBuilder
{
    private string sourcesFolder;
    public override string Name => "Import Model";
    public override void Execute(Project project)
    {
        sourcesFolder = project.SourcesFolder;
        CreateSourceFolder();
        UnzipDefaultModel(project);
        CreateCustomAssetsFolder(project.SourcesFolder);
        CreateAndFillNewProjectDirectory(project);
    }
    private void CreateAndFillNewProjectDirectory(Project project)
    {
        DirectoryInfo dir = new DirectoryInfo(sourcesFolder);
        if (dir.Exists)
        {
            string newDir = Path.Combine(sourcesFolder, DockerBuilder.DockerFolderName);
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            string cmdDocker = Path.Combine(newDir, project.Name + ".cmd");
            if (!File.Exists(cmdDocker))
            {
              using (StreamWriter writer = new StreamWriter(cmdDocker, false))
              {
                 writer.WriteLine(CreateCmdTemplate());
              }
            }
        }
        else
        {
            throw new Exception(sourcesFolder + " not exists!");
        }
    }
    private StringBuilder CreateCmdTemplate()
    {
        StringBuilder template = new StringBuilder();
        template.AppendLine("docker run --env-file \"{envFilePath}\" ^\n" +
                            "-it --name {projectName} ^\n" +
                            "-v \"{parentPathProject}\":/home/origam/HTML5/data/origam ^\n" +
                            "-p {dockerPort}:443 ^\n" +
                            "origam/server:master-latest.linux");
        return template;
    }
    private void UnzipDefaultModel(Project project)
    {
        ZipFile.ExtractToDirectory(project.DefaultModelPath, sourcesFolder);
    }
    private void CreateSourceFolder()
    {
        DirectoryInfo dir = new DirectoryInfo(sourcesFolder);
        if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
        {
            throw new Exception($"Sources folder {sourcesFolder} already exists and is not empty.");
        }
        dir.Create();
    }
    private void CreateCustomAssetsFolder(string sourcesFolder)
    {
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(sourcesFolder, "customAssets"));
        if (!dir.Exists)
        {
            dir.Create();
        }
    }
    public override void Rollback()
    {
        if (Directory.Exists(sourcesFolder))
        {
            GitManager.DeleteDirectory(sourcesFolder);
        }
    }
}
