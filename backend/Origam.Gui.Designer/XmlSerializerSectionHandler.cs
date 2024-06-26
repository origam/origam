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
using System.Xml.Serialization;
using System.Configuration;

namespace Origam.Gui.Designer;
public class XmlSerializerSectionHandler : IConfigurationSectionHandler
{
	object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
	{
		XPathNavigator navigator1 = section.CreateNavigator();
		string text1 = (string) navigator1.Evaluate("string(@type)");
		Type type1 = Type.GetType(text1);
		XmlSerializer serializer1 = new XmlSerializer(type1);
		return serializer1.Deserialize(new XmlNodeReader(section));
	}
}
