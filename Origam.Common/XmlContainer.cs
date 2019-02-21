using System.Net.Mime;
using System.Xml;

namespace Origam
{
    public class XmlContainer : IXmlContainer
    {
        public XmlContainer()
        {
            Xml = new XmlDocument();
        }

        public XmlContainer(XmlDocument xmlDocument)
        {
            Xml = xmlDocument;
        }

        public XmlContainer(string xmlString)
        {
            Xml = new XmlDocument();
            if (!string.IsNullOrEmpty(xmlString))
            {
                Xml.LoadXml(xmlString);
            }
        }

        public XmlDocument Xml { get; }

        public void Load(XmlReader xmlReader)
        {
            Xml.Load(xmlReader);
        }

        public void LoadXml(string xmlString)
        {
            Xml.LoadXml(xmlString);
        }

        public object Clone()
        {
            return Xml.Clone();
        }
    }
}