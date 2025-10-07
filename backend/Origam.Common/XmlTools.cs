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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Origam;

/// <summary>
/// Summary description for XmlTools.
/// </summary>
public class XmlTools
{
    public const string XslNameSpace = "http://www.w3.org/1999/XSL/Transform";
    public const string AsNameSpace = "http://schema.advantages.cz/AsapFunctions";

    private XmlTools() { }

    public static string XPathArgToString(object arg)
    {
        string s = arg as String;
        if (s != null)
            return s;
        XPathNodeIterator xpni = arg as XPathNodeIterator;
        if (xpni != null)
        {
            if (xpni.MoveNext())
            {
                return xpni.Current.Value;
            }
            else
            {
                return "";
            }
        }
        return ConvertToString(arg);
    }

    public static string ConvertToString(object val)
    {
        if (val == null)
        {
            return null;
        }
        if (val is string)
        {
            return (string)val;
        }
        else if (val is double)
        {
            return XmlConvert.ToString((double)val);
        }
        else if (val is int)
        {
            return XmlConvert.ToString((int)val);
        }
        else if (val is bool)
        {
            return XmlConvert.ToString((bool)val);
        }
        else if (val is DateTime)
        {
            return XmlConvert.ToString((DateTime)val, XmlDateTimeSerializationMode.Unspecified);
        }
        else if (val is decimal)
        {
            return XmlConvert.ToString((decimal)val);
        }
        else if (val is byte[])
        {
            return Convert.ToBase64String((byte[])val);
        }
        else if (val is IList)
        {
            return Strings.ArrayCannotBeConverted;
        }
        else
        {
            return val.ToString();
        }
    }

    /// <summary>
    /// Returns current iterator's node with all its child nodes and also parent nodes,
    /// excluding any other parent's children.
    /// </summary>
    /// <param name="iter"></param>
    /// <returns></returns>
    public static XmlDocument GetXmlSlice(XPathNodeIterator iter)
    {
        XmlDocument result = new XmlDocument();
        var nodes = new List<XmlNode>();
        // get a copy of a navigator, so we can traverse up to parents
        XPathNavigator parentNavigator = iter.Current.Clone();

        // we have to remember previous node, so we can skip importing it when importing parent
        XmlNode previousNode = ((IHasXmlNode)iter.Current).GetNode();
        while (parentNavigator.MoveToParent())
        {
            XmlNode currentNode = ((IHasXmlNode)parentNavigator).GetNode();
            // skip the topmost element
            if (!(currentNode is XmlDocument))
            {
                XmlNode currentNodeCopy = result.ImportNode(currentNode, false);
                // now get all nodes except of any node with a name of the previous node
                foreach (XmlNode childNode in (currentNode.ChildNodes))
                {
                    if (childNode.Name != previousNode.Name)
                    {
                        currentNodeCopy.AppendChild(result.ImportNode(childNode, true));
                    }
                }
                nodes.Add(currentNodeCopy);

                previousNode = currentNode;
            }
        }
        // now go in reverse order and import all nodes from the root up to our element
        XmlNode currentElement = result;
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            currentElement = currentElement.AppendChild(nodes[i]);
        }
        // finally add our current element and all its children (deep copy)
        XmlNode deepCopy = result.ImportNode(((IHasXmlNode)iter.Current).GetNode(), true);
        currentElement.AppendChild(deepCopy);
        return result;
    }

    public static IList<string> ResolveTransformationParameters(string transformationText)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(transformationText);
        return ResolveTransformationParameters(doc);
    }

    public static IList<string> ResolveTransformationParameters(XmlDocument doc)
    {
        return ResolveTransformationParameterElements(doc)
            .Select(node => node.GetAttribute("name"))
            .ToList();
    }

    public static IList<XmlElement> ResolveTransformationParameterElements(XmlDocument doc)
    {
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("xsl", XslNameSpace);
        return doc.SelectNodes("/xsl:stylesheet/xsl:param", nsmgr).Cast<XmlElement>().ToList();
    }

    public static string FormatXmlString(object value)
    {
        if (value is DateTime)
        {
            return FormatXmlDateTime((DateTime)value);
        }
        else if (value is decimal)
        {
            return XmlConvert.ToString((decimal)value);
        }
        else if (value is float)
        {
            return XmlConvert.ToString((float)value);
        }
        else if (value is double)
        {
            return XmlConvert.ToString((double)value);
        }
        else if (value is bool)
        {
            return XmlConvert.ToString((bool)value);
        }
        else if (value == null || value == DBNull.Value)
        {
            return String.Empty;
        }
        else
        {
            return value.ToString();
        }
    }

    public static string FormatXmlDateTime(DateTime date)
    {
        if (date.Hour == 0 & date.Minute == 0 & date.Second == 0 & date.Millisecond == 0)
        {
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(date);
            int hours = offset.Duration().Hours;
            string sign = hours >= 0 ? "+" : "-";
            string result =
                date.ToString("yyyy-MM-dd")
                + "T00:00:00.0000000"
                + sign
                + hours.ToString("00")
                + ":"
                + offset.Minutes.ToString("00");
            return result;
        }
        else
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }
    }
}
