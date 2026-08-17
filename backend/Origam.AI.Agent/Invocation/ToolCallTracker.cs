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
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Extensions;
using Origam.AI.Agent.Models.Responses;

namespace Origam.AI.Agent.Invocation;

public class ToolCallTracker(int maxIterations) : IToolInvocationFilter
{
    private static readonly string[] MutatingVerbs =
    [
        "create",
        "persist",
        "update",
        "delete",
        "save",
        "remove",
    ];

    private static readonly string[] TargetArgumentNames = ["schemaItemId", "SchemaItemId"];

    private readonly List<string> invokedFunctions = new();
    private readonly List<AffectedNode> affectedNodes = new();

    public IReadOnlyList<string> InvokedFunctions => invokedFunctions;
    public IReadOnlyList<AffectedNode> AffectedNodes => affectedNodes;
    public bool ModelChanged { get; private set; }
    public bool LimitReached { get; private set; }

    public async ValueTask<object?> OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        ToolInvocation next,
        CancellationToken cancellationToken
    )
    {
        invokedFunctions.Add(context.Function.Name);

        if (context.Iteration >= maxIterations - 1)
        {
            LimitReached = true;
        }

        var result = await next(context, cancellationToken);

        try
        {
            Inspect(context, result);
        }
        catch
        {
            // Result inspection is best-effort telemetry; never break the chat over it.
        }

        return result;
    }

    private void Inspect(FunctionInvocationContext context, object? result)
    {
        var functionName = context.Function.Name;
        var isMutating = MutatingVerbs.Any(verb =>
            functionName.Contains(value: verb, comparisonType: StringComparison.OrdinalIgnoreCase)
        );

        var content = ExtractContent(result);
        var extractedFromResult = false;

        if (content is not null && LooksLikeJsonObject(content))
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (
                root.TryGetProperty(propertyName: "node", out var nodeElement)
                && nodeElement.ValueKind == JsonValueKind.Object
                && !WasDiscarded(root)
            )
            {
                var origamId = nodeElement.GetStringOrNull(propertyName: "origamId");
                if (IsGuid(origamId))
                {
                    AddNode(
                        origamId!,
                        nodeElement.GetStringOrNull(propertyName: "nodeText"),
                        nodeElement.GetStringOrNull(propertyName: "itemTypeName"),
                        action: "created"
                    );
                    extractedFromResult = true;
                    ModelChanged = true;
                }
            }

            if (
                root.TryGetProperty(propertyName: "searchResults", out var resultsElement)
                && resultsElement.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var resultElement in resultsElement.EnumerateArray())
                {
                    var schemaId = resultElement.GetStringOrNull(propertyName: "schemaId");
                    if (IsGuid(schemaId))
                    {
                        AddNode(
                            schemaId!,
                            resultElement.GetStringOrNull(propertyName: "foundIn"),
                            resultElement.GetStringOrNull(propertyName: "type"),
                            action: "created"
                        );
                        extractedFromResult = true;
                        ModelChanged = true;
                    }
                }
            }
        }

        if (isMutating)
        {
            ModelChanged = true;
            if (!extractedFromResult)
            {
                var targetId = GetTargetArgumentId(context);
                if (IsGuid(targetId))
                {
                    AddNode(
                        targetId!,
                        label: null,
                        itemTypeName: null,
                        ClassifyAction(functionName)
                    );
                }
            }
        }
    }

    private static bool WasDiscarded(JsonElement root)
    {
        return root.TryGetProperty(propertyName: "discarded", out var discarded)
            && discarded.ValueKind == JsonValueKind.True;
    }

    private void AddNode(string origamId, string? label, string? itemTypeName, string action)
    {
        var normalizedLabel = string.IsNullOrWhiteSpace(label) ? null : label;
        var normalizedType = string.IsNullOrWhiteSpace(itemTypeName) ? null : itemTypeName;

        var existing = affectedNodes.FirstOrDefault(node => node.OrigamId == origamId);
        if (existing is not null)
        {
            if (existing.Label is null && normalizedLabel is not null)
            {
                affectedNodes.Remove(existing);
                affectedNodes.Add(
                    existing with
                    {
                        Label = normalizedLabel,
                        ItemTypeName = normalizedType ?? existing.ItemTypeName,
                    }
                );
            }
            return;
        }

        affectedNodes.Add(new AffectedNode(origamId, normalizedLabel, normalizedType, action));
    }

    private static string ClassifyAction(string functionName)
    {
        if (
            functionName.Contains(
                value: "delete",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
            || functionName.Contains(
                value: "remove",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return "deleted";
        }
        return "updated";
    }

    private static string? GetTargetArgumentId(FunctionInvocationContext context)
    {
        foreach (var argumentName in TargetArgumentNames)
        {
            if (context.Arguments.TryGetValue(argumentName, out var value) && value is not null)
            {
                return AsString(value);
            }
        }
        return null;
    }

    private static string? AsString(object value)
    {
        return value is JsonElement { ValueKind: JsonValueKind.String } element
            ? element.GetString()
            : value.ToString();
    }

    private static string? ExtractContent(object? value)
    {
        switch (value)
        {
            case null:
                return null;
            case string text:
                return text;
            case JsonElement element:
                return element.GetRawText();
        }

        var contentProperty = value.GetType().GetProperty(name: "Content");
        if (contentProperty is not null)
        {
            var content = contentProperty.GetValue(value);
            return content switch
            {
                null => null,
                string text => text,
                JsonElement element => element.GetRawText(),
                _ => content.ToString(),
            };
        }

        return value.ToString();
    }

    private static bool IsGuid(string? value)
    {
        return Guid.TryParse(value, out var parsed) && parsed != Guid.Empty;
    }

    private static bool LooksLikeJsonObject(string content)
    {
        return content.TrimStart().StartsWith(value: '{');
    }
}
