using System.ComponentModel.DataAnnotations;
using Origam.Architect.Server.Services;
using Origam.Server.Attributes;

namespace Origam.Architect.Server.Models;

public class CloseEditorModel
{
    [Required]
    public string EditorId { get; set; }
    
    public EditorId GetTypedEditorId() => new EditorId(EditorId); 
}