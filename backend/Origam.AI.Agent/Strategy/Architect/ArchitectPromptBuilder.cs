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
using Origam.AI.Agent.Chat;
using Origam.AI.Agent.Extensions;
using Origam.AI.Agent.Models.Requests;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Strategy.Architect.ItemTypes;
using Origam.AI.Agent.Strategy.Architect.ModelIndex;

namespace Origam.AI.Agent.Strategy.Architect;

public static class ArchitectPromptBuilder
{
    public static List<ChatMessage> Build(
        ArchitectPromptPack prompts,
        ModelIndexContent modelIndex,
        ItemTypePromptSections itemTypeSections,
        AgentRunSettings settings,
        AliasMappingService aliasMappingService,
        string customInstructions
    )
    {
        var messages = new List<ChatMessage>();

        messages.AddSystemMessage(prompts.Replying);
        messages.AddSystemMessage(
            string.Join(
                separator: "\n\n",
                prompts.ToolUse,
                prompts.CommunityWebSearch,
                prompts.Exploration
            )
        );
        messages.AddSystemMessage(prompts.ModelItems);

        if (!string.IsNullOrWhiteSpace(itemTypeSections.Types))
        {
            messages.AddSystemMessage(itemTypeSections.Types);
        }

        if (!string.IsNullOrWhiteSpace(modelIndex.Index))
        {
            messages.AddSystemMessage(prompts.ModelIndexHeader + "\n\n" + modelIndex.Index);
        }

        if (!string.IsNullOrWhiteSpace(modelIndex.Updates))
        {
            messages.AddSystemMessage(modelIndex.Updates);
        }

        var focusMessage = BuildFocusMessage(prompts, settings.Focus, aliasMappingService);
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
            messages.AddSystemMessage(prompts.SessionSummaryHeader + "\n\n" + settings.Summary);
        }

        if (!string.IsNullOrWhiteSpace(customInstructions))
        {
            messages.AddSystemMessage(
                prompts.CustomInstructionsHeader + "\n\n" + customInstructions
            );
        }

        return messages;
    }

    private static string BuildFocusMessage(
        ArchitectPromptPack prompts,
        ChatFocus? focus,
        AliasMappingService aliasMappingService
    )
    {
        if (focus is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine(prompts.FocusHeader);

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
