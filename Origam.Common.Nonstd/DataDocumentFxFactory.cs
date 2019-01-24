using System;
using System.Data;
using System.Xml;

namespace Origam
{
   
    public class DataDocumentFxFactory : IDataDocumentFactory
    {
        public IDataDocument New()
        {
            return new DataDocumentFx();
        }
        public IDataDocument New(XmlDocument xmlDoc)
        {
            return new DataDocumentFx(xmlDoc);
        }
        public IDataDocument New(DataSet dataSet)
        {
            return new DataDocumentFx(dataSet);
        }
    }
}