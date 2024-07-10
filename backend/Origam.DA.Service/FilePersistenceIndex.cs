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
using System.Threading;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;

namespace Origam.DA.Service;
public class FilePersistenceIndex: IDisposable
{
    private static readonly log4net.ILog log
        = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private ItemTracker itemTracker;
    private readonly ReaderWriterLockSlim readWriteLock =
        new ReaderWriterLockSlim();
    public HashSet<Guid> LoadedPackages { internal get; set; }
    public IEnumerable<OrigamFile> OrigamFiles => ItemTracker.OrigamFiles;
    private ItemTracker ItemTracker {
        get
        {
            return readWriteLock.RunReader(()=> itemTracker);  
        }
    }
    public FilePersistenceIndex(OrigamPathFactory pathFactory) 
        : this(new ItemTracker(pathFactory), new HashSet<Guid>())
    {
    }
    protected FilePersistenceIndex(ItemTracker itemTracker, HashSet<Guid> loadedPackages)
    {
        LoadedPackages = loadedPackages;
        this.itemTracker = itemTracker;
    }
    internal static FilePersistenceIndex GetPackageIgnoringVersion(
        FilePersistenceIndex original) => 
        new PackageIgnoringPersistenceIndex(
            original.ItemTracker,
            original.LoadedPackages);
    internal static FilePersistenceIndex GetPackageRespectingVersion(
        FilePersistenceIndex original)=>
        new FilePersistenceIndex(
            original.ItemTracker,
            original.LoadedPackages);
    public void ClearCache()
    {
        readWriteLock.RunWriter(()=> itemTracker.ClearCache());
    }
    
    public Maybe<XmlLoadError> ReloadFiles(TrackerLoaderFactory trackerLoaderFactory)
    {
        return readWriteLock.RunWriter(() =>
        {
            itemTracker.Clear();
            return trackerLoaderFactory.XmlLoader.LoadInto(
                itemTracker: itemTracker,
                mode: MetaModelUpgradeMode.Ignore);
        });
    }
    
    public void AddOrReplace(PersistedObjectInfo objInfo)
    {
        readWriteLock.RunWriter(() => itemTracker.AddOrReplace(objInfo));
    }
    
    public void AddOrReplace(OrigamFile origamFile)
    {
        readWriteLock.RunWriter(() => itemTracker.AddOrReplace(origamFile));
    }
    public void AddToPersist(TrackerLoaderFactory trackerLoaderFactory, bool unloadProject)
    {
        if (unloadProject)
        {
            trackerLoaderFactory.BinLoader.StopTask();
            PersistActualIndex(trackerLoaderFactory.BinLoader);
        }
        else
        {
            trackerLoaderFactory.BinLoader.MarkToSave(itemTracker);
        }
    }
    internal void PersistActualIndex(IBinFileLoader binFileLoader)
    {
            readWriteLock.RunWriter(() =>
               binFileLoader.Persist(itemTracker));
    }
    internal PersistedObjectInfo GetById(Guid id)
    {
        return readWriteLock.RunReader(() => { 
            PersistedObjectInfo objInfo = itemTracker.GetById(id);
            if (objInfo == null) return null;
            return BelongsToALoadedPackage(objInfo) ? objInfo : null;
        });
    }
    internal PersistedObjectInfo GetParent(IFilePersistent instance) =>
        instance.FileParentId == Guid.Empty 
            ? null 
            : GetById(instance.FileParentId);
    internal IEnumerable<PersistedObjectInfo> GetByParentId(Guid parentId)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.GetByParentId(parentId)
                .Where(BelongsToALoadedPackage)
            );
    }
    internal IEnumerable<PersistedObjectInfo> GetByParentFolder(
        string category, Guid folderId)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.GetByParentFolder(category, folderId)
                .Where(BelongsToALoadedPackage)
            );
    }
    internal IEnumerable<PersistedObjectInfo> GetListByCategory(
        string category)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.GetListByCategory(category)
                .Where(BelongsToALoadedPackage)
            );
    }
    internal IEnumerable<PersistedObjectInfo> GetByPackage(Guid packageId)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.GetByPackage(packageId));
    }
    protected virtual bool BelongsToALoadedPackage(PersistedObjectInfo objInfo)
    {
        if (LoadedPackages.Count == 0) return true;
        
        bool isPackageOrPackageReference =  
         objInfo.OrigamFile.Path.FileName == OrigamFile.PackageFileName;
        if (isPackageOrPackageReference) return true;
        Guid parentPackageId = objInfo.OrigamFile.ParentFolderIds.PackageId;
        return LoadedPackages.Contains(parentPackageId) ||
               LoadedPackages.Contains(objInfo.Id);
    }
    public Dictionary<Guid,DirectoryInfo> GetLoadedPackageDirectories()
    {
        return LoadedPackages.ToDictionary(
            id => id,
            FindPackageDirectory);
    }
    public DirectoryInfo FindPackageDirectory(Guid packageId)
    {
        return readWriteLock.RunReader(() => 
            itemTracker.PackegeFiles
                .First(orFile => orFile.ContainedObjects.Keys.Contains(packageId))
                .Path.Directory
            );  
    }
    public OrigamFile GetByRelativePath(string instanceRelativeFilePath)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.GetByPath(instanceRelativeFilePath));
    }
    public void InitItemTracker(TrackerLoaderFactory trackerLoaderFactory,
        MetaModelUpgradeMode mode)
    {
        readWriteLock.RunWriter(() =>
        {
#if ORIGAM_CLIENT // temporary solution of: https://bitbucket.org/origamsource/origam-source/issues/198/architect-does-not-upgrade-meta-model-if
            if (itemTracker.IsEmpty)
            {
                trackerLoaderFactory.BinLoader.LoadInto(itemTracker);
            }
            if (itemTracker.IsEmpty)
            {
                Maybe<XmlLoadError> error = trackerLoaderFactory.XmlLoader.LoadInto(
                    itemTracker: itemTracker,
                    mode: MetaModelUpgradeMode.ThrowIfOutdated);
                if(error.HasValue)
                {
                    throw new Exception(error.Value.Message);
                }
                trackerLoaderFactory.BinLoader.Persist(itemTracker);
            }
#else
            Maybe<XmlLoadError> error = trackerLoaderFactory.XmlLoader.LoadInto(
                itemTracker: itemTracker,
                mode: mode);
            if(error.HasValue)
            {
                throw new Exception(error.Value.Message);
            }
#endif
        });
    }
    public Maybe<string> GetFileHash(FileInfo file)
    {
        return readWriteLock.RunReader(() => itemTracker.GetFileHash(file));
    }
    public Maybe<ExternalFile> GetExternalFile(FileInfo file)
    {
        return readWriteLock.RunReader(() => itemTracker.GetExternalFile(file));
    }
    public void RenameDirectory(DirectoryInfo dirToRename, string newDirPath)
    {
        readWriteLock.RunWriter(() =>
            itemTracker.RenameDirectory(dirToRename, newDirPath));
    }
    public void AddOrReplaceHash(ITrackeableFile origamFile)
    {
        readWriteLock.RunWriter(() => 
            itemTracker.AddOrReplaceHash(origamFile));
    }
    public void RemoveHash(OrigamFile origamFile)
    {
        readWriteLock.RunWriter(() => itemTracker.RemoveHash(origamFile));
    }
    public void Remove(OrigamFile origamFile)
    {
        readWriteLock.RunWriter(() =>  itemTracker.Remove(origamFile));
    }
    
    public void Remove(PersistedObjectInfo updatedObjectInfo)
    {
        readWriteLock.RunWriter(() => itemTracker.Remove(updatedObjectInfo));
    }
    public bool HasFile(string newRelativePath)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.HasFile(newRelativePath));
    }
    public void Dispose()
    {
        readWriteLock?.Dispose();
        itemTracker?.Clear();
        itemTracker = null;
    }
    public IEnumerable<FileInfo> GetByDirectory(DirectoryInfo dir)
    {
        return readWriteLock.RunReader(() =>
            itemTracker.GetByDirectory(dir));
    }
}
internal class PackageIgnoringPersistenceIndex: FilePersistenceIndex
{
    public PackageIgnoringPersistenceIndex(ItemTracker itemTracker, 
         HashSet<Guid> loadedPackages) 
        : base(itemTracker, loadedPackages)
    {
    }
    protected override bool BelongsToALoadedPackage(PersistedObjectInfo objInfo) =>
        true;
}
