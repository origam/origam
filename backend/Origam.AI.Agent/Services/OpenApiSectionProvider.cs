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
using Origam.AI.Agent.Services.OpenApi;

namespace Origam.AI.Agent.Services;

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
        "Model",
        "PropertyEditor",
        "SectionEditor",
    };

    private static readonly HashSet<string> HttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "get",
        "post",
        "put",
        "patch",
        "delete",
    };

    private static readonly HashSet<string> PathsNeverExposedAsTools = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "/Model/GetEntityIndex",
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

            swaggerBytes = await httpClient.GetByteArrayAsync(openApiUri, cancellationToken);
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
                if (PathsNeverExposedAsTools.Contains(path))
                {
                    continue;
                }

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
