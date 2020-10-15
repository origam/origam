#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using Origam.Git;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using static Origam.NewProjectEnums;

namespace Origam.ProjectAutomation
{
    public class FileModelImportBuilder: AbstractBuilder
    {
        private const string ModelZipName = "DefaultModel.zip";
        private string modelSourcesFolder;
        private string SourcesFolder;

        public override string Name => "Import Model";

        public override void Execute(Project project)
        {
            modelSourcesFolder = project.ModelSourceFolder;
            SourcesFolder = project.SourcesFolder;
            CreateSourceFolder();
            switch(project.TypeTemplate)
            {
                case TypeTemplate.Default:
                    CreateModelFolder();
                    UnzipDefaultModel(project);
                    CreateCustomAssetsFolder(project.SourcesFolder);
                    break;
                case TypeTemplate.Open:
                case TypeTemplate.Template:
                    CloneGitRepository(project);
                    CheckModelDirectory(project);
                    project.NewPackageId = GetPackageId();
                    break;
                default:
                    throw new Exception("Bad TypeTemplate " + project.TypeTemplate.ToString());
            }
        }

        private void CloneGitRepository(Project project)
        {
            GitManager gitManager = new GitManager();
            gitManager.CloneRepository(project.GitRepositoryLink, SourcesFolder,
                project.RepositoryUsername,project.RepositoryPassword);
        }
        private void CheckModelDirectory(Project project)
        {
            DirectoryInfo dir = new DirectoryInfo(modelSourcesFolder);
            if (!dir.Exists)
            {
                modelSourcesFolder = SourcesFolder;
                project.ModelSourceFolder = SourcesFolder;
            }
        }
        private string GetPackageId()
        {
            DirectoryInfo dir = new DirectoryInfo(modelSourcesFolder);
            String modelId = "";
            if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
            {
                string[] exclude_dirs = new [] {"Root","Root Menu","Security","l10n", ".git" };
                List<string> list_exclude_dirs = exclude_dirs.ToList();
                string xmlPath = "";
                do
                {
                    DirectoryInfo model = dir.EnumerateDirectories().Where(it => !list_exclude_dirs.Contains(it.Name)).First();
                    if(model==null)
                    {
                        throw new Exception("Can´t find package for guidId. It looks like that it is not origam project.");
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
                    modelId = XmlUtils.ReadId(xmlFileData)??XmlUtils.ReadNewModelId(xmlFileData);
                }
                else
                {
                    throw new Exception("Can´t find package for guidId. It looks like that it is not origam project.");
                }
            }
            if(string.IsNullOrEmpty(modelId))
            {
                throw new Exception("Can´t find package ID. It looks like that it is problem with parse origamPackage. Please Contact Origam Team.");
            }
            return modelId;
        }

        private void UnzipDefaultModel(Project project)
        {
            string zipPath =
                Path.Combine(project.ServerTemplateFolder,"Model", ModelZipName);
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, modelSourcesFolder);
        }

        private void CreateSourceFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(SourcesFolder);
            if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
            {
                throw new Exception($"Sources folder {SourcesFolder} already exists and is not empty.");
            }
            dir.Create();
        }
        private void CreateModelFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(modelSourcesFolder);
            if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
            {
                throw new Exception($"Sources folder {SourcesFolder} already exists and is not empty.");
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
            if (Directory.Exists(modelSourcesFolder))
            {
                GitManager.DeleteDirectory(modelSourcesFolder);
            }
        }
        

    }
}