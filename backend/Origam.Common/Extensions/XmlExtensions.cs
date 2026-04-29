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
using System.IO;
#pragma warning disable IDE0005
using System.Linq;
#pragma warning restore IDE0005
using System.Xml;
using System.Xml.Linq;

namespace Origam.Extensions;

public static class XmlExtensions
{
    private class XmlnsIndentedWriter : XmlWriter
    {
        // WriteStartDocument is skipped, so we start on root
        private bool isRootElement = true;
        private int indentLevel = -1;
        private readonly Stream stream;
        private readonly XmlWriter baseWriter;

        private XmlnsIndentedWriter(Stream output, XmlWriter baseWriter)
        {
            stream = output;
            this.baseWriter = baseWriter;
        }

        public static new XmlWriter Create(Stream stream, XmlWriterSettings settings)
        {
            var writer = XmlWriter.Create(output: stream, settings: settings);
            return new XmlnsIndentedWriter(output: stream, baseWriter: writer);
        }

        private void WriteRawText(string text)
        {
            baseWriter.Flush();
            var buffer = baseWriter.Settings.Encoding.GetBytes(s: text);
            stream.Write(buffer: buffer, offset: 0, count: buffer.Length);
        }

        #region XmlWriter implementation
        public override WriteState WriteState => baseWriter.WriteState;

        public override void Flush()
        {
            baseWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return baseWriter.LookupPrefix(ns: ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            baseWriter.WriteBase64(buffer: buffer, index: index, count: count);
        }

        public override void WriteCData(string text)
        {
            baseWriter.WriteCData(text: text);
        }

        public override void WriteCharEntity(char ch)
        {
            baseWriter.WriteCharEntity(ch: ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            baseWriter.WriteChars(buffer: buffer, index: index, count: count);
        }

        public override void WriteComment(string text)
        {
            baseWriter.WriteComment(text: text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            baseWriter.WriteDocType(name: name, pubid: pubid, sysid: sysid, subset: subset);
        }

        public override void WriteEndAttribute()
        {
            if (indentLevel >= 0)
            {
                WriteRawText(
                    text: baseWriter.Settings.NewLineChars + new string(c: ' ', count: indentLevel)
                );
            }
            baseWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            baseWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            baseWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            baseWriter.WriteEntityRef(name: name);
        }

        public override void WriteFullEndElement()
        {
            baseWriter.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            baseWriter.WriteProcessingInstruction(name: name, text: text);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            baseWriter.WriteRaw(buffer: buffer, index: index, count: count);
        }

        public override void WriteRaw(string data)
        {
            baseWriter.WriteRaw(data: data);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            baseWriter.WriteStartAttribute(prefix: prefix, localName: localName, ns: ns);
        }

        public override void WriteStartDocument()
        {
            baseWriter.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            baseWriter.WriteStartDocument(standalone: standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (isRootElement)
            {
                if (indentLevel < 0)
                {
                    // initialize the indent level;
                    indentLevel = 1;
                }
                else
                {
                    // do not track indent for the whole document;
                    // when second element starts, we are done
                    isRootElement = false;
                    indentLevel = -1;
                }
            }
            baseWriter.WriteStartElement(prefix: prefix, localName: localName, ns: ns);
        }

        public override void WriteString(string text)
        {
            baseWriter.WriteString(text: text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            baseWriter.WriteSurrogateCharEntity(lowChar: lowChar, highChar: highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            baseWriter.WriteWhitespace(ws: ws);
        }
        #endregion
    }

    public static IEnumerable<XmlNode> GetAllNodes(this XmlNode topNode)
    {
        foreach (var node in topNode.ChildNodes)
        {
            var xmlNode = (XmlNode)node;
            yield return xmlNode;
            foreach (var childNode in GetAllNodes(topNode: xmlNode))
            {
                yield return childNode;
            }
        }
    }

    public static int GetDepth(this XmlNode node)
    {
        int depth = 0;
        XmlNode parent = node.ParentNode;
        while (parent != null)
        {
            parent = parent.ParentNode;
            depth++;
        }
        return depth;
    }

    public static string ToBeautifulString(
        this XmlDocument document,
        XmlWriterSettings xmlWriterSettings
    )
    {
        var memoryStream = new MemoryStream();
        var writer = XmlnsIndentedWriter.Create(stream: memoryStream, settings: xmlWriterSettings);
        try
        {
            document.WriteContentTo(xw: writer);
            writer.Flush();
            memoryStream.Flush();
            memoryStream.Position = 0;
            var streamReader = new StreamReader(stream: memoryStream);
            return streamReader.ReadToEnd();
        }
        finally
        {
            memoryStream.Close();
        }
    }

    public static string ToBeautifulString(
        this XDocument document,
        XmlWriterSettings xmlWriterSettings
    )
    {
        MemoryStream mStream = new MemoryStream();
        XmlWriter writer = XmlWriter.Create(output: mStream, settings: xmlWriterSettings);
        try
        {
            document.Save(writer: writer);
            writer.Flush();
            mStream.Flush();
            mStream.Position = 0;
            StreamReader sReader = new StreamReader(stream: mStream);
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
            NewLineOnAttributes = true,
            DoNotEscapeUriAttributes = false,
        };
        // The Replace is necessary because extra new lines were added after
        // each attribute in the first node (the one with the namespaces).
        // This seems to only happen in .net8.0 not .net4.7.1
        return ToBeautifulString(document: document, xmlWriterSettings: xmlWriterSettings)
            .Replace(oldValue: "\r\n \r\n ", newValue: "\r\n ");
    }

    public static string ToBeautifulString(this XDocument document)
    {
        var xmlWriterSettings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = true };
        return ToBeautifulString(document: document, xmlWriterSettings: xmlWriterSettings);
    }

    public static XmlDocument RemoveAllEmptyAttributesAndNodes(this XmlDocument doc)
    {
#if NETSTANDARD
        foreach (
            XmlAttribute att in doc.SelectNodes(xpath: "descendant::*/@*[not(normalize-space(.))]")
        )
        {
            att.OwnerElement.RemoveAttributeNode(oldAttr: att);
        }
        if (string.IsNullOrEmpty(value: doc.OuterXml))
        {
            return doc;
        }
        var elements = XDocument.Parse(text: doc.OuterXml);
        elements
            .Descendants()
            .Where(predicate: e => e.IsEmpty || string.IsNullOrWhiteSpace(value: e.Value))
            .Remove();
#endif
        return doc;
    }

    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using (var nodeReader = new XmlNodeReader(node: xmlDocument))
        {
            nodeReader.MoveToContent();
            return XDocument.Load(reader: nodeReader);
        }
    }

    public static bool AttributeIsFalseOrMissing(this XmlElement element, string attributeName)
    {
        var value = element.GetAttribute(name: attributeName);
        return value == "false" || string.IsNullOrEmpty(value: value);
    }
}
