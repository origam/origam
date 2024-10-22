using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Origam.Architect.Server.Models;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ModelController(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TreeNodeFactory treeNodeFactory,
    ILogger<ModelController> log) : ControllerBase
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    [HttpGet("GetTopNodes")]
    public ActionResult<List<TreeNode>> GetTopNodes()
    {
        if (schemaService.ActiveExtension == null)
        {
            return new List<TreeNode>();
        }

        return schemaService.ActiveExtension
            .ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .Select(x => new TreeNode
            {
                Id = x.NodeId,
                NodeText = x.NodeText,
                HasChildNodes = x.HasChildNodes,
                Children = x.ChildNodes()
                    .Cast<ISchemaItemProvider>()
                    .Select(treeNodeFactory.Create).ToList()
            }).ToList();
    }

    [HttpGet("GetChildren")]
    public async Task<ActionResult<List<TreeNode>>> GetChildren(
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

        List<TreeNode> nodes = GetProviderTopChildren(provider);
        return Ok(nodes);
    }

    private List<TreeNode> GetChildren(Guid id, bool isNonPersistentItem,
        string nodeText)
    {
        IBrowserNode2 provider = persistenceProvider
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
            .Select(treeNodeFactory.Create)
            .ToList();
    }

    private List<TreeNode> GetProviderTopChildren(
        ISchemaItemProvider provider)
    {
        List<TreeNode> nodes = provider.ChildGroups
            .Where(x => x.ParentGroup == null)
            .OrderBy(x => x.NodeText)
            .Select(treeNodeFactory.Create)
            .Concat(provider.ChildItems
                .Where(x => x.Group == null)
                .OrderBy(x => x.NodeText)
                .Select(treeNodeFactory.Create))
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

    [HttpPost("DeleteSchemaItem")]
    public IActionResult DeleteSchemaItem(
        [Required] [FromBody] DeleteModel input)
    {
        try
        {
            persistenceProvider.BeginTransaction();
            var instance =
                persistenceProvider.RetrieveInstance<IBrowserNode2>(
                    input.SchemaItemId);
            instance.Delete();
        }
        catch (InvalidOperationException ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return BadRequest(ex);
        }
        persistenceProvider.EndTransaction();
        return Ok();
    }
}