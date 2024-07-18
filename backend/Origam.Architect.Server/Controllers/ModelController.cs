using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ModelController : ControllerBase
{
    [HttpGet("GetNode")]
    public async Task<ActionResult<ModelNode>> GetNode([FromQuery] string id)
    {
        var node = new ModelNode
        {
            Key = id,
            Title = $"Node {id}",
            // Children = new List<ModelNode>
            // {
            //     new ModelNode { Key = $"{id}-0", Title = $"Child of {id}", IsLeaf = false },
            //     new ModelNode { Key = $"{id}-1", Title = $"Leaf of {id}", IsLeaf = true }
            // }
        };

        return Ok(node);
    }
    [HttpGet("GetChildren")]
    public async Task<ActionResult<List<ModelNode>>> GetChildren([FromQuery] string id)
    {
        var children = new List<ModelNode>
        {
            new ModelNode
                { Key = $"{id}-0", Title = $"Child of {id}", IsLeaf = false },
            new ModelNode
                { Key = $"{id}-1", Title = $"Leaf of {id}", IsLeaf = true }
        };

        return Ok(children);
    }
}

public class ModelNode
{
    public string Key { get; set; }
    public string Title { get; set; }
    public bool IsLeaf { get; set; }
    public List<ModelNode> Children { get; set; }
}