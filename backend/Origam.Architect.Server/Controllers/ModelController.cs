using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ModelController : ControllerBase
{
    private readonly SchemaService schemaService;

    public ModelController(SchemaService schemaService)
    {
        this.schemaService = schemaService;
    }

    [HttpGet("GetTopNodes")]
    public ActionResult<List<ModelNode>> GetTopNodes()
    {
        if (schemaService.ActiveExtension == null)
        {
            return new List<ModelNode>();
        }

        return schemaService.ActiveExtension
            .ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .Select(x => new ModelNode
            {
                Id = x.NodeId,
                Title = x.NodeText,
                IsLeaf = x.HasChildNodes,
                Children = x.ChildNodes()
                    .Cast<ISchemaItemProvider>()
                    .Select(ModelNode.Create).ToList()
            }).ToList();
    }
    
    [HttpGet("GetChildren")]
    public async Task<ActionResult<List<ModelNode>>> GetChildren([FromQuery] string id)
    {
        return Ok(null);
    }
}