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
using Origam.Architect.Server.Services.Move;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.SchemaInfo;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ModelController(
    SchemaItemMoveService moveService,
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TreeNodeFactory treeNodeFactory,
    ModelTransactionRunner modelTransactionRunner,
    TabService tabService,
    ModelGroupService modelGroupService,
    MenuItemService menuItemService
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

        ISchemaItemProvider provider = treeNodeFactory.FindRootProvider(id);
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

        return treeNodeFactory.CreateChildren(provider, depth: 0);
    }

    private List<TreeNode> GetProviderTopChildren(ISchemaItemProvider provider)
    {
        List<TreeNode> nodes = provider
            .ChildGroups.Where(x => x.ParentGroup == null)
            .OrderBy(x => x.NodeText)
            .Select(treeNodeFactory.Create)
            .Concat(
                TreeNodeFactory
                    .OrderForTree(provider.ChildItems.Where(x => x.Group == null))
                    .Select(treeNodeFactory.Create)
            )
            .ToList();
        return nodes;
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
            modelTransactionRunner.Run(() => instance.Delete());
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(statusCode: 400, ex.Message);
        }

        tabService.InvalidateTabsInRoot(deletedRootItem, changedByTabId: null);
        return Ok(new DeleteResult(Deleted: true, Id: input.SchemaItemId, Name: deletedName));
    }

    [HttpPost("CreateGroup")]
    public ActionResult<TreeNode> CreateGroup([Required] [FromBody] CreateGroupModel input) =>
        modelGroupService.Create(input);

    [HttpPost("RenameGroup")]
    public ActionResult<TreeNode> RenameGroup([Required] [FromBody] RenameGroupModel input) =>
        modelGroupService.Rename(input);

    [HttpPost("DeleteGroup")]
    public ActionResult<DeleteGroupResult> DeleteGroup(
        [Required] [FromBody] DeleteGroupModel input
    ) => modelGroupService.Delete(input);

    [HttpGet("GetMenuItems")]
    [EndpointDescription(
        "List the model item types that can be created as children of the given node (the 'New' "
            + "context menu). Each entry has a caption, for example 'Database Field', and a "
            + "typeName; either one can be passed as newTypeName to POST /Tab/CreateNode. Entries "
            + "with a name are parameters the node expects but does not reference yet (for example "
            + "the xsl:param of a transformation called from a workflow step); create them by "
            + "passing a change that sets Name to that value."
    )]
    public IEnumerable<MenuItemInfo> GetMenuItems(
        [FromQuery] string id,
        [FromQuery] bool isNonPersistentItem,
        [FromQuery] string nodeText
    ) => menuItemService.GetMenuItems(id, isNonPersistentItem, nodeText);

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
            TreeNode treeNode = treeNodeFactory.CreateRecursive(node, depth);
            return Ok(treeNode);
        }

        ISchemaItemProvider provider = treeNodeFactory.FindRootProvider(id);
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

    [HttpGet("GetSchemaItemInfos")]
    public ActionResult<List<SchemaItemInfo>> GetSchemaItemInfos()
    {
        if (!ReferenceIndexManager.Initialized)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        if (schemaService.ActiveExtension == null)
        {
            return new List<SchemaItemInfo>();
        }
        return ((FilePersistenceProvider)persistenceProvider).RetrieveSchemaItemInfos();
    }

    [HttpPost("GetMoveVerdicts")]
    public ActionResult<List<MoveVerdictResult>> GetMoveVerdicts(
        [Required] [FromBody] MoveVerdictsModel input
    )
    {
        return Ok(moveService.GetMoveVerdicts(input.Source, input.Targets));
    }

    [HttpPost("GetMoveTargets")]
    [EndpointDescription(
        "List the places a model item can be moved or copied to - the list the Architect's "
            + "'Move to...' dialog shows. Pass source = {id: <id of the item>} and nothing else. "
            + "isSourceInActivePackage false means the item belongs to another package and can "
            + "only be COPIED, never moved. Each target has id (pass it unchanged as target.id to "
            + "POST /Model/MoveNode), nodeText, path (where it sits in the tree), packageName (the "
            + "package that owns that folder; empty for the top folder of the type, which no "
            + "package owns), isInActivePackage, isCurrentLocation (the item is there now - 'the "
            + "same folder' means this target; remember it in case the user wants the item moved "
            + "back), canMove and canCopy. A top level item such as an entity, a constant or a "
            + "screen can go to the top folder of its type or to a folder of that type in any "
            + "loaded package; an item that can change its parent, such as a field, a filter, a "
            + "menu item or a workflow step, lists the possible parent items instead. Skip this "
            + "call when you already know the target id, for example the entity id from MODEL "
            + "INDEX that a field should move to."
    )]
    public ActionResult<MoveTargetsResult> GetMoveTargets(
        [Required] [FromBody] MoveTargetsModel input
    )
    {
        return Ok(moveService.GetMoveTargets(input.Source));
    }

    [HttpPost("MoveNode")]
    [EndpointDescription(
        "Move or copy one model item to another place in the model tree, like drag and drop, "
            + "cut and paste or copy and paste in the Architect. source = {id: <the item>}, "
            + "target = {id: <where it goes>}: a target id from POST /Model/GetMoveTargets, a "
            + "folder id, a top level folder id (a type name, not a GUID) or a parent item id "
            + "such as the entity a field should move to. Do not pass nodeText or "
            + "isNonPersistentItem. A folder of another package is a normal target for both a "
            + "move and a copy: the folder only decides where the item appears in the tree. "
            + "isCopy false MOVES the item: only its place changes, its id stays the same, "
            + "everything that references it keeps working, and the package that owns it stays "
            + "the same. Only items of the active package can be moved. isCopy true COPIES the "
            + "item: the copy gets a new id, it belongs to the ACTIVE package wherever it is "
            + "placed, it is renamed to 'Copy of <name>' when the name is already taken there, "
            + "and the original stays untouched. The change is saved to disk at once, there is "
            + "no separate persist step. Folders themselves cannot be moved or copied, and no "
            + "call changes the package that owns an item - never imitate that by copying and "
            + "deleting, tell the user instead. On success returns {node, sourceLocation, "
            + "parentNodeIds}: node.origamId is the moved item's unchanged id or the new copy's "
            + "id, node.nodeText its name; sourceLocation is where the item was before a move, or "
            + "where the original stays for a copy, with id, nodeText, path and packageName "
            + "(empty for the top folder of the type). When you report a move, give both places "
            + "by their path, and pass sourceLocation.id as target.id when the user wants the "
            + "move undone. An error response carries the reason the model refused, for example "
            + "that the item belongs to another package and can only be copied; when the user "
            + "asked for a move, tell them so and ask before making a copy instead."
    )]
    public ActionResult<MoveNodeResult> MoveNode([Required] [FromBody] MoveNodeModel input)
    {
        return Ok(moveService.Move(input.Source, input.Target, input.IsCopy));
    }
}
