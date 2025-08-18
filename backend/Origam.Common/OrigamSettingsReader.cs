#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Origam.Extensions;


namespace Origam;
public class OrigamSettingsReader
{
    private static string DefaultPathToOrigamSettings => 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OrigamSettings.config");
    private readonly string pathToOrigamSettings;
    public OrigamSettingsReader(string pathToOrigamSettings = null)
    {
        this.pathToOrigamSettings = pathToOrigamSettings
                                    ?? DefaultPathToOrigamSettings;
    }
    public OrigamSettingsCollection GetAll()
    {
        XmlReader reader=null;
        try
        {
            XmlNode arrayOfOrigamSettingsNode =
                GetSettingsNode();
            if (arrayOfOrigamSettingsNode.ChildNodes.Count == 0)
            {
                return new OrigamSettingsCollection();
            }
            reader = new XmlNodeReader(arrayOfOrigamSettingsNode);
            XmlSerializer deserializer =
                new XmlSerializer(typeof(OrigamSettings[]));
            OrigamSettings[] settings =
                (OrigamSettings[]) deserializer.Deserialize(reader);
            return new OrigamSettingsCollection(settings);
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
    private XmlNode GetSettingsNode()
    {
        return GetNodeByPath("OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings");
    }
    private XmlNode GetNodeByPath(string pathToNode)
    {
        XmlDocument document = new XmlDocument();
        document.Load(pathToOrigamSettings);
        var arrayOfOrigamSettingsNode = document.SelectSingleNode(
            pathToNode);
        if (arrayOfOrigamSettingsNode == null)
        {
            throw new OrigamSettingsException(string.Format(Strings.PathNotFound, pathToNode));
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
        doc.AppendChild(orSettings);
        return doc;
    }

    public void Write(OrigamSettingsCollection configuration)
    {
        XmlDocument document = CreateEmptyDocument();
        XmlNode xmlSerializerNode = 
            document.SelectSingleNode("OrigamSettings/xmlSerializerSection");
        var xmlDocument = new XmlDocument();
        var nav = xmlDocument.CreateNavigator();
        using (var writer = nav.AppendChild())
        {
            var ser = new XmlSerializer(typeof(OrigamSettings[]));
            ser.Serialize(writer, configuration.ToArray());
        }
        xmlSerializerNode.InnerXml = xmlDocument.InnerXml;
        document.Save(pathToOrigamSettings);
    }
    public string GetDefaultPathToOrigamSettings()
    {
        return DefaultPathToOrigamSettings;
    }
}
