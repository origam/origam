using System.Data;
using System.Xml;

namespace Origam
{
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