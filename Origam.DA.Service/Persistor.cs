using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service
{
    class Persistor
    {
        private readonly IPersistenceProvider persistenceProvider;
        private readonly FilePersistenceIndex index;
        private readonly TransactionStore transactionStore =
            new TransactionStore();
        private readonly OrigamFileFactory origamFileFactory;
        private readonly OrigamFileManager origamFileManager;
        private readonly TrackerLoaderFactory trackerLoaderFactory;
        public bool InTransaction { get; private set; }

        public Persistor(IPersistenceProvider persistenceProvider,
            FilePersistenceIndex index, OrigamFileFactory origamFileFactory,
            OrigamFileManager origamFileManager, TrackerLoaderFactory trackerLoaderFactory)
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
            IFilePersistent instance = (IFilePersistent) obj;
           

            var newOrigamFile = FindFileWhereInstanceShouldBeStored(instance);
            var currentOrigamFile = FindFileWhereInstanceIsStoredNow(instance);

            bool isNotInCurrentFile = currentOrigamFile
                          ?.ContainedObjects
                          .ContainsKey(instance.Id) == false;
            if (isNotInCurrentFile && instance.IsDeleted)
            {
                return;
            }

            RenameRelatedItems(instance,newOrigamFile);

            if (newOrigamFile != currentOrigamFile)
            {
                RemoveFromFile(instance, currentOrigamFile);
            }

            UpdateFile(instance, newOrigamFile);
            
            if (!InTransaction)
            {
                ProcessTransactionStore();
            }
            instance.IsPersisted = true;
        }

        private void UpdateFile(IFilePersistent instance, OrigamFile newFile)
        {
            ElementName elementName = ElementNameFactory.Create(instance.GetType());
            if (instance.IsFileRootElement)
            {
                newFile.ParentFolderIds.AddRange(instance.ParentFolderIds);
            }

            var objectInfo = CraeteObjectInfo(elementName, instance, newFile);
            WriteToXmlDocument(newFile, instance, elementName); 
            UpdateIndex(instance, objectInfo);
            transactionStore.AddOrReplace(newFile);
        }

        private void RemoveFromFile(IFilePersistent instance, OrigamFile origamFile)
        {
            if (origamFile == null) return;
            origamFile.DeferredSaveDocument = GetDocumentToWriteTo(origamFile);
            origamFile.RemoveInstance(instance.Id);
            transactionStore.AddOrReplace(origamFile);
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
            if (InTransaction) throw new Exception("Already in transaction! Cannot start a new one.");
            InTransaction = true;
        }

        public void EndTransaction()
        {
            if (!InTransaction) throw new Exception("Not in transaction! No transaction  to end.");
            ProcessTransactionStore();
            InTransaction = false;
        }

        private void ProcessTransactionStore()
        {
            foreach (OrigamFile origamFile in transactionStore.Files)
            {
                origamFile.FinalizeSave();
                origamFile.DeferredSaveDocument = null;
            }

            transactionStore.Clear();
            index.Persist(trackerLoaderFactory);
        }

        public void EndTransactionDontSave()
        {
            foreach (OrigamFile origamFile in transactionStore.Files)
            {
                origamFile.DeferredSaveDocument = null;
            }
            transactionStore.Clear();
            InTransaction = false;
        }

        public PersistedObjectInfo GetObjInfoFromTransactionStore(Guid id)
        {
            foreach (var origamFile in transactionStore.Files)
            {
                if (origamFile.ContainedObjects.ContainsKey(id))
                {
                    return origamFile.ContainedObjects[id];
                }
            }
            return null;
        }

        private PersistedObjectInfo CraeteObjectInfo(ElementName elementName,
            IFilePersistent instance, OrigamFile origamFile)
        {
            PersistedObjectInfo updatedObjectInfo = new PersistedObjectInfo(
                elementName: elementName,
                id: instance.Id,
                parentId: instance.FileParentId,
                isFolder: instance.IsFolder,
                origamFile: origamFile);
            origamFile.ContainedObjects[instance.Id] = updatedObjectInfo;
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
                    "There was an error during renaming. Some directories and groups were not renamed and model " +
                    "is in inconsistent state as a result of that. Please rename the already processed " +
                    "directories and groups back as a rollback of this operation is not implemented yet." +
                    "Original error: " + e.Message, e);
            }
        }

        private void RenameGroup(SchemaItemGroup group, string newName)
        {
            RenameGroupDirectory(group, newName);
            group.Name = newName;
            group.Persist();
        }

        private void RenameGroupDirectory(SchemaItemGroup group, string newName)
        {
            PersistedObjectInfo objInfo = index.GetById(group.Id);
            if (objInfo == null) return;
            DirectoryInfo groupDir = index.GetById(group.Id).OrigamFile.Path.Directory;
            origamFileManager.RenameDirectory(groupDir, newName);
        }

        private OrigamFile FindFileWhereInstanceIsStoredNow(IFilePersistent instance)
        {
            PersistedObjectInfo objInfo;
            OrigamFile origamFile;
            if (InTransaction && transactionStore.Contains(instance.RelativeFilePath))
            {
                Guid id = instance.Id;
                origamFile = transactionStore.Get(instance.RelativeFilePath);
                origamFile.ContainedObjects.TryGetValue(id, out objInfo);
                if(objInfo != null) return origamFile;
            }

            objInfo = index.GetById(instance.Id);
            origamFile = objInfo?.OrigamFile;
            return origamFile;
        }
        
        private OrigamFile FindFileWhereInstanceShouldBeStored(
            IFilePersistent instance)
        {
            if (InTransaction && transactionStore.Contains(instance.RelativeFilePath))
            {
                return transactionStore.Get(instance.RelativeFilePath);
            }

            PersistedObjectInfo parentObjInfo = index.GetParent(instance);
            if (parentObjInfo?.OrigamFile.Path.Relative == instance.RelativeFilePath)
            {
                return parentObjInfo.OrigamFile;
            }
            var objInfo = index.GetById(instance.Id);
            if(objInfo?.OrigamFile.Path.Relative == instance.RelativeFilePath)
            {
                return objInfo.OrigamFile;
            }

            OrigamFile origamFile = index.GetByRelativePath(instance.RelativeFilePath);
            if (origamFile?.Path.Relative == instance.RelativeFilePath)
            {
                return origamFile;
            }

            return origamFileFactory.New(
                     parentFolderIds: instance.ParentFolderIds,
                     relativePath: instance.RelativeFilePath,
                     isGroup: instance.IsFolder);
        }

        private XmlDocument GetDocumentToWriteTo(OrigamFile origamFile)
        {
            if (InTransaction && transactionStore.Contains(origamFile.Path.Relative))
            {
                return transactionStore.Get(origamFile.Path.Relative)
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
                RuleTools.DoOnFirstViolation(
                    objectToCheck: instance,
                    action: ex => throw new Exception("Instance " + instance.Id + " cannot be persisted!\n" + ex.Message, ex));
            }
        }
    }

    internal class TransactionStore
    {
        private readonly IDictionary<string, OrigamFile> pathFileDict =
            new Dictionary<string, OrigamFile>();
        public IEnumerable<OrigamFile> Files => pathFileDict.Values;

        public void AddOrReplace(OrigamFile file)
        {
            if (file.DeferredSaveDocument == null)
            {
                throw new ArgumentException();
            }
            pathFileDict[file.Path.Relative] = file;
        }
        public OrigamFile Get(string relativePath) => pathFileDict[relativePath];

        public bool Contains(string relativePath) => pathFileDict.ContainsKey(relativePath);

        public void Clear() => pathFileDict.Clear();
    }
}