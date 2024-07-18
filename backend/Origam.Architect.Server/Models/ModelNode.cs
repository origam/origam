using Origam.Schema;

namespace Origam.Architect.Server.Controllers;

public class ModelNode
{
    public string Id { get; set; }
    public string Title { get; set; }
    public bool IsLeaf { get; set; }
    public List<ModelNode> Children { get; set; }

    public static ModelNode Create(ISchemaItemProvider provider)
    {
        return new ModelNode
        {
            Id = provider.NodeId,
            Title = provider.NodeText,
            IsLeaf = provider.HasChildItems
        };
    }
    public static ModelNode Create(ISchemaItem schemaItem)
    {
        return new ModelNode
        {
            Id = schemaItem.Id.ToString(),
            Title = schemaItem.NodeText,
            IsLeaf = schemaItem.HasChildItems
        };
    }
}