using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MoreLinq;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.DA.Service;
public class OrigamXmlDocument : XmlDocument
{
    public bool IsEmpty => ChildNodes.Count < 2;
    public XmlElement FileElement => LastChild as XmlElement;
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
        XmlAttribute attributeToUpdate = FileElement.Attributes
            .Cast<XmlAttribute>()
            .FirstOrDefault(attr => attr.Value == nameSpace && attr.LocalName != nameSpaceName);
        if (attributeToUpdate != null)
        {
            FileElement.RemoveAttribute(attributeToUpdate.Name);
        }
        FileElement.SetAttribute("xmlns:"+nameSpaceName, nameSpace);
        return nameSpaceName;
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
    public void UseTopNamespacePrefixesEverywhere()
    {
        foreach (XmlNode node in FileElement.GetAllNodes())
        {
            XmlAttribute namespaceDefinition = FileElement.Attributes
                .Cast<XmlAttribute>()
                .First(attr => attr.Value == node.NamespaceURI);
            if (node.Prefix != namespaceDefinition.LocalName)
            {
                node.Prefix = namespaceDefinition.LocalName;
            }
        }
    }
}
