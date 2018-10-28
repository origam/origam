using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service
{
    class Persistor
    {
        private readonly IPersistenceProvider persistenceProvider;
        private readonly FilePersistenceIndex index;
        private readonly IDictionary<string, OrigamFile> transactionStore =
            new Dictionary<string, OrigamFile>();
        private readonly OrigamFileFactory origamFileFactory;
        private readonly OrigamFileManager origamFileManager;
        private bool inTransaction;
        private readonly TrackerLoaderFactory trackerLoaderFactory;

        public Persistor(IPersistenceProvider persistenceProvider, 
            FilePersistenceIndex index, OrigamFileFactory origamFileFactory, 
            OrigamFileManager origamFileManager,TrackerLoaderFactory trackerLoaderFactory)
        {
            this.persistenceProvider = persistenceProvider;
            this.index = index;
            this.origamFileFactory = origamFileFactory;
            this.origamFileManager = origamFileManager;
            this.trackerLoaderFactory = trackerLoaderFactory;
        }

        public void Persist(IPersistent obj)
        {
            CheckObjectCanBePersisted(obj);
            IFilePersistent instance = (IFilePersistent)obj;
            ElementName elementName = ElementNameFactory.Create(instance.GetType());

            (OrigamFile origamFile, PersistedObjectInfo persistedObjectInfo) = 
                FindOrigamFileAndObjectInfo(instance);
            if (persistedObjectInfo == null && instance.IsDeleted)
            {
                return;
            }
            
            RenameRelatedItems(instance, persistedObjectInfo?.OrigamFile);
 
            if (origamFile == null)
            {                
                origamFile = origamFileFactory.New(
                    parentFolderIds: instance.ParentFolderIds,
                    relativePath: instance.RelativeFilePath,
                    isGroup: instance.IsFolder);
            }

            RemoveFromOldFile(instance, origamFile);

            var updatedObjectInfo = UpdateObjectInfo(elementName, instance, origamFile);

            if (instance.IsFileRootElement)
            {
                origamFile.ParentFolderIds.AddRange(instance.ParentFolderIds);
            }

            WriteToXmlDocument(origamFile, instance, elementName);
            UpdateIndex(instance, updatedObjectInfo);
            transactionStore[origamFile.Path.Relative] = updatedObjectInfo.OrigamFile;
            if (!inTransaction)
            {
                EndTransaction();
            }
            instance.IsPersisted = true;
        }

        private void RemoveFromOldFile(IFilePersistent instance, OrigamFile origamFile)
        {
            bool migthBeMovingBetweeFiles = !instance.IsDeleted && !instance.IsFileRootElement;
            if (migthBeMovingBetweeFiles)
            {
                OrigamFile oldOrigamFile = index.GetById(instance.Id)?.OrigamFile;
                bool movingBetweenFiles = oldOrigamFile?.Path.Relative != origamFile.Path.Relative;
                if (movingBetweenFiles)
                {
                    oldOrigamFile.DeferredSaveDocument = GetDocumentToWriteTo(oldOrigamFile);
                    oldOrigamFile.RemoveInstance(instance.Id);
                    transactionStore[oldOrigamFile.Path.Relative] = oldOrigamFile;
                }
            }
        }

        private void UpdateIndex(IFilePersistent instance,
            PersistedObjectInfo updatedObjectInfo)
        {
            if (instance.IsDeleted)
            {
                 index.Remove(updatedObjectInfo);
            }
            else
            {
                index.AddOrReplace(updatedObjectInfo);
            }
        }
        
        public void BeginTransaction()
        {
            inTransaction = true;
        }

        public void EndTransaction()
        {       
            foreach (OrigamFile origamFile in transactionStore.Values)
            {
                origamFile.FinalizeSave();
                origamFile.DeferredSaveDocument = null;
            }
            transactionStore.Clear();
            index.Persist(trackerLoaderFactory);
            inTransaction = false;
        }
        
        public PersistedObjectInfo GetObjInfoFromTransactionStore(Guid id)
        {
            foreach (var item in transactionStore)
            {
                if (item.Value.ContainedObjects.ContainsKey(id))
                {
                    return  item.Value.ContainedObjects[id];
                }
            }
            return null;
        }

        private PersistedObjectInfo UpdateObjectInfo(ElementName elementName,
            IFilePersistent instance, OrigamFile origamFile)
        {
            PersistedObjectInfo updatedObjectInfo = new PersistedObjectInfo(
                elementName: elementName,
                id: instance.Id,
                parentId: instance.FileParentId,
                isFolder: instance.IsFolder,
                origamFile: origamFile);
            origamFile.ContainedObjects[instance.Id] = updatedObjectInfo;

            bool fileWillBeMoved =
                origamFile != null &&
                !instance.IsDeleted &&
                instance.IsFileRootElement &&
                origamFile.Path.Relative != instance.RelativeFilePath;

            if (fileWillBeMoved)
            {
                updatedObjectInfo.OrigamFile.NewPath =
                    origamFileManager.MakeNewOrigamPath(
                        instance: instance,
                        resolveNamingConflicts: origamFile
                            .MultipleFilesCanBeInSingleFolder);
            }

            return updatedObjectInfo;
        }

        private void WriteToXmlDocument(OrigamFile origamFile, IFilePersistent instance,
            ElementName elementName)
        {
            origamFile.DeferredSaveDocument = GetDocumentToWriteTo(origamFile);
            if (instance.IsDeleted)
            {
                origamFile.RemoveInstance(instance.Id);
            }
            else
            {
                origamFile.WriteInstance(instance, elementName);
            }
        }

        private void RenameRelatedItems(IFilePersistent instance,
            OrigamFile containingFile)
        {
            switch (instance)
            {
                case SchemaExtension extension:
                    RenameSchemaExtension(containingFile, extension);
                    break;
                case SchemaItemGroup group:
                    RenameGroupDirectory(group, group.Name);
                    break;
                default:
                    break;
            }
        }

        private void RenameSchemaExtension(OrigamFile containingFile,
            SchemaExtension schemaExtension)
        {
            if (!schemaExtension.WasRenamed) return;
            if (containingFile == null) return;

            try
            {
                origamFileManager.RenameDirectory(containingFile.Path.Directory,
                    schemaExtension.Name);
                persistenceProvider.RetrieveList<SchemaItemGroup>(null)
                    .Where(group => group.Name == schemaExtension.OldName)
                    .ForEach(group => RenameGroup(group, schemaExtension.Name));
            } 
            catch (Exception e)
            {
                throw new Exception(
                    "There was an error during renaming. Some directories and groups were not renamed and model" +
                    "is in inconsistent state as a result of that. Please rename the already processed" +
                    "directories and groups back as a rollback of this operation is not implemented yet." +
                    "Original error: " + e.Message, e);
            }
        }

        private void RenameGroup(SchemaItemGroup group, string newName)
        {
            RenameGroupDirectory(group, newName);
            group.Name = newName;
            Persist(group);
        }

        private void RenameGroupDirectory(SchemaItemGroup group, string newName)
        {
            PersistedObjectInfo objInfo = index.GetById(group.Id);
            if (objInfo == null) return;
            DirectoryInfo groupDir = index.GetById(group.Id).OrigamFile.Path.Directory;
            origamFileManager.RenameDirectory(groupDir, newName);
        }

        private (OrigamFile, PersistedObjectInfo) FindOrigamFileAndObjectInfo(
            IFilePersistent instance)
        {
            PersistedObjectInfo objInfo;
            OrigamFile origamFile;
            if (inTransaction && transactionStore.ContainsKey(instance.RelativeFilePath))
            {
                Guid id = instance.Id;
                origamFile = transactionStore[instance.RelativeFilePath];
                origamFile.ContainedObjects.TryGetValue(id, out objInfo);
                return (origamFile, objInfo);
            }  
           
            PersistedObjectInfo parentObjInfo = index.GetParent(instance);
            if (parentObjInfo?.OrigamFile.Path.Relative == instance.RelativeFilePath)
            {
                origamFile = parentObjInfo.OrigamFile;
                origamFile.ContainedObjects.TryGetValue(instance.Id, out objInfo);
                return (origamFile, objInfo);
            }
            objInfo = index.GetById(instance.Id);
            if (objInfo == null) return (null, null);
            origamFile = objInfo.OrigamFile 
                         ?? index.GetByRelativePath(instance.RelativeFilePath);
            return (origamFile, objInfo);
        }

        private XmlDocument GetDocumentToWriteTo(OrigamFile origamFile)
        {
            if (inTransaction && transactionStore.ContainsKey(origamFile.Path.Relative))
            {
                return  transactionStore[origamFile.Path.Relative]
                    .DeferredSaveDocument;
            } 
            if (origamFile.Path.Exists)
            {
                XmlDocument openDoc = new XmlDocument();
                openDoc.Load(origamFile.Path.Absolute);
                return openDoc;
            }
            return OrigamXmlManager.NewDocument();
        }

        private static void CheckObjectCanBePersisted(IPersistent obj)
        {
            if (!(obj is IFilePersistent instance))
            {
                throw new Exception(
                    "Object does not implement IFilePersistent interface "
                    + obj.GetType());
            }
            if (!instance.IsDeleted)
            {
                DatabasePersistenceProvider.CheckInstanceRules(instance);
            }
        }  
    }
}