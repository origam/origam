using System.Data;
using System.Xml;

namespace Origam
{
    public class DataDocumentCore : IDataDocument
    {
        private readonly DataSet dataSet;

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
        public void Load(XmlNodeReader xmlNodeReader)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlNodeReader);
            using (XmlReader xmlReader = new XmlNodeReader(xmlDocument))
            {
                dataSet.ReadXml(xmlReader);
            }

        }

        public object Clone()
        {
            return new DataDocumentCore(dataSet);
        }
    }
}
