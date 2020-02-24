using System.Xml;

namespace Origam.DA.Service
{
    public class OrigamXmlDocument : XmlDocument
    {
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
            return ChildNodes[1]?.Attributes?[xmlNameSpaceName]?.InnerText;
        }

        public bool IsEmpty => ChildNodes.Count < 2;
    }
}