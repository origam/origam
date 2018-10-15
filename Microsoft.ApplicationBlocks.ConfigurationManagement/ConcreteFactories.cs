// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// ConcreteFactories.cs
//
// Factory pattern implementation for the visitors used on the block.
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
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Xml;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{

	#region ConfigSectionHandlerFactory

	internal sealed class ConfigSectionHandlerFactory
	{
		#region Declarations

		private static HybridDictionary		_icshCache;
		private static XmlDocument			_xmlAppConfigDoc;

		#endregion

		#region Constructors

		static ConfigSectionHandlerFactory()
		{
			_icshCache = new HybridDictionary(5, true);
		}


		private ConfigSectionHandlerFactory(){}


		#endregion

		#region Private Helper Methods

		private static XmlNode FindSectionNode( XmlDocument xmlDoc, string sectionName )
		{
			XmlNode currentNode = xmlDoc.SelectSingleNode( string.Format( @"/configuration/configSections/section[@name='{0}']", sectionName ) ); 
			return currentNode;
		}

		private static void PutInCache( IConfigurationSectionHandler icshInstance , string sectionName )
		{			
			lock( _icshCache.SyncRoot )
			{
				_icshCache[ sectionName ] = icshInstance;
			}
		}

		#endregion

		#region Create Overloads

		public static IConfigurationSectionHandler Create( string sectionName )
		{
			if( _xmlAppConfigDoc == null )
			{
				lock( typeof(ConfigSectionHandlerFactory) )
				{
					// Load the Application.config file
					if( _xmlAppConfigDoc == null )
					{
						_xmlAppConfigDoc = new XmlDocument();
						_xmlAppConfigDoc.Load( AppDomain.CurrentDomain.SetupInformation.ConfigurationFile );
					}
				}
			}

			IConfigurationSectionHandler icshInstance = null;

			//  try to find the ICSH in the cache
			icshInstance = _icshCache[ sectionName ] as IConfigurationSectionHandler;
			if( icshInstance != null )
				return icshInstance;

			//  look in Application.config file
			XmlNode xmlSectionNode = FindSectionNode( _xmlAppConfigDoc, sectionName );
			
			//  if it s still not found we can not proceed further. Throw an error message.
			//  Without a type definition for the section handler we must stop executing.
			if( xmlSectionNode == null )
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionSectionNotFound", sectionName ] );

			//  get the fully-qualified type name
			string fullTypeName = xmlSectionNode.Attributes[ "type" ].Value;
			if( fullTypeName == null )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionTypeNotSpecified", sectionName ] );

			//  create instance using sister utility class
			icshInstance = GenericFactory.Create( fullTypeName ) as IConfigurationSectionHandler;			
			
			//  the configuration section handler shouldn´t be null
			if ( icshInstance == null )
			{
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidSectionHandler", fullTypeName ] );
			}

			//  remember to cache it
			PutInCache( icshInstance , sectionName );
                
			return icshInstance;	
		}

		#endregion

	}

	#endregion

	#region StorageReaderFactory


	internal sealed class StorageReaderFactory
	{
		#region Declarations

		private static HybridDictionary		_storageCache;

		#endregion

		#region Constructors

		static StorageReaderFactory()
		{
			_storageCache = new HybridDictionary(5, true);

		}


		private StorageReaderFactory(){}


		#endregion

		#region Private Helper Methods

		private static IConfigurationStorageReader GetByCreating( string sectionName )
		{
			ConfigurationManagementSettings configMgmtSet	= null;
			ConfigSectionSettings sectionSettings			= null;
			IConfigurationStorageReader storageReader		= null;

			// Get the configuration settings object that wraps all our config info
			configMgmtSet = ConfigurationManagementSettings.Instance;

			// Get the requested config section by name
			sectionSettings = configMgmtSet[ sectionName ];

			// Create a new provider Instance
			if( sectionSettings.Provider != null )
				// Call the generic factory
				storageReader = (IConfigurationStorageReader)GenericFactory.Create( 
																					sectionSettings.Provider.AssemblyName, 
																					sectionSettings.Provider.TypeName);


			if(storageReader == null)
				throw new ConfigurationException(
					Resource.ResourceManager["RES_ExceptionProviderInvalidType", 
					sectionSettings.Provider.AssemblyName + "," + sectionSettings.Provider.TypeName ] );
                    
			// INITIALIZE the Reader
			InitializeStorage( sectionName, storageReader );

			// Registers to the event on the provider
			storageReader.ConfigChanges += new ConfigurationChanged( ConfigurationManager.OnConfigChanges ) ;

			// Cache the provider
			lock( _storageCache.SyncRoot )
			{
				// Cache Storage Provider
				_storageCache[ sectionSettings.Name ] = storageReader;
			}

			// Return the storage reader instance
			return storageReader;
		}

		
		private static void InitializeStorage( string sectionName , IConfigurationStorageReader reader )
		{
			//  get the config section we need:
			ConfigurationManagementSettings configMngmtSet = ConfigurationManagementSettings.Instance;
			ConfigSectionSettings sectionSettings = (ConfigSectionSettings)configMngmtSet.Sections[ sectionName ];

			//  get the DataProtection object for this section (if there is one)
			IDataProtection dataProtection = DataProtectionFactory.Create( sectionName );

			if( sectionSettings.Provider != null )
			{
				reader.Init( sectionName, sectionSettings.Provider.OtherAttributes, dataProtection );
			}
			else
			{
				reader.Init( sectionName, new ListDictionary (), dataProtection );
			}		
		}
		
		#endregion

		#region Create Overloads


		public static IConfigurationStorageReader Create( string sectionName )
		{
			IConfigurationStorageReader storageReader = null;

			//  first try to get storageReader from cache
			storageReader = _storageCache[ sectionName ] as IConfigurationStorageReader;

			// if storageReader is not null return it, else create, init, cache and return it.
			if ( storageReader != null )
			{
				return storageReader;
			}
			else
			{
				try
				{
					//  create using helper and return
					return GetByCreating( sectionName );
				}
				catch( Exception e )
				{
					throw new ConfigurationException(
						Resource.ResourceManager[ "RES_ExceptionAppMisConfigured", sectionName ], e );
				}
			}

		}



		#endregion

		#region Other Public Methods

		/// <summary>
		/// pass-through to underlying collection object's ContainsKey method
		/// we never want to give direct access to collection so we must forward this call to protect 
		/// the private member's anonymity...and type in case we reoptimize storage later
		/// </summary>
		/// <param name="key">key whose existence we want to query</param>
		/// <returns>boolean true if key exists</returns>
		public static bool ContainsKey( string key )
		{
			return _storageCache.Contains( key );
		}

		
		/// <summary>
		/// Clears the internal cache of all IConfigurationStorageReader objects
		/// </summary>
		public static void ClearCache()
		{
			lock( _storageCache.SyncRoot )
			{
				_storageCache.Clear();
			}
		}


		#endregion

	}


	#endregion
	
	#region DataProtectionFactory


	internal sealed class DataProtectionFactory
	{
		#region Declarations

		private static HybridDictionary						_protectionCache				= null;

		#endregion

		#region Constructors

		static DataProtectionFactory()
		{
			_protectionCache = new HybridDictionary(5, true);
		}


		private DataProtectionFactory(){}


		#endregion

		#region Private Helper Methods

		
		private static IDataProtection GetByCreating( string sectionName )
		{
			IDataProtection						idp					= null;
			string								exceptionDetail		= "";
			ConfigSectionSettings				sectionSettings		= null;
			ConfigurationManagementSettings		configMgmtSet		= null;
			
			//  get ref to main configmgmtsettings instance (the singleton)
			configMgmtSet = ConfigurationManagementSettings.Instance;

			//  Get the requested config section by name
			sectionSettings = (ConfigSectionSettings)configMgmtSet.Sections[ sectionName ];

			// define detailed exception info here so we can use it at both possible throw points below
			exceptionDetail = 
				Resource.ResourceManager[ "RES_ExceptionDetailType" , 
				sectionSettings.Provider.TypeName, 
				sectionSettings.Provider.AssemblyName ]; 

			//  make sure that having found correct Section, the DataProtection entry exists
			if( sectionSettings.DataProtection != null )
			{
				try
				{
					// Instatiate the IDP implementation using Generic factory
					idp = GenericFactory.Create( sectionSettings.DataProtection.AssemblyName, sectionSettings.DataProtection.TypeName ) as IDataProtection;

					//  if the IDP is null, throw an exception. It's not a good thing, we have a misconfiguration and need to stop executing.
					if( idp == null )
						throw new ConfigurationException( 
							Resource.ResourceManager[ "RES_ExceptionProtectionProviderInvalidType", 
							exceptionDetail ] );

					//  NOW initialize the IDataProtection instance...for instance, it might need to know where its key is stored
					idp.Init( sectionSettings.DataProtection.OtherAttributes );
				}
				catch( Exception e )
				{
					throw new ConfigurationException( 
						Resource.ResourceManager[ "RES_ExceptionDataProtectionProviderInit",
						sectionSettings.DataProtection.AssemblyName + "-" + sectionSettings.DataProtection.TypeName,
						exceptionDetail ],
						e);
				}
			}


			//  is IDP null?  even if it is,  put in cache...that way, we will just cache a null value
			//  and avoid looking for one we know doesn't exist each time
			lock( _protectionCache.SyncRoot )
			{
				_protectionCache[ sectionName ] = idp;
			}
			

			//  RETURN the IDataProtection instance (or null if this section isn't configured to have one)
			return idp;
				
		}


		#endregion

		#region Create Overloads
		
		/// <summary>
		/// returns protection provider for default section if one exists 
		/// otherwise throws exception because dataprotection doesn't exist in default section, or default not defined
		/// </summary>
		/// <returns></returns>
		public static IDataProtection Create(  )
		{
			IDataProtection						idp				= null;
			
			//  attempt to get from cache
			idp = _protectionCache[ ConfigurationManagementSettings.Instance.DefaultSectionName ] as IDataProtection;

			if ( idp != null )
			{
				return idp;
			}
			else
			{
				//  get the default config section name, and pass to other overload
				return GetByCreating( ConfigurationManagementSettings.Instance.DefaultSectionName );
			}


		}


		/// <summary>
		/// Creates or loads from internal cache an IDataProtection instance, type-specific to the SectionName passed in
		/// </summary>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		public static IDataProtection Create( string sectionName )
		{
			IDataProtection idp				= null;

			//  attempt to get from cache
			idp = _protectionCache[ sectionName ] as IDataProtection;

			if ( idp != null )
			{
				return idp;
			}
			else
			{
				return GetByCreating( sectionName );
			}
			
		}


		#endregion

		#region Clear
		/// <summary>
		/// Clears the internal cache of all DataProtectionFactory
		/// </summary>
		public static void ClearCache()
		{
			lock( _protectionCache.SyncRoot )
			{
				_protectionCache.Clear();
			}
		}
		#endregion
	}


	#endregion

	#region CacheFactory


	internal sealed class CacheFactory
	{
		#region Declarations

		private static HybridDictionary		_cacheObjectCache;

		#endregion

		#region Constructors

		static CacheFactory()
		{
			_cacheObjectCache = new HybridDictionary(5, true);

		}

		private CacheFactory(){}

		#endregion

		#region Create Overloads

		/// <summary>
		/// creates or retrieves from internal cache a ConfigurationManagementCache object, which is just an
		/// encapsulation of cache settings in the configuration file--such as frequency of refresh, type of cache, location of cache etc.
		/// </summary>
		/// <param name="sectionName">the name of the config section from which we wish to read cache parameters</param>
		/// <returns>ConfigurationManagementCache object which encapsulates cache settings.</returns>
		public static ConfigurationManagementCache Create( string sectionName )
		{
			ConfigurationManagementCache		cacheSettings		= null;
			ConfigurationManagementSettings		configMgmtSet		= null;
			ConfigSectionSettings				sectionSettings		= null;

			
			//  look in our internal dictionary for the cacheSettings object
			cacheSettings = _cacheObjectCache[ sectionName ] as ConfigurationManagementCache;

			//  if it's not null return it otherwise create, cache, and return it;
			if ( cacheSettings != null )
			{
				//  found, return it
				return cacheSettings;
			}
			else
			{
				//  need a fresh one
				//  get config settings object for this sectionname
				//  get the configuration settings object that wraps all our config info
				configMgmtSet = ConfigurationManagementSettings.Instance;

				//  get the requested config section by name
				sectionSettings = configMgmtSet[ sectionName ];
		        
				//  NOW actually check if this section asks for a cache at all; if it does not, just return null
				if ( sectionSettings != null && sectionSettings.Cache != null )
				{
					if ( false != sectionSettings.Cache.IsEnabled )
					{
						//  new the cacheSettings object
						cacheSettings = new ConfigurationManagementCache( sectionSettings.Name, sectionSettings.Cache );

						//  add it to internal cache
						lock ( _cacheObjectCache.SyncRoot )
						{
							_cacheObjectCache[ sectionSettings.Name ] = cacheSettings;
						}
					}
				}

				//  The sectionSettings DOES NOT HAVE CACHE, or it is NOT ENABLED--so just return null
				else
				{
					cacheSettings = null;
				}

				//  return it
				return cacheSettings;
			}
		}


		#endregion

		#region Clear

		/// <summary>
		/// Clears the internal cache of ConfigurationManagementCache objects.  
		/// </summary>
		public static void ClearCache()
		{
			lock( _cacheObjectCache.SyncRoot )
			{
				_cacheObjectCache.Clear();
			}
		}

		//  clears a particular cacheSettings entry based on section name
		public static void ClearCache( string sectionName )
		{
			//  get the particular ConfigurationManagementCache referred to by key
			ConfigurationManagementCache cacheSettings = Create( sectionName );

			//  if it's not null, clear it
			if ( cacheSettings != null )
			{
				cacheSettings.Clear();
			}
		}

		#endregion

	}


	#endregion

}
