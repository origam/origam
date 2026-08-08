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

using System.Text.Json;
using AGUI.Abstractions;
using Origam.AI.Agent.Services;

namespace Origam.AI.Agent;

public record FocusItem(string Label, string? ItemTypeName = null, string? OrigamId = null);

public record FocusNode(
    string Label,
    string? ItemTypeName = null,
    string? OrigamId = null,
    string? Path = null
);

public record ChatFocus(
    FocusItem? ActiveEditor = null,
    FocusItem? ActiveNode = null,
    IReadOnlyList<FocusItem>? OpenTabs = null,
    IReadOnlyList<FocusNode>? VisibleNodes = null
);

public record AgentRunSettings(
    IReadOnlyList<string> EnabledSections,
    string? Summary,
    ChatFocus? Focus
)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static AgentRunSettings FromRunAgentInput(RunAgentInput? input)
    {
        if (input is null)
        {
            return new AgentRunSettings(
                OpenApiSectionProvider.SafeDefaultSections,
                Summary: null,
                Focus: null
            );
        }

        var forwardedProperties = Deserialize<ForwardedProperties>(input.ForwardedProperties);
        var state = Deserialize<AgentState>(input.State);

        var enabledSections = forwardedProperties?.EnabledSections is { Count: > 0 } sections
            ? sections
            : OpenApiSectionProvider.SafeDefaultSections;

        return new AgentRunSettings(enabledSections, forwardedProperties?.Summary, state?.Focus);
    }

    private static TPayload? Deserialize<TPayload>(JsonElement? element)
        where TPayload : class
    {
        if (element is not { ValueKind: JsonValueKind.Object } value)
        {
            return null;
        }

        try
        {
            return value.Deserialize<TPayload>(JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private record ForwardedProperties(IReadOnlyList<string>? EnabledSections, string? Summary);

    private record AgentState(ChatFocus? Focus);
}
