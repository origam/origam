using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Origam.Extensions
{
    public static class XmlExtensions
    {
        public static IEnumerable<XmlNode> GetAllNodes(this XmlNode topNode)
        {
            foreach (var node in topNode.ChildNodes)
            {
                var xmlNode = (XmlNode) node;
                yield return xmlNode;
                foreach (var childNode in GetAllNodes(xmlNode))
                {
                    yield return childNode;
                }
            }
        }
        
        public static int GetDepth(this XmlNode node)
        {
            int depth = 0;
            XmlNode parent = node.ParentNode;
            while (parent!=null)
            {
                parent = parent.ParentNode;
                depth++;
            }

            return depth;
        }
        
        public static string ToBeautifulString(this XmlDocument document, 
            XmlWriterSettings xmlWriterSettings)
        {
            MemoryStream mStream = new MemoryStream();
            XmlWriter writer =  XmlWriter.Create(mStream,xmlWriterSettings);
            try
            {
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();
                mStream.Position = 0;
                StreamReader sReader = new StreamReader(mStream);
                return sReader.ReadToEnd();
            }
            finally
            {
                mStream.Close();
            }
        }
    }
}