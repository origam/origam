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
public sealed class SectionWidgetIntegrationTests : AgentIntegrationTestBase
{
    private const string WidgetSectionSetupName = "WidgetSection_Setup";
    private const string LabelText = "Audit info";
    private const string GroupBoxTitle = "Extra";

    private WidgetTestModel widgets = null!;
    private ActiveEditorFocus widgetFocus = null!;
    private string? widgetSetupProblem;

    [OneTimeTearDown]
    public async Task DeleteTheWidgetSection()
    {
        if (widgets is not null)
        {
            await Model.CloseAllTabsAsync();
            await DeleteItemsNamedAsync([
                widgets.SectionName,
                widgets.YesConstantName,
                widgets.NoConstantName,
                widgets.EntityName,
                widgets.ChildEntityName,
            ]);
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_AsTextBox_IsBoundToTheStringField()
    {
        var trace = await AddWidgetAsync(
            $"add a text box for the field {WidgetTestModel.TextField} below the existing widgets"
        );
        await RequireWidgetAsync(trace, shortType: "AsTextBox", WidgetTestModel.TextField);
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_AsDateBox_IsBoundToTheDateField()
    {
        var trace = await AddWidgetAsync(
            $"add a date box for the field {WidgetTestModel.DateField} below the existing widgets"
        );
        await RequireWidgetAsync(trace, shortType: "AsDateBox", WidgetTestModel.DateField);
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_AsCheckBox_IsBoundToTheBooleanField()
    {
        var trace = await AddWidgetAsync(
            $"add a check box for the field {WidgetTestModel.BooleanField} below the existing "
                + "widgets"
        );
        await RequireWidgetAsync(trace, shortType: "AsCheckBox", WidgetTestModel.BooleanField);
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_Label_CarriesTheRequestedText()
    {
        var trace = await AddWidgetAsync(
            $"add a label with the text {LabelText} below the existing widgets"
        );
        var section = await ReadSectionAsync(trace);
        var label = section
            .Descendants()
            .FirstOrDefault(widget =>
                widget.ShortType == "Label" && widget.Property("Text") == LabelText
            );
        Assert.That(
            label,
            Is.Not.Null,
            $"No Label with Text '{LabelText}' is in the saved section. "
                + DescribeAll(section, trace)
        );
        Assert.That(label!.BoundField, Is.Null, message: "A label must not be bound to a field.");
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_AsDropDown_TakesTheLookupOfTheField()
    {
        var trace = await AddWidgetAsync(
            $"add a drop-down (AsDropDown) for the field {WidgetTestModel.LookupField} below the "
                + "existing widgets"
        );
        var dropDown = await RequireWidgetAsync(
            trace,
            shortType: "AsDropDown",
            WidgetTestModel.LookupField
        );
        Assert.That(
            dropDown.Property("DataLookup"),
            Is.EqualTo(widgets.LookupId).IgnoreCase,
            "The drop-down does not use the field's default lookup. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_ColorPicker_IsBoundToTheIntegerField()
    {
        var trace = await AddWidgetAsync(
            $"add a color picker for the field {WidgetTestModel.ColorField} below the existing "
                + "widgets"
        );
        await RequireWidgetAsync(trace, shortType: "ColorPicker", WidgetTestModel.ColorField);
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_BlobControl_StoresTheFileInTheBlobField()
    {
        var trace = await AddWidgetAsync(
            $"add a file upload control (BlobControl) for the field "
                + $"{WidgetTestModel.FileNameField} that stores the file content in the field "
                + $"{WidgetTestModel.BlobField}, below the existing widgets"
        );
        var blobControl = await RequireWidgetAsync(
            trace,
            shortType: "BlobControl",
            WidgetTestModel.FileNameField
        );
        Assert.That(
            blobControl.Property("BlobMember"),
            Is.EqualTo(WidgetTestModel.BlobField),
            "BlobMember does not name the blob field. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_ImageBox_IsBoundToTheBlobField()
    {
        var trace = await AddWidgetAsync(
            $"add an image box showing the field {WidgetTestModel.BlobField} below the existing "
                + "widgets"
        );
        await RequireWidgetAsync(trace, shortType: "ImageBox", WidgetTestModel.BlobField);
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_GroupBox_HoldsTheTextBoxCreatedInsideIt()
    {
        var trace = await AddWidgetAsync(
            $"add a group box titled {GroupBoxTitle} below the existing widgets and put a text "
                + $"box for the field {WidgetTestModel.GroupedTextField} inside it"
        );
        var section = await ReadSectionAsync(trace);
        var groupBox = section
            .Descendants()
            .FirstOrDefault(widget =>
                widget.ShortType == "GroupBoxWithChamfer"
                && widget.Property("Text") == GroupBoxTitle
            );
        Assert.That(
            groupBox,
            Is.Not.Null,
            $"No GroupBox titled '{GroupBoxTitle}' is in the saved section. "
                + DescribeAll(section, trace)
        );
        Assert.That(
            groupBox!.Find(shortType: "AsTextBox", WidgetTestModel.GroupedTextField),
            Is.Not.Null,
            $"The text box for {WidgetTestModel.GroupedTextField} is not inside the group box. "
                + DescribeAll(section, trace)
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_RadioButtons_SelectTheValuesOfTheirConstants()
    {
        var testModel = await RequireWidgetSectionAsync();
        var trace = await AddWidgetAsync(
            $"add two radio buttons for the field {WidgetTestModel.RadioField} below the "
                + "existing widgets: one labelled Yes that sets the value Yes and one labelled "
                + $"No that sets the value No. Use the data constants {testModel.YesConstantName} "
                + $"(String value Yes) and {testModel.NoConstantName} (String value No), creating "
                + "them if they do not exist yet"
        );
        var section = await ReadSectionAsync(trace);
        var radios = section.FindAll(shortType: "AsRadioButton", WidgetTestModel.RadioField);
        Assert.That(
            radios,
            Has.Count.EqualTo(2),
            $"Expected two radio buttons bound to {WidgetTestModel.RadioField}. "
                + DescribeAll(section, trace)
        );
        Assert.That(
            radios.Select(radio => radio.Property("Text")),
            Is.EquivalentTo(new[] { "Yes", "No" }),
            "The radio buttons do not carry the texts Yes and No. " + trace.Describe()
        );
        foreach (var radio in radios)
        {
            Assert.That(
                IsGuid(radio.Property("ValueConstant")),
                Is.True,
                $"The radio button '{radio.Property("Text")}' has no ValueConstant. "
                    + trace.Describe()
            );
        }
        Assert.That(
            radios.Select(radio => radio.Property("ValueConstant")).Distinct().Count(),
            Is.EqualTo(2),
            "Both radio buttons select the same constant. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_MultiColumnAdapterFieldWrapper_SwitchesBetweenItsTwoChildren()
    {
        var testModel = await RequireWidgetSectionAsync();
        var trace = await AddWidgetAsync(
            $"add a MultiColumnAdapterFieldWrapper that switches on the field "
                + $"{WidgetTestModel.RadioField}, below the existing widgets: when "
                + $"{WidgetTestModel.RadioField} is Yes it shows a text box for the field "
                + $"{WidgetTestModel.WrapperTextField}, when it is No it shows a date box for "
                + $"the field {WidgetTestModel.WrapperDateField}. Use the data constants "
                + $"{testModel.YesConstantName} (String value Yes) and "
                + $"{testModel.NoConstantName} (String value No) for the conditions, creating "
                + "them if they do not exist yet"
        );
        var wrapper = await RequireWidgetAsync(
            trace,
            shortType: "MultiColumnAdapterFieldWrapper",
            WidgetTestModel.RadioField
        );
        var textBox = wrapper.Find(shortType: "AsTextBox", WidgetTestModel.WrapperTextField);
        var dateBox = wrapper.Find(shortType: "AsDateBox", WidgetTestModel.WrapperDateField);
        Assert.That(
            textBox,
            Is.Not.Null,
            $"The wrapper has no text box for {WidgetTestModel.WrapperTextField}. "
                + wrapper.Describe()
                + " "
                + trace.Describe()
        );
        Assert.That(
            dateBox,
            Is.Not.Null,
            $"The wrapper has no date box for {WidgetTestModel.WrapperDateField}. "
                + wrapper.Describe()
                + " "
                + trace.Describe()
        );
        Assert.That(
            IsGuid(textBox!.Property("MappingCondition"))
                && IsGuid(dateBox!.Property("MappingCondition")),
            Is.True,
            "A wrapper child has no MappingCondition constant. " + trace.Describe()
        );
        Assert.That(
            textBox.Property("MappingCondition"),
            Is.Not.EqualTo(dateBox!.Property("MappingCondition")).IgnoreCase,
            "Both wrapper children are shown for the same value. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_Checklist_TakesTheLookupOfTheArrayField()
    {
        var trace = await AddWidgetAsync(
            $"add a checklist for the field {WidgetTestModel.ChecklistArrayField} below the "
                + "existing widgets"
        );
        var checklist = await RequireWidgetAsync(
            trace,
            shortType: "Checklist",
            WidgetTestModel.ChecklistArrayField
        );
        Assert.That(
            checklist.Property("DataLookup"),
            Is.EqualTo(widgets.LookupId).IgnoreCase,
            "The checklist does not use the array field's default lookup. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task Widget_TagInput_TakesTheLookupOfTheArrayField()
    {
        var trace = await AddWidgetAsync(
            $"add a tag input for the field {WidgetTestModel.TagArrayField} below the existing "
                + "widgets"
        );
        var tagInput = await RequireWidgetAsync(
            trace,
            shortType: "TagInput",
            WidgetTestModel.TagArrayField
        );
        Assert.That(
            tagInput.Property("DataLookup"),
            Is.EqualTo(widgets.LookupId).IgnoreCase,
            "The tag input does not use the array field's default lookup. " + trace.Describe()
        );
    }

    private async Task<WidgetTestModel> RequireWidgetSectionAsync()
    {
        if (widgetFocus is not null)
        {
            return widgets;
        }
        Assert.That(
            widgetSetupProblem,
            Is.Null,
            "The widget section was not built earlier in this run: " + widgetSetupProblem
        );
        try
        {
            await BuildWidgetSectionAsync();
        }
        catch (Exception exception) when (exception is not IgnoreException)
        {
            widgetSetupProblem = exception.Message;
            BenchmarkReport.RecordOutcome(
                WidgetSectionSetupName,
                status: "Failed",
                exception.Message
            );
            throw;
        }
        BenchmarkReport.RecordOutcome(WidgetSectionSetupName, status: "Passed", message: null);
        widgetFocus = new ActiveEditorFocus(
            new ChatFocusNode(
                widgets.SectionName,
                ItemTypeName: "Screen Section",
                widgets.SectionId,
                Path: "root"
            )
        );
        TestContext.Progress.WriteLine(
            $"widget section {widgets.SectionName} ({widgets.SectionId}) on entity "
                + $"{widgets.EntityName} ({widgets.EntityId})"
        );
        return widgets;
    }

    private async Task BuildWidgetSectionAsync()
    {
        var builder = new ArchitectModelBuilder(AgentClient.Architect);
        var lookupId = await builder.FindItemIdAsync(
            WidgetTestModel.LookupName,
            WidgetTestModel.LookupTypeName
        );
        if (lookupId is null)
        {
            Assert.Ignore(
                $"The lookup '{WidgetTestModel.LookupName}' from the Root package is not in the "
                    + "loaded model, so the drop-down, tag input and checklist tests have no "
                    + "lookup to bind."
            );
        }
        widgets = new WidgetTestModel(lookupId!);

        var conversation = new AgentConversation();
        var entityTrace = await RunSetupTurnAsync(widgets.EntityPrompt, conversation);
        var arrayFieldsTrace = await RunSetupTurnAsync(widgets.ArrayFieldsPrompt, conversation);
        var sectionTrace = await RunSetupTurnAsync(
            widgets.SectionPrompt,
            conversation,
            mustChangeModel: false
        );
        if (
            await builder.FindItemIdAsync(
                widgets.SectionName,
                WidgetTestModel.ScreenSectionTypeName
            )
            is null
        )
        {
            sectionTrace = await RunSetupTurnAsync(widgets.SectionConfirmPrompt, conversation);
        }
        var problem = await widgets.CompleteAsync(builder);
        Assert.That(
            problem,
            Is.Null,
            "The agent did not build the entity and section the widgets need: "
                + problem
                + Environment.NewLine
                + entityTrace.Describe()
                + Environment.NewLine
                + arrayFieldsTrace.Describe()
                + Environment.NewLine
                + sectionTrace.Describe()
        );
    }

    private async Task<AgentRunTrace> RunSetupTurnAsync(
        string prompt,
        AgentConversation conversation,
        bool mustChangeModel = true
    )
    {
        var trace = await RunBenchmarkAsync(
            prompt,
            focus: null,
            conversation,
            WidgetSectionSetupName
        );
        Assert.That(trace.ErrorMessage, Is.Null, trace.Describe());
        Assert.That(
            trace.Result!.ModelChanged || !mustChangeModel,
            Is.True,
            "The setup turn changed nothing in the model. " + trace.Describe()
        );
        return trace;
    }

    private async Task<AgentRunTrace> AddWidgetAsync(string request)
    {
        var testModel = await RequireWidgetSectionAsync();
        var trace = await RunBenchmarkAsync(
            prompt: $"In the screen section {testModel.SectionName} that I have open, {request}, "
                + "then save the section. Do not ask me to confirm anything, just do it.",
            widgetFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);
        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("SectionEditor"),
            Is.True,
            "The agent never called a SectionEditor tool. Tools used: " + toolNames
        );
        Assert.That(
            trace.UsedTool("Save"),
            Is.True,
            "The agent never saved the section. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent reports no model change. " + trace.Describe()
        );
        return trace;
    }

    private async Task<DesignerWidget> ReadSectionAsync(AgentRunTrace trace)
    {
        var section = await new SectionEditorProbe(AgentClient.Architect).ReadPersistedAsync(
            widgets.SectionId
        );
        TestContext.WriteLine("saved section:" + Environment.NewLine + section.Describe());
        Assert.That(
            section.Children,
            Is.Not.Empty,
            "The saved section has no widgets at all. " + trace.Describe()
        );
        return section;
    }

    private async Task<DesignerWidget> RequireWidgetAsync(
        AgentRunTrace trace,
        string shortType,
        string boundField
    )
    {
        var section = await ReadSectionAsync(trace);
        var widget = section.Find(shortType, boundField);
        Assert.That(
            widget,
            Is.Not.Null,
            $"No {shortType} bound to {boundField} is in the saved section. "
                + DescribeAll(section, trace)
        );
        return widget!;
    }

    private static string DescribeAll(DesignerWidget section, AgentRunTrace trace)
    {
        return "Widgets: " + section.Describe() + Environment.NewLine + trace.Describe();
    }

    private static bool IsGuid(string value)
    {
        return Guid.TryParse(value, out var id) && id != Guid.Empty;
    }
}
