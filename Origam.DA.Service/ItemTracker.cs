using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;
using Origam.DA.Service;
using Origam.Extensions;
using MoreLinq;
using ProtoBuf;

namespace Origam.DA.Service
{
    public class ItemTracker
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FileHashIndex fileHashIndex =
            new FileHashIndex();
        private readonly IDictionary<Guid, PersistedObjectInfo> objectLocationIndex = 
            new Dictionary<Guid, PersistedObjectInfo>();  
        private readonly ObjectInfoIndex<ElementName> elementNameIndex = 
            new ObjectInfoIndex<ElementName>();
        private readonly ObjectInfoIndex<Guid> treeIndex = 
            new ObjectInfoIndex<Guid>();
        private readonly ObjectInfoIndex<string> folderIndex = 
            new ObjectInfoIndex<string>();
        private readonly OrigamPathFactory pathFactory;

        public ICollection<OrigamFile> OrigamFiles => fileHashIndex.Files;

        public ItemTracker(OrigamPathFactory pathFactory)
        {
            this.pathFactory = pathFactory;
        }

        internal void CleanUp()
        {
            elementNameIndex.CleanUp();
            treeIndex.CleanUp();
            folderIndex.CleanUp();
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
            fileHashIndex.Clear();
            objectLocationIndex.Clear();
            elementNameIndex.Clear();
            treeIndex.Clear();
            folderIndex.Clear();
        }

        public void AddOrReplaceHash(OrigamFile origamFile)
        {
            fileHashIndex.AddOrReplace(origamFile);
        }

        public void AddOrReplace(OrigamFile origamFile)
        {
            foreach (var entry in origamFile.ContainedObjects)
            {   
                PersistedObjectInfo objectInfo = entry.Value;
                AddOrReplace(objectInfo);
            }
        }

        public void AddOrReplace(PersistedObjectInfo objectInfo)
        {
            objectLocationIndex[objectInfo.Id] = objectInfo;
            treeIndex.AddOrReplace(objectInfo.ParentId, objectInfo);
            elementNameIndex.AddOrReplace(objectInfo.ElementName, objectInfo);
            if (objectInfo.ParentId.Equals(Guid.Empty))
            {
                foreach (var item in objectInfo.OrigamFile.ParentFolderIds)
                {
                    string pathAndId = item.Key + item.Value;
                    folderIndex.AddOrReplace(pathAndId, objectInfo);
                }
            }
        }
        
        public void Remove(PersistedObjectInfo objectInfo)    
        {
            objectLocationIndex.Remove(objectInfo.Id);
            treeIndex.Remove(objectInfo.Id);
            elementNameIndex.Remove(objectInfo.Id);
            folderIndex.Remove(objectInfo.Id);
        }

        public bool ContainsFile(FileInfo file) => 
            fileHashIndex.ContainsFile(file);

        public PersistedObjectInfo GetById(Guid id)
        {
            return objectLocationIndex.ContainsKey(id) ?
                objectLocationIndex[id] : null;
        }

        public IEnumerable<PersistedObjectInfo> GetByParentId(Guid parentId) =>
            treeIndex[parentId];

        public IEnumerable<PersistedObjectInfo> GetByParentFolder
            (ElementName elementName, Guid folderId)
        {
            string key = $"{elementName}{folderId}";
            return folderIndex[key];
        }

        internal IEnumerable<PersistedObjectInfo> GetByPackage(Guid packageId)
        {
            return OrigamFiles
                .Where(x => x.ParentFolderIds.PackageId == packageId)
                .SelectMany(x => x.ContainedObjects.Values);
        }

        public IEnumerable<PersistedObjectInfo> GetListByElementName(
            ElementName elementName)
        {
            return elementNameIndex[elementName];
        }

        public void KeepOnly(IEnumerable<FileInfo> filesToKeep)
        {
            HashSet<string> relativePathsToKeep = filesToKeep   
                .Select(fileInfo=> pathFactory.Create(fileInfo).Relative)
                .ToHashSet();

            fileHashIndex.RemoveValuesWhere(origamFile =>
                !relativePathsToKeep.Contains(origamFile.Path.Relative));
            objectLocationIndex.RemoveByValueSelector(objInfo =>    
                !relativePathsToKeep.Contains(objInfo.OrigamFile.Path.Relative));
            
            elementNameIndex.KeepOnlyItemsOnPaths(relativePathsToKeep);
            treeIndex.KeepOnlyItemsOnPaths(relativePathsToKeep);
            folderIndex.KeepOnlyItemsOnPaths(relativePathsToKeep);
        }

        public bool HasFile(string relativePath)
        {
            return OrigamFiles
                .Any(orFile => orFile.Path.Relative == relativePath);
        }

        public void RenameDirectory(DirectoryInfo dirToRename, string newDirPath)
        {
            OrigamFiles.ToList()   
                .Where(origamFile => dirToRename.IsOnPathOf(origamFile.Path.Directory))
                .ForEach(origamFile =>
                {
                    RemoveHash(origamFile);
                    Remove(origamFile);
                    origamFile.Path = origamFile.Path.UpdateToNew(dirToRename, newDirPath);
                    AddOrReplace(origamFile);
                });
        }

        public void RemoveHash(OrigamFile origamFile)
        {
            fileHashIndex.Remove(origamFile);
        }

        public void Remove(OrigamFile origamFile)
        {
            bool removeFilter(PersistedObjectInfo objInfo) => 
                objInfo.OrigamFile == origamFile;
            
            objectLocationIndex.RemoveByValueSelector(removeFilter);
            elementNameIndex.RemoveWhere(removeFilter);
            treeIndex.RemoveWhere(removeFilter);
            folderIndex.RemoveWhere(removeFilter);
        }
        
        public Dictionary<string, int> GetStats()
        {
            return new Dictionary<string, int>
            {
                {"fileHashIndex count", fileHashIndex.Count},
                {"objectLocationIndex count" , objectLocationIndex.Count},
                {"elementNameIndex count" , elementNameIndex.Count},
                {"treeIndex count" , treeIndex.Count},
                {"folderIndex count" , folderIndex.Count}
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
    }
    
    internal class FileHashIndex
    {
        private readonly IDictionary<string,string> hashFileDict =
            new Dictionary<string, string>();
        private readonly IDictionary<string,OrigamFile> pathDict =
            new Dictionary<string,OrigamFile>();

        public ICollection<OrigamFile> Files => pathDict.Values;
        public int Count => hashFileDict.Count;
       
        public void AddOrReplace(OrigamFile newOrFile)
        {
            if (newOrFile.FileHash == null) return;
            pathDict[newOrFile.Path.Relative] = newOrFile;
            hashFileDict[newOrFile.Path.Absolute.ToLower()] =  newOrFile.FileHash;
            System.Diagnostics.Debug.Assert(pathDict.Count == hashFileDict.Count);
        }

        public bool ContainsFile(FileInfo file)
        {
            if (!hashFileDict.ContainsKey(file.FullName.ToLower())) return false;

            string registeredHash = hashFileDict[file.FullName.ToLower()];
            return registeredHash == file.GetFileBase64Hash();
        }

        internal void RemoveValuesWhere(Func<OrigamFile, bool> func)
        {
            List<KeyValuePair<string, OrigamFile>> removedPairs =
                pathDict.RemoveByValueSelector(func);
            
            foreach (KeyValuePair<string, OrigamFile> keyValuePair in removedPairs)
            {
                OrigamFile origamFileToRemove = keyValuePair.Value;
                hashFileDict.Remove(origamFileToRemove.Path.Absolute.ToLower());  
            }
        }

        public void Clear()
        {
            hashFileDict.Clear();
            pathDict.Clear();
        }

        public void Remove(OrigamFile orFileToRemove)
        {
            hashFileDict.RemoveByKeySelector(fullPath =>
                fullPath == orFileToRemove.Path.Absolute.ToLower());
            pathDict.RemoveByKeySelector(relativePath =>
                relativePath == orFileToRemove.Path.Relative);
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

        public int AddValueAndGetId(TValue elementName)
        {
            if (ValueToId.ContainsKey(elementName))
            {
                return ValueToId[elementName];
            } 
            else
            {
                highestId++;
                ValueToId.Add(elementName, highestId);
                IdToValue.Add(highestId, elementName);
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
                str += key + ": " + objectInfoIndex[key].Print(inLine:true)+"|\n";
            }
            return str;
        }
    }
}