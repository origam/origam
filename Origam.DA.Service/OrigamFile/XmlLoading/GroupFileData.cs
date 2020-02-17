using System;
using System.Collections.Generic;

namespace Origam.DA.Service
{
    public class GroupFileData : ObjectFileData
    {
        public Guid GroupId { get; }
        public override Folder FolderToDetermineParentGroup => Folder.Parent;
        public override Folder FolderToDetermineReferenceGroup => Folder.Parent;
        public GroupFileData(IList<ElementName> parentFolders,XmlFileData xmlFileData,
            OrigamFileFactory origamFileFactory) :
            base(new ParentFolders(parentFolders), xmlFileData, origamFileFactory)
        {
            string groupIdStr = 
                xmlFileData
                    ?.XmlDocument
                    ?.SelectSingleNode("//g:group",xmlFileData.NamespaceManager)
                    ?.Attributes?[$"x:{OrigamFile.IdAttribute}"]
                    ?.Value 
                ?? throw new Exception($"Could not read group id from: {FileInfo.FullName}");
            
            GroupId = new Guid(groupIdStr);
        }
    }
}