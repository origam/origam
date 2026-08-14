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

using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Origam.AI.Agent;

public sealed class ScriptedChatClient : IChatClient
{
    private const int TextChunkLength = 12;

    private readonly AiScriptStore scriptStore;
    private readonly IChatClient? liveChatClient;

    public ScriptedChatClient(AiScriptStore scriptStore, IChatClient? liveChatClient)
    {
        this.scriptStore = scriptStore;
        this.liveChatClient = liveChatClient;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        if (scriptStore.Script is null)
        {
            return await RequireLiveChatClient()
                .GetResponseAsync(messages, options, cancellationToken);
        }

        var updates = new List<ChatResponseUpdate>();
        await foreach (
            var update in GetStreamingResponseAsync(messages, options, cancellationToken)
        )
        {
            updates.Add(update);
        }
        return updates.ToChatResponse();
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var script = scriptStore.Script;
        if (script is null)
        {
            await foreach (
                var update in RequireLiveChatClient()
                    .GetStreamingResponseAsync(messages, options, cancellationToken)
            )
            {
                yield return update;
            }
            yield break;
        }

        var step = SelectStep(script, messages);

        if (step.DelayMs > 0)
        {
            await Task.Delay(step.DelayMs, cancellationToken);
        }

        if (step.Error is not null)
        {
            throw new InvalidOperationException(step.Error);
        }

        var messageId = Guid.NewGuid().ToString(format: "N");
        var toolCalls = step.ToolCalls ?? Array.Empty<ScriptedToolCall>();

        foreach (var toolCall in toolCalls)
        {
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                MessageId = messageId,
                Contents =
                [
                    new FunctionCallContent(
                        Guid.NewGuid().ToString(format: "N"),
                        toolCall.Name,
                        ReadArguments(toolCall.Arguments)
                    ),
                ],
            };
        }

        foreach (var chunk in SplitIntoChunks(step.Text))
        {
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                MessageId = messageId,
                Contents = [new TextContent(chunk)],
            };
        }

        if (toolCalls.Count == 0)
        {
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                MessageId = messageId,
                Contents =
                [
                    new UsageContent(
                        new UsageDetails
                        {
                            InputTokenCount = script.PromptTokens,
                            OutputTokenCount = script.CompletionTokens,
                            TotalTokenCount = script.PromptTokens + script.CompletionTokens,
                        }
                    ),
                ],
            };
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey is null && serviceType.IsInstanceOfType(this))
        {
            return this;
        }
        return liveChatClient?.GetService(serviceType, serviceKey);
    }

    public void Dispose()
    {
        liveChatClient?.Dispose();
    }

    private IChatClient RequireLiveChatClient()
    {
        return liveChatClient
            ?? throw new InvalidOperationException(
                "No AI API key is configured and no response is queued. "
                    + "Set Ai:ApiKey, or POST /agent/test/script before starting a run."
            );
    }

    private static ScriptedStep SelectStep(AiScript script, IEnumerable<ChatMessage> messages)
    {
        var messageList = messages.ToList();
        var lastUserMessageIndex = messageList.FindLastIndex(message =>
            message.Role == ChatRole.User
        );
        var completedToolRounds = messageList
            .Skip(lastUserMessageIndex + 1)
            .Count(message => message.Contents.Any(content => content is FunctionResultContent));

        if (completedToolRounds >= script.Steps.Count)
        {
            throw new InvalidOperationException(
                $"The queued script has {script.Steps.Count} step(s) but step "
                    + $"{completedToolRounds + 1} was requested. The last step must not call tools."
            );
        }

        return script.Steps[completedToolRounds];
    }

    private static Dictionary<string, object?>? ReadArguments(JsonElement? arguments)
    {
        if (arguments is not { ValueKind: JsonValueKind.Object } element)
        {
            return null;
        }

        return element
            .EnumerateObject()
            .ToDictionary(property => property.Name, property => (object?)property.Value);
    }

    private static IEnumerable<string> SplitIntoChunks(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        for (var offset = 0; offset < text.Length; offset += TextChunkLength)
        {
            yield return text.Substring(offset, Math.Min(TextChunkLength, text.Length - offset));
        }
    }
}
