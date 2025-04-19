using Origam.Architect.Server.Models;

namespace Origam.Architect.Server.ReturnModels;

public class SectionEditorModel
{
    public SectionEditorData Data { get; set; }
    public bool IsDirty { get; set; }
}