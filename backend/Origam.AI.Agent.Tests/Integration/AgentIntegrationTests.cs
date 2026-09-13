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

using System.Globalization;
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
    private static readonly string[]? DefaultSections = null;
    private static readonly string[] ExpandedTreePath = ["Data", "Entities", "Dimensions"];

    private readonly List<string> createdEntityNames = [];

    private ArchitectAgentClient agentClient = null!;
    private ArchitectModelProbe model = null!;
    private AgentHealth agentHealth = null!;
    private ChatFocusPayload chatFocus = null!;

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
        chatFocus = await ChatFocusFactory.FromExpandedPathAsync(
            agentClient.Architect,
            ExpandedTreePath,
            CancellationToken.None
        );
        BenchmarkReport.Backend = backend;
        TestContext.Progress.WriteLine(
            $"{backend} | {agentHealth.Model} @ {agentHealth.Endpoint} "
                + $"(key configured: {agentHealth.HasApiKey})"
        );
    }

    [OneTimeTearDown]
    public void DisconnectFromArchitectServer()
    {
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
            DefaultSections
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
            DefaultSections
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
            DefaultSections,
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
            DefaultSections,
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
            DefaultSections
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
            DefaultSections
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
            DefaultSections,
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
            DefaultSections,
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

        foreach (var entityName in createdEntityNames)
        {
            foreach (var itemId in await model.FindSchemaItemsAsync(entityName))
            {
                var status = await model.DeleteSchemaItemAsync(itemId);
                TestContext.WriteLine($"cleanup: {entityName} {itemId} -> {status}");
            }
        }
        createdEntityNames.Clear();
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

    private async Task<AgentRunTrace> RunBenchmarkAsync(
        string prompt,
        IReadOnlyList<string>? enabledSections,
        AgentConversation? conversation = null
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
            enabledSections,
            CancellationToken.None,
            chatFocus,
            conversation
        );

        TestContext.Progress.WriteLine($"focus: {chatFocus.Describe()}");
        TestContext.Progress.WriteLine($"prompt: {prompt}");
        TestContext.Progress.WriteLine(trace.Describe());

        var price = await LiteLlmPricing.TryGetPriceAsync(
            agentHealth.Model,
            CancellationToken.None
        );
        var cost = price is null ? null : (decimal?)LiteLlmPricing.EstimateCost(price, trace.Usage);

        BenchmarkReport.Record(
            new BenchmarkRow(
                TestContext.CurrentContext.Test.Name,
                agentHealth.Model,
                prompt,
                trace.ToolCalls,
                trace.ReplyText,
                trace.Usage,
                trace.Duration,
                cost
            )
        );

        Assert.That(
            trace.ToolCalls.Count > 0 || trace.Usage.TotalTokens > 0,
            Is.True,
            "The agent produced an empty stream: no tool calls, no text and no token usage, "
                + "and no RUN_ERROR either. This usually means the upstream call was throttled "
                + "or dropped and the failure was swallowed instead of reported."
        );

        var costText = cost is null
            ? "n/a"
            : cost.Value.ToString(format: "F6", CultureInfo.InvariantCulture);
        TestContext.Progress.WriteLine(
            $"tokens: prompt={trace.Usage.PromptTokens} cached={trace.Usage.CachedTokens} "
                + $"output={trace.Usage.CompletionTokens} | "
                + $"{trace.Duration.TotalSeconds.ToString(format: "F1", CultureInfo.InvariantCulture)}s | "
                + $"cost={costText} USD"
        );
        return trace;
    }
}
