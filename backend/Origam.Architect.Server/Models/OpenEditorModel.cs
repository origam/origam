using Origam.Server.Attributes;

namespace Origam.Architect.Server.Models;

public class OpenEditorModel
{
    [RequiredNonDefault]
    public Guid SchemaItemId { get; set; }
}