namespace Origam.Architect.Server.Models.Requests;

public class SearchResult
{
    public string FoundIn { get; set; }
    public string RootType { get; set; }
    public string Type { get; set; }
    public string Folder { get; set; }
    public string Package { get; set; }
    public bool PackageReference { get; set; }
    public Guid SchemaId { get; set; }
    public List<string> ParentNodeIds { get; set; }
}
