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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MoreLinq;

namespace Origam.DA.Service
{
    public static class OrigamDocumentSorter
    {
        public static XmlDocument CopyAndSort(OrigamXmlDocument doc)
        {
            var newDoc = new OrigamXmlDocument();
            doc.FileElement.Attributes
                .Cast<XmlAttribute>()
                .OrderBy(attribute => attribute.Value)
                .ForEach(attribute => 
                    newDoc.FileElement.SetAttribute(attribute.Name, attribute.Value));
            
            doc.ChildNodes
                .Cast<XmlNode>()
                .OrderBy(node => node.Name)
                .ForEach(node => CopyNodes(node, newDoc.FileElement, newDoc));
            return newDoc;
        }

        private static void CopyNodes(XmlNode node, XmlNode targetNode, OrigamXmlDocument newDoc)
        {
            node.ChildNodes
                .Cast<XmlNode>()
                .OrderBy(childNode => childNode.NamespaceURI)
                .ThenBy(childNode => childNode.LocalName)
                .ThenBy(childNode => childNode.Attributes?["x:id"]?.Value ?? "zzzzzzzz")
                .ForEach(childNode =>
                {
                    var xmlns = string.IsNullOrEmpty(childNode.NamespaceURI)
                        ? newDoc.FileElement.Attributes["xmlns"].Value
                        : childNode.NamespaceURI;
                    XmlElement childCopy = newDoc.CreateElement(childNode.Name, xmlns);
                    CopyAttributes(childNode, childCopy);
                    targetNode.AppendChild(childCopy);
                    CopyNodes(childNode, childCopy, newDoc);
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
}
