// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// SqlStorage.cs
//
// This file contains a read-write storage implementation that uses microsoft
// SqlServer 2000 to save the configuration.
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
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

using Microsoft.ApplicationBlocks.Data;
using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement.Storage
{
	/// <summary>
	/// Sample SqlServer storage provider used to get data from a database
	/// </summary>
	/// <remarks>This privider uses the following attributes on the XML file:
	/// <list type="">
	///		<item><b>connectionString</b>. Used to specify the connection string for a SqlServer database</item>
	///		<item><b>getConfigSP</b>. The stored procedure used to get the database configuration. This stored procedure must return an XML. This stored procedure must have the following parameters: <c>@sectionName varchar(50)</c></item>
	///		<item><b>setConfigSP</b>. A stored procedure used to save the configuration on the database. This stored prodedure must have the following parameters: <c>@param_section varchar(50), @param_name varchar(50), @param_value varchar(255)</c></item>
	/// </list>
	/// </remarks>
	internal class SqlStorage : IConfigurationStorageWriter
	{
		#region Declare Variables

		string _connectionString			= null;
		string _getConfigSP					= null;
		string _setConfigSP					= null;
		string _sectionName					= null;
		bool _isSigned						= false;
		bool _isEncrypted					= false;
		bool _isInitOk						= false;
		IDataProtection _dataProtection		= null;

		#endregion

		#region Constructor
		public SqlStorage()
		{
			// The event will never be raised because, as the instance is not created,
			// no handler was added before. This is used to avoid the compiler warning.
			if( ConfigChanges != null ) 
				ConfigChanges( null, null );
		}


		#endregion

		#region IConfigurationStorageReader implementation

		public void Init( string sectionName, ListDictionary configStorageParameters, IDataProtection dataProtection )
		{
			// Inits the provider properties
			_sectionName = sectionName;

			// Use the registry path attribute first
			string regKey = configStorageParameters[ "connectionStringRegKeyPath" ] as string;
			if( regKey != null && regKey.Length != 0 )
				_connectionString = DataProtectionHelper.GetRegistryDefaultValue( regKey, "connectionString", "connectionStringRegKeyPath" );

			// If the connection string was not in the regustry, use the 'connectionString' attribute
			if( _connectionString == null || _connectionString.Length == 0 )
			{
				_connectionString = configStorageParameters[ "connectionString" ] as string;
				if( _connectionString == null || _connectionString.Length == 0 )
					throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidConnectionString", _connectionString ] );
			}

			_getConfigSP = configStorageParameters[ "getConfigSP" ] as string;
			if( _getConfigSP == null )
				_getConfigSP = "cmab_get_config";

			_setConfigSP = configStorageParameters[ "setConfigSP" ] as string;
			if( _setConfigSP == null )
				_setConfigSP = "cmab_set_config";

			string signedString = configStorageParameters[ "signed" ] as string;
			if( signedString != null && signedString.Length != 0 )
				_isSigned = bool.Parse( signedString );
			
			string encryptedString = configStorageParameters[ "encrypted" ] as string;
			if( encryptedString != null && encryptedString.Length != 0 )
				_isEncrypted = bool.Parse( encryptedString );

			this._dataProtection = dataProtection;
            
			if( ( _isSigned || _isEncrypted ) && _dataProtection == null )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidDataProtectionConfiguration", _sectionName ] );

			try
			{
				using( SqlConnection sqlConnection = new SqlConnection( _connectionString ) )
				{
					//  attempt to open the database...catch the exception early here rather than elsewhere when we're trying to get data
					sqlConnection.Open();
					_isInitOk = true;
				}
			}
			catch( Exception e )
			{
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionCantOpenConnection", _connectionString ], e );
			}
		}

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
				using( SqlDataReader reader = SqlHelper.ExecuteReader( _connectionString, _getConfigSP, SectionName ) )
				{
					if( !reader.Read() )
						return null;
					xmlSection = reader.IsDBNull( 0 ) ? null : reader.GetString( 0 );
					xmlSignature = reader.IsDBNull( 1 ) ? null : reader.GetString( 1 );
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
					throw new ConfigurationException( 
						Resource.ResourceManager[ "RES_ExceptionSignatureCheckFailed", 
						SectionName ] );
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

		/// <summary>
		/// Not used.
		/// </summary>
		public virtual event ConfigurationChanged ConfigChanges;

		#endregion

		#region IConfigurationStorageWriter implementation

		/// <summary>
		/// Writes a section on the XML document
		/// </summary>
		public void Write( XmlNode value )
		{
			string paramSignature = "";
			string paramValue = value.OuterXml;
			
			if( IsEncrypted )
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
			if( IsSigned )
			{
				// Keep the hashed value
				byte[] hash = _dataProtection.ComputeHash( Encoding.UTF8.GetBytes( paramValue ) );
				paramSignature = Convert.ToBase64String( hash ); 
			}
			int rows;
			try
			{
				SqlParameter sectionValueParameter = new SqlParameter( @"@section_value", SqlDbType.NText );
				sectionValueParameter.Value = paramValue;

				rows = SqlHelper.ExecuteNonQuery( 
					ConnectionString, CommandType.StoredProcedure, SetConfigSP, 
					new SqlParameter( @"@section_name", SectionName ),
					sectionValueParameter,
					new SqlParameter( @"@section_signature", paramSignature ) );
				if( rows != 1 )
				{
					throw new ConfigurationException( 
						Resource.ResourceManager[ "RES_ExceptionStoredProcedureUpdatedNoRecords", 
						SetConfigSP, SectionName, paramValue, rows ] );
				}
			}
			catch( Exception e )
			{
				throw new ConfigurationException( 
					Resource.ResourceManager[ "RES_ExceptionCantExecuteStoredProcedure", 
					SetConfigSP, SectionName, paramValue, e ] );
			}
		}


		#endregion

		#region Protected properties
		protected string ConnectionString
		{
			get{ return _connectionString; }
		}
		protected string GetConfigSP
		{ 
			get{ return _getConfigSP; } 
		}
		protected string SetConfigSP
		{
			get{ return _setConfigSP; }
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
}
