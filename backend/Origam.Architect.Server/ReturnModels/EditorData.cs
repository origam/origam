namespace Origam.Architect.Server.ReturnModels;

public class EditorData
{
    public TreeNode Node { get; set; }
    public IEnumerable<EditorProperty> Properties { get; set; }
    public bool IsPersisted { get; set; }
    public string ParentNodeId { get; set; }
}