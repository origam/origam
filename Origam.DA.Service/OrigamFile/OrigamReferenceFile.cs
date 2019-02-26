using System;
using System.Collections.Generic;
using System.IO;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class OrigamReferenceFile : ITrackeableFile
    {
        public OrigamReferenceFile(OrigamPath origamPath, IDictionary<ElementName, Guid> parentFolders)
        {
            Path = origamPath;
            FileHash = new FileInfo(Path.Absolute).GetFileBase64Hash();
            ParentFolderIds = new ParentFolders(parentFolders, origamPath);
        }

        public OrigamReferenceFile(OrigamPath origamPath, List<ElementName> parentFolders)
        {
            Path = origamPath;
            FileHash = new FileInfo(Path.Absolute).GetFileBase64Hash();
            ParentFolderIds = new ParentFolders(parentFolders);
        }

        public OrigamReferenceFile(OrigamPath origamPath, IDictionary<ElementName, Guid> parentFolderIds, string fileHash)
        {
            Path = origamPath;
            ParentFolderIds = new ParentFolders(parentFolderIds, origamPath);
            FileHash = fileHash;
        }

        public IDictionary<Guid, PersistedObjectInfo> ContainedObjects { get; } =
            new Dictionary<Guid, PersistedObjectInfo>();
        public OrigamPath Path { get; set; }
        public string FileHash { get; }
        public ParentFolders ParentFolderIds { get; }
    }
}