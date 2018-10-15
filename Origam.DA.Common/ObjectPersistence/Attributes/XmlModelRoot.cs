using System.Xml.Serialization;
using Origam.OrigamEngine;

namespace Origam.DA.ObjectPersistence
{
    public class XmlModelRoot: XmlRootAttribute
    {
        public XmlModelRoot(string elementName) : base(elementName)
        {
            Namespace = "http://schemas.origam.com/" +
                        VersionProvider.CurrentModelMeta + "/model-element";
        }
    }
}