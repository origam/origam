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
using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure.Architect;

namespace Origam.AI.Agent.Tests.Integration.Widgets;

[TestFixture]
[Category(AgentIntegrationTestBase.IntegrationCategory)]
public sealed class ScreenSectionDataMemberTests : AgentIntegrationTestBase
{
    private const string DataMemberProperty = "DataMember";
    private const string MasterEntityName = "WidgetSectionTestMaster";
    private const string ForeignSectionName = "ArrayTest";
    private const string AllDataTypesName = "AllDataTypes";
    private const string AllDataTypesDataStructureId = "31791c3b-7265-439e-ac96-ddd57aa82579";
    private const string ArraySectionName = "AllDataTypes_WithArray";
    private const string ArrayFieldName = "ArrayTestId";
    private const string ChatBusinessPartnerDataStructureId =
        "d7921e0c-b763-4d07-a019-7e948b4c49a6";
    private const string UserProfileNewDataStructureId = "ee5cb6a9-8724-41ae-b599-f89913868f34";
    private const string BusinessPartnerName = "BusinessPartner";
    private const string LookupEntityName = "BusinessPartnerLookup";
    private const string UserProfileNewName = "UserProfile_New";
    private const string NewUserSectionName = "BusinessPartner_NewUser";
    private const string MembershipUserSectionName = "BusinessPartner_CreateMembershipUser";

    private ScreenTestModel screens = null!;
    private string screenId = string.Empty;
    private string? packageToRestore;

    [OneTimeSetUp]
    public async Task ActivateThePackageThatHoldsTheSections()
    {
        screens = new ScreenTestModel(AgentClient.Architect);
        if (
            InProcessArchitect.ActivePackageName is { } activePackage
            && activePackage != ScreenTestModel.PackageName
        )
        {
            if (!await InProcessArchitect.ActivatePackageAsync(ScreenTestModel.PackageName))
            {
                Assert.Ignore($"The package '{ScreenTestModel.PackageName}' is not in the model.");
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
    public async Task CreateAnEmptyScreen()
    {
        var screen = await screens.CreateEmptyScreenAsync(
            "DataMemberCheck" + Guid.NewGuid().ToString("N")[..8]
        );
        CreatedEntityNames.Add(screen.Name);
        screenId = screen.Id;
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task ASectionShowingItsOwnEntity_IsNotWarnedAbout()
    {
        Assert.That(
            await PlaceSectionAsync(
                ScreenTestModel.MasterSectionName,
                ScreenTestModel.MasterDataMember
            ),
            Is.Empty,
            message: "The section shows exactly the entity it was built on."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task ASectionPointedAtAnotherEntityOfTheDataStructure_IsWarnedAbout()
    {
        var warnings = await PlaceSectionAsync(
            ScreenTestModel.MasterSectionName,
            ScreenTestModel.DetailDataMember
        );
        TestContext.WriteLine("warnings: " + string.Join(Environment.NewLine, warnings));

        Assert.That(
            string.Join(separator: " | ", warnings),
            Does.Contain(MasterEntityName).And.Contain(ScreenTestModel.DetailDataMember),
            "The client looks every field of the section up in the entity of the DataMember "
                + "and fails to open the screen, so the designer has to say which DataMember fits."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task ASectionWhoseEntityIsNotInTheDataStructure_IsWarnedAbout()
    {
        var warnings = await PlaceSectionAsync(
            ForeignSectionName,
            ScreenTestModel.MasterDataMember
        );
        TestContext.WriteLine("warnings: " + string.Join(Environment.NewLine, warnings));

        Assert.That(
            string.Join(separator: " | ", warnings),
            Does.Contain(ForeignSectionName).And.Contain(ScreenTestModel.DataStructureName),
            "No DataMember of the screen can show this section, so the designer has to name "
                + "the entity and the data structure that do not fit."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task ASectionWithAFieldTheDataStructureLeavesOut_IsWarnedAbout()
    {
        await UseDataStructureAsync(AllDataTypesDataStructureId);
        var warnings = await PlaceSectionAsync(ArraySectionName, AllDataTypesName);
        TestContext.WriteLine("warnings: " + string.Join(Environment.NewLine, warnings));

        Assert.That(
            string.Join(separator: " | ", warnings),
            Does.Contain(ArrayFieldName),
            "The data structure leaves the Array field out, so the client cannot find the "
                + "field of the section and fails to open the screen."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task ADataStructureEntityThatOnlySharesTheName_IsNamedAndAFittingDataSourceSuggested()
    {
        await UseDataStructureAsync(ChatBusinessPartnerDataStructureId);
        var warnings = await PlaceSectionAsync(NewUserSectionName, BusinessPartnerName);
        TestContext.WriteLine("warnings: " + string.Join(Environment.NewLine, warnings));

        Assert.That(
            string.Join(separator: " | ", warnings),
            Does.Contain(LookupEntityName).And.Contain(UserProfileNewName),
            "The DataMember is called like the entity of the section but shows another entity, "
                + "so the warning has to name that entity and the data source that fits."
        );
    }

    [TestCase(NewUserSectionName)]
    [TestCase(MembershipUserSectionName)]
    [Category(MutatingCategory)]
    public async Task ASectionOnTheDataSourceOfItsOwnScreens_IsNotWarnedAbout(string sectionName)
    {
        await UseDataStructureAsync(UserProfileNewDataStructureId);

        Assert.That(
            await PlaceSectionAsync(sectionName, BusinessPartnerName),
            Is.Empty,
            message: "The data source the warning suggests has to hold every field of the section."
        );
    }

    private async Task UseDataStructureAsync(string dataStructureId)
    {
        using var response = await AgentClient.Architect.PostAsJsonAsync(
            requestUri: "/ScreenEditor/Update",
            new
            {
                schemaItemId = screenId,
                selectedDataSourceId = dataStructureId,
                modelChanges = Array.Empty<object>(),
            },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
    }

    private async Task<IReadOnlyList<string>> PlaceSectionAsync(
        string sectionName,
        string dataMember
    )
    {
        var widgetId = await screens.AddWidgetAsync(screenId, sectionName);
        await screens.SetPropertyAsync(screenId, widgetId, DataMemberProperty, dataMember);
        return (await new ScreenEditorProbe(AgentClient.Architect).ReadAsync(screenId)).Warnings;
    }
}
