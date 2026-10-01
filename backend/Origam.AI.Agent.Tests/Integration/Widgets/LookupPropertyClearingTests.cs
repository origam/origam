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
public sealed class LookupPropertyClearingTests : AgentIntegrationTestBase
{
    private const string LookupProperty = "StyleId";
    private static readonly string ClearedValue = Guid.Empty.ToString();

    private ScreenTestModel screens = null!;
    private string screenId = string.Empty;
    private string widgetId = string.Empty;
    private string? packageToRestore;

    [OneTimeSetUp]
    public async Task ActivateThePackageThatHoldsTheWidgets()
    {
        screens = new ScreenTestModel(AgentClient.Architect);
        if (
            InProcessArchitect.ActivePackageName is { } activePackage
            && activePackage != ScreenTestModel.PackageName
        )
        {
            if (!await InProcessArchitect.ActivatePackageAsync(ScreenTestModel.PackageName))
            {
                Assert.Ignore(
                    $"The package '{ScreenTestModel.PackageName}' is not in the model, so the "
                        + "widget whose lookup this test clears has nowhere to live."
                );
            }
            packageToRestore = activePackage;
        }

        if (await screens.FindDataStructureIdAsync() is null)
        {
            Assert.Ignore(
                $"The data structure '{ScreenTestModel.DataStructureName}' is not in the loaded "
                    + "model, so no screen can be built here."
            );
        }
    }

    [OneTimeTearDown]
    public async Task PutTheOriginalPackageBack()
    {
        await Model.CloseAllTabsAsync();
        if (packageToRestore is not null)
        {
            await InProcessArchitect.ActivatePackageAsync(packageToRestore);
        }
    }

    [SetUp]
    public async Task PutAWidgetOnAScreen()
    {
        var screen = await screens.CreateEmptyScreenAsync(
            "LookupClear" + Guid.NewGuid().ToString("N")[..8]
        );
        CreatedEntityNames.Add(screen.Name);
        screenId = screen.Id;
        widgetId = await screens.AddWidgetAsync(screenId, ScreenTestModel.MasterSectionName);
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task TheEmptyEntryOfALookup_TakesTheValueBackOff()
    {
        var offeredValue = await FindOfferedValueAsync();
        if (offeredValue is null)
        {
            Assert.Ignore(
                $"The widget offers no value for '{LookupProperty}', so there is nothing to "
                    + "set and take back off here."
            );
        }

        await screens.SetPropertyAsync(screenId, widgetId, LookupProperty, offeredValue);
        Assert.That(
            await ReadLookupAsync(),
            Is.EqualTo(offeredValue),
            $"The editor did not take the '{LookupProperty}' value at all."
        );

        await screens.SetPropertyAsync(screenId, widgetId, LookupProperty, value: null);
        Assert.That(
            await ReadLookupAsync(),
            Is.EqualTo(ClearedValue).Or.Empty,
            $"Picking the empty entry of the '{LookupProperty}' lookup left the old value in "
                + "place, so a value chosen by mistake can never be taken back off."
        );
    }

    private async Task<string> ReadLookupAsync()
    {
        var property = await FindLookupAsync();
        return
            property.TryGetProperty(propertyName: "value", out var value)
            && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : string.Empty;
    }

    private async Task<string?> FindOfferedValueAsync()
    {
        var property = await FindLookupAsync();
        if (!property.TryGetProperty(propertyName: "dropDownValues", out var offered))
        {
            return null;
        }

        return offered
            .EnumerateArray()
            .Select(option =>
                option.TryGetProperty(propertyName: "value", out var value)
                && value.ValueKind == JsonValueKind.String
                    ? value.GetString()
                    : null
            )
            .FirstOrDefault(value => !string.IsNullOrEmpty(value) && value != ClearedValue);
    }

    private async Task<JsonElement> FindLookupAsync()
    {
        using var response = await AgentClient.Architect.PostAsJsonAsync(
            requestUri: "/ScreenEditor/Update",
            new { schemaItemId = screenId, modelChanges = Array.Empty<object>() },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        var widget = FindWidget(
            document.RootElement.GetProperty("data").GetProperty("rootControl")
        );
        Assert.That(
            widget,
            Is.Not.Null,
            message: "The widget this test put on the screen is no longer in the editor."
        );

        var property = widget!
            .Value.GetProperty("properties")
            .EnumerateArray()
            .Where(candidate =>
                candidate.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() == LookupProperty
            )
            .Select(candidate => candidate.Clone())
            .FirstOrDefault();
        Assert.That(
            property.ValueKind,
            Is.EqualTo(JsonValueKind.Object),
            $"The widget has no '{LookupProperty}' property to clear."
        );
        return property;
    }

    private JsonElement? FindWidget(JsonElement control)
    {
        if (control.TryGetProperty(propertyName: "id", out var id) && id.GetString() == widgetId)
        {
            return control.Clone();
        }

        if (!control.TryGetProperty(propertyName: "children", out var children))
        {
            return null;
        }

        return children.EnumerateArray().Select(FindWidget).FirstOrDefault(found => found != null);
    }
}
