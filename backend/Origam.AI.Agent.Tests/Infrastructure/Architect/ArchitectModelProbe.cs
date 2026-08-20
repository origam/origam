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

public sealed class ArchitectModelProbe(HttpClient architect)
{
    private const string AncestorsNodeText = "_Ancestors";
    private const string DatabaseFieldTypeName = "Database Field";

    public async Task<IReadOnlyList<string>> FindSchemaItemsAsync(
        string exactName,
        string? itemTypeName = null
    )
    {
        var body = await architect.GetStringAsync(
            requestUri: "/Search/SearchSchema?query=" + Uri.EscapeDataString(exactName),
            CancellationToken.None
        );

        using var document = JsonDocument.Parse(body);
        return document
            .RootElement.EnumerateArray()
            .Where(node =>
                node.TryGetProperty(propertyName: "nodeText", out var text)
                && text.GetString() == exactName
                && (
                    itemTypeName is null
                    || (
                        node.TryGetProperty(propertyName: "itemTypeName", out var type)
                        && type.GetString() == itemTypeName
                    )
                )
            )
            .Select(node => node.GetProperty("origamId").GetString())
            .OfType<string>()
            .ToList();
    }

    public async Task<IReadOnlyList<DatabaseField>> ReadDatabaseFieldsAsync(string entityId)
    {
        var body = await architect.GetStringAsync(
            requestUri: $"/Model/GetSchemaNodeDetails?id={Uri.EscapeDataString(entityId)}&depth=3",
            CancellationToken.None
        );

        using var document = JsonDocument.Parse(body);
        var fields = new List<DatabaseField>();
        CollectDatabaseFields(document.RootElement, fields);
        return fields;
    }

    public async Task<bool> PointsAtEntityAsync(string fieldId, string targetEntityId)
    {
        try
        {
            using var response = await architect.PostAsJsonAsync(
                requestUri: "/Tab/Open",
                new { schemaItemId = fieldId },
                CancellationToken.None
            );
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
            using var document = JsonDocument.Parse(body);
            if (
                !document.RootElement.TryGetProperty(propertyName: "data", out var properties)
                || properties.ValueKind != JsonValueKind.Array
            )
            {
                return false;
            }

            return ReadPropertyValue(properties, propertyName: "ForeignKeyEntity")
                    .Contains(targetEntityId, StringComparison.OrdinalIgnoreCase)
                && ReadPropertyValue(properties, propertyName: "ForeignKeyField").Length > 0;
        }
        finally
        {
            await CloseAllTabsAsync();
        }
    }

    public async Task CloseAllTabsAsync()
    {
        using var response = await architect.PostAsync(
            requestUri: "/Tab/CloseAll",
            content: null,
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyDictionary<string, string>> ReadPersistedPropertiesAsync(
        string itemId
    )
    {
        await CloseAllTabsAsync();
        return await ReadItemPropertiesAsync(itemId);
    }

    public async Task<IReadOnlyDictionary<string, string>> ReadItemPropertiesAsync(string itemId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/Tab/Open",
            new { schemaItemId = itemId },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (
            !document.RootElement.TryGetProperty(propertyName: "data", out var properties)
            || properties.ValueKind != JsonValueKind.Array
        )
        {
            return values;
        }

        foreach (var property in properties.EnumerateArray())
        {
            if (
                property.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() is { } propertyName
                && property.TryGetProperty(propertyName: "value", out var value)
            )
            {
                values[propertyName] =
                    value.ValueKind == JsonValueKind.Null ? string.Empty : value.ToString();
            }
        }

        return values;
    }

    public async Task<int> DeleteSchemaItemAsync(string itemId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/Model/DeleteSchemaItem",
            new { schemaItemId = itemId },
            CancellationToken.None
        );
        return (int)response.StatusCode;
    }

    private static void CollectDatabaseFields(JsonElement node, List<DatabaseField> fields)
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
            node.TryGetProperty(propertyName: "itemTypeName", out var itemTypeName)
            && itemTypeName.GetString() == DatabaseFieldTypeName
            && node.TryGetProperty(propertyName: "origamId", out var origamId)
            && origamId.GetString() is { } fieldId
        )
        {
            fields.Add(new DatabaseField(fieldId, nodeText ?? string.Empty));
        }

        if (
            node.TryGetProperty(propertyName: "children", out var children)
            && children.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var child in children.EnumerateArray())
            {
                CollectDatabaseFields(child, fields);
            }
        }
    }

    private static string ReadPropertyValue(JsonElement properties, string propertyName)
    {
        foreach (var property in properties.EnumerateArray())
        {
            if (
                property.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() == propertyName
                && property.TryGetProperty(propertyName: "value", out var value)
            )
            {
                return value.ValueKind == JsonValueKind.Null ? string.Empty : value.ToString();
            }
        }

        return string.Empty;
    }
}
