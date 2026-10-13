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
using Origam.AI.Agent.Tests.Integration;

namespace Origam.AI.Agent.Tests.Infrastructure.Benchmark;

public static class BenchmarkRecorder
{
    public static async Task RecordAsync(
        AgentHealth agentHealth,
        string prompt,
        AgentRunTrace trace,
        string? testName = null
    )
    {
        TestContext.Progress.WriteLine($"prompt: {prompt}");
        TestContext.Progress.WriteLine(trace.Describe());

        var price = await LiteLlmPricing.TryGetPriceAsync(
            agentHealth.Model,
            CancellationToken.None
        );
        var cost = price is null ? null : (decimal?)LiteLlmPricing.EstimateCost(price, trace.Usage);

        BenchmarkReport.Record(
            new BenchmarkRow(
                testName ?? TestContext.CurrentContext.Test.Name,
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
    }
}
