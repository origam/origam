namespace Origam.Architect.Server.ReturnModels;

public class EditorData
{
    public TreeNode Node { get; set; }
    public object Data { get; set; }
    public bool IsPersisted { get; set; }
    public string ParentNodeId { get; set; }
}