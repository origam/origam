using System;
using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;

namespace Origam.DA.Service
{
    public interface ITrackeableFile
    {
        IDictionary<Guid, PersistedObjectInfo> ContainedObjects { get; }
        OrigamPath Path { get; set; }
        string FileHash { get;}
        ParentFolders ParentFolderIds { get; }
    }
}