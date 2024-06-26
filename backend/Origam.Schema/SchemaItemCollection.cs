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

namespace Origam.Schema;
using System;
using System.Collections;
using Origam.DA.ObjectPersistence;


/// <summary>
///     <para>
///       A collection that stores <see cref='Origam.Schema.AbstractSchemaItem'/> objects.
///    </para>
/// </summary>
/// <seealso cref='Origam.Schema.SchemaItemCollection'/>
[Serializable()]
public class SchemaItemCollection : OrigamCollectionBase, IDisposable, ICollection
{
	bool _clearing = false;
	Hashtable _nonPersistedItems;
	IPersistenceProvider _persistence;
	ISchemaItemProvider _rootProvider;
	#region Constructors
	public SchemaItemCollection() 
	{
	}
	public SchemaItemCollection(IPersistenceProvider persistence) 
	{
		_persistence = persistence;
	}
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Schema.SchemaItemCollection'/>.
	///    </para>
	/// </summary>
	public SchemaItemCollection(IPersistenceProvider persistence, ISchemaItemProvider provider, AbstractSchemaItem parentItem) 
	{
		if(persistence == null)
		{
			throw new ArgumentOutOfRangeException("persistence", persistence, "Persistence cannot be null.");
		}
		_persistence = persistence;
		_rootProvider = provider;
		this.ParentSchemaItem = parentItem;
	}
    
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Schema.SchemaItemCollection'/> based on another <see cref='Origam.Schema.SchemaItemCollection'/>.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///       A <see cref='Origam.Schema.SchemaItemCollection'/> from which the contents are copied
	/// </param>
	public SchemaItemCollection(SchemaItemCollection value) 
	{
		this.AddRange(value);
	}
    
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Schema.SchemaItemCollection'/> containing any array of <see cref='Origam.Schema.AbstractSchemaItem'/> objects.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///       A array of <see cref='Origam.Schema.AbstractSchemaItem'/> objects with which to intialize the collection
	/// </param>
	public SchemaItemCollection(AbstractSchemaItem[] value) 
	{
		this.AddRange(value);
	}
	#endregion
	#region Collection methods
	/// <summary>
	/// <para>Represents the entry at the specified index of the <see cref='Origam.Schema.AbstractSchemaItem'/>.</para>
	/// </summary>
	/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
	/// <value>
	///    <para> The entry at the specified index of the collection.</para>
	/// </value>
	/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
	public AbstractSchemaItem this[int index] 
	{
		get 
		{
#if ORIGAM_CLIENT
			return ((AbstractSchemaItem)(List[index]));
#else
			return this.GetItem(List[index] as Key);
#endif
		}
		set 
		{
#if ORIGAM_CLIENT
			List[index] = value;
#else
			List[index] = value.PrimaryKey;
#endif
		}
	}
	/// <summary>
	///    <para>Adds a <see cref='Origam.Schema.AbstractSchemaItem'/> with the specified value to the 
	///    <see cref='Origam.Schema.SchemaItemCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Schema.AbstractSchemaItem'/> to add.</param>
	/// <returns>
	///    <para>The index at which the new element was inserted.</para>
	/// </returns>
	/// <seealso cref='Origam.Schema.SchemaItemCollection.AddRange'/>
	public int Add(AbstractSchemaItem value) 
	{
#if ORIGAM_CLIENT
		int ret = List.Add(value);
		if(value.IsAbstract)
		{
			SetDerivedFrom(value);
		}
#else
		if(!value.IsPersisted || value.IsAbstract || !value.UseObjectCache)
		{
			if(value.IsAbstract)
			{
				SetDerivedFrom(value);
			}
			if(_nonPersistedItems == null)
			{
				_nonPersistedItems = new Hashtable();
			}
			_nonPersistedItems.Add(value.PrimaryKey, value);
		}
		int ret = List.Add(value.PrimaryKey);
#endif	
		return ret;
	}
    
	/// <summary>
	/// <para>Copies the elements of an array to the end of the <see cref='Origam.Schema.SchemaItemCollection'/>.</para>
	/// </summary>
	/// <param name='value'>
	///    An array of type <see cref='Origam.Schema.AbstractSchemaItem'/> containing the objects to add to the collection.
	/// </param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <seealso cref='Origam.Schema.SchemaItemCollection.Add'/>
	public void AddRange(AbstractSchemaItem[] value) 
	{
		for (int i = 0; (i < value.Length); i = (i + 1)) 
		{
			this.Add(value[i]);
		}
	}
    
	/// <summary>
	///     <para>
	///       Adds the contents of another <see cref='Origam.Schema.SchemaItemCollection'/> to the end of the collection.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///    A <see cref='Origam.Schema.SchemaItemCollection'/> containing the objects to add to the collection.
	/// </param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <seealso cref='Origam.Schema.SchemaItemCollection.Add'/>
	public void AddRange(SchemaItemCollection value) 
	{
		for (int i = 0; (i < value.Count); i = (i + 1)) 
		{
			this.Add(value[i]);
		}
	}
    
	/// <summary>
	/// <para>Gets a value indicating whether the 
	///    <see cref='Origam.Schema.SchemaItemCollection'/> contains the specified <see cref='Origam.Schema.AbstractSchemaItem'/>.</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Schema.AbstractSchemaItem'/> to locate.</param>
	/// <returns>
	/// <para><see langword='true'/> if the <see cref='Origam.Schema.AbstractSchemaItem'/> is contained in the collection; 
	///   otherwise, <see langword='false'/>.</para>
	/// </returns>
	/// <seealso cref='Origam.Schema.SchemaItemCollection.IndexOf'/>
	public bool Contains(AbstractSchemaItem value) 
	{
#if ORIGAM_CLIENT
		return List.Contains(value);
#else
		return List.Contains(value.PrimaryKey);
#endif
	}
    
	/// <summary>
	/// <para>Copies the <see cref='Origam.Schema.SchemaItemCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
	///    specified index.</para>
	/// </summary>
	/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Origam.Schema.SchemaItemCollection'/> .</para></param>
	/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Origam.Schema.SchemaItemCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
	/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
	/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
	/// <seealso cref='System.Array'/>
#if ! ORIGAM_CLIENT
	void ICollection.CopyTo(System.Array array, int index) 
	{
		ArrayList itemArray = new ArrayList();
		for(int i=0; i<this.Count; i++)
		{
			itemArray.Add(this[i]);
		}
		itemArray.CopyTo(array, index);
	}
#endif
	public void CopyTo(AbstractSchemaItem[] array, int index) 
	{
#if ORIGAM_CLIENT
		List.CopyTo(array, index);
#else
		Key[] keys = new Key[array.Length];
		for(int i=0; i<this.Count; i++)
		{
			keys[i] = array[i].PrimaryKey;
		}
		List.CopyTo(keys, index);
#endif
	}
	/// <summary>
	///    <para>Returns the index of a <see cref='Origam.Schema.AbstractSchemaItem'/> in 
	///       the <see cref='Origam.Schema.SchemaItemCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Schema.AbstractSchemaItem'/> to locate.</param>
	/// <returns>
	/// <para>The index of the <see cref='Origam.Schema.AbstractSchemaItem'/> of <paramref name='value'/> in the 
	/// <see cref='Origam.Schema.SchemaItemCollection'/>, if found; otherwise, -1.</para>
	/// </returns>
	/// <seealso cref='Origam.Schema.SchemaItemCollection.Contains'/>
	public int IndexOf(AbstractSchemaItem value) 
	{
#if ORIGAM_CLIENT
		return List.IndexOf(value);
#else
		return List.IndexOf(value.PrimaryKey);
#endif
	}
    
	/// <summary>
	/// <para>Inserts a <see cref='Origam.Schema.AbstractSchemaItem'/> into the <see cref='Origam.Schema.SchemaItemCollection'/> at the specified index.</para>
	/// </summary>
	/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
	/// <param name=' value'>The <see cref='Origam.Schema.AbstractSchemaItem'/> to insert.</param>
	/// <returns><para>None.</para></returns>
	/// <seealso cref='Origam.Schema.SchemaItemCollection.Add'/>
	public void Insert(int index, AbstractSchemaItem value) 
	{
#if ORIGAM_CLIENT
		List.Insert(index, value);
#else
		List.Insert(index, value.PrimaryKey);
#endif
	}
    
	/// <summary>
	///    <para>Returns an enumerator that can iterate through 
	///       the <see cref='Origam.Schema.SchemaItemCollection'/> .</para>
	/// </summary>
	/// <returns><para>None.</para></returns>
	/// <seealso cref='System.Collections.IEnumerator'/>
	public new IDataEntityItemEnumerator GetEnumerator() 
	{
		return new IDataEntityItemEnumerator(this);
	}
    
	/// <summary>
	///    <para> Removes a specific <see cref='Origam.Schema.AbstractSchemaItem'/> from the 
	///    <see cref='Origam.Schema.SchemaItemCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Schema.AbstractSchemaItem'/> to remove from the <see cref='Origam.Schema.SchemaItemCollection'/> .</param>
	/// <returns><para>None.</para></returns>
	/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
	public void Remove(AbstractSchemaItem value) 
	{
#if ORIGAM_CLIENT
		List.Remove(value);
#else
		List.Remove(value.PrimaryKey);
#endif
	}
	#endregion
	#region Properties
	bool _removeDeletedItems = true;
	public bool RemoveDeletedItems 
	{
		get => _removeDeletedItems;
		set => _removeDeletedItems = value;
	}
	bool _deleteItemsOnClear = true;
	public bool DeleteItemsOnClear
	{
		get => _deleteItemsOnClear;
		set => _deleteItemsOnClear = value;
	}
	bool _updateParentItem = true;
	public bool UpdateParentItem
	{
		get => _updateParentItem;
		set => _updateParentItem = value;
	}
	#endregion
	#region Handling methods
	protected override void OnSet(int index, object oldValue, object newValue) 
	{
#if ORIGAM_CLIENT
		AbstractSchemaItem oldItem = oldValue as AbstractSchemaItem;
		AbstractSchemaItem newItem = newValue as AbstractSchemaItem;
#else
		AbstractSchemaItem oldItem = GetItem(oldValue as Key);
		AbstractSchemaItem newItem = GetItem(newValue as Key);
#endif
		if(UpdateParentItem)
		{
			newItem.ParentItem = this.ParentSchemaItem;
			oldItem.ParentItem = null;
		}
#if ! ORIGAM_CLIENT
		oldItem.Deleted -= SchemaItem_Deleted;			
		newItem.Deleted += SchemaItem_Deleted;			
#endif
	}
    
	protected override void OnInsert(int index, object value) 
	{
#if ORIGAM_CLIENT
		AbstractSchemaItem item = value as AbstractSchemaItem;
        if (item.IsAbstract)
        {
            SetDerivedFrom(item);
        }
#else
        AbstractSchemaItem item = GetItem(value as Key);
#endif
		if(UpdateParentItem)
		{
			System.Diagnostics.Debug.Assert(item.IsAbstract == false 
				|| item.ParentItem == null 
				|| item.ParentItem.PrimaryKey.Equals(this.ParentSchemaItem.PrimaryKey));
			item.ParentItem = this.ParentSchemaItem;
		}
#if ! ORIGAM_CLIENT
		item.Deleted += SchemaItem_Deleted;
#endif
	}
    
	protected override void OnClear() 
	{
		if(! _disposing)
		{
			_clearing = true;
#if ORIGAM_CLIENT
			foreach(AbstractSchemaItem item in this.List)
			{
#else
			foreach(Key key in this.List)
			{
                AbstractSchemaItem item;
                try
                {
				    item = GetItem(key);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (this.DeleteItemsOnClear)
                    {
                        throw;
                    }
                    else
                    {
                        continue;
                    }
                }
#endif
				if(this.DeleteItemsOnClear)
				{
					item.IsDeleted = true;
				}
#if ! ORIGAM_CLIENT
				item.Deleted -= SchemaItem_Deleted;
#endif
			}
			if(_nonPersistedItems != null)
			{
				_nonPersistedItems.Clear();
				_nonPersistedItems = null;
			}
			_clearing = false;
		}
	}
    
	protected override void OnRemove(int index, object value) 
	{
#if ORIGAM_CLIENT
		AbstractSchemaItem item = value as AbstractSchemaItem;
#else
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
		if(_nonPersistedItems != null && _nonPersistedItems.Contains(value))
		{
			_nonPersistedItems.Remove(value);
		}
#endif
		if(item != null)
		{
			item.ParentItem = null;
#if ! ORIGAM_CLIENT
		item.Deleted -= SchemaItem_Deleted;			
#endif
		}
	}
    
	protected override void OnValidate(object value) 
	{
	}
	#endregion
	#region Enumerator class
	public class IDataEntityItemEnumerator : object, IEnumerator 
	{
        
		private IEnumerator baseEnumerator;
#if ! ORIGAM_CLIENT
		private SchemaItemCollection _collection;
#endif
        
		private IEnumerable temp;
        
		public IDataEntityItemEnumerator(SchemaItemCollection mappings) 
		{
#if ! ORIGAM_CLIENT
			_collection = mappings;
#endif
			this.temp = mappings;
			this.baseEnumerator = temp.GetEnumerator();
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
	#endregion
	#region Events
	#endregion
	#region Other Methods
	private AbstractSchemaItem _parentSchemaItem = null;
	public AbstractSchemaItem ParentSchemaItem
	{
		get => _parentSchemaItem;
		set => _parentSchemaItem = value;
	}
#if ! ORIGAM_CLIENT
	internal AbstractSchemaItem GetItem(Key key)
	{
		if(_nonPersistedItems != null && _nonPersistedItems.ContainsKey(key))
		{
			return  _nonPersistedItems[key] as AbstractSchemaItem;
		}
		AbstractSchemaItem item = _persistence.RetrieveInstance(typeof(AbstractSchemaItem), key, true, false) as AbstractSchemaItem;
		if(item == null)
		{
			if(_nonPersistedItems != null && _nonPersistedItems.ContainsKey(key))
			{
				item = _nonPersistedItems[key] as AbstractSchemaItem;
			}
			else
			{
				throw new ArgumentOutOfRangeException("Item not found by primary key");
			}
		}
		else
		{
			if(_nonPersistedItems != null && _nonPersistedItems.ContainsKey(key))
			{
				_nonPersistedItems.Remove(key);
			}
		}
		SetDerivedFrom(item);

		// Set the same root provider for all child items
		item.RootProvider = _rootProvider;
		return item;
	}
	private AbstractSchemaItem GetNonPersistedItem(Key key)
	{
		if(_nonPersistedItems == null) return null;
		if(_nonPersistedItems.ContainsKey(key))
		{
			return _nonPersistedItems[key] as AbstractSchemaItem;
		}
		else
		{
			return null;
		}
	}
#endif
	private void SetDerivedFrom(AbstractSchemaItem item)
	{
		if(item.ParentItem != null)
		{
			// If we assign derived items, we mark them
			if(!item.ParentItem.PrimaryKey.Equals(this.ParentSchemaItem.PrimaryKey))
			{
				item.DerivedFrom = item.ParentItem;
				item.ParentItem = this.ParentSchemaItem;
			}
		}
	}
	#endregion
	private void SchemaItem_Deleted(object sender, EventArgs e)
	{
		if(!_clearing)
		{
			AbstractSchemaItem si = sender as AbstractSchemaItem;
			if(this.RemoveDeletedItems && this.Contains(si))
			{
				this.Remove(si);
			}
		}
	}
	#region IDisposable Members
	private bool _disposing = false;
	public void Dispose()
	{
		_disposing = true;
		base.Clear();
	}
	#endregion
	
	public IEnumerable<AbstractSchemaItem> ToGeneric()
	{
		foreach (var item in this)
		{
			 yield return item;
		}
	}
	public List<AbstractSchemaItem> ToList()
	{
		return  ToGeneric().ToList();
	}
}
