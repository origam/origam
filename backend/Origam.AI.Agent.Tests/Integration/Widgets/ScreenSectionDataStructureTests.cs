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

using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure.Architect;

namespace Origam.AI.Agent.Tests.Integration.Widgets;

[TestFixture]
[Category(AgentIntegrationTestBase.IntegrationCategory)]
public sealed class ScreenSectionDataStructureTests : AgentIntegrationTestBase
{
    private const string SectionName = "WidgetSectionTestDetail";
    private const string SectionTypeName = "Screen Section";
    private const string ForeignEntityName = "BusinessPartner";
    private const string PackageName = "Widgets";
    private const string DataStructureName = "WidgetSectionTest";

    private SectionDataSourceProbe probe = null!;
    private string sectionId = string.Empty;
    private string foreignEntityId = string.Empty;
    private string? packageToRestore;

    [OneTimeSetUp]
    public async Task FindTheSectionThatScreensShow()
    {
        probe = new SectionDataSourceProbe(AgentClient.Architect);
        if (
            InProcessArchitect.ActivePackageName is { } activePackage
            && activePackage != PackageName
        )
        {
            if (!await InProcessArchitect.ActivatePackageAsync(PackageName))
            {
                Assert.Ignore(
                    $"The package '{PackageName}' is not in the model, so there "
                        + "is no screen section that screens show."
                );
            }
            packageToRestore = activePackage;
        }

        if (!await probe.WaitForReferenceIndexAsync())
        {
            Assert.Ignore(
                "The model reference index is not available, so the screens that show a screen "
                    + "section cannot be looked up."
            );
        }

        var builder = new ArchitectModelBuilder(AgentClient.Architect);
        sectionId = await builder.FindItemIdAsync(SectionName, SectionTypeName) ?? string.Empty;
        if (sectionId == string.Empty)
        {
            Assert.Ignore($"The screen section '{SectionName}' is not in the loaded model.");
        }

        foreignEntityId =
            await probe.FindEntityIdAsync(sectionId, ForeignEntityName) ?? string.Empty;
        if (foreignEntityId == string.Empty)
        {
            Assert.Ignore(
                $"The entity '{ForeignEntityName}' is not offered to '{SectionName}' as a data "
                    + "source, so the section cannot be moved off its own data structure here."
            );
        }
    }

    [OneTimeTearDown]
    public async Task DropTheEditedSectionAndPutThePackageBack()
    {
        await Model.CloseAllTabsAsync();
        if (packageToRestore is not null)
        {
            await InProcessArchitect.ActivatePackageAsync(packageToRestore);
        }
    }

    [Test]
    public async Task AnUntouchedSection_FitsTheDataStructureOfEveryScreenThatShowsIt()
    {
        Assert.That(
            await probe.ReadWarningsAsync(sectionId),
            Is.Empty,
            $"'{SectionName}' is expected to fit the screens that show it before anything "
                + "changes its entity."
        );
    }

    [Test]
    public async Task PointingTheSectionAtAnotherEntity_WarnsAboutTheScreensThatBreak()
    {
        var warnings = await probe.ReadWarningsAsync(sectionId, foreignEntityId);
        TestContext.WriteLine("warnings: " + string.Join(Environment.NewLine, warnings));

        Assert.That(
            warnings,
            Is.Not.Empty,
            $"Moving '{SectionName}' to '{ForeignEntityName}' leaves every screen that shows it "
                + "with a data structure that does not hold that entity, and the client then "
                + "fails to open those screens. The editor has to say so."
        );
        Assert.That(
            string.Join(separator: " | ", warnings),
            Does.Contain(ForeignEntityName).And.Contain(DataStructureName),
            "The warning has to name the entity and the data structure that do not fit, "
                + "otherwise nobody can act on it."
        );
    }
}
