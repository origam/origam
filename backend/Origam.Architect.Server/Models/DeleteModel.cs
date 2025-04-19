using Origam.Server.Attributes;

namespace Origam.Architect.Server.Models;

public class DeleteModel
{
    [RequiredNonDefault]
    public Guid SchemaItemId { get; set; }
}