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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MoreLinq;

namespace Origam.DA.Service;
public static class OrigamDocumentSorter
{
    private class XmlnsComparer : IComparer<string>
    {
        private const string XmlnsX = "xmlns:x";
        public int Compare(string x, string y)
        {
            if ((x == XmlnsX) && (y == XmlnsX))
            {
                return 0;
            }
            if (x == XmlnsX)
            {
                return 1;
            }
            if (y == XmlnsX)
            {
                return -1;
            }
            return string.Compare(x, y);
        }
    }
    public static XmlDocument CopyAndSort(OrigamXmlDocument doc)
    {
        var nameSpaceInfo = NamespaceInfo.Create(doc);
        var newDoc = new OrigamXmlDocument();
        doc.FileElement.Attributes
            .Cast<XmlAttribute>()
            .OrderBy(
                attribute => attribute.Name, 
                new XmlnsComparer(),
                OrderByDirection.Ascending)
            .ForEach(attribute => 
                newDoc.FileElement.SetAttribute(attribute.Name, attribute.Value));
        doc.ChildNodes
            .Cast<XmlNode>()
            .ForEach(node => CopyNodes(node, newDoc.FileElement, newDoc, nameSpaceInfo));
        return newDoc;
    }
    private static void CopyNodes(
            XmlNode node, 
            XmlElement targetNode, 
            OrigamXmlDocument newDoc, 
            NamespaceInfo namespaceInfo)
    {
        var fullId = namespaceInfo.PersistencePrefix + ":id";
        CopyAttributes(node, targetNode);
        node.ChildNodes
            .Cast<XmlNode>()
            .OrderBy(childNode => childNode.Prefix + childNode.LocalName)
            .ThenBy(childNode 
                => childNode.Attributes?[fullId]?.Value 
                   ?? childNode.Attributes?["id"]?.Value 
                   ?? "zzzzzzzz")
            .ForEach(childNode =>
            {
                var xmlns = string.IsNullOrEmpty(childNode.NamespaceURI)
                    ? newDoc.FileElement.Attributes["xmlns"].Value
                    : childNode.NamespaceURI;
                var childCopy = newDoc.CreateElement(childNode.Name, xmlns);
                targetNode.AppendChild(childCopy);
                CopyNodes(childNode, childCopy, newDoc, namespaceInfo);
            });
    }
    private static void CopyAttributes(XmlNode childNode, XmlElement childCopy)
    {
        childNode?.Attributes
            ?.Cast<XmlAttribute>()
            .OrderBy(attr => attr.LocalName) 
            .ForEach(attr =>
                childCopy.SetAttribute(
                    localName: attr.LocalName,
                    namespaceURI: attr.NamespaceURI,
                    value: attr.Value));
    }
}
class NamespaceInfo
{
    public static NamespaceInfo Create(OrigamXmlDocument doc)
    {
        var xmlAttributes = doc.FileElement?.Attributes?
            .Cast<XmlAttribute>();
        return new NamespaceInfo
        {
            PersistencePrefix = xmlAttributes
                ?.FirstOrDefault(attr => attr.Value.StartsWith(
                    "http://schemas.origam.com/model-persistence"))
                ?.LocalName ?? "",
            AbstractSchemaPrefix = xmlAttributes
                ?.FirstOrDefault(attr => attr.Value.StartsWith(
                    "http://schemas.origam.com/Origam.Schema.ISchemaItem"))
                ?.LocalName ?? ""
        };
    }
    public string PersistencePrefix { get; set; }
    public string AbstractSchemaPrefix { get; set; }
}
