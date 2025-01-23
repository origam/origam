using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.ReturnModels;
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
    public PackagesInfo GetAll()
    {
        var packages = schemaService.AllPackages
            .OrderBy(x => x.Name)
            .Select(x => new PackageModel(x.Id, x.NodeText));

        return new PackagesInfo
        {
            Packages = packages,
            ActivePackageId = schemaService.ActiveSchemaExtensionId == Guid.Empty
                ? null 
                : schemaService.ActiveSchemaExtensionId
        };
    }
    
    [HttpPost("SetActive")]
    public ActionResult SetActive([FromBody]PackageIdentifier package)
    {
        if (schemaService.ActiveSchemaExtensionId == package.Id)
        {
            return Ok();
        }
        SecurityManager.SetServerIdentity();
        schemaService.LoadSchema(package.Id);
        return Ok();
    }
}

public record PackageIdentifier(Guid Id);
