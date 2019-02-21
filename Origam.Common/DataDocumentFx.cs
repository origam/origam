using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Origam
{
#if !NETSTANDARD
    public class DataDocumentFx : IDataDocument
    {
        private readonly XmlDataDocument xmlDataDocument;

        public DataDocumentFx(DataSet dataSet)
        {
            xmlDataDocument = new XmlDataDocument(dataSet);
        }

        public DataDocumentFx(XmlDocument xmlDoc)
        {
            xmlDataDocument = new XmlDataDocument();
            foreach (XmlNode childNode in xmlDoc.ChildNodes)
            {
                var importNode = xmlDataDocument.ImportNode(childNode, true);
                xmlDataDocument.AppendChild(importNode);
            }
        }

        public XmlDocument Xml => xmlDataDocument;

        public DataSet DataSet => xmlDataDocument.DataSet;
        public void AppendChild(XmlNodeType element, string prefix, string name)
        {
            XmlNode node = xmlDataDocument.CreateNode(element, prefix, name);
            xmlDataDocument.AppendChild(node);
        }

        public void AppendChild(XmlElement documentElement, bool deep)
        {
            XmlNode node = xmlDataDocument.ImportNode(documentElement, true);
            xmlDataDocument.AppendChild(node);
        }

        public void DocumentElementAppendChild(XmlNode node)
        {
            XmlNode newNode = xmlDataDocument.ImportNode(node, true);
            xmlDataDocument.DocumentElement.AppendChild(newNode);
        }

        public void Load(XmlReader xmlReader)
        {
            xmlDataDocument.Load(xmlReader);
        }

        public void LoadXml(string xmlString)
        {
            xmlDataDocument.LoadXml(xmlString);
        }

        public object Clone()
        {
            return new DataDocumentFx(xmlDataDocument);
        }
    }
#endif
}
