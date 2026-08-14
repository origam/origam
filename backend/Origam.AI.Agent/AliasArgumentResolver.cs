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
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Services;

namespace Origam.AI.Agent;

public class AliasArgumentResolver : IToolInvocationFilter
{
    public const string ResolveAliasesProperty = "resolveAliases";

    private static readonly Regex AliasPattern = new(
        pattern: "^[A-Za-z]{1,4}_[0-9a-z]{1,10}$",
        options: RegexOptions.Compiled
    );

    private readonly AliasMappingService aliasMappingService;

    public AliasArgumentResolver(AliasMappingService aliasMappingService)
    {
        this.aliasMappingService = aliasMappingService;
    }

    public async ValueTask<object?> OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        ToolInvocation next,
        CancellationToken cancellationToken
    )
    {
        if (
            context.Function.AdditionalProperties.TryGetValue(
                ResolveAliasesProperty,
                out var resolveAliases
            ) && resolveAliases is false
        )
        {
            return await next(context, cancellationToken);
        }

        foreach (var name in context.Arguments.Keys.ToArray())
        {
            if (!context.Arguments.TryGetValue(name, out var original))
            {
                continue;
            }

            var resolved = ResolveValue(original);
            if (!ReferenceEquals(resolved, original))
            {
                context.Arguments[name] = resolved;
            }
        }

        return await next(context, cancellationToken);
    }

    private object? ResolveValue(object? value)
    {
        switch (value)
        {
            case string text:
                return ResolveAlias(text);
            case JsonElement element:
                return ResolveJsonElement(element);
            case IEnumerable<object?> items:
                return items.Select(ResolveValue).ToList();
            default:
                return value;
        }
    }

    private object? ResolveJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
            {
                var text = element.GetString();
                return text is null ? element : ResolveAlias(text);
            }
            case JsonValueKind.Array:
            {
                var items = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    items.Add(ResolveJsonElement(item));
                }
                return items;
            }
            case JsonValueKind.Object:
            {
                var properties = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject())
                {
                    properties[property.Name] = ResolveJsonElement(property.Value);
                }
                return properties;
            }
            default:
                return element;
        }
    }

    private object ResolveAlias(string text)
    {
        if (!AliasPattern.IsMatch(text))
        {
            return text;
        }

        return aliasMappingService.ResolveUuid(text);
    }
}
