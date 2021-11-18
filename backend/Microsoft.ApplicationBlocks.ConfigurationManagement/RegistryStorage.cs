// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// RegistryStorage.cs
//
// This file contains a read-write storage implementation that uses the windows
// registry to save the configuration.
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
using System.Text;
using System.Xml;
using Microsoft.Win32;

using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement.Storage
{
	#region RegistryStorage class
	/// <summary>
	/// Sample storage provider used to get data from the windows's registry
	/// </summary>
	/// <remarks>This privider uses the following attributes on the XML file:
	/// <list type="">
	///		<item><b>registrySubKey</b>. Used to specify the subkey for a Windows's registry value</item>
	///		<item><b>encrypted</b>. Used to specify if the section must be encrypted </item>
	///		<item><b>signed</b>. Used to specify if the section must be signed </item>
	/// </list>
	/// </remarks>
	internal class RegistryStorage : IConfigurationStorageWriter
	{
		#region Declare Variables
		RegistryKey _registrySubkey = null;
		RegistryHive registryRoot = RegistryHive.CurrentUser;
		bool _isSigned = false;
		bool _isEncrypted = false;
		bool _isInitOk = false;
		IDataProtection _dataProtection;
		string _sectionName = null;
		#endregion

		#region Default constructor
		public RegistryStorage()
		{
			// The event will never be raised because as the instance is not created,
			// no handler was added before. This is used to avoid the compiler warning.
			if( ConfigChanges != null ) 
				ConfigChanges( null, null );
		}
		#endregion

		#region IConfigurationStorageReader implementation

		public void Init( string sectionName, ListDictionary configStorageParameters, IDataProtection dataProtection )
		{
			_sectionName = sectionName;

			// Inits the provider properties
			string registryRootString = configStorageParameters[ "registryRoot" ] as string;
			if( registryRootString != null && registryRootString.Length != 0 )
				registryRoot = (RegistryHive)Enum.Parse( typeof( RegistryHive), registryRootString, true );

			string registrySubKeyString = configStorageParameters[ "registrySubKey" ] as string;
			if( registrySubKeyString == null || registrySubKeyString.Length == 0 )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidRegistrySubKey", registrySubKeyString ] );
            
			string registryPath = "";
			switch( registryRoot )
			{
				case RegistryHive.CurrentUser:
					registryPath = Registry.CurrentUser.Name;
					break;
				case RegistryHive.LocalMachine:
					registryPath = Registry.LocalMachine.Name;
					break;
				case RegistryHive.Users:
					registryPath = Registry.Users.Name;
					break;
			}
			
			_registrySubkey = GetRegistrySubKey( registryRoot, registrySubKeyString );
			if( _registrySubkey == null )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidRegistrySubKey", registrySubKeyString ] );

			string signedString = configStorageParameters[ "signed" ] as string;
			if( signedString != null && signedString.Length != 0 )
				_isSigned = bool.Parse( signedString );
			
			string encryptedString = configStorageParameters[ "encrypted" ] as string;
			if( encryptedString != null && encryptedString.Length != 0 )
				_isEncrypted = bool.Parse( encryptedString );

			this._dataProtection = dataProtection;

			if( ( _isSigned || _isEncrypted ) && _dataProtection == null )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidDataProtectionConfiguration", _sectionName ] );

			_isInitOk = true;
		}

		/// <summary>
		/// Event used internally to know when the storage detect some changes 
		/// on the config
		/// </summary>
		public virtual event ConfigurationChanged ConfigChanges;

		public bool Initialized
		{ 
			get
			{
				return _isInitOk;
			} 
		}

		public XmlNode Read()
		{
			string xmlSection, xmlSignature;
			try
			{
				using( RegistryKey sectionKey = _registrySubkey.OpenSubKey( SectionName, false ) )
				{
					if( sectionKey == null )
						return null;

					xmlSection = (string)sectionKey.GetValue( "value" ); 
					xmlSignature = (string)sectionKey.GetValue( "signature" );
				}
			}
			catch( Exception e )
			{
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionCantGetSectionData", SectionName ], e );
			}
			if( xmlSection == null )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionCantGetSectionData", SectionName ] );

			XmlDocument xmlDoc = null;
			if( _isSigned )
			{
				// Compute the hash
				byte[] hash = _dataProtection.ComputeHash( Encoding.UTF8.GetBytes( xmlSection ) );
				string newHashString = Convert.ToBase64String( hash ); 
				
				// Compare the hashes
				if( newHashString != xmlSignature )
					throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionSignatureCheckFailed", SectionName ] );
			}
			else
			{
				if( xmlSignature != null && xmlSignature.Length != 0 )
					throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidSignatureConfig", _sectionName ] );
			}

			if( _isEncrypted )
			{
				byte[] encryptedBytes = null;
				byte[] decryptedBytes = null;
				try
				{
					try
					{
						encryptedBytes = Convert.FromBase64String( xmlSection );
						decryptedBytes = _dataProtection.Decrypt( encryptedBytes );
						xmlSection = Encoding.UTF8.GetString( decryptedBytes );
					}
					catch( Exception e )
					{
						throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidEncryptedString" ], e );
					}
				}
				finally
				{
					if( decryptedBytes != null )
						Array.Clear( decryptedBytes, 0, decryptedBytes.Length );
					if( encryptedBytes != null )
						Array.Clear( encryptedBytes, 0, encryptedBytes.Length );
				}
			}
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml( xmlSection );
			return xmlDoc.DocumentElement;
		}


		#endregion

		#region IConfigurationStorageWriter implementation

		/// <summary>
		/// Writes a section on the XML document
		/// </summary>
		public void Write( XmlNode value )
		{
			if( SectionName == null || SectionName.Length == 0 )
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionCantUseNullKeys" ] );

			string paramSignature = "";
			string paramValue = value.OuterXml;
			
			if( paramValue.Length > 500000 )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionRegistryValueLimit", paramValue.Length ] );

			try
			{
				if( _isEncrypted )
				{
					byte[] decryptedBytes = null;
					byte[] encryptedBytes = null;
					try
					{
						decryptedBytes = Encoding.UTF8.GetBytes( paramValue );
						encryptedBytes = DataProtection.Encrypt( decryptedBytes );
						paramValue = Convert.ToBase64String( encryptedBytes );
					}
					finally
					{
						if( decryptedBytes != null ) Array.Clear( decryptedBytes, 0, decryptedBytes.Length );
						if( encryptedBytes != null ) Array.Clear( encryptedBytes, 0, encryptedBytes.Length );
					}
				}
				if( _isSigned )
				{
					// Compute the hash
					byte[] hash = _dataProtection.ComputeHash( Encoding.UTF8.GetBytes( paramValue ) );
					paramSignature = Convert.ToBase64String( hash ); 
				}
				RegistryKey sectionKey = RegistrySubkey.OpenSubKey( SectionName, true );
				if( sectionKey == null )
					sectionKey = RegistrySubkey.CreateSubKey( SectionName );
				using( sectionKey )
				{
					sectionKey.SetValue( "value", paramValue );
					sectionKey.SetValue( "signature", paramSignature );
				}
			}
			catch( Exception e )
			{
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionCantWriteRegistry", 
					RegistrySubkey.ToString(), SectionName, e.ToString() ], e );
			}
		}

		#endregion
	
		#region Private methods

		//  only HKCU, HKLM, and HKU are valid.  Others are too much of a security risk, even with lock-down; 
		//  DON'T store transient config data in sensitive registry hives;
		//  the hives accessed by ConfigMan should be locked down by ACL to restrict activity to the least necessary
		private RegistryKey GetRegistrySubKey( RegistryHive root, string subKey )
		{
			RegistryKey subKeyObject = null;
			switch( root )
			{
				case RegistryHive.CurrentUser:
					subKeyObject = Registry.CurrentUser.OpenSubKey( subKey, true );
					break;
				case RegistryHive.LocalMachine:
					subKeyObject = Registry.LocalMachine.OpenSubKey( subKey, true );
					break;
				case RegistryHive.Users:
					subKeyObject = Registry.Users.OpenSubKey( subKey, true );
					break;
				default :
					//  if they ask for a disallowed Hive, throw here...limit to HKCU, HKU, HKLM
					string errString = String.Format( Resource.ResourceManager[ "RES_ExceptionDisallowedRegistryKey" ],  Enum.GetName( typeof(RegistryHive), root ) );
					throw new Exception( errString );
			}

			return subKeyObject;
		}


		#endregion

		#region protected properties
		protected RegistryKey RegistrySubkey
		{
			get{ return _registrySubkey; }
		}

		protected RegistryHive RegistryRoot
		{
			get{ return registryRoot; }
		}
		protected bool IsSigned
		{
			get{ return _isSigned; }
		}
		protected bool IsEncrypted
		{
			get{ return _isEncrypted; }
		}
		protected bool IsInitOK
		{
			get{ return _isInitOk; }
		}
		protected IDataProtection DataProtection
		{
			get{ return _dataProtection; }
		}
		protected string SectionName
		{
			get{ return _sectionName; }
		}
		#endregion
	}
	#endregion
}
