using System;
using System.Data;
using System.Xml;

namespace Origam
{
    public class DataDocumentFactory
    {
        private static IDataDocumentFactory internalFactory;

        public static IDataDocumentFactory GetFactory()
        {
            if (internalFactory == null)
            {
//                Type type = Type.GetType("Origam.DataDocumentFxFactory,Origam.NetFX");
//                internalFactory = type != null
//                    ? (IDataDocumentFactory)Activator.CreateInstance(type)
//                    : new DataDocumentCoreFactory();
                internalFactory= new DataDocumentCoreFactory();
            }

            return internalFactory;
        }

        public static IDataDocument New()
        {
            return GetFactory().New();
        }
        public static IDataDocument New(XmlDocument xmlDoc)
        {
            return GetFactory().New(xmlDoc);
        }
        public static IDataDocument New(DataSet dataSet)
        {
            return GetFactory().New(dataSet);
        }
    }

    public interface IDataDocumentFactory
    {
        IDataDocument New();
        IDataDocument New(XmlDocument xmlDoc);
        IDataDocument New(DataSet dataSet);
    }

    public class DataDocumentCoreFactory : IDataDocumentFactory
    {
        public IDataDocument New()
        {
            return new DataDocumentCore();
        }
        public IDataDocument New(XmlDocument xmlDoc)
        {
            return new DataDocumentCore(xmlDoc);
        }
        public IDataDocument New(DataSet dataSet)
        {
            return new DataDocumentCore(dataSet);
        }
    }
}