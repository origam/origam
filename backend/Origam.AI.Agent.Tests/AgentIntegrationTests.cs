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
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure;

namespace Origam.AI.Agent.Tests;

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
    private const string AncestorsNodeText = "_Ancestors";

    private static readonly string[] DefaultSections = [];
    private static readonly string[] ExpandedTreePath = ["Data", "Entities", "Dimensions"];

    private readonly List<string> createdEntityNames = [];

    private ArchitectAgentClient agentClient = null!;
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

        var toolNames = string.Join(separator: ", ", trace.ToolNames);
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            UsedTool(trace, toolNameFragment: "CreateNode"),
            Is.True,
            "CreateNode was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );
        Assert.That(trace.Result.AffectedNodes, Is.Not.Empty);

        var matches = await FindSchemaItemsAsync(entityName);
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

        var toolNames = string.Join(separator: ", ", trace.ToolNames);
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
            foreach (var field in await ReadDatabaseFieldsAsync(childEntityId))
            {
                if (await PointsAtEntityAsync(field.Id, mainEntityId))
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
        var fieldNames = (await ReadDatabaseFieldsAsync(entityId)).Select(field => field.Name);
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
            await FindSchemaItemsAsync(entityName, itemTypeName: "Database Entity"),
            Is.Empty,
            $"The agent reported the deletion but '{entityName}' ({entityId}) is still in the "
                + "model. "
                + deleteTrace.Describe()
        );
    }

    [TearDown]
    public void RecordTestOutcome()
    {
        var result = TestContext.CurrentContext.Result;
        BenchmarkReport.RecordOutcome(
            TestContext.CurrentContext.Test.Name,
            result.Outcome.Status.ToString(),
            result.Message
        );
    }

    [TearDown]
    public async Task DeleteEntitiesCreatedByTests()
    {
        if (createdEntityNames.Count == 0)
        {
            return;
        }

        foreach (var entityName in createdEntityNames)
        {
            foreach (var itemId in await FindSchemaItemsAsync(entityName))
            {
                using var response = await agentClient.Architect.PostAsJsonAsync(
                    requestUri: "/Model/DeleteSchemaItem",
                    new { schemaItemId = itemId },
                    CancellationToken.None
                );
                TestContext.WriteLine(
                    $"cleanup: {entityName} {itemId} -> {(int)response.StatusCode}"
                );
            }
        }
        createdEntityNames.Clear();
    }

    private static bool UsedTool(AgentRunTrace trace, string toolNameFragment)
    {
        return trace.ToolNames.Any(name =>
            name.Contains(
                value: toolNameFragment,
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        );
    }

    private async Task<IReadOnlyList<string>> FindSchemaItemsAsync(
        string exactName,
        string? itemTypeName = null
    )
    {
        var body = await agentClient.Architect.GetStringAsync(
            requestUri: "/Model/SearchSchema?query=" + Uri.EscapeDataString(exactName),
            CancellationToken.None
        );

        using var document = JsonDocument.Parse(body);
        return document
            .RootElement.EnumerateArray()
            .Where(node =>
                node.TryGetProperty(propertyName: "nodeText", out var text)
                && text.GetString() == exactName
                && (
                    itemTypeName is null
                    || (
                        node.TryGetProperty(propertyName: "itemTypeName", out var type)
                        && type.GetString() == itemTypeName
                    )
                )
            )
            .Select(node => node.GetProperty("origamId").GetString())
            .OfType<string>()
            .ToList();
    }

    private async Task<string> RequireEntityAsync(string entityName)
    {
        var matches = await FindSchemaItemsAsync(entityName, itemTypeName: "Database Entity");
        Assert.That(
            matches,
            Is.Not.Empty,
            $"The agent reported success but the database entity '{entityName}' is not in the "
                + "model."
        );
        return matches[0];
    }

    private async Task<IReadOnlyList<DatabaseField>> ReadDatabaseFieldsAsync(string entityId)
    {
        var body = await agentClient.Architect.GetStringAsync(
            requestUri: $"/Model/GetSchemaNodeDetails?id={Uri.EscapeDataString(entityId)}&depth=3",
            CancellationToken.None
        );

        using var document = JsonDocument.Parse(body);
        var fields = new List<DatabaseField>();
        CollectDatabaseFields(document.RootElement, fields);
        return fields;
    }

    private static void CollectDatabaseFields(JsonElement node, List<DatabaseField> fields)
    {
        if (node.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var nodeText = node.TryGetProperty(propertyName: "nodeText", out var text)
            ? text.GetString()
            : null;
        if (nodeText == AncestorsNodeText)
        {
            return;
        }

        if (
            node.TryGetProperty(propertyName: "itemTypeName", out var itemTypeName)
            && itemTypeName.GetString() == "Database Field"
            && node.TryGetProperty(propertyName: "origamId", out var origamId)
            && origamId.GetString() is { } fieldId
        )
        {
            fields.Add(new DatabaseField(fieldId, nodeText ?? string.Empty));
        }

        if (
            node.TryGetProperty(propertyName: "children", out var children)
            && children.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var child in children.EnumerateArray())
            {
                CollectDatabaseFields(child, fields);
            }
        }
    }

    private async Task<bool> PointsAtEntityAsync(string fieldId, string targetEntityId)
    {
        using var response = await agentClient.Architect.PostAsJsonAsync(
            requestUri: "/Tab/Open",
            new { schemaItemId = fieldId },
            CancellationToken.None
        );
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        if (
            !document.RootElement.TryGetProperty(propertyName: "data", out var properties)
            || properties.ValueKind != JsonValueKind.Array
        )
        {
            return false;
        }

        return ReadPropertyValue(properties, propertyName: "ForeignKeyEntity")
                .Contains(targetEntityId, StringComparison.OrdinalIgnoreCase)
            && ReadPropertyValue(properties, propertyName: "ForeignKeyField").Length > 0;
    }

    private sealed record DatabaseField(string Id, string Name);

    private static string ReadPropertyValue(JsonElement properties, string propertyName)
    {
        foreach (var property in properties.EnumerateArray())
        {
            if (
                property.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() == propertyName
                && property.TryGetProperty(propertyName: "value", out var value)
            )
            {
                return value.ValueKind == JsonValueKind.Null ? string.Empty : value.ToString();
            }
        }

        return string.Empty;
    }

    private async Task<AgentRunTrace> RunBenchmarkAsync(
        string prompt,
        IReadOnlyList<string> enabledSections,
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
