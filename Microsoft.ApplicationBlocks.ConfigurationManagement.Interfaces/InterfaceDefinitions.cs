// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// InterfaceDefinitions.cs
//
// This file contains the definition for the interfaces used on the application block
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
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Xml;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	#region Delegates
	/// <summary>
	/// Used the specify the configuration have been changed on the storage
	/// </summary>
	[Serializable] 
	public delegate void ConfigurationChanged( IConfigurationStorageReader storageProvider, string sectionName );

	#endregion

    #region Provider Interfaces

    /// <summary>
    /// Allows end users to implement their own configuration management storage.
    /// All storage providers must implement this interface
    /// </summary>
    [ComVisible(false)]
    public interface IConfigurationStorageReader
    {	
        /// <summary>
        /// Inits the config provider 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="configStorageParameters">Configuration parameters</param>
        /// <param name="dataProtection">Data protection interface.</param>param>
        void Init( string sectionName, ListDictionary configStorageParameters, IDataProtection dataProtection );		
        
        /// <summary>
        /// Returns an XML representation of the configuration data
        /// </summary>
        /// <returns></returns>
        XmlNode Read();

        /// <summary>
        /// Event to indicate a change in the configuration storage
        /// </summary>
        event ConfigurationChanged ConfigChanges;

		/// <summary>
		/// Whether the provider has been initialized
		/// </summary>
		bool Initialized{ get; }
    }

    /// <summary>
    /// Implemented by configuration providers to allow for writeable storage of configuration 
    /// information
    /// </summary>
    [ComVisible(false)]
    public interface IConfigurationStorageWriter
		: IConfigurationStorageReader
    {	
        /// <summary>
        /// This method writes the xml-serialized object to the underlying storage 
        /// </summary>
        void Write( XmlNode value );
    }

	/// <summary>
	/// Implemented by custom section handlers in order to allow a writeable implementation
	/// </summary>
	[ComVisible(false)]
	public interface IConfigurationSectionHandlerWriter
		: IConfigurationSectionHandler
	{	
		/// <summary>
		/// This method converts the public fields and read/write properties of an object into XML.
		/// </summary>
		XmlNode Serialize( object value );
	}
    #endregion
    
    #region DataProtection Interfaces
    /// <summary>
    /// Implemented by data protection providers to allow for encrypt information
    /// </summary>
    [ComVisible(false)]
    public interface IDataProtection
		: IDisposable
    {	
        /// <summary>
        /// Inits the data protection provider 
        /// </summary>
        /// <param name="dataProtectionParameters">Data protection parameters</param>
        void Init( ListDictionary dataProtectionParameters );
        
        /// <summary>
        /// Encrypts a raw of bytes that represents a plain text
        /// </summary>
        /// <param name="plainText">plain text</param>
        /// <returns>a cipher value</returns>
        byte[] Encrypt( byte[] plainText );
        
        /// <summary>
        /// Decrypts a cipher value
        /// </summary>
        /// <param name="cipherText">cipher text</param>
        /// <returns>a raw of bytes that represents a plain text</returns>
        byte[] Decrypt( byte[] cipherText );
        
        /// <summary>
        /// Computes a hash
        /// </summary>
        /// <param name="plainText">plain text</param>
        /// <returns>hash data</returns>
        byte[] ComputeHash( byte[] plainText );
    }
    #endregion
}
