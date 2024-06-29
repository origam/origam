using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema;

class ArchitectSchemaItemCollection: CheckedList<Key>, IDisposable,
    ISchemaItemCollection
{
    private Hashtable nonPersistedItems = new ();
    private readonly IPersistenceProvider persistence;
    private readonly ISchemaItemProvider rootProvider;
    
    private bool disposing;
    private bool clearing;
    public bool DeleteItemsOnClear { get; set; } = true;
    public bool RemoveDeletedItems { get; set; } = true;
    public bool UpdateParentItem { get; set; } = true;
    public AbstractSchemaItem ParentSchemaItem { get; set;}
    
    public ArchitectSchemaItemCollection()
    {
    }
    public ArchitectSchemaItemCollection(IPersistenceProvider persistence,
        ISchemaItemProvider rootProvider, AbstractSchemaItem parentItem)
    {
        this.persistence = persistence;
        this.rootProvider = rootProvider;
        ParentSchemaItem = parentItem;
    }

    public IEnumerator<AbstractSchemaItem> GetEnumerator()
    {
        return new SchemaItemEnumerator(base.InnerList, GetItem);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(AbstractSchemaItem item)
    {
        if (!item.IsPersisted || item.IsAbstract || !item.UseObjectCache)
        {
            if (item.IsAbstract)
            {
                SetDerivedFrom(item);
            }

            nonPersistedItems ??= new Hashtable();
            nonPersistedItems.Add(item.PrimaryKey, item);
        }

        base.Add(item.PrimaryKey);
    }

    public void Clear()
    {
        base.Clear();
    }

    public bool Contains(AbstractSchemaItem item)
    {
        return base.Contains(item.PrimaryKey);
    }

    public void CopyTo(AbstractSchemaItem[] array, int index)
    {
        var keysToCopy = array.Select(x => x.PrimaryKey).ToArray();
        base.CopyTo(keysToCopy, index);
    }

    public bool Remove(AbstractSchemaItem item)
    {
        return base.Remove(item.PrimaryKey);
    }

    public int Count => base.Count;
    public bool IsReadOnly { get; } = false;
    public int IndexOf(AbstractSchemaItem item)
    {
        return base.IndexOf(item.PrimaryKey);
    }

    public void Insert(int index, AbstractSchemaItem item)
    {
        base.Insert(index, item.PrimaryKey);
    }

    public void RemoveAt(int index)
    {
        base.RemoveAt(index);
    }

    public AbstractSchemaItem this[int index]
    {
        get => GetItem(base[index]);
        set => base[index] = value.PrimaryKey;
    }
    
    private AbstractSchemaItem GetItem(Key key)
    {
        if (nonPersistedItems != null &&
            nonPersistedItems.ContainsKey(key))
        {
            return nonPersistedItems[key] as AbstractSchemaItem;
        }

        var item =
            persistence.RetrieveInstance(typeof(AbstractSchemaItem), key,
                true, false) as AbstractSchemaItem;
        if (item == null)
        {
            if (nonPersistedItems != null &&
                nonPersistedItems.ContainsKey(key))
            {
                item = nonPersistedItems[key] as AbstractSchemaItem;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "Item not found by primary key");
            }
        }
        else
        {
            if (nonPersistedItems != null &&
                nonPersistedItems.ContainsKey(key))
            {
                nonPersistedItems.Remove(key);
            }
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
                AbstractSchemaItem item;
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
        AbstractSchemaItem item = null;
        try
        {
            item = GetItem(value);
        }
        catch (ArgumentOutOfRangeException)
        {
            // in case the item was deleted we will get an exception
            // but that is ok
        }

        if (nonPersistedItems != null && nonPersistedItems.Contains(value))
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
    
    private void SetDerivedFrom(AbstractSchemaItem item)
    {
        if (item.ParentItem != null)
        {
            // If we assign derived items, we mark them
            if (!item.ParentItem.PrimaryKey.Equals(ParentSchemaItem
                    .PrimaryKey))
            {
                item.DerivedFrom = item.ParentItem;
                item.ParentItem = ParentSchemaItem;
            }
        }
    }
    
    private void SchemaItem_Deleted(object sender, EventArgs e)
    {
        if (!clearing)
        {
            var si = sender as AbstractSchemaItem;
            if (RemoveDeletedItems && Contains(si))
            {
                Remove(si);
            }
        }
    }
    public void Dispose()
    {
        disposing = true;
        Clear();
    }
}

class SchemaItemEnumerator : IEnumerator<AbstractSchemaItem>
{
    private readonly Func<Key, AbstractSchemaItem> keyToItem;
    private readonly IEnumerator<Key> keyEnumerator;
    public SchemaItemEnumerator(IList<Key> keys, Func<Key, AbstractSchemaItem> keyToItem)
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

    public AbstractSchemaItem Current {
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