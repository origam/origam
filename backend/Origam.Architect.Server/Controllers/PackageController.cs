using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PackageController : ControllerBase
{
    private readonly SchemaService schemaService;

    public PackageController(SchemaService schemaService)
    {
        this.schemaService = schemaService;
    }

    [HttpGet("GetAll")]
    public List<PackageModel> GetAll()
    {
        return schemaService.AllPackages
            .OrderBy(x => x.Name)
            .Select(x => new PackageModel(x.Id, x.NodeText))
            .ToList();
    }
    
    [HttpPost("SetActive")]
    public ActionResult SetActive([FromBody]PackageIdentifier package)
    {
        SecurityManager.SetServerIdentity();
        schemaService.LoadSchema(package.Id);
        return Ok();
    }
}

public record PackageIdentifier(Guid Id);

public class PackageModel(Guid id, string name)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
}