#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation
{
    public class FileModelImportBuilder: AbstractBuilder
    {
        private const string ModelZipName = "DefaultModel.zip";
        private string sourcesFolder;
        
        public override string Name => "Import Model";

        public override void Execute(Project project)
        {
            sourcesFolder = project.SourcesFolder;
            CreateSourceFolder();
            switch(project.TypeTemplate)
            {
                case TypeTemplate.Default:
                    UnzipDefaultModel(project);
                    break;
                case TypeTemplate.Open:
                case TypeTemplate.Template:
                    CloneGitRepository(project);
                    break;
                default:
                    throw new Exception("Bad TypeTemplate " + project.TypeTemplate.ToString());
            }
        }

        private void CloneGitRepository(Project project)
        {
            GitManager gitManager = new GitManager();
            gitManager.CloneRepository(project.GitRepositoryLink, sourcesFolder);
            project.NewPackageId = GetPackageId();
        }

        private string GetPackageId()
        {
            DirectoryInfo dir = new DirectoryInfo(sourcesFolder);
            String modelId = "";
            if (dir.Exists && dir.EnumerateFileSystemInfos().Any())
            {
                string[] exclude_dirs = new [] {"Root","Root Menu","Security","l10n", ".git" };
                DirectoryInfo model = dir.EnumerateDirectories().Where(it => !exclude_dirs.Contains(it.Name)).First() ;
                string xmlPath = Path.Combine(sourcesFolder, model.Name, ".origamPackage");
                if (File.Exists(xmlPath))
                {
                    FileInfo fi = new FileInfo(xmlPath);
                    using (XmlReader xmlReader = new XmlTextReader(fi.OpenRead()))
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.EndElement) continue;
                            Guid? retrievedId = XmlUtils.ReadId(xmlReader);
                            if (retrievedId.HasValue)
                            {
                                modelId = retrievedId.ToString();
                                break;
                            }
                        }
                    }
                }
            }
            return modelId;
        }

        private void UnzipDefaultModel(Project project)
        {
            string zipPath =
                Path.Combine(project.ServerTemplateFolder,"Model", ModelZipName);
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, sourcesFolder);
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

        public override void Rollback()
        {
            if (Directory.Exists(sourcesFolder))
            {
                GitManager.DeleteDirectory(sourcesFolder);
            }
        }
        

    }
}