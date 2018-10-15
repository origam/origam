using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class ParentFolders: IDictionary<ElementName, Guid>
    {
        private readonly Dictionary<ElementName, Guid> innerDict =
            new Dictionary<ElementName, Guid>();

        private static void CheckIsValid(
            IDictionary<ElementName, Guid> parentFolderIds, OrigamPath parentOrigamFilePath)
        {
            if (parentOrigamFilePath.Relative.EndsWith(OrigamFile.PackageFileName))
            {
                return;
            }
            CheckIsValid(parentFolderIds);
        }

        private static void CheckIsValid( IDictionary<ElementName, Guid> parentFolderIds)
        {
            if(parentFolderIds[OrigamFile.PackageNameUri].Equals(Guid.Empty))
                throw new Exception("Package id cannot be empty");
            if (parentFolderIds.Count != 2)
                throw new Exception("Wrong ParentFolderId number: "+parentFolderIds.Count);
        }

        public Guid PackageId
        {
            get => this[OrigamFile.PackageNameUri];
            set => this[OrigamFile.PackageNameUri] = value;
        }

        public Guid GroupId
        {
            get => this[OrigamFile.GroupNameUri];
            set => this[OrigamFile.GroupNameUri] = value;
        }

        public void CopyTo(ParentFolders other)
        {
            other.AddRange(this);
        }

        public ParentFolders()
        {
        }

        public ParentFolders(IDictionary<ElementName,Guid> parentIdDict,
            OrigamPath patentFilePath)
        {
            CheckIsValid(parentIdDict,patentFilePath);
            this.AddRange(parentIdDict);
        }

        public ParentFolders(IList<ElementName> defaultFolders)
        {
            if (defaultFolders == null || defaultFolders.Count == 0)
            {
                throw  new ArgumentException(nameof(defaultFolders)+" cannot be null or empty.");
            }
            foreach (var item in defaultFolders)
            {
                Add(item, Guid.Empty);
            }
        }
        
        public void CheckIsValid()
        {
            CheckIsValid(this);
        }

        public void CheckIsValid(OrigamPath parentOrigamFilePath)
        {
            CheckIsValid(this, parentOrigamFilePath);
        }

        public bool CointainsNoEmptyIds => 
            this.All(item => !item.Value.Equals(Guid.Empty));

        public IEnumerator<KeyValuePair<ElementName, Guid>> GetEnumerator()
        {
            return innerDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<ElementName, Guid> item)
        {
            innerDict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            innerDict.Clear();
        }

        public bool Contains(KeyValuePair<ElementName, Guid> item)
        {
            return innerDict.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<ElementName, Guid>[] array, int arrayIndex)
        {
            foreach (var keyValuePair in array)
            {
                innerDict.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public bool Remove(KeyValuePair<ElementName, Guid> item)
        {
            return innerDict.Remove(item.Key);
        }

        public int Count => innerDict.Count;
        public bool IsReadOnly => false;
        public bool ContainsKey(ElementName key)
        {
            return innerDict.ContainsKey(key);
        }

        public void Add(ElementName key, Guid value)
        {
            innerDict.Add(key, value);
        }

        public bool Remove(ElementName key)
        {
            return innerDict.Remove(key);
        }

        public bool TryGetValue(ElementName key, out Guid value)
        {
            return innerDict.TryGetValue(key, out value);
        }

        public Guid this[ElementName key]
        {
            get => innerDict[key];
            set => innerDict[key] = value;
        }

        public ICollection<ElementName> Keys => innerDict.Keys;
        public ICollection<Guid> Values => innerDict.Values;
    }
}