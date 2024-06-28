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

namespace Origam.Schema;

[Serializable]
public abstract class OrigamCollectionBase<T> : IList<T>, ICollection<T>
{
    // Fields
    private List<T> list;

    // Methods
    protected OrigamCollectionBase()
    {
        this.list = new List<T>();
    }

    protected OrigamCollectionBase(int capacity)
    {
        this.list = new List<T>(capacity);
    }

    public void Clear()
    {
        this.OnClear();
        this.InnerList.Clear();
        this.OnClearComplete();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this.InnerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    protected virtual void OnClear() { }

    protected virtual void OnClearComplete() { }

    protected virtual void OnInsert(int index, T value) { }

    protected virtual void OnInsertComplete(int index, T value) { }

    protected virtual void OnRemove(int index, T value) { }

    protected virtual void OnRemoveComplete(int index, T value) { }

    protected virtual void OnSet(int index, T oldValue, T newValue) { }

    protected virtual void OnSetComplete(int index, T oldValue, T newValue) { }

    protected virtual void OnValidate(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
    }

    public void RemoveAt(int index)
    {
        if ((index < 0) || (index >= this.InnerList.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
        }
        
        T value = this.InnerList[index];
        this.OnValidate(value);
        this.OnRemove(index, value);
        this.InnerList.RemoveAt(index);
        try
        {
            this.OnRemoveComplete(index, value);
        }
        catch
        {
            this.InnerList.Insert(index, value);
            throw;
        }
    }

    public void Add(T value)
    {
        this.OnValidate(value);
        this.OnInsert(this.InnerList.Count, value);
        int index = this.InnerList.Count;
        this.InnerList.Add(value);
        try
        {
            this.OnInsertComplete(index, value);
        }
        catch
        {
            this.InnerList.RemoveAt(index);
            throw;
        }
    }

    public bool Contains(T value)
    {
        return this.InnerList.Contains(value);
    }

    public int IndexOf(T value)
    {
        return this.InnerList.IndexOf(value);
    }

    public void Insert(int index, T value)
    {
        if ((index < 0) || (index > this.InnerList.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
        }
        this.OnValidate(value);
        this.OnInsert(index, value);
        this.InnerList.Insert(index, value);
        try
        {
            this.OnInsertComplete(index, value);
        }
        catch
        {
            this.InnerList.RemoveAt(index);
            throw;
        }
    }

    public bool Remove(T value)
    {
        this.OnValidate(value);
        int index = this.InnerList.IndexOf(value);
        if (index < 0)
        {
            return false;
        }
        this.OnRemove(index, value);
        this.InnerList.RemoveAt(index);
        try
        {
            this.OnRemoveComplete(index, value);
        }
        catch
        {
            this.InnerList.Insert(index, value);
            throw;
        }
        return true;
    }

    // Properties
    public int Capacity
    {
        get { return this.InnerList.Capacity; }
        set { this.InnerList.Capacity = value; }
    }

    public int Count
    {
        get { return this.list?.Count ?? 0; }
    }

    protected List<T> InnerList
    {
        get
        {
            if (this.list == null)
            {
                this.list = new List<T>();
            }
            return this.list;
        }
    }

    bool ICollection<T>.IsReadOnly
    {
        get { return false; }
    }

    public T this[int index]
    {
        get
        {
            if ((index < 0) || (index >= this.InnerList.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
            }
            return this.InnerList[index];
        }
        set
        {
            if ((index < 0) || (index >= this.InnerList.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
            }
            this.OnValidate(value);
            T oldValue = this.InnerList[index];
            this.OnSet(index, oldValue, value);
            this.InnerList[index] = value;
            try
            {
                this.OnSetComplete(index, oldValue, value);
            }
            catch
            {
                this.InnerList[index] = oldValue;
                throw;
            }
        }
    }
    public void CopyTo(T[] array, int arrayIndex)
    {
        this.InnerList.CopyTo(array, arrayIndex);
    }
}

