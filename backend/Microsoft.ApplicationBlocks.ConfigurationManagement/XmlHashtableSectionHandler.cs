// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// XmlHashtableSectionHandler.cs
//
// A sample section handler that converts a hashtable so it can be used on
// any application that uses a Hashtable to handle the configuration.
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
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	/// <summary>
	/// A sample section handler which stores a hashtable on the confguration
	/// storage.
	/// </summary>
	internal class XmlHashtableSectionHandler : IConfigurationSectionHandler, IConfigurationSectionHandlerWriter
	{
		private static XmlSerializer _xmlSerializer;

		static XmlHashtableSectionHandler()
		{		
			_xmlSerializer = new XmlSerializer( typeof(XmlSerializableHashtable) );
		}


		#region Implementation of IConfigurationSectionHandler

		object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
		{
			if( section.ChildNodes.Count == 0 )
				return new Hashtable();

			lock( this )
			{
				try
				{
					XmlSerializableHashtable xmlHt = (XmlSerializableHashtable)_xmlSerializer.Deserialize( new XmlNodeReader( section ) );
					return xmlHt.InnerHashtable;
				}
				catch( Exception e )
				{
					throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionCantDeserializeHashtable" ], e );
				}
			}
		}

		

		#endregion

		#region Implementation of IConfigurationSectionHandlerWriter
		
		XmlNode IConfigurationSectionHandlerWriter.Serialize(object value)
		{
			if( !(value is Hashtable) )
			{
				throw new ConfigurationException( Resource.ResourceManager[ "RES_ExceptionInvalidConfigurationInstance" ] );
			}

			//  get the stringwriter instance
			StringWriter sw = new StringWriter(  );

			//  serialize a new XmlSerializableHashtable...
			//  use its constructor to pass in the actual (non-serializable) hashtable we've been given
			_xmlSerializer.Serialize( sw, new XmlSerializableHashtable( (Hashtable)value ) );

			//  put the xml-serialized text into an xml doc
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( sw.ToString() ); 

			//  return it
			return doc.DocumentElement;
		}
		

		#endregion
	}
}
