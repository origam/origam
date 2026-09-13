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
using System.Net;
using System.Text;

namespace Origam.AI.Agent.Tests.Infrastructure.Benchmark;

public static class BenchmarkHtmlReport
{
    private const string ProjectMarker = "Origam.AI.Agent.Tests.csproj";
    private const string ReportFileName = "benchmark-report.html";
    private const int MaxToolTextLength = 2000;

    private const string Styles = """
        :root { color-scheme: light dark; }
        * { box-sizing: border-box; }
        body { margin: 0; padding: 32px; font: 14px/1.5 -apple-system, "Segoe UI", Roboto, sans-serif;
               background: #f6f7f9; color: #1b1f23; }
        h1 { margin: 0 0 4px; font-size: 20px; }
        .meta { color: #6a737d; font-size: 13px; margin-bottom: 20px; }
        .meta a { color: inherit; }
        .cards { display: flex; flex-wrap: wrap; gap: 12px; margin-bottom: 24px; }
        .card { background: #fff; border: 1px solid #e1e4e8; border-radius: 8px; padding: 12px 16px;
                min-width: 110px; }
        .card .value { display: block; font-size: 20px; font-weight: 600; }
        .card .label { color: #6a737d; font-size: 12px; text-transform: uppercase;
                       letter-spacing: .04em; }
        .card.failed .value { color: #cb2431; }
        .card.passed .value { color: #22863a; }
        table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e1e4e8;
                border-radius: 8px; overflow: hidden; }
        th, td { padding: 10px 12px; text-align: right; border-bottom: 1px solid #eaecef; }
        th:first-child, td:first-child { text-align: left; }
        th { background: #f6f8fa; font-size: 12px; text-transform: uppercase; color: #6a737d;
             letter-spacing: .04em; }
        tr:last-child td { border-bottom: none; }
        tr.total td { font-weight: 600; background: #f6f8fa; }
        .badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 12px;
                 font-weight: 600; }
        .badge.passed { background: #dcffe4; color: #22863a; }
        .badge.failed { background: #ffeef0; color: #cb2431; }
        .badge.other { background: #f1f8ff; color: #0366d6; }
        td.detail { padding: 0; }
        details { border-top: 1px solid #eaecef; }
        details > summary { cursor: pointer; padding: 8px 12px; color: #0366d6; font-size: 13px; }
        .turn { padding: 0 12px 12px 28px; }
        .label-row { font-size: 12px; text-transform: uppercase; letter-spacing: .04em;
                     color: #6a737d; margin: 12px 0 4px; }
        pre { margin: 0; padding: 10px 12px; background: #f6f8fa; border: 1px solid #eaecef;
              border-radius: 6px; overflow-x: auto; white-space: pre-wrap; word-break: break-word;
              font: 12px/1.45 ui-monospace, Consolas, monospace; }
        pre.failure { background: #ffeef0; border-color: #f9d0d5; color: #86181d; }
        ol.tools { margin: 0; padding-left: 20px; }
        ol.tools li { margin-bottom: 10px; }
        ol.tools code { font-weight: 600; }
        @media (prefers-color-scheme: dark) {
          body { background: #0d1117; color: #c9d1d9; }
          .card, table { background: #161b22; border-color: #30363d; }
          th { background: #161b22; color: #8b949e; }
          th, td, details { border-color: #21262d; }
          tr.total td { background: #12171d; }
          pre { background: #0d1117; border-color: #30363d; }
          pre.failure { background: #2d1114; border-color: #6b2b31; color: #ffa198; }
          .meta, .card .label, .label-row { color: #8b949e; }
          .badge.passed { background: #12261a; color: #56d364; }
          .badge.failed { background: #2d1114; color: #ff7b72; }
          .badge.other { background: #10202f; color: #58a6ff; }
        }
        """;

    public static string Write(
        IReadOnlyList<BenchmarkRow> rows,
        IReadOnlyDictionary<string, BenchmarkOutcome> outcomes,
        string? backend
    )
    {
        string reportPath = Path.Combine(ResolveProjectDirectory(), ReportFileName);
        File.WriteAllText(reportPath, Render(rows, outcomes, backend), Encoding.UTF8);
        return reportPath;
    }

    private static string ResolveProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, ProjectMarker)))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        return AppContext.BaseDirectory;
    }

    private static string Render(
        IReadOnlyList<BenchmarkRow> rows,
        IReadOnlyDictionary<string, BenchmarkOutcome> outcomes,
        string? backend
    )
    {
        var testNames = rows.Select(row => row.TestName).Distinct(StringComparer.Ordinal).ToList();
        var failedCount = testNames.Count(name => StatusOf(outcomes, name) == "Failed");

        var html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\">");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine("<title>ORIGAM AI agent benchmark</title>");
        html.AppendLine($"<style>{Styles}</style></head><body>");

        html.AppendLine("<h1>ORIGAM AI agent benchmark</h1>");
        html.AppendLine(
            "<div class=\"meta\">"
                + Encode(rows[0].Model)
                + " &middot; "
                + Encode(backend ?? "unknown backend")
                + " &middot; "
                + Encode(
                    DateTime.Now.ToString(format: "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                )
                + " &middot; prices <a href=\""
                + Encode(LiteLlmPricing.CatalogueUrl)
                + "\">LiteLLM catalogue</a></div>"
        );

        AppendCards(html, rows, testNames.Count, failedCount);

        html.AppendLine("<table><thead><tr>");
        html.AppendLine(
            "<th>test</th><th>status</th><th>turns</th><th>tools</th><th>prompt</th>"
                + "<th>cached</th><th>output</th><th>sec</th><th>USD</th>"
        );
        html.AppendLine("</tr></thead><tbody>");

        foreach (var testName in testNames)
        {
            var testRows = rows.Where(row => row.TestName == testName).ToList();
            AppendTestRow(html, testName, testRows, StatusOf(outcomes, testName));
            AppendDetailRow(html, testRows, outcomes.GetValueOrDefault(testName));
        }

        AppendTotalRow(html, rows);
        html.AppendLine("</tbody></table></body></html>");
        return html.ToString();
    }

    private static void AppendCards(
        StringBuilder html,
        IReadOnlyList<BenchmarkRow> rows,
        int testCount,
        int failedCount
    )
    {
        html.AppendLine("<div class=\"cards\">");
        AppendCard(
            html,
            testCount.ToString(CultureInfo.InvariantCulture),
            label: "tests",
            cssClass: null
        );
        AppendCard(
            html,
            (testCount - failedCount).ToString(CultureInfo.InvariantCulture),
            label: "passed",
            cssClass: "passed"
        );
        AppendCard(
            html,
            failedCount.ToString(CultureInfo.InvariantCulture),
            label: "failed",
            cssClass: failedCount > 0 ? "failed" : null
        );
        AppendCard(
            html,
            rows.Sum(row => row.ToolCalls.Count).ToString(CultureInfo.InvariantCulture),
            label: "tool calls",
            cssClass: null
        );
        AppendCard(
            html,
            rows.Sum(row => row.Usage.TotalTokens)
                .ToString(format: "N0", CultureInfo.InvariantCulture),
            label: "tokens",
            cssClass: null
        );
        AppendCard(
            html,
            rows.Sum(row => row.Duration.TotalSeconds)
                .ToString(format: "F1", CultureInfo.InvariantCulture) + " s",
            label: "duration",
            cssClass: null
        );
        AppendCard(
            html,
            "$" + FormatCost(rows.Sum(row => row.CostUsd ?? 0m)),
            label: "cost",
            cssClass: null
        );
        html.AppendLine("</div>");
    }

    private static void AppendCard(StringBuilder html, string value, string label, string? cssClass)
    {
        html.AppendLine(
            $"<div class=\"card{(cssClass is null ? "" : " " + cssClass)}\">"
                + $"<span class=\"value\">{Encode(value)}</span>"
                + $"<span class=\"label\">{Encode(label)}</span></div>"
        );
    }

    private static void AppendTestRow(
        StringBuilder html,
        string testName,
        IReadOnlyList<BenchmarkRow> testRows,
        string status
    )
    {
        html.AppendLine("<tr>");
        html.AppendLine($"<td>{Encode(testName)}</td>");
        html.AppendLine(
            $"<td><span class=\"badge {BadgeClass(status)}\">{Encode(status)}</span></td>"
        );
        html.AppendLine($"<td>{testRows.Count}</td>");
        html.AppendLine($"<td>{testRows.Sum(row => row.ToolCalls.Count)}</td>");
        html.AppendLine($"<td>{FormatNumber(testRows.Sum(row => row.Usage.PromptTokens))}</td>");
        html.AppendLine($"<td>{FormatNumber(testRows.Sum(row => row.Usage.CachedTokens))}</td>");
        html.AppendLine(
            $"<td>{FormatNumber(testRows.Sum(row => row.Usage.CompletionTokens))}</td>"
        );
        html.AppendLine(
            $"<td>{testRows.Sum(row => row.Duration.TotalSeconds).ToString(format: "F1", CultureInfo.InvariantCulture)}</td>"
        );
        html.AppendLine($"<td>{FormatCost(testRows.Sum(row => row.CostUsd ?? 0m))}</td>");
        html.AppendLine("</tr>");
    }

    private static void AppendDetailRow(
        StringBuilder html,
        IReadOnlyList<BenchmarkRow> testRows,
        BenchmarkOutcome? outcome
    )
    {
        html.AppendLine("<tr><td class=\"detail\" colspan=\"9\">");
        for (int index = 0; index < testRows.Count; index++)
        {
            var row = testRows[index];
            html.AppendLine("<details><summary>");
            html.AppendLine(
                Encode(
                    $"turn {index + 1} · {row.ToolCalls.Count} tool calls · "
                        + $"{row.Duration.TotalSeconds.ToString(format: "F1", CultureInfo.InvariantCulture)} s · "
                        + $"${FormatCost(row.CostUsd)}"
                )
            );
            html.AppendLine("</summary><div class=\"turn\">");

            html.AppendLine("<div class=\"label-row\">prompt</div>");
            html.AppendLine($"<pre>{Encode(row.Prompt)}</pre>");

            if (row.ToolCalls.Count > 0)
            {
                html.AppendLine("<div class=\"label-row\">tool calls</div><ol class=\"tools\">");
                foreach (var toolCall in row.ToolCalls)
                {
                    html.AppendLine($"<li><code>{Encode(toolCall.Name)}</code>");
                    html.AppendLine($"<pre>{Encode(Shorten(toolCall.Arguments))}</pre>");
                    html.AppendLine(
                        $"<pre>{Encode(Shorten(toolCall.Result ?? "(no result)"))}</pre></li>"
                    );
                }
                html.AppendLine("</ol>");
            }

            if (row.ReplyText.Trim().Length > 0)
            {
                html.AppendLine("<div class=\"label-row\">reply</div>");
                html.AppendLine($"<pre>{Encode(row.ReplyText)}</pre>");
            }

            html.AppendLine("</div></details>");
        }

        if (outcome?.Message is { Length: > 0 } failureMessage)
        {
            html.AppendLine("<details open><summary>failure</summary><div class=\"turn\">");
            html.AppendLine($"<pre class=\"failure\">{Encode(failureMessage)}</pre>");
            html.AppendLine("</div></details>");
        }

        html.AppendLine("</td></tr>");
    }

    private static void AppendTotalRow(StringBuilder html, IReadOnlyList<BenchmarkRow> rows)
    {
        html.AppendLine("<tr class=\"total\">");
        html.AppendLine("<td>TOTAL</td><td></td>");
        html.AppendLine($"<td>{rows.Count}</td>");
        html.AppendLine($"<td>{rows.Sum(row => row.ToolCalls.Count)}</td>");
        html.AppendLine($"<td>{FormatNumber(rows.Sum(row => row.Usage.PromptTokens))}</td>");
        html.AppendLine($"<td>{FormatNumber(rows.Sum(row => row.Usage.CachedTokens))}</td>");
        html.AppendLine($"<td>{FormatNumber(rows.Sum(row => row.Usage.CompletionTokens))}</td>");
        html.AppendLine(
            $"<td>{rows.Sum(row => row.Duration.TotalSeconds).ToString(format: "F1", CultureInfo.InvariantCulture)}</td>"
        );
        html.AppendLine($"<td>{FormatCost(rows.Sum(row => row.CostUsd ?? 0m))}</td>");
        html.AppendLine("</tr>");
    }

    private static string StatusOf(
        IReadOnlyDictionary<string, BenchmarkOutcome> outcomes,
        string testName
    )
    {
        return outcomes.GetValueOrDefault(testName)?.Status ?? "Unknown";
    }

    private static string BadgeClass(string status)
    {
        return status switch
        {
            "Passed" => "passed",
            "Failed" => "failed",
            _ => "other",
        };
    }

    private static string FormatNumber(int value)
    {
        return value.ToString(format: "N0", CultureInfo.InvariantCulture);
    }

    private static string FormatCost(decimal? cost)
    {
        return cost is null
            ? "n/a"
            : cost.Value.ToString(format: "F6", CultureInfo.InvariantCulture);
    }

    private static string Shorten(string text)
    {
        return text.Length <= MaxToolTextLength
            ? text
            : text[..MaxToolTextLength] + $"… (+{text.Length - MaxToolTextLength} chars)";
    }

    private static string Encode(string text)
    {
        return WebUtility.HtmlEncode(text);
    }
}
