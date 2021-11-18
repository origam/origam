// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// XmlFileStorage.cs
//
// This file contains a read-write storage implementation that uses an 
// xml file to save the configuration.
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
using System.Text;
using System.Xml;

using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement.Storage
{
	/// <summary>
	/// NOTE only need to implement IConfigurationStorageWriter interface, that interface aggregates Reader AND Writer functionality.
	/// If we wished only a Reader, we would implement the lesser IConfigurationStorageReader interface.
	/// </summary>
	internal class XmlFileStorage : IConfigurationStorageWriter
	{
		#region Declare Variables
		string _applicationDocumentPath = null;
		string _sectionName = null;
		bool _isSigned = false;
		bool _isEncrypted = false;
		bool _isRefreshOnChange = false;
		bool _isInitOk = false;
        FileSystemWatcher _fileWatcherApp = null;        
		IDataProtection _dataProtection;
		#endregion

		#region Constructor

		public XmlFileStorage()
		{
		}

		#endregion

		#region IConfigurationStorageReader implementation

		/// <summary>
		/// Inits the provider
		/// </summary>
		public void Init( string sectionName, ListDictionary configStorageParameters, IDataProtection dataProtection )	
		{
			// Inits the provider properties
			_sectionName = sectionName;

			_applicationDocumentPath = configStorageParameters[ "path" ] as string;
			if( _applicationDocumentPath == null )
				_applicationDocumentPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            else
                _applicationDocumentPath = Path.GetFullPath( _applicationDocumentPath );

			string signedString = configStorageParameters[ "signed" ] as string;
			if( signedString != null && signedString.Length != 0 )
				_isSigned = bool.Parse( signedString );
			
			string encryptedString = configStorageParameters[ "encrypted" ] as string;
			if( encryptedString != null && encryptedString.Length != 0 )
				_isEncrypted = bool.Parse( encryptedString );

			string refreshOnChangeString = configStorageParameters[ "refreshOnChange" ] as string;
			if( refreshOnChangeString != null && refreshOnChangeString.Length != 0 )
				_isRefreshOnChange = bool.Parse( refreshOnChangeString );
			
			this._dataProtection = dataProtection;
			
			if( ( _isSigned || _isEncrypted ) && _dataProtection == null )
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidDataProtectionConfiguration", _sectionName ] );

			//  here we set up a file-watch that, if refreshOnChange is enabled, will fire an event when config 
			//  changes and cause cache to flush
			if( _isRefreshOnChange )
			{
                if(Path.IsPathRooted(_applicationDocumentPath))
                    _fileWatcherApp = new FileSystemWatcher(Path.GetDirectoryName(_applicationDocumentPath), Path.GetFileName(_applicationDocumentPath));
                else
                    _fileWatcherApp = new FileSystemWatcher(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Path.GetFileName(_applicationDocumentPath));
                        
                _fileWatcherApp.EnableRaisingEvents = true;
                _fileWatcherApp.Changed += new FileSystemEventHandler( OnChanged );

			}
			_isInitOk = true;
		}

		public bool Initialized
		{ 
			get
			{
				return _isInitOk;
			} 
		}

		/// <summary>
		/// Return a section node
		/// </summary>
		public XmlNode Read()
		{
            XmlDocument xmlApplicationDocument = new XmlDocument();
			try
			{
				if ( File.Exists(_applicationDocumentPath) )
					LoadXmlFile(xmlApplicationDocument, _applicationDocumentPath);
				else
					throw new ConfigurationException(Resource.ResourceManager["RES_ExceptionConfigurationFileNotFound", xmlApplicationDocument, _sectionName]);
			}
			catch (XmlException)
			{
				 throw new ConfigurationException(Resource.ResourceManager["RES_ExceptionInvalidConfigurationDocument"]);
			}

            // Select the item nodes
			XmlNode sectionNode = xmlApplicationDocument.SelectSingleNode( @"/configuration/" + SectionName );
			if( sectionNode == null )
				return null;

			// Clone the XmlNode to prevent concurrency problems
			sectionNode = sectionNode.CloneNode( true );

			if( _isSigned || _isEncrypted )
			{
				XmlNode encryptedNode = sectionNode.SelectSingleNode( "encryptedData" );
				string sectionData = "";
				if( encryptedNode != null )
					sectionData = encryptedNode.InnerXml;

				XmlNode signatureNode = sectionNode.SelectSingleNode( "signature" );
				string xmlSignature = "";
				if( signatureNode != null )
					xmlSignature = signatureNode.InnerXml;

				if( _isSigned )
				{
					// Compute the hash
					byte[] hash = _dataProtection.ComputeHash( Encoding.UTF8.GetBytes( sectionData ) );
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
							encryptedBytes = Convert.FromBase64String( sectionData );
							decryptedBytes = _dataProtection.Decrypt( encryptedBytes );
							sectionData = Encoding.UTF8.GetString( decryptedBytes );
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

				XmlDocument xmlDoc = new XmlDocument();
				XmlNode newNode = xmlDoc.CreateElement( SectionName );
				newNode.InnerXml = sectionData;
				xmlDoc.AppendChild( newNode );
				return xmlDoc.FirstChild.FirstChild;
			}
			else
			{
				return sectionNode.FirstChild;
			}
		}


		/// <summary>
		/// Event to indicate a change in the configuration storage
		/// </summary>
		public event ConfigurationChanged ConfigChanges;

		#endregion

		#region IConfigurationStorageWriter implementation
		
		/// <summary>
		/// Writes a section on the XML document
		/// </summary>
		public void Write( XmlNode value )
		{
            XmlDocument xmlApplicationDocument = new XmlDocument(); 
			lock( this.GetType() )
			{
                if ( File.Exists(_applicationDocumentPath) )
				{
					try
					{
						LoadXmlFile(xmlApplicationDocument, _applicationDocumentPath);
					}
                    catch (XmlException)
					{
						throw new ConfigurationException(Resource.ResourceManager["RES_ExceptionInvalidConfigurationDocument"]);
					}
				}
                else
				{
                    XmlNode configNode = xmlApplicationDocument.CreateElement("configuration");
                    xmlApplicationDocument.AppendChild(configNode);
				}
			}

            // Select the item nodes
			XmlNode sectionNode = xmlApplicationDocument.SelectSingleNode( @"/configuration/"  + SectionName );

			if( sectionNode != null )
			{
				// Remove the node contents
				sectionNode.RemoveAll();
			}
			else
			{
				// If the node does not exist, then it's created
				sectionNode = xmlApplicationDocument.CreateElement( SectionName );
				XmlNode configurationNode = xmlApplicationDocument.SelectSingleNode( @"/configuration" );
				configurationNode.AppendChild( sectionNode );
			}

			XmlNode cloneNode;
			if( IsSigned || IsEncrypted )
			{
				string paramValue = value.OuterXml;
				string paramSignature = "";
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
					// Compute the hash
					byte[] hash = _dataProtection.ComputeHash( Encoding.UTF8.GetBytes( paramValue ) );
					paramSignature = Convert.ToBase64String( hash ); 
				}

				XmlNode signatureNode = sectionNode.OwnerDocument.CreateElement( "signature" );
				signatureNode.InnerText = paramSignature;
				sectionNode.AppendChild( signatureNode );
				XmlNode encryptedNode = sectionNode.OwnerDocument.CreateElement( "encryptedData" );
				encryptedNode.InnerXml = paramValue;
				sectionNode.AppendChild( encryptedNode );
			}
			else
			{
				cloneNode = xmlApplicationDocument.ImportNode( value, true ); 

				// Appends the node to the document
				sectionNode.AppendChild( cloneNode );
			}

			// Save the document
			lock( this.GetType() )
			{
				if( _fileWatcherApp != null ) 
                    _fileWatcherApp.EnableRaisingEvents = false;

				using( FileStream fs = new FileStream( _applicationDocumentPath, FileMode.Create ) )
				{
					xmlApplicationDocument.Save( fs );
					fs.Flush();
				}

				if( _fileWatcherApp != null )
                    _fileWatcherApp.EnableRaisingEvents = true;
			}
		}
		
		#endregion

		#region Protected properties
		protected string ApplicationDocumentPath
		{
			get{ return _applicationDocumentPath; }
		}

        protected bool IsSigned
		{
			get{ return _isSigned; }
		}
		
        protected bool IsEncrypted
		{
			get{ return _isEncrypted; }
		}
		
        public bool IsRefreshOnChange
		{
			get{ return _isRefreshOnChange; }
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

		#region Protected methods
		/// <summary>
		/// FileSystemWatcher Event Handler
		/// </summary>
		protected virtual void OnChanged( object sender, FileSystemEventArgs e ) 
		{
			// Notify file changes to the configuration manager
			if (ConfigChanges != null)
				ConfigChanges( this, SectionName );
		}

		/// <summary>
		/// Loads the Xml file on the document
		/// </summary>
		/// <param name="doc">An Xml document instance</param>
		/// <param name="fileName">The file name</param>
		void LoadXmlFile( XmlDocument doc, string fileName )
		{
            using( FileStream fs = new FileStream( fileName, FileMode.Open, FileAccess.Read ) )
			{
                doc.Load( fs );
			}
		}
		#endregion
	}
}
