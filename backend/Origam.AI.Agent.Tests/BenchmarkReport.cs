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
using System.Text;
using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure;

namespace Origam.AI.Agent.Tests;

public sealed record BenchmarkRow(
    string TestName,
    string Model,
    string Prompt,
    IReadOnlyList<Infrastructure.ToolInvocation> ToolCalls,
    string ReplyText,
    RunUsage Usage,
    TimeSpan Duration,
    decimal? CostUsd
);

[SetUpFixture]
public sealed class BenchmarkReport
{
    private const int NameColumnWidth = 44;

    private static readonly List<BenchmarkRow> CollectedRows = [];
    private static readonly Dictionary<string, BenchmarkOutcome> CollectedOutcomes = new(
        StringComparer.Ordinal
    );

    public static string? Backend { get; set; }

    public static void Record(BenchmarkRow row)
    {
        lock (CollectedRows)
        {
            CollectedRows.Add(row);
        }
    }

    public static void RecordOutcome(string testName, string status, string? message)
    {
        lock (CollectedRows)
        {
            CollectedOutcomes[testName] = new BenchmarkOutcome(status, message);
        }
    }

    [OneTimeTearDown]
    public void CompleteRun()
    {
        lock (CollectedRows)
        {
            if (CollectedRows.Count > 0)
            {
                TestContext.Progress.WriteLine(Render(CollectedRows));
                TestContext.Progress.WriteLine(
                    "html report: "
                        + BenchmarkHtmlReport.Write(CollectedRows, CollectedOutcomes, Backend)
                );
                CollectedRows.Clear();
                CollectedOutcomes.Clear();
            }
        }
        InProcessArchitect.Shutdown();
    }

    private static string Render(IReadOnlyList<BenchmarkRow> rows)
    {
        var separator = new string(c: '-', count: NameColumnWidth + 46);
        var report = new StringBuilder();
        report.AppendLine();
        report.AppendLine("ORIGAM AI AGENT BENCHMARK");
        report.AppendLine($"model:   {rows[0].Model}");
        report.AppendLine($"backend: {Backend ?? "unknown"}");
        report.AppendLine($"prices:  {LiteLlmPricing.CatalogueUrl}");
        report.AppendLine(separator);
        report.AppendLine(
            "test".PadRight(NameColumnWidth)
                + "tools".PadLeft(6)
                + "prompt".PadLeft(9)
                + "cached".PadLeft(8)
                + "output".PadLeft(8)
                + "sec".PadLeft(7)
                + "USD".PadLeft(11)
        );
        report.AppendLine(separator);

        foreach (var row in rows)
        {
            report.AppendLine(
                Shorten(row.TestName).PadRight(NameColumnWidth)
                    + row.ToolCalls.Count.ToString(CultureInfo.InvariantCulture).PadLeft(6)
                    + row.Usage.PromptTokens.ToString(CultureInfo.InvariantCulture).PadLeft(9)
                    + row.Usage.CachedTokens.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                    + row.Usage.CompletionTokens.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                    + row.Duration.TotalSeconds.ToString(format: "F1", CultureInfo.InvariantCulture)
                        .PadLeft(7)
                    + FormatCost(row.CostUsd).PadLeft(11)
            );
        }

        report.AppendLine(separator);
        var totalCost = rows.Sum(row => row.CostUsd ?? 0m);
        report.AppendLine(
            "TOTAL".PadRight(NameColumnWidth)
                + rows.Sum(row => row.ToolCalls.Count)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(6)
                + rows.Sum(row => row.Usage.PromptTokens)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(9)
                + rows.Sum(row => row.Usage.CachedTokens)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(8)
                + rows.Sum(row => row.Usage.CompletionTokens)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(8)
                + rows.Sum(row => row.Duration.TotalSeconds)
                    .ToString(format: "F1", CultureInfo.InvariantCulture)
                    .PadLeft(7)
                + FormatCost(totalCost).PadLeft(11)
        );
        return report.ToString();
    }

    private static string FormatCost(decimal? cost)
    {
        return cost is null
            ? "n/a"
            : cost.Value.ToString(format: "F6", CultureInfo.InvariantCulture);
    }

    private static string Shorten(string testName)
    {
        return testName.Length <= NameColumnWidth - 1
            ? testName
            : testName[..(NameColumnWidth - 2)] + "…";
    }
}
