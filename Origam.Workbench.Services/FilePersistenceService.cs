#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA;
using Origam.DA.Service;
using Origam.OrigamEngine;

namespace Origam.Workbench.Services
{
    public class FilePersistenceService : IPersistenceService
    {
        private readonly FilePersistenceProvider schemaProvider;
        private readonly IList<ElementName> defaultFolders;
        
        public FileEventQueue FileEventQueue { get; }
        public IPersistenceProvider SchemaProvider => schemaProvider;
        public IPersistenceProvider SchemaListProvider { get; }
        
        public event EventHandler<FileSystemChangeEventArgs> ReloadNeeded;
            
        public FilePersistenceService(IList<ElementName> defaultFolders,
            string basePath = null, bool watchFileChanges = true, bool useBinFile = true)    
        {
            this.defaultFolders = defaultFolders;
            var topDirectory = GetTopDirectory(basePath);
            topDirectory.Create();
            var pathFactory = new OrigamPathFactory(topDirectory);
            var pathToIndexBin = new FileInfo(Path.Combine(topDirectory.FullName, "index.bin"));
            var index = new FilePersistenceIndex(pathFactory);
            var fileChangesWatchDog =
                GetNewWatchDog(topDirectory, watchFileChanges, pathToIndexBin);
            FileEventQueue = 
                new FileEventQueue(index, fileChangesWatchDog);
            var origamFileManager = new OrigamFileManager(
                                            index,
                                            pathFactory,
                                            FileEventQueue);
            var origamFileFactory =  new OrigamFileFactory(
                                            origamFileManager, 
                                            defaultFolders,
                                            pathFactory,
                                            FileEventQueue);
            var objectFileDataFactory = new ObjectFileDataFactory(
                                            origamFileFactory, 
                                            defaultFolders);
            var xmlFileDataFactory =
                new XmlFileDataFactory(InitializeVersionFixers());
            var trackerLoaderFactory = new TrackerLoaderFactory(
                                            topDirectory, 
                                            objectFileDataFactory, 
                                            origamFileFactory,
                                            xmlFileDataFactory,
                                            pathToIndexBin,
                                            useBinFile);
            index.InitItemTracker(trackerLoaderFactory);
            
            schemaProvider = new FilePersistenceProvider(
                topDirectory: topDirectory,
                fileEventQueue: FileEventQueue,
                index: index,
                origamFileFactory: origamFileFactory,
                trackerLoaderFactory: trackerLoaderFactory,
                origamFileManager: origamFileManager);
            
            FileEventQueue.ReloadNeeded += OnReloadNeeded;
            SchemaListProvider = schemaProvider;
        }

        private static DirectoryInfo GetTopDirectory(string basePath)
        {
            if (basePath == null)
            {
                basePath = ConfigurationManager.GetActiveConfiguration().ModelSourceControlLocation;
            }

            if (string.IsNullOrEmpty(basePath))
            {
                throw new ArgumentException("File system persisted model cannot be open because the "+nameof(OrigamSettings.ModelSourceControlLocation) +" is not set. ");
            }

            return new DirectoryInfo(basePath);
        }

        private void OnReloadNeeded(object sender, FileSystemChangeEventArgs args)
        {
            ReloadNeeded?.Invoke(this, args);
        }

        private List<MetaVersionFixer> InitializeVersionFixers()
        {
            var metaVersionFixers = new List<MetaVersionFixer>
            {
                new MetaVersionFixer(
                    xmlNameSpaceName: "xmlns:x",
                    currentVersion: VersionProvider.CurrentPersistenceMeta,
                    failIfNamespaceNotFound: true),
                
                new MetaVersionFixer(
                        xmlNameSpaceName: "xmlns",
                        currentVersion: VersionProvider.CurrentModelMeta,
                        failIfNamespaceNotFound: false),
                
                new MetaVersionFixer(
                        xmlNameSpaceName: "xmlns:p",
                        currentVersion: new Version("1.0.0"), 
                        failIfNamespaceNotFound: false)   
            };
            return metaVersionFixers;
        }

        private IFileChangesWatchDog GetNewWatchDog(DirectoryInfo topDir,
            bool watchFileChanges, FileInfo pathToIndexBin)
        {
            if (!watchFileChanges)
            {
                return new NullWatchDog();
            }
            return new FileChangesWatchDog(
                topDir: topDir,
                fileExtensionsToIgnore: new HashSet<string>{"bak", "debug"},
                filesToIgnore: new List<FileInfo> {pathToIndexBin},
                directoryNamesToIgnore: new List<string>{".git"});
        }

        public Maybe<XmlLoadError> Reload(bool tryUpdate) => 
            schemaProvider.ReloadFiles(tryUpdate);

        public void LoadSchema(ArrayList extensions, bool append, bool loadDocumentation, bool loadDeploymentScripts, string transactionId)
        {
            throw new NotImplementedException();
        }

        public SchemaExtension LoadSchema(Guid schemaExtensionId, 
            bool loadDocumentation, bool loadDeploymentScripts,
            string transactionId) => LoadSchema(schemaExtensionId);

        private SchemaExtension LoadSchema(Guid schemaExtensionId)
        {
            schemaProvider.FlushCache();
            var loadedSchema = schemaProvider
                    .RetrieveInstance(typeof(SchemaExtension), new ModelElementKey(schemaExtensionId))
                    as SchemaExtension;
            if (loadedSchema == null)
            {
                throw new Exception("Shema: "+schemaExtensionId+" could not be found");  
            }
            HashSet<Guid> loadedPackageIds = loadedSchema.IncludedPackages
                .Select(package => package.Id)
                .ToHashSet();
            loadedPackageIds.Add(loadedSchema.Id);
            schemaProvider.LoadedPackages = loadedPackageIds; 
            return loadedSchema;
        }

        public SchemaExtension LoadSchema(Guid schemaExtensionId, Guid extraExtensionId, bool loadDocumentation, bool loadDeploymentScripts, string transactionId)
        {
            return LoadSchema(schemaExtensionId);
        }

        public void LoadSchemaList()
        {
        }

        public void UpdateRepository()
        {
        }

        public bool IsRepositoryVersionCompatible()
        {
            return true;
        }

        public bool CanUpdateRepository()
        {
            throw new NotImplementedException();
        }

        public void ExportPackage(Guid extensionId, string fileName)
        {
            throw new NotImplementedException();
        }

        public void MergePackage(Guid extensionId, System.Data.DataSet data, string transcationId)
        {
            throw new NotImplementedException();
        }

        public void InitializeService()
        {
        }

        public void UnloadService()
        {
            schemaProvider.PersistIndex();
            Dispose();
        }

        public object Clone()
        {
            return new FilePersistenceService(defaultFolders);
        }

        public void MergeSchema(System.Data.DataSet schema, Key activePackage)
        {
            throw new NotImplementedException();
        }


        public void InitializeRepository()
        {
        }

        public void Dispose()
        {
            schemaProvider?.Dispose();
            FileEventQueue.ReloadNeeded -= OnReloadNeeded;
            FileEventQueue?.Dispose();
            SchemaListProvider?.Dispose();
        }
    }
}
