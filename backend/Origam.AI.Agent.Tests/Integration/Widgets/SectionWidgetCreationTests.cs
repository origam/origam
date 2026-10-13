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
using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure.Architect;

namespace Origam.AI.Agent.Tests.Integration.Widgets;

[TestFixture]
[Category(AgentIntegrationTestBase.IntegrationCategory)]
public sealed class SectionWidgetCreationTests : AgentIntegrationTestBase
{
    private const string SectionName = "WidgetSectionTestMaster";
    private const string SectionTypeName = "Screen Section";
    private const string PackageName = "Widgets";
    private const string EnumProperty = "CaptionPosition";
    private const string AddedWidgetType = "Origam.Gui.Win.AsTextBox";
    private const string AddedWidgetField = "Text2";

    private string sectionId = string.Empty;
    private string? packageToRestore;

    [OneTimeSetUp]
    public async Task FindTheSectionThatAlreadyHasWidgets()
    {
        if (
            InProcessArchitect.ActivePackageName is { } activePackage
            && activePackage != PackageName
        )
        {
            if (!await InProcessArchitect.ActivatePackageAsync(PackageName))
            {
                Assert.Ignore($"The package '{PackageName}' is not in the model.");
            }
            packageToRestore = activePackage;
        }

        sectionId =
            await new ArchitectModelBuilder(AgentClient.Architect).FindItemIdAsync(
                SectionName,
                SectionTypeName
            ) ?? string.Empty;
        if (sectionId == string.Empty)
        {
            Assert.Ignore($"The screen section '{SectionName}' is not in the loaded model.");
        }
    }

    [OneTimeTearDown]
    public async Task PutThePackageBack()
    {
        await Model.CloseAllTabsAsync();
        if (packageToRestore is not null)
        {
            await InProcessArchitect.ActivatePackageAsync(packageToRestore);
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task AddingAWidget_KeepsTheValuesOfTheWidgetsAlreadyThere()
    {
        var section = await ReadSectionAsync();
        var before = ReadEnumValues(section);
        if (before.Count == 0)
        {
            Assert.Ignore(
                $"No widget of '{SectionName}' carries a '{EnumProperty}' value, so nothing "
                    + "here could be lost."
            );
        }

        await AddTextBoxAsync(section.GetProperty("id").GetString() ?? string.Empty);

        Assert.That(
            ReadEnumValues(await ReadSectionAsync()),
            Is.SupersetOf(before),
            $"Adding a widget changed the '{EnumProperty}' of the widgets that were already in "
                + $"'{SectionName}'. Their values are dropped from the model file on the next "
                + "save, although nobody edited them."
        );
    }

    private async Task<JsonElement> ReadSectionAsync()
    {
        using var response = await AgentClient.Architect.PostAsJsonAsync(
            requestUri: "/SectionEditor/Update",
            new { schemaItemId = sectionId, modelChanges = Array.Empty<object>() },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("data").GetProperty("rootControl").Clone();
    }

    private async Task AddTextBoxAsync(string rootPanelId)
    {
        using var response = await AgentClient.Architect.PostAsJsonAsync(
            requestUri: "/SectionEditor/CreateItem",
            new
            {
                editorSchemaItemId = sectionId,
                parentControlSetItemId = rootPanelId,
                componentType = AddedWidgetType,
                fieldName = AddedWidgetField,
                top = 200,
                left = 10,
            },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
    }

    private static Dictionary<string, string> ReadEnumValues(JsonElement control)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        Collect(control, values);
        return values;
    }

    private static void Collect(JsonElement control, Dictionary<string, string> values)
    {
        var id = control.GetProperty("id").GetString() ?? string.Empty;
        foreach (var property in control.GetProperty("properties").EnumerateArray())
        {
            if (
                property.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() == EnumProperty
                && property.TryGetProperty(propertyName: "value", out var value)
                && value.ValueKind != JsonValueKind.Null
            )
            {
                values[id] = value.ToString();
            }
        }

        if (control.TryGetProperty(propertyName: "children", out var children))
        {
            foreach (var child in children.EnumerateArray())
            {
                Collect(child, values);
            }
        }
    }
}
