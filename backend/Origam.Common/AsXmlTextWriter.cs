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
using System.Xml.XPath;

namespace Origam;

/// <summary>
/// Summary description for AsXmlTextWriter.
/// </summary>
public class AsXmlTextWriter : XmlTextWriter
{
    public AsXmlTextWriter(TextWriter tw)
        : base(w: tw) { }

    public void WriteNode(XPathNavigator navigator)
    {
        bool flag;
        if (navigator == null)
        {
            throw new ArgumentNullException(paramName: "navigator");
        }
        int num = 0;
        navigator = navigator.Clone();
        Label_0018:
        flag = false;
        switch (navigator.NodeType)
        {
            case XPathNodeType.Root:
            {
                flag = true;
                break;
            }

            case XPathNodeType.Element:
            {
                this.WriteStartElement(
                    prefix: navigator.Prefix,
                    localName: navigator.LocalName,
                    ns: navigator.NamespaceURI
                );
                if (navigator.MoveToFirstAttribute())
                {
                    do
                    {
                        this.WriteStartAttribute(
                            prefix: navigator.Prefix,
                            localName: navigator.LocalName,
                            ns: navigator.NamespaceURI
                        );
                        this.WriteString(text: navigator.Value);
                        this.WriteEndAttribute();
                    } while (navigator.MoveToNextAttribute());
                    navigator.MoveToParent();
                }
                if (navigator.MoveToFirstNamespace(namespaceScope: XPathNamespaceScope.Local))
                {
                    this.WriteLocalNamespaces(nsNav: navigator);
                    navigator.MoveToParent();
                }
                flag = true;
                break;
            }

            case XPathNodeType.Text:
            {
                this.WriteString(text: navigator.Value);
                break;
            }

            case XPathNodeType.SignificantWhitespace:
            case XPathNodeType.Whitespace:
            {
                this.WriteWhitespace(ws: navigator.Value);
                break;
            }

            case XPathNodeType.ProcessingInstruction:
            {
                this.WriteProcessingInstruction(name: navigator.LocalName, text: navigator.Value);
                break;
            }

            case XPathNodeType.Comment:
            {
                this.WriteComment(text: navigator.Value);
                break;
            }
        }
        if (flag)
        {
            if (navigator.MoveToFirstChild())
            {
                num++;
                goto Label_0018;
            }
            if (navigator.NodeType == XPathNodeType.Element)
            {
                if (navigator.IsEmptyElement)
                {
                    this.WriteEndElement();
                }
                else
                {
                    this.WriteFullEndElement();
                }
            }
        }
        while (num != 0)
        {
            if (navigator.MoveToNext())
            {
                goto Label_0018;
            }
            num--;
            navigator.MoveToParent();
            if (navigator.NodeType == XPathNodeType.Element)
            {
                this.WriteFullEndElement();
            }
        }
    }

    private void WriteLocalNamespaces(XPathNavigator nsNav)
    {
        string localName = nsNav.LocalName;
        string str2 = nsNav.Value;
        if (nsNav.MoveToNextNamespace(namespaceScope: XPathNamespaceScope.Local))
        {
            this.WriteLocalNamespaces(nsNav: nsNav);
        }
        if (localName.Length == 0)
        {
            this.WriteAttributeString(
                prefix: string.Empty,
                localName: "xmlns",
                ns: "http://www.w3.org/2000/xmlns/",
                value: str2
            );
        }
        else
        {
            this.WriteAttributeString(
                prefix: "xmlns",
                localName: localName,
                ns: "http://www.w3.org/2000/xmlns/",
                value: str2
            );
        }
    }
}
