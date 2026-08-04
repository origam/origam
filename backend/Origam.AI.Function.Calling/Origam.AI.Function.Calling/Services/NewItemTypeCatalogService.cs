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

namespace Origam.AI.Function.Calling.Services;

public class NewItemTypeCatalogService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string architectBaseUrl;
    private readonly SemaphoreSlim buildLock = new(initialCount: 1, maxCount: 1);

    private string? cachedSection;
    private string? lastError;

    public NewItemTypeCatalogService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    )
    {
        this.httpClientFactory = httpClientFactory;
        architectBaseUrl =
            configuration.GetSection("Architect")["BaseUrl"] ?? "https://localhost:7099";
    }

    public string? LastError => lastError;

    public async Task<string> GetPromptSectionAsync(CancellationToken cancellationToken)
    {
        if (cachedSection is not null)
        {
            return cachedSection;
        }

        await buildLock.WaitAsync(cancellationToken);
        try
        {
            if (cachedSection is not null)
            {
                return cachedSection;
            }

            cachedSection = await BuildSectionAsync(cancellationToken);
            return cachedSection;
        }
        finally
        {
            buildLock.Release();
        }
    }

    private async Task<string> BuildSectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("architect");
            var lines = new List<string>();

            foreach (var provider in await GetProvidersAsync(httpClient, cancellationToken))
            {
                string captions = await GetCaptionsAsync(
                    httpClient,
                    provider.Id,
                    cancellationToken
                );
                if (captions.Length > 0)
                {
                    lines.Add($"Under {provider.Name}: {captions}");
                }
            }

            string? entityId = await GetSampleDatabaseEntityIdAsync(httpClient, cancellationToken);
            if (entityId is not null)
            {
                string captions = await GetCaptionsAsync(httpClient, entityId, cancellationToken);
                if (captions.Length > 0)
                {
                    lines.Add($"Inside a Database Entity: {captions}");
                }
            }

            if (lines.Count == 0)
            {
                return string.Empty;
            }

            lastError = null;
            var builder = new StringBuilder();
            builder.AppendLine("## ITEM TYPES YOU CAN CREATE");
            builder.AppendLine(
                "Pass one of the captions below to CreateNode as newTypeName; the server resolves "
                    + "it to the real type name, so you do NOT have to call GetMenuItems first. "
                    + "Call GetMenuItems only when the parent you are creating under is not covered "
                    + "by this list, or when CreateNode tells you the name did not match."
            );
            foreach (string line in lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }
        catch (Exception exception)
        {
            lastError = $"{exception.GetType().Name}: {exception.Message}";
            return string.Empty;
        }
    }

    private async Task<IReadOnlyList<ProviderNode>> GetProvidersAsync(
        HttpClient httpClient,
        CancellationToken cancellationToken
    )
    {
        var response = await httpClient.GetAsync(
            $"{architectBaseUrl}/Model/GetTopNodes",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            lastError = $"Architect returned {(int)response.StatusCode} for GetTopNodes.";
            return Array.Empty<ProviderNode>();
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ProviderNode>();
        }

        var providers = new List<ProviderNode>();
        foreach (JsonElement category in document.RootElement.EnumerateArray())
        {
            if (
                !category.TryGetProperty(propertyName: "children", out JsonElement children)
                || children.ValueKind != JsonValueKind.Array
            )
            {
                continue;
            }

            foreach (JsonElement child in children.EnumerateArray())
            {
                string? id = GetString(child, propertyName: "origamId");
                string? name = GetString(child, propertyName: "nodeText");
                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                {
                    providers.Add(new ProviderNode(id, name));
                }
            }
        }

        return providers;
    }

    private async Task<string?> GetSampleDatabaseEntityIdAsync(
        HttpClient httpClient,
        CancellationToken cancellationToken
    )
    {
        var response = await httpClient.GetAsync(
            $"{architectBaseUrl}/Model/GetEntityIndex",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (JsonElement card in document.RootElement.EnumerateArray())
        {
            string? kind = GetString(card, propertyName: "kind");
            string? id = GetString(card, propertyName: "id");
            if (
                !string.IsNullOrWhiteSpace(id)
                && kind is not null
                && kind.StartsWith(
                    value: "Database",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return id;
            }
        }

        return null;
    }

    private async Task<string> GetCaptionsAsync(
        HttpClient httpClient,
        string nodeId,
        CancellationToken cancellationToken
    )
    {
        var response = await httpClient.GetAsync(
            $"{architectBaseUrl}/Model/GetMenuItems?id={Uri.EscapeDataString(nodeId)}",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return string.Empty;
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var captions = new List<string>();
        foreach (JsonElement item in document.RootElement.EnumerateArray())
        {
            string? caption = GetString(item, propertyName: "caption");
            if (!string.IsNullOrWhiteSpace(caption))
            {
                captions.Add(caption);
            }
        }

        return string.Join(separator: ", ", captions);
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return
            element.TryGetProperty(propertyName, out JsonElement property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private record ProviderNode(string Id, string Name);
}
