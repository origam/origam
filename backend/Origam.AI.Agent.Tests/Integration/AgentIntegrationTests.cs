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
using Origam.AI.Agent.Models.Responses;
using Origam.AI.Agent.Tests.Infrastructure.Agent;
using Origam.AI.Agent.Tests.Infrastructure.Architect;
using Origam.AI.Agent.Tests.Infrastructure.Benchmark;

namespace Origam.AI.Agent.Tests.Integration;

[TestFixture]
[Explicit(
    "Calls a live LLM and costs money. Boots Architect in-process; needs the ORIGAM model on "
        + "disk and an AI API key in appsettings.Development.json."
)]
[Category("AiIntegration")]
public sealed class AgentIntegrationTests
{
    private const string ArchitectUrlVariable = "ORIGAM_ARCHITECT_URL";
    private const string MutatingCategory = "AiMutating";
    private const string WidgetSectionSetupName = "WidgetSection_Setup";
    private const string LabelText = "Audit info";
    private const string GroupBoxTitle = "Extra";
    private static readonly string[]? DefaultSections = null;
    private static readonly string[] ExpandedTreePath = ["Data", "Entities", "Dimensions"];

    private readonly List<string> createdEntityNames = [];

    private ArchitectAgentClient agentClient = null!;
    private ArchitectModelProbe model = null!;
    private SectionEditorProbe sectionProbe = null!;
    private AgentHealth agentHealth = null!;
    private ChatFocusPayload chatFocus = null!;
    private WidgetTestModel widgets = null!;
    private ActiveEditorFocus widgetFocus = null!;
    private string? widgetSetupProblem;

    [OneTimeSetUp]
    public async Task ConnectToArchitect()
    {
        var liveUrl = Environment.GetEnvironmentVariable(ArchitectUrlVariable);
        string backend;
        if (string.IsNullOrWhiteSpace(liveUrl))
        {
            try
            {
                agentClient = new ArchitectAgentClient(
                    await InProcessArchitect.GetClientAsync(CancellationToken.None)
                );
            }
            catch (Exception exception)
            {
                Assert.Ignore(
                    $"The in-process Architect did not boot: {exception.GetType().Name}: "
                        + exception.Message
                );
            }
            backend = $"in-process Architect, package '{InProcessArchitect.ActivePackageName}'";
        }
        else
        {
            agentClient = new ArchitectAgentClient(liveUrl);
            backend = $"live Architect at {liveUrl}";
        }

        var health = await agentClient.TryGetHealthAsync(CancellationToken.None);
        if (health is null)
        {
            Assert.Ignore($"No agent endpoint answering on the {backend}.");
        }

        agentHealth = health!;
        model = new ArchitectModelProbe(agentClient.Architect);
        sectionProbe = new SectionEditorProbe(agentClient.Architect);
        chatFocus = await ChatFocusFactory.FromExpandedPathAsync(
            agentClient.Architect,
            ExpandedTreePath,
            CancellationToken.None
        );
        BenchmarkReport.Backend = backend;
        TestContext.Progress.WriteLine(
            $"{backend} | {agentHealth.Model} @ {agentHealth.Endpoint} "
                + $"(key configured: {agentHealth.HasApiKey}) | focus: {chatFocus.Describe()}"
        );
    }

    [OneTimeTearDown]
    public async Task DeleteTheWidgetSectionThenDisconnect()
    {
        if (widgets is not null)
        {
            await model.CloseAllTabsAsync();
            await DeleteItemsNamedAsync([
                widgets.SectionName,
                widgets.YesConstantName,
                widgets.NoConstantName,
                widgets.EntityName,
                widgets.ChildEntityName,
            ]);
        }
        agentClient?.Dispose();
    }

    [SetUp]
    public async Task DropTabsLeftOpenByThePreviousTest()
    {
        await model.CloseAllTabsAsync();
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task CreateNodeFunctionCall_ReallyCreatesTheEntityInTheModel()
    {
        var entityName = "AiBenchmark" + Guid.NewGuid().ToString("N")[..8];
        createdEntityNames.Add(entityName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Create a new database entity named {entityName} in dimensions folder"
                + "Do not ask me to confirm anything, just create it.",
            chatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("CreateNode"),
            Is.True,
            "CreateNode was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );
        Assert.That(trace.Result.AffectedNodes, Is.Not.Empty);

        var matches = await model.FindSchemaItemsAsync(entityName);
        Assert.That(
            matches,
            Is.Not.Empty,
            $"The agent reported success but '{entityName}' is not in the model."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task LookupWizard_CreatesTheLookupAndItsDataStructure()
    {
        var lookupName = "AiLookup" + Guid.NewGuid().ToString("N")[..8];
        var dataStructureName = "Lookup" + lookupName;
        createdEntityNames.Add(lookupName);
        createdEntityNames.Add(dataStructureName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Create a lookup named {lookupName} for the entity Dimension1: display "
                + "field Name, id filter GetId, no list filter. Do not ask me to confirm "
                + "anything, just create it.",
            chatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("PostWizardsLookups"),
            Is.True,
            "The lookup wizard was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called the lookup wizard but nothing was persisted. " + trace.Describe()
        );

        var builder = new ArchitectModelBuilder(agentClient.Architect);
        Assert.That(
            await builder.FindItemIdAsync(lookupName, WidgetTestModel.LookupTypeName),
            Is.Not.Null,
            $"The agent reported success but the lookup '{lookupName}' is not in the model."
        );
        Assert.That(
            await builder.FindItemIdAsync(dataStructureName, itemTypeName: "Data Structure"),
            Is.Not.Null,
            $"The lookup was created but its data structure '{dataStructureName}' is not in the model."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task OneToManyFunctionCalls_BothChildEntitiesPointAtTheMainEntity()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var mainEntityName = "AiBenchmarkOrder" + suffix;
        var firstChildEntityName = "AiBenchmarkOrderLine" + suffix;
        var secondChildEntityName = "AiBenchmarkOrderNote" + suffix;
        createdEntityNames.Add(firstChildEntityName);
        createdEntityNames.Add(secondChildEntityName);
        createdEntityNames.Add(mainEntityName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Create three database entities named {mainEntityName}, "
                + $"{firstChildEntityName} and {secondChildEntityName}. {mainEntityName} is the "
                + "main entity and the other two are its children in a one-to-many relation: "
                + $"give each child a foreign key field named ref{mainEntityName}Id that points "
                + $"at {mainEntityName} and at its primary key. Do not ask me to confirm "
                + "anything, just create everything.",
            chatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );

        var mainEntityId = await RequireEntityAsync(mainEntityName);
        foreach (var childEntityName in new[] { firstChildEntityName, secondChildEntityName })
        {
            var childEntityId = await RequireEntityAsync(childEntityName);
            var foreignKeyFields = new List<string>();
            foreach (var field in await model.ReadDatabaseFieldsAsync(childEntityId))
            {
                if (await model.PointsAtEntityAsync(field.Id, mainEntityId))
                {
                    foreignKeyFields.Add(field.Id);
                }
            }

            Assert.That(
                foreignKeyFields,
                Is.Not.Empty,
                $"'{childEntityName}' has no database field whose ForeignKeyEntity and "
                    + $"ForeignKeyField point at '{mainEntityName}' ({mainEntityId}). "
                    + trace.Describe()
            );
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task DeleteFollowUp_RemovesTheEntityTheAgentCreatedInTheSameConversation()
    {
        var entityName = "AiBenchmarkDeleteMe" + Guid.NewGuid().ToString("N")[..8];
        createdEntityNames.Add(entityName);
        var conversation = new AgentConversation();

        var createTrace = await RunBenchmarkAsync(
            prompt: $"Create a new database entity named {entityName} with one database field "
                + "named Name. Do not ask me to confirm anything, just create it.",
            chatFocus,
            conversation
        );

        Assert.That(createTrace.ErrorMessage, Is.Null);
        Assert.That(
            createTrace.Result!.ModelChanged,
            Is.True,
            "Nothing was persisted by the create turn. " + createTrace.Describe()
        );

        var entityId = await RequireEntityAsync(entityName);
        var fieldNames = (await model.ReadDatabaseFieldsAsync(entityId)).Select(field =>
            field.Name
        );
        Assert.That(
            fieldNames,
            Does.Contain("Name"),
            $"'{entityName}' was created without a database field named Name. "
                + createTrace.Describe()
        );

        var deleteTrace = await RunBenchmarkAsync(
            prompt: "Now delete that entity you have just created, together with everything "
                + "inside it. Do not ask me to confirm anything, just delete it.",
            chatFocus,
            conversation
        );

        Assert.That(deleteTrace.ErrorMessage, Is.Null);
        Assert.That(
            deleteTrace.Result!.ModelChanged,
            Is.True,
            "The delete turn changed nothing in the model. " + deleteTrace.Describe()
        );
        Assert.That(
            await model.FindSchemaItemsAsync(entityName, itemTypeName: "Database Entity"),
            Is.Empty,
            $"The agent reported the deletion but '{entityName}' ({entityId}) is still in the "
                + "model. "
                + deleteTrace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task CommunityGuidedApi_ReadsTheForumGuideAndThenCreatesTheDataPage()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var entityName = "AiBenchmarkApi" + suffix;
        var dataStructureName = "AiBenchmarkApi" + suffix + "Structure";
        var pageName = "AiBenchmarkApi" + suffix + "Page";
        var pageUrl = "api/public/aibenchmark" + suffix;
        createdEntityNames.Add(pageName);
        createdEntityNames.Add(dataStructureName);
        createdEntityNames.Add(entityName);

        var trace = await RunBenchmarkAsync(
            prompt: "Search the ORIGAM community forum for the guide on how to create a simple "
                + "API and read it before you touch the model, then build the API the way that "
                + $"guide describes: a database entity named {entityName} with one database "
                + $"field named Name, a data structure named {dataStructureName} over that "
                + $"entity, and a data page named {pageName} that publishes that data structure "
                + $"as JSON on the url {pageUrl}. Do not ask me to confirm anything, just create "
                + "everything.",
            chatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);

        var createIndex = trace.FirstToolIndex("CreateNode");
        Assert.That(
            createIndex,
            Is.GreaterThanOrEqualTo(0),
            "CreateNode was never called. Tools used: " + toolNames
        );

        var searchIndex = trace.FirstToolIndex("SearchCommunity");
        Assert.That(
            searchIndex,
            Is.GreaterThanOrEqualTo(0).And.LessThan(createIndex),
            "The agent did not search the community forum before it started creating the API. "
                + "Tools used: "
                + toolNames
        );

        var readTopicIndex = trace.FirstToolIndex("ReadCommunityTopic");
        Assert.That(
            readTopicIndex,
            Is.GreaterThanOrEqualTo(0).And.LessThan(createIndex),
            "The agent never opened a community topic before it started creating the API, so it "
                + "built the API without the guide. Tools used: "
                + toolNames
        );

        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );

        var dataStructureMatches = await model.FindSchemaItemsAsync(
            dataStructureName,
            itemTypeName: "Data Structure"
        );
        Assert.That(
            dataStructureMatches,
            Is.Not.Empty,
            $"The data structure '{dataStructureName}' the API should read from is not in the "
                + "model. "
                + trace.Describe()
        );

        var pageMatches = await model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page");
        Assert.That(
            pageMatches,
            Is.Not.Empty,
            $"The agent reported success but the data page '{pageName}' is not in the model. "
                + trace.Describe()
        );

        var pageProperties = await model.ReadItemPropertiesAsync(pageMatches[0]);
        Assert.That(
            pageProperties.GetValueOrDefault(key: "Url", defaultValue: ""),
            Does.Contain(pageUrl).IgnoreCase,
            $"'{pageName}' was published on a different url. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "MimeType", defaultValue: ""),
            Does.Contain("json").IgnoreCase,
            $"'{pageName}' does not return JSON. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "DataStructure", defaultValue: ""),
            Does.Contain(dataStructureMatches[0]).IgnoreCase,
            $"'{pageName}' does not read from '{dataStructureName}' "
                + $"({dataStructureMatches[0]}). "
                + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task UnguidedApi_BuildsTheJsonApiWithoutSearchingTheForum()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var entityName = "AiBenchmarkJsonApi" + suffix;
        var dataStructureName = "AiBenchmarkJsonApi" + suffix + "Structure";
        var pageName = "AiBenchmarkJsonApi" + suffix + "Page";
        var pageUrl = "api/public/aibenchmarkjson" + suffix;
        createdEntityNames.Add(pageName);
        createdEntityNames.Add(dataStructureName);
        createdEntityNames.Add(entityName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Make me a public JSON API on the url {pageUrl} that returns the records of "
                + $"a new database entity named {entityName} with one text field named Name. Call "
                + $"the data structure {dataStructureName} and the data page {pageName}. Do not "
                + "ask me to confirm anything, just build it.",
            chatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("Community"),
            Is.False,
            "The community forum was available but this task is plain model work, so the agent "
                + "should have built the API without spending calls on the forum. Tools used: "
                + toolNames
        );
        Assert.That(
            trace.UsedTool("CreateNode"),
            Is.True,
            "CreateNode was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );

        var entityId = await RequireEntityAsync(entityName);
        var fieldNames = (await model.ReadDatabaseFieldsAsync(entityId))
            .Select(field => field.Name)
            .ToList();
        Assert.That(
            fieldNames,
            Does.Contain("Name"),
            $"'{entityName}' has no database field named Name. Fields found: "
                + string.Join(separator: ", ", fieldNames)
                + ". "
                + trace.Describe()
        );

        var dataStructureMatches = await model.FindSchemaItemsAsync(
            dataStructureName,
            itemTypeName: "Data Structure"
        );
        Assert.That(
            dataStructureMatches,
            Is.Not.Empty,
            $"The data structure '{dataStructureName}' the API should read from is not in the "
                + "model. "
                + trace.Describe()
        );

        var pageMatches = await model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page");
        Assert.That(
            pageMatches,
            Is.Not.Empty,
            $"The agent reported success but the data page '{pageName}' is not in the model. "
                + trace.Describe()
        );

        var pageProperties = await model.ReadPersistedPropertiesAsync(pageMatches[0]);
        Assert.That(
            pageProperties.GetValueOrDefault(key: "Url", defaultValue: ""),
            Does.Contain(pageUrl).IgnoreCase,
            $"'{pageName}' was published on a different url. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "MimeType", defaultValue: ""),
            Does.Contain("json").IgnoreCase,
            $"'{pageName}' does not return JSON. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "DataStructure", defaultValue: ""),
            Does.Contain(dataStructureMatches[0]).IgnoreCase,
            $"'{pageName}' does not read from '{dataStructureName}' "
                + $"({dataStructureMatches[0]}). "
                + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task PropertyEditorFollowUp_ChangesTheUrlOfTheDataPageItAlreadyPersisted()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var entityName = "AiBenchmarkProperty" + suffix;
        var dataStructureName = "AiBenchmarkProperty" + suffix + "Structure";
        var pageName = "AiBenchmarkProperty" + suffix + "Page";
        var originalUrl = "api/public/aibenchmarkproperty" + suffix;
        var changedUrl = "api/public/aibenchmarkproperty" + suffix + "renamed";
        createdEntityNames.Add(pageName);
        createdEntityNames.Add(dataStructureName);
        createdEntityNames.Add(entityName);
        var conversation = new AgentConversation();

        var createTrace = await RunBenchmarkAsync(
            prompt: $"Create a database entity named {entityName} with one database field named "
                + $"Name, a data structure named {dataStructureName} over that entity, and a data "
                + $"page named {pageName} that publishes that data structure as JSON on the url "
                + $"{originalUrl}. Do not ask me to confirm anything, just create everything.",
            chatFocus,
            conversation
        );

        Assert.That(createTrace.ErrorMessage, Is.Null);

        var pagesBeforeEdit = await model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page");
        Assert.That(
            pagesBeforeEdit,
            Is.Not.Empty,
            $"The setup turn left no data page named '{pageName}' in the model, so there is "
                + "nothing for the follow-up turn to edit. "
                + createTrace.Describe()
        );

        var pageId = pagesBeforeEdit[0];
        Assert.That(
            (await model.ReadPersistedPropertiesAsync(pageId)).GetValueOrDefault(
                key: "Url",
                defaultValue: ""
            ),
            Does.Contain(originalUrl).IgnoreCase,
            $"The setup turn did not publish '{pageName}' on '{originalUrl}', so the follow-up "
                + "turn would not be changing what this test assumes. "
                + createTrace.Describe()
        );

        var editTrace = await RunBenchmarkAsync(
            prompt: $"Now change the url of the data page {pageName} to {changedUrl}. Edit the "
                + "page that already exists, do not create a second one, and do not ask me to "
                + "confirm anything.",
            chatFocus,
            conversation
        );

        var toolNames = editTrace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(editTrace.ErrorMessage, Is.Null);
        Assert.That(
            editTrace.UsedTool("PropertyEditor"),
            Is.True,
            "The agent never called PropertyEditor/Update, so it did not edit the page it had "
                + "already persisted. Tools used: "
                + toolNames
        );
        Assert.That(
            await model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page"),
            Is.EqualTo(new[] { pageId }),
            $"The agent replaced '{pageName}' instead of editing the page it had already "
                + "persisted. "
                + editTrace.Describe()
        );
        Assert.That(
            (await model.ReadPersistedPropertiesAsync(pageId)).GetValueOrDefault(
                key: "Url",
                defaultValue: ""
            ),
            Does.Contain(changedUrl).IgnoreCase,
            $"'{pageName}' still carries the old url once its tab is dropped and the item is "
                + "read again. PropertyEditor/Update only writes the copy held in the open tab, "
                + "so without a following Tab/PersistChanges the new url never reaches the disk. "
                + editTrace.Describe()
        );
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

    [TearDown]
    public async Task RecordOutcomeThenDeleteWhatTheAgentCreated()
    {
        var result = TestContext.CurrentContext.Result;
        BenchmarkReport.RecordOutcome(
            TestContext.CurrentContext.Test.Name,
            result.Outcome.Status.ToString(),
            result.Message
        );

        if (createdEntityNames.Count == 0)
        {
            return;
        }

        await model.CloseAllTabsAsync();
        await DeleteItemsNamedAsync(createdEntityNames);
        createdEntityNames.Clear();
    }

    private async Task DeleteItemsNamedAsync(IReadOnlyList<string> itemNames)
    {
        foreach (var itemName in itemNames)
        {
            foreach (var itemId in await model.FindSchemaItemsAsync(itemName))
            {
                var status = await model.DeleteSchemaItemAsync(itemId);
                TestContext.Progress.WriteLine($"cleanup: {itemName} {itemId} -> {status}");
            }
        }
    }

    private async Task<string> RequireEntityAsync(string entityName)
    {
        var matches = await model.FindSchemaItemsAsync(entityName, itemTypeName: "Database Entity");
        Assert.That(
            matches,
            Is.Not.Empty,
            $"The agent reported success but the database entity '{entityName}' is not in the "
                + "model."
        );
        return matches[0];
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
        var builder = new ArchitectModelBuilder(agentClient.Architect);
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

    private async Task<SectionWidget> ReadSectionAsync(AgentRunTrace trace)
    {
        var section = await sectionProbe.ReadPersistedAsync(widgets.SectionId);
        TestContext.WriteLine("saved section:" + Environment.NewLine + section.Describe());
        Assert.That(
            section.Children,
            Is.Not.Empty,
            "The saved section has no widgets at all. " + trace.Describe()
        );
        return section;
    }

    private async Task<SectionWidget> RequireWidgetAsync(
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

    private async Task<AgentRunTrace> RunBenchmarkAsync(
        string prompt,
        object? focus,
        AgentConversation? conversation = null,
        string? testName = null
    )
    {
        if (!agentHealth.HasApiKey)
        {
            Assert.Ignore(
                "The Architect server reports no AI API key, so /agent/architect is not mapped."
            );
        }

        var trace = await agentClient.RunAsync(
            prompt,
            DefaultSections,
            CancellationToken.None,
            focus,
            conversation
        );
        await BenchmarkRecorder.RecordAsync(agentHealth, prompt, trace, testName);
        return trace;
    }

    private static string DescribeAll(SectionWidget section, AgentRunTrace trace)
    {
        return "Widgets: " + section.Describe() + Environment.NewLine + trace.Describe();
    }

    private static bool IsGuid(string value)
    {
        return Guid.TryParse(value, out var id) && id != Guid.Empty;
    }
}
