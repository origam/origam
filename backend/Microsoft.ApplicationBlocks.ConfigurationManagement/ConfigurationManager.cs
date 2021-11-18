// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// ConfigurationManager.cs
//
// Public class and entry point for the CMAB.
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
using System.Configuration;
using System.Collections;
using System.Xml;
 
using Microsoft.ApplicationBlocks.ConfigurationManagement.Storage;
using Microsoft.ApplicationBlocks.ConfigurationManagement.DataProtection;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
    #region Configuration Manager class
    /// <summary>
	/// The Configuration Manager class manages the configuration information based on settings in the configuration file.
	/// </summary>
    public sealed class ConfigurationManager
    {
        #region Declare variables
        private const string				DEFAULTSECTION_NAME			= "Default";
		private static string				_defaultSectionName			= null;
		private static bool					_isInitialized				= false;
		private static Exception			_initException				= null;
        #endregion

		#region Constructors

        // Do not allow this class to be instantiated
        static ConfigurationManager()
        {
			try
			{
				InitAllProviders();
				_isInitialized = true;
			}
			catch( Exception e )
			{
				_initException = e;
			}
        }
				
		private ConfigurationManager(){}

		#endregion

		#region Public methods

		#region Initialize
		/// <summary>
		/// Initializes the configuration management support
		/// </summary>
		public static void Initialize()
		{
			// Check the static initialization result
			if( !_isInitialized )
			{
				throw _initException;
			}
		}
		#endregion
		
		#region Read

		/// <summary>
		/// Returns the section defined as the DefaultSection on the configuration
		/// file or the first section defined.
		/// </summary>
		/// <returns></returns>
		public static Hashtable Read()
		{
			// Check the static initialization result
			if( !_isInitialized )
				throw _initException;

			object section = Read( _defaultSectionName );
			if( section == null )
				return null;
			if( !( section is Hashtable ) )
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionConfigurationManagerDefaultSectionIsNotHashtable" ] );
			return (Hashtable)section;
		}

        /// <summary>
        /// Returns a single specified value
        /// </summary>
		public static object Read( string sectionName )
		{
			ConfigurationManagementCache			cacheSettings			= null;
			CacheValue								cachedValue				= null;
			IConfigurationStorageReader				configReader			= null;
			XmlNode									configSectionNode		= null;
			IConfigurationSectionHandler			customSectionHandler	= null;
			object									icshValue				= null;

			// Check the static initialization result
			if( !_isInitialized )
				throw _initException;

			// Validate the section name
			if( sectionName == null || sectionName.Length == 0 )
				throw new ArgumentNullException( "sectionName", Resource.ResourceManager[ "RES_ExceptionConfigurationManagerInvalidSectionName"] );

			if( sectionName.IndexOf( "/" ) != -1 )
                throw new NotSupportedException( Resource.ResourceManager[ "RES_ExceptionSectionGroupsNotSupported"] );
 
            if( !IsValidSection( sectionName ) )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionSectionInvalid", sectionName ] );

            // get a cacheSettings object from cachefactory, so we know what the cache settings are
			cacheSettings = CacheFactory.Create( sectionName );
            
			//  get cache wrapper object CacheValue from cachemanager--IF cache not null (i.e. config says "don't cache"...as in case of very lively data)
			if ( cacheSettings != null )
			{
				cachedValue = (CacheValue)cacheSettings[ sectionName ];
			}

			// If the value was cached return the value
			if( cachedValue != null )
			{
				//  return its contents, cast to "object" (since we don't know what it is)
				return (object)(cachedValue.Value);
			}
   
            // Create an instance of the storage reader for the given section
			configReader = StorageReaderFactory.Create( sectionName );

			// No provider for the requested section
			if( configReader == null )
				throw new Exception( 
					Resource.ResourceManager[ "RES_ExceptionStorageProviderException",
					sectionName ] );

			//  here we're actually using the storageReader to read an XmlNode.
			//  then feed that node to its associated IConfigurationSectionHandler implementation, which decodes the node 
			//  and returns an object of some type...here represented by icshValue.
			configSectionNode = null;
			try
			{
				configSectionNode = configReader.Read();
			}
			catch( Exception providerException )
			{
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionStorageProviderException",
					sectionName ], 
					providerException );
			}

			// The storage provider returns null
			// OK to return null here, they may just not have written the section yet.
			if( configSectionNode == null )
				return null;

			// Use the Factory to get our custom ICSH instance
            customSectionHandler = ConfigSectionHandlerFactory.Create( sectionName );

			// Using the ICSH _itself_ as a Factory, create our icshValue from it...in other words,
			// the ICSH is taking this XmlNode and morphing it to an object instance.
			icshValue = customSectionHandler.Create( null, null, configSectionNode );
				
			// Update the cache.
			if( cacheSettings != null )
			{
				// The plain value is stored into the cache.
				cacheSettings[ cacheSettings.SectionName ] = icshValue;
			}
            
			return icshValue;
		}

		#endregion
        
		#region Write

		/// <summary>
		/// Writes the default section (used with the Setting property), to the 
		/// storage provider.
		/// </summary>
		/// <remarks>This method uses the same instance returned by the Setting 
		/// property. If the Settings class is modified this method must be 
		/// called, otherwise the changes are not saved.</remarks>
		public static void Write( Hashtable value )
		{
			// Check the static initialization result
			if( !_isInitialized )
				throw _initException;

			if( value == null )
				throw new ArgumentNullException( "value" );

			Write( _defaultSectionName, value );
		}

        /// <summary>
        /// Writes a single value using the specified section
        /// </summary>
		public static void Write(string sectionName, object configValue)
		{
			IConfigurationStorageWriter			configStorageWriter				= null;
			IConfigurationStorageReader			configStorageReader				= null;
			IConfigurationSectionHandler		configSectionHandler			= null;
			IConfigurationSectionHandlerWriter	configSectionHandlerWriter		= null;
			XmlNode								xmlNode							= null;
			ConfigurationManagementCache		sectionCache					= null;

			// Check the static initialization result
			if( !_isInitialized )
				throw _initException;

			//  Validate the section name
			if( sectionName == null || sectionName.Length == 0 )
				throw new ArgumentNullException( "sectionName", Resource.ResourceManager[ "RES_ExceptionConfigurationManagerInvalidSectionName" ] );
			
            if( sectionName.IndexOf( "/" ) != -1 )
                throw new NotSupportedException( Resource.ResourceManager[ "RES_ExceptionSectionGroupsNotSupported"] );

            if( !IsValidSection( sectionName ) )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionSectionInvalid", sectionName ] );

			//  get the storage READER from factory...then we will query its type to see if it can WRITE too
			configStorageReader = StorageReaderFactory.Create( sectionName );

			//  If the storage is not a wirter throw an exception because we can't Write()
			if ( ! ( configStorageReader is IConfigurationStorageWriter ) )	
			{
				throw new ConfigurationException( 
					Resource.ResourceManager["RES_ExceptionHandlerNotWritable", 
					sectionName ] );
			}

			//  put Writer cast of the Reader into the configStorageWriter local object...yes, we could just cast the original back onto itself
			//  but it's clearer here to have two distinct names
			configStorageWriter = (IConfigurationStorageWriter)configStorageReader;

			// get the section handler
			configSectionHandler = ConfigSectionHandlerFactory.Create( sectionName );

			//  If the section handler is not a writer throw an exception
			if( !( configSectionHandler is IConfigurationSectionHandlerWriter ) )
			{
				throw new ConfigurationException( 
					Resource.ResourceManager["RES_ExceptionHandlerNotWritable", 
					sectionName ] );
			}
			
			//  cast the ICSH instance to our custom ICSHWriter type
			configSectionHandlerWriter = (IConfigurationSectionHandlerWriter)configSectionHandler;

			//  Just as ICSH takes XML passed to it and creates an object essentially like XML serialization, 
			//  the ICSHWriter we define does the reverse--turns an object into XML.
			//  The IConfigurationSectionHandlerWriter implementation of course is free to use just that--the built-in .NET xml serialization, 
			//  and the Quickstarts show this.
			xmlNode = configSectionHandlerWriter.Serialize( configValue );

			// writes the configValue using the storage provider
			try
			{
			  	//  we have the node. Now WRITE this node to the storage location using an instance of
				//  IConfigurationStorageWriter
				configStorageWriter.Write( xmlNode );
			}
			catch( Exception storageProviderException )
			{
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionStorageProviderException",
					sectionName ], 
					storageProviderException );
			}

			sectionCache = CacheFactory.Create( sectionName );
            
            // Update the cache. 
			if( sectionCache != null )
			{
				// Store plain configValue on the cache
				sectionCache[ sectionCache.SectionName ] = configValue;
			}
		}

		#endregion

		#region IsReadOnly
        /// <summary>
        /// Determines whether the section is readonly or not
        /// </summary>
        public static bool IsReadOnly(string sectionName)
        {
			// Check the static initialization result
			if( !_isInitialized )
				throw _initException;

			// Validate the section name
			if( sectionName == null || sectionName.Length == 0 )
				throw new ArgumentNullException( "sectionName", 
					Resource.ResourceManager["RES_ExceptionConfigurationManagerInvalidSectionName" ] );
			
			if  (  !StorageReaderFactory.ContainsKey( sectionName )  ) // The section doesn´t exist
               throw new ConfigurationException(
					Resource.ResourceManager["RES_ExceptionSectionInvalid", sectionName ] ); 
            
			// create an instance of provider for specified section name.
            IConfigurationStorageReader configProvider = StorageReaderFactory.Create( sectionName );
			
			// create an instance of the section handler
			IConfigurationSectionHandler configSection = ConfigSectionHandlerFactory.Create( sectionName );

			//  ask the specified storageProvider if it's IConfigurationStorageWriter
			if( ( configProvider is IConfigurationStorageWriter ) && 
				( configSection is IConfigurationSectionHandlerWriter ) )
			{
				return false;
			}
			else
			{
				return true;
			}
        }
		#endregion

		#endregion

		#region Private/Internal Methods & Event Handlers

        /// <summary>
		/// Method added to initialize all the providers.
		/// </summary>
		/// 
		private static bool InitAllProviders()
		{
            ConfigurationManagementSettings			configMgmtSet		= null;
			ConfigSectionSettings					sectionSettings		= null;

			//  get the configuration settings object that wraps all our config info
			configMgmtSet = ConfigurationManagementSettings.Instance;
			
            try
			{				
				_defaultSectionName = configMgmtSet.DefaultSectionName;

				//  have to deal with DictionaryEntry here
                foreach( DictionaryEntry de in configMgmtSet.Sections)
                {
					sectionSettings = (ConfigSectionSettings)de.Value;

					// Set the default section
					if( _defaultSectionName == null || _defaultSectionName.Trim().Length == 0 )
					{
						_defaultSectionName = sectionSettings.Name;
					}

					//  use Factory class to make a storageReader; 
					//  NOTE that we only demand it be a Reader at this point, that is sufficient to initialize it.
					//  IF at a later point we wish to use a Writer, _we will query the object to see if it can be a Writer_
					StorageReaderFactory.Create( sectionSettings.Name );

                    //  call into cachefactory so that (if valid) a cache object is created for this section name...
					//  remember the configMgmtCache objects are just encapsulations of the cache settings for a given config section
					CacheFactory.Create( sectionSettings.Name );
                }
                return true;
			}
			catch( Exception e )
			{
				// On any error loading the providers the cache list is cleaned.
				StorageReaderFactory.ClearCache();
				CacheFactory.ClearCache();
				DataProtectionFactory.ClearCache();

                throw new ConfigurationException( 
					Resource.ResourceManager["RES_ExceptionProviderInit", 
					sectionSettings.Name , 
					sectionSettings.Provider.TypeName , 
					sectionSettings.Provider.AssemblyName ], e);
					//"Creating Storage Provider and Cache for section '{0}' where storage type = '{1}' and assembly = '{2}' "
			}
		}

        /// <summary>
        /// ConfigChanges Event Handler
        /// </summary>
        internal static void OnConfigChanges( IConfigurationStorageReader storageProvider, string sectionName ) // : ConfigurationChanged
        {
			CacheFactory.ClearCache( sectionName );
        }

		internal static bool IsValidSection( string sectionName )
		{
			ConfigurationManagementSettings			configMgmtSet		= null;

			//  get the configuration settings object that wraps all our config info
			configMgmtSet = ConfigurationManagementSettings.Instance;
            
            if( configMgmtSet == null )
                throw new ConfigurationException( Resource.ResourceManager["RES_ExceptionLoadingConfiguration"] );
            
			return configMgmtSet.Sections[ sectionName ] != null;
		}
	

		#endregion

		#region Singleton

		/// <summary>
		/// The connection manager singleton instance.
		/// </summary>
		public static ConfigurationManager Items
		{
			get 
			{
				// Check the static initialization result
				if( !_isInitialized )
					throw _initException;

				//  check if singleton exists yet, if not make it
				if ( _singleton == null )
				{
					_singleton = new ConfigurationManager();
				}
				return _singleton;
			}
		}  private static ConfigurationManager _singleton;

		#endregion

		#region Item-Instance 

		/// <summary>
		/// Indexer used to get he hashtable instance when the default section
		/// returns a hashtable.
		/// </summary>
		public object this[ string key ]
		{
			get
			{
				Hashtable section = ConfigurationManager.Read();
				if( section == null ) return null;
				return section[ key ];
			}

			set
			{
				Hashtable htSection = ConfigurationManager.Read();
				htSection[ key ] = value;
				ConfigurationManager.Write( htSection );
			}
		}

		#endregion
	}
    #endregion
} 
