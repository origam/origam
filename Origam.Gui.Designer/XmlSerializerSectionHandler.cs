using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Configuration;

namespace Origam.Gui.Designer
{
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
}
