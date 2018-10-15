// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// CacheStorage.cs
//
// Cache storage support. This file defines the interface and a sample implementation.
// 
//
// For more information see the Configuration Management Application Block Implementation Overview. 
// 
// ===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ==============================================================================

using System;
using System.Collections;

using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	#region ICacheStorage

    /// <summary>
	/// This interface must be implemented by cache storage providers
	/// </summary>
	internal interface ICacheStorage
	{
		/// <summary>
		/// Adds an item to the cache
		/// </summary>
		/// <param name="key">item key</param>
		/// <param name="item">item value</param>
		void Add(string key, CacheValue item);
        
		/// <summary>
		/// Gets a item from the cache
		/// </summary>
		/// <param name="key">item key</param>
		/// <returns>item value</returns>
		CacheValue Get(string key);
        
		/// <summary>
		/// Determines whether the cache contains the specific key
		/// </summary>
		/// <param name="key">item key</param>
		/// <returns>true if the item exist, otherwise false</returns>
		bool ContainsKey(string key);
        
		/// <summary>
		/// Removes all elements from the cache
		/// </summary>
		void Clear();
	}

	#endregion

	#region Cache Storage classes


	/// <summary>
	/// The cache value used to hold the cache type
	/// </summary>
	internal class CacheValue
	{
		public CacheValue( object value, DateTime itemAge )
		{
			_value = value;
            _itemAge = itemAge;
		}

		public object Value
		{
			get{ return _value; }
		} object _value;

        public DateTime ItemAge
        {
            get{ return _itemAge; }
        } DateTime _itemAge;
	}
	#endregion

	#region MemoryCacheStorage


	/// <summary>
	/// This class implements a cache in memory
	/// </summary>
	internal class MemoryCacheStorage : ICacheStorage
	{
		#region Declare Variables
		private Hashtable items = new Hashtable();
		#endregion

		/// <summary>
		/// Adds an item to the cache
		/// </summary>
		public void Add(string key, CacheValue item)
		{
			lock( items.SyncRoot )
			{
				items[ key ] = item;
			}
		}

		/// <summary>
		/// Gets a item from the cache
		/// </summary>
		public CacheValue Get(string key)
		{
			return (CacheValue)items[key];
		}

		/// <summary>
		/// Determines whether the cache contains the specific key
		/// </summary>
		public bool ContainsKey(string key)
		{
			return items.ContainsKey(key); 
		}

		/// <summary>
		/// Removes all elements from the cache
		/// </summary>
		public void Clear()
		{
			lock( items.SyncRoot )
				items.Clear();
		}
	}
	#endregion
   
}
