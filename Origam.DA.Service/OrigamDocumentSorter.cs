using System.Linq;
using System.Xml;
using MoreLinq;

namespace Origam.DA.Service
{
    public static class OrigamDocumentSorter
    {
        public static XmlDocument CopyAndSort(XmlDocument doc)
        {
            var newDoc = OrigamXmlManager.NewDocument();
            doc.ChildNodes
                .Cast<XmlNode>()
                .OrderBy(node => node.Name)
                .ForEach(node => CopyNodes(node, newDoc.ChildNodes[1], newDoc));
            return newDoc;
        }

        private static void CopyNodes(XmlNode node, XmlNode targetNode, XmlDocument newDoc)
        {
            node.ChildNodes
                .Cast<XmlNode>()
                .OrderBy(childNode => node.Name)
                .ForEach(childNode =>
                {
                    var xmlns = string.IsNullOrEmpty(childNode.NamespaceURI)
                        ? newDoc.ChildNodes[1].Attributes["xmlns"].Value
                        : childNode.NamespaceURI;
                    XmlElement childCopy = newDoc.CreateElement(childNode.Name, xmlns);
                    CopyAttributes(childNode, childCopy);
                    targetNode.AppendChild(childCopy);
                    CopyNodes(childNode, childCopy, newDoc);
                });
        }

        private static void CopyAttributes(XmlNode childNode, XmlElement childCopy)
        {
            childNode?.Attributes
                ?.Cast<XmlAttribute>()
                .OrderBy(attr => attr.LocalName) 
                .ForEach(attr =>
                    childCopy.SetAttribute(
                        localName: attr.LocalName,
                        namespaceURI: attr.NamespaceURI,
                        value: attr.Value));
        }
    }
}
