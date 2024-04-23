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
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace Origam;

/// <summary>
/// Summary description for AsXmlTextWriter.
/// </summary>
public class AsXmlTextWriter : XmlTextWriter
{
	public AsXmlTextWriter(TextWriter tw) : base (tw)
	{
		}

	public void WriteNode(XPathNavigator navigator)
	{
			bool flag;
			if (navigator == null)
			{
				throw new ArgumentNullException("navigator");
			}
			int num = 0;
			navigator = navigator.Clone();
			Label_0018:
				flag = false;
			switch (navigator.NodeType)
			{
				case XPathNodeType.Root:
					flag = true;
					break;

				case XPathNodeType.Element:
					this.WriteStartElement(navigator.Prefix, navigator.LocalName, navigator.NamespaceURI);
					if (navigator.MoveToFirstAttribute())
					{
						do
						{
							this.WriteStartAttribute(navigator.Prefix, navigator.LocalName, navigator.NamespaceURI);
							this.WriteString(navigator.Value);
							this.WriteEndAttribute();
						}
						while (navigator.MoveToNextAttribute());
						navigator.MoveToParent();
					}
					if (navigator.MoveToFirstNamespace(XPathNamespaceScope.Local))
					{
						this.WriteLocalNamespaces(navigator);
						navigator.MoveToParent();
					}
					flag = true;
					break;

				case XPathNodeType.Text:
					this.WriteString(navigator.Value);
					break;

				case XPathNodeType.SignificantWhitespace:
				case XPathNodeType.Whitespace:
					this.WriteWhitespace(navigator.Value);
					break;

				case XPathNodeType.ProcessingInstruction:
					this.WriteProcessingInstruction(navigator.LocalName, navigator.Value);
					break;

				case XPathNodeType.Comment:
					this.WriteComment(navigator.Value);
					break;
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
			if (nsNav.MoveToNextNamespace(XPathNamespaceScope.Local))
			{
				this.WriteLocalNamespaces(nsNav);
			}
			if (localName.Length == 0)
			{
				this.WriteAttributeString(string.Empty, "xmlns", "http://www.w3.org/2000/xmlns/", str2);
			}
			else
			{
				this.WriteAttributeString("xmlns", localName, "http://www.w3.org/2000/xmlns/", str2);
			}
		}
}