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

using System.Data;
using System.Xml;
using Origam.Service.Core;

namespace Origam;

#if !NETSTANDARD
public class DataDocumentFx : IDataDocument
{
#pragma warning disable 618 // XmlDataDocument is obsolete, but we cannot get rid of it just yet
    private readonly XmlDataDocument xmlDataDocument;

    public DataDocumentFx(DataSet dataSet)
    {
        xmlDataDocument = new XmlDataDocument(dataSet);
    }

    public DataDocumentFx(XmlDocument xmlDoc)
    {
        xmlDataDocument = new XmlDataDocument();
        foreach (XmlNode childNode in xmlDoc.ChildNodes)
        {
            var importNode = xmlDataDocument.ImportNode(childNode, true);
            xmlDataDocument.AppendChild(importNode);
        }
    }
#pragma warning restore 618
    public XmlDocument Xml => xmlDataDocument;
    public DataSet DataSet => xmlDataDocument.DataSet;

    public void AppendChild(XmlNodeType element, string prefix, string name)
    {
        XmlNode node = xmlDataDocument.CreateNode(element, prefix, name);
        xmlDataDocument.AppendChild(node);
    }

    public void AppendChild(XmlElement documentElement, bool deep)
    {
        XmlNode node = xmlDataDocument.ImportNode(documentElement, true);
        xmlDataDocument.AppendChild(node);
    }

    public void DocumentElementAppendChild(XmlNode node)
    {
        XmlNode newNode = xmlDataDocument.ImportNode(node, true);
        xmlDataDocument.DocumentElement.AppendChild(newNode);
    }

    public void Load(XmlReader xmlReader, bool doProcessing)
    {
        xmlDataDocument.Load(xmlReader);
    }

    public void LoadXml(string xmlString)
    {
        xmlDataDocument.LoadXml(xmlString);
    }

    public object Clone()
    {
        return new DataDocumentFx(xmlDataDocument);
    }
}
#endif
