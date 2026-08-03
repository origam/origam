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

namespace Origam.AI.Function.Calling;

public class CreateNodeValidationFilter : IToolInvocationFilter
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string architectBaseUrl;

    public CreateNodeValidationFilter(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    )
    {
        this.httpClientFactory = httpClientFactory;
        architectBaseUrl =
            configuration.GetSection("Architect")["BaseUrl"] ?? "https://localhost:7099";
    }

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

        IReadOnlyList<string>? creatableTypes = await GetCreatableTypesAsync(nodeId);

        if (
            creatableTypes is { Count: > 0 }
            && !creatableTypes.Contains(newTypeName, StringComparer.OrdinalIgnoreCase)
        )
        {
            return BuildRejectionMessage(newTypeName, creatableTypes);
        }

        return await next(context, cancellationToken);
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

    private static string? AsString(object? value)
    {
        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => value?.ToString(),
        };
    }

    private async Task<IReadOnlyList<string>?> GetCreatableTypesAsync(string nodeId)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(value: 10));
            var httpClient = httpClientFactory.CreateClient("architect");
            var requestUri = new Uri(
                new Uri(architectBaseUrl),
                relativeUri: $"Model/GetMenuItems?id={Uri.EscapeDataString(nodeId)}"
            );

            var response = await httpClient.GetAsync(requestUri, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(timeout.Token);
            return ParseTypeNames(json);
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<string>? ParseTypeNames(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var typeNames = new List<string>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (
                    property.Name.Equals(
                        value: "typeName",
                        comparisonType: StringComparison.OrdinalIgnoreCase
                    )
                    && property.Value.ValueKind == JsonValueKind.String
                )
                {
                    var value = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        typeNames.Add(value);
                    }
                }
            }
        }

        return typeNames;
    }

    private static string BuildRejectionMessage(
        string newTypeName,
        IReadOnlyList<string> creatableTypes
    )
    {
        return $"CreateNode was blocked: '{newTypeName}' cannot be created under this parent node. "
            + "Do not retry with this type. The item types that can actually be created here are: "
            + string.Join(separator: ", ", creatableTypes)
            + ". Pick a newTypeName from that list. Note that groups/folders "
            + "(Origam.Schema.SchemaItemGroup) are not created with CreateNode.";
    }
}
