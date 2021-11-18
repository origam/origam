// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// XmlSerializerSectionHandler.cs
//
// A sample section handler that uses xmlserializer to store any xml serializable
// class on the configuration.
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
	/// 
	/// </summary>
	internal class XmlSerializerSectionHandler : IConfigurationSectionHandler, IConfigurationSectionHandlerWriter
	{
		Hashtable _xmlSerializerCache = new Hashtable();

		public XmlSerializerSectionHandler() { }

		#region Implementation of IConfigurationSectionHandler
		object IConfigurationSectionHandler.Create( object parent, object configContext, XmlNode section )
		{
			XmlNode xmlSerializerSection = section;
			string typeName = xmlSerializerSection.Attributes[ "type" ].Value;
			Type classType = Type.GetType( typeName, true );
			
			XmlSerializer xs = _xmlSerializerCache[ classType ] as XmlSerializer;
			if( xs == null )
			{
				xs = new XmlSerializer( classType );
				_xmlSerializerCache[ classType ] = xs;
			}
			lock( xs )
				return xs.Deserialize( new XmlNodeReader( xmlSerializerSection.ChildNodes[0] ) );
		}
		#endregion

		#region Implementation of IConfigurationSectionHandlerWriter
		XmlNode IConfigurationSectionHandlerWriter.Serialize( object value )
		{
			XmlSerializer xs = _xmlSerializerCache[ value.GetType() ] as XmlSerializer;
			if( xs == null )
			{
				xs = new XmlSerializer( value.GetType() );
				_xmlSerializerCache[ value.GetType() ] = xs;
			}

			StringWriter sw = new StringWriter( System.Globalization.CultureInfo.CurrentUICulture );
			XmlTextWriter xmlTw = new XmlTextWriter( sw );
			xmlTw.WriteStartElement( "XmlSerializerSection" ) ;
			xmlTw.WriteAttributeString( "type", value.GetType().FullName + ", " + value.GetType().Assembly.FullName );
			lock( xs )
				xs.Serialize( xmlTw, value );
			xmlTw.WriteEndElement();
			xmlTw.Flush();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml( sw.ToString() );
			return doc.DocumentElement;
		}
		#endregion
	}
}
