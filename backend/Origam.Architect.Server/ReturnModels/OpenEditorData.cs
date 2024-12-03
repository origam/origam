namespace Origam.Architect.Server.ReturnModels;

public class OpenEditorData
{
    public TreeNode Node { get; set; }
    public object Data { get; set; }
    public bool IsPersisted { get; set; }
    public string ParentNodeId { get; set; }
    public bool IsDirty { get; set; }
}