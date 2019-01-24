using System.Data;
using System.Xml;

namespace Origam
{
    public interface IDataDocument
    {
          XmlDocument Xml { get; }
          DataSet DataSet { get; }
          void Load(XmlNodeReader xmlNodeReader);

         IDataDocument Clone();
    }
}