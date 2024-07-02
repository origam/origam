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

namespace Origam.Schema.ItemCollection;

// T is supposed to be Key or AbstractSchemItem
[Serializable]
public abstract class SchemaItemCollectionBase<T> : IList<T>, IDisposable
{
    private List<T> list;
    
    protected bool disposing;
    protected bool clearing;
    
    public bool DeleteItemsOnClear { get; set; } = true;
    public bool RemoveDeletedItems { get; set; } = true;
    public bool UpdateParentItem { get; set; } = true;
    
    public AbstractSchemaItem ParentSchemaItem { get; set;}

    public SchemaItemCollectionBase()
    {
        list = new List<T>();
    }

    public SchemaItemCollectionBase(int capacity)
    {
        list = new List<T>(capacity);
    }
        
    protected void SetDerivedFrom(AbstractSchemaItem item)
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

    public void Clear()
    {
        OnClear();
        InnerList.Clear();
        OnClearComplete();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return InnerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    protected virtual void OnClear()
    {
    }

    protected virtual void OnClearComplete() // Redundant
    {
    }

    protected virtual void OnInsert(int index, T value)
    {
    }

    protected virtual void OnInsertComplete(int index, T value) // Redundant
    {
    }

    protected virtual void OnRemove(int index, T value)
    {
    }

    protected virtual void OnRemoveComplete(int index, T value) // Redundant
    {
    }

    protected virtual void OnSet(int index, T oldValue, T newValue)
    {
    }

    protected virtual void OnSetComplete(int index, T oldValue, T newValue) // Redundant
    {
    }

    protected virtual void OnValidate(T value) // Redundant
    {
    }

    public void RemoveAt(int index)
    {
        if ((index < 0) || (index >= InnerList.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                "ArgumentOutOfRange_Index");
        }

        T value = InnerList[index];
        OnValidate(value);
        OnRemove(index, value);
        InnerList.RemoveAt(index);
        try
        {
            OnRemoveComplete(index, value);
        }
        catch
        {
            InnerList.Insert(index, value);
            throw;
        }
    }

    public virtual void Add(T value)
    {
        OnValidate(value);
        OnInsert(InnerList.Count, value);
        int index = InnerList.Count;
        InnerList.Add(value);
        try
        {
            OnInsertComplete(index, value);
        }
        catch
        {
            InnerList.RemoveAt(index);
            throw;
        }
    }

    public abstract void AddRange(IEnumerable<AbstractSchemaItem> value);
    
    public bool Contains(T value)
    {
        return InnerList.Contains(value);
    }

    public int IndexOf(T value)
    {
        return InnerList.IndexOf(value);
    }

    public void Insert(int index, T value)
    {
        if ((index < 0) || (index > InnerList.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                "ArgumentOutOfRange_Index");
        }

        OnValidate(value);
        OnInsert(index, value);
        InnerList.Insert(index, value);
        try
        {
            OnInsertComplete(index, value);
        }
        catch
        {
            InnerList.RemoveAt(index);
            throw;
        }
    }

    public bool Remove(T value)
    {
        OnValidate(value);
        int index = InnerList.IndexOf(value);
        if (index < 0)
        {
            return false;
        }

        OnRemove(index, value);
        InnerList.RemoveAt(index);
        try
        {
            OnRemoveComplete(index, value);
        }
        catch
        {
            InnerList.Insert(index, value);
            throw;
        }

        return true;
    }

    public int Capacity
    {
        get => InnerList.Capacity;
        set => InnerList.Capacity = value;
    }

    public int Count => list?.Count ?? 0;

    protected List<T> InnerList
    {
        get { return list ??= new List<T>(); }
    }

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            if ((index < 0) || (index >= InnerList.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    "ArgumentOutOfRange_Index");
            }

            return InnerList[index];
        }
        set
        {
            if ((index < 0) || (index >= InnerList.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    "ArgumentOutOfRange_Index");
            }

            OnValidate(value);
            T oldValue = InnerList[index];
            OnSet(index, oldValue, value);
            InnerList[index] = value;
            try
            {
                OnSetComplete(index, oldValue, value);
            }
            catch
            {
                InnerList[index] = oldValue;
                throw;
            }
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        InnerList.CopyTo(array, arrayIndex);
    }
    
    public void Dispose()
    {
        disposing = true;
        Clear();
    }
}