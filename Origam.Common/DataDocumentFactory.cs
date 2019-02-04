using System;
using System.Data;
using System.Xml;

namespace Origam
{
    public class DataDocumentFactory
    {
        public static IDataDocument New(XmlDocument xmlDoc)
        {
#if NETSTANDARD
            return new DataDocumentCore(xmlDoc);
# else
            return new DataDocumentFx(xmlDoc);
#endif
        }
        public static IDataDocument New(DataSet dataSet)
        {
#if NETSTANDARD
            return new DataDocumentCore(dataSet);
# else
            return new DataDocumentFx(dataSet);
#endif
        }
    }
}