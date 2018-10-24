#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Origam.Extensions;

namespace Origam
{
	/// <summary>
	/// Summary description for ConfigurationManager.
	/// </summary>
	public class ConfigurationManager2
	{
		//todo: review whether object is necessary, through the whole code only OrigamSettings is used and unnecessary casts are performed
		private static OrigamSettings _activeConfiguration;
		
		public static void SetActiveConfiguration(OrigamSettings configuration)
		{
			_activeConfiguration = configuration;
		}

		public static OrigamSettings GetActiveConfiguration()
		{
			return _activeConfiguration;
		}

		public static OrigamSettingsCollection GetAllConfigurations()
		{
			var settingsCollection = 
				new OrigamSettingsReader().GetAll();

			if(settingsCollection.Count == 1)
			{
				settingsCollection[0].BaseFolder = AssemblyTools.GetAssemblyLocation();
				WriteConfiguration("OrigamSettings", settingsCollection);
			}
			return settingsCollection;
			
//			else
//			{
//				throw new Exception(ResourceUtils.GetString("SettingsInvalidFormat"));
//			}
		}

		public static void WriteConfiguration(string name, OrigamSettingsCollection configuration)
		{
			// do some sanity check
			SortedList list = new SortedList();
			foreach(OrigamSettings setting in (configuration as OrigamSettingsCollection))
			{
				if(!list.Contains(setting.Name))
				{
					list.Add(setting.Name, setting);
				}
				else
				{
					throw new Exception(ResourceUtils.GetString("CantSaveConfig"));
				}
			}

			new OrigamSettingsReader().Write(configuration);

			//Microsoft.Practices.EnterpriseLibrary.Configuration.ConfigurationManager.WriteConfiguration(name, configuration);
		}


	}

	

	public class OrigamSettingsReader
	{
	    private static string PathToOrigamSettings =>
	        Path.Combine(AssemblyTools.GetAssemblyLocation(), "OrigamSettings.config");

        public OrigamSettingsCollection GetAll()
		{
			XmlReader reader=null;
			try
			{
				XmlNode arrayOfOrigamSettingsNode =
					GetSettingsNodesOrThrow(PathToOrigamSettings);
				if (arrayOfOrigamSettingsNode.ChildNodes.Count == 0)
				{
					return new OrigamSettingsCollection();
				}

				reader = new XmlNodeReader(arrayOfOrigamSettingsNode);
				XmlSerializer deserializer =
					new XmlSerializer(typeof(OrigamSettings[]));
				OrigamSettings[] movies =
					(OrigamSettings[]) deserializer.Deserialize(reader);

				return new OrigamSettingsCollection(movies);
			}
			catch (Exception ex)
			{
				throw new OrigamSettingsException(ex.Message, ex);
			}
			finally
			{
				reader?.Dispose();
			}
		}

	    public IOrigamAuthorizationProvider GetAuthorizationProvider()
	    {
            throw new NotImplementedException();
	    }

	    public IOrigamProfileProvider GetProfileProvider()
	    {
	        throw new NotImplementedException();
        }


	    private XmlNode GetSettingsNodesOrThrow(string pathToOrigamSettings)
		{
			XmlDocument document = new XmlDocument();
			document.Load(pathToOrigamSettings);

			var arrayOfOrigamSettingsNode = document.SelectSingleNode(
				"OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings");
			if (arrayOfOrigamSettingsNode == null)
			{
				throw new OrigamSettingsException("Could not find path \"OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings\" in OrigamSettings.config");
			}

			return arrayOfOrigamSettingsNode;
		}

		public static XmlDocument CreateEmptyDocument()
		{
			XmlDocument doc = new XmlDocument();
			XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
			doc.AppendChild(xmlDeclaration);
            
			XmlElement orSettings = doc.CreateElement("OrigamSettings");
			XmlElement serialization = doc.CreateElement("xmlSerializerSection");
			XmlAttribute typeAttr = doc.CreateAttribute("type");
			typeAttr.Value = "Origam.OrigamSettingsCollection, Origam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			serialization.Attributes.Append(typeAttr);
			XmlElement settingsArray = doc.CreateElement("ArrayOfOrigamSettings");
			settingsArray.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
			settingsArray.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
			orSettings.AppendChild(serialization);
			serialization.AppendChild(settingsArray);
            
			XmlElement security = doc.CreateElement("Security");
			orSettings.AppendChild(security);
			doc.AppendChild(orSettings);
			return doc;
		}
	
		public void Write(OrigamSettingsCollection configuration)
		{
			XmlDocument document = CreateEmptyDocument();
			XmlNode arrayOfOrigamSettingsNode = 
				document.SelectSingleNode("OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings");
			var xmlDocument = new XmlDocument();
			var nav = xmlDocument.CreateNavigator();
			using (var writer = nav.AppendChild())
			{
				var ser = new XmlSerializer(typeof(OrigamSettings[]));
				ser.Serialize(writer, configuration.ToList<OrigamSettings>().ToArray());
			}
			arrayOfOrigamSettingsNode.InnerXml = xmlDocument.InnerXml;
			document.Save(PathToOrigamSettings);
		}
    }

	public class OrigamSettingsException: Exception
	{
		private static string MakeMessage(string message) =>
			"Cannot read OrigamSettings.config... " + message;
		
		public OrigamSettingsException(string message, Exception innerException) 
			: base(MakeMessage(message), innerException)
		{	
		}
		public OrigamSettingsException(string message) 
			: base(MakeMessage(message))
		{
		}
	}
}

