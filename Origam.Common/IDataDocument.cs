using System;
using System.Data;
using System.Xml;

namespace Origam
{
    public interface IDataDocument: IXmlContainer
    {
        DataSet DataSet { get; }
        void AppendChild(XmlNodeType element, string prefix, string name);
        void AppendChild(XmlElement documentElement, bool deep);
        void DocumentElementAppendChild(XmlNode node);
    }

    public interface IXmlContainer: ICloneable
    {
        XmlDocument Xml { get; }
        void Load(XmlReader xmlReader);
        void LoadXml(string xmlString);
    }
}