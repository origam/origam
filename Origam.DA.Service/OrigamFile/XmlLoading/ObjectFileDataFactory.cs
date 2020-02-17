using System.Collections.Generic;

namespace Origam.DA.Service
{
    public class ObjectFileDataFactory
    {
        private readonly OrigamFileFactory origamFileFactory;
        private readonly IList<ElementName> parentFolders;

        public ObjectFileDataFactory(OrigamFileFactory origamFileFactory, IList<ElementName> parentFolders)
        {
            this.origamFileFactory = origamFileFactory;
            this.parentFolders = parentFolders;
        }

        public PackageFileData NewPackageFileData(XmlFileData xmlData)
        {
            return new PackageFileData(parentFolders, xmlData, origamFileFactory); 
        }

        public GroupFileData NewGroupFileData(XmlFileData xmlData)
        {
            return new GroupFileData(parentFolders, xmlData, origamFileFactory);   
        }
        public ObjectFileData NewObjectFileData(XmlFileData xmlData)
        {
            return new ObjectFileData(new ParentFolders(parentFolders), xmlData, origamFileFactory); 
        }

        public ReferenceFileData NewReferenceFileData(XmlFileData xmlData)
        {
            return new ReferenceFileData(xmlData, origamFileFactory);
        }
    }
}