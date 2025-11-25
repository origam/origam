using System.ComponentModel.DataAnnotations;

namespace Origam.Architect.Server.Models;

public sealed class XsltTransformModel
{
    [Required]
    public Guid SchemaItemId { get; set; }
}