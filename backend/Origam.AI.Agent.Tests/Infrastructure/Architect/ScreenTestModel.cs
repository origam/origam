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

public sealed class ScreenTestModel(HttpClient architect)
{
    public const string PackageName = "Widgets";
    public const string DataStructureName = "WidgetSectionTest";
    public const string DataStructureTypeName = "Data Structure";
    public const string MasterSectionName = "WidgetSectionTestMaster";
    public const string DetailSectionName = "WidgetSectionTestDetail";
    public const string MasterDataMember = "WidgetSectionTestMaster";
    public const string DetailDataMember = "WidgetSectionTestMaster.WidgetSectionTestDetail";
    public const string PluginName = "TestPlugin";
    public const string TreeIdColumn = "Id";
    public const string TreeParentIdColumn = "refWidgetSectionTestMasterId";
    public const string TreeNameColumn = "Text1";
    public const int ScreenWidth = 500;
    public const int ScreenHeight = 500;

    private const string ScreenProviderNodeId = "Origam.Schema.GuiModel.FormSchemaItemProvider";
    private const string ScreenTypeName = "Screen";

    private string dataStructureId = string.Empty;

    public async Task<string?> FindDataStructureIdAsync()
    {
        dataStructureId =
            await new ArchitectModelBuilder(architect).FindItemIdAsync(
                DataStructureName,
                DataStructureTypeName
            ) ?? string.Empty;
        return dataStructureId == string.Empty ? null : dataStructureId;
    }

    public async Task<TestScreen> CreateEmptyScreenAsync(string screenName)
    {
        using var createResponse = await architect.PostAsJsonAsync(
            requestUri: "/Tab/CreateNode",
            new
            {
                nodeId = ScreenProviderNodeId,
                newTypeName = ScreenTypeName,
                changes = new[] { new { name = "Name", value = screenName } },
                persist = false,
            },
            CancellationToken.None
        );
        createResponse.EnsureSuccessStatusCode();

        var createBody = await createResponse.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(createBody);
        var screenId =
            document.RootElement.GetProperty("node").GetProperty("origamId").GetString()
            ?? string.Empty;

        using var dataStructureResponse = await architect.PostAsJsonAsync(
            requestUri: "/ScreenEditor/Update",
            new
            {
                schemaItemId = screenId,
                selectedDataSourceId = dataStructureId,
                modelChanges = Array.Empty<object>(),
            },
            CancellationToken.None
        );
        dataStructureResponse.EnsureSuccessStatusCode();
        return new TestScreen(screenName, screenId);
    }

    public async Task<string> AddWidgetAsync(string screenId, string controlName)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/ScreenEditor/CreateItem",
            new { editorSchemaItemId = screenId, controlName },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("screenItem").GetProperty("id").GetString()
            ?? string.Empty;
    }

    public async Task SetPropertyAsync(
        string screenId,
        string widgetId,
        string propertyName,
        string? value
    )
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/ScreenEditor/Update",
            new
            {
                schemaItemId = screenId,
                modelChanges = new[]
                {
                    new
                    {
                        schemaItemId = widgetId,
                        changes = new[] { new { name = propertyName, value } },
                    },
                },
            },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
    }
}
