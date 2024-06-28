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
#if ORIGAM_CLIENT
public class SchemaItemCollection : OrigamCollectionBase<AbstractSchemaItem>,
    IDisposable
#else
public class SchemaItemCollection : OrigamCollectionBase<Key>, IDisposable
#endif
{
    private bool _clearing = false;
    private Hashtable _nonPersistedItems;
    private IPersistenceProvider _persistence;
    private ISchemaItemProvider _rootProvider;
    private bool _disposing = false;
    private AbstractSchemaItem _parentSchemaItem = null;

    public SchemaItemCollection()
    {
    }

    public SchemaItemCollection(IPersistenceProvider persistence)
    {
        _persistence = persistence;
    }

    public SchemaItemCollection(IPersistenceProvider persistence,
        ISchemaItemProvider provider, AbstractSchemaItem parentItem)
    {
        _persistence = persistence ??
                       throw new ArgumentOutOfRangeException(
                           nameof(persistence),
                           "Persistence cannot be null.");
        _rootProvider = provider;
        ParentSchemaItem = parentItem;
    }

    public SchemaItemCollection(SchemaItemCollection value)
    {
        AddRange(value);
    }

    public SchemaItemCollection(AbstractSchemaItem[] value)
    {
        AddRange(value);
    }

    public AbstractSchemaItem this[int index]
    {
        get
        {
#if ORIGAM_CLIENT
            return base[index];
#else
            return GetItem(base[index]);
#endif
        }
        set
        {
#if ORIGAM_CLIENT
            base[index] = value;
#else
            base[index] = value.PrimaryKey;
#endif
        }
    }

    public void Add(AbstractSchemaItem value)
    {
#if ORIGAM_CLIENT
        base.Add(value);
        if (value.IsAbstract)
        {
            SetDerivedFrom(value);
        }
#else
        if (!value.IsPersisted || value.IsAbstract || !value.UseObjectCache)
        {
            if (value.IsAbstract)
            {
                SetDerivedFrom(value);
            }

            _nonPersistedItems ??= new Hashtable();
            _nonPersistedItems.Add(value.PrimaryKey, value);
        }

        base.Add(value.PrimaryKey);
#endif
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
#if ORIGAM_CLIENT
        return base.Contains(value);
#else
        return base.Contains(value.PrimaryKey);
#endif
    }


    public void CopyTo(AbstractSchemaItem[] array, int index)
    {
#if ORIGAM_CLIENT
        base.CopyTo(array, index);
#else
        var keys = new Key[array.Length];
        for (int i = 0; i < Count; i++)
        {
            keys[i] = array[i].PrimaryKey;
        }

        base.CopyTo(keys, index);
#endif
    }

    public int IndexOf(AbstractSchemaItem value)
    {
#if ORIGAM_CLIENT
        return base.IndexOf(value);
#else
        return base.IndexOf(value.PrimaryKey);
#endif
    }

    public void Insert(int index, AbstractSchemaItem value)
    {
#if ORIGAM_CLIENT
        base.Insert(index, value);
#else
        base.Insert(index, value.PrimaryKey);
#endif
    }

    public new DataEntityItemEnumerator GetEnumerator()
    {
        return new DataEntityItemEnumerator(this);
    }

    public void Remove(AbstractSchemaItem value)
    {
#if ORIGAM_CLIENT
        base.Remove(value);
#else
        base.Remove(value.PrimaryKey);
#endif
    }

    public bool RemoveDeletedItems { get; set; } = true;

    public bool DeleteItemsOnClear { get; set; } = true;

    public bool UpdateParentItem { get; set; } = true;

#if ORIGAM_CLIENT
    protected override void OnSet(int index, AbstractSchemaItem oldValue,
        AbstractSchemaItem newValue)
    {
        var oldItem = oldValue;
        var newItem = newValue;
#else
    protected override void OnSet(int index, Key oldValue, Key newValue)
    {
        var oldItem = GetItem(oldValue);
        var newItem = GetItem(newValue);
#endif
        if (UpdateParentItem)
        {
            newItem.ParentItem = ParentSchemaItem;
            oldItem.ParentItem = null;
        }
#if !ORIGAM_CLIENT
        oldItem.Deleted -= SchemaItem_Deleted;
        newItem.Deleted += SchemaItem_Deleted;
#endif
    }

#if ORIGAM_CLIENT
    protected override void OnInsert(int index, AbstractSchemaItem value)
    {
        var item = value;
        if (item.IsAbstract)
        {
            SetDerivedFrom(item);
        }
#else
    protected override void OnInsert(int index, Key value)
    {
        var item = GetItem(value as Key);
#endif
        if (UpdateParentItem)
        {
            item.ParentItem = ParentSchemaItem;
        }
#if !ORIGAM_CLIENT
        item.Deleted += SchemaItem_Deleted;
#endif
    }

    protected override void OnClear()
    {
        if (!_disposing)
        {
            _clearing = true;
#if ORIGAM_CLIENT
            foreach (AbstractSchemaItem item in this)
            {
#else
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
#endif
                if (DeleteItemsOnClear)
                {
                    item.IsDeleted = true;
                }
#if !ORIGAM_CLIENT
                item.Deleted -= SchemaItem_Deleted;
#endif
            }

            if (_nonPersistedItems != null)
            {
                _nonPersistedItems.Clear();
                _nonPersistedItems = null;
            }

            _clearing = false;
        }
    }

#if ORIGAM_CLIENT
    protected override void OnRemove(int index, AbstractSchemaItem value)
    {
        var item = value;
#else
    protected override void OnRemove(int index, Key value)
    {
        AbstractSchemaItem item = null;
        try
        {
            item = GetItem(value as Key);
        }
        catch (ArgumentOutOfRangeException)
        {
            // in case the item was deleted we will get an exception
            // but that is ok
        }

        if (_nonPersistedItems != null && _nonPersistedItems.Contains(value))
        {
            _nonPersistedItems.Remove(value);
        }
#endif
        if (item != null)
        {
            item.ParentItem = null;
#if !ORIGAM_CLIENT
            item.Deleted -= SchemaItem_Deleted;
#endif
        }
    }

    public class DataEntityItemEnumerator : object, IEnumerator
    {
        private IEnumerator baseEnumerator;
#if ! ORIGAM_CLIENT
        private SchemaItemCollection _collection;
#endif

        private IEnumerable temp;

        public DataEntityItemEnumerator(SchemaItemCollection mappings)
        {
#if ! ORIGAM_CLIENT
            _collection = mappings;
#endif
            temp = mappings;
            baseEnumerator = temp.GetEnumerator();
        }

        public AbstractSchemaItem Current
        {
            get
            {
#if ORIGAM_CLIENT
                return ((AbstractSchemaItem)(baseEnumerator.Current));
#else
                Key key = ((Key)(baseEnumerator.Current));
                return _collection.GetItem(key);
#endif
            }
        }

        object IEnumerator.Current
        {
            get
            {
#if ORIGAM_CLIENT
                return baseEnumerator.Current;
#else
                Key key = ((Key)(baseEnumerator.Current));
                return _collection.GetItem(key);
#endif
            }
        }

        public bool MoveNext()
        {
            return baseEnumerator.MoveNext();
        }

        bool IEnumerator.MoveNext()
        {
            return baseEnumerator.MoveNext();
        }

        public void Reset()
        {
            baseEnumerator.Reset();
        }

        void IEnumerator.Reset()
        {
            baseEnumerator.Reset();
        }
    }

    public AbstractSchemaItem ParentSchemaItem
    {
        get => _parentSchemaItem;
        set => _parentSchemaItem = value;
    }

#if !ORIGAM_CLIENT
    internal AbstractSchemaItem GetItem(Key key)
    {
        if (_nonPersistedItems != null &&
            _nonPersistedItems.ContainsKey(key))
        {
            return _nonPersistedItems[key] as AbstractSchemaItem;
        }

        var item =
            _persistence.RetrieveInstance(typeof(AbstractSchemaItem), key,
                true, false) as AbstractSchemaItem;
        if (item == null)
        {
            if (_nonPersistedItems != null &&
                _nonPersistedItems.ContainsKey(key))
            {
                item = _nonPersistedItems[key] as AbstractSchemaItem;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "Item not found by primary key");
            }
        }
        else
        {
            if (_nonPersistedItems != null &&
                _nonPersistedItems.ContainsKey(key))
            {
                _nonPersistedItems.Remove(key);
            }
        }

        SetDerivedFrom(item);
        item.RootProvider = _rootProvider;
        return item;
    }
#endif

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
        if (!_clearing)
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
        _disposing = true;
        Clear();
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