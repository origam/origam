#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
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
    TreeNodeFactory treeNodeFactory) : ControllerBase
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
                OrigamId = x.NodeId,
                Id = x.NodeId + x.NodeText,
                NodeText = x.NodeText,
                HasChildNodes = x.HasChildNodes,
                Children = x.ChildNodes()
                    .Cast<ISchemaItemProvider>()
                    .OrderBy(child => child.NodeText, StringComparer.OrdinalIgnoreCase)
                    .Select(treeNodeFactory.Create)
                    .ToList()
            })
            .ToList();
    }

    [HttpGet("GetChildren")]
    public ActionResult<List<TreeNode>> GetChildren(
        [FromQuery] string id, [FromQuery] bool isNonPersistentItem,
        [FromQuery] string nodeText)
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
            .Where(x => x is not ISchemaItem item || item.IsPersisted)
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
        ISchemaItem instance = null;
        foreach (ISchemaItemProvider provider in schemaService.Providers)
        {
            instance = provider.ChildItemsRecursive
                .FirstOrDefault(x => x.Id == input.SchemaItemId);
            if (instance != null)
            {
                break;
            }
        }

        if (instance == null)
        {
            return NotFound();
        }

        try
        {
            persistenceProvider.BeginTransaction();
            instance.Delete();
        }
        catch (InvalidOperationException ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(400, ex.Message);
        }

        persistenceProvider.EndTransaction();
        return Ok();
    }

    // Inspired by class Origam.Workbench.Commands.SchemaItemEditorsMenuBuilder, 
    // method public AsMenuCommand[] BuildSubmenu(object owner)
    [HttpGet("GetMenuItems")]
    public IEnumerable<MenuItemInfo> GetMenuItems([FromQuery] string id,
        [FromQuery] bool isNonPersistentItem, [FromQuery] string nodeText)
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

        IBrowserNode2 instance = persistenceProvider
            .RetrieveInstance<IBrowserNode2>(schemaItemId);

        ISchemaItemFactory factory = isNonPersistentItem
            ? new NonpersistentSchemaItemNode
            {
                NodeText = nodeText,
                ParentNode = instance
            }
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
                iconIndex: null);
        }

        return new MenuItemInfo(
            caption: attr.Name,
            typeName: type.FullName,
            iconName: attr.Icon is string iconName ? iconName : null,
            iconIndex: attr.Icon is int iconIndex ? iconIndex : null);
    }
}