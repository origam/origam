using Origam.Schema;

namespace Origam.Architect.Server.Models;

public class ScreenEditorItem {
    public Guid EditorSchemaItemId { get; set; }
    public Guid ParentControlSetItemId { get; set; }
    public string ComponentType { get; set; }
    public string FieldName { get; set; }
    public int Top { get; set; }
    public int Left { get; set; }
}