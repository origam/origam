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

        public XmlDocument Xml { get; }

        public void Load(XmlNodeReader xmlNodeReader)
        {
            Xml.Load(xmlNodeReader);
        }

        public object Clone()
        {
            return Xml.Clone();
        }
    }
}