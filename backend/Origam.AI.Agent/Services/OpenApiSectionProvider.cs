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

public record SectionOperation(string Method, string Path, string Name, string Description);

public class ApiSection
{
    public required string Name { get; init; }
    public required IReadOnlyList<SectionOperation> Operations { get; init; }
    public required HashSet<string> Paths { get; init; }
    public IReadOnlyList<AITool>? Tools { get; set; }
}

public record SectionFunctionInfo(string Name, string? Method, string? Path, string Description);

public record SectionInfo(
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    int FunctionCount,
    IReadOnlyList<SectionFunctionInfo> Functions,
    bool EnabledByDefault
);

public class OpenApiSectionProvider
{
    public static readonly IReadOnlyList<string> SafeDefaultSections = new[]
    {
        "Wizard",
        "Search",
        "Tab",
        "Model",
        "PropertyEditor",
        "CommunityWebSearch",
        "ItemTypeCatalog",
    };

    public const string BetaTag = "beta";
    public const string UnstableTag = "very unstable";

    public const string AgentApiSectionName = "Origam.AI.Agent";

    private static readonly HashSet<string> SectionsOutOfBeta = new(StringComparer.Ordinal)
    {
        "Model",
        "Wizard",
    };

    private static readonly Dictionary<string, IReadOnlyList<string>> AdditionalSectionTags = new(
        StringComparer.Ordinal
    )
    {
        ["DeploymentScripts"] = new[] { UnstableTag },
        ["DeploymentScriptsGenerator"] = new[] { UnstableTag },
    };

    private static readonly Dictionary<string, string> SectionDescriptions = new(
        StringComparer.Ordinal
    )
    {
        ["DeploymentScripts"] =
            "Makes a deployment version current and runs its deployment scripts against the database.",
        ["DeploymentScriptsGenerator"] =
            "Compares the model with the database and adds the differences to a deployment version or back into the model.",
        ["Documentation"] = "Opens and edits the documentation attached to a model element.",
        ["ItemTypeCatalog"] =
            "Lists the item types that can be created under a node and the properties each of them has.",
        ["Model"] =
            "Browses the model tree, reads node details, searches the schema and deletes model elements.",
        ["Package"] = "Lists the packages of the model and switches the active one.",
        ["PropertyEditor"] = "Writes property values on the element.",
        ["ScreenEditor"] =
            "Edits a screen opened in the designer: creates, updates and deletes the items on it.",
        ["Search"] =
            "Finds model elements by text and shows what references them and what they depend on.",
        ["SectionEditor"] =
            "Edits a screen section opened in the designer: creates, updates and deletes the items on it.",
        ["Tab"] = "Opens, closes and saves editor tabs, and creates new model nodes inside them.",
        ["Wizard"] =
            "Creates screens, lookups, menu items, work queue classes and filters through the Architect wizards.",
        ["Xslt"] =
            "Validates and runs XSLT transformations and reads their parameters, settings and rule sets.",
    };

    private static readonly HashSet<string> HttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "get",
        "post",
        "put",
        "patch",
        "delete",
    };

    private static readonly HashSet<string> SectionsNeverExposedAsTools = new(
        StringComparer.Ordinal
    )
    {
        "ChatHistory",
        AgentApiSectionName,
        "Test",
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

    public static IReadOnlyList<string> TagsFor(string sectionName)
    {
        var tags = new List<string>();
        if (!SectionsOutOfBeta.Contains(sectionName))
        {
            tags.Add(BetaTag);
        }

        tags.AddRange(AdditionalSectionTags.GetValueOrDefault(sectionName, Array.Empty<string>()));
        return tags;
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
                SectionDescriptions.GetValueOrDefault(section.Name, string.Empty),
                TagsFor(section.Name),
                section.Operations.Count,
                section
                    .Operations.Select(operation => new SectionFunctionInfo(
                        operation.Name,
                        operation.Method.ToUpperInvariant(),
                        operation.Path,
                        operation.Description
                    ))
                    .ToArray(),
                SafeDefaultSections.Contains(section.Name)
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
                    if (SectionsNeverExposedAsTools.Contains(tag))
                    {
                        continue;
                    }

                    var name = OpenApiFunctionBuilder.BuildToolName(tag, method, path);
                    var description = ReadDescription(operation);

                    if (!operationsByTag.TryGetValue(tag, out var list))
                    {
                        list = new List<SectionOperation>();
                        operationsByTag[tag] = list;
                    }

                    list.Add(new SectionOperation(method, path, name, description));
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

    private static string ReadDescription(JsonElement operation)
    {
        foreach (var propertyName in new[] { "description", "summary" })
        {
            if (
                operation.TryGetProperty(propertyName, out var value)
                && value.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(value.GetString())
            )
            {
                return value.GetString()!;
            }
        }

        return string.Empty;
    }
}
