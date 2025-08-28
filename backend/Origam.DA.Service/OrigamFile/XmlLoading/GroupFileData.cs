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
using Origam.DA.Service.NamespaceMapping;
using Origam.Schema;

namespace Origam.DA.Service;

public class GroupFileData : ObjectFileData
{
    private static readonly string GroupXpath =
        $"//{XmlNamespaceTools.GetXmlNamespaceName(typeof(SchemaItemGroup))}:group";
    public Guid GroupId { get; }
    public override Folder FolderToDetermineParentGroup => Folder.Parent;
    public override Folder FolderToDetermineReferenceGroup => Folder.Parent;

    public GroupFileData(
        IList<string> parentFolders,
        XmlFileData xmlFileData,
        OrigamFileFactory origamFileFactory
    )
        : base(new ParentFolders(parentFolders), xmlFileData, origamFileFactory)
    {
        string groupIdStr =
            xmlFileData
                ?.XmlDocument?.SelectSingleNode(GroupXpath, xmlFileData.NamespaceManager)
                ?.Attributes?[$"x:{OrigamFile.IdAttribute}"]?.Value
            ?? throw new Exception($"Could not read group id from: {FileInfo.FullName}");

        GroupId = new Guid(groupIdStr);
    }
}
