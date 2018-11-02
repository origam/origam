using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Origam
{
    public class DataDocument : IDataDocument
    {
        private readonly XmlDataDocument xmlDataDocument;

        public DataDocument()
        {
            xmlDataDocument = new XmlDataDocument();
        }

        public DataDocument(DataSet dataSet)
        {
            xmlDataDocument = new XmlDataDocument(dataSet);
        }

        public DataDocument(XmlDocument xmlDoc)
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
        public void Load(XmlNodeReader xmlNodeReader)
        {
            xmlDataDocument.Load(xmlNodeReader);
        }

        public IDataDocument Clone()
        {
            return new DataDocument(xmlDataDocument);
        }
    }

    public class DataDocumentFactory
    {
        public static IDataDocument New()
        {
            return new DataDocument();
        }
        public static IDataDocument New(XmlDocument xmlDoc)
        {
            return new DataDocument(xmlDoc);
        }
        public static IDataDocument New(DataSet dataSet)
        {
            return new DataDocument(dataSet);
        }
    }
}
