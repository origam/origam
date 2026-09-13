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
using Origam.AI.Agent.Models.Responses;

namespace Origam.AI.Agent.Tests.Infrastructure.Agent;

public sealed record AgentRunTrace(
    IReadOnlyList<ToolCallRecord> ToolCalls,
    string ReplyText,
    RunResult? Result,
    string? ErrorMessage,
    TimeSpan Duration
)
{
    private const int MaxToolTextLength = 500;

    public IReadOnlyList<string> ToolNames => ToolCalls.Select(toolCall => toolCall.Name).ToList();

    public RunUsage Usage =>
        Result?.Usage
        ?? new RunUsage(PromptTokens: 0, CompletionTokens: 0, TotalTokens: 0, CachedTokens: 0);

    public bool UsedTool(string toolNameFragment)
    {
        return FirstToolIndex(toolNameFragment) >= 0;
    }

    public int FirstToolIndex(string toolNameFragment)
    {
        for (var index = 0; index < ToolCalls.Count; index++)
        {
            if (
                ToolCalls[index]
                    .Name.Contains(
                        value: toolNameFragment,
                        comparisonType: StringComparison.OrdinalIgnoreCase
                    )
            )
            {
                return index;
            }
        }

        return -1;
    }

    public string DescribeToolNames()
    {
        return ToolCalls.Count == 0 ? "(none)" : string.Join(separator: ", ", ToolNames);
    }

    public string Describe()
    {
        var transcript = new StringBuilder();
        transcript.AppendLine("----- agent run -----");

        if (ToolCalls.Count == 0)
        {
            transcript.AppendLine("tools: (none)");
        }
        foreach (var toolCall in ToolCalls)
        {
            transcript.AppendLine($"tool {toolCall.Name}({Shorten(toolCall.Arguments)})");
            transcript.AppendLine("  -> " + Shorten(toolCall.Result ?? "(no result)"));
        }

        transcript.AppendLine("llm reply:");
        var reply = ReplyText.Trim();
        transcript.AppendLine(reply.Length == 0 ? "(the model returned no text)" : reply);

        if (ErrorMessage is not null)
        {
            transcript.AppendLine("run error: " + ErrorMessage);
        }

        transcript.Append("---------------------");
        return transcript.ToString();
    }

    private static string Shorten(string text)
    {
        var singleLine = text.ReplaceLineEndings(" ").Trim();
        return singleLine.Length <= MaxToolTextLength
            ? singleLine
            : singleLine[..MaxToolTextLength]
                + $"… (+{singleLine.Length - MaxToolTextLength} chars)";
    }
}
