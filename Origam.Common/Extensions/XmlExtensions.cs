#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Origam.Extensions
{
    public static class XmlExtensions
    {
        public static IEnumerable<XmlNode> GetAllNodes(this XmlNode topNode)
        {
            foreach (var node in topNode.ChildNodes)
            {
                var xmlNode = (XmlNode) node;
                yield return xmlNode;
                foreach (var childNode in GetAllNodes(xmlNode))
                {
                    yield return childNode;
                }
            }
        }
        
        public static int GetDepth(this XmlNode node)
        {
            int depth = 0;
            XmlNode parent = node.ParentNode;
            while (parent!=null)
            {
                parent = parent.ParentNode;
                depth++;
            }

            return depth;
        }
        
        public static string ToBeautifulString(this XmlDocument document, 
            XmlWriterSettings xmlWriterSettings)
        {
            MemoryStream mStream = new MemoryStream();
            XmlWriter writer =  XmlWriter.Create(mStream,xmlWriterSettings);
            try
            {
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();
                mStream.Position = 0;
                StreamReader sReader = new StreamReader(mStream);
                return sReader.ReadToEnd();
            }
            finally
            {
                mStream.Close();
            }
        }
         public static string ToBeautifulString(this XmlDocument document)
         {
             XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
             {
                 Indent = true,
                 NewLineOnAttributes = true
             }; 
             return ToBeautifulString(document, xmlWriterSettings);
         }

        public static XmlDocument RemoveAllEmptyAttributesAndNodes(this XmlDocument doc)
        {
#if NETSTANDARD
            foreach (XmlAttribute att in doc.SelectNodes("descendant::*/@*[not(normalize-space(.))]"))
            {
                att.OwnerElement.RemoveAttributeNode(att);
            }
            if (!string.IsNullOrEmpty(doc.OuterXml))
            {
                var elements = XDocument.Parse(doc.OuterXml);
                elements.Descendants().Where(e => e.IsEmpty || string.IsNullOrWhiteSpace(e.Value)).Remove();
            }
#endif
            return doc;
        }
    }
}