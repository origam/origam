using System.Xml.Linq;

namespace Origam.DA.Service.NamespaceMapping;
internal interface IPropertyToNamespaceMapping
{
    XNamespace NodeNamespace { get; }
    XNamespace GetNamespaceByXmlAttributeName(string xmlAttributeName);
    void AddNamespacesToDocumentAndAdjustMappings(OrigamXDocument document);
}
