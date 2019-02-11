#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

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
    public class OrigamFile:IDisposable
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

        public OrigamPath NewPath { get; set; }
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
        public virtual bool MultipleFilesCanBeInSingleFolder => true;

        
        public override string ToString() => Path.Absolute;

        public OrigamFile(OrigamPath path, IDictionary<ElementName,Guid> parentFolderIds,
            OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue,
            bool isAFullyWrittenFile = false )
        {
            this.origamFileManager = origamFileManager;
            origamFileManager.HashChanged += OnHashChanged;
            this.path = path;
            externalFileManger = new ExternalFileManager(this, origamPathFactory, fileEventQueue);
            if (isAFullyWrittenFile)
            {
                InitFileHash();
            }
            origamXmlManager = new OrigamXmlManager(
                path, 
                new ParentFolders(parentFolderIds, path),
                externalFileManger,
                origamPathFactory);
        }

        private void OnHashChanged(object sender, HashChangedEventArgs args)
        {
            FileHash = args.Hash;
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
            if (NewPath != null)
            {
                MoveToNewPath();
            }
            origamXmlManager.InvalidateCache();
        }

        public void ClearCache()
        {
            origamXmlManager.InvalidateCache();
        }

        private void MoveToNewPath()
        {
            IEnumerable<ExternalFilePath> externalPaths = origamXmlManager.GetExternalFilePaths();
            externalFileManger.MoveFiles(externalPaths,this);
            DirectoryInfo oldDirectory = Path.Directory;
            origamXmlManager.UpdateExternalLinks(this);
            origamFileManager.WriteToDisc(this, DeferredSaveDocument);
            origamFileManager.MoveFile(this);
            origamFileManager.RemoveDirectoryIfEmpty(oldDirectory);
            MakeNewReferenceFileIfNeeded(Path.Directory);
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
        private void InitFileHash()
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
            origamFileManager.WriteFileIfNotExist(fullPath, contents);
        }

        public void RemoveFromCache(IPersistent instance)
        {
            origamXmlManager.RemoveFromCache(instance);
        }

        public void Dispose()
        {
            origamFileManager.HashChanged -= OnHashChanged;
        }
    }

    class OrigamGroupFile : OrigamFile
    {
        public OrigamGroupFile(OrigamPath path, IDictionary<ElementName, Guid> parentFolderIds,
             OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue, bool isAFullyWrittenFile = false) 
            : base(path, parentFolderIds, origamFileManager,origamPathFactory, fileEventQueue, isAFullyWrittenFile)
        {
        }

        public OrigamGroupFile(OrigamPath path, IDictionary<ElementName, Guid> parentFolderIds,
            OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue, string fileHash) 
            : base(path, parentFolderIds, origamFileManager,origamPathFactory, fileEventQueue, fileHash)
        {
        }
        
        protected override void MakeNewReferenceFileIfNeeded(DirectoryInfo directory)
        {
        }
        
        public override void WriteInstance(IFilePersistent instance,
            ElementName elementName)
        {
            XmlNode contentNode = DeferredSaveDocument.ChildNodes[1];
            bool anotherGroupPresent = contentNode.ChildNodes
                .Cast<XmlNode>()
                .Where(node => node.Name == "group")
                .Any(node => Guid.Parse(node.Attributes["x:id"].Value) != instance.Id);

            if (anotherGroupPresent)
            {
                throw new InvalidOperationException("Single .origamGroup file can contain only one group");
            }
            base.WriteInstance(instance, elementName);
        }
        public override bool MultipleFilesCanBeInSingleFolder => false;
    }

    public class OrigamFileFactory
    {
        private readonly IList<ElementName> defaultParentFolders;
        private readonly OrigamFileManager origamFileManager;
        private readonly OrigamPathFactory origamPathFactory;
        private readonly FileEventQueue fileEventQueue;

        public OrigamFileFactory(
            OrigamFileManager origamFileManager, IList<ElementName> defaultParentFolders,
            OrigamPathFactory origamPathFactory,FileEventQueue fileEventQueue)
        {
            this.origamPathFactory = origamPathFactory;
            this.defaultParentFolders = defaultParentFolders;

            this.origamFileManager = origamFileManager;
            this.fileEventQueue = fileEventQueue;
        }
        
        public OrigamFile New(string  relativePath, IDictionary<ElementName,Guid> parentFolderIds,
            bool isGroup, bool isAFullyWrittenFile=false)
        {
            IDictionary<ElementName, Guid> parentFolders =
                GetNonEmptyParentFolders(parentFolderIds);

            OrigamPath path = origamPathFactory.CreateFromRelative(relativePath);

            if (isGroup)
            {
                return new OrigamGroupFile( path,  parentFolders,  
                    origamFileManager,origamPathFactory,fileEventQueue, isAFullyWrittenFile);
            } 
            else
            {
                return new OrigamFile( path,  parentFolders, origamFileManager,
                    origamPathFactory, fileEventQueue,isAFullyWrittenFile);  
            }
        }
        
        public OrigamFile New(FileInfo fileInfo, IDictionary<ElementName,Guid> parentFolderIds,
           bool isAFullyWrittenFile=false)
        {
            IDictionary<ElementName, Guid> parentFolders =
                GetNonEmptyParentFolders(parentFolderIds);

            OrigamPath path = origamPathFactory.Create(fileInfo);

            switch (fileInfo.Name)
            {
                case OrigamFile.GroupFileName:
                    return new OrigamGroupFile( path,  parentFolders,
                        origamFileManager,origamPathFactory,fileEventQueue, isAFullyWrittenFile);
                default:
                    return new OrigamFile( path,  parentFolders,
                        origamFileManager, origamPathFactory, fileEventQueue,
                        isAFullyWrittenFile);     
            }
        }

        private IDictionary<ElementName, Guid> GetNonEmptyParentFolders(IDictionary<ElementName, Guid> parentFolderIds)
        {
            IDictionary<ElementName, Guid> parentFolders = parentFolderIds.Count == 0
                ? new ParentFolders(defaultParentFolders)
                : parentFolderIds;
            return parentFolders;
        }

        public OrigamFile New(string relativePath, string fileHash,
            IDictionary<ElementName, Guid> parentFolderIds)
        {
            OrigamPath path = origamPathFactory.CreateFromRelative(relativePath);
            switch (path.FileName)
            {
                case OrigamFile.GroupFileName: 
                    return new OrigamGroupFile(path, parentFolderIds, 
                        origamFileManager, origamPathFactory, fileEventQueue, fileHash);
                default:
                    return new OrigamFile( path, parentFolderIds,
                        origamFileManager, origamPathFactory, fileEventQueue, fileHash);
            }
        }  
    }
}
