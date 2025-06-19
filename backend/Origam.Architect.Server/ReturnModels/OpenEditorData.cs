#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.ReturnModels;

public class OpenEditorData
{
    public string EditorId { get; }
    public string EditorType { get; }
    public TreeNode Node { get; }
    public object Data { get; }
    public bool IsPersisted { get; }
    public string ParentNodeId { get; }
    public bool IsDirty { get; }

    public OpenEditorData(EditorId editorId, TreeNode node, 
        object data, bool isPersisted, string parentNodeId = null,
        bool isDirty = false)
    {
        EditorId = editorId.ToString();
        EditorType = editorId.Type == Services.EditorType.Default
            ? node.DefaultEditor.ToString()
            : editorId.Type.ToString();
        Node = node;
        Data = data;
        IsPersisted = isPersisted;
        ParentNodeId = parentNodeId;
        IsDirty = isDirty;
    }
}