// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// ConfigurationManagerSectionHandler.cs
//
// Section handler used to read the CMAB configuration for the sectrions
// placed on the configuration file.
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
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
    #region Class Definitions
    /// <summary>
    /// Class that defines the settings within the configuration management settings in the config file.
    /// </summary>
    internal class ConfigurationManagementSettings
    {	
		#region Member fields

        private HybridDictionary							_configSections		= new HybridDictionary(5, false);
		private static ConfigurationManagementSettings		_singletonSelf		= null;

		#endregion
       	
		#region Constructors

		static ConfigurationManagementSettings(){}

		public ConfigurationManagementSettings(){}

		#endregion

		#region Singleton

		public static ConfigurationManagementSettings Instance
		{
			get
			{
				//  check if singleton exists yet; 
				if ( _singletonSelf == null )
				{
					//  NO, load it
					_singletonSelf  = (ConfigurationManagementSettings)ConfigurationSettings.GetConfig( "applicationConfigurationManagement" );
				}
				
				//  return it
				return	_singletonSelf;
				
			}
		}


		#endregion

		#region Public properties

        /// <summary>
        /// An ArrayList containing all of the ConfigSectionSettings listed in the config file.
        /// </summary>
        public HybridDictionary Sections
        {
            get{ return _configSections; }
        }


		/// <summary>
		/// The section used when the user adds the DefaultSectionName attribute to the
		/// applicationConfigurationManagement configuration section.
		/// </summary>
		public string DefaultSectionName
		{
			get{ return _defaultSectionName; }

			set{ _defaultSectionName = value; }
		} string _defaultSectionName;

		public ConfigSectionSettings this[ string key ]
		{
			get { return (ConfigSectionSettings)_configSections[ key ]; }
		}

		#endregion

		#region Public methods

        /// <summary>
        /// Adds a ConfigSectionSettings to the arraylist of sections.
        /// </summary>
        public void AddConfigurationSection(ConfigSectionSettings configSection)
        {
            _configSections[ configSection.Name ] = configSection;
        }


		#endregion
    }

    /// <summary>
    /// Class that defines the cache settings within the configuration management settings in the config file.
    /// </summary>
    internal class ConfigCacheSettings
    {
		#region Member fields
        private bool						_isEnabled				= false;
        private string						_refresh;
		#endregion

		#region Public properties
        /// <summary>
        /// This property specifies if the cache should be enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get{ return _isEnabled; }
            set{ _isEnabled = value; }
        }

		/// <summary>
        /// Absolute time format for refresh of the config data cache. 
        /// </summary>
        public string Refresh
        {
            get{ return _refresh; }
            set{ _refresh = value; }
        }
		#endregion
    }
        
    /// <summary>
    /// Class that defines the provider settings within the configuration management settings in the config file.
    /// </summary>
    internal class ConfigProviderSettings
    {
		#region Member fields
	    private string _typeName;
        private string _assemblyName;
        private ListDictionary  _otherAttributes = new ListDictionary ();
		#endregion

		#region Public properties
	    /// <summary>
        /// The assembly name of the configuration provider component that will be used to invoke the object.
        /// </summary>
        public string AssemblyName
        {
            get{ return _assemblyName; }
            set{ _assemblyName = value; }
        }

        /// <summary>
        /// The type name of the configuration provider component that will be used to invoke the object.
        /// </summary>
        public string TypeName
        {
            get{ return _typeName; }
            set{ _typeName = value; }
        }

        /// <summary>
        /// An collection of any other attributes included within the provider tag in the config file. 
        /// </summary>
        public ListDictionary OtherAttributes
        {
            get{ return _otherAttributes; }
        }
		#endregion

		#region Public members
        /// <summary>
        /// Allows name/value pairs to be added to the Other Attributes collection.
        /// </summary>
        public void AddOtherAttributes(string name, string value)
        {
            _otherAttributes.Add(name, value);
        }
		#endregion
    }

    /// <summary>
    /// Class that defines the protection provider settings within the configuration management settings in the config file.
    /// </summary>
    internal class DataProtectionProviderSettings
    {
		#region Declare variables
        private string _typeName;
        private string _assemblyName;
        private ListDictionary _otherAttributes = new ListDictionary();
		#endregion

		#region Public properties
        /// <summary>
        /// The assembly name of the protection configuration provider component that will be used to invoke the object.
        /// </summary>
        public string AssemblyName
        {
            get{ return _assemblyName; }
            set{ _assemblyName = value; }
        }

        /// <summary>
        /// The type name of the protection provider component that will be used to invoke the object.
        /// </summary>
        public string TypeName
        {
            get{ return _typeName; }
            set{ _typeName = value; }
        }

        /// <summary>
        /// An collection of any other attributes included within the provider tag in the config file. 
        /// </summary>
        public ListDictionary OtherAttributes
        {
            get{ return _otherAttributes; }
        }
		#endregion

		#region Public methods
        /// <summary>
        /// Allows name/value pairs to be added to the Other Attributes collection.
        /// </summary>
        public void AddOtherAttributes(string name, string value)
        {
            _otherAttributes.Add(name, value);
        }
		#endregion
    }
    
    /// <summary>
    /// Class that defines the section settings within the configuration management settings in the config file.
    /// </summary>
    internal class ConfigSectionSettings
    {
		#region Declare variables
        private ConfigCacheSettings _cache;
        private ConfigProviderSettings _provider;
        private DataProtectionProviderSettings _protection;
        private string _name;
		#endregion

		#region Public properties

        /// <summary>
        /// A ConfigCacheSettings configurated in the config file.
        /// </summary>
        public ConfigCacheSettings Cache
        {
            get{ return _cache; }
            set{ _cache = value; }
        }


        /// <summary>
        /// A ConfigProviderSettings configurated in the config file.
        /// </summary>
        public ConfigProviderSettings Provider
        {
            get{ return _provider; }
            set{ _provider = value; }
        }


        /// <summary>
        /// A ProtectionProviderSettings configurated in the config file.
        /// </summary>
        public DataProtectionProviderSettings DataProtection
        {
            get{ return _protection; }
            set{ _protection = value; }
        }
        

        /// <summary>
        /// This property specifies the section name
        /// </summary>
        public string Name
        {
            get{ return _name; }
            set{ _name = value; }
        }


		#endregion
    }

    #endregion
    
    #region ConfigurationManagerSectionHandler

    /// <summary>
    /// The Configuration Section Handler for the "configurationManagement" section of the config file. 
    /// </summary>
    internal class ConfigurationManagerSectionHandler : IConfigurationSectionHandler
    {
		#region Members
		bool _isValidDocument = true;
		string _schemaErrors = "";
		#endregion

		#region Constructors
        /// <summary>
        /// The constructor for the ConfigurationManagerSectionHandler to initialize the resource file.
        /// </summary>
        public ConfigurationManagerSectionHandler()
        {
        }
		#endregion

		#region Implementation of IConfigurationSectionHandler

        /// <summary>
        /// Builds the ConfigurationManagementSettings, ConfigurationProviderSettings and ConfigurationItemsSettings structures based on the configuration file.
        /// </summary>
        /// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
        /// <param name="configContext">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
        /// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
        /// <returns>The ConfigurationManagementSettings struct built from the configuration settings.</returns>
		public object Create(object parent,object configContext,XmlNode section)
        {
            try
            {
                ConfigurationManagementSettings configSettings = new ConfigurationManagementSettings();
                
                // Exit if there is no configuration settings.
                if (section == null) return configSettings;
                
				// Validate the document using a schema
				XmlValidatingReader vreader = new XmlValidatingReader( new XmlTextReader( new StringReader( section.OuterXml ) ) );
				using( Stream xsdFile = Resource.ResourceManager.GetStream( "Microsoft.ApplicationBlocks.ConfigurationManagement.ConfigSchema.xsd" ) )
	 		    {
					using( StreamReader sr = new StreamReader( xsdFile ) )
					{
						vreader.ValidationEventHandler += new ValidationEventHandler( ValidationCallBack );
						vreader.Schemas.Add( XmlSchema.Read( new XmlTextReader( sr ), null ) );
						vreader.ValidationType = ValidationType.Schema;
						// Validate the document
						while (vreader.Read()){}

						if( !_isValidDocument )
						{
							throw new ConfigurationException( Resource.ResourceManager[ "Res_ExceptionDocumentNotValidated", _schemaErrors ] );
						}
					}
				}

				XmlAttribute attr = section.Attributes[ "defaultSection" ];
				if( attr != null )
					configSettings.DefaultSectionName = attr.Value;

                //#region Loop through the section components and load them into the ConfigurationManagementSettings

                ConfigSectionSettings sectionSettings;
                foreach(XmlNode configChildNode in section.ChildNodes)
                {
                    if (configChildNode.Name == "configSection" )
                    {	
						ProcessConfigSection( configChildNode, out sectionSettings, configSettings );
					}
                }

                //#endregion

                // Return the ConfigurationManagementSettings loaded with the values from the config file.
                return configSettings;

            }
            catch (Exception exc)
            {
                throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionLoadingConfiguration" ], exc, section);
            }
        }


		#endregion

		#region Private methods
		/// <summary>
		/// Process the "configSection" xml node section
		/// </summary>
		/// <param name="configChildNode"></param>
		/// <param name="sectionSettings"></param>
		/// <param name="configSettings"></param>
		void ProcessConfigSection( XmlNode configChildNode, out ConfigSectionSettings sectionSettings, ConfigurationManagementSettings configSettings )
		{
			// Initialize a new ConfigSectionSettings.
			sectionSettings = new ConfigSectionSettings();

			// Get a collection of all the attributes.
			XmlAttributeCollection nodeAttributes = configChildNode.Attributes;

			//#region Remove the known attributes and load the struct values

			// Remove the name attribute from the node and set its value in ConfigSectionSettings.
			XmlNode currentAttribute = nodeAttributes.RemoveNamedItem( "name" );
			if (currentAttribute != null) sectionSettings.Name = currentAttribute.Value;

			// Loop through the section components and load them into the ConfigurationManagementSettings.
			ConfigCacheSettings cacheSettings;
			ConfigProviderSettings providerSettings;
			DataProtectionProviderSettings protectionSettings;
			foreach(XmlNode sectionChildNode in configChildNode.ChildNodes)
			{
				switch ( sectionChildNode.Name )
				{
					case "configCache" :
						ProcessConfigCacheSection( sectionChildNode, out cacheSettings, sectionSettings );
						break;
					case "configProvider" : 
						ProcessConfigProviderSection( sectionChildNode, out providerSettings, sectionSettings );
						break;
					case "protectionProvider" :
						ProcessProtectionProvider( sectionChildNode, out protectionSettings, sectionSettings );
						break;
					default :
						break;
				}
			}
			//#endregion

			// Add the ConfigurationSectionSettings to the sections collection.
			configSettings.AddConfigurationSection(sectionSettings);
		}

		/// <summary>
		/// Process the "configCache" xml node section
		/// </summary>
		/// <param name="sectionChildNode"></param>
		/// <param name="cacheSettings"></param>
		/// <param name="sectionSettings"></param>
		void ProcessConfigCacheSection( XmlNode sectionChildNode, out ConfigCacheSettings cacheSettings, ConfigSectionSettings sectionSettings )
		{
			// Initialize a new ConfigCacheSettings.
			cacheSettings = new ConfigCacheSettings();

			// Get a collection of all the attributes.
			XmlAttributeCollection nodeAttributes = sectionChildNode.Attributes;

			// Remove the enabled attribute from the node and set its value in ConfigCacheSettings.
			XmlNode currentAttribute = nodeAttributes.RemoveNamedItem( "enabled" );
			if (currentAttribute != null && 
				currentAttribute.Value.ToUpper( System.Globalization.CultureInfo.CurrentUICulture ) == "TRUE" ) 
				cacheSettings.IsEnabled = true;

			// Remove the refresh attribute from the node and set its value in ConfigCacheSettings.
			currentAttribute = nodeAttributes.RemoveNamedItem( "refresh" );
			if (currentAttribute != null) 
				cacheSettings.Refresh = currentAttribute.Value;

			// Set the ConfigurationCacheSettings to the section cache.
			sectionSettings.Cache = cacheSettings;
		}

		/// <summary>
		/// Process the "configProvider" xml node section
		/// </summary>
		/// <param name="sectionChildNode"></param>
		/// <param name="providerSettings"></param>
		/// <param name="sectionSettings"></param>
		void ProcessConfigProviderSection( XmlNode sectionChildNode, out ConfigProviderSettings providerSettings, ConfigSectionSettings sectionSettings )
		{
			// Initialize a new ConfigProviderSettings.
			providerSettings = new ConfigProviderSettings();

			// Get a collection of all the attributes.
			XmlAttributeCollection nodeAttributes = sectionChildNode.Attributes;
                                
			//#region Remove the provider known attributes and load the struct values
			// Remove the assembly attribute from the node and set its value in ConfigProviderSettings.
			XmlNode currentAttribute = nodeAttributes.RemoveNamedItem( "assembly" );
			if (currentAttribute != null) providerSettings.AssemblyName = currentAttribute.Value.Trim();

			// Remove the type attribute from the node and set its value in ConfigProviderSettings.
			currentAttribute = nodeAttributes.RemoveNamedItem( "type" );
			if (currentAttribute != null) providerSettings.TypeName = currentAttribute.Value;
			//#endregion

			//#region Loop through any other attributes and load them into OtherAttributes
			// Loop through any other attributes and load them into OtherAttributes.
			for (int i = 0; i < nodeAttributes.Count; i++)
			{
				providerSettings.AddOtherAttributes( nodeAttributes.Item(i).Name , nodeAttributes.Item(i).Value );
			}
			//#endregion
                                
			// Set the ConfigurationProviderSettings to the section provider.
			sectionSettings.Provider = providerSettings;
		}

		/// <summary>
		/// Process the "protectionProvider" xml node section
		/// </summary>
		/// <param name="sectionChildNode"></param>
		/// <param name="protectionSettings"></param>
		/// <param name="sectionSettings"></param>
		void ProcessProtectionProvider( XmlNode sectionChildNode, out DataProtectionProviderSettings protectionSettings, ConfigSectionSettings sectionSettings )
		{
			// Initialize a new DataProtectionProviderSettings.
			protectionSettings = new DataProtectionProviderSettings(); 

			// Get a collection of all the attributes.
			XmlAttributeCollection nodeAttributes = sectionChildNode.Attributes;
                                
			//#region Remove the provider known attributes and load the struct values
			// Remove the assembly attribute from the node and set its value in DataProtectionProviderSettings.
			XmlNode currentAttribute = nodeAttributes.RemoveNamedItem( "assembly" );
			if (currentAttribute != null) protectionSettings.AssemblyName = currentAttribute.Value;

			// Remove the type attribute from the node and set its value in DataProtectionProviderSettings.
			currentAttribute = nodeAttributes.RemoveNamedItem( "type" );
			if (currentAttribute != null) protectionSettings.TypeName = currentAttribute.Value;
			//#endregion

			//#region Loop through any other attributes and load them into OtherAttributes
			// Loop through any other attributes and load them into OtherAttributes.
			for (int i = 0; i < nodeAttributes.Count; i++)
			{
				protectionSettings.AddOtherAttributes(nodeAttributes.Item(i).Name,nodeAttributes.Item(i).Value);
			}
			//#endregion
                                
			// Set the DataProtectionProviderSettings to the section provider.
			sectionSettings.DataProtection = protectionSettings;
		}

		private void ValidationCallBack( object sender, ValidationEventArgs args )
		{
			_isValidDocument = false;
			_schemaErrors += args.Message + Environment.NewLine;
		}

		#endregion
    }


    #endregion
}
