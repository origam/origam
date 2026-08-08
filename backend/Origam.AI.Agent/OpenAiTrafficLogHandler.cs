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
using System.Text;
using System.Text.Json;

namespace Origam.AI.Agent;

public class OpenAiTrafficLogHandler : DelegatingHandler
{
    private const int MessagePreview = 160;
    private const int ToolCallPreview = 500;
    private const int BodyPreview = 1000;

    private readonly ILogger<OpenAiTrafficLogHandler> logger;

    public OpenAiTrafficLogHandler(ILogger<OpenAiTrafficLogHandler> logger)
    {
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync();
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation(
                message: "--> OpenAI request\n{Details}",
                DescribeRequest(requestBody)
            );
        }

        var stopwatch = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        await response.Content.LoadIntoBufferAsync();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation(
            message: "<-- OpenAI response\n{Details}",
            DescribeResponse(response, responseBody, stopwatch.Elapsed)
        );

        return response;
    }

    private static string DescribeRequest(string body)
    {
        var builder = new StringBuilder();

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var hasMessages =
                root.TryGetProperty(propertyName: "messages", out var messages)
                && messages.ValueKind == JsonValueKind.Array;
            var toolCount = root.TryGetProperty(propertyName: "tools", out var tools)
                ? tools.GetArrayLength()
                : 0;
            var messageCount = hasMessages ? messages.GetArrayLength() : 0;

            builder.AppendLine(
                $"      {messageCount} messages, {body.Length} chars, {toolCount} tools"
            );

            if (!hasMessages)
            {
                return builder.ToString().TrimEnd();
            }

            var index = 0;
            foreach (var message in messages.EnumerateArray())
            {
                index++;
                var role = message.TryGetProperty(propertyName: "role", out var roleElement)
                    ? roleElement.GetString() ?? "?"
                    : "?";
                var content = ReadContent(message);
                builder.AppendLine(
                    $"      [{index, 2}] {role, -9} {content.Length, 6} | {Preview(content, MessagePreview)}"
                );
                AppendToolCalls(builder, message);
            }
        }
        catch (JsonException)
        {
            builder.AppendLine($"      unparsed body | {Preview(body, BodyPreview)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string DescribeResponse(
        HttpResponseMessage response,
        string body,
        TimeSpan elapsed
    )
    {
        var builder = new StringBuilder();
        var status = (int)response.StatusCode;

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var usage = string.Empty;
            if (root.TryGetProperty(propertyName: "usage", out var usageElement))
            {
                var cached = usageElement.TryGetProperty(
                    propertyName: "prompt_tokens_details",
                    out var promptDetails
                )
                    ? ReadNumber(promptDetails, name: "cached_tokens")
                    : 0;
                usage =
                    $", {ReadNumber(usageElement, name: "prompt_tokens")} prompt"
                    + $" ({cached} cached)"
                    + $" + {ReadNumber(usageElement, name: "completion_tokens")} completion tokens";
            }

            builder.AppendLine($"      {status} in {elapsed.TotalSeconds:0.##}s{usage}");

            if (
                !root.TryGetProperty(propertyName: "choices", out var choices)
                || choices.ValueKind != JsonValueKind.Array
            )
            {
                builder.AppendLine($"           body | {Preview(body, BodyPreview)}");
                return builder.ToString().TrimEnd();
            }

            foreach (var choice in choices.EnumerateArray())
            {
                if (!choice.TryGetProperty(propertyName: "message", out var message))
                {
                    continue;
                }

                var content = ReadContent(message);
                if (content.Length > 0)
                {
                    builder.AppendLine($"        content | {Preview(content, BodyPreview)}");
                }

                AppendToolCalls(builder, message);
            }
        }
        catch (JsonException)
        {
            builder.AppendLine($"      {status} in {elapsed.TotalSeconds:0.##}s");
            builder.AppendLine($"           body | {Preview(body, BodyPreview)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendToolCalls(StringBuilder builder, JsonElement message)
    {
        if (
            !message.TryGetProperty(propertyName: "tool_calls", out var toolCalls)
            || toolCalls.ValueKind != JsonValueKind.Array
        )
        {
            return;
        }

        foreach (var toolCall in toolCalls.EnumerateArray())
        {
            if (!toolCall.TryGetProperty(propertyName: "function", out var function))
            {
                continue;
            }

            var name = function.TryGetProperty(propertyName: "name", out var nameElement)
                ? nameElement.GetString() ?? "?"
                : "?";
            var arguments = function.TryGetProperty(
                propertyName: "arguments",
                out var argumentsElement
            )
                ? argumentsElement.GetString() ?? string.Empty
                : string.Empty;
            builder.AppendLine($"           call | {name}({Preview(arguments, ToolCallPreview)})");
        }
    }

    private static string ReadContent(JsonElement message)
    {
        if (!message.TryGetProperty(propertyName: "content", out var content))
        {
            return string.Empty;
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? string.Empty;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var parts = new StringBuilder();
        foreach (var part in content.EnumerateArray())
        {
            if (
                part.TryGetProperty(propertyName: "text", out var text)
                && text.ValueKind == JsonValueKind.String
            )
            {
                parts.Append(text.GetString());
            }
        }

        return parts.ToString();
    }

    private static int ReadNumber(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : 0;
    }

    private static string Preview(string value, int limit)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "(empty)";
        }

        var text = value.ReplaceLineEndings(" ").Trim();
        return text.Length <= limit ? text : text.Substring(startIndex: 0, length: limit) + " ...";
    }
}
