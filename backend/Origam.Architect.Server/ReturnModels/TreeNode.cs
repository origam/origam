using Origam.Schema;
using Origam.Schema.EntityModel;
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
    public string EditorType { get; set; }


    public static string ToTreeNodeId(IBrowserNode2 node)
    {
        return node.NodeId + node.NodeText;
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
            EditorType = GetEditorType(node)
        };
    }

    private string GetEditorType(IBrowserNode2 node)
    {
        if (node is not ISchemaItem || node is Package)
        {
            return null;
        }

        string itemType = node.GetType().ToString();
        if(itemType == "Origam.Schema.GuiModel.FormControlSet" 
           || itemType == "Origam.Schema.GuiModel.PanelControlSet"
           || itemType == "Origam.Schema.GuiModel.ControlSetItem")
        {
            return "ScreenSectionEditor";
        }
        if(itemType == "Origam.Schema.EntityModel.XslTransformation"
           || itemType == "Origam.Schema.RuleModel.XslRule"
           || itemType == "Origam.Schema.RuleModel.EndRule"
           || itemType == "Origam.Schema.RuleModel.ComplexDataRule")
        {
            return "XslTEditor";
        }
        if(itemType == "Origam.Schema.EntityModel.XsdDataStructure")
        {
            return null;
        }
        if(itemType == "Origam.Schema.DeploymentModel.ServiceCommandUpdateScriptActivity")
        {
            return null;
        }
        if (node is EntityUIAction)
        {
            return null;
        }
        if (itemType == "Origam.Schema.WorkflowModel.Workflow")
        {
            return null;
        }
        return "GridEditor";
    }
}
