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

using Origam.DA.ObjectPersistence;
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
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Git;
using Origam.OrigamEngine;

namespace Origam.Workbench.Services;
public class FilePersistenceService : IPersistenceService
{
    private readonly FilePersistenceProvider schemaProvider;
    private readonly IMetaModelUpgradeService metaModelUpgradeService;
    private readonly IList<string> defaultFolders;
    private readonly string pathToRuntimeModelConfig;
    public FileEventQueue FileEventQueue { get; }
    public IPersistenceProvider SchemaProvider => schemaProvider;
    public IPersistenceProvider SchemaListProvider { get; }
    
    public event EventHandler<FileSystemChangeEventArgs> ReloadNeeded;
        
    public FilePersistenceService(IMetaModelUpgradeService metaModelUpgradeService, 
        IList<string> defaultFolders, string pathToRuntimeModelConfig,
        string basePath = null, bool watchFileChanges = true, bool useBinFile = true,
        bool checkRules = true, MetaModelUpgradeMode mode = MetaModelUpgradeMode.Ignore)
    {
        this.metaModelUpgradeService = metaModelUpgradeService;
        this.defaultFolders = defaultFolders;
        DirectoryInfo topDirectory = GetTopDirectory(basePath);
        topDirectory.Create();
        this.pathToRuntimeModelConfig = 
            CheckRuntimeModelConfigPathAndUpdateGitIgnore(pathToRuntimeModelConfig, topDirectory);
        var pathFactory = new OrigamPathFactory(topDirectory);
        var pathToIndexBin = new FileInfo(Path.Combine(topDirectory.FullName, "index.bin"));
        var index = new FilePersistenceIndex(pathFactory);
        var ignoredFileFilter = new FileFilter(
            fileExtensionsToIgnore: new HashSet<string> {"bak", "debug"},
            filesToIgnore: new List<FileInfo>
            {
                pathToIndexBin, 
                new FileInfo(this.pathToRuntimeModelConfig)
            },
            directoryNamesToIgnore: new List<string> {".git", "l10n"});
        var fileChangesWatchDog =
            GetNewWatchDog(
                topDir: topDirectory,
                watchFileChanges: watchFileChanges, 
                ignoredFileFilter: ignoredFileFilter
            );
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
        var xmlFileDataFactory = new XmlFileDataFactory();
        var trackerLoaderFactory = new TrackerLoaderFactory(
                                        topDirectory, 
                                        objectFileDataFactory, 
                                        origamFileFactory,
                                        xmlFileDataFactory,
                                        pathToIndexBin,
                                        useBinFile,
                                        index,
                                        metaModelUpgradeService);
        index.InitItemTracker(
            trackerLoaderFactory: trackerLoaderFactory, 
            mode: mode);
        
        schemaProvider = new FilePersistenceProvider(
            topDirectory: topDirectory,
            fileEventQueue: FileEventQueue,
            ignoredFileFilter: ignoredFileFilter,
            index: index,
            origamFileFactory: origamFileFactory,
            trackerLoaderFactory: trackerLoaderFactory,
            origamFileManager: origamFileManager,
            checkRules: checkRules,
            runtimeModelConfig: new RuntimeModelConfig(this.pathToRuntimeModelConfig));
        
        FileEventQueue.ReloadNeeded += OnReloadNeeded;
        SchemaListProvider = schemaProvider;
    }
    private string CheckRuntimeModelConfigPathAndUpdateGitIgnore(string pathCandidate,
        DirectoryInfo topDirectory)
    {
        if (string.IsNullOrWhiteSpace(pathCandidate))
        {
            string configFileName = "RuntimeModelConfiguration.json";
            IgnoreFileTools.TryAdd(topDirectory.FullName, configFileName);
            return Path.Combine(topDirectory.FullName, configFileName);
        }
        if(!Path.IsPathRooted(pathCandidate))
        {
            IgnoreFileTools.TryAdd(topDirectory.FullName, pathCandidate);
            return Path.Combine(topDirectory.FullName, pathCandidate);
        }
        return pathCandidate;
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
    private IFileChangesWatchDog GetNewWatchDog(DirectoryInfo topDir,
        bool watchFileChanges, FileFilter ignoredFileFilter)
    {
        if (!watchFileChanges)
        {
            return new NullWatchDog();
        }
        return new FileChangesWatchDog(topDir, ignoredFileFilter);
    }
    public Maybe<XmlLoadError> Reload() => 
        schemaProvider.ReloadFiles();
    
    public Package LoadSchema(Guid schemaExtensionId)
    {
        schemaProvider.FlushCache();
        var loadedSchema = schemaProvider
                .RetrieveInstance(typeof(Package), new ModelElementKey(schemaExtensionId))
                as Package;
        if (loadedSchema == null)
        {
            throw new Exception("Shema: "+schemaExtensionId+" could not be found");  
        }
        HashSet<Guid> loadedPackageIds = new HashSet<Guid>(loadedSchema.IncludedPackages
            .Select(package => package.Id));
        loadedPackageIds.Add(loadedSchema.Id);
        schemaProvider.LoadedPackages = loadedPackageIds; 
        return loadedSchema;
    }
    
    public void InitializeService()
    {
    }
    public void UnloadService()
    {
        try
        {
            schemaProvider.PersistIndex(true);
        }
        finally
        {
            Dispose();
        }
    }
    public object Clone()
    {
        return new FilePersistenceService(metaModelUpgradeService, defaultFolders,
            mode: MetaModelUpgradeMode.Ignore, 
            pathToRuntimeModelConfig: pathToRuntimeModelConfig);
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
