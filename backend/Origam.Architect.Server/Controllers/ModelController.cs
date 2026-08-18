#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
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
    GitNodeStatusService gitNodeStatusService,
    TabService tabService,
    EntityIndexService entityIndexService
) : ControllerBase
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    [HttpGet("GetTopNodes")]
    public ActionResult<List<TreeNode>> GetTopNodes()
    {
        if (schemaService.ActiveExtension == null)
        {
            return new List<TreeNode>();
        }

        return schemaService
            .ActiveExtension.ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .Select(x => new TreeNode
            {
                OrigamId = x.NodeId,
                Id = x.NodeId + x.NodeText,
                NodeText = x.NodeText,
                HasChildNodes = x.HasChildNodes,
                NodeLevelType = NodeLevelType.Category,
                Children = x.ChildNodes()
                    .Cast<ISchemaItemProvider>()
                    .OrderBy(child => child.NodeText, StringComparer.OrdinalIgnoreCase)
                    .Select(treeNodeFactory.Create)
                    .ToList(),
            })
            .ToList();
    }

    [HttpGet("GetChildren")]
    public ActionResult<List<TreeNode>> GetChildren(
        [FromQuery] string id,
        [FromQuery] bool isNonPersistentItem,
        [FromQuery] string nodeText
    )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id cannot be empty");
        }

        if (schemaService.ActiveExtension == null)
        {
            return BadRequest("No schema extension is active");
        }

        if (Guid.TryParse(id, out var guidId))
        {
            var childNodes = GetChildren(guidId, isNonPersistentItem, nodeText);
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

    private List<TreeNode> GetChildren(Guid id, bool isNonPersistentItem, string nodeText)
    {
        IBrowserNode2 provider = persistenceProvider.RetrieveInstance<IBrowserNode2>(id);
        if (isNonPersistentItem)
        {
            provider = new NonpersistentSchemaItemNode
            {
                NodeText = nodeText,
                ParentNode = provider,
            };
        }

        return provider
            .ChildNodes()
            .Cast<IBrowserNode2>()
            .OrderBy(x => x.NodeText)
            .Where(x => x is not ISchemaItem item || item.IsPersisted)
            .Select(treeNodeFactory.Create)
            .ToList();
    }

    private List<TreeNode> GetProviderTopChildren(ISchemaItemProvider provider)
    {
        List<TreeNode> nodes = provider
            .ChildGroups.Where(x => x.ParentGroup == null)
            .OrderBy(x => x.NodeText)
            .Select(treeNodeFactory.Create)
            .Concat(
                provider
                    .ChildItems.Where(x => x.Group == null)
                    .OrderBy(x => x.NodeText)
                    .Select(treeNodeFactory.Create)
            )
            .ToList();
        return nodes;
    }

    private ISchemaItemProvider GetRootProviderById(string id)
    {
        ISchemaItemProvider provider = schemaService
            .ActiveExtension.ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .SelectMany(x => x.ChildNodes().Cast<ISchemaItemProvider>())
            .FirstOrDefault(x => x.NodeId == id);
        return provider;
    }

    [HttpPost("DeleteSchemaItem")]
    [EndpointDescription(
        "Permanently delete a model item - a field, an entity, a filter, a relationship, a "
            + "screen and so on - from the model and from disk. schemaItemId is the id of the "
            + "item ITSELF: to delete a field pass that field's id, not its parent entity's id. "
            + "The item must already be saved. There is no undo, and anything still referencing "
            + "the deleted item stops working. On success returns {deleted, id, name} - treat "
            + "that as proof the item is gone and do not call this again for the same id. A 404 "
            + "means no saved item has that id, which usually means an earlier delete of it "
            + "already succeeded; never retry the same id after a 404. A 400 carries the reason "
            + "the model refused the delete."
    )]
    public IActionResult DeleteSchemaItem([Required] [FromBody] DeleteModel input)
    {
        ISchemaItem instance = null;
        foreach (ISchemaItemProvider provider in schemaService.Providers)
        {
            instance = provider.ChildItemsRecursive.FirstOrDefault(x => x.Id == input.SchemaItemId);
            if (instance != null)
            {
                break;
            }
        }

        if (instance == null)
        {
            return NotFound(
                $"No saved model item has id {input.SchemaItemId}. It was either never saved or "
                    + "it has already been deleted - a previous delete of this id may have "
                    + "succeeded. Do not repeat this call with the same id."
            );
        }

        string deletedName = instance.Name;
        ISchemaItem deletedRootItem = instance.RootItem;
        try
        {
            persistenceProvider.BeginTransaction();
            instance.Delete();
        }
        catch (InvalidOperationException ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(statusCode: 400, ex.Message);
        }

        persistenceProvider.EndTransaction();
        gitNodeStatusService.ClearCache();
        tabService.InvalidateTabsInRoot(deletedRootItem, changedByTabId: null);
        return Ok(new DeleteResult(Deleted: true, Id: input.SchemaItemId, Name: deletedName));
    }

    [HttpGet("GetMenuItems")]
    [EndpointDescription(
        "List the model item types that can be created as children of the given node (the 'New' "
            + "context menu). Each entry has a caption, for example 'Database Field', and a "
            + "typeName; either one can be passed as newTypeName to POST /Tab/CreateNode."
    )]
    public IEnumerable<MenuItemInfo> GetMenuItems(
        [FromQuery] string id,
        [FromQuery] bool isNonPersistentItem,
        [FromQuery] string nodeText
    )
    {
        if (!Guid.TryParse(id, out Guid schemaItemId))
        {
            ISchemaItemProvider provider = GetRootProviderById(id);
            if (provider == null)
            {
                return new List<MenuItemInfo>();
            }

            return provider.NewItemTypes.Select(GetMenuInfo);
        }

        IBrowserNode2 instance = persistenceProvider.RetrieveInstance<IBrowserNode2>(schemaItemId);

        ISchemaItemFactory factory = isNonPersistentItem
            ? new NonpersistentSchemaItemNode { NodeText = nodeText, ParentNode = instance }
            : (ISchemaItemFactory)instance;

        return factory.NewItemTypes.Select(GetMenuInfo);
    }

    private MenuItemInfo GetMenuInfo(Type type)
    {
        SchemaItemDescriptionAttribute attr = type.SchemaItemDescription();
        if (attr is null)
        {
            return new MenuItemInfo(
                caption: type.Name,
                typeName: type.FullName,
                iconName: null,
                iconIndex: null
            );
        }

        return new MenuItemInfo(
            caption: attr.Name,
            typeName: type.FullName,
            iconName: attr.Icon is string iconName ? iconName : null,
            iconIndex: attr.Icon is int iconIndex ? iconIndex : null
        );
    }

    [HttpGet("GetSchemaNodeDetails")]
    public ActionResult<TreeNode> GetSchemaNodeDetails(
        [FromQuery] string id,
        [FromQuery] int depth = 3
    )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id cannot be empty");
        }

        if (Guid.TryParse(id, out var guidId))
        {
            IBrowserNode2 node = persistenceProvider.RetrieveInstance<IBrowserNode2>(guidId);
            if (node == null)
            {
                return NotFound();
            }
            var treeNode = LoadTreeRecursive(node, depth);
            return Ok(treeNode);
        }

        ISchemaItemProvider provider = GetRootProviderById(id);
        if (provider == null)
        {
            return NotFound();
        }

        var providerNode = new TreeNode
        {
            Id = id,
            OrigamId = id,
            NodeText = provider.NodeText,
            NodeLevelType = NodeLevelType.Provider,
        };
        providerNode.Children = GetProviderTopChildren(provider);
        return Ok(providerNode);
    }

    private TreeNode LoadTreeRecursive(IBrowserNode2 node, int remainingDepth)
    {
        TreeNode treeNode = treeNodeFactory.Create(node);
        if (remainingDepth > 0 && treeNode.HasChildNodes)
        {
            var children = node.ChildNodes()
                .Cast<IBrowserNode2>()
                .OrderBy(x => x.NodeText)
                .Where(x => x is not ISchemaItem item || item.IsPersisted)
                .Select(child => LoadTreeRecursive(child, remainingDepth - 1))
                .ToList();
            treeNode.Children = children;
        }
        return treeNode;
    }

    [HttpGet("GetEntityIndex")]
    public ActionResult<List<EntityCard>> GetEntityIndex()
    {
        return Ok(entityIndexService.Get());
    }
}
