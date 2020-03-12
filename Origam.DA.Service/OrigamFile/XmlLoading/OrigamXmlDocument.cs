using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MoreLinq;
using Origam.DA.Common;
using Origam.Extensions;

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

        public OrigamXmlDocument(XDocument xDocument)
        {
            using(var xmlReader = xDocument.CreateReader())
            {
                Load(xmlReader);
            }
        }

        public OrigamXmlDocument()
        {
            string xml = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><x:file xmlns:x=\"{0}\"/>",
                OrigamFile.ModelPersistenceUri);
            LoadXml(xml);
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
        
        public void RemoveWithNamespace(XmlNode nodeToDelete)
        {
            nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
            bool moreNodesInTheNamespaceExist = 
                this.GetAllNodes()
                .Any(node => node.NamespaceURI == nodeToDelete.NamespaceURI);
            if (!moreNodesInTheNamespaceExist)
            {
                var namespaceToRemove = FileElement.Attributes
                    .Cast<XmlAttribute>()
                    .First(attr => attr.Value == nodeToDelete.NamespaceURI);
                FileElement.Attributes.Remove(namespaceToRemove);
            }
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