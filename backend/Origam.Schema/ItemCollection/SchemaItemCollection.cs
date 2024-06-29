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

using System.Collections.Generic;
using System.Linq;
using Origam.DA.ObjectPersistence;
using System;
using System.Collections;

namespace Origam.Schema;

// Refactoring notes
// removing ICollection along with the "void ICollection.CopyTo(Array array, int index)"
// implementation seem to have ony caused the following not to work:
// ArrayList childrenCopy =  new ArrayList(someSchemaItemCollection.ChildItems),
// That is ok because we can replace it with:
// List<AbstractSchemaItem> childrenCopy =  someSchemaItemCollection.ChildItems.ToList(),
//
[Serializable]
public class SchemaItemCollection : ISchemaItemCollection
{
    private readonly ISchemaItemCollection itemList;

    public SchemaItemCollection()
    {
#if ORIGAM_CLIENT
        itemList = new ServerSchemaItemCollection();
#else
        itemList = new ArchitectSchemaItemCollection();
#endif
    }
    public SchemaItemCollection(IPersistenceProvider persistence,
        ISchemaItemProvider provider, AbstractSchemaItem parentItem)
    {
#if ORIGAM_CLIENT
        itemList = new ServerSchemaItemCollection(parentItem);
#else
        itemList =
            new ArchitectSchemaItemCollection(persistence, provider,
                parentItem);
#endif
    }

    public SchemaItemCollection(SchemaItemCollection value)
    {
        AddRange(value);
    }

    public SchemaItemCollection(AbstractSchemaItem[] value)
    {
        AddRange(value);
    }

    public void RemoveAt(int index)
    {
       itemList.RemoveAt(index);
    }

    public AbstractSchemaItem this[int index]
    {
        get => itemList[index];
        set => itemList[index] = value;
    }

    public void Add(AbstractSchemaItem value)
    {
        itemList.Add(value);
    }

    public void Clear()
    {
        itemList.Clear();
    }

    public void AddRange(AbstractSchemaItem[] value)
    {
        foreach (var item in value)
        {
            Add(item);
        }
    }

    public void AddRange(SchemaItemCollection value)
    {
        foreach (AbstractSchemaItem item in value)
        {
            Add(item);
        }
    }

    public bool Contains(AbstractSchemaItem value)
    {
        return itemList.Contains(value);
    }

    public void CopyTo(AbstractSchemaItem[] array, int index)
    {
        itemList.CopyTo(array, index);
    }
    
    public int Count => itemList.Count;
    public bool IsReadOnly { get; } = false;

    public int IndexOf(AbstractSchemaItem value)
    {
        return itemList.IndexOf(value);
    }

    public void Insert(int index, AbstractSchemaItem value)
    {
        itemList.Insert(index, value);
    }

    public IEnumerator<AbstractSchemaItem> GetEnumerator()
    {
        return itemList.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return itemList.GetEnumerator();
    }

    public bool Remove(AbstractSchemaItem value)
    {
        return itemList.Remove(value);
    }
    
    public bool RemoveDeletedItems
    {
        get => itemList.RemoveDeletedItems;
        set => itemList.RemoveDeletedItems = value;
    }
    public bool DeleteItemsOnClear
    {
        get => itemList.DeleteItemsOnClear;
        set => itemList.DeleteItemsOnClear = value;
    }
    public bool UpdateParentItem
    {
        get => itemList.UpdateParentItem;
        set => itemList.UpdateParentItem = value;
    }    
    public AbstractSchemaItem ParentSchemaItem
    {
        get => itemList.ParentSchemaItem;
        set => itemList.ParentSchemaItem = value;
    }

    public void Dispose()
    {
        itemList.Dispose();
    }

    public IEnumerable<AbstractSchemaItem> ToGeneric()
    {
        foreach (AbstractSchemaItem item in this)
        {
            yield return item;
        }
    }

    public List<AbstractSchemaItem> ToList()
    {
        return ToGeneric().ToList();
    }
}