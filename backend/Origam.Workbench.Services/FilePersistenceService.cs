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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Schema;
#if !NETSTANDARD
using Origam.Git;
#endif

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

    public FilePersistenceService(
        IMetaModelUpgradeService metaModelUpgradeService,
        IList<string> defaultFolders,
        string pathToRuntimeModelConfig,
        string basePath = null,
        bool watchFileChanges = true,
        bool useBinFile = true,
        bool checkRules = true,
        MetaModelUpgradeMode mode = MetaModelUpgradeMode.Ignore
    )
    {
        this.metaModelUpgradeService = metaModelUpgradeService;
        this.defaultFolders = defaultFolders;
        DirectoryInfo topDirectory = GetTopDirectory(basePath: basePath);
        topDirectory.Create();
        this.pathToRuntimeModelConfig = CheckRuntimeModelConfigPathAndUpdateGitIgnore(
            pathCandidate: pathToRuntimeModelConfig,
            topDirectory: topDirectory
        );
        var pathFactory = new OrigamPathFactory(toDirectory: topDirectory);
        var pathToIndexBin = new FileInfo(
            fileName: Path.Combine(path1: topDirectory.FullName, path2: "index.bin")
        );
        var index = new FilePersistenceIndex(pathFactory: pathFactory);
        var ignoredFileFilter = new FileFilter(
            fileExtensionsToIgnore: new HashSet<string> { "bak", "debug" },
            filesToIgnore: new List<FileInfo>
            {
                pathToIndexBin,
                new FileInfo(fileName: this.pathToRuntimeModelConfig),
            },
            directoryNamesToIgnore: new List<string> { ".git", "l10n" }
        );
        var fileChangesWatchDog = GetNewWatchDog(
            topDir: topDirectory,
            watchFileChanges: watchFileChanges,
            ignoredFileFilter: ignoredFileFilter
        );
        FileEventQueue = new FileEventQueue(index: index, watchDog: fileChangesWatchDog);
        var origamFileManager = new OrigamFileManager(
            index: index,
            origamPathFactory: pathFactory,
            fileEventQueue: FileEventQueue
        );
        var origamFileFactory = new OrigamFileFactory(
            origamFileManager: origamFileManager,
            defaultParentFolders: defaultFolders,
            origamPathFactory: pathFactory,
            fileEventQueue: FileEventQueue
        );
        var objectFileDataFactory = new ObjectFileDataFactory(
            origamFileFactory: origamFileFactory,
            parentFolders: defaultFolders
        );
        var xmlFileDataFactory = new XmlFileDataFactory();
        var trackerLoaderFactory = new TrackerLoaderFactory(
            topDirectory: topDirectory,
            objectFileDataFactory: objectFileDataFactory,
            origamFileFactory: origamFileFactory,
            xmlFileDataFactory: xmlFileDataFactory,
            pathToIndexFile: pathToIndexBin,
            useBinFile: useBinFile,
            filePersistence: index,
            metaModelUpgradeService: metaModelUpgradeService
        );
        index.InitItemTracker(trackerLoaderFactory: trackerLoaderFactory, mode: mode);

        schemaProvider = new FilePersistenceProvider(
            topDirectory: topDirectory,
            fileEventQueue: FileEventQueue,
            ignoredFileFilter: ignoredFileFilter,
            trackerLoaderFactory: trackerLoaderFactory,
            origamFileFactory: origamFileFactory,
            index: index,
            origamFileManager: origamFileManager,
            checkRules: checkRules,
            runtimeModelConfig: new RuntimeModelConfig(
                pathToConfigFile: this.pathToRuntimeModelConfig
            )
        );

        FileEventQueue.ReloadNeeded += OnReloadNeeded;
        SchemaListProvider = schemaProvider;
    }

    private string CheckRuntimeModelConfigPathAndUpdateGitIgnore(
        string pathCandidate,
        DirectoryInfo topDirectory
    )
    {
        if (string.IsNullOrWhiteSpace(value: pathCandidate))
        {
            string configFileName = "RuntimeModelConfiguration.json";
#if !NETSTANDARD
            IgnoreFileTools.TryAdd(
                ignoreFileDir: topDirectory.FullName,
                ignoreFileEntry: configFileName
            );
#endif
            return Path.Combine(path1: topDirectory.FullName, path2: configFileName);
        }
        if (!Path.IsPathRooted(path: pathCandidate))
        {
#if !NETSTANDARD
            IgnoreFileTools.TryAdd(
                ignoreFileDir: topDirectory.FullName,
                ignoreFileEntry: pathCandidate
            );
#endif
            return Path.Combine(path1: topDirectory.FullName, path2: pathCandidate);
        }
        return pathCandidate;
    }

    private static DirectoryInfo GetTopDirectory(string basePath)
    {
        if (basePath == null)
        {
            basePath = ConfigurationManager.GetActiveConfiguration().ModelSourceControlLocation;
        }
        if (string.IsNullOrEmpty(value: basePath))
        {
            throw new ArgumentException(
                message: "File system persisted model cannot be open because the "
                    + nameof(OrigamSettings.ModelSourceControlLocation)
                    + " is not set. "
            );
        }
        return new DirectoryInfo(path: basePath);
    }

    private void OnReloadNeeded(object sender, FileSystemChangeEventArgs args)
    {
        ReloadNeeded?.Invoke(sender: this, e: args);
    }

    private IFileChangesWatchDog GetNewWatchDog(
        DirectoryInfo topDir,
        bool watchFileChanges,
        FileFilter ignoredFileFilter
    )
    {
        if (!watchFileChanges)
        {
            return new NullWatchDog();
        }
        return new FileChangesWatchDog(topDir: topDir, ignoreFileFilter: ignoredFileFilter);
    }

    public Maybe<XmlLoadError> Reload() => schemaProvider.ReloadFiles();

    public Package LoadSchema(Guid schemaExtensionId)
    {
        schemaProvider.FlushCache();
        var loadedSchema =
            schemaProvider.RetrieveInstance(
                type: typeof(Package),
                primaryKey: new ModelElementKey(id: schemaExtensionId)
            ) as Package;
        if (loadedSchema == null)
        {
            throw new Exception(message: "Shema: " + schemaExtensionId + " could not be found");
        }
        HashSet<Guid> loadedPackageIds = new HashSet<Guid>(
            collection: loadedSchema.IncludedPackages.Select(selector: package => package.Id)
        );
        loadedPackageIds.Add(item: loadedSchema.Id);
        schemaProvider.LoadedPackages = loadedPackageIds;
        return loadedSchema;
    }

    public void InitializeService() { }

    public void UnloadService()
    {
        try
        {
            schemaProvider.PersistIndex(unloadProject: true);
        }
        finally
        {
            Dispose();
        }
    }

    public object Clone()
    {
        return new FilePersistenceService(
            metaModelUpgradeService: metaModelUpgradeService,
            defaultFolders: defaultFolders,
            pathToRuntimeModelConfig: pathToRuntimeModelConfig,
            mode: MetaModelUpgradeMode.Ignore
        );
    }

    public void InitializeRepository() { }

    public void Dispose()
    {
        schemaProvider?.Dispose();
        FileEventQueue.ReloadNeeded -= OnReloadNeeded;
        FileEventQueue?.Dispose();
        SchemaListProvider?.Dispose();
    }
}
