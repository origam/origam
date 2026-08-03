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
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using Origam.AI.Function.Calling.Services.OpenApi;

namespace Origam.AI.Function.Calling.Services;

public record SectionOperation(string Method, string Path, string DisplayName, bool Destructive);

public class ApiSection
{
    public required string Name { get; init; }
    public required IReadOnlyList<SectionOperation> Operations { get; init; }
    public required HashSet<string> Paths { get; init; }
    public bool HasDestructive => Operations.Any(operation => operation.Destructive);
    public IReadOnlyList<AITool>? Tools { get; set; }
}

public record SectionInfo(
    string Name,
    int FunctionCount,
    IReadOnlyList<string> Functions,
    bool HasDestructive
);

public class OpenApiSectionProvider
{
    public static readonly IReadOnlyList<string> SafeDefaultSections = new[]
    {
        "Wizard",
        "Search",
        "Documentation",
        "Tab",
    };

    private static readonly HashSet<string> HttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "get",
        "post",
        "put",
        "patch",
        "delete",
    };

    private static readonly Dictionary<string, string> OperationDescriptionsByPath = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["/Tab/CreateNode"] =
            "Create a new model item under a parent node. This is how you create database "
            + "fields, virtual fields, function-call fields, lookup fields, relationships, "
            + "parameters, filters, indexes, and even new entities. Workflow: (1) call "
            + "GetMenuItems with the parent node id to list the item types that can be created "
            + "there, each with a caption and a typeName; (2) call this with nodeId set to the "
            + "parent id and newTypeName set to the chosen typeName (a fully-qualified .NET type "
            + "name such as Origam.Schema.EntityModel.FieldMappingItem for a Database Field). The "
            + "response returns the new item's editable properties, each with validation errors "
            + "(for example 'Name cannot be empty') to fill in and save. Always take newTypeName "
            + "from GetMenuItems; never invent it.",
        ["/Model/GetMenuItems"] =
            "List the model-item types that can be created as children of the given node (the "
            + "'New' context menu). Each entry has a caption (for example 'Database Field') and a "
            + "typeName (for example Origam.Schema.EntityModel.FieldMappingItem). Pass that "
            + "typeName as the newTypeName argument of CreateNode to actually create the item.",
    };

    private readonly IHttpClientFactory httpClientFactory;
    private readonly string architectBaseUrl;
    private readonly SemaphoreSlim loadLock = new(1, 1);

    private byte[]? swaggerBytes;
    private Dictionary<string, ApiSection>? sections;

    public string BaseUrl => architectBaseUrl;

    public string? LastError { get; private set; }

    public OpenApiSectionProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    )
    {
        this.httpClientFactory = httpClientFactory;
        architectBaseUrl =
            configuration.GetSection("Architect")["BaseUrl"] ?? "https://localhost:7099";
    }

    public async Task<IReadOnlyList<SectionInfo>?> GetSectionsAsync(
        CancellationToken cancellationToken
    )
    {
        if (!await EnsureLoadedAsync(cancellationToken))
        {
            return null;
        }

        return sections!
            .Values.OrderBy(section => section.Name, StringComparer.Ordinal)
            .Select(section => new SectionInfo(
                section.Name,
                section.Operations.Count,
                section.Operations.Select(operation => operation.DisplayName).ToArray(),
                section.HasDestructive
            ))
            .ToArray();
    }

    public async Task<IReadOnlyList<AITool>?> GetToolsAsync(
        string sectionName,
        CancellationToken cancellationToken
    )
    {
        if (!await EnsureLoadedAsync(cancellationToken))
        {
            return null;
        }

        if (!sections!.TryGetValue(sectionName, out var section))
        {
            return null;
        }

        if (section.Tools is not null)
        {
            return section.Tools;
        }

        await loadLock.WaitAsync(cancellationToken);
        try
        {
            if (section.Tools is not null)
            {
                return section.Tools;
            }

            section.Tools = OpenApiFunctionBuilder.Build(
                swaggerBytes!,
                section.Name,
                section.Paths,
                architectBaseUrl,
                httpClientFactory
            );
            return section.Tools;
        }
        catch (Exception exception)
        {
            LastError = $"{exception.GetType().Name}: {exception.Message}";
            return null;
        }
        finally
        {
            loadLock.Release();
        }
    }

    private async Task<bool> EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (sections is not null)
        {
            return true;
        }

        await loadLock.WaitAsync(cancellationToken);
        try
        {
            if (sections is not null)
            {
                return true;
            }

            var httpClient = httpClientFactory.CreateClient("architect");
            var openApiUri = new Uri(
                new Uri(architectBaseUrl),
                relativeUri: "swagger/v1/swagger.json"
            );

            var downloadedBytes = await httpClient.GetByteArrayAsync(openApiUri, cancellationToken);
            swaggerBytes = PatchDescriptions(downloadedBytes);
            sections = ParseSections(swaggerBytes);
            LastError = null;
            return true;
        }
        catch (Exception exception)
        {
            LastError = $"{exception.GetType().Name}: {exception.Message}";
            return false;
        }
        finally
        {
            loadLock.Release();
        }
    }

    private static byte[] PatchDescriptions(byte[] bytes)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(bytes);
        }
        catch (JsonException)
        {
            return bytes;
        }

        if (root?["paths"] is not JsonObject paths)
        {
            return bytes;
        }

        foreach (var entry in OperationDescriptionsByPath)
        {
            if (paths[entry.Key] is not JsonObject pathItem)
            {
                continue;
            }

            foreach (var methodEntry in pathItem)
            {
                if (
                    HttpMethods.Contains(methodEntry.Key)
                    && methodEntry.Value is JsonObject operation
                )
                {
                    operation["description"] = entry.Value;
                }
            }
        }

        return JsonSerializer.SerializeToUtf8Bytes(root);
    }

    private static Dictionary<string, ApiSection> ParseSections(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        var operationsByTag = new Dictionary<string, List<SectionOperation>>(
            StringComparer.Ordinal
        );

        if (document.RootElement.TryGetProperty(propertyName: "paths", out var paths))
        {
            foreach (var pathEntry in paths.EnumerateObject())
            {
                var path = pathEntry.Name;
                foreach (var methodEntry in pathEntry.Value.EnumerateObject())
                {
                    var method = methodEntry.Name;
                    if (!HttpMethods.Contains(method))
                    {
                        continue;
                    }

                    var operation = methodEntry.Value;
                    var tag = ReadFirstTag(operation);
                    var displayName = ReadDisplayName(operation, method, path);
                    var destructive = IsDestructive(method, path, tag);

                    if (!operationsByTag.TryGetValue(tag, out var list))
                    {
                        list = new List<SectionOperation>();
                        operationsByTag[tag] = list;
                    }

                    list.Add(new SectionOperation(method, path, displayName, destructive));
                }
            }
        }

        return operationsByTag.ToDictionary(
            entry => entry.Key,
            entry => new ApiSection
            {
                Name = entry.Key,
                Operations = entry.Value,
                Paths = entry
                    .Value.Select(operation => operation.Path)
                    .ToHashSet(StringComparer.Ordinal),
            },
            StringComparer.Ordinal
        );
    }

    private static string ReadFirstTag(JsonElement operation)
    {
        if (
            operation.TryGetProperty(propertyName: "tags", out var tags)
            && tags.ValueKind == JsonValueKind.Array
            && tags.GetArrayLength() > 0
        )
        {
            var firstTag = tags[0].GetString();
            if (!string.IsNullOrWhiteSpace(firstTag))
            {
                return firstTag;
            }
        }

        return "General";
    }

    private static string ReadDisplayName(JsonElement operation, string method, string path)
    {
        if (
            operation.TryGetProperty(propertyName: "operationId", out var operationId)
            && operationId.ValueKind == JsonValueKind.String
        )
        {
            var value = operationId.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return $"{method.ToUpperInvariant()} {path}";
    }

    private static bool IsDestructive(string method, string path, string tag)
    {
        if (method.Equals(value: "delete", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var haystack = $"{path} {tag}";
        return haystack.Contains(
                value: "delete",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
            || haystack.Contains(
                value: "remove",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
            || haystack.Contains(
                value: "closeall",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
            || haystack.Contains(
                value: "deploy",
                comparisonType: StringComparison.OrdinalIgnoreCase
            );
    }
}
