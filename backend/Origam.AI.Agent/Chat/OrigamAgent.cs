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
using System.Runtime.ExceptionServices;
using AGUI.Abstractions;
using AGUI.Server;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Origam.AI.Agent.Invocation;
using Origam.AI.Agent.Models.Responses;
using Origam.AI.Agent.Strategy;

namespace Origam.AI.Agent.Chat;

public sealed class OrigamAgent : DelegatingAIAgent
{
    private const int MaxToolIterations = 80;
    private const int SummarizeEveryAssistantTurns = 5;
    private static readonly TimeSpan StreamIdleTimeout = TimeSpan.FromSeconds(value: 120);

    private readonly IAgentTargetStrategy target;
    private readonly SessionSummarizerService sessionSummarizerService;

    public OrigamAgent(
        AIAgent innerAgent,
        IAgentTargetStrategy target,
        SessionSummarizerService sessionSummarizerService
    )
        : base(innerAgent)
    {
        if (target.ToolProviders.Count == 0)
        {
            throw new InvalidOperationException(
                $"Agent target '{target.Name}' registered no tool providers."
            );
        }

        this.target = target;
        this.sessionSummarizerService = sessionSummarizerService;
    }

    protected override Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return RunCoreStreamingAsync(messages, session, options, cancellationToken)
            .ToAgentResponseAsync(cancellationToken);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var incomingChatOptions = (options as ChatClientAgentRunOptions)?.ChatOptions;
        RunAgentInput? runAgentInput = null;
        if (incomingChatOptions is not null)
        {
            incomingChatOptions.TryGetRunAgentInput(out runAgentInput);
        }
        var settings = AgentRunSettings.FromRunAgentInput(
            runAgentInput,
            target.Options.DefaultSections
        );

        var incomingMessages = messages.ToList();
        var conversation = await BuildConversationAsync(
            incomingMessages,
            settings,
            cancellationToken
        );

        var toolTracker = new ToolCallTracker(MaxToolIterations);
        var runOptions = await BuildRunOptionsAsync(
            incomingChatOptions,
            settings,
            toolTracker,
            cancellationToken
        );

        var reply = new AgentReplyAccumulator();
        var stream = new IdleTimeoutStream(
            StreamIdleTimeout,
            string.Format(target.Prompts.StreamStalled, StreamIdleTimeout.TotalSeconds)
        );

        var updates = stream.ReadAsync(
            streamCancellationToken =>
                InnerAgent.RunStreamingAsync(
                    conversation,
                    session,
                    runOptions,
                    streamCancellationToken
                ),
            cancellationToken
        );

        await foreach (var update in updates.ConfigureAwait(false))
        {
            reply.Add(update);
            yield return update;
        }

        var updatedSummary = stream.Failure is null
            ? await SummarizeAsync(incomingMessages, reply.ReplyText, settings, cancellationToken)
            : null;

        if (stream.Failure is null && !reply.ClosedWithText)
        {
            yield return new AgentResponseUpdate(
                new ChatResponseUpdate(ChatRole.Assistant, target.Prompts.EmptyReply)
                {
                    MessageId = Guid.NewGuid().ToString(format: "N"),
                }
            );
        }

        yield return AguiEvents.Create(
            AguiEvents.RunResultName,
            new RunResult(
                toolTracker.ModelChanged,
                toolTracker.AffectedNodes,
                toolTracker.LimitReached,
                updatedSummary,
                reply.Usage
            )
        );

        if (stream.Failure is not null)
        {
            ExceptionDispatchInfo.Capture(stream.Failure).Throw();
        }
    }

    private async Task<List<ChatMessage>> BuildConversationAsync(
        IReadOnlyList<ChatMessage> incomingMessages,
        AgentRunSettings settings,
        CancellationToken cancellationToken
    )
    {
        var context = await target.Context.GetContextAsync(settings, cancellationToken);
        var conversation = new List<ChatMessage>(context);
        conversation.AddRange(incomingMessages);
        return conversation;
    }

    private async Task<ChatClientAgentRunOptions> BuildRunOptionsAsync(
        ChatOptions? incomingChatOptions,
        AgentRunSettings settings,
        ToolCallTracker toolTracker,
        CancellationToken cancellationToken
    )
    {
        var runChatOptions = incomingChatOptions?.Clone() ?? new ChatOptions();
#pragma warning disable OPENAI001
        runChatOptions.RawRepresentationFactory = _ => new CreateResponseOptions
        {
            StoredOutputEnabled = true,
        };
#pragma warning restore OPENAI001
        runChatOptions.Tools = await BuildToolsAsync(
            runChatOptions.Tools,
            settings.EnabledSections,
            cancellationToken
        );

        var toolInvocation = ToolInvocationPipeline.Build(target.CreateFilters(toolTracker));
        return new ChatClientAgentRunOptions
        {
            ChatOptions = runChatOptions,
            ChatClientFactory = innerChatClient => new FunctionInvokingChatClient(innerChatClient)
            {
                MaximumIterationsPerRequest = MaxToolIterations,
                FunctionInvoker = (context, invocationToken) =>
                    toolInvocation(context, invocationToken),
            },
        };
    }

    private async Task<IList<AITool>> BuildToolsAsync(
        IList<AITool>? clientTools,
        IReadOnlyList<string> enabledSections,
        CancellationToken cancellationToken
    )
    {
        var tools = new List<AITool>();
        foreach (var toolProvider in target.ToolProviders)
        {
            tools.AddRange(await toolProvider.GetToolsAsync(enabledSections, cancellationToken));
        }

        if (clientTools is { Count: > 0 })
        {
            tools.AddRange(clientTools);
        }

        return tools;
    }

    private async Task<string?> SummarizeAsync(
        IReadOnlyList<ChatMessage> conversation,
        string reply,
        AgentRunSettings settings,
        CancellationToken cancellationToken
    )
    {
        if (!sessionSummarizerService.IsConfigured)
        {
            return null;
        }

        var assistantTurns = conversation.Count(message => message.Role == ChatRole.Assistant) + 1;
        if (assistantTurns % SummarizeEveryAssistantTurns != 0)
        {
            return null;
        }

        var latestUserMessage =
            conversation.LastOrDefault(message => message.Role == ChatRole.User)?.Text
            ?? string.Empty;
        var priorMessages = conversation
            .Where(message => message.Role == ChatRole.User || message.Role == ChatRole.Assistant)
            .SkipLast(1)
            .ToList();

        return await sessionSummarizerService.SummarizeAsync(
            settings.Summary,
            priorMessages,
            latestUserMessage,
            reply,
            cancellationToken
        );
    }
}
