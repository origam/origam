using System;
using System.Data;
using System.Xml;

namespace Origam
{
    public interface IDataDocument: IXmlContainer
    {
         DataSet DataSet { get; }
    }

    public interface IXmlContainer: ICloneable
    {
        XmlDocument Xml { get; }
        void Load(XmlNodeReader xmlNodeReader);
    }
}