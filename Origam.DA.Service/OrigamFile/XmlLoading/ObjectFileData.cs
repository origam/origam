using System;
using System.IO;
using System.Xml;

namespace Origam.DA.Service
{
    public class ObjectFileData
    {
        public virtual ParentFolders ParentFolderIds { get; }
        private readonly XmlFileData xmlFileData;
        public Folder Folder { get;}
        public FileInfo FileInfo => xmlFileData.FileInfo;
        public virtual Folder FolderToDetermineParentGroup => Folder;
        public virtual Folder FolderToDetermineReferenceGroup => Folder;
        private readonly IOrigamFileFactory origamFileFactory;
        
        public ObjectFileData(ParentFolders parentFolders, XmlFileData xmlFileData,
            IOrigamFileFactory origamFileFactory)
        {
            this.origamFileFactory = origamFileFactory;
            this.xmlFileData = xmlFileData
                               ?? throw new ArgumentNullException(nameof(xmlFileData));
            ParentFolderIds = parentFolders;
            Folder = new Folder(xmlFileData.FileInfo.DirectoryName);
        }
        public ITrackeableFile Read()
        {
            ITrackeableFile origamFile = origamFileFactory.New(
                fileInfo: xmlFileData.FileInfo,
                parentFolderIds: ParentFolderIds,
                isAFullyWrittenFile: true);

            Guid parentId = Guid.Empty;
            RecursiveNodeRead(xmlFileData.XmlDocument,parentId, origamFile);
            return origamFile;
        }

        private void RecursiveNodeRead(XmlNode currentNode, Guid parentNodeId,
            ITrackeableFile origamFile )
        {
            foreach (object nodeObj in currentNode.ChildNodes)
            {
                var childNode = (XmlNode) nodeObj;
                XmlAttribute idAttribute = childNode.Attributes?[$"x:{OrigamFile.IdAttribute}"];
                Guid currentNodeId; 
                if (idAttribute != null)
                {
                    Guid parentId = ParseParentId(parentNodeId, childNode);
                    bool isFolder = ParseIsFolder(childNode);
                    currentNodeId = new Guid(idAttribute.Value);
                    var objectInfo = new PersistedObjectInfo(
                        elementName: ElementNameFactory.Create(childNode),
                        id:currentNodeId ,
                        parentId: parentId,
                        isFolder: isFolder,
                        origamFile: (OrigamFile)origamFile);

                    if (origamFile.ContainedObjects.ContainsKey(objectInfo.Id))
                    {
                        throw new InvalidOperationException("Duplicate object with id: "+objectInfo.Id+" in: "+origamFile.Path.Relative);
                    }
                    
                    origamFile.ContainedObjects.Add(objectInfo.Id, objectInfo);
                } 
                else
                {
                    currentNodeId = Guid.Empty;
                }
                RecursiveNodeRead(childNode, currentNodeId, origamFile);
            }
        }

        private static bool ParseIsFolder(XmlNode childNode)
        {
            bool.TryParse(
                childNode?.Attributes?[$"x:{OrigamFile.IsFolderAttribute}"]?.Value,
                out bool isFolder);
            return isFolder;
        }

        private static Guid ParseParentId(Guid parentNodeId, XmlNode childNode)
        {
            XmlAttribute nodeAttribute = childNode?.Attributes?[$"x:{OrigamFile.ParentIdAttribute}"];
            Guid parentId = nodeAttribute != null
                ? new Guid(nodeAttribute.Value)
                : parentNodeId;
            return parentId;
        }
    }
}