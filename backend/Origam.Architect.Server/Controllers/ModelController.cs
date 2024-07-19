using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ModelController : ControllerBase
{
    private readonly SchemaService schemaService;
    private readonly IPersistenceService persistenceService;

    public ModelController(SchemaService schemaService,
        IPersistenceService persistenceService)
    {
        this.schemaService = schemaService;
        this.persistenceService = persistenceService;
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
                NodeText = x.NodeText,
                HasChildNodes = x.HasChildNodes,
                Children = x.ChildNodes()
                    .Cast<ISchemaItemProvider>()
                    .Select(ModelNode.Create).ToList()
            }).ToList();
    }

    [HttpGet("GetChildren")]
    public async Task<ActionResult<List<ModelNode>>> GetChildren(
        [FromQuery] string id, [FromQuery] bool isNonPersistentItem, 
        [FromQuery] string nodeText)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id cannot be empty");
        }

        if (Guid.TryParse(id, out var guidId))
        {
            var childNodes = GetChildren(
                guidId, isNonPersistentItem, nodeText);
            return Ok(childNodes);
        }

        ISchemaItemProvider provider = GetRootProviderById(id);
        if (provider == null)
        {
            return NotFound();
        }

        List<ModelNode> nodes = GetProviderTopChildren(provider);
        return Ok(nodes);
    }

    private List<ModelNode> GetChildren(Guid id, bool isNonPersistentItem,
        string nodeText)
    {
        IBrowserNode2 provider = persistenceService.SchemaProvider
            .RetrieveInstance<IBrowserNode2>(id);
        if (isNonPersistentItem)
        {
            provider = new NonpersistentSchemaItemNode
                {
                    NodeText = nodeText,
                    ParentNode = provider
                };
        }
        
        return provider
            .ChildNodes().Cast<IBrowserNode2>()
            .OrderBy(x => x.NodeText)
            .Select(ModelNode.Create)
            .ToList();
    }

    private static List<ModelNode> GetProviderTopChildren(
        ISchemaItemProvider provider)
    {
        List<ModelNode> nodes = provider.ChildGroups
            .Where(x => x.ParentGroup == null)
            .OrderBy(x => x.NodeText)
            .Select(ModelNode.Create)
            .Concat(provider.ChildItems
                .Where(x => x.Group == null)
                .OrderBy(x => x.NodeText)
                .Select(ModelNode.Create))
            .ToList();
        return nodes;
    }

    private ISchemaItemProvider GetRootProviderById(string id)
    {
        ISchemaItemProvider provider = schemaService.ActiveExtension
            .ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .SelectMany(x => x.ChildNodes().Cast<ISchemaItemProvider>())
            .FirstOrDefault(x => x.NodeId == id);
        return provider;
    }
}