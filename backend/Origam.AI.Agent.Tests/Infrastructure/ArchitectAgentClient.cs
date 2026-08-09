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

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Origam.AI.Agent.Tests.Infrastructure;

public sealed class ArchitectAgentClient : IDisposable
{
    private const string HealthPath = "/agent/health";
    private const string RunPath = "/agent/architect";
    private const string DataPrefix = "data:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;
    private readonly bool ownsHttpClient;

    public ArchitectAgentClient(string baseUrl)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };
        httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(10),
        };
        ownsHttpClient = true;
    }

    public ArchitectAgentClient(HttpClient existingClient)
    {
        httpClient = existingClient;
        ownsHttpClient = false;
    }

    public HttpClient Architect => httpClient;

    public async Task<AgentHealth?> TryGetHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(HealthPath, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<AgentHealth>(body, JsonOptions);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public async Task<AgentRunTrace> RunAsync(
        string prompt,
        IReadOnlyList<string> enabledSections,
        CancellationToken cancellationToken,
        object? focus = null,
        AgentConversation? conversation = null
    )
    {
        using var request = BuildRequest(prompt, enabledSections, focus, conversation);
        var stopwatch = Stopwatch.StartNew();

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();

        var toolCallOrder = new List<string>();
        var toolCallNames = new Dictionary<string, string>(StringComparer.Ordinal);
        var toolCallArguments = new Dictionary<string, StringBuilder>(StringComparer.Ordinal);
        var toolCallResults = new Dictionary<string, string>(StringComparer.Ordinal);
        var replyText = new StringBuilder();
        RunResult? runResult = null;
        string? errorMessage = null;

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using (stream.ConfigureAwait(false))
        {
            using var reader = new StreamReader(stream);
            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (!line.StartsWith(value: DataPrefix, comparisonType: StringComparison.Ordinal))
                {
                    continue;
                }

                var json = line[DataPrefix.Length..].Trim();
                if (json.Length == 0)
                {
                    continue;
                }

                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                switch (ReadString(root, propertyName: "type"))
                {
                    case "TOOL_CALL_START":
                    {
                        var toolCallId = ReadString(root, propertyName: "toolCallId") ?? "";
                        toolCallOrder.Add(toolCallId);
                        toolCallNames[toolCallId] =
                            ReadString(root, propertyName: "toolCallName") ?? "";
                        toolCallArguments[toolCallId] = new StringBuilder();
                        break;
                    }
                    case "TOOL_CALL_ARGS":
                    {
                        var toolCallId = ReadString(root, propertyName: "toolCallId") ?? "";
                        if (toolCallArguments.TryGetValue(toolCallId, out var arguments))
                        {
                            arguments.Append(ReadString(root, propertyName: "delta"));
                        }
                        break;
                    }
                    case "TOOL_CALL_RESULT":
                    {
                        var toolCallId = ReadString(root, propertyName: "toolCallId") ?? "";
                        toolCallResults[toolCallId] =
                            ReadString(root, propertyName: "content") ?? "";
                        break;
                    }
                    case "TEXT_MESSAGE_CONTENT":
                    {
                        replyText.Append(ReadString(root, propertyName: "delta"));
                        break;
                    }
                    case "CUSTOM":
                    {
                        if (ReadString(root, propertyName: "name") == AguiEvents.RunResultName)
                        {
                            runResult = root.GetProperty("value")
                                .Deserialize<RunResult>(JsonOptions);
                        }
                        break;
                    }
                    case "RUN_ERROR":
                    {
                        errorMessage = ReadString(root, propertyName: "message") ?? "unknown";
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }

        var toolCalls = toolCallOrder
            .Select(toolCallId => new ToolInvocation(
                toolCallNames.GetValueOrDefault(toolCallId, defaultValue: ""),
                toolCallArguments.GetValueOrDefault(toolCallId)?.ToString() ?? "",
                toolCallResults.GetValueOrDefault(toolCallId)
            ))
            .ToList();

        conversation?.Append(prompt, replyText.ToString());

        return new AgentRunTrace(
            toolCalls,
            replyText.ToString(),
            runResult,
            errorMessage,
            stopwatch.Elapsed
        );
    }

    public void Dispose()
    {
        if (ownsHttpClient)
        {
            httpClient.Dispose();
        }
    }

    private static HttpRequestMessage BuildRequest(
        string prompt,
        IReadOnlyList<string> enabledSections,
        object? focus,
        AgentConversation? conversation
    )
    {
        var messages = new List<AgentMessage>(conversation?.Messages ?? [])
        {
            new(Guid.NewGuid().ToString(), Role: "user", prompt),
        };

        var payload = new
        {
            threadId = conversation?.ThreadId ?? Guid.NewGuid().ToString(),
            runId = Guid.NewGuid().ToString(),
            state = new { focus },
            messages,
            tools = Array.Empty<object>(),
            context = Array.Empty<object>(),
            forwardedProps = new { enabledSections },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, RunPath)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                mediaType: "application/json"
            ),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        return request;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
    }
}
