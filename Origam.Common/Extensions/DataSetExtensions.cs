using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Origam.Extensions
{
    public static class DataSetExtensions
    {
        public static XDocument ToXDocument(this DataSet dataSet)
        {
            using (var stream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8)
                    {Formatting = Formatting.None}
                )
                {
                    dataSet.WriteXml(xmlTextWriter);
                    stream.Position = 0;
                    var xmlReader = XmlReader.Create(stream);
                    xmlReader.MoveToContent();
                    return XDocument.Load(xmlReader);
                }
            }
        }
    }
}