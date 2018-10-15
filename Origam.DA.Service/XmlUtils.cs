using System;
using System.Xml;

namespace Origam.DA.Service
{
    public static class XmlUtils
    {
        public static Guid? ReadId(XmlNode node)
        {
            return ReadGuid(node, OrigamFile.IdAttribute);
        }
        
        public static Guid? ReadParenId(XmlNode node)
        {
            return ReadGuid(node, OrigamFile.ParentIdAttribute);
        }
        
        public static Guid? ReadId(XmlReader xmlReader) =>
            ReadGuid(xmlReader,
                OrigamFile.IdAttribute,
                OrigamFile.ModelPersistenceUri);

        private static Guid? ReadGuid(XmlReader xmlReader, 
            string attrName, string attrNamespace)
        {
            string result = xmlReader.GetAttribute(
                attrName,
                attrNamespace);
            if (string.IsNullOrWhiteSpace(result))
            {
                return null;
            }
            else
            {
                return new Guid(result);
            }
        }
        
        private static Guid? ReadGuid(XmlNode node, string attrName)
        {
            if (node?.Attributes == null)  return null;
            XmlAttribute idAtt = 
                node.Attributes[attrName, OrigamFile.ModelPersistenceUri];
            Guid? id = null;
            if (idAtt != null)
            {
                id = new Guid(idAtt.Value);
            }

            return id;
        }

        public static string ReadType(XmlReader xmlReader) =>
            xmlReader.GetAttribute(
                OrigamFile.TypeAttribute,
                OrigamFile.ModelPersistenceUri);
    }
}