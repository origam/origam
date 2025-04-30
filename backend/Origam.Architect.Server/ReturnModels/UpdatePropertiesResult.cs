namespace Origam.Architect.Server.ReturnModels;

public class UpdatePropertiesResult
{
    public IEnumerable<PropertyUpdate> PropertyUpdates { get; set; }
    public bool IsDirty { get; set; }
}