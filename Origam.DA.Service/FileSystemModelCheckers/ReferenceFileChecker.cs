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
        private readonly ReferenceFileFactory referenceFileFactory;

        public ReferenceFileChecker(FilePersistenceProvider filePersistenceProvider)
        {
            topDirectory = filePersistenceProvider.TopDirectory;
            this.filePersistenceProvider = filePersistenceProvider;
            var origamPathFactory = new OrigamPathFactory(topDirectory);
            referenceFileFactory = new ReferenceFileFactory(origamPathFactory);
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

        private  ReferenceFileData ReadToFileData(FileInfo groupReferenceFile)
        {
            var xmlFileDataFactory = new XmlFileDataFactory(new List<MetaVersionFixer>());
            Result<XmlFileData, XmlLoadError> result = xmlFileDataFactory.Create(groupReferenceFile);

            var xmlFileData = new ReferenceFileData(result.Value, referenceFileFactory);
            return xmlFileData;
        }
    }

    class ReferenceFileFactory: IOrigamFileFactory
    {
        private readonly OrigamPathFactory origamPathFactory;
        private List<ElementName> parentFolders = new List<ElementName>
        {
            OrigamFile.PackageNameUri,
            OrigamFile.GroupNameUri
        };

        public ReferenceFileFactory(OrigamPathFactory origamPathFactory)
        {
            this.origamPathFactory = origamPathFactory;
        }

        public ITrackeableFile New(FileInfo fileInfo, IDictionary<ElementName, Guid> parentFolderIds,
            bool isAFullyWrittenFile = false)
        {
            OrigamPath path = origamPathFactory.Create(fileInfo);
            return new OrigamReferenceFile(path, parentFolders);
        }

        public OrigamFile New(string relativePath, IDictionary<ElementName, Guid> parentFolderIds,
            bool isGroup, bool isAFullyWrittenFile = false)
        {
            throw new NotImplementedException();
        }

        public ITrackeableFile New(string relativePath, string fileHash,
            IDictionary<ElementName, Guid> parentFolderIds)
        {
            throw new NotImplementedException();
        }
    }
}