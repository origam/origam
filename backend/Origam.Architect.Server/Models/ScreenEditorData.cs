using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Models;

public class ScreenEditorData
{
    public List<DataSource> DataSources { get; set; }
    public string Name { get; set; }
    public Guid SchemaExtensionId { get; set; }
    public Guid SelectedDataSourceId { get; set; }
    public IEnumerable<ToolBoxItem> Sections { get; set; }
    public IEnumerable<ToolBoxItem> Widgets { get; set; }
    public ApiControl RootControl { get; set; }
}

public class ToolBoxItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class ScreenEditorModel
{
    public ScreenEditorData Data { get; set; }
    public bool IsDirty { get; set; }
}