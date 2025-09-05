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

public class OpenEditorData(
    EditorId editorId,
    TreeNode node,
    object data,
    bool isPersisted,
    string parentNodeId = null,
    bool isDirty = false
)
{
    public string EditorId { get; } = editorId.ToString();
    public string EditorType { get; } =
        editorId.Type == Services.EditorType.Default
            ? node.DefaultEditor.ToString()
            : editorId.Type.ToString();

    public TreeNode Node { get; } = node;
    public object Data { get; } = data;
    public bool IsPersisted { get; } = isPersisted;
    public string ParentNodeId { get; } = parentNodeId;
    public bool IsDirty { get; } = isDirty;
}
