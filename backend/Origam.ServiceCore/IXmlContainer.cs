using System;
using System.Xml;

namespace Origam.ServiceCore
{
    public interface IXmlContainer: ICloneable
    {
        XmlDocument Xml { get; }
        void Load(XmlReader xmlReader,bool doProcessing = true);
        void LoadXml(string xmlString);
        void DocumentElementAppendChild(XmlNode node);
    }
}