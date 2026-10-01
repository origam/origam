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

public sealed class SectionEditorProbe(HttpClient architect)
{
    public async Task<SectionWidget> ReadPersistedAsync(string sectionId)
    {
        using var closeResponse = await architect.PostAsync(
            requestUri: "/Tab/CloseAll",
            content: null,
            CancellationToken.None
        );
        closeResponse.EnsureSuccessStatusCode();
        return await ReadAsync(sectionId);
    }

    public async Task<SectionWidget> ReadAsync(string sectionId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/SectionEditor/Update",
            new { schemaItemId = sectionId, modelChanges = Array.Empty<object>() },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        return ReadWidget(document.RootElement.GetProperty("data").GetProperty("rootControl"));
    }

    private static SectionWidget ReadWidget(JsonElement control)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in control.GetProperty("properties").EnumerateArray())
        {
            if (
                property.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() is { } propertyName
                && property.TryGetProperty(propertyName: "value", out var value)
            )
            {
                properties[propertyName] =
                    value.ValueKind == JsonValueKind.Null ? string.Empty : value.ToString();
            }
        }

        var children = new List<SectionWidget>();
        if (
            control.TryGetProperty(propertyName: "children", out var childElements)
            && childElements.ValueKind == JsonValueKind.Array
        )
        {
            children.AddRange(childElements.EnumerateArray().Select(ReadWidget));
        }

        return new SectionWidget(
            control.GetProperty("type").GetString() ?? string.Empty,
            control.TryGetProperty(propertyName: "boundField", out var boundField)
                ? boundField.GetString()
                : null,
            properties,
            children
        );
    }
}
