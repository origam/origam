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
using System.Text;
using AGUI.Abstractions;
using AGUI.Server;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Tools;

namespace Origam.AI.Agent;

public sealed class ArchitectAgent : DelegatingAIAgent
{
    private const int MaxToolIterations = 80;
    private const int SummarizeEveryAssistantTurns = 5;

    private readonly OpenApiSectionProvider sectionProvider;
    private readonly AliasMappingService aliasMappingService;
    private readonly YamlSchemaSerializer yamlSerializer;
    private readonly ModelIndexService modelIndexService;
    private readonly SessionSummarizerService sessionSummarizerService;
    private readonly NewItemTypeCatalogService newItemTypeCatalogService;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;
    private readonly ILogger<ToolErrorFilter> toolErrorLogger;

    public ArchitectAgent(
        AIAgent innerAgent,
        OpenApiSectionProvider sectionProvider,
        AliasMappingService aliasMappingService,
        YamlSchemaSerializer yamlSerializer,
        ModelIndexService modelIndexService,
        SessionSummarizerService sessionSummarizerService,
        NewItemTypeCatalogService newItemTypeCatalogService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ToolErrorFilter> toolErrorLogger
    )
        : base(innerAgent)
    {
        this.sectionProvider = sectionProvider;
        this.aliasMappingService = aliasMappingService;
        this.yamlSerializer = yamlSerializer;
        this.modelIndexService = modelIndexService;
        this.sessionSummarizerService = sessionSummarizerService;
        this.newItemTypeCatalogService = newItemTypeCatalogService;
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
        this.toolErrorLogger = toolErrorLogger;
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
        var settings = AgentRunSettings.FromRunAgentInput(runAgentInput);

        var conversation = new List<ChatMessage>();
        conversation.AddRange(
            ArchitectPromptBuilder.Build(
                await modelIndexService.GetContentAsync(cancellationToken),
                await newItemTypeCatalogService.GetPromptSectionsAsync(
                    settings.Focus,
                    cancellationToken
                ),
                settings,
                aliasMappingService
            )
        );
        var incomingMessages = messages.ToList();
        conversation.AddRange(incomingMessages);

        var toolTracker = new ToolCallTracker(MaxToolIterations);
        var toolInvocation = ToolInvocationPipeline.Build(
            new List<IToolInvocationFilter>
            {
                new ToolErrorFilter(toolErrorLogger),
                new ResponseCompactionFilter(),
                new AliasArgumentResolver(aliasMappingService),
                new CreateNodeValidationFilter(httpClientFactory, configuration),
                toolTracker,
            }
        );

        var runChatOptions = incomingChatOptions?.Clone() ?? new ChatOptions();
        runChatOptions.Tools = await BuildToolsAsync(
            runChatOptions.Tools,
            settings.EnabledSections,
            cancellationToken
        );

        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = runChatOptions,
            ChatClientFactory = innerChatClient => new FunctionInvokingChatClient(innerChatClient)
            {
                MaximumIterationsPerRequest = MaxToolIterations,
                FunctionInvoker = (context, invocationToken) =>
                    toolInvocation(context, invocationToken),
            },
        };

        var usage = new UsageDetails();
        var replyText = new StringBuilder();
        Exception? streamFailure = null;

        var updates = InnerAgent
            .RunStreamingAsync(conversation, session, runOptions, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                AgentResponseUpdate update;
                try
                {
                    if (!await updates.MoveNextAsync().ConfigureAwait(false))
                    {
                        break;
                    }
                    update = updates.Current;
                }
                catch (Exception exception)
                {
                    streamFailure = exception;
                    break;
                }

                foreach (var content in update.Contents)
                {
                    if (content is UsageContent usageContent)
                    {
                        usage.Add(usageContent.Details);
                    }
                    else if (content is TextContent textContent)
                    {
                        replyText.Append(textContent.Text);
                    }
                }

                yield return update;
            }
        }
        finally
        {
            await updates.DisposeAsync().ConfigureAwait(false);
        }

        var updatedSummary = streamFailure is null
            ? await SummarizeAsync(
                incomingMessages,
                replyText.ToString(),
                settings,
                cancellationToken
            )
            : null;

        yield return AguiEvents.Create(
            AguiEvents.RunResultName,
            new RunResult(
                toolTracker.ModelChanged,
                toolTracker.AffectedNodes,
                toolTracker.LimitReached,
                updatedSummary,
                TokenUsageReader.Read(usage)
            )
        );

        if (streamFailure is not null)
        {
            ExceptionDispatchInfo.Capture(streamFailure).Throw();
        }
    }

    private async Task<IList<AITool>> BuildToolsAsync(
        IList<AITool>? clientTools,
        IReadOnlyList<string> enabledSections,
        CancellationToken cancellationToken
    )
    {
        var timeTool = new TimeTool();
        var schemaTool = new SchemaExplorationTool(
            httpClientFactory,
            aliasMappingService,
            yamlSerializer,
            configuration
        );

        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(timeTool.GetCurrentTime),
            AIFunctionFactory.Create(schemaTool.ExploreNodeAsync),
            AIFunctionFactory.Create(schemaTool.SearchSchemaAsync),
        };

        foreach (var sectionName in enabledSections)
        {
            var sectionTools = await sectionProvider.GetToolsAsync(sectionName, cancellationToken);
            if (sectionTools is not null)
            {
                tools.AddRange(sectionTools);
            }
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
