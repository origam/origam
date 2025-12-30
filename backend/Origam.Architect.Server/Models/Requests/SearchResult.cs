namespace Origam.Architect.Server.Models.Requests;

public class SearchResult
{
    public string Name { get; set; }
    public Guid SchemaId { get; set; }
    public List<string> ParentNodeIds { get; set; }
}
