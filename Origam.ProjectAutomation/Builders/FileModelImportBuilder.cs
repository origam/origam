using System;
using System.IO;
using System.Linq;

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
            UnzipDefaultModel(project);
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
                Directory.Delete(sourcesFolder, true);
            }
        }

    }
}