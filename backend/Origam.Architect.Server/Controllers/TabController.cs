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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.AI.Agent.Extensions;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TabController(
    TreeNodeFactory treeNodeFactory,
    TabService tabService,
    TabResponseFactory tabResponseFactory,
    DocumentationHelperService documentationHelper
) : ControllerBase
{
    [HttpPost("CreateNode")]
    [EndpointDescription(
        "Create a new model item under a parent node. This is how database fields, virtual "
            + "fields, function-call fields, lookup fields, relationships, parameters, filters, "
            + "indexes and whole entities are created. nodeId is the parent item's id, "
            + "newTypeName is the item type's caption (for example 'Database Field') or its full "
            + "type name, changes carries the properties to set immediately as [{name, value}], "
            + "and persist writes the item to disk in the same call. The response returns the new "
            + "item's id, whether it was saved, its editable properties and any validation errors, "
            + "plus parentName and parentOrigamId telling you where the item actually landed - "
            + "check those instead of re-reading the tree, and report that location to the user "
            + "rather than saying you could not verify it. For a new entity the response also "
            + "carries primaryKeyFieldId, the id of that entity's primary key field - pass it as "
            + "ForeignKeyField when you create a foreign key pointing at this entity, instead of "
            + "exploring the entity to find it. When newTypeName does not match, the error lists "
            + "the captions that are valid under that parent. When the response carries "
            + "discarded true, the item failed validation and was thrown away - it does not exist, "
            + "its id is dead, and this is not a successful creation. Read the errors, call "
            + "CreateNode again with those properties filled in, and never report a discarded item "
            + "as created."
    )]
    public OpenTabData CreateNode([Required] [FromBody] NewItemModel input)
    {
        NewItemResult result = tabService.CreateNode(input);
        OpenTabData tabData = result.Discarded
            ? tabResponseFactory.DiscardedTabData(result.Tab)
            : tabResponseFactory.CreatedTabData(result.Tab);

        if (result.CloseWhenDone)
        {
            tabService.CloseTab(result.Tab.Id);
        }

        return tabData;
    }

    [HttpGet("GetOpen")]
    public IActionResult GetOpen()
    {
        var items = tabService
            .GetOpenTabs()
            .Select(tab =>
            {
                var item = tab.Item;
                TreeNode treeNode = treeNodeFactory.Create(item);

                return tab.Id.Type switch
                {
                    TabType.Default => new OpenTabData(
                        tabId: tab.Id,
                        node: treeNode,
                        data: tabResponseFactory.GetEditorData(treeNode, item),
                        isPersisted: item.IsPersisted,
                        parentNodeId: TreeNode.ToTreeNodeId(item.ParentItem),
                        isDirty: tab.IsDirty,
                        isStale: tab.IsStale
                    ),
                    TabType.DocumentationEditor => new OpenTabData(
                        tabId: tab.Id,
                        node: treeNode,
                        data: documentationHelper.GetData(tab.DocumentationData, item.Name),
                        isPersisted: item.IsPersisted,
                        isDirty: tab.IsDirty,
                        isStale: tab.IsStale
                    ),
                    _ => throw new Exception("Unknown tab type: " + tab.Id.Type),
                };
            })
            .ToList();
        return Ok(items);
    }

    [HttpPost("Open")]
    public IActionResult Open([Required] [FromBody] OpenTabModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.SchemaItemId);
        ISchemaItem item = tab.Item;
        TreeNode treeNode = treeNodeFactory.Create(item);

        var openTabData = new OpenTabData(
            tabId: tab.Id,
            node: treeNode,
            data: tabResponseFactory.GetEditorData(treeNode, item),
            isPersisted: true
        );
        return Ok(openTabData);
    }

    [HttpPost("Close")]
    public IActionResult Close([Required] [FromBody] CloseTabModel input)
    {
        tabService.CloseTab(input.GetTypedTabId());
        return Ok();
    }

    [HttpPost("CloseAll")]
    public IActionResult CloseAll()
    {
        tabService.CloseAllTabs();
        return Ok();
    }

    [HttpPost("PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        TabData tabData = tabService.OpenDefaultTab(input.SchemaItemId);
        if (!tabService.CanPersist(tabData.Item))
        {
            return BadRequest("No Datasource selected can't save");
        }

        tabService.PersistItem(tabData);

        bool closeWhenDone = Request.IsFromAIAgent();
        if (closeWhenDone)
        {
            tabService.CloseTab(tabData.Id);
        }

        return Ok();
    }
}
