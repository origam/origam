using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Origam.DA.Service.FileSystemModeCheckers;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service
{
    class ReferenceFileChecker : IFileSystemModelChecker
    {
        private readonly DirectoryInfo topDirectory;
        private readonly FilePersistenceProvider filePersistenceProvider;

        public ReferenceFileChecker(FilePersistenceProvider filePersistenceProvider)
        {
            topDirectory = filePersistenceProvider.TopDirectory;
            this.filePersistenceProvider = filePersistenceProvider;
        }

        public ModelErrorSection GetErrors()
        {
            List<string> errors = topDirectory
                .GetAllFilesInSubDirectories()
                .Where(file => file.Name == OrigamFile.ReferenceFileName)
                .Select(ReadToFileData)
                .Select(CheckAndReturnErrors)
                .Where(errMessage => !string.IsNullOrEmpty(errMessage))
                .ToList();

            return new ModelErrorSection
            (
                caption : "Invalid Reference Files",
                errorMessages : errors
            );
        }

        private string CheckAndReturnErrors(ReferenceFileData fileData)
        {
            Guid groupId = fileData.ParentFolderIds.GroupId;
            Guid packageId = fileData.ParentFolderIds.PackageId;

            if (filePersistenceProvider.RetrieveInstance<SchemaItemGroup>(groupId) == null)
            {
                return "Group \"" + groupId + "\" referenced in \"file://" + fileData.XmlFileData.FileInfo.FullName + "\" cannot be found.";
            }

            if (filePersistenceProvider.RetrieveInstance<SchemaExtension>(packageId) == null)
            {
                return "Group \"" + groupId + "\" referenced in \"file://" + fileData.XmlFileData.FileInfo.FullName + "\" cannot be found.";
            }

            return null;
        }

        private static ReferenceFileData ReadToFileData(FileInfo groupReferenceFile)
        {
            var xmlFileDataFactory = new XmlFileDataFactory(new List<MetaVersionFixer>());
            Result<XmlFileData, XmlLoadError> result = xmlFileDataFactory.Create(groupReferenceFile);

            var xmlFileData = new ReferenceFileData(result.Value);
            return xmlFileData;
        }
    }
}