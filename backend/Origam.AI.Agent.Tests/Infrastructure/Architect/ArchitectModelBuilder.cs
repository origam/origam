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

public sealed class ArchitectModelBuilder(HttpClient architect)
{
    private const string AncestorsNodeText = "_Ancestors";

    public async Task<string?> ReadSectionEntityIdAsync(string sectionId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/SectionEditor/Update",
            new { schemaItemId = sectionId, modelChanges = Array.Empty<object>() },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        return document
            .RootElement.GetProperty("data")
            .GetProperty("selectedDataSourceId")
            .GetString();
    }

    public async Task<string?> FindItemIdAsync(string exactName, string itemTypeName)
    {
        var body = await architect.GetStringAsync(
            requestUri: "/Search/SearchSchemaByName?query=" + Uri.EscapeDataString(exactName),
            CancellationToken.None
        );
        using var document = JsonDocument.Parse(body);
        return document
            .RootElement.EnumerateArray()
            .Where(node =>
                node.GetProperty("nodeText").GetString() == exactName
                && node.TryGetProperty(propertyName: "itemTypeName", out var type)
                && type.GetString() == itemTypeName
            )
            .Select(node => node.GetProperty("origamId").GetString())
            .FirstOrDefault();
    }

    public async Task<IReadOnlyDictionary<string, EntityField>> ReadFieldsAsync(string entityId)
    {
        var body = await architect.GetStringAsync(
            requestUri: $"/Model/GetSchemaNodeDetails?id={Uri.EscapeDataString(entityId)}&depth=3",
            CancellationToken.None
        );
        using var document = JsonDocument.Parse(body);
        var fields = new Dictionary<string, EntityField>(StringComparer.Ordinal);
        CollectFields(document.RootElement, fields);
        return fields;
    }

    private static void CollectFields(JsonElement node, Dictionary<string, EntityField> fields)
    {
        if (node.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var nodeText = node.TryGetProperty(propertyName: "nodeText", out var text)
            ? text.GetString()
            : null;
        if (nodeText == AncestorsNodeText)
        {
            return;
        }

        if (
            nodeText is not null
            && node.TryGetProperty(propertyName: "itemTypeName", out var itemTypeName)
            && itemTypeName.GetString() is { } typeName
            && typeName.EndsWith(value: "Field", StringComparison.Ordinal)
            && node.TryGetProperty(propertyName: "origamId", out var origamId)
            && origamId.GetString() is { } fieldId
        )
        {
            fields[nodeText] = new EntityField(fieldId, nodeText, typeName);
        }

        if (
            node.TryGetProperty(propertyName: "children", out var children)
            && children.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var child in children.EnumerateArray())
            {
                CollectFields(child, fields);
            }
        }
    }
}
