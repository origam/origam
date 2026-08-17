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

namespace Origam.AI.Agent.Strategy.Architect.Api;

public sealed class ArchitectApiClient(IHttpClientFactory httpClientFactory, string baseUrl)
{
    public const string HttpClientName = "architect";

    private const string JsonMediaType = "application/json";

    public string BaseUrl { get; } = baseUrl.TrimEnd('/');

    public Task<ArchitectResponse> GetSchemaNodeDetailsAsync(
        string nodeId,
        int depth,
        CancellationToken cancellationToken
    )
    {
        return GetAsync(
            $"/Model/GetSchemaNodeDetails?id={Uri.EscapeDataString(nodeId)}&depth={depth}",
            cancellationToken
        );
    }

    public Task<ArchitectResponse> SearchSchemaAsync(
        string query,
        CancellationToken cancellationToken
    )
    {
        return GetAsync(
            $"/Model/SearchSchema?query={Uri.EscapeDataString(query)}",
            cancellationToken
        );
    }

    public Task<ArchitectResponse> GetEntityIndexAsync(CancellationToken cancellationToken)
    {
        return GetAsync(relativeUrl: "/Model/GetEntityIndex", cancellationToken);
    }

    public Task<ArchitectResponse> GetMenuItemsAsync(
        string nodeId,
        CancellationToken cancellationToken
    )
    {
        return GetAsync(
            $"/Model/GetMenuItems?id={Uri.EscapeDataString(nodeId)}",
            cancellationToken
        );
    }

    public Task<ArchitectResponse> GetItemTypeCatalogAsync(CancellationToken cancellationToken)
    {
        return GetAsync(relativeUrl: "/ItemTypeCatalog/Get", cancellationToken);
    }

    public Task<byte[]> GetOpenApiDocumentAsync(CancellationToken cancellationToken)
    {
        return CreateHttpClient()
            .GetByteArrayAsync(
                BuildUrl(relativeUrl: "/swagger/v1/swagger.json"),
                cancellationToken
            );
    }

    public async Task<ArchitectResponse> SendAsync(
        HttpMethod method,
        string relativeUrl,
        string? jsonPayload,
        CancellationToken cancellationToken
    )
    {
        var requestUrl = BuildUrl(relativeUrl);
        using var request = new HttpRequestMessage(method, requestUrl);
        if (!string.IsNullOrEmpty(jsonPayload))
        {
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, JsonMediaType);
        }

        using var response = await CreateHttpClient().SendAsync(request, cancellationToken);
        return await ReadResponseAsync(response, requestUrl, cancellationToken);
    }

    private async Task<ArchitectResponse> GetAsync(
        string relativeUrl,
        CancellationToken cancellationToken
    )
    {
        var requestUrl = BuildUrl(relativeUrl);
        using var response = await CreateHttpClient().GetAsync(requestUrl, cancellationToken);
        return await ReadResponseAsync(response, requestUrl, cancellationToken);
    }

    private static async Task<ArchitectResponse> ReadResponseAsync(
        HttpResponseMessage response,
        string requestUrl,
        CancellationToken cancellationToken
    )
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ArchitectResponse(response.StatusCode, body, requestUrl);
    }

    private string BuildUrl(string relativeUrl)
    {
        return BaseUrl + "/" + relativeUrl.TrimStart('/');
    }

    private HttpClient CreateHttpClient()
    {
        return httpClientFactory.CreateClient(HttpClientName);
    }
}
