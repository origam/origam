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
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace Origam.AI.Agent.Services.OpenApi;

public static class OpenApiFunctionBuilder
{
    private const string SchemaReferencePrefix = "#/components/schemas/";

    private static readonly HashSet<string> HttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "get",
        "post",
        "put",
        "patch",
        "delete",
    };

    public static IReadOnlyList<AITool> Build(
        byte[] swaggerBytes,
        string sectionName,
        HashSet<string> selectedPaths,
        string baseUrl,
        IHttpClientFactory httpClientFactory
    )
    {
        var root = JsonNode.Parse(swaggerBytes) as JsonObject;
        var components = root?["components"]?["schemas"] as JsonObject;
        var tools = new List<AITool>();

        if (root?["paths"] is not JsonObject paths)
        {
            return tools;
        }

        var pluginName = SanitizeName(sectionName);

        foreach (var pathEntry in paths)
        {
            if (
                !selectedPaths.Contains(pathEntry.Key) || pathEntry.Value is not JsonObject pathItem
            )
            {
                continue;
            }

            foreach (var methodEntry in pathItem)
            {
                if (
                    !HttpMethods.Contains(methodEntry.Key)
                    || methodEntry.Value is not JsonObject operation
                )
                {
                    continue;
                }

                tools.Add(
                    BuildFunction(
                        pluginName,
                        pathEntry.Key,
                        methodEntry.Key,
                        operation,
                        components,
                        baseUrl,
                        httpClientFactory
                    )
                );
            }
        }

        return tools;
    }

    private static OpenApiFunction BuildFunction(
        string pluginName,
        string path,
        string method,
        JsonObject operation,
        JsonObject? components,
        string baseUrl,
        IHttpClientFactory httpClientFactory
    )
    {
        var properties = new JsonObject();
        var required = new JsonArray();
        var parameters = new List<OpenApiParameter>();

        AppendOperationParameters(operation, components, properties, required, parameters);
        AppendRequestBody(operation, components, properties, required, parameters);

        var schema = new JsonObject { ["type"] = "object", ["properties"] = properties };
        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        return new OpenApiFunction(
            $"{pluginName}-{BuildFunctionName(method, path)}",
            ReadDescription(operation, method, path),
            JsonSerializer.Deserialize<JsonElement>(schema.ToJsonString()),
            new HttpMethod(method.ToUpperInvariant()),
            path,
            baseUrl,
            parameters,
            httpClientFactory
        );
    }

    private static void AppendOperationParameters(
        JsonObject operation,
        JsonObject? components,
        JsonObject properties,
        JsonArray required,
        List<OpenApiParameter> parameters
    )
    {
        if (operation["parameters"] is not JsonArray parameterArray)
        {
            return;
        }

        foreach (var item in parameterArray)
        {
            if (item is not JsonObject parameter)
            {
                continue;
            }

            var parameterName = parameter["name"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                continue;
            }

            var resolved =
                Resolve(
                    parameter["schema"],
                    components,
                    new HashSet<string>(StringComparer.Ordinal)
                ) as JsonObject
                ?? new JsonObject { ["type"] = "string" };

            if (
                parameter["description"]?.GetValue<string>() is string parameterDescription
                && resolved["description"] is null
            )
            {
                resolved["description"] = parameterDescription;
            }

            properties[parameterName] = resolved;

            if (parameter["required"]?.GetValue<bool>() == true)
            {
                required.Add(parameterName);
            }

            var location = parameter["in"]?.GetValue<string>();
            parameters.Add(
                new OpenApiParameter(
                    parameterName,
                    string.Equals(
                        location,
                        b: "path",
                        comparisonType: StringComparison.OrdinalIgnoreCase
                    )
                        ? OpenApiParameterLocation.Path
                        : OpenApiParameterLocation.Query
                )
            );
        }
    }

    private static void AppendRequestBody(
        JsonObject operation,
        JsonObject? components,
        JsonObject properties,
        JsonArray required,
        List<OpenApiParameter> parameters
    )
    {
        var bodySchema = operation["requestBody"]?["content"]?["application/json"]?["schema"];
        if (bodySchema is null)
        {
            return;
        }

        if (
            Resolve(bodySchema, components, new HashSet<string>(StringComparer.Ordinal))
            is not JsonObject resolved
        )
        {
            return;
        }

        if (resolved["properties"] is not JsonObject bodyProperties)
        {
            return;
        }

        foreach (var property in bodyProperties)
        {
            properties[property.Key] = property.Value?.DeepClone();
            parameters.Add(new OpenApiParameter(property.Key, OpenApiParameterLocation.Body));
        }

        if (resolved["required"] is JsonArray bodyRequired)
        {
            foreach (var item in bodyRequired)
            {
                if (item?.GetValue<string>() is string name)
                {
                    required.Add(name);
                }
            }
        }
    }

    private static JsonNode? Resolve(
        JsonNode? node,
        JsonObject? components,
        HashSet<string> visiting
    )
    {
        if (node is JsonArray array)
        {
            var copy = new JsonArray();
            foreach (var item in array)
            {
                copy.Add(Resolve(item, components, visiting));
            }

            return copy;
        }

        if (node is not JsonObject objectNode)
        {
            return node?.DeepClone();
        }

        if (objectNode["$ref"]?.GetValue<string>() is string reference)
        {
            if (!reference.StartsWith(SchemaReferencePrefix, StringComparison.Ordinal))
            {
                return new JsonObject { ["type"] = "object" };
            }

            var schemaName = reference.Substring(SchemaReferencePrefix.Length);
            if (!visiting.Add(schemaName))
            {
                return new JsonObject { ["type"] = "object" };
            }

            var resolved = Resolve(components?[schemaName], components, visiting);
            visiting.Remove(schemaName);
            return resolved ?? new JsonObject { ["type"] = "object" };
        }

        var result = new JsonObject();
        foreach (var property in objectNode)
        {
            result[property.Key] = Resolve(property.Value, components, visiting);
        }

        return result;
    }

    private static string ReadDescription(JsonObject operation, string method, string path)
    {
        if (operation["description"]?.GetValue<string>() is string description)
        {
            return description;
        }

        if (operation["summary"]?.GetValue<string>() is string summary)
        {
            return summary;
        }

        return $"{method.ToUpperInvariant()} {path}";
    }

    private static string BuildFunctionName(string method, string path)
    {
        var builder = new StringBuilder();
        builder.Append(Capitalize(method));
        foreach (
            var segment in path.Split(
                separator: '/',
                options: StringSplitOptions.RemoveEmptyEntries
            )
        )
        {
            builder.Append(Capitalize(SanitizeName(segment)));
        }

        return builder.ToString();
    }

    private static string SanitizeName(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray());
    }

    private static string Capitalize(string value)
    {
        return value.Length == 0
            ? value
            : char.ToUpperInvariant(value[0]) + value.Substring(startIndex: 1).ToLowerInvariant();
    }
}
