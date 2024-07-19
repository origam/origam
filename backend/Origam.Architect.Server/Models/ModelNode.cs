using Origam.Schema;
using Origam.UI;

namespace Origam.Architect.Server.Controllers;

public class ModelNode
{
    public string Id { get; set; }
    public string NodeText { get; set; }
    public bool HasChildNodes { get; set; }
    public bool IsNonPersistentItem { get; set; }
    public string IconUrl { get; set; }
    public List<ModelNode> Children { get; set; }

    public static ModelNode Create(IBrowserNode2 node)
    {
        return new ModelNode
        {
            Id = node.NodeId,
            NodeText = node.NodeText,
            IsNonPersistentItem = node is NonpersistentSchemaItemNode,
            HasChildNodes = node.HasChildNodes
        };
    }
}