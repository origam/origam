using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.Services;
using Origam.Schema;

namespace Origam.Architect.Server.Models;

public class SectionEditorModel
{
    public List<DataSource> DataSources { get; set; }
    public string Name { get; set; }
    public Guid SchemaExtensionId { get; set; }
    public Guid SelectedDataSourceId { get; set; }
    public IEnumerable<EditorField> Fields { get; set; }
    public ApiControl RootControl { get; set; }
}

public class DataSource
{
    public Guid SchemaItemId { get; set; }
    public string Name { get; set; }
}

public  class EditorField
{
    public OrigamDataType Type { get; set; }
    public String Name { get; set; }
}