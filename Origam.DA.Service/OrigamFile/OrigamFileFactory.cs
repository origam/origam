using System;
using System.Collections.Generic;
using System.IO;

namespace Origam.DA.Service
{
    public class OrigamFileFactory : IOrigamFileFactory
    {
        private readonly IList<ElementName> defaultParentFolders;
        private readonly OrigamFileManager origamFileManager;
        private readonly OrigamPathFactory origamPathFactory;
        private readonly FileEventQueue fileEventQueue;

        public OrigamFileFactory(
            OrigamFileManager origamFileManager, IList<ElementName> defaultParentFolders,
            OrigamPathFactory origamPathFactory,FileEventQueue fileEventQueue)
        {
            this.origamPathFactory = origamPathFactory;
            this.defaultParentFolders = defaultParentFolders;

            this.origamFileManager = origamFileManager;
            this.fileEventQueue = fileEventQueue;
        }
        
        public OrigamFile New(string  relativePath, IDictionary<ElementName,Guid> parentFolderIds,
            bool isGroup, bool isAFullyWrittenFile=false)
        {
            IDictionary<ElementName, Guid> parentFolders =
                GetNonEmptyParentFolders(parentFolderIds);

            OrigamPath path = origamPathFactory.CreateFromRelative(relativePath);

            if (isGroup)
            {
                return new OrigamGroupFile( path,  parentFolders,  
                    origamFileManager,origamPathFactory,fileEventQueue, isAFullyWrittenFile);
            } 
            else
            {
                return new OrigamFile( path,  parentFolders, origamFileManager,
                    origamPathFactory, fileEventQueue,isAFullyWrittenFile);  
            }
        }
        
        public ITrackeableFile New(FileInfo fileInfo, IDictionary<ElementName,Guid> parentFolderIds,
            bool isAFullyWrittenFile=false)
        {
            IDictionary<ElementName, Guid> parentFolders =
                GetNonEmptyParentFolders(parentFolderIds);

            OrigamPath path = origamPathFactory.Create(fileInfo);

            switch (fileInfo.Name)
            {
                case OrigamFile.ReferenceFileName:
                    return new OrigamReferenceFile(path, parentFolders);
                case OrigamFile.GroupFileName:
                    return new OrigamGroupFile( path,  parentFolders,
                        origamFileManager,origamPathFactory,fileEventQueue, isAFullyWrittenFile);
                default:
                    return new OrigamFile( path,  parentFolders,
                        origamFileManager, origamPathFactory, fileEventQueue,
                        isAFullyWrittenFile);     
            }
        }

        private IDictionary<ElementName, Guid> GetNonEmptyParentFolders(IDictionary<ElementName, Guid> parentFolderIds)
        {
            IDictionary<ElementName, Guid> parentFolders = parentFolderIds.Count == 0
                ? new ParentFolders(defaultParentFolders)
                : parentFolderIds;
            return parentFolders;
        }

        public ITrackeableFile New(string relativePath, string fileHash,
            IDictionary<ElementName, Guid> parentFolderIds)
        {
            OrigamPath path = origamPathFactory.CreateFromRelative(relativePath);
            switch (path.FileName)
            {
                case OrigamFile.ReferenceFileName:
                    return new OrigamReferenceFile(path, parentFolderIds, fileHash);
                case OrigamFile.GroupFileName: 
                    return new OrigamGroupFile(path, parentFolderIds, 
                        origamFileManager, origamPathFactory, fileEventQueue, fileHash);
                default:
                    return new OrigamFile( path, parentFolderIds,
                        origamFileManager, origamPathFactory, fileEventQueue, fileHash);
            }
        }  
    }
}