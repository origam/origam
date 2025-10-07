#region license

/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.ItemCollection;

class ArchitectISchemaItemCollection : SchemaItemCollectionBase<Key>, ISchemaItemCollection
{
    private Dictionary<Key, ISchemaItem> nonPersistedItems = new();
    private readonly IPersistenceProvider persistence;
    private readonly ISchemaItemProvider rootProvider;

    public ArchitectISchemaItemCollection() { }

    public ArchitectISchemaItemCollection(
        IPersistenceProvider persistence,
        ISchemaItemProvider rootProvider,
        ISchemaItem parentItem
    )
    {
        this.persistence = persistence;
        this.rootProvider = rootProvider;
        ParentSchemaItem = parentItem;
    }

    public new IEnumerator<ISchemaItem> GetEnumerator()
    {
        return new SchemaItemEnumerator(InnerList, GetItem);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ISchemaItem item)
    {
        if (!item.IsPersisted || item.IsAbstract || !item.UseObjectCache)
        {
            if (item.IsAbstract)
            {
                SetDerivedFrom(item);
            }

            nonPersistedItems ??= new Dictionary<Key, ISchemaItem>();
            nonPersistedItems.Add(item.PrimaryKey, item);
        }

        base.Add(item.PrimaryKey);
    }

    public bool Contains(ISchemaItem item)
    {
        return base.Contains(item.PrimaryKey);
    }

    public void CopyTo(ISchemaItem[] array, int index)
    {
        int destinationIndex = index;
        for (int i = 0; i < Count; i++)
        {
            array[destinationIndex] = this[i];
            destinationIndex++;
        }
    }

    public bool Remove(ISchemaItem item)
    {
        return base.Remove(item?.PrimaryKey);
    }

    public int IndexOf(ISchemaItem item)
    {
        return base.IndexOf(item?.PrimaryKey);
    }

    public void Insert(int index, ISchemaItem item)
    {
        base.Insert(index, item?.PrimaryKey);
    }

    public new ISchemaItem this[int index]
    {
        get => GetItem(base[index]);
        set => base[index] = value.PrimaryKey;
    }

    private ISchemaItem GetItem(Key key)
    {
        ISchemaItem item = null;
        nonPersistedItems?.TryGetValue(key, out item);
        if (item != null)
        {
            item.RootProvider = rootProvider;
            return item;
        }

        item = persistence.RetrieveInstance(typeof(ISchemaItem), key, true, false) as ISchemaItem;
        if (item == null)
        {
            nonPersistedItems?.TryGetValue(key, out item);
            if (item == null)
            {
                throw new ArgumentOutOfRangeException("Item not found by primary key");
            }
        }
        else
        {
            nonPersistedItems?.Remove(key);
        }

        SetDerivedFrom(item);
        item.RootProvider = rootProvider;
        return item;
    }

    protected override void OnClear()
    {
        if (!disposing)
        {
            clearing = true;
            foreach (Key key in InnerList)
            {
                ISchemaItem item;
                try
                {
                    item = GetItem(key);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (DeleteItemsOnClear)
                    {
                        throw;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (DeleteItemsOnClear)
                {
                    item.IsDeleted = true;
                }
                item.Deleted -= SchemaItem_Deleted;
            }

            if (nonPersistedItems != null)
            {
                nonPersistedItems.Clear();
                nonPersistedItems = null;
            }

            clearing = false;
        }
    }

    protected override void OnInsert(int index, Key value)
    {
        var item = GetItem(value);
        if (UpdateParentItem)
        {
            item.ParentItem = ParentSchemaItem;
        }
        item.Deleted += SchemaItem_Deleted;
    }

    protected override void OnRemove(int index, Key value)
    {
        ISchemaItem item = null;
        try
        {
            item = GetItem(value);
        }
        catch (ArgumentOutOfRangeException)
        {
            // in case the item was deleted we will get an exception
            // but that is ok
        }

        if (nonPersistedItems != null && nonPersistedItems.ContainsKey(value))
        {
            nonPersistedItems.Remove(value);
        }

        if (item != null)
        {
            item.ParentItem = null;
            item.Deleted -= SchemaItem_Deleted;
        }
    }

    protected override void OnSet(int index, Key oldValue, Key newValue)
    {
        var oldItem = GetItem(oldValue);
        var newItem = GetItem(newValue);
        if (UpdateParentItem)
        {
            newItem.ParentItem = ParentSchemaItem;
            oldItem.ParentItem = null;
        }
        oldItem.Deleted -= SchemaItem_Deleted;
        newItem.Deleted += SchemaItem_Deleted;
    }

    public override void AddRange(IEnumerable<ISchemaItem> other)
    {
        foreach (var item in other)
        {
            Add(item);
        }
    }

    private void SchemaItem_Deleted(object sender, EventArgs e)
    {
        if (!clearing)
        {
            var si = sender as ISchemaItem;
            if (RemoveDeletedItems && Contains(si))
            {
                Remove(si);
            }
        }
    }
}

class SchemaItemEnumerator : IEnumerator<ISchemaItem>
{
    private readonly Func<Key, ISchemaItem> keyToItem;
    private readonly IEnumerator<Key> keyEnumerator;

    public SchemaItemEnumerator(IList<Key> keys, Func<Key, ISchemaItem> keyToItem)
    {
        this.keyToItem = keyToItem;
        keyEnumerator = keys.GetEnumerator();
    }

    public bool MoveNext()
    {
        return keyEnumerator.MoveNext();
    }

    public void Reset()
    {
        keyEnumerator.Reset();
    }

    public ISchemaItem Current
    {
        get
        {
            Key currentKey = keyEnumerator.Current;
            return keyToItem(currentKey);
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        keyEnumerator.Dispose();
    }
}
