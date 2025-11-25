using System.ComponentModel.DataAnnotations;

namespace Origam.Architect.Server.Models;

public sealed class XsltValidateModel
{
    [Required]
    public Guid SchemaItemId { get; set; }
}
