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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.FileSystemModeCheckers;
using Origam.DA.Service.FileSystemModelCheckers;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service;
public interface IFilePersistenceProvider: IPersistenceProvider
{
    DirectoryInfo GetParentPackageDirectory(Guid itemId);
    bool Has(Guid id);
    DirectoryInfo TopDirectory { get; }
}
public class FilePersistenceProvider : AbstractPersistenceProvider,
    IFilePersistenceProvider
{
    private FilePersistenceIndex index;
    private readonly Persistor persistor;
    private readonly FileEventQueue fileEventQueue;
    private readonly FileFilter ignoredFileFilter;
    private readonly ILocalizationCache localizationCache;
    private readonly TrackerLoaderFactory trackerLoaderFactory;
    private readonly OrigamFileManager origamFileManager;
    private readonly IRuntimeModelConfig runtimeModelConfig;
    public override bool InTransaction => persistor.IsInTransaction;
    public override ILocalizationCache LocalizationCache => localizationCache;
    public HashSet<Guid> LoadedPackages {
        set => index.LoadedPackages = value;
    }
    
    public DirectoryInfo TopDirectory { get; }
    public FilePersistenceProvider(DirectoryInfo topDirectory,
        FileEventQueue fileEventQueue,
        FileFilter ignoredFileFilter,
        TrackerLoaderFactory trackerLoaderFactory,
        OrigamFileFactory origamFileFactory,
        FilePersistenceIndex index, OrigamFileManager origamFileManager,
        bool checkRules, IRuntimeModelConfig runtimeModelConfig)
    {
        CheckRules = checkRules;
        this.origamFileManager = origamFileManager;
        this.runtimeModelConfig = runtimeModelConfig;
        this.trackerLoaderFactory = trackerLoaderFactory;
        localizationCache = new LocalizationCache();
        TopDirectory = topDirectory;
        this.index = index;
        this.fileEventQueue = fileEventQueue;
        this.ignoredFileFilter = ignoredFileFilter;
        persistor = new Persistor(
            this,index,origamFileFactory,origamFileManager, trackerLoaderFactory);
        fileEventQueue.Start();
        InstancePersisted += (sender, persistent) =>
        {
            if (persistent is AbstractSchemaItem item)
            {
                ReferenceIndexManager.UpdateNowOrDeffer(item);
            }
        };
        runtimeModelConfig.ConfigurationReloaded += OnRuntimeModelConfigReloaded;
    }
    private void OnRuntimeModelConfigReloaded(object sender, List<Guid> invalidatedItemIds)
    {
        foreach (Guid itemId in invalidatedItemIds)
        {
            FindPersistedObjectInfo(itemId)?
                .OrigamFile.RemoveFromCache(itemId);
        }
    }
    #region UNUSED
    public override ICompiledModel CompiledModel
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    #endregion

    public override void RestrictToLoadedPackage(bool restrictToLoadedPackage)
    {
        index = restrictToLoadedPackage
            ? FilePersistenceIndex.GetPackageRespectingVersion(index) 
            : FilePersistenceIndex.GetPackageIgnoringVersion(index);
    }
    public void PersistIndex()
    {
        PersistIndex(false);
    }
    public void PersistIndex(bool unloadProject)
    {
        index.AddToPersist(trackerLoaderFactory,unloadProject);
    }
    public override void BeginTransaction()
    {
        persistor.BeginTransaction();
    }
    public override void EndTransaction()
    {       
        persistor.EndTransaction();
        base.EndTransaction();
    }
    public override void EndTransactionDontSave()
    {
        persistor.EndTransactionDontSave();
        Maybe<XmlLoadError> result = ReloadFiles();
        if(result.HasValue)
        {
            throw new Exception(result.Value.Message);
        }
        PersistIndex(false);
    }
    public override bool IsInTransaction => persistor.IsInTransaction;
    public bool CheckRules { get; }
    private IFilePersistent RetrieveInstance(PersistedObjectInfo persistedObjInfo,
        bool useCache=true)
    {
        var id = persistedObjInfo.Id;
        var retrievedInstance = persistedObjInfo.OrigamFile.LoadObject(
            id, this, useCache);
        if(retrievedInstance == null)
        {
            // we know the instance was persisted (we found PersistedObjectInfo),
            // but we could not find it...
            throw new Exception("PersistedObjectInfo was found but no instance was returned.");
        }
        runtimeModelConfig.SetConfigurationValues(retrievedInstance);
        return retrievedInstance;
    }
    public override object RetrieveValue(Guid instanceId, Type parentType,
        string fieldName)
    {
        var objInfo = index.GetById(instanceId);
        return objInfo?.OrigamFile.GetFromExternalFile(
            instanceId,fieldName);
    }
    public new T RetrieveInstance<T>(Guid id)
    {
        return (T)RetrieveInstance(typeof(T) ,new Key{{"Id",id}});
    }
    public override object RetrieveInstance(Type type, Key primaryKey) => 
        RetrieveInstance(type, primaryKey, true);
    public override object RetrieveInstance(Type type, Key primaryKey,
        bool useCache) => 
        RetrieveInstance(
            type: type, 
            primaryKey: primaryKey, 
            useCache: useCache, 
            throwNotFoundException: true);
    public override object RetrieveInstance(Type type, Key primaryKey, 
        bool useCache, bool throwNotFoundException) => 
        RetrieveInstance(primaryKey, useCache, throwNotFoundException);
    private IFilePersistent RetrieveInstance(Key primaryKey, bool useCache, 
        bool throwNotFoundException)
    {
        if((Guid)primaryKey["Id"] == Guid.Empty)
        {
            return null;
        }
        var persistedObjectInfo = FindPersistedObjectInfo(primaryKey);
        if(persistedObjectInfo == null && throwNotFoundException)
        {
            throw new Exception("Could not find instance with id: "+primaryKey["Id"]);
        }
        return persistedObjectInfo == null ? null : 
            RetrieveInstance(persistedObjectInfo, useCache);
    }
    private PersistedObjectInfo FindPersistedObjectInfo(Key primaryKey)
    {
        var id = (Guid)primaryKey["Id"];
        return id.Equals(Guid.Empty) ? null : FindPersistedObjectInfo(id);
    }
    public PersistedObjectInfo FindPersistedObjectInfo(Guid id)
    {
        return 
            persistor.GetObjInfoFromTransactionStore(id) ?? index.GetById(id);
    }
    public override List<string> Files(IPersistent persistentObject)
    {
        var result = new List<string>();
        var fileInfo = FindPersistedObjectInfo(persistentObject.Id);
        if(fileInfo == null)
        {
            // new not yet persisted instance
            return new List<string>();
        }
        result.Add(fileInfo.OrigamFile.Path.Relative);
        result.AddRange(fileInfo.OrigamFile.ExternalFiles
            .Select(x => x.FullName)
            .ToList());
        return result;
    }
    public override void RefreshInstance(IPersistent persistentObject)
    {
        if(!(persistentObject is IFilePersistent origObject))
        {
            throw new InvalidOperationException(
                $"Object does not implement {nameof(IFilePersistent)}");
        }
        if(!origObject.IsPersisted)
        {
            throw new InvalidOperationException(
                ResourceUtils.GetString("NoRefreshForNotPersisted"));
        }
        var upToDateObject =
            this.RetrieveInstance(
                primaryKey: origObject.PrimaryKey, 
                useCache: true, 
                throwNotFoundException: false) 
            ?? throw new Exception("Cannot refresh object that does not exist any more. Object id: "+persistentObject.Id);
        Reflector.CopyMembers(
            source: upToDateObject,
            target: origObject,
            attributeTypes: new[]
            {
                typeof(XmlAttributeAttribute),
                typeof(XmlExternalFileReference),
                typeof(XmlParentAttribute),
                typeof(XmlReferenceAttribute)
            }
        );
    }
    public override void RemoveFromCache(IPersistent instance)
    {
        index.GetById(instance.Id)?.OrigamFile.RemoveFromCache(instance.Id);
    }
    public override List<T> RetrieveList<T>(IDictionary<string, object> filter=null)
    {
        if(filter != null && filter.Count > 0)
        {
            throw new NotImplementedException("Filtering not implemented.");
        }
        return typeof(T)
            .GetAllPublicSubTypes()
            .SelectMany(RetrieveAll)
            .Distinct()
            .Where(x=> x != null)
            .Cast<T>()
            .ToList();
    }
    private IEnumerable<object> RetrieveAll(Type type)
    {
        var category = CategoryFactory.Create(type);
        if(string.IsNullOrWhiteSpace(category))
        {
            return new List<object>();
        }
        return index
            .GetListByCategory(category)
            .Select(objInfo => RetrieveInstance(objInfo));
    }
    public override List<T> RetrieveListByParent<T>(Key primaryKey,
    string parentTableName, string childTableName, bool useCache)
    {
        return RetrieveListByParent(
                (Guid) primaryKey["Id"],
                CategoryFactory.Create(typeof(T)),
                typeof(T),
                useCache)
            .ToList<T>();
    }
    private ArrayList RetrieveListByParent(Guid id, string category,
        Type type, bool useCache)
    {         
        var result = new ArrayList();
        foreach (var objInfo in index.GetByParentId(id))
        {
            if(!string.IsNullOrWhiteSpace(category)
            && (objInfo.Category != category))
            {
                continue;
            }
            object instance = RetrieveInstance(objInfo, useCache);
            if(type == null || type.IsInstanceOfType(instance))
            {
                result.Add(instance);
            }
        }
        return result;
    }
    public override void Persist(IPersistent obj)
    {
        persistor.Persist(obj, CheckRules);
        runtimeModelConfig.UpdateConfig(obj);
        base.Persist(obj);
    }
    public override void FlushCache()
    {
        index.ClearCache();
    }
    public override void DeletePackage(Guid packageId)
    {
        var dependentPackages = string.Join(", ",
            RetrieveList<PackageReference>()
                .Where(x => x.ReferencedPackage.Id == packageId)
                .Select(x => x.Package.Name)
            );
        if(dependentPackages != "")
        {
            throw new Exception("Cannot delete this package because it is referenced by: "+dependentPackages);
        }
        var packageDir = index.FindPackageDirectory(packageId);
        origamFileManager.RemoveDirectoryWithContents(packageDir);
        Maybe<XmlLoadError> result = ReloadFiles();
        if(result.HasValue)
        {
            throw new Exception(result.Value.Message);
        }
    }
    public override object Clone()
    {
        throw new NotImplementedException();
    }
    public override void Dispose()
    {
        fileEventQueue.Stop();
        localizationCache.Dispose();
        index?.Dispose();
        origamFileManager.Dispose();
        runtimeModelConfig.ConfigurationReloaded -= OnRuntimeModelConfigReloaded;
        runtimeModelConfig.Dispose();
    }
    public override T[] FullTextSearch<T>(string text)
    {
        var lookingForAGuid 
            = Guid.TryParse(text, out Guid guidToLookFor);
        return lookingForAGuid
            ? FindSingleItemById<T>(guidToLookFor)
            : FindStringInPersistedFiles<T>(text);
    }
    private T[] FindStringInPersistedFiles<T>(string text)
    {
        return new FlatFileSearcher(text)
            .SearchIn(index.GetLoadedPackageDirectories().Values)
            .Select(itemId => index.GetById(itemId))
            .Select(objInfo => RetrieveInstance(objInfo))
            .OfType<T>()
            .ToArray();
    }
    private T[] FindSingleItemById<T>(Guid guidToLookFor)
    {
        var objInfo = index.GetById(guidToLookFor);
        if(objInfo != null)
        {
            return new[] { (T)RetrieveInstance(objInfo) };
        }
        return new T[0];
    }
    public override List<T> RetrieveListByPackage<T>(Guid packageId)
    {
        return index.GetByPackage(packageId)
            .Select(objInfo => RetrieveInstance(objInfo))
            .Where(obj => obj is T)
            .Cast<T>()
            .ToList();
    }
    public override List<T> RetrieveListByCategory<T>(string category)
    {
        return index                       
            .GetListByCategory(category)
            .Select(objInfo => RetrieveInstance(objInfo))
            .Cast<T>()
            .ToList();
    }
    public override List<T> RetrieveListByGroup<T>(Key primaryKey)
    {
        var category = CategoryFactory.Create(typeof(T));
        return index
            .GetByParentFolder(category,  (Guid)primaryKey["Id"])
            .Select(objInfo => RetrieveInstance(objInfo))
            .Where(x => x is T)
            .Cast<T>()
            .ToList();
    }
    public Maybe<XmlLoadError> ReloadFiles()
    {
        localizationCache.Reload();
        return index.ReloadFiles(trackerLoaderFactory);
    }
    public DirectoryInfo GetParentPackageDirectory(Guid itemId)
    {
        var item = (AbstractSchemaItem)RetrieveInstance(
                type: null, 
                primaryKey: new Key {{"Id", itemId}});
        if(item == null)
        {
            throw new Exception("Item "+itemId+" not found in model");
        }
        var packageId = item.SchemaExtensionId;
        index
            .GetLoadedPackageDirectories()
            .TryGetValue(packageId, out var directory);
        return directory ?? throw new Exception("package: "+ packageId+" not found among currently loaded packages");
    }
    public bool Has(Guid id)
    {
        var retrievedInstance = RetrieveInstance(
            type: null,
            primaryKey: new Key {{"Id", id}}, 
            useCache: true, 
            throwNotFoundException: false);
        return retrievedInstance != null;
    }
    public List<ModelErrorSection> GetFileErrors(string[] ignoreDirectoryNames, CancellationToken cancellationToken)
    {
        List<FileInfo> modelDirectoryFiles = TopDirectory
            .GetAllFilesInSubDirectories()
            .ToList();
        return 
            new IFileSystemModelChecker[]
                {
                    new ModelStructureChecker(TopDirectory),
                    new FileNameChecker(this, index),
                    new DuplicateIdChecker(this, modelDirectoryFiles),
                    new ReferenceFileChecker(this, modelDirectoryFiles),
                    new DirectoryChecker(ignoreDirectoryNames, this),
                    new XmlReferencePropertyChecker(this),
                    new DeadModelFilesChecker(
                        filePersistenceProvider: this,
                        ignoredFileFilter: ignoredFileFilter, 
                        modelDirectoryFiles: modelDirectoryFiles)
                }
                .SelectMany(checker =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return checker.GetErrors();
                })
                .Where(errorSection => !errorSection.IsEmpty)
                .ToList();
    }   
}
