using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class OrigamXmlDocument : XmlDocument
    {
        public bool IsEmpty => ChildNodes.Count < 2;
        public XmlElement FileElement => (XmlElement) ChildNodes[1];
        
        private readonly Dictionary<string,string> namespacesToRename 
            = new Dictionary<string, string>();
        
        public IEnumerable<OrigamNameSpace> Namespaces =>   
            FileElement.Attributes
                .Cast<XmlAttribute>()
                .Select(attr => attr.Value)
                .Distinct()
                .Select(OrigamNameSpace.Create);
        
        public IEnumerable<XmlNode> ClassNodes => this.GetAllNodes().Skip(2);
        public OrigamXmlDocument(string pathToXml)
        {
            Load(pathToXml);
        }

        public OrigamXmlDocument()
        {
            string xml = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><x:file xmlns:x=\"{0}\"/>",
                OrigamFile.ModelPersistenceUri);
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

        public void AddNamespaceForRenaming(string oldNamespace, string updatedNamespace)
        {
            namespacesToRename.Add(oldNamespace, updatedNamespace);
        }

        public void FixNamespaces()
        {
            RenameNamespaces();
            RemoveUnusedNamespaces();
        }

        private void RenameNamespaces()
        {
            string xmlString = OuterXml;
            foreach (var pair in namespacesToRename)
            {
                xmlString = xmlString.Replace(pair.Key, pair.Value);
            }

            LoadXml(xmlString);
        }

        private void RemoveUnusedNamespaces()
        {
            foreach (var nameSpaceAttribute in FileElement.Attributes.ToArray<XmlAttribute>())
            {
                bool namespaceIsUsed = this.GetAllNodes()
                    .Any(node => NamespaceIsUsed(node, nameSpaceAttribute));

                if (!namespaceIsUsed)
                {
                    FileElement.Attributes.Remove(nameSpaceAttribute);
                }
            }
        }

        private static bool NamespaceIsUsed(XmlNode node, XmlAttribute nameSpaceAttribute)
        {
            return node.NamespaceURI == nameSpaceAttribute.Value ||
                   (node.Attributes != null &&
                    node.Attributes
                        .Cast<XmlAttribute>()
                        .Any(attr => attr.NamespaceURI == nameSpaceAttribute.Value));
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

    public class OrigamXDocument
    {
        public XDocument XDocument { get; }

        public bool IsEmpty => XDocument.Root == null || !XDocument.Root.Descendants().Any();
        public XElement FileElement => XDocument.Root;

        // public IEnumerable<OrigamNameSpace> Namespaces =>   
        //     FileElement.Attributes
        //         .Cast<XmlAttribute>()
        //         .Select(attr => attr.Value)
        //         .Distinct()
        //         .Select(OrigamNameSpace.Create);
        
        public IEnumerable<XElement> ClassNodes => XDocument.Root.Descendants();
        public OrigamXDocument()
        {
            string xml = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><x:file xmlns:x=\"{0}\"/>",
                OrigamFile.ModelPersistenceUri);
            
            using (var reader = new XmlTextReader(xml))
            {
                reader.MoveToContent();
                XDocument = XDocument.Load(reader);
            }
        }

        public OrigamXDocument(XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                XDocument = XDocument.Load(nodeReader);
            }
        }

        public OrigamXDocument(FileInfo file)
        {
            XDocument = XDocument.Load(file.FullName);
        }

        public string AddNamespace(string nameSpaceName, string nameSpace)
        {
            if (IsEmpty)
            {
                throw new Exception("Cannot add namespace to an empty document");
            }
            var nextNamespaceName = GetNextNamespaceName(nameSpaceName);
            FileElement.SetAttributeValue(
                XName.Get( nextNamespaceName, "http://www.w3.org/2000/xmlns/"),
                nameSpace);
            return nextNamespaceName;
        }
        
        private string GetNextNamespaceName(string nameSpaceName)
        {
            string currentValue = XDocument.Root
                ?.Attributes(XName.Get(nameSpaceName, "http://www.w3.org/2000/xmlns/"))
                .FirstOrDefault()
                ?.Value;
            if (string.IsNullOrEmpty(currentValue))
            {
                return nameSpaceName;
            }
            
            var lastNamespaceSuffix = FileElement
                .Attributes()
                .Select(attr => attr.Name.LocalName)
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