#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace Origam.DA.Service;

public class OrigamFileFactory : IOrigamFileFactory
{
    private readonly IList<string> defaultParentFolders;
    private readonly OrigamFileManager origamFileManager;
    private readonly OrigamPathFactory origamPathFactory;
    private readonly FileEventQueue fileEventQueue;

    public OrigamFileFactory(
        OrigamFileManager origamFileManager,
        IList<string> defaultParentFolders,
        OrigamPathFactory origamPathFactory,
        FileEventQueue fileEventQueue
    )
    {
        this.origamPathFactory = origamPathFactory;
        this.defaultParentFolders = defaultParentFolders;
        this.origamFileManager = origamFileManager;
        this.fileEventQueue = fileEventQueue;
    }

    public OrigamFile New(
        string relativePath,
        IDictionary<string, Guid> parentFolderIds,
        bool isGroup,
        bool isAFullyWrittenFile = false
    )
    {
        IDictionary<string, Guid> parentFolders = GetNonEmptyParentFolders(parentFolderIds);
        OrigamPath path = origamPathFactory.CreateFromRelative(relativePath);
        if (isGroup)
        {
            return new OrigamGroupFile(
                path,
                parentFolders,
                origamFileManager,
                origamPathFactory,
                fileEventQueue,
                isAFullyWrittenFile
            );
        }
        else
        {
            return new OrigamFile(
                path,
                parentFolders,
                origamFileManager,
                origamPathFactory,
                fileEventQueue,
                isAFullyWrittenFile
            );
        }
    }

    public ITrackeableFile New(
        FileInfo fileInfo,
        IDictionary<string, Guid> parentFolderIds,
        bool isAFullyWrittenFile = false
    )
    {
        IDictionary<string, Guid> parentFolders = GetNonEmptyParentFolders(parentFolderIds);
        OrigamPath path = origamPathFactory.Create(fileInfo);
        switch (fileInfo.Name)
        {
            case OrigamFile.ReferenceFileName:
                return new OrigamReferenceFile(path, parentFolders);
            case OrigamFile.GroupFileName:
                return new OrigamGroupFile(
                    path,
                    parentFolders,
                    origamFileManager,
                    origamPathFactory,
                    fileEventQueue,
                    isAFullyWrittenFile
                );
            default:
                return new OrigamFile(
                    path,
                    parentFolders,
                    origamFileManager,
                    origamPathFactory,
                    fileEventQueue,
                    isAFullyWrittenFile
                );
        }
    }

    private IDictionary<string, Guid> GetNonEmptyParentFolders(
        IDictionary<string, Guid> parentFolderIds
    )
    {
        IDictionary<string, Guid> parentFolders =
            parentFolderIds.Count == 0 ? new ParentFolders(defaultParentFolders) : parentFolderIds;
        return parentFolders;
    }

    public ITrackeableFile New(
        string relativePath,
        string fileHash,
        IDictionary<string, Guid> parentFolderIds
    )
    {
        OrigamPath path = origamPathFactory.CreateFromRelative(relativePath);
        switch (path.FileName)
        {
            case OrigamFile.ReferenceFileName:
                return new OrigamReferenceFile(path, parentFolderIds, fileHash);
            case OrigamFile.GroupFileName:
                return new OrigamGroupFile(
                    path,
                    parentFolderIds,
                    origamFileManager,
                    origamPathFactory,
                    fileEventQueue,
                    fileHash
                );
            default:
                return new OrigamFile(
                    path,
                    parentFolderIds,
                    origamFileManager,
                    origamPathFactory,
                    fileEventQueue,
                    fileHash
                );
        }
    }
}
