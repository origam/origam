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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.OrigamEngine;
using Origam.Schema;

namespace Origam.DA.Service
{
    public class OrigamFile : ITrackeableFile
    {
        internal const string IdAttribute = "id";
        internal const string IsFolderAttribute = "isFolder";
        internal const string ParentIdAttribute = "parentId";
        internal const string TypeAttribute = "type";
        public const string PackageFileName = PersistenceFiles.PackageFileName; 
        public const string ReferenceFileName = PersistenceFiles.ReferenceFileName; 
        public const string GroupFileName = PersistenceFiles.GroupFileName; 
        public static readonly string OrigamExtension = PersistenceFiles.Extension;
        public static readonly ElementName ModelPersistenceUri =
            ElementNameFactory.CreatePersistenceElName(
                $"http://schemas.origam.com/{VersionProvider.CurrentPersistenceMeta}/model-persistence");        
        public static readonly ElementName PackageUri =
            ElementNameFactory.CreatePackageElName(
                $"http://schemas.origam.com/{VersionProvider.CurrentPackageMeta}/package");
        public static readonly ElementName GroupUri =
            ElementNameFactory.CreateModelElName(
                $"http://schemas.origam.com/{VersionProvider.CurrentModelMeta}/model-element");
        public static readonly ElementName PackageNameUri =
            ElementNameFactory.CreatePackageElName(
                $"http://schemas.origam.com/{VersionProvider.CurrentPackageMeta}/packagepackage");
        public static readonly ElementName GroupNameUri =
            ElementNameFactory.CreateModelElName(
                $"http://schemas.origam.com/{VersionProvider.CurrentModelMeta}/model-elementgroup");

        private readonly OrigamFileManager origamFileManager;
        private readonly ExternalFileManager externalFileManger;
        private readonly OrigamXmlManager origamXmlManager;
        private OrigamPath path;
 
        private bool IsInAGroup => ParentFolderIds.CointainsNoEmptyIds;

        public XmlDocument DeferredSaveDocument
        {
            get => origamXmlManager.OpenDocument;
            set => origamXmlManager.OpenDocument = value;
        }

        public IEnumerable<FileInfo> ExternalFiles => externalFileManger
            .Files
            .Select(filePath => new FileInfo(filePath));
        public OrigamPath Path
        {
            get => path;
            set
            {
                path = value;
                origamXmlManager.Path = value;
            }
        }
        public string FileHash { get; private set; }
        public IDictionary<Guid, PersistedObjectInfo> ContainedObjects =>
            origamXmlManager.ContainedObjects;
        public ParentFolders ParentFolderIds =>
            origamXmlManager.ParentFolderIds;

        private bool IsEmpty => ContainedObjects.Count == 0;
        
        public override string ToString() => Path.Absolute;

        public OrigamFile(OrigamPath path, IDictionary<ElementName,Guid> parentFolderIds,
            OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue,
            bool isAFullyWrittenFile = false )
        {
            this.origamFileManager = origamFileManager;
            this.path = path;
            externalFileManger = new ExternalFileManager(this, origamPathFactory, fileEventQueue);
            if (isAFullyWrittenFile)
            {
                UpdateHash();
            }
            origamXmlManager = new OrigamXmlManager(
                path, 
                new ParentFolders(parentFolderIds, path),
                externalFileManger);
        }

        public OrigamFile(OrigamPath path, IDictionary<ElementName, Guid> parentFolderIds,
            OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue, string fileHash):
            this( path, parentFolderIds, origamFileManager,
             origamPathFactory, fileEventQueue)
        {           
            FileHash = fileHash;
        }

        public IFilePersistent LoadObject(Guid id, IPersistenceProvider provider, bool useCache) => 
            origamXmlManager.LoadObject(id, provider, useCache);

        public void RemoveInstance(Guid id)
        {
            origamXmlManager.RemoveInstance(id);
        }

        public virtual void WriteInstance(IFilePersistent instance,
            ElementName elementName)
        {
            origamXmlManager.WriteInstance(instance, elementName);
        }

        public object GetFromExternalFile(Guid instanceId, string fieldName) => 
            externalFileManger.GetValue(instanceId, fieldName);

        public static bool IsPersistenceFile(FileInfo fileInfo)
        {
            var ignoreCase = StringComparison.InvariantCultureIgnoreCase;
            return
                IsOrigamFile(fileInfo)||
                string.Equals(fileInfo.Name, PackageFileName, ignoreCase) ||
                string.Equals(fileInfo.Name, ReferenceFileName, ignoreCase) ||
                string.Equals(fileInfo.Name, GroupFileName, ignoreCase) ;
        }

        public static bool IsOrigamFile(FileInfo fileInfo)
        {
            var ignoreCase = StringComparison.InvariantCultureIgnoreCase;
            return string.Equals(fileInfo.Extension, OrigamExtension, ignoreCase);
        }

        public void FinalizeSave()    
        {
            externalFileManger.UpdateFilesOnDisc();
            if (IsEmpty)
            {
                origamFileManager.DeleteFile(this);
                origamFileManager.RemoveDirectoryIfEmpty(Path.Directory);
                return;
            }
            origamFileManager.WriteToDisc(this, DeferredSaveDocument);
            MakeNewReferenceFileIfNeeded(Path.Directory);
            origamXmlManager.InvalidateCache();
        }

        public void ClearCache()
        {
            origamXmlManager.InvalidateCache();
        }

        public Maybe<ExternalFile> GetExternalFile(FileInfo externalFile)
        {
            return externalFileManger.GetExternalFile(externalFile);
        }

        protected virtual void MakeNewReferenceFileIfNeeded(DirectoryInfo directory)
        {
            if (!IsInAGroup) return;

            bool referenceFileIsMissing = !directory
                .GetFiles()
                .Any(file =>
                    file.Name == GroupFileName ||
                    file.Name == ReferenceFileName);
            if (referenceFileIsMissing)
            {
                WriteGroupReferenceFile(
                    ParentFolderIds,
                    directory);
            }
        }
        public void UpdateHash()
        {
            FileHash =  new FileInfo(Path.Absolute).GetFileBase64Hash();
        } 

        private void WriteGroupReferenceFile
            (ParentFolders parentFolderIds, DirectoryInfo directory)
        {
            List<string> contentsList= new List<string>();
            contentsList.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            contentsList.Add($"<x:file xmlns:x=\"{ModelPersistenceUri}\">");
            contentsList.AddRange(parentFolderIds.Select(item => $"    <x:groupReference x:type=\"{item.Key}\" x:refId=\"{item.Value}\"/>"));
            contentsList.Add("</x:file>");
            string  contents = string.Join("\n", contentsList);

            string fullPath = System.IO.Path.Combine(directory.FullName, ReferenceFileName);
            
            origamFileManager.WriteReferenceFileToDisc(fullPath, contents, parentFolderIds);
        }

        public void RemoveFromCache(IPersistent instance)
        {
            origamXmlManager.RemoveFromCache(instance);
        }
    }
}
