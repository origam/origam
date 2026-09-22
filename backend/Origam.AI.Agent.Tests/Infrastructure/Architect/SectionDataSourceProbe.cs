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

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed class SectionDataSourceProbe(HttpClient architect)
{
    private static readonly TimeSpan IndexTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan IndexPollDelay = TimeSpan.FromSeconds(1);

    public async Task<bool> WaitForReferenceIndexAsync()
    {
        var deadline = DateTime.UtcNow + IndexTimeout;
        while (DateTime.UtcNow < deadline)
        {
            using var response = await architect.GetAsync(
                requestUri: "/Model/GetSchemaItemInfos",
                CancellationToken.None
            );
            if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
            {
                return response.IsSuccessStatusCode;
            }
            await Task.Delay(IndexPollDelay);
        }
        return false;
    }

    public async Task<IReadOnlyList<string>> ReadWarningsAsync(
        string sectionId,
        string? entityId = null
    )
    {
        using var document = await UpdateAsync(sectionId, entityId);
        return document
            .RootElement.GetProperty("data")
            .GetProperty("warnings")
            .EnumerateArray()
            .Select(warning => warning.GetString() ?? string.Empty)
            .ToList();
    }

    public async Task<string?> FindEntityIdAsync(string sectionId, string entityName)
    {
        using var document = await UpdateAsync(sectionId, entityId: null);
        return document
            .RootElement.GetProperty("data")
            .GetProperty("dataSources")
            .EnumerateArray()
            .Where(dataSource => dataSource.GetProperty("name").GetString() == entityName)
            .Select(dataSource => dataSource.GetProperty("schemaItemId").GetString())
            .FirstOrDefault();
    }

    private async Task<JsonDocument> UpdateAsync(string sectionId, string? entityId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/SectionEditor/Update",
            new
            {
                schemaItemId = sectionId,
                selectedDataSourceId = entityId,
                modelChanges = Array.Empty<object>(),
            },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        return JsonDocument.Parse(body);
    }
}
