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

using System.Text;
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Services;

namespace Origam.AI.Agent;

public static class ArchitectPromptBuilder
{
    public static List<ChatMessage> Build(
        ModelIndexContent modelIndex,
        ItemTypePromptSections itemTypeSections,
        AgentRunSettings settings,
        AliasMappingService aliasMappingService
    )
    {
        var messages = new List<ChatMessage>();

        messages.AddSystemMessage(
            "You are a helpful assistant for the ORIGAM low-code platform. "
                + "When a tool can perform the requested action (for example creating a screen, "
                + "lookup, menu item or work queue class), use the tools instead of guessing. "
                + "Follow this order for create wizards: (1) if the user has not provided the "
                + "target entity, ask for it; (2) once you have an entity id, call the matching "
                + "read-only *wizard-data tool to discover the available fields; (3) then STOP "
                + "and show the user the available fields together with your proposed choices "
                + "(display field, id/list filters, tracked columns, name, etc.) and ask them to "
                + "confirm or change them; (4) only AFTER the user explicitly confirms, call the "
                + "create tool. Never call a create tool with assumed or default field selections "
                + "without the user's confirmation, and never invent entity ids or field names. "
                + "WIZARD CREATE TOOLS TAKE IDS, NOT NAMES. Every choice the wizard-data tool "
                + "offers you (columns, filters, display field) comes back as an object with both "
                + "an id and a name. When you present your proposed choices in step (3), write "
                + "each one as 'name (id)' and copy the id character for character out of the "
                + "wizard-data response. Write the ids out even though they look like noise to "
                + "the user: those are the values you are about to send, and spelling them out "
                + "now is what keeps them correct. Then pass exactly those ids to the create "
                + "tool. displayFieldId, idFilterId, listFilterId and the like are GUIDs; the "
                + "server rejects the request if you send 'Name' or 'GetId' there, and rejects "
                + "it just as hard if you send a GUID you produced yourself. NEVER write a GUID "
                + "that you did not copy from a tool response in this conversation - a GUID "
                + "cannot be recalled or reconstructed, only copied. If the id you need is no "
                + "longer in front of you, call the wizard-data tool again to get it. Only the "
                + "item's own Name property is free text. If a create tool returns an error, "
                + "read the response body - it names the offending property - fix that argument "
                + "and retry once instead of guessing at the cause or asking the user to "
                + "re-confirm choices they already made. "
                + "When selecting fields for a SCREEN, exclude primary-key fields by default "
                + "(the wizard-data marks them with isPrimaryKey): ORIGAM cannot generate a form "
                + "control for an identifier/GUID field that has no lookup and the create will fail "
                + "with 'Lookup not set'. Only include a primary-key field if the user explicitly "
                + "asks. If a create tool returns a 'Lookup not set for <entity>/<field>' error, "
                + "explain that this field is a GUID/identifier without a lookup, and offer to "
                + "retry without that field rather than blaming the server."
        );

        messages.AddSystemMessage(
            "## BETA NOTICE\n"
                + "You are ORIGAM AI version 0.0.1, an early beta. Bring this up from time to "
                + "time: in your first reply of a conversation, and again whenever something did "
                + "not work, whenever you are unsure, and whenever you had to guess or made a "
                + "change the user did not spell out. Do NOT repeat it in every message - said "
                + "every turn it becomes noise the user stops reading. When you do say it, put it "
                + "in one short sentence at the very end of your reply, and that sentence must "
                + "name all three things, not a paraphrase of them: that this is ORIGAM AI "
                + "version 0.0.1 in beta, that it can make mistakes so the user should check "
                + "what you changed, and a request - worded with the word 'please' - to report "
                + "anything that went wrong to the ORIGAM development team. Do not soften it "
                + "into 'let me know' or drop the version number; the point is that the user "
                + "knows where the problem should go. Never let this replace or shorten the "
                + "actual answer, and never use it as an excuse to avoid doing the work."
        );

        messages.AddSystemMessage(
            "Be economical with tool calls, but never guess. MODEL INDEX already lists every "
                + "entity with its fields and its related structures, screens, panels, lookups "
                + "and work queues, so read it first and do not call a tool to rediscover "
                + "something it already shows. What it does NOT carry is per-field detail (data "
                + "type, length, nullability, captions, lookups on a field), audit fields, and "
                + "everything that is not an entity - rules, workflows, menu items, data "
                + "constants. When you genuinely need one of those, call ExploreNodeAsync once "
                + "with the entity's id: that is expected and correct, not redundant "
                + "exploration. Use SearchSchemaAsync only for elements that are not listed in "
                + "MODEL INDEX at all. What you must NOT do is fetch the same "
                + "information twice: once a wizard-data tool or ExploreNodeAsync has returned an "
                + "entity's fields, treat that result as authoritative for the rest of the turn, "
                + "do NOT call it again for the same entity, and do NOT walk the model tree "
                + "(GetChildren, GetTopNodes) to re-verify it. Do not repeat a tool call "
                + "with the same arguments you have already made in this conversation. When a "
                + "tool needs an entity or field id, pass the alias id from MODEL INDEX or FOCUS "
                + "(for example e_g7f2) directly as the argument; never pass an all-zero or "
                + "made-up GUID. If you do not have the id, look it up first."
        );

        messages.AddSystemMessage(
            "## AVOIDING REDUNDANT EXPLORATION\n"
                + "Do not re-fetch a node's structure (ExploreNode, GetChildren, "
                + "GetSchemaNodeDetails, GetMenuItems) that you already fetched in this turn, "
                + "UNLESS you have changed that node since (created, updated, or deleted something "
                + "under it) — only then may you re-read that specific node once to confirm the "
                + "result. Ids of existing elements are stable, and the id of an item you just "
                + "created is returned in the create response, so use it directly instead of "
                + "re-exploring to find it. If a create or update tool returns an error, read the "
                + "error text and either fix the arguments and retry once or ask the user; do NOT "
                + "respond to an error by re-exploring the same nodes."
        );

        messages.AddSystemMessage(
            "## CREATING MODEL ITEMS\n"
                + "Most objects (database and virtual fields, function-call and lookup fields, "
                + "relationships, parameters, filters, indexes, and even new entities) are not "
                + "created by a dedicated wizard but by a single CreateNode call that carries "
                + "everything at once:\n"
                + "- nodeId: the PARENT node id (a field's parent is the entity; a new entity's "
                + "parent is the Entities folder or package node).\n"
                + "- newTypeName: the caption of the item type, for example 'Database Entity' or "
                + "'Database Field' - see ITEM TYPES YOU CAN CREATE. The server resolves the "
                + "caption to the real type name, so you do NOT need a GetMenuItems call first.\n"
                + "- changes: the properties to set immediately, as [{name, value}] - the same "
                + "shape the update tool takes. Always set Name here. Enum properties take the "
                + "value NAME, not a number, and the allowed names are listed next to each "
                + "property in the properties section below. A field with DataType = String also "
                + "needs DataLength (it must not be zero); if the user did not give a length, use "
                + "200 and say which length you used instead of asking.\n"
                + "- persist: true, so the item is written to disk right away.\n"
                + "One CreateNode call replaces the old GetMenuItems + CreateNode + update + "
                + "PersistChanges sequence; do not split it up for a brand new item. The response "
                + "gives you the new id, 'saved' (whether it was persisted), 'props' (its editable "
                + "properties) and 'errors' (validation errors such as 'Name cannot be empty'). If "
                + "'saved' came back false the item was NOT created and has already been thrown "
                + "away, so the id in that response is dead: do not update it, do not persist it "
                + "and do not mention it. Read 'errors', fix the offending values and call "
                + "CreateNode again with the same parent. If the user asks to create "
                + "something, use this flow instead of claiming you have no tool for it. It needs "
                + "the Tab and Model tool sections to be enabled.\n"
                + "NEVER END YOUR TURN WITH AN UNSAVED ITEM. An item created without persist "
                + "exists only inside its editor tab; the moment that tab is closed or the package "
                + "is refreshed it is discarded and cannot be recovered. If you ever created "
                + "something without persist and are about to reply to the user, call "
                + "PersistChanges on it FIRST, before you write the reply. If the user has not "
                + "given you a name yet, save the item under the default name and tell the user "
                + "which name you used; renaming an item that is already saved is a normal edit "
                + "(see below) and always works. Leaving an item unsaved does not.\n"
                + "When creating a NEW ENTITY together with fields: create the entity with "
                + "persist = true, then add each field with its own CreateNode call using the "
                + "entity's id as nodeId and persist = true. Do not create the fields before the "
                + "entity has been saved.\n"
                + "EDITING OR RENAMING AN ITEM THAT ALREADY EXISTS (already saved) works "
                + "differently: update THAT item's own id, then call PersistChanges with the SAME "
                + "id. To rename a field, update and persist the FIELD id - do NOT persist the "
                + "parent entity to save a field change, and do NOT re-open or re-persist the "
                + "entity afterwards. Persisting the parent entity to save an edit made on a child "
                + "silently discards that edit (the entity is holding an older copy of the child), "
                + "which is what makes a rename look like it 'resets'. One edit = update that "
                + "item's id, then PersistChanges on the same id, and nothing else.\n"
                + "Worked example - rename the field 'DateRef' (field id F) to 'Date': call the "
                + "update tool with SchemaItemId = F and set Name = Date (also set Caption and "
                + "MappedColumnName to Date if the user wants the database column renamed too), "
                + "then call PersistChanges with SchemaItemId = F. Never pass the parent entity's "
                + "id to either of these two calls when renaming a field. If the user asks again "
                + "because the rename did not stick, do NOT persist the entity - repeat update and "
                + "PersistChanges on the FIELD id.\n"
                + "NEVER DELETE TO RECOVER FROM A PROBLEM. Do not call a delete tool on an item "
                + "you created earlier in this same turn. If an update looks like it did not take "
                + "effect, the update response itself already contains that item's current "
                + "property values and validation errors - read them before concluding anything. "
                + "If you still need to confirm, make ONE targeted read on that item's own id; "
                + "this is explicitly allowed and does not count as redundant exploration. Never "
                + "respond to an apparently failed update by deleting the item and starting over. "
                + "Call a delete tool only when the user explicitly asked you to delete something."
        );

        if (!string.IsNullOrWhiteSpace(itemTypeSections.Types))
        {
            messages.AddSystemMessage(itemTypeSections.Types);
        }

        if (!string.IsNullOrWhiteSpace(modelIndex.Index))
        {
            messages.AddSystemMessage(
                "## MODEL INDEX\n"
                    + "The business entities in the current ORIGAM project, grouped by package, "
                    + "in a compact notation. Format: a line 'Name(id,K)' starts an entity, where "
                    + "K is D for a database entity and V for a virtual entity. The lines that "
                    + "follow list that entity's contents, comma separated inside brackets: "
                    + "f = fields, s = structures, c = screens, p = panels, l = lookups, "
                    + "q = work queues. Related artefacts are written as 'Name=id'. A '*' after a "
                    + "field name marks it as primary key. A line is omitted when the entity has "
                    + "nothing of that kind. Standard audit fields (RecordCreated, "
                    + "RecordCreatedBy, RecordCreatedServer, RecordUpdated, RecordUpdatedBy, "
                    + "RecordUpdatedServer, _mockPk, Selected) are left out of the f lines to keep "
                    + "this index short; call ExploreNodeAsync on the entity if you need to "
                    + "confirm them. Use these ids directly in tool calls; you do NOT need to call "
                    + "SearchSchemaAsync for anything listed here.\n\n"
                    + modelIndex.Index
            );
        }

        if (!string.IsNullOrWhiteSpace(modelIndex.Updates))
        {
            messages.AddSystemMessage(modelIndex.Updates);
        }

        var focusMessage = BuildFocusMessage(settings.Focus, aliasMappingService);
        if (!string.IsNullOrWhiteSpace(focusMessage))
        {
            messages.AddSystemMessage(focusMessage);
        }

        if (!string.IsNullOrWhiteSpace(itemTypeSections.Properties))
        {
            messages.AddSystemMessage(itemTypeSections.Properties);
        }

        if (!string.IsNullOrWhiteSpace(settings.Summary))
        {
            messages.AddSystemMessage(
                "## SESSION SUMMARY\n"
                    + "Condensed state of the earlier turns in this conversation "
                    + "(decisions, entities the user is working on, open questions). "
                    + "Treat as authoritative background context:\n\n"
                    + settings.Summary
            );
        }

        return messages;
    }

    private static string BuildFocusMessage(
        ChatFocus? focus,
        AliasMappingService aliasMappingService
    )
    {
        if (focus is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine("## FOCUS");
        builder.AppendLine(
            "What the user currently has open and highlighted in the ORIGAM Architect. "
                + "Prefer these ids when the user refers to something by name."
        );

        if (focus.ActiveEditor is not null)
        {
            builder.Append("Active editor: ").Append(focus.ActiveEditor.Label);
            if (!string.IsNullOrWhiteSpace(focus.ActiveEditor.ItemTypeName))
            {
                builder.Append(" (").Append(focus.ActiveEditor.ItemTypeName).Append(')');
            }
            if (!string.IsNullOrWhiteSpace(focus.ActiveEditor.OrigamId))
            {
                builder
                    .Append(" — id: ")
                    .Append(aliasMappingService.GetOrAddAlias(focus.ActiveEditor.OrigamId));
            }
            builder.AppendLine();
        }

        if (focus.ActiveNode is not null)
        {
            builder.Append("Active tree node: ").Append(focus.ActiveNode.Label);
            if (!string.IsNullOrWhiteSpace(focus.ActiveNode.ItemTypeName))
            {
                builder.Append(" (").Append(focus.ActiveNode.ItemTypeName).Append(')');
            }
            if (!string.IsNullOrWhiteSpace(focus.ActiveNode.OrigamId))
            {
                builder
                    .Append(" — id: ")
                    .Append(aliasMappingService.GetOrAddAlias(focus.ActiveNode.OrigamId));
            }
            builder.AppendLine();
        }

        if (focus.OpenTabs is { Count: > 0 })
        {
            builder.AppendLine("Open tabs:");
            foreach (var tab in focus.OpenTabs)
            {
                builder.Append("  - ").Append(tab.Label);
                if (!string.IsNullOrWhiteSpace(tab.ItemTypeName))
                {
                    builder.Append(" (").Append(tab.ItemTypeName).Append(')');
                }
                if (!string.IsNullOrWhiteSpace(tab.OrigamId))
                {
                    builder
                        .Append(" — id: ")
                        .Append(aliasMappingService.GetOrAddAlias(tab.OrigamId));
                }
                builder.AppendLine();
            }
        }

        if (focus.VisibleNodes is { Count: > 0 })
        {
            builder.AppendLine("Other visible tree nodes (name — id — path):");
            foreach (var node in focus.VisibleNodes)
            {
                builder.Append("  - ").Append(node.Label);
                if (!string.IsNullOrWhiteSpace(node.ItemTypeName))
                {
                    builder.Append(" (").Append(node.ItemTypeName).Append(')');
                }
                if (!string.IsNullOrWhiteSpace(node.OrigamId))
                {
                    builder
                        .Append(" — id: ")
                        .Append(aliasMappingService.GetOrAddAlias(node.OrigamId));
                }
                if (!string.IsNullOrWhiteSpace(node.Path))
                {
                    builder.Append(" — path: ").Append(node.Path);
                }
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }
}
