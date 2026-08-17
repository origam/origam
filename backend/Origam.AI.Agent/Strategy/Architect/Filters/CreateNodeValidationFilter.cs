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
using Origam.AI.Agent.Invocation;
using Origam.AI.Agent.Strategy.Architect.Api;
using Origam.AI.Agent.Strategy.Architect.ItemTypes;

namespace Origam.AI.Agent.Strategy.Architect.Filters;

public class CreateNodeValidationFilter(
    ArchitectApiClient architectApi,
    NewItemTypeCatalogService catalogService,
    ArchitectPromptPack prompts
) : IToolInvocationFilter
{
    private static readonly TimeSpan MenuItemsTimeout = TimeSpan.FromSeconds(value: 10);

    public async ValueTask<object?> OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        ToolInvocation next,
        CancellationToken cancellationToken
    )
    {
        string? nodeId = GetArgument(context.Arguments, name: "nodeId");
        string? newTypeName = GetArgument(context.Arguments, name: "newTypeName");

        if (string.IsNullOrWhiteSpace(nodeId) || string.IsNullOrWhiteSpace(newTypeName))
        {
            return await next(context, cancellationToken);
        }

        var creatableTypes = await GetCreatableTypesAsync(nodeId, cancellationToken);

        if (creatableTypes is not { Count: > 0 })
        {
            return await next(context, cancellationToken);
        }

        string? resolvedTypeName = ResolveTypeName(newTypeName, creatableTypes);
        if (resolvedTypeName is null)
        {
            return BuildRejectionMessage(newTypeName, creatableTypes);
        }

        if (!string.Equals(resolvedTypeName, newTypeName, StringComparison.Ordinal))
        {
            SetArgument(context.Arguments, name: "newTypeName", resolvedTypeName);
        }

        string? emptyRequired = FindEmptyRequiredProperty(context.Arguments, resolvedTypeName);
        if (emptyRequired is not null)
        {
            return emptyRequired;
        }

        return await next(context, cancellationToken);
    }

    private string? FindEmptyRequiredProperty(AIFunctionArguments arguments, string typeName)
    {
        ItemType? type = catalogService.CachedCatalog?.Types.FirstOrDefault(candidate =>
            string.Equals(candidate.TypeName, typeName, StringComparison.Ordinal)
        );
        if (type is null)
        {
            return null;
        }

        foreach (var (name, value) in ReadChanges(arguments))
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            ItemTypeProperty? property = type.Properties.FirstOrDefault(candidate =>
                candidate.Required
                && string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)
            );
            if (property is null)
            {
                continue;
            }

            string suggestion = string.IsNullOrWhiteSpace(property.CommonValue)
                ? prompts.CreateNodeSuggestAnyValue
                : string.Format(
                    prompts.CreateNodeSuggestCommonValue,
                    type.Caption,
                    property.CommonValue
                );

            return string.Format(prompts.CreateNodeEmptyRequired, name, type.Caption, suggestion);
        }

        return null;
    }

    private static IEnumerable<(string Name, string? Value)> ReadChanges(
        AIFunctionArguments arguments
    )
    {
        string? raw = GetArgument(arguments, name: "changes");
        if (string.IsNullOrWhiteSpace(raw))
        {
            yield break;
        }

        JsonElement parsed;
        try
        {
            parsed = JsonDocument.Parse(raw).RootElement;
        }
        catch (JsonException)
        {
            yield break;
        }

        if (parsed.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (JsonElement change in parsed.EnumerateArray())
        {
            if (change.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string? name = change.GetStringOrNullIgnoreCase(propertyName: "name");
            if (name is not null)
            {
                yield return (name, ReadValue(change));
            }
        }
    }

    private static string? ReadValue(JsonElement change)
    {
        if (!change.TryGetProperty(propertyName: "value", out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => value.GetString(),
            _ => value.GetRawText(),
        };
    }

    private static string? ResolveTypeName(
        string requested,
        IReadOnlyList<CreatableType> creatableTypes
    )
    {
        string trimmed = requested.Trim();

        CreatableType? exactMatch = creatableTypes.FirstOrDefault(type =>
            type.TypeName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
            || type.Caption.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
            || ShortName(type.TypeName).Equals(trimmed, StringComparison.OrdinalIgnoreCase)
        );
        if (exactMatch is not null)
        {
            return exactMatch.TypeName;
        }

        string normalized = Normalize(trimmed);
        if (normalized.Length == 0)
        {
            return null;
        }

        var normalizedMatches = creatableTypes
            .Where(type =>
                Normalize(type.Caption) == normalized
                || Normalize(ShortName(type.TypeName)) == normalized
            )
            .ToList();
        if (normalizedMatches.Count == 1)
        {
            return normalizedMatches[0].TypeName;
        }

        var partialMatches = creatableTypes
            .Where(type =>
                Normalize(type.Caption).Contains(normalized, StringComparison.Ordinal)
                || Normalize(type.TypeName).Contains(normalized, StringComparison.Ordinal)
            )
            .ToList();

        return partialMatches.Count == 1 ? partialMatches[0].TypeName : null;
    }

    private static string ShortName(string typeName)
    {
        int lastDot = typeName.LastIndexOf(value: '.');
        return lastDot < 0 ? typeName : typeName.Substring(lastDot + 1);
    }

    private static string Normalize(string value)
    {
        return new string(
            value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray()
        );
    }

    private static string? GetArgument(AIFunctionArguments arguments, string name)
    {
        foreach (var pair in arguments)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return AsString(pair.Value);
            }
        }

        return null;
    }

    private static void SetArgument(AIFunctionArguments arguments, string name, string value)
    {
        string? existingKey = arguments
            .Keys.ToArray()
            .FirstOrDefault(key => string.Equals(key, name, StringComparison.OrdinalIgnoreCase));
        if (existingKey is not null)
        {
            arguments[existingKey] = value;
        }
    }

    private static string? AsString(object? value)
    {
        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => value?.ToString(),
        };
    }

    private async Task<IReadOnlyList<CreatableType>?> GetCreatableTypesAsync(
        string nodeId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(MenuItemsTimeout);

            var response = await architectApi.GetMenuItemsAsync(nodeId, timeout.Token);
            return response.IsSuccess ? ParseCreatableTypes(response.Body) : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<CreatableType>? ParseCreatableTypes(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var creatableTypes = new List<CreatableType>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string? typeName = element.GetStringOrNullIgnoreCase(propertyName: "typeName");
            if (string.IsNullOrWhiteSpace(typeName))
            {
                continue;
            }

            string caption = element.GetStringOrNullIgnoreCase(propertyName: "caption") ?? typeName;
            creatableTypes.Add(new CreatableType(caption, typeName));
        }

        return creatableTypes;
    }

    private string BuildRejectionMessage(
        string newTypeName,
        IReadOnlyList<CreatableType> creatableTypes
    )
    {
        return string.Format(
            prompts.CreateNodeTypeRejected,
            newTypeName,
            string.Join(separator: ", ", creatableTypes.Select(type => type.Caption))
        );
    }

    private record CreatableType(string Caption, string TypeName);
}
