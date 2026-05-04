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

#pragma warning disable IDE0005
using System.Data;
using System.Xml;
using Origam.Service.Core;
#pragma warning restore IDE0005

namespace Origam;

#if !NETSTANDARD
public class DataDocumentFx : IDataDocument
{
#pragma warning disable 618 // XmlDataDocument is obsolete, but we cannot get rid of it just yet
    private readonly XmlDataDocument xmlDataDocument;

    public DataDocumentFx(DataSet dataSet)
    {
        xmlDataDocument = new XmlDataDocument(dataset: dataSet);
    }

    public DataDocumentFx(XmlDocument xmlDoc)
    {
        xmlDataDocument = new XmlDataDocument();
        foreach (XmlNode childNode in xmlDoc.ChildNodes)
        {
            var importNode = xmlDataDocument.ImportNode(node: childNode, deep: true);
            xmlDataDocument.AppendChild(newChild: importNode);
        }
    }
#pragma warning restore 618
    public XmlDocument Xml => xmlDataDocument;
    public DataSet DataSet => xmlDataDocument.DataSet;

    public void AppendChild(XmlNodeType element, string prefix, string name)
    {
        XmlNode node = xmlDataDocument.CreateNode(type: element, name: prefix, namespaceURI: name);
        xmlDataDocument.AppendChild(newChild: node);
    }

    public void AppendChild(XmlElement documentElement, bool deep)
    {
        XmlNode node = xmlDataDocument.ImportNode(node: documentElement, deep: true);
        xmlDataDocument.AppendChild(newChild: node);
    }

    public void DocumentElementAppendChild(XmlNode node)
    {
        XmlNode newNode = xmlDataDocument.ImportNode(node: node, deep: true);
        xmlDataDocument.DocumentElement.AppendChild(newChild: newNode);
    }

    public void Load(XmlReader xmlReader, bool doProcessing)
    {
        xmlDataDocument.Load(reader: xmlReader);
    }

    public void LoadXml(string xmlString)
    {
        xmlDataDocument.LoadXml(xml: xmlString);
    }

    public object Clone()
    {
        return new DataDocumentFx(xmlDoc: xmlDataDocument);
    }
}
#endif
