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

public sealed class ModelItemFactory(HttpClient architect)
{
    private const string EntityTypeName = "Database Entity";
    private const string FieldTypeName = "Database Field";

    public async Task<string> FindTopLevelFolderIdAsync(string categoryText, string folderText)
    {
        var body = await architect.GetStringAsync(
            requestUri: "/Model/GetTopNodes",
            CancellationToken.None
        );
        using var document = JsonDocument.Parse(body);
        foreach (var category in document.RootElement.EnumerateArray())
        {
            if (category.GetProperty("nodeText").GetString() != categoryText)
            {
                continue;
            }

            foreach (var folder in category.GetProperty("children").EnumerateArray())
            {
                if (folder.GetProperty("nodeText").GetString() == folderText)
                {
                    return folder.GetProperty("origamId").GetString()
                        ?? throw new InvalidOperationException(
                            $"The top level folder {categoryText}/{folderText} has no id."
                        );
                }
            }
        }

        throw new InvalidOperationException(
            $"The model has no top level folder {categoryText}/{folderText}."
        );
    }

    public Task<string> CreateEntityAsync(string parentNodeId, string name)
    {
        return CreateItemAsync(parentNodeId, EntityTypeName, [("Name", name)]);
    }

    public Task<string> CreateTextFieldAsync(string entityId, string name)
    {
        return CreateItemAsync(
            entityId,
            FieldTypeName,
            [("Name", name), ("DataType", "String"), ("DataLength", "200")]
        );
    }

    private async Task<string> CreateItemAsync(
        string parentNodeId,
        string typeName,
        IReadOnlyList<(string Name, string Value)> properties
    )
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/Tab/CreateNode",
            new
            {
                nodeId = parentNodeId,
                newTypeName = typeName,
                changes = properties.Select(property => new
                {
                    name = property.Name,
                    value = property.Value,
                }),
                persist = true,
            },
            CancellationToken.None
        );
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"CreateNode of a {typeName} under {parentNodeId} answered "
                    + $"{(int)response.StatusCode}: {body}"
            );
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (
            root.TryGetProperty(propertyName: "discarded", out var discarded)
            && discarded.ValueKind == JsonValueKind.True
        )
        {
            throw new InvalidOperationException(
                $"CreateNode discarded the new {typeName} under {parentNodeId}: {body}"
            );
        }

        return root.GetProperty("node").GetProperty("origamId").GetString()
            ?? throw new InvalidOperationException(
                $"CreateNode of a {typeName} returned no id: {body}"
            );
    }
}
