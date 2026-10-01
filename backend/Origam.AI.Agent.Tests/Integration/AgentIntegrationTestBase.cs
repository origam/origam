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

public abstract class AgentIntegrationTestBase
{
    public const string ExplicitReason =
        "Calls a live LLM and costs money. Boots Architect in-process; needs the ORIGAM model on "
        + "disk and an AI API key in appsettings.Development.json.";
    public const string IntegrationCategory = "AiIntegration";
    protected const string MutatingCategory = "AiMutating";

    private const string ArchitectUrlVariable = "ORIGAM_ARCHITECT_URL";
    private static readonly string[]? DefaultSections = null;
    private static readonly string[] ExpandedTreePath = ["Data", "Entities", "Dimensions"];

    private AgentHealth agentHealth = null!;

    protected List<string> CreatedEntityNames { get; } = [];
    protected ArchitectAgentClient AgentClient { get; private set; } = null!;
    protected ArchitectModelProbe Model { get; private set; } = null!;
    protected ChatFocusPayload ChatFocus { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task ConnectToArchitect()
    {
        var liveUrl = Environment.GetEnvironmentVariable(ArchitectUrlVariable);
        string backend;
        if (string.IsNullOrWhiteSpace(liveUrl))
        {
            try
            {
                AgentClient = new ArchitectAgentClient(
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
            AgentClient = new ArchitectAgentClient(liveUrl);
            backend = $"live Architect at {liveUrl}";
        }

        var health = await AgentClient.TryGetHealthAsync(CancellationToken.None);
        if (health is null)
        {
            Assert.Ignore($"No agent endpoint answering on the {backend}.");
        }

        agentHealth = health!;
        Model = new ArchitectModelProbe(AgentClient.Architect);
        ChatFocus = await ChatFocusFactory.FromExpandedPathAsync(
            AgentClient.Architect,
            ExpandedTreePath,
            CancellationToken.None
        );
        BenchmarkReport.Backend = backend;
        TestContext.Progress.WriteLine(
            $"{backend} | {agentHealth.Model} @ {agentHealth.Endpoint} "
                + $"(key configured: {agentHealth.HasApiKey}) | focus: {ChatFocus.Describe()}"
        );
    }

    [OneTimeTearDown]
    public void Disconnect()
    {
        AgentClient?.Dispose();
    }

    [SetUp]
    public async Task DropTabsLeftOpenByThePreviousTest()
    {
        await Model.CloseAllTabsAsync();
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

        if (CreatedEntityNames.Count == 0)
        {
            return;
        }

        await Model.CloseAllTabsAsync();
        await DeleteItemsNamedAsync(CreatedEntityNames);
        CreatedEntityNames.Clear();
    }

    protected async Task DeleteItemsNamedAsync(IReadOnlyList<string> itemNames)
    {
        foreach (var itemName in itemNames)
        {
            foreach (var itemId in await Model.FindSchemaItemsAsync(itemName))
            {
                var status = await Model.DeleteSchemaItemAsync(itemId);
                TestContext.Progress.WriteLine($"cleanup: {itemName} {itemId} -> {status}");
            }
        }
    }

    protected async Task<AgentRunTrace> RunBenchmarkAsync(
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

        var trace = await AgentClient.RunAsync(
            prompt,
            DefaultSections,
            CancellationToken.None,
            focus,
            conversation
        );
        await BenchmarkRecorder.RecordAsync(agentHealth, prompt, trace, testName);
        return trace;
    }
}
