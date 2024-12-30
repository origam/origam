namespace Origam.Architect.Server.Models;

public class ScreenEditorItemModel {
    public Guid EditorSchemaItemId { get; set; }
    public Guid ParentControlSetItemId { get; set; }
    public Guid ControlItemId { get; set; }
    public int Top { get; set; }
    public int Left { get; set; }
}