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
using System.Linq;
using Origam.Extensions;

namespace Origam.DA.Service;

public class ParentFolders : IDictionary<string, Guid>
{
    private readonly Dictionary<string, Guid> innerDict = new Dictionary<string, Guid>();

    private static void CheckIsValid(
        IDictionary<string, Guid> parentFolderIds,
        OrigamPath parentOrigamFilePath
    )
    {
        if (parentOrigamFilePath.Relative.EndsWith(OrigamFile.PackageFileName))
        {
            return;
        }
        CheckIsValid(parentFolderIds);
    }

    private static void CheckIsValid(IDictionary<string, Guid> parentFolderIds)
    {
        if (parentFolderIds[OrigamFile.PackageCategory].Equals(Guid.Empty))
            throw new Exception("Package id cannot be empty");
        if (parentFolderIds.Count != 2)
            throw new Exception("Wrong ParentFolderId number: " + parentFolderIds.Count);
    }

    public Guid PackageId
    {
        get => this[OrigamFile.PackageCategory];
        set => this[OrigamFile.PackageCategory] = value;
    }
    public Guid GroupId
    {
        get => this[OrigamFile.GroupCategory];
        set => this[OrigamFile.GroupCategory] = value;
    }

    public void CopyTo(ParentFolders other)
    {
        other.AddOrReplaceRange(this);
    }

    public ParentFolders() { }

    public ParentFolders(IDictionary<string, Guid> parentIdDict, OrigamPath patentFilePath)
    {
        CheckIsValid(parentIdDict, patentFilePath);
        this.AddOrReplaceRange(parentIdDict);
    }

    public ParentFolders(IList<string> defaultFolders)
    {
        if (defaultFolders == null || defaultFolders.Count == 0)
        {
            throw new ArgumentException(nameof(defaultFolders) + " cannot be null or empty.");
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

    public bool CointainsNoEmptyIds => this.All(item => !item.Value.Equals(Guid.Empty));

    public IEnumerator<KeyValuePair<string, Guid>> GetEnumerator()
    {
        return innerDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<string, Guid> item)
    {
        if (innerDict.Count == 2 && !innerDict.ContainsKey(item.Key))
            throw new Exception("Max. Parent folder number is 2 ");
        innerDict.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        innerDict.Clear();
    }

    public bool Contains(KeyValuePair<string, Guid> item)
    {
        return innerDict.ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<string, Guid>[] array, int arrayIndex)
    {
        foreach (var keyValuePair in array)
        {
            innerDict.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }

    public bool Remove(KeyValuePair<string, Guid> item)
    {
        return innerDict.Remove(item.Key);
    }

    public int Count => innerDict.Count;
    public bool IsReadOnly => false;

    public bool ContainsKey(string key)
    {
        return innerDict.ContainsKey(key);
    }

    public void Add(string key, Guid value)
    {
        if (innerDict.Count == 2 && !innerDict.ContainsKey(key))
            throw new Exception("Max. Parent folder number is 2 ");
        innerDict.Add(key, value);
    }

    public bool Remove(string key)
    {
        return innerDict.Remove(key);
    }

    public bool TryGetValue(string key, out Guid value)
    {
        return innerDict.TryGetValue(key, out value);
    }

    public Guid this[string key]
    {
        get => innerDict[key];
        set
        {
            if (innerDict.Count == 2 && !innerDict.ContainsKey(key))
                throw new Exception("Max. Parent folder number is 2 ");
            innerDict[key] = value;
        }
    }
    public ICollection<string> Keys => innerDict.Keys;
    public ICollection<Guid> Values => innerDict.Values;
}
