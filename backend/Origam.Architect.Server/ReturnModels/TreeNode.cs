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

using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;

namespace Origam.Architect.Server.ReturnModels;

public class TreeNode
{
    public string OrigamId { get; set; }
    public string Id { get; set; }
    public string NodeText { get; set; }
    public bool HasChildNodes { get; set; }
    public bool IsNonPersistentItem { get; set; }
    public string IconUrl { get; set; }
    public List<TreeNode> Children { get; set; }
    public EditorSubType? DefaultEditor { get; set; }

    public static string ToTreeNodeId(IBrowserNode2 node)
    {
        return node == null ? null : node.NodeId + node.NodeText;
    }
}

public class TreeNodeFactory
{
    public TreeNode Create(IBrowserNode2 node)
    {
        return new TreeNode
        {
            OrigamId = node.NodeId,
            Id = TreeNode.ToTreeNodeId(node),
            NodeText = node.NodeText,
            IsNonPersistentItem = node is NonpersistentSchemaItemNode,
            HasChildNodes = node.HasChildNodes,
            DefaultEditor = GetEditorType(node),
            IconUrl = GetIcon(node),
        };
    }

    private string GetIcon(IBrowserNode2 node)
    {
        if (node is SchemaItemGroup or AbstractSchemaItemProvider)
        {
            return "/Icons/directory.svg";
        }

        return null;
    }

    private EditorSubType? GetEditorType(IBrowserNode2 node)
    {
        if (node is not ISchemaItem || node is Package)
        {
            return null;
        }

        string itemType = node.GetType().ToString();
        if (itemType == "Origam.Schema.GuiModel.FormControlSet")
        {
            return EditorSubType.ScreenEditor;
        }
        if (
            itemType == "Origam.Schema.GuiModel.PanelControlSet"
            || itemType == "Origam.Schema.GuiModel.ControlSetItem"
        )
        {
            return EditorSubType.ScreenSectionEditor;
        }
        if (
            itemType == "Origam.Schema.EntityModel.XslTransformation"
            || itemType == "Origam.Schema.RuleModel.XslRule"
            || itemType == "Origam.Schema.RuleModel.EndRule"
            || itemType == "Origam.Schema.RuleModel.ComplexDataRule"
        )
        {
            return EditorSubType.XsltEditor;
        }
        if (itemType == "Origam.Schema.EntityModel.XsdDataStructure")
        {
            return EditorSubType.GridEditor;
        }
        if (itemType == "Origam.Schema.DeploymentModel.ServiceCommandUpdateScriptActivity")
        {
            return EditorSubType.GridEditor;
        }
        if (node is EntityUIAction)
        {
            return EditorSubType.GridEditor;
        }
        if (itemType == "Origam.Schema.WorkflowModel.Workflow")
        {
            return EditorSubType.GridEditor;
        }
        return EditorSubType.GridEditor;
    }
}

public enum EditorSubType
{
    GridEditor,
    XsltEditor,
    ScreenSectionEditor,
    ScreenEditor,
}
