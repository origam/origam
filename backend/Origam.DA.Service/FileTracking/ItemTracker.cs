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
using System.Reflection;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service;
public class ItemTracker
{
    private static readonly log4net.ILog log
        = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
    private readonly FileHashIndex fileHashIndex =
        new FileHashIndex();
    private readonly IDictionary<Guid, PersistedObjectInfo> objectLocationIndex =
        new Dictionary<Guid, PersistedObjectInfo>();
    private readonly ObjectInfoIndex<string> categoryIndex =
        new ObjectInfoIndex<string>();
    private readonly ObjectInfoIndex<Guid> treeIndex =
        new ObjectInfoIndex<Guid>();
    private readonly ObjectInfoIndex<string> folderIndex =
        new ObjectInfoIndex<string>();
    private readonly OrigamPathFactory pathFactory;
    public IEnumerable<OrigamFile> OrigamFiles => fileHashIndex.OrigamFiles;
    public IEnumerable<ITrackeableFile> AllFiles => fileHashIndex.AllFiles;
    public IEnumerable<OrigamFile> PackegeFiles => fileHashIndex.PackageFiles;
    public bool IsEmpty => !OrigamFiles.Any();
    public ItemTracker(OrigamPathFactory pathFactory)
    {
        this.pathFactory = pathFactory;
    }
    internal void CleanUp()
    {
        LogTreeIndexState("Cleaning up");
        categoryIndex.CleanUp();
        treeIndex.CleanUp();
        folderIndex.CleanUp();
        LogTreeIndexState("Clean up finished");
    }
    public void ClearCache()
    {
        foreach (OrigamFile origamFile in OrigamFiles)
        {
            origamFile.ClearCache();
        }
    }
    public void Clear()
    {
        LogTreeIndexState("Clearing ItemTracker");
        fileHashIndex.Clear();
        objectLocationIndex.Clear();
        categoryIndex.Clear();
        treeIndex.Clear();
        folderIndex.Clear();
        LogTreeIndexState("ItemTracker cleared");
    }
    public void AddOrReplaceHash(ITrackeableFile origamFile)
    {
        fileHashIndex.AddOrReplace(origamFile);
    }
    public void AddOrReplace(ITrackeableFile origamFile)
    {
        foreach (var entry in origamFile.ContainedObjects)
        {
            PersistedObjectInfo objectInfo = entry.Value;
            AddOrReplace(objectInfo);
        }
    }
    public void AddOrReplace(PersistedObjectInfo objectInfo)
    {
        LogTreeIndexState("Adding: " + objectInfo.Id + "objectInfo.ParentId: " + objectInfo.ParentId);
        objectLocationIndex[objectInfo.Id] = objectInfo;
        treeIndex.AddOrReplace(objectInfo.ParentId, objectInfo);
        categoryIndex.AddOrReplace(objectInfo.Category, objectInfo);
        if (objectInfo.ParentId.Equals(Guid.Empty))
        {
            foreach (var item in objectInfo.OrigamFile.ParentFolderIds)
            {
                string pathAndId = item.Key + item.Value;
                folderIndex.AddOrReplace(pathAndId, objectInfo);
            }
        }
        LogTreeIndexState("Added: " + objectInfo.Id);
    }
    public void Remove(PersistedObjectInfo objectInfo)
    {
        LogTreeIndexState("Removing: " + objectInfo.Id);
        objectLocationIndex.Remove(objectInfo.Id);
        treeIndex.Remove(objectInfo.Id);
        categoryIndex.Remove(objectInfo.Id);
        folderIndex.Remove(objectInfo.Id);
        LogTreeIndexState("Removed: " + objectInfo.Id);
    }
    private void LogTreeIndexState(string message)
    {
        if (!log.IsDebugEnabled) return;
        log.Debug(message + ", treeIndex.Count: " + treeIndex?.Count);
    }
    public bool ContainsFile(FileInfo file) =>
        fileHashIndex.ContainsFile(file);
    public PersistedObjectInfo GetById(Guid id)
    {
        return objectLocationIndex.ContainsKey(id) ? objectLocationIndex[id] : null;
    }
    public IEnumerable<PersistedObjectInfo> GetByParentId(Guid parentId) =>
        treeIndex[parentId];
    public IEnumerable<PersistedObjectInfo> GetByParentFolder
        (string category, Guid folderId)
    {
        string key = $"{category}{folderId}";
        return folderIndex[key];
    }
    internal IEnumerable<PersistedObjectInfo> GetByPackage(Guid packageId)
    {
        return OrigamFiles
            .Where(x => x.ParentFolderIds.PackageId == packageId)
            .SelectMany(x => x.ContainedObjects.Values);
    }
    public IEnumerable<PersistedObjectInfo> GetListByCategory(string category)
    {
        return categoryIndex[category];
    }
    public void KeepOnly(IEnumerable<FileInfo> filesToKeep)
    {
        LogTreeIndexState("KeepOnly method running");
        HashSet<string> relativePathsToKeep = new HashSet<string>(filesToKeep
            .Select(fileInfo => pathFactory.Create(fileInfo).Relative));
        fileHashIndex.RemoveValuesWhere(origamFile =>
            !relativePathsToKeep.Contains(origamFile.Path.Relative));
        objectLocationIndex.RemoveByValueSelector(objInfo =>
            !relativePathsToKeep.Contains(objInfo.OrigamFile.Path.Relative));
        categoryIndex.KeepOnlyItemsOnPaths(relativePathsToKeep);
        treeIndex.KeepOnlyItemsOnPaths(relativePathsToKeep);
        folderIndex.KeepOnlyItemsOnPaths(relativePathsToKeep);
        LogTreeIndexState("KeepOnly method finished");
    }
    public bool HasFile(string relativePath)
    {
        return AllFiles
            .Any(orFile => orFile.Path.Relative == relativePath);
    }
    public void RenameDirectory(DirectoryInfo dirToRename, string newDirPath)
    {
        AllFiles.ToList()
            .Where(origamFile => dirToRename.IsOnPathOf(origamFile.Path.Directory))
            .ForEach(origamFile =>
            {
                RemoveHash(origamFile);
                Remove(origamFile);
                origamFile.Path = origamFile.Path.UpdateToNew(dirToRename, newDirPath);
                AddOrReplace(origamFile);
                AddOrReplaceHash(origamFile);
            });
    }
    public void RemoveHash(ITrackeableFile origamFile)
    {
        fileHashIndex.Remove(origamFile);
    }
    public void Remove(ITrackeableFile origamFile)
    {
        LogTreeIndexState("Removing file: " + origamFile.Path.Relative);
        bool removeFilter(PersistedObjectInfo objInfo) =>
            objInfo.OrigamFile == origamFile;
        objectLocationIndex.RemoveByValueSelector(removeFilter);
        categoryIndex.RemoveWhere(removeFilter);
        treeIndex.RemoveWhere(removeFilter);
        folderIndex.RemoveWhere(removeFilter);
        LogTreeIndexState("Removed file: " + origamFile.Path.Relative);
    }
    public Dictionary<string, int> GetStats()
    {
        return new Dictionary<string, int>
        {
            {"fileHashIndex count", fileHashIndex.Count},
            {"objectLocationIndex count", objectLocationIndex.Count},
            {"elementNameIndex count", categoryIndex.Count},
            {"treeIndex count", treeIndex.Count},
            {"folderIndex count", folderIndex.Count}
        };
    }
    public string Print()
    {
        //return treeIndex.Print();
        return folderIndex.Print();
    }
    public OrigamFile GetByPath(string relativeFilePath)
    {
        return OrigamFiles
            .FirstOrDefault(file => file.Path.RelativeEquals(relativeFilePath));
    }
    public Maybe<ExternalFile> GetExternalFile(FileInfo externalFile)
    {
        return OrigamFiles
            .Select(x => x.GetExternalFile(externalFile))
            .FirstOrDefault(maybeFile => maybeFile.HasValue);
    }
    public string GetFileHash(FileInfo file)
    {
        return fileHashIndex.GetHash(file.FullName);
    }
    public IEnumerable<FileInfo> GetByDirectory(DirectoryInfo dir)
    {
        return AllFiles
            //.Where(origamFile => new FileInfo(origamFile.Path.Absolute).IsOnPathOf(dir))
            .Where(origamFile => dir.IsOnPathOf(origamFile.Path.Absolute))
            .SelectMany(file =>
                {
                    if (file is OrigamFile origamFile)
                    {
                        return origamFile.ExternalFiles.Append(new FileInfo(origamFile.Path.Absolute));
                    }
                    return new[] {new FileInfo(file.Path.Absolute)};
                }
            );
    }
}
internal class FileHashIndex
{
    private readonly IDictionary<string,string> hashFileDict =
        new Dictionary<string, string>();
    private readonly IDictionary<string, ITrackeableFile> pathDict =
        new Dictionary<string, ITrackeableFile>();
    private IDictionary<string, OrigamFile> packageFiles =
        new Dictionary<string, OrigamFile>();
    public ICollection<ITrackeableFile> AllFiles => pathDict.Values;
    public IEnumerable<OrigamFile> OrigamFiles => AllFiles.OfType<OrigamFile>();
    public IEnumerable<OrigamFile> PackageFiles => packageFiles.Values;
    public int Count => hashFileDict.Count;
   
    public void AddOrReplace(ITrackeableFile newTrackAble)
    {
        if (newTrackAble.FileHash == null) return;
        pathDict[newTrackAble.Path.Relative] = newTrackAble;
        hashFileDict[newTrackAble.Path.Absolute.ToLower()] =  newTrackAble.FileHash;
        if (OrigamFile.IsPackageFile(newTrackAble.Path))
        {
            packageFiles[newTrackAble.Path.Relative] = (OrigamFile)newTrackAble;
        }
        System.Diagnostics.Debug.Assert(pathDict.Count == hashFileDict.Count);
    }
    public bool ContainsFile(FileInfo file)
    {
        if (!hashFileDict.ContainsKey(file.FullName.ToLower())) return false;
        string registeredHash = hashFileDict[file.FullName.ToLower()];
        return registeredHash == file.GetFileBase64Hash();
    }
    internal void RemoveValuesWhere(Func<ITrackeableFile, bool> func)
    {
        List<KeyValuePair<string, ITrackeableFile>> removedPairs =
            pathDict.RemoveByValueSelector(func);
        
        foreach (KeyValuePair<string, ITrackeableFile> keyValuePair in removedPairs)
        {
            ITrackeableFile origamFileToRemove = keyValuePair.Value;
            hashFileDict.Remove(origamFileToRemove.Path.Absolute.ToLower());
            packageFiles.Remove(origamFileToRemove.Path.Relative);
        }
    }
    public void Clear()
    {
        hashFileDict.Clear();
        pathDict.Clear();
        packageFiles.Clear();
    }
    public void Remove(ITrackeableFile orFileToRemove)
    {
        hashFileDict.RemoveByKeySelector(fullPath =>
            fullPath == orFileToRemove.Path.Absolute.ToLower());
        pathDict.RemoveByKeySelector(relativePath =>
            relativePath == orFileToRemove.Path.Relative);
        packageFiles.Remove(orFileToRemove.Path.Relative);
        System.Diagnostics.Debug.Assert(pathDict.Count == hashFileDict.Count);
    }
    public string GetHash(string pathAbsolute)
    {
        hashFileDict.TryGetValue(pathAbsolute.ToLower(), out string hash);
        return hash;
    }
}

[ProtoContract]
public class AutoIncrementedIntIndex<TValue>
{
    [ProtoMember(2)]
    private int highestId;
    [ProtoMember(1)]
    public IDictionary<int, TValue> IdToValue { get; } 
        = new Dictionary<int, TValue>();
    public IDictionary<TValue, int> ValueToId { get; } =
        new Dictionary<TValue, int>();
    public AutoIncrementedIntIndex()
    {
    }
    public int AddValueAndGetId(TValue category)
    {
        if (ValueToId.ContainsKey(category))
        {
            return ValueToId[category];
        } 
        else
        {
            highestId++;
            ValueToId.Add(category, highestId);
            IdToValue.Add(highestId, category);
            return highestId;
        }
    }
    public TValue this[int id] => IdToValue[id];
    public int this[TValue value] => ValueToId[value];
    public override string ToString()
    {
       return "AutoIncrementedIntIndex:\n" +
                  "highestId: " + highestId + "\n" +
                  "IdToValue: " + IdToValue.Print()+
                  "ValueToId: " + ValueToId.Print();
    }
}
internal class ObjectInfoIndex<T>
{
    private readonly IDictionary<T, IDictionary<Guid, PersistedObjectInfo>>
        objectInfoIndex = new Dictionary<T, IDictionary<Guid, PersistedObjectInfo>>();
    private readonly IDictionary<Guid,T> guidIndex= new Dictionary<Guid, T>();
    public IEnumerable<PersistedObjectInfo> this[T key]=>
         objectInfoIndex.ContainsKey(key) ?
                objectInfoIndex[key].Values :
                new List<PersistedObjectInfo>();
    
    public int Count => objectInfoIndex.Count(x => x.Value.Count!=0);
    
    public void AddOrReplace(T key, PersistedObjectInfo objInfo)
    {
        RemoveObjInfoIfPresent(objInfo.Id);
        var idToObjInfoDictionary = GetIndexDict(key);
        idToObjInfoDictionary[objInfo.Id] = objInfo;
        guidIndex[objInfo.Id] = key;
    }
    public void Remove(Guid id)
    {
        RemoveObjInfoIfPresent(id);
        guidIndex.Remove(id);
    }
    private void RemoveObjInfoIfPresent(Guid id)
    {
        if (guidIndex.ContainsKey(id))
        {
            T oldKey = guidIndex[id];
            objectInfoIndex[oldKey].Remove(id);
        }
    }
    public void CleanUp()
    {
        objectInfoIndex
            .Where(x=> objectInfoIndex[x.Key].Count == 0)
            .ToList()
            .ForEach(x=>objectInfoIndex.Remove(x));
    }
    public void RemoveWhere(Func<PersistedObjectInfo, bool> removeFilter)
    {
        foreach (var tDictPair in objectInfoIndex)
        {
            var innerDict = tDictPair.Value;
            RemoveObjInfosWhere(innerDict, removeFilter);
        }
        objectInfoIndex.RemoveByValueSelector(innerDict => innerDict.Count == 0);
    }
    public void KeepOnlyItemsOnPaths(IEnumerable<string> relativePathsToKeep)
    {
        foreach (var keyInnerDictPair in objectInfoIndex)
        {
            var innerDictionary = keyInnerDictPair.Value;
            RemoveObjInfosWhere(innerDictionary, objInfo =>
                !relativePathsToKeep.Contains(objInfo.OrigamFile.Path.Relative));
        }
    }
    private void RemoveObjInfosWhere(IDictionary<Guid, PersistedObjectInfo> dict, 
        Func<PersistedObjectInfo, bool> selectorFunc)
    {
        List<KeyValuePair<Guid, PersistedObjectInfo>> pairsToRemove = dict
            .Where(entry => selectorFunc.Invoke(entry.Value))
            .ToList();
        foreach (var pair in pairsToRemove)
        {
            dict.Remove(pair);
            var objInfoId = pair.Value.Id;
            guidIndex.Remove(objInfoId);
        }
    }
    private IDictionary<Guid, PersistedObjectInfo> GetIndexDict(T key)
    {
        if (!objectInfoIndex.ContainsKey(key))
        {
            var list = new Dictionary<Guid, PersistedObjectInfo>();
            objectInfoIndex.Add(key, list);
            return list;
        }
        return objectInfoIndex[key]; 
    }
    public void Clear()
    {
        objectInfoIndex.Clear();
        guidIndex.Clear();
    }
    public string Print()
    {
        string str = "";
        var sortedKeys = objectInfoIndex.Keys.ToList();
        sortedKeys.Sort();
        foreach (var key in sortedKeys)
        {
            if (objectInfoIndex[key].Count == 0)
            {
                continue;
            }
            str += key + ": " + objectInfoIndex[key].Print(inLine:true)+"|\n";
        }
        return str;
    }
}
