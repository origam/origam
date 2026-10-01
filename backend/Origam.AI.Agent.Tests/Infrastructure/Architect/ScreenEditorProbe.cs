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

using System.Net.Http.Json;
using System.Text.Json;

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed class ScreenEditorProbe(HttpClient architect)
{
    public async Task<ScreenEditorContent> ReadPersistedAsync(string screenId)
    {
        using var closeResponse = await architect.PostAsync(
            requestUri: "/Tab/CloseAll",
            content: null,
            CancellationToken.None
        );
        closeResponse.EnsureSuccessStatusCode();
        return await ReadAsync(screenId);
    }

    public async Task<ScreenEditorContent> ReadAsync(string screenId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/ScreenEditor/Update",
            new { schemaItemId = screenId, modelChanges = Array.Empty<object>() },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.GetProperty("data");
        return new ScreenEditorContent(
            DesignerWidget.FromJson(data.GetProperty("rootControl")),
            ReadStrings(data, propertyName: "warnings"),
            ReadStrings(data, propertyName: "dataMembers")
        );
    }

    private static IReadOnlyList<string> ReadStrings(JsonElement data, string propertyName)
    {
        if (
            !data.TryGetProperty(propertyName, out var values)
            || values.ValueKind != JsonValueKind.Array
        )
        {
            return [];
        }

        return values.EnumerateArray().Select(value => value.GetString() ?? string.Empty).ToList();
    }
}
