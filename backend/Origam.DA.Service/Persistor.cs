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
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service;
class Persistor
{
    private readonly IPersistenceProvider persistenceProvider;
    private readonly FilePersistenceIndex index;
    private readonly TransactionStore transactionStore =
        new TransactionStore();
    private readonly OrigamFileFactory origamFileFactory;
    private readonly OrigamFileManager origamFileManager;
    private readonly TrackerLoaderFactory trackerLoaderFactory;
    public bool IsInTransaction { get; private set; }
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
    public void Persist(IPersistent obj, bool checkRules)
    {
        CheckObjectCanBePersisted(obj, checkRules);
        IFilePersistent instance = (IFilePersistent) obj;
        OrigamFile newOrigamFile = FindFileWhereInstanceShouldBeStored(instance);
        OrigamFile currentOrigamFile = FindFileWhereInstanceIsStoredNow(instance);
        if (currentOrigamFile!= null 
            && !currentOrigamFile.ContainedObjects.ContainsKey(instance.Id) 
            && instance.IsDeleted)
        {
            return;
        }
        if (newOrigamFile != currentOrigamFile)
        {
            RemoveFromFile(instance, currentOrigamFile);
        }
        RenameRelatedItems(instance, currentOrigamFile);
        UpdateFile(instance, newOrigamFile);
        
        if (!IsInTransaction)
        {
            ProcessTransactionStore();
        }
        instance.IsPersisted = true;
    }
    private void UpdateFile(IFilePersistent instance, OrigamFile newFile)
    {
        if (instance.IsFileRootElement)
        {
            newFile.ParentFolderIds.AddOrReplaceRange(instance.ParentFolderIds);
        }
        var objectInfo = CreateObjectInfo(instance, newFile);
        WriteToXmlDocument(newFile, instance); 
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
        if (IsInTransaction) throw new Exception("Already in transaction! Cannot start a new one.");
        IsInTransaction = true;
    }
    public void EndTransaction()
    {
        if (!IsInTransaction) throw new Exception("Not in transaction! No transaction  to end.");
        ProcessTransactionStore();
        IsInTransaction = false;
    }
    private void ProcessTransactionStore()
    {
        transactionStore.ProcessTransactions();
        index.AddToPersist(trackerLoaderFactory,false);
    }
    public void EndTransactionDontSave()
    {
        transactionStore.Clear();
        IsInTransaction = false;
    }
    public PersistedObjectInfo GetObjInfoFromTransactionStore(Guid id)
    {
        return transactionStore.FindObjInfo(id);
    }
    private PersistedObjectInfo CreateObjectInfo(IFilePersistent instance, OrigamFile origamFile)
    {
        PersistedObjectInfo updatedObjectInfo = new PersistedObjectInfo(
            category: CategoryFactory.Create(instance.GetType()),
            id: instance.Id,
            parentId: instance.FileParentId,
            isFolder: instance.IsFolder,
            origamFile: origamFile,
            fullTypeName: instance.GetType().FullName,
            version: Versions.GetCurrentClassVersion(instance.GetType()));
        origamFile.ContainedObjects[instance.Id] = updatedObjectInfo;
        return updatedObjectInfo;
    }
    private void WriteToXmlDocument(OrigamFile origamFile, IFilePersistent instance)
    {
        origamFile.DeferredSaveDocument = GetDocumentToWriteTo(origamFile);
        if (instance.IsDeleted)
        {
            origamFile.RemoveInstance(instance.Id);
        }
        else
        {
            origamFile.WriteInstance(instance);
        }
    }
    private void RenameRelatedItems(IFilePersistent instance,
        OrigamFile containingFile)
    {
        if (containingFile == null)
        {
            return;
        }
        switch (instance)
        {
            case Package extension:
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
        Package package)
    {
        if (!package.WasRenamed) return;
        if (containingFile == null) return;
        try
        {
            origamFileManager.RenameDirectory(containingFile.Path.Directory,
                package.Name);
            persistenceProvider.RetrieveList<SchemaItemGroup>(null)
                .Where(group => group.Name == package.OldName)
                .ForEach(group => RenameGroup(group, package.Name));
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
        if (IsInTransaction && transactionStore.Contains(instance.RelativeFilePath))
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
        if (IsInTransaction && transactionStore.Contains(instance.RelativeFilePath))
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
    private OrigamXmlDocument GetDocumentToWriteTo(OrigamFile origamFile)
    {
        if (IsInTransaction && transactionStore.Contains(origamFile.Path.Relative))
        {
            return transactionStore.Get(origamFile.Path.Relative)
                .DeferredSaveDocument;
        }
        return origamFile.Path.Exists 
            ? new OrigamXmlDocument(origamFile.Path.Absolute) 
            : new OrigamXmlDocument();
    }
    private static void CheckObjectCanBePersisted(IPersistent obj, bool checkRules)
    {
        if (!(obj is IFilePersistent instance))
        {
            throw new Exception(
                "Object does not implement IFilePersistent interface "
                + obj.GetType());
        }
        if (checkRules && !instance.IsDeleted)
        {
            RuleTools.DoOnFirstViolation(
                objectToCheck: instance,
                action: ex => throw new Exception("Instance " + instance.Id + " cannot be persisted!\n" + ex.Message, ex));
        }
    }
    
    private void ScheduleFolderRenamingTaskIfApplicable(IFilePersistent instance, OrigamFile origamFile)
    {
        switch (instance)
        {
            case Package schemaExtension:
                transactionStore.FolderRenamingTasks.Enqueue(
                    new RenameSchemaExtensionTask(
                        origamFileManager: origamFileManager,
                        index: index,
                        persistenceProvider: persistenceProvider,
                        origamFile: origamFile,
                        package: schemaExtension));
                break;
            case SchemaItemGroup group:
                transactionStore.FolderRenamingTasks.Enqueue(
                    new RenameGroupDirectoryTask(
                        origamFileManager: origamFileManager,
                        persistenceIndex: index, 
                        group: group, 
                        newName: group.Name));
                break;
            default:
                break;
        }
    }
}
class RenameSchemaExtensionTask: IDeferredTask
{
    private readonly Package _package;
    private readonly OrigamFile origamFile;
    private readonly IPersistenceProvider persistenceProvider;
    private readonly FilePersistenceIndex index;
    private readonly OrigamFileManager origamFileManager;
    public RenameSchemaExtensionTask(OrigamFileManager origamFileManager,
        FilePersistenceIndex index,
        IPersistenceProvider persistenceProvider, OrigamFile origamFile,
        Package package)
    {
        this._package = package;
        this.origamFile = origamFile;
        this.origamFileManager = origamFileManager;
        this.index = index;
        this.persistenceProvider = persistenceProvider;
    }
    public void Run()
    {
        if (!_package.WasRenamed) return;
        if (origamFile == null) return;
        try
        {
            origamFileManager.RenameDirectory(origamFile.Path.Directory,
                _package.Name);
            persistenceProvider
                .RetrieveList<SchemaItemGroup>(null)
                .Where(group => group.Name == _package.OldName)
                .ForEach(group => RenameGroup(group, _package.Name));
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
        new RenameGroupDirectoryTask(
            origamFileManager,
            index,
            group, 
            newName)
        .Run();
        group.Name = newName;
        group.Persist();
    }
}
class RenameGroupDirectoryTask: IDeferredTask
{
    private readonly SchemaItemGroup group;
    private readonly string newName;
    private readonly FilePersistenceIndex persistenceIndex;
    private readonly OrigamFileManager origamFileManager;
    public RenameGroupDirectoryTask(OrigamFileManager origamFileManager,
        FilePersistenceIndex persistenceIndex, SchemaItemGroup group, string newName)
    {
        this.origamFileManager = origamFileManager;
        this.persistenceIndex = persistenceIndex;
        this.group = group;
        this.newName = newName;
    }
    public void Run()   
    {
        PersistedObjectInfo objInfo = persistenceIndex.GetById(group.Id);
        if (objInfo == null) return;
        DirectoryInfo groupDir = persistenceIndex.GetById(group.Id).OrigamFile.Path.Directory;
        origamFileManager.RenameDirectory(groupDir, newName);
    }
}

internal class TransactionStore
{
    private readonly ReaderWriterLockSlim readWriteLock =
        new ReaderWriterLockSlim();
    public Queue<IDeferredTask> FolderRenamingTasks { get; } =
        new Queue<IDeferredTask>();
    private readonly IDictionary<string, OrigamFile> pathFileDict =
        new Dictionary<string, OrigamFile>();
     public void AddOrReplace(OrigamFile file)
     {
         readWriteLock.RunWriter(() =>
         {
            if (file.DeferredSaveDocument == null)
            {
                throw new ArgumentException();
            }
            pathFileDict[file.Path.Relative] = file;
         });
    }
    public OrigamFile Get(string relativePath)
    {
       return readWriteLock.RunReader(() => pathFileDict[relativePath]);
    }
    public bool Contains(string relativePath)
    {
        return readWriteLock.RunReader(() => pathFileDict.ContainsKey(relativePath));
    }
    public void Clear()
    {
        readWriteLock.RunWriter(() =>
        {
            foreach (OrigamFile origamFile in pathFileDict.Values)
            {
                origamFile.DeferredSaveDocument = null;
            }
            pathFileDict.Clear();
        });
    }
    public void ProcessTransactions()
    {
        readWriteLock.RunWriter(() =>
        {
            foreach (IDeferredTask task in FolderRenamingTasks)
            {
                task.Run();
            }
            IOrderedEnumerable<OrigamFile> topGroupFilesFirst = pathFileDict.Values
                .OrderBy(x => x is not OrigamGroupFile)
                .ThenBy(x => x.Path.Absolute.Length);
            foreach (OrigamFile origamFile in topGroupFilesFirst)
            {
                origamFile.FinalizeSave();
                origamFile.DeferredSaveDocument = null;
            }
            pathFileDict.Clear();
        });
    }
    public PersistedObjectInfo FindObjInfo(Guid id)
    {
        return readWriteLock.RunReader(() =>
        {
            foreach (var origamFile in pathFileDict.Values)
            {
                if (origamFile.ContainedObjects.ContainsKey(id))
                {
                    return origamFile.ContainedObjects[id];
                }
            }
            return null;
        });
    }
}
