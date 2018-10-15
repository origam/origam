using System;
using System.Collections.Generic;

namespace Origam.DA.ObjectPersistence
{
    public interface IFilePersistent : IPersistent
    {
        string RelativeFilePath { get; }
        Guid FileParentId { get; set; }
        bool IsFolder { get; }
        IDictionary<ElementName, Guid> ParentFolderIds { get; }
        string Path { get; }
        bool IsFileRootElement { get; }
    }
}
