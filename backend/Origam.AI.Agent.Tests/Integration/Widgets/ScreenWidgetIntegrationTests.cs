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
using Origam.AI.Agent.Tests.Infrastructure.Agent;
using Origam.AI.Agent.Tests.Infrastructure.Architect;

namespace Origam.AI.Agent.Tests.Integration.Widgets;

[TestFixture]
[Explicit(AgentIntegrationTestBase.ExplicitReason)]
[Category(AgentIntegrationTestBase.IntegrationCategory)]
public sealed class ScreenWidgetIntegrationTests : AgentIntegrationTestBase
{
    private const string LabelText = "Audit info";
    private const string SecondLabelText = "Scratch";
    private const string FirstTabTitle = "Rows";
    private const string SecondTabTitle = "Info";
    private const string ThirdTabTitle = "Notes";
    private const string PluginTextProperty = "test1";
    private const string PluginNumberProperty = "test2";
    private const string PluginTextValue = "screen";
    private const string PluginNumberValue = "7";

    private ScreenTestModel screens = null!;
    private TestScreen screen = null!;
    private string? packageToRestore;

    [OneTimeSetUp]
    public async Task ActivateThePackageThatHoldsTheScreenWidgets()
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
                        + "screen widgets and the sections these tests place have nowhere to live."
                );
            }
            packageToRestore = activePackage;
        }

        if (await screens.FindDataStructureIdAsync() is null)
        {
            Assert.Ignore(
                $"The data structure '{ScreenTestModel.DataStructureName}' is not in the loaded "
                    + "model, so a screen built here would have no data members to bind to."
            );
        }
    }

    [OneTimeTearDown]
    public async Task PutTheOriginalPackageBack()
    {
        if (packageToRestore is not null)
        {
            await InProcessArchitect.ActivatePackageAsync(packageToRestore);
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_ScreenSection_FillsTheScreenAndShowsItsDataMember()
    {
        var trace = await BuildScreenAsync(
            $"add the screen section {ScreenTestModel.MasterSectionName} for the data member "
                + ScreenTestModel.MasterDataMember
        );
        var content = await ReadScreenAsync(trace);
        var section = RequireWidget(content, shortType: "PanelControlSet", trace);
        Assert.That(
            section.Property("DataMember"),
            Is.EqualTo(ScreenTestModel.MasterDataMember),
            "The screen section shows another data member. " + Describe(content, trace)
        );
        Assert.That(
            (section.Number("Top"), section.Number("Left"), section.Number("Width")),
            Is.EqualTo((0, 0, ScreenTestModel.ScreenWidth)),
            "The only widget of a screen is stretched over the whole screen by the server. "
                + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_SplitPanel_DocksItsTwoSectionsSideBySide()
    {
        var trace = await BuildScreenAsync(
            "add a SplitPanel whose two children sit side by side: the screen section "
                + $"{ScreenTestModel.MasterSectionName} for the data member "
                + $"{ScreenTestModel.MasterDataMember} on the left and the screen section "
                + $"{ScreenTestModel.DetailSectionName} for the data member "
                + $"{ScreenTestModel.DetailDataMember} on the right"
        );
        var content = await ReadScreenAsync(trace);
        var splitPanel = RequireWidget(content, shortType: "SplitPanel", trace);
        Assert.That(
            splitPanel.Property("Orientation"),
            Is.EqualTo("1"),
            "Orientation 1 is what puts the two halves next to each other. "
                + Describe(content, trace)
        );
        Assert.That(
            splitPanel.Children,
            Has.Count.EqualTo(2),
            "A SplitPanel holds exactly two widgets. " + Describe(content, trace)
        );

        var (left, right) = (splitPanel.Children[0], splitPanel.Children[1]);
        Assert.That(
            new[] { left.Property("DataMember"), right.Property("DataMember") },
            Is.EquivalentTo(
                new[] { ScreenTestModel.MasterDataMember, ScreenTestModel.DetailDataMember }
            ),
            "The two halves do not show the two sections that were asked for. "
                + Describe(content, trace)
        );
        Assert.That(
            right.Number("Left"),
            Is.GreaterThanOrEqualTo(left.Number("Left") + left.Number("Width")),
            "The server did not dock the second half to the right of the first one. "
                + Describe(content, trace)
        );
        Assert.That(
            right.Number("Top"),
            Is.EqualTo(left.Number("Top")),
            "Halves that sit side by side start at the same height. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_TabControl_KeepsOneWidgetOnEachOfItsTwoPages()
    {
        var trace = await BuildScreenAsync(
            $"add a TabControl with two tab pages: the first titled {FirstTabTitle} holds the "
                + $"screen section {ScreenTestModel.DetailSectionName} for the data member "
                + $"{ScreenTestModel.DetailDataMember}, the second titled {SecondTabTitle} holds "
                + $"a Label with the text {LabelText}"
        );
        var content = await ReadScreenAsync(trace);
        var tabControl = RequireWidget(content, shortType: "AsTabControl", trace);
        var pages = tabControl.FindAll(shortType: "TabPage");
        Assert.That(
            pages.Select(page => page.Property("Text")),
            Is.EquivalentTo(new[] { FirstTabTitle, SecondTabTitle }),
            "The tab pages do not carry the two captions that were asked for. "
                + Describe(content, trace)
        );

        var sectionPage = RequirePage(pages, FirstTabTitle, content, trace);
        var section = sectionPage.Find(shortType: "PanelControlSet");
        Assert.That(
            section?.Property("DataMember"),
            Is.EqualTo(ScreenTestModel.DetailDataMember),
            $"The '{FirstTabTitle}' page does not hold the screen section. "
                + Describe(content, trace)
        );
        Assert.That(
            (section!.Number("Top"), section.Number("Left")),
            Is.EqualTo((0, 0)),
            "The only widget of a tab page is stretched over the page by the server. "
                + Describe(content, trace)
        );

        var labelPage = RequirePage(pages, SecondTabTitle, content, trace);
        Assert.That(
            labelPage.Find(shortType: "Label")?.Property("Text"),
            Is.EqualTo(LabelText),
            $"The '{SecondTabTitle}' page does not hold the label. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_Panel_HoldsTheLabelCreatedInsideIt()
    {
        var trace = await BuildScreenAsync(
            $"add a Panel and put a Label with the text {LabelText} inside it"
        );
        var content = await ReadScreenAsync(trace);
        var panel = RequireWidget(content, shortType: "Panel", trace);
        Assert.That(
            panel.Find(shortType: "Label")?.Property("Text"),
            Is.EqualTo(LabelText),
            "The label is not inside the panel. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_AsTree_GetsItsDataMemberAndThreeColumns()
    {
        var trace = await BuildScreenAsync(
            $"add an AsTree for the data member {ScreenTestModel.DetailDataMember} with IDColumn "
                + $"{ScreenTestModel.TreeIdColumn}, ParentIDColumn "
                + $"{ScreenTestModel.TreeParentIdColumn} and NameColumn "
                + ScreenTestModel.TreeNameColumn
        );
        var content = await ReadScreenAsync(trace);
        var tree = RequireWidget(content, shortType: "AsTreeView", trace);
        Assert.That(
            (
                tree.Property("DataMember"),
                tree.Property("IDColumn"),
                tree.Property("ParentIDColumn"),
                tree.Property("NameColumn")
            ),
            Is.EqualTo(
                (
                    ScreenTestModel.DetailDataMember,
                    ScreenTestModel.TreeIdColumn,
                    ScreenTestModel.TreeParentIdColumn,
                    ScreenTestModel.TreeNameColumn
                )
            ),
            "The tree cannot be built from these columns. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_Plugin_CarriesItsDataMemberAndCustomProperties()
    {
        var trace = await BuildScreenAsync(
            $"add the widget {ScreenTestModel.PluginName} for the data member "
                + $"{ScreenTestModel.MasterDataMember} and set its property "
                + $"{PluginTextProperty} to {PluginTextValue} and its property "
                + $"{PluginNumberProperty} to {PluginNumberValue}"
        );
        var content = await ReadScreenAsync(trace);
        var plugin = RequireWidget(content, shortType: "SectionLevelPlugin", trace);
        Assert.That(
            plugin.Property("Text"),
            Is.EqualTo(ScreenTestModel.PluginName),
            "The client looks the plugin up by the name in Text. " + Describe(content, trace)
        );
        Assert.That(
            (
                plugin.Property("DataMember"),
                plugin.Property(PluginTextProperty),
                plugin.Property(PluginNumberProperty)
            ),
            Is.EqualTo((ScreenTestModel.MasterDataMember, PluginTextValue, PluginNumberValue)),
            "The plugin did not keep the properties it was given. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_Panel_IsBuiltOnItsOwnWhenNothingElseIsAsked()
    {
        var trace = await BuildScreenAsync("add a Panel and leave it empty");
        var content = await ReadScreenAsync(trace);
        var panel = RequireWidget(content, shortType: "Panel", trace);
        Assert.That(
            panel.Children,
            Is.Empty,
            "The agent put something into a panel that was to stay empty. "
                + Describe(content, trace)
        );
        Assert.That(
            content.Root.Children,
            Has.Count.EqualTo(1),
            "The client shows only the first widget of a screen, so an empty panel has to be "
                + "the only one. "
                + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_Label_IsBuiltOnItsOwnWithTheTextItWasGiven()
    {
        var trace = await BuildScreenAsync($"add a single Label with the text {LabelText}");
        var content = await ReadScreenAsync(trace);
        Assert.That(
            RequireWidget(content, shortType: "Label", trace).Property("Text"),
            Is.EqualTo(LabelText),
            "The label does not carry the text it was given. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_TabControl_TakesAThirdPage()
    {
        var trace = await BuildScreenAsync(
            $"add a TabControl with three tab pages titled {FirstTabTitle}, {SecondTabTitle} and "
                + $"{ThirdTabTitle}, and put the screen section "
                + $"{ScreenTestModel.MasterSectionName} for the data member "
                + $"{ScreenTestModel.MasterDataMember} on the {FirstTabTitle} page"
        );
        var content = await ReadScreenAsync(trace);
        var tabControl = RequireWidget(content, shortType: "AsTabControl", trace);
        Assert.That(
            tabControl.FindAll(shortType: "TabPage").Select(page => page.Property("Text")),
            Is.EquivalentTo(new[] { FirstTabTitle, SecondTabTitle, ThirdTabTitle }),
            "A TabControl starts with two pages, so the third one has to be added. "
                + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_SplitPanel_NestsAnotherSplitPanelInOneOfItsHalves()
    {
        var trace = await BuildScreenAsync(
            "add a SplitPanel whose upper half is another SplitPanel holding the screen section "
                + $"{ScreenTestModel.MasterSectionName} for the data member "
                + $"{ScreenTestModel.MasterDataMember} and the screen section "
                + $"{ScreenTestModel.DetailSectionName} for the data member "
                + $"{ScreenTestModel.DetailDataMember}, and whose lower half is the screen "
                + $"section {ScreenTestModel.MasterSectionName} for the data member "
                + ScreenTestModel.MasterDataMember
        );
        var content = await ReadScreenAsync(trace);
        var outer = RequireWidget(content, shortType: "SplitPanel", trace);
        var inner = outer.Children.FirstOrDefault(child => child.ShortType == "SplitPanel");
        Assert.That(
            inner,
            Is.Not.Null,
            "The second SplitPanel is not a child of the first one. " + Describe(content, trace)
        );
        Assert.That(
            (outer.Children.Count, inner!.Children.Count),
            Is.EqualTo((2, 2)),
            "Both SplitPanels have to hold exactly two widgets. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Screen_DeletingAWidget_LeavesTheOtherWidgetsInPlace()
    {
        var trace = await BuildScreenAsync(
            $"add a Panel with two Labels inside it, one with the text {LabelText} and one with "
                + $"the text {SecondLabelText}, then delete the Label with the text "
                + SecondLabelText
        );
        var content = await ReadScreenAsync(trace);
        var panel = RequireWidget(content, shortType: "Panel", trace);
        Assert.That(
            panel.FindAll(shortType: "Label").Select(label => label.Property("Text")),
            Is.EqualTo(new[] { LabelText }),
            $"Only the label '{SecondLabelText}' was to be deleted. " + Describe(content, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Screen_MovingAWidget_PutsItUnderTheNewParent()
    {
        var trace = await BuildScreenAsync(
            $"add a TabControl whose first page is titled {FirstTabTitle} and whose second page "
                + $"is titled {SecondTabTitle}, put a Label with the text {LabelText} on the "
                + $"{FirstTabTitle} page, then move that Label to the {SecondTabTitle} page"
        );
        var content = await ReadScreenAsync(trace);
        var pages = RequireWidget(content, shortType: "AsTabControl", trace)
            .FindAll(shortType: "TabPage");
        Assert.That(
            RequirePage(pages, SecondTabTitle, content, trace)
                .Find(shortType: "Label")
                ?.Property("Text"),
            Is.EqualTo(LabelText),
            $"The label did not end up on the '{SecondTabTitle}' page. " + Describe(content, trace)
        );
        Assert.That(
            RequirePage(pages, FirstTabTitle, content, trace).FindAll(shortType: "Label"),
            Is.Empty,
            $"The label is still on the '{FirstTabTitle}' page as well, so it was copied "
                + "instead of moved. "
                + Describe(content, trace)
        );
    }

    private async Task<AgentRunTrace> BuildScreenAsync(string request)
    {
        screen = await screens.CreateEmptyScreenAsync(
            "AiScreen" + Guid.NewGuid().ToString("N")[..8]
        );
        CreatedEntityNames.Add(screen.Name);

        var trace = await RunBenchmarkAsync(
            prompt: $"In the screen {screen.Name} that I have open, {request}, then save the "
                + "screen. Do not ask me to confirm anything, just do it.",
            screen.Focus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);
        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("ScreenEditor"),
            Is.True,
            "The agent never called a ScreenEditor tool. Tools used: " + toolNames
        );
        Assert.That(
            trace.UsedTool("Save"),
            Is.True,
            "The agent never saved the screen. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent reports no model change. " + trace.Describe()
        );
        return trace;
    }

    private async Task<ScreenEditorContent> ReadScreenAsync(AgentRunTrace trace)
    {
        var content = await new ScreenEditorProbe(AgentClient.Architect).ReadPersistedAsync(
            screen.Id
        );
        TestContext.WriteLine("saved screen:" + Environment.NewLine + content.Describe());
        Assert.That(
            content.Root.Children,
            Is.Not.Empty,
            "The saved screen has no widgets at all. " + trace.Describe()
        );
        Assert.That(
            content.Warnings,
            Is.Empty,
            "The agent saved a screen the client cannot show. " + Describe(content, trace)
        );
        return content;
    }

    private static DesignerWidget RequireWidget(
        ScreenEditorContent content,
        string shortType,
        AgentRunTrace trace
    )
    {
        var widget = content.Root.Find(shortType);
        Assert.That(
            widget,
            Is.Not.Null,
            $"No {shortType} is in the saved screen. " + Describe(content, trace)
        );
        return widget!;
    }

    private static DesignerWidget RequirePage(
        IReadOnlyList<DesignerWidget> pages,
        string title,
        ScreenEditorContent content,
        AgentRunTrace trace
    )
    {
        var page = pages.FirstOrDefault(candidate => candidate.Property("Text") == title);
        Assert.That(
            page,
            Is.Not.Null,
            $"The saved screen has no tab page titled '{title}'. " + Describe(content, trace)
        );
        return page!;
    }

    private static string Describe(ScreenEditorContent content, AgentRunTrace trace)
    {
        return "Screen: "
            + Environment.NewLine
            + content.Describe()
            + Environment.NewLine
            + trace.Describe();
    }
}
