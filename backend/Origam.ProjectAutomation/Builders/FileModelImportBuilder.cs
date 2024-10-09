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

using Origam.DA.Service;
using Origam.Extensions;
using Origam.Git;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Origam.ProjectAutomation.Builders;
using static Origam.NewProjectEnums;

namespace Origam.ProjectAutomation;
public class FileModelImportBuilder: AbstractBuilder
{
    private string modelSourcesFolder;
    private string sourcesFolder;
    public override string Name => "Import Model";
    public override void Execute(Project project)
    {
        modelSourcesFolder = project.ModelSourceFolder;
        sourcesFolder = project.SourcesFolder;
        CreateSourceFolder();
        switch(project.TypeTemplate)
        {
            case TypeTemplate.Default:
                UnzipDefaultModel(project);

                CreateCustomAssetsFolder(project.SourcesFolder);
                CreateAndFillNewProjectDirectory(project);
                
                break;
            case TypeTemplate.Open:
            case TypeTemplate.Template:
                CloneGitRepository(project);
                CheckModelDirectory(project);
                CreateAndFillNewProjectDirectory(project);
                project.NewPackageId = GetFromDockerEnvFile(project)?? GetPackageId();
                break;
            default:
                throw new Exception("Bad TypeTemplate " + project.TypeTemplate.ToString());
        }
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
        template.AppendLine("docker run --env-file \"{envFilePath}\" -it --name {projectName} -v \"{parentPathProject}\":/home/origam/HTML5/data/origam -p {dockerPort}:443 origam/server:master-latest.linux");
        return template;
    }
    private void CloneGitRepository(Project project)
    {
        GitManager gitManager = new GitManager();
        gitManager.CloneRepository(project.GitRepositoryLink, sourcesFolder,
            project.RepositoryUsername,project.RepositoryPassword);
    }
    private void CheckModelDirectory(Project project)
    {
        DirectoryInfo dir = new DirectoryInfo(modelSourcesFolder);
        if (!dir.Exists)
        {
            modelSourcesFolder = sourcesFolder;
            project.ModelSourceFolder = sourcesFolder;
        }
    }
    private string GetPackageId()
    {
        string modelId = "";
        DirectoryInfo dir = new DirectoryInfo(modelSourcesFolder);
        if (string.IsNullOrEmpty(modelId) && dir.Exists && dir.EnumerateFileSystemInfos().Any())
        {
            string[] exclude_dirs = new [] {"Root","Root Menu","Security","l10n", ".git" };
            List<string> list_exclude_dirs = exclude_dirs.ToList();
            string xmlPath = "";
            do
            {
                DirectoryInfo model = dir.EnumerateDirectories().Where(directoryInfo => directoryInfo.Name.Contains("Root Menu")).First();
                if(model==null)
                {
                    throw new Exception("Can't find package for guidId. It looks like that it is not origam project.");
                }
                xmlPath = Path.Combine(modelSourcesFolder, model.Name, ".origamPackage");
                if(!File.Exists(xmlPath))
                {
                    list_exclude_dirs.Add(model.Name);
                }
            } while (!File.Exists(xmlPath));
            if (File.Exists(xmlPath))
            {
                FileInfo fileInfo = new FileInfo(xmlPath);
                OrigamXmlDocument xmlDocument = new OrigamXmlDocument(xmlPath);
                var xmlFileData = new XmlFileData(xmlDocument, fileInfo);
                modelId = XmlUtils.ReadId(xmlFileData) ?? XmlUtils.ReadNewModelId(xmlFileData);
            }
            else
            {
                throw new Exception("Can't find package for guidId. It looks like that it is not origam project.");
            }
        }
        if(string.IsNullOrEmpty(modelId))
        {
            throw new Exception("Can't find package ID. It looks like that it is problem with parse origamPackage. Please Contact Origam Team.");
        }
        return modelId;
    }
    private string GetFromDockerEnvFile(Project project)
    {
        string path = Path.Combine(project.SourcesFolder, DockerBuilder.DockerFolderName);
        if(!Directory.Exists(path))
        {
            return null;
        }
        var files = Directory.GetFiles(path, "*.env");
        if(files.Length == 0)
        {
            return null;
        }
        string[] lines = File.ReadAllLines(files[0]);
        string guidId = lines.Where(line => line.Contains("OrigamSettings_SchemaExtensionGuid"))
            .Select(line => { return line.Split("=")[1] ; }).FirstOrDefault();
        return string.IsNullOrEmpty(guidId)? null: guidId;
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
