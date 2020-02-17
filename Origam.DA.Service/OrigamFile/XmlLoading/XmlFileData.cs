using System.IO;
using System.Xml;

namespace Origam.DA.Service
{
    public class XmlFileData
    {
        public XmlDocument XmlDocument { get; }
        public XmlNamespaceManager NamespaceManager{ get; }
        public FileInfo FileInfo { get;}

        public XmlFileData(XmlDocument xmlDocument, FileInfo fileInfo)
        {
            XmlDocument = xmlDocument;
            FileInfo = fileInfo;
            NamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);       
            NamespaceManager.AddNamespace("x",OrigamFile.ModelPersistenceUri);
            NamespaceManager.AddNamespace("p",OrigamFile.PackageUri);
            NamespaceManager.AddNamespace("g",OrigamFile.GroupUri);
        }
    }
}