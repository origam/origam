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
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service
{
    public interface IFilePersistenceProvider: IPersistenceProvider
    {
        DirectoryInfo GetParentPackageDirectory(Guid itemId);
        bool Has(Guid id);
        DirectoryInfo TopDirectory { get; }
    }

    public class FilePersistenceProvider : AbstractPersistenceProvider,
        IFilePersistenceProvider
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);
        private FilePersistenceIndex index;
        private readonly Persistor persistor;
        private readonly FileEventQueue fileEventQueue;
        private readonly ILocalizationCache localizationCache;
        private readonly TrackerLoaderFactory trackerLoaderFactory;
        private readonly OrigamFileManager origamFileManager;

        public override bool InTransaction => persistor.InTransaction;
        public override ILocalizationCache LocalizationCache => localizationCache;

        public HashSet<Guid> LoadedPackages {
            set => index.LoadedPackages = value;
        }
        
        public DirectoryInfo TopDirectory { get; }

        public FilePersistenceProvider(DirectoryInfo topDirectory,
            FileEventQueue fileEventQueue,
            TrackerLoaderFactory trackerLoaderFactory, OrigamFileFactory origamFileFactory,
            FilePersistenceIndex index, OrigamFileManager origamFileManager)
        {
            this.origamFileManager = origamFileManager;
            this.trackerLoaderFactory = trackerLoaderFactory;
            localizationCache = new LocalizationCache();
            TopDirectory = topDirectory;
            this.index = index;
            this.fileEventQueue = fileEventQueue;
            persistor = new Persistor(
                this,index,origamFileFactory,origamFileManager, trackerLoaderFactory);
            fileEventQueue.Start();
        }

        #region UNUSED
        public ICompiledModel CompiledModel
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
            index.Persist(trackerLoaderFactory);
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
            ReloadFiles(tryUpdate: false);
            PersistIndex();
        }

        private IFilePersistent RetrieveInstance(PersistedObjectInfo persistedObjInfo,
            bool useCache=true)
        {
            Guid id = persistedObjInfo.Id;
            IFilePersistent retrievedInstance =
                persistedObjInfo.OrigamFile.LoadObject(id, this, useCache);
            if (retrievedInstance == null)
            {
                // we know the instance was persisted (we found PersistedObjectInfo),
                // but we could not find it...
                throw new Exception("PersistedObjectInfo was found but no instance was returned.");
            }
            return retrievedInstance;
        }

        public override object RetrieveValue(Guid instanceId, Type parentType,
            string fieldName)
        {
            PersistedObjectInfo objInfo = index.GetById(instanceId);
            return objInfo?.OrigamFile.GetFromExternalFile(instanceId,fieldName);
        }

        public T RetrieveInstance<T>(Guid id)
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
                throwNotFoundException: false);

        public override object RetrieveInstance(Type type, Key primaryKey, 
            bool useCache, bool throwNotFoundException) => 
            RetrieveInstance(primaryKey, useCache, throwNotFoundException);

        private IFilePersistent RetrieveInstance(Key primaryKey, bool useCache, 
            bool throwNotFoundException)
        {
            PersistedObjectInfo persistedObjectInfo =
                FindPersistedObjectInfo(primaryKey);
            if (persistedObjectInfo == null && throwNotFoundException)
            {
                throw new Exception("Could not find instance with id: "+primaryKey["Id"]);
            }
            return persistedObjectInfo == null ? null : 
                RetrieveInstance(persistedObjectInfo, useCache);
        }

        private PersistedObjectInfo FindPersistedObjectInfo(Key primaryKey)
        {
            Guid id = (Guid)primaryKey["Id"];
            if (id.Equals(Guid.Empty))
            {
                return null;
            }
            return FindPersistedObjectInfo(id);
        }

        public PersistedObjectInfo FindPersistedObjectInfo(Guid id)
        {
            return 
                persistor.GetObjInfoFromTransactionStore(id) ?? index.GetById(id);
        }

        public override void RefreshInstance(IPersistent persistentObject)
        {
            if (!(persistentObject is IFilePersistent origObject))
            {
                throw new InvalidOperationException(
                    $"Object does not implement {nameof(IFilePersistent)}");
            }
            if (!origObject.IsPersisted)
            {
                throw new InvalidOperationException(
                    ResourceUtils.GetString("NoRefreshForNotPersisted"));
            }
            
            IFilePersistent upToDateObject =
                this.RetrieveInstance(
                    primaryKey: origObject.PrimaryKey, 
                    useCache: true, 
                    throwNotFoundException: false);

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
            index.GetById(instance.Id)?.OrigamFile.RemoveFromCache(instance);
        }

        public override List<T> RetrieveList<T>(IDictionary<string, object> filter=null)
        {
            if (filter != null && filter.Count > 0)
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
            ElementName elementName = ElementNameFactory.Create(type);
            if (elementName == null) return new List<object>();

            return index
                .GetListByElementName(elementName)
                .Select(objInfo => RetrieveInstance(objInfo));
        }

        public override List<T> RetrieveListByParent<T>(Key primaryKey,
        string parentTableName, string childTableName, bool useCache)
        {
            return RetrieveListByParent(
                    (Guid) primaryKey["Id"],
                    ElementNameFactory.Create(typeof(T)),
                    typeof(T),
                    useCache)
                .ToList<T>();
        }

        private ArrayList RetrieveListByParent(Guid id, ElementName elementName,
            Type type, bool useCache)
        {         
            ArrayList result = new ArrayList();
            foreach (PersistedObjectInfo objInfo in index.GetByParentId(id))    
            {
                if (elementName == null || objInfo.ElementName == elementName)
                {
                    object instance = RetrieveInstance(objInfo, useCache);
                    if (type == null || type.IsInstanceOfType(instance))
                    {
                        result.Add(instance);
                    }
                }
            }
            return result;
        }

        public override void Persist(IPersistent obj)
        {
            persistor.Persist(obj);
            base.Persist(obj);
        }

        public override void FlushCache()
        {
            index.ClearCache();
        }

        public override void DeletePackage(Guid packageId)
        {
            string dependentPackages = string.Join(", ",
                RetrieveList<PackageReference>()
                    .Where(x => x.ReferencedPackage.Id == packageId)
                    .Select(x => x.Package.Name)
                );
            if (dependentPackages != "")
            {
                throw new Exception("Cannot delete this package because it is referenced by: "+dependentPackages);
            }

            DirectoryInfo packageDir = index.FindPackageDirectory(packageId);
            origamFileManager.RemoveDirectoryWithContents(packageDir);

            ReloadFiles(false);
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
        }

        public override T[] FullTextSearch<T>(string text)
        {
            bool lookingForAGuid =
                Guid.TryParse(text, out Guid guidToLookFor);

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
            PersistedObjectInfo objInfo = index.GetById(guidToLookFor);
            if (objInfo != null)
            {
                return new T[] { (T)RetrieveInstance(objInfo) };
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

        public override List<T> RetrieveListByType<T>(string itemType)
        {
            ElementName elName =
                ElementNameFactory.Create(OrigamFile.GroupUri, itemType);
            return index                       
                .GetListByElementName(elName)
                .Select(objInfo => RetrieveInstance(objInfo))
                .Cast<T>()
                .ToList();
        }

        public override List<T> RetrieveListByGroup<T>(Key primaryKey)
        {
            ElementName elementName = ElementNameFactory.Create(typeof(T));
            return index
                .GetByParentFolder(elementName,  (Guid)primaryKey["Id"])
                .Select(objInfo => RetrieveInstance(objInfo))
                .Where(x => x is T)
                .Cast<T>()
                .ToList();
        }

        public Maybe<XmlLoadError> ReloadFiles(bool tryUpdate)
        {
            localizationCache.Reload();
            return index.ReloadFiles(trackerLoaderFactory, tryUpdate);
        }

        public DirectoryInfo GetParentPackageDirectory(Guid itemId)
        {
            var item = (AbstractSchemaItem)RetrieveInstance(
                    type: null, 
                    primaryKey: new Key {{"Id", itemId}});
            Guid packageId = item.SchemaExtensionId;
            index
                .GetLoadedPackageDirectories()
                .TryGetValue(packageId, out var directory);
            return directory ?? throw new Exception("package: "+ packageId+" not found among currently loaded packages");
        }

        public bool Has(Guid id)
        {
            object retrieveInstance = RetrieveInstance(
                type: null,
                primaryKey: new Key {{"Id", id}});
            return retrieveInstance != null;
        }

        public List<string> GetFileErrors(string[] ignoreDirectoryNames)
        {
            return new FileSystemModelChecker(TopDirectory, ignoreDirectoryNames).GetFileErrors();
        }
    }
}
