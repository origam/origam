using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Origam.DA.Common;

namespace Origam.DA.Service
{
    public class OrigamXDocument
    {
        public XDocument XDocument { get; }

        public bool IsEmpty => XDocument.Root == null || !XDocument.Root.Descendants().Any();
        public XElement FileElement => XDocument.Root;
        public IEnumerable<OrigamNameSpace> Namespaces => GetNamespaces(XDocument);
        
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpaceShortCut"> example: "asi"</param>
        /// <param name="nameSpace">example: "http://schemas.origam.com/Origam.Schema.AbstractSchemaItem/6.0.0"</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string AddNamespace(string nameSpaceShortCut, string nameSpace)
        {
            if (IsEmpty)
            {
                throw new Exception("Cannot add namespace to an empty document");
            }
            var nextNamespaceName = GetNextNamespaceName(nameSpaceShortCut, nameSpace);
            FileElement.SetAttributeValue(
                XName.Get( nextNamespaceName, "http://www.w3.org/2000/xmlns/"),
                nameSpace);
            return nextNamespaceName;
        }

        public static IEnumerable<OrigamNameSpace> GetNamespaces(XDocument doc)
        {
            return doc.Root
                .Attributes()
                .Select(attr => attr.Value)
                .Distinct()
                .Select(OrigamNameSpace.CreateOrGet);
        }

        private string GetNextNamespaceName(string nameSpaceName, string nameSpace)
        {
            string currentValue = XDocument.Root
                ?.Attributes(XName.Get(nameSpaceName, "http://www.w3.org/2000/xmlns/"))
                .FirstOrDefault()
                ?.Value;
            if (string.IsNullOrEmpty(currentValue) || currentValue == nameSpace)
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

        public void RenameNamespace(XNamespace oldNamespace, XNamespace updatedNamespace)
        {
            XAttribute namespaceAttribute = FileElement
                .Attributes()
                .First(attr => attr.Value == oldNamespace);
            namespaceAttribute.Remove();
            FileElement.Add(
                new XAttribute(XNamespace.Xmlns + namespaceAttribute.Name.LocalName, 
                    updatedNamespace));
            
            foreach (XElement el in FileElement.Descendants())  
            {
                if (el.Name.Namespace == oldNamespace)
                {
                    el.Name = updatedNamespace.GetName(el.Name.LocalName);
                }

                el.Attributes()
                    .Where(attr => attr.Name.Namespace == oldNamespace)
                    .ToList()
                    .ForEach(attr =>
                    {
                        attr.Remove();
                        el.Add(new XAttribute(updatedNamespace.GetName(attr.Name.LocalName), attr.Value));
                    });
            } 
        }

        public void FixNamespaces()
        {
            var nodeNamespaces = FileElement
                .Descendants()
                .Select(node => node.Name.NamespaceName);

            var usedNamespaces = FileElement
                .Descendants()
                .Attributes()
                .Select(attr => attr.Name.NamespaceName)
                .Concat(nodeNamespaces)
                .Distinct()
                .ToArray();
            
            FileElement
                .Attributes()
                .Where(attr => !usedNamespaces.Contains(attr.Value))
                .Remove();
        }

        public XNamespace FindClassNamespace(string fullTypeName)
        {
            return 
               FileElement
                   .Attributes()
                   .Select(attr => attr.Value)
                   .FirstOrDefault(nameSpace => nameSpace.Contains(fullTypeName))
               ?? throw new Exception($"Could not find namespace for class {fullTypeName} in {XDocument}");
        }
    }
}