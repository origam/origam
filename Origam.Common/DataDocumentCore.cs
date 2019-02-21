using System.Data;
using System.Xml;

namespace Origam
{
    public class DataDocumentCore : IDataDocument
    {
        private DataSet dataSet;

        public DataDocumentCore()
        {
            dataSet = new DataSet();
        }

        public DataDocumentCore(DataSet dataSet)
        {
            this.dataSet = dataSet;
        }

        public DataDocumentCore(XmlDocument xmlDocument)
        {
            WriteToDataSet(xmlDocument);
        }

        private void WriteToDataSet(XmlDocument xmlDocument)
        {
            dataSet = new DataSet();
            using (XmlReader xmlReader = new XmlNodeReader(xmlDocument))
            {
                dataSet.ReadXml(xmlReader);
            }
        }

        public XmlDocument Xml
        {
            get
            {
                XmlDocument xmlDocument = new XmlDocument();
                if (dataSet.Tables.Count != 0)
                {
                    xmlDocument.LoadXml(dataSet.GetXml());
                }
                return xmlDocument;
            }
        }

        public DataSet DataSet => dataSet;
        public void AppendChild(XmlNodeType element, string prefix, string name)
        {
            XmlDocument newDocument = Xml;
            XmlNode node = newDocument.CreateNode(element, prefix, name);
            newDocument.AppendChild(node);
            WriteToDataSet(newDocument);
        }

        public void AppendChild(XmlElement documentElement, bool deep)
        {
            XmlDocument newDocument = Xml;
            XmlNode node = newDocument.ImportNode(documentElement, deep);
            newDocument.AppendChild(node);
            WriteToDataSet(newDocument);
        }

        public void DocumentElementAppendChild(XmlNode node)
        {
            XmlDocument newDocument = Xml;
            XmlNode newNode = newDocument.ImportNode(node, true);
            newDocument.AppendChild(newNode);
            WriteToDataSet(newDocument);
        }

        public void Load(XmlReader xmlReader)
        {
            dataSet.ReadXml(xmlReader);
        }

        public void LoadXml(string xmlString)
        {
            XmlDocument newDocument = Xml;
            newDocument.LoadXml(xmlString);
            WriteToDataSet(newDocument);
        }

        public object Clone()
        {
            return new DataDocumentCore(dataSet);
        }
    }
}
