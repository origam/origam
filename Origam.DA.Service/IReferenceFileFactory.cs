using System;
using System.Collections.Generic;
using System.IO;

namespace Origam.DA.Service
{
    public interface IReferenceFileFactory
    {
        ITrackeableFile New(FileInfo fileInfo, IDictionary<ElementName, Guid> parentFolderIds, bool isAFullyWrittenFile = false);
    }
}