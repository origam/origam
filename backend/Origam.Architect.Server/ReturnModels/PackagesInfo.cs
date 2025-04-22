namespace Origam.Architect.Server.ReturnModels;

public class PackagesInfo
{
    public IEnumerable<PackageModel> Packages { get; set; }
    public Guid? ActivePackageId { get; set; }
}

public class PackageModel(Guid id, string name)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
}