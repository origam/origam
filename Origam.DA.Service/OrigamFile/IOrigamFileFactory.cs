using System;
using System.Collections.Generic;
using System.IO;

namespace Origam.DA.Service
{
    public interface IOrigamFileFactory
    {
        ITrackeableFile New(FileInfo fileInfo, IDictionary<ElementName, Guid> parentFolderIds, bool isAFullyWrittenFile = false);
        OrigamFile New(string relativePath, IDictionary<ElementName, Guid> parentFolderIds, bool isGroup, bool isAFullyWrittenFile = false);
        ITrackeableFile New(string relativePath, string fileHash, IDictionary<ElementName, Guid> parentFolderIds);
    }
}