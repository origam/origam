using System;
using System.Linq;
using System.Xml;

namespace Origam.DA.Service
{
    public class OrigamXmlDocument : XmlDocument
    {
        public bool IsEmpty => ChildNodes.Count < 2;
        public XmlElement FileElement => (XmlElement) ChildNodes[1];
        public OrigamXmlDocument(string pathToXml)
        {
            Load(pathToXml);
        }

        public OrigamXmlDocument()
        {
            string xml = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><x:file xmlns:x=\"{0}\" xmlns=\"{1}\" xmlns:p=\"{2}\"/>",
                OrigamFile.ModelPersistenceUri,
                OrigamFile.GroupUri,
                OrigamFile.PackageUri);
            LoadXml(xml);
        }

        public string GetNameSpaceByName(string xmlNameSpaceName)
        {
            if (IsEmpty) return null;
            return FileElement?.Attributes[xmlNameSpaceName]?.InnerText;
        }

        public string AddNamespace(string nameSpaceName, string nameSpace)
        {
            if (IsEmpty)
            {
                throw new Exception("Cannot add namespace to an empty document");
            }
            var nextNamespaceName = GetNextNamespaceName(nameSpaceName);
            FileElement.SetAttribute("xmlns:"+nextNamespaceName, nameSpace);
            return nextNamespaceName;
        }

        private string GetNextNamespaceName(string nameSpaceName)
        {
            string currentValue = FileElement.Attributes[nameSpaceName]?.InnerText;
            if (string.IsNullOrEmpty(currentValue))
            {
                return nameSpaceName;
            }
            
            var lastNamespaceSuffix = FileElement.Attributes
                .Cast<XmlAttribute>()
                .Select(attr => attr.Name)
                .Where(attrName => attrName.StartsWith(nameSpaceName))
                .Select(attrName =>
                    attrName.Substring(nameSpaceName.Length, attrName.Length- nameSpaceName.Length))
                .Last();
            int.TryParse(lastNamespaceSuffix, out int lastNamespaceNumber);
            int nextNamespaceNumber = lastNamespaceNumber + 1;
            return nameSpaceName + nextNamespaceNumber;
        }
    }
}