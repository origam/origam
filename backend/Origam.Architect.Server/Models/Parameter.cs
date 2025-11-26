using System.ComponentModel.DataAnnotations;

namespace Origam.Architect.Server.Models;

public class Parameter
{
    [Required]
    public string Name { get; set; }

    public string Value { get; set; }
}
