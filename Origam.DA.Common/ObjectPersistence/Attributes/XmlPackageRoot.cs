using System.Xml.Serialization;
using Origam.OrigamEngine;

namespace Origam.DA.ObjectPersistence
{
    public class XmlPackageRoot: XmlRootAttribute
    {
        public XmlPackageRoot(string elementName) : base(elementName)
        {
            Namespace = "http://schemas.origam.com/"+VersionProvider.CurrentPackageMeta+"/package";
        }
    }
}