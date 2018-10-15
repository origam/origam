// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// ConfigurationManagementCache.cs
//
// Cache management for the application block.
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

using Microsoft.ApplicationBlocks.ConfigurationManagement.DataProtection;
 
namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
    #region ConfigurationManagementCache Class

    /// <summary>
	/// This class provides cache services to the Configuration Manager.
	/// </summary>
	internal class ConfigurationManagementCache
	{	
        #region Declare Variables

        private string _sectionName;
        private ICacheStorage _storage;
        private string _refresh;

        #endregion
		        
        #region Constructors
        /// <summary>
        /// Constructor allowing the cache configuration to be set.
        /// </summary>
        /// <param name="sectionName">The section to be cached</param>
        /// <param name="cacheSettings">The settings for the cache</param>
        public ConfigurationManagementCache( string sectionName, ConfigCacheSettings cacheSettings )
        {
            _refresh = cacheSettings.Refresh;
            _storage = ActivateCacheStorage();

			this._sectionName = sectionName;
        }
        #endregion

		#region Indexer

        /// <summary>
        /// Class indexer
        /// </summary>
        public object this [string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Add(key, value);
            }
        }


		#endregion

		#region Properties

        /// <summary>
        /// Absolute time format for refresh of the config data cache. 
        /// </summary>
        public string Refresh
        {
            get
            {
                return _refresh;
            }
        }
        


        /// <summary>
        /// This property specifies the section name associated with this cache
        /// </summary>
        public string SectionName
        {
            get
            {
                return _sectionName;
            }
        }


		#endregion

        /// <summary>
		/// Puts an item in the cache
		/// </summary>
		private void Add( string key, object item )
		{	
			if( key == null )
				throw new ArgumentNullException( "key", Resource.ResourceManager[ "RES_ExceptionInvalidKeyValue" ] );

			if( item == null )
				throw new ArgumentNullException( "item", Resource.ResourceManager[ "RES_ExceptionInvalidCacheElement" ] );

			_storage.Add( key, new CacheValue( item, DateTime.Now ) );
    	}
		

		/// <summary>
		/// Gets the item with the specific key
		/// </summary>
        private CacheValue Get( string key )
		{	
			CacheValue		returnValue		= null;

			returnValue = _storage.Get( key );

			if ( ( returnValue == null ) || true == ExtendedFormatHelper.IsExtendedExpired(_refresh, returnValue.ItemAge, DateTime.Now ) )
			{
				// item has expired from cache
				// use WeakReference for cache items to respond better to memory pressure.  
				// The cache is freed on memory pressure.
				return null;
			}
			else
			{
				return returnValue;
			}           
		}
		

        /// <summary>
        /// Determines whether the cache contains the specific key
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _storage.ContainsKey(key);
        }


        /// <summary>
        /// Removes all elements from the cache
        /// </summary>
        public void Clear()
        {
			if( _storage != null )
				_storage.Clear();
        }


        /// <summary>
        /// Returns a CacheStorage instance
        /// </summary>
        /// <returns>Instance of a specific CacheStorage implementation.</returns>
        private ICacheStorage ActivateCacheStorage()
        {
            return new MemoryCacheStorage();
        }



}
    #endregion
}
