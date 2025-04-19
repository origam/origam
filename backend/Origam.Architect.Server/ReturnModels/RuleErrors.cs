namespace Origam.Architect.Server.ReturnModels;

public class PropertyUpdate
{
    public string PropertyName { get; set; }
    public List<string> Errors { get; set; }
    public DropDownValue[] DropDownValues { get; set; }
}