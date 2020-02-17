using System;
using System.Collections.Generic;

namespace Origam.DA.Service
{
    public class PackageFileData: ObjectFileData
    {
        public Guid PackageId { get; }

        public PackageFileData(IList<ElementName> parentFolders,XmlFileData xmlFileData, 
            OrigamFileFactory origamFileFactory) :
            base(new ParentFolders(parentFolders),xmlFileData, origamFileFactory)
        {
            string idStr = xmlFileData
                               ?.XmlDocument
                               ?.SelectSingleNode("//p:package", xmlFileData.NamespaceManager)
                               ?.Attributes?[$"x:{OrigamFile.IdAttribute}"]
                               ?.Value
                           ?? throw new Exception($"Could not read package id form file: {xmlFileData.FileInfo.FullName}");
            PackageId = new Guid(idStr);
        }
    }
}