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
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Origam.AI.Agent;

public record RunUsage(int PromptTokens, int CompletionTokens, int TotalTokens, int CachedTokens);

public record RunResult(
    bool ModelChanged,
    IReadOnlyList<AffectedNode> AffectedNodes,
    bool ToolLimitReached,
    string? UpdatedSummary,
    RunUsage Usage
);

public static class AguiEvents
{
    public const string RunResultName = "origam.agent.runResult";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static AgentResponseUpdate Create(string name, object payload)
    {
        var customEvent = new CustomEvent
        {
            Name = name,
            Value = JsonSerializer.SerializeToElement(payload, JsonOptions),
        };

        return new AgentResponseUpdate(new ChatResponseUpdate { RawRepresentation = customEvent });
    }
}
