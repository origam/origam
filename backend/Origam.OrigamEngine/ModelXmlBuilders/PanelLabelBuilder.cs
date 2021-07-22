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

using System.Xml;

namespace Origam.OrigamEngine.ModelXmlBuilders
{
	/// <summary>
	/// Summary description for TextBoxBuilder.
	/// </summary>
	public class PanelLabelBuilder
	{
		public static void Build(XmlElement childrenElement, string text, int top, int left, int height, int width)
		{
			XmlElement formElement = childrenElement.OwnerDocument.CreateElement("FormElement");
			childrenElement.AppendChild(formElement);

			formElement.SetAttribute("Type", "Label");
			formElement.SetAttribute("Title", text);

			formElement.SetAttribute("X", XmlConvert.ToString(left));
			formElement.SetAttribute("Y", XmlConvert.ToString(top));
			formElement.SetAttribute("Width", XmlConvert.ToString(width));
			formElement.SetAttribute("Height", XmlConvert.ToString(height));
		}
	}
}
