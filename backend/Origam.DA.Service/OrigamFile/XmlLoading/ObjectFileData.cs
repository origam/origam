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
using System.IO;
using System.Xml;
using Origam.DA.Common;

namespace Origam.DA.Service;

public class ObjectFileData
{
    public virtual ParentFolders ParentFolderIds { get; }
    private readonly XmlFileData xmlFileData;
    public Folder Folder { get; }
    public FileInfo FileInfo => xmlFileData.FileInfo;
    public virtual Folder FolderToDetermineParentGroup => Folder;
    public virtual Folder FolderToDetermineReferenceGroup => Folder;
    private readonly IOrigamFileFactory origamFileFactory;

    public ObjectFileData(
        ParentFolders parentFolders,
        XmlFileData xmlFileData,
        IOrigamFileFactory origamFileFactory
    )
    {
        this.origamFileFactory = origamFileFactory;
        this.xmlFileData = xmlFileData ?? throw new ArgumentNullException(nameof(xmlFileData));
        ParentFolderIds = parentFolders;
        Folder = new Folder(xmlFileData.FileInfo.DirectoryName);
    }

    public ITrackeableFile Read()
    {
        ITrackeableFile origamFile = origamFileFactory.New(
            fileInfo: xmlFileData.FileInfo,
            parentFolderIds: ParentFolderIds,
            isAFullyWrittenFile: true
        );
        Guid parentId = Guid.Empty;
        RecursiveNodeRead(xmlFileData.XmlDocument, parentId, origamFile);
        return origamFile;
    }

    private void RecursiveNodeRead(
        XmlNode currentNode,
        Guid parentNodeId,
        ITrackeableFile origamFile
    )
    {
        foreach (object nodeObj in currentNode.ChildNodes)
        {
            var childNode = (XmlNode)nodeObj;
            XmlAttribute idAttribute = childNode.Attributes?[$"x:{OrigamFile.IdAttribute}"];
            Guid currentNodeId;
            if (idAttribute != null)
            {
                var nodeNameSpace = OrigamNameSpace.CreateOrGet(childNode.NamespaceURI);
                Guid parentId = ParseParentId(parentNodeId, childNode);
                bool isFolder = ParseIsFolder(childNode);
                currentNodeId = new Guid(idAttribute.Value);
                var objectInfo = new PersistedObjectInfo(
                    category: childNode.LocalName,
                    id: currentNodeId,
                    parentId: parentId,
                    isFolder: isFolder,
                    origamFile: (OrigamFile)origamFile,
                    fullTypeName: nodeNameSpace.FullTypeName,
                    version: nodeNameSpace.Version
                );
                if (origamFile.ContainedObjects.ContainsKey(objectInfo.Id))
                {
                    throw new InvalidOperationException(
                        "Duplicate object with id: "
                            + objectInfo.Id
                            + " in: "
                            + origamFile.Path.Relative
                    );
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
            out bool isFolder
        );
        return isFolder;
    }

    private static Guid ParseParentId(Guid parentNodeId, XmlNode childNode)
    {
        XmlAttribute nodeAttribute = childNode?.Attributes?[$"x:{OrigamFile.ParentIdAttribute}"];
        Guid parentId = nodeAttribute != null ? new Guid(nodeAttribute.Value) : parentNodeId;
        return parentId;
    }
}
