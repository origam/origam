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
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ModelController(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TreeNodeFactory treeNodeFactory,
    GitNodeStatusService gitNodeStatusService
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
            return StatusCode(statusCode: 400, ex.Message);
        }

        persistenceProvider.EndTransaction();
        gitNodeStatusService.ClearCache();
        return Ok();
    }

    [HttpGet("GetMenuItems")]
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

    [HttpGet("SearchSchema")]
    public ActionResult<List<TreeNode>> SearchSchema([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query cannot be empty");
        }

        var results = new List<TreeNode>();
        if (schemaService.ActiveExtension == null)
        {
            return Ok(results);
        }

        foreach (ISchemaItemProvider provider in schemaService.Providers)
        {
            var matches = provider
                .ChildItemsRecursive.Where(x =>
                    x.Name != null && x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                )
                .Take(50 - results.Count)
                .Select(x => new TreeNode
                {
                    OrigamId = x.Id.ToString("D"),
                    NodeText = x.Name,
                    ItemType = x.GetType().FullName,
                    ItemTypeName = x.GetType().SchemaItemDescription()?.Name,
                });

            results.AddRange(matches);
            if (results.Count >= 50)
            {
                break;
            }
        }

        return Ok(results);
    }

    [HttpGet("GetEntityIndex")]
    public ActionResult<List<EntityCard>> GetEntityIndex()
    {
        if (schemaService.ActiveExtension == null)
        {
            return Ok(new List<EntityCard>());
        }

        var allItems = schemaService
            .Providers.SelectMany(provider => provider.ChildItemsRecursive)
            .Where(item => item.IsPersisted)
            .ToList();

        var entities = allItems.OfType<IDataEntity>().ToList();

        var structuresByEntityId = new Dictionary<Guid, List<DataStructure>>();
        foreach (DataStructure structure in allItems.OfType<DataStructure>())
        {
            foreach (DataStructureEntity dse in structure.Entities)
            {
                IDataEntity linked = dse.EntityDefinition;
                if (linked == null)
                {
                    continue;
                }
                Guid entityId = (Guid)linked.PrimaryKey["Id"];
                if (!structuresByEntityId.TryGetValue(entityId, out var list))
                {
                    list = new List<DataStructure>();
                    structuresByEntityId[entityId] = list;
                }
                if (!list.Any(existing => existing.Id == structure.Id))
                {
                    list.Add(structure);
                }
            }
        }

        var screensByStructureId = allItems
            .OfType<FormControlSet>()
            .Where(screen => screen.DataSourceId != Guid.Empty)
            .GroupBy(screen => screen.DataSourceId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var panelsByEntityId = allItems
            .OfType<PanelControlSet>()
            .Where(panel => panel.DataSourceId != Guid.Empty)
            .GroupBy(panel => panel.DataSourceId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var lookupsByStructureId = new Dictionary<Guid, List<AbstractDataLookup>>();
        foreach (AbstractDataLookup lookup in allItems.OfType<AbstractDataLookup>())
        {
            if (lookup.ListDataStructureId == Guid.Empty)
            {
                continue;
            }
            if (!lookupsByStructureId.TryGetValue(lookup.ListDataStructureId, out var list))
            {
                list = new List<AbstractDataLookup>();
                lookupsByStructureId[lookup.ListDataStructureId] = list;
            }
            list.Add(lookup);
        }

        var workQueuesByEntityId = allItems
            .OfType<WorkQueueClass>()
            .Where(workQueue => workQueue.EntityId != Guid.Empty)
            .GroupBy(workQueue => workQueue.EntityId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var cards = new List<EntityCard>(entities.Count);
        foreach (IDataEntity entity in entities.OrderBy(entity => entity.Name))
        {
            var entityItem = (ISchemaItem)entity;
            Guid entityId = (Guid)entityItem.PrimaryKey["Id"];

            structuresByEntityId.TryGetValue(entityId, out var structures);
            structures ??= new List<DataStructure>();

            var screens = structures
                .SelectMany(structure =>
                    screensByStructureId.TryGetValue(structure.Id, out var list)
                        ? list
                        : Enumerable.Empty<FormControlSet>()
                )
                .Distinct()
                .ToList();

            var lookups = structures
                .SelectMany(structure =>
                    lookupsByStructureId.TryGetValue(structure.Id, out var list)
                        ? list
                        : Enumerable.Empty<AbstractDataLookup>()
                )
                .Distinct()
                .ToList();

            panelsByEntityId.TryGetValue(entityId, out var panels);
            workQueuesByEntityId.TryGetValue(entityId, out var workQueues);

            var fields = entity
                .EntityColumns.Where(column => column.Name != null)
                .OrderBy(column => column.Name)
                .Select(column => column.Name)
                .ToList();

            var primaryKey = entity
                .EntityColumns.Where(column => column.IsPrimaryKey && column.Name != null)
                .OrderBy(column => column.Name)
                .Select(column => column.Name)
                .ToList();

            cards.Add(
                new EntityCard(
                    Id: entityId.ToString("D"),
                    Name: entity.Name,
                    Kind: entity.GetType().SchemaItemDescription()?.Name ?? entity.GetType().Name,
                    Package: entityItem.Group?.Name,
                    Fields: fields,
                    PrimaryKey: primaryKey,
                    Structures: structures
                        .Select(structure => new RelatedItem(
                            structure.Id.ToString("D"),
                            structure.Name
                        ))
                        .ToList(),
                    Screens: screens
                        .Select(screen => new RelatedItem(screen.Id.ToString("D"), screen.Name))
                        .ToList(),
                    Panels: (panels ?? new List<PanelControlSet>())
                        .Select(panel => new RelatedItem(panel.Id.ToString("D"), panel.Name))
                        .ToList(),
                    Lookups: lookups
                        .Select(lookup => new RelatedItem(lookup.Id.ToString("D"), lookup.Name))
                        .ToList(),
                    WorkQueues: (workQueues ?? new List<WorkQueueClass>())
                        .Select(workQueue => new RelatedItem(
                            workQueue.Id.ToString("D"),
                            workQueue.Name
                        ))
                        .ToList()
                )
            );
        }

        return Ok(cards);
    }
}
