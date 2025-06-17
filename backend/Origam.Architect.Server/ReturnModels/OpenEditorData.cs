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