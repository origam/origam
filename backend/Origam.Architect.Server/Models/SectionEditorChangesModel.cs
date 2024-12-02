using Origam.Architect.Server.Controllers;

namespace Origam.Architect.Server.Models;

public class SectionEditorChangesModel
{
    public Guid SchemaItemId { get; set; }
    public string Name { get; set; }
    public Guid SelectedDataSourceId { get; set; }

    public List<ChangesModel> ModelChanges { get; set; }
}