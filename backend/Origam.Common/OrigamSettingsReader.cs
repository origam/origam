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
using System.Xml;
using System.Xml.Serialization;

namespace Origam;

public class OrigamSettingsReader
{
    private static string DefaultPathToOrigamSettings =>
        Path.Combine(path1: AppDomain.CurrentDomain.BaseDirectory, path2: "OrigamSettings.config");
    private readonly string pathToOrigamSettings;

    public OrigamSettingsReader(string pathToOrigamSettings = null)
    {
        this.pathToOrigamSettings = pathToOrigamSettings ?? DefaultPathToOrigamSettings;
    }

    public OrigamSettingsCollection GetAll()
    {
        XmlReader reader = null;
        try
        {
            XmlNode arrayOfOrigamSettingsNode = GetSettingsNode();
            if (arrayOfOrigamSettingsNode.ChildNodes.Count == 0)
            {
                return new OrigamSettingsCollection();
            }
            reader = new XmlNodeReader(node: arrayOfOrigamSettingsNode);
            XmlSerializer deserializer = new XmlSerializer(type: typeof(OrigamSettings[]));
            OrigamSettings[] settings = (OrigamSettings[])
                deserializer.Deserialize(xmlReader: reader);
            return new OrigamSettingsCollection(value: settings);
        }
        catch (Exception ex)
        {
            throw new OrigamSettingsException(message: ex.Message, innerException: ex);
        }
        finally
        {
            reader?.Dispose();
        }
    }

    private XmlNode GetSettingsNode()
    {
        return GetNodeByPath(
            pathToNode: "OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings"
        );
    }

    private XmlNode GetNodeByPath(string pathToNode)
    {
        XmlDocument document = new XmlDocument();
        document.Load(filename: pathToOrigamSettings);
        var arrayOfOrigamSettingsNode = document.SelectSingleNode(xpath: pathToNode);
        if (arrayOfOrigamSettingsNode == null)
        {
            throw new OrigamSettingsException(
                message: string.Format(format: Strings.PathNotFound, arg0: pathToNode)
            );
        }
        return arrayOfOrigamSettingsNode;
    }

    public static XmlDocument CreateEmptyDocument()
    {
        XmlDocument doc = new XmlDocument();
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration(
            version: "1.0",
            encoding: "UTF-8",
            standalone: null
        );
        doc.AppendChild(newChild: xmlDeclaration);

        XmlElement orSettings = doc.CreateElement(name: "OrigamSettings");
        XmlElement serialization = doc.CreateElement(name: "xmlSerializerSection");
        XmlAttribute typeAttr = doc.CreateAttribute(name: "type");
        typeAttr.Value =
            "Origam.OrigamSettingsCollection, Origam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        serialization.Attributes.Append(node: typeAttr);
        XmlElement settingsArray = doc.CreateElement(name: "ArrayOfOrigamSettings");
        settingsArray.SetAttribute(
            name: "xmlns:xsi",
            value: "http://www.w3.org/2001/XMLSchema-instance"
        );
        settingsArray.SetAttribute(name: "xmlns:xsd", value: "http://www.w3.org/2001/XMLSchema");
        orSettings.AppendChild(newChild: serialization);
        serialization.AppendChild(newChild: settingsArray);
        doc.AppendChild(newChild: orSettings);
        return doc;
    }

    public void Write(OrigamSettingsCollection configuration)
    {
        XmlDocument document = CreateEmptyDocument();
        XmlNode xmlSerializerNode = document.SelectSingleNode(
            xpath: "OrigamSettings/xmlSerializerSection"
        );
        var xmlDocument = new XmlDocument();
        var nav = xmlDocument.CreateNavigator();
        using (var writer = nav.AppendChild())
        {
            var ser = new XmlSerializer(type: typeof(OrigamSettings[]));
            ser.Serialize(xmlWriter: writer, o: configuration.ToArray());
        }
        xmlSerializerNode.InnerXml = xmlDocument.InnerXml;
        document.Save(filename: pathToOrigamSettings);
    }

    public string GetDefaultPathToOrigamSettings()
    {
        return DefaultPathToOrigamSettings;
    }
}
