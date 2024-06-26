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

namespace Origam.DA.Service;
public class PersistedObjectInfo
{
    public PersistedObjectInfo(string category, Guid id, Guid parentId,
        bool isFolder, OrigamFile origamFile, string fullTypeName, Version version)
    {
        FullTypeName = fullTypeName;
        Version = version;
        Category = category;
        Id = id;
        ParentId = parentId;
        OrigamFile = origamFile;
        IsFolder = isFolder;
    }
    public OrigamFile OrigamFile { get; }
    public Guid Id { get; }
    public bool IsFolder { get; } = false;
    public Guid ParentId { get; }
    public string Category { get; }
    
    public string FullTypeName { get; }
    public Version Version { get; }
    public override string ToString()
    {
        return "OrigamFile path:" + OrigamFile.Path.Absolute + ", Id: " + Id + ", ParentId: " + ParentId +
               ", Category: " + Category+ ", IsFolder: "+ IsFolder;
    }
}
