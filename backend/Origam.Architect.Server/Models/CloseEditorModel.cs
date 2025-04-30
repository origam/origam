using Origam.Server.Attributes;

namespace Origam.Architect.Server.Models;

public class CloseEditorModel
{
    [RequiredNonDefault]
    public Guid SchemaItemId { get; set; }
}