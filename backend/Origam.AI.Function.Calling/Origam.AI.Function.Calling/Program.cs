#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Origam.AI.Function.Calling;
using Origam.AI.Function.Calling.Plugins;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(
        frontendCorsPolicy,
        policy =>
            policy
                .WithOrigins("https://localhost:5173", "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
    )
);

builder
    .Services.AddHttpClient("architect")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        }
    );
builder.Services.AddTransient<RateLimitRetryHandler>();
builder.Services.AddTransient<OpenAiTrafficLogHandler>();
builder
    .Services.AddHttpClient(name: "openai", client => client.Timeout = TimeSpan.FromMinutes(10))
    .AddHttpMessageHandler<OpenAiTrafficLogHandler>()
    .AddHttpMessageHandler<RateLimitRetryHandler>();
builder.Services.AddSingleton<Origam.AI.Function.Calling.Services.OpenApiSectionProvider>();
builder.Services.AddSingleton<ImageDescriptionService>();

builder.Services.AddSingleton<Origam.AI.Function.Calling.Services.AliasMappingService>();
builder.Services.AddSingleton<Origam.AI.Function.Calling.Services.YamlSchemaSerializer>();
builder.Services.AddSingleton<Origam.AI.Function.Calling.Services.ModelIndexService>();
builder.Services.AddSingleton<Origam.AI.Function.Calling.Services.SessionSummarizerService>();

var app = builder.Build();

app.UseCors(frontendCorsPolicy);

var aiSettings = app.Configuration.GetSection("Ai");
var endpoint = aiSettings["Endpoint"] ?? "https://integrate.api.nvidia.com/v1";
var model = aiSettings["Model"] ?? "z-ai/glm-5.2";
var apiKey = aiSettings["ApiKey"] ?? "";

app.MapGet(
    pattern: "/aichat/health",
    () =>
        Results.Ok(
            new
            {
                status = "ok",
                endpoint,
                model,
                hasApiKey = !string.IsNullOrWhiteSpace(apiKey),
            }
        )
);

app.MapGet(
    pattern: "/aichat/sections",
    async (
        Origam.AI.Function.Calling.Services.OpenApiSectionProvider sectionProvider,
        CancellationToken cancellationToken
    ) =>
    {
        var sections = await sectionProvider.GetSectionsAsync(cancellationToken);
        if (sections is null)
        {
            return Results.Ok(
                new
                {
                    available = false,
                    baseUrl = sectionProvider.BaseUrl,
                    error = sectionProvider.LastError
                        ?? "Architect server unreachable (is it running with Swagger in Development?).",
                    defaultSections = Origam
                        .AI
                        .Function
                        .Calling
                        .Services
                        .OpenApiSectionProvider
                        .SafeDefaultSections,
                    sections = Array.Empty<object>(),
                }
            );
        }
        return Results.Ok(
            new
            {
                available = true,
                baseUrl = sectionProvider.BaseUrl,
                defaultSections = Origam
                    .AI
                    .Function
                    .Calling
                    .Services
                    .OpenApiSectionProvider
                    .SafeDefaultSections,
                sections = sections.Select(section => new
                {
                    name = section.Name,
                    functionCount = section.FunctionCount,
                    functions = section.Functions,
                    hasDestructive = section.HasDestructive,
                }),
            }
        );
    }
);

app.MapPost(
    pattern: "/aichat/message",
    async (
        ChatRequest request,
        Origam.AI.Function.Calling.Services.OpenApiSectionProvider sectionProvider,
        ImageDescriptionService imageDescriptionService,
        Origam.AI.Function.Calling.Services.AliasMappingService aliasMappingService,
        Origam.AI.Function.Calling.Services.YamlSchemaSerializer yamlSerializer,
        Origam.AI.Function.Calling.Services.ModelIndexService modelIndexService,
        Origam.AI.Function.Calling.Services.SessionSummarizerService sessionSummarizerService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ToolErrorFilter> toolErrorLogger,
        CancellationToken cancellationToken
    ) =>
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new { error = "Message is required." });
        }

        string? imageDescription = null;
        if (request.Images is { Count: > 0 } && imageDescriptionService.IsConfigured)
        {
            imageDescription = await imageDescriptionService.DescribeAsync(
                request.Message,
                request.Images,
                cancellationToken
            );
        }

        var modelIndexYaml = await modelIndexService.GetYamlAsync(cancellationToken);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: model,
            endpoint: new Uri(endpoint),
            apiKey: apiKey,
            httpClient: httpClientFactory.CreateClient("openai")
        );
        kernelBuilder.Plugins.AddFromType<TimePlugin>();

        var schemaPlugin = new SchemaExplorationPlugin(
            httpClientFactory,
            aliasMappingService,
            yamlSerializer,
            configuration
        );
        kernelBuilder.Plugins.AddFromObject(schemaPlugin);

        var kernel = kernelBuilder.Build();

        var enabledSections = request.EnabledSections is { Count: > 0 }
            ? request.EnabledSections
            : Origam.AI.Function.Calling.Services.OpenApiSectionProvider.SafeDefaultSections;
        foreach (var sectionName in enabledSections)
        {
            var sectionPlugin = await sectionProvider.GetPluginAsync(
                sectionName,
                cancellationToken
            );
            if (sectionPlugin is not null)
            {
                kernel.Plugins.Add(sectionPlugin);
            }
        }

        var functionTracker = new FunctionCallTracker();
        kernel.FunctionInvocationFilters.Add(new ToolErrorFilter(toolErrorLogger));
        kernel.FunctionInvocationFilters.Add(new AliasArgumentResolver(aliasMappingService));
        kernel.FunctionInvocationFilters.Add(
            new CreateNodeValidationFilter(httpClientFactory, configuration)
        );
        kernel.FunctionInvocationFilters.Add(functionTracker);
        var toolCallLimitFilter = new ToolCallLimitFilter(maxIterations: 80);
        kernel.AutoFunctionInvocationFilters.Add(toolCallLimitFilter);

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(
            "You are a helpful assistant for the ORIGAM low-code platform. "
                + "When a tool can perform the requested action (for example creating a screen, "
                + "lookup, menu item or work queue class), use the tools instead of guessing. "
                + "Follow this order for create wizards: (1) if the user has not provided the "
                + "target entity, ask for it; (2) once you have an entity id, call the matching "
                + "read-only *wizard-data tool to discover the available fields; (3) then STOP "
                + "and show the user the available fields together with your proposed choices "
                + "(display field, id/list filters, tracked columns, name, etc.) and ask them to "
                + "confirm or change them; (4) only AFTER the user explicitly confirms, call the "
                + "create tool. Never call a create tool with assumed or default field selections "
                + "without the user's confirmation, and never invent entity ids or field names. "
                + "WIZARD CREATE TOOLS TAKE IDS, NOT NAMES. Every choice the wizard-data tool "
                + "offers you (columns, filters, display field) comes back as an object with both "
                + "an id and a name. When you present your proposed choices in step (3), write "
                + "each one as 'name (id)' and copy the id character for character out of the "
                + "wizard-data response. Write the ids out even though they look like noise to "
                + "the user: those are the values you are about to send, and spelling them out "
                + "now is what keeps them correct. Then pass exactly those ids to the create "
                + "tool. displayFieldId, idFilterId, listFilterId and the like are GUIDs; the "
                + "server rejects the request if you send 'Name' or 'GetId' there, and rejects "
                + "it just as hard if you send a GUID you produced yourself. NEVER write a GUID "
                + "that you did not copy from a tool response in this conversation - a GUID "
                + "cannot be recalled or reconstructed, only copied. If the id you need is no "
                + "longer in front of you, call the wizard-data tool again to get it. Only the "
                + "item's own Name property is free text. If a create tool returns an error, "
                + "read the response body - it names the offending property - fix that argument "
                + "and retry once instead of guessing at the cause or asking the user to "
                + "re-confirm choices they already made. "
                + "When selecting fields for a SCREEN, exclude primary-key fields by default "
                + "(the wizard-data marks them with isPrimaryKey): ORIGAM cannot generate a form "
                + "control for an identifier/GUID field that has no lookup and the create will fail "
                + "with 'Lookup not set'. Only include a primary-key field if the user explicitly "
                + "asks. If a create tool returns a 'Lookup not set for <entity>/<field>' error, "
                + "explain that this field is a GUID/identifier without a lookup, and offer to "
                + "retry without that field rather than blaming the server."
        );

        chatHistory.AddSystemMessage(
            "Be economical with tool calls, but never guess. MODEL INDEX already lists every "
                + "entity with its fields and its related structures, screens, panels, lookups "
                + "and work queues, so read it first and do not call a tool to rediscover "
                + "something it already shows. What it does NOT carry is per-field detail (data "
                + "type, length, nullability, captions, lookups on a field), audit fields, and "
                + "everything that is not an entity - rules, workflows, menu items, data "
                + "constants. When you genuinely need one of those, call ExploreNodeAsync once "
                + "with the entity's id: that is expected and correct, not redundant "
                + "exploration. Use SearchSchemaAsync only for elements that are not listed in "
                + "MODEL INDEX at all. What you must NOT do is fetch the same "
                + "information twice: once a wizard-data tool or ExploreNodeAsync has returned an "
                + "entity's fields, treat that result as authoritative for the rest of the turn, "
                + "do NOT call it again for the same entity, and do NOT walk the model tree "
                + "(GetChildren, GetTopNodes) to re-verify it. Do not repeat a tool call "
                + "with the same arguments you have already made in this conversation. When a "
                + "tool needs an entity or field id, pass the alias id from MODEL INDEX or FOCUS "
                + "(for example n_c4b132a4) directly as the argument; never pass an all-zero or "
                + "made-up GUID. If you do not have the id, look it up first."
        );

        chatHistory.AddSystemMessage(
            "## AVOIDING REDUNDANT EXPLORATION\n"
                + "Do not re-fetch a node's structure (ExploreNode, GetChildren, "
                + "GetSchemaNodeDetails, GetMenuItems) that you already fetched in this turn, "
                + "UNLESS you have changed that node since (created, updated, or deleted something "
                + "under it) — only then may you re-read that specific node once to confirm the "
                + "result. Ids of existing elements are stable, and the id of an item you just "
                + "created is returned in the create response, so use it directly instead of "
                + "re-exploring to find it. If a create or update tool returns an error, read the "
                + "error text and either fix the arguments and retry once or ask the user; do NOT "
                + "respond to an error by re-exploring the same nodes."
        );

        chatHistory.AddSystemMessage(
            "## CREATING MODEL ITEMS\n"
                + "Most objects (database and virtual fields, function-call and lookup fields, "
                + "relationships, parameters, filters, indexes, and even new entities) are not "
                + "created by a dedicated wizard but by a generic two-step flow:\n"
                + "1. Call GetMenuItems with the PARENT node id (a field's parent is the entity; "
                + "a new entity's parent is the Entities folder or package node) to list the item "
                + "types that can be created there. Each entry has a caption (for example "
                + "'Database Field') and a typeName (for example "
                + "Origam.Schema.EntityModel.FieldMappingItem).\n"
                + "2. Call CreateNode with nodeId set to the parent id and newTypeName set to that "
                + "typeName.\n"
                + "The CreateNode response lists the new item's editable properties with "
                + "validation errors (for example 'Name cannot be empty'): set the required ones "
                + "and save. Never invent a newTypeName — always take it from GetMenuItems. If the "
                + "user asks to create something, use this flow instead of claiming you have no "
                + "tool for it. This flow needs the Tab and Model tool sections to be enabled.\n"
                + "NEVER END YOUR TURN WITH AN UNSAVED ITEM. An item created by CreateNode exists "
                + "only inside its editor tab until it is persisted; the moment that tab is closed "
                + "or the package is refreshed the item is discarded and cannot be recovered. So "
                + "whenever you created something and are about to reply to the user - to ask for "
                + "a name, to ask for confirmation, or just to report what you did - call "
                + "PersistChanges on it FIRST, before you write the reply. If the user has not "
                + "given you a name yet, save the item under the default name CreateNode assigned "
                + "and tell the user which name you used; renaming an item that is already saved "
                + "is a normal edit (see below) and always works. Leaving an item unsaved does "
                + "not.\n"
                + "When creating a NEW ENTITY together with fields: create the entity, then add "
                + "every field with CreateNode using the entity's id as nodeId (you do NOT need to "
                + "save the entity first - adding fields to an unsaved entity is supported), and "
                + "finally call PersistChanges ONCE on the ENTITY id to save the entity together "
                + "with all its fields. Do not call PersistChanges on the individual fields of an "
                + "unsaved entity; persisting the entity persists its fields. This works only if "
                + "you do all of it in one uninterrupted sequence of tool calls; if you need to "
                + "stop and talk to the user in between, persist the entity before you reply and "
                + "add the remaining fields to the saved entity afterwards.\n"
                + "EDITING OR RENAMING AN ITEM THAT ALREADY EXISTS (already saved) works "
                + "differently: update THAT item's own id, then call PersistChanges with the SAME "
                + "id. To rename a field, update and persist the FIELD id - do NOT persist the "
                + "parent entity to save a field change, and do NOT re-open or re-persist the "
                + "entity afterwards. Persisting the parent entity to save an edit made on a child "
                + "silently discards that edit (the entity is holding an older copy of the child), "
                + "which is what makes a rename look like it 'resets'. One edit = update that "
                + "item's id, then PersistChanges on the same id, and nothing else.\n"
                + "Worked example - rename the field 'DateRef' (field id F) to 'Date': call the "
                + "update tool with SchemaItemId = F and set Name = Date (also set Caption and "
                + "MappedColumnName to Date if the user wants the database column renamed too), "
                + "then call PersistChanges with SchemaItemId = F. Never pass the parent entity's "
                + "id to either of these two calls when renaming a field. If the user asks again "
                + "because the rename did not stick, do NOT persist the entity - repeat update and "
                + "PersistChanges on the FIELD id.\n"
                + "NEVER DELETE TO RECOVER FROM A PROBLEM. Do not call a delete tool on an item "
                + "you created earlier in this same turn. If an update looks like it did not take "
                + "effect, the update response itself already contains that item's current "
                + "property values and validation errors - read them before concluding anything. "
                + "If you still need to confirm, make ONE targeted read on that item's own id; "
                + "this is explicitly allowed and does not count as redundant exploration. Never "
                + "respond to an apparently failed update by deleting the item and starting over. "
                + "Call a delete tool only when the user explicitly asked you to delete something."
        );

        if (!string.IsNullOrWhiteSpace(modelIndexYaml))
        {
            chatHistory.AddSystemMessage(
                "## MODEL INDEX\n"
                    + "The business entities in the current ORIGAM project, grouped by package, "
                    + "in a compact notation. Format: a line 'Name(id,K)' starts an entity, where "
                    + "K is D for a database entity and V for a virtual entity. The lines that "
                    + "follow list that entity's contents, comma separated inside brackets: "
                    + "f = fields, s = structures, c = screens, p = panels, l = lookups, "
                    + "q = work queues. Related artefacts are written as 'Name=id'. A '*' after a "
                    + "field name marks it as primary key. A line is omitted when the entity has "
                    + "nothing of that kind. Standard audit fields (RecordCreated, "
                    + "RecordCreatedBy, RecordCreatedServer, RecordUpdated, RecordUpdatedBy, "
                    + "RecordUpdatedServer, _mockPk, Selected) are left out of the f lines to keep "
                    + "this index short; call ExploreNodeAsync on the entity if you need to "
                    + "confirm them. Use these ids directly in tool calls; you do NOT need to call "
                    + "SearchSchemaAsync for anything listed here.\n\n"
                    + modelIndexYaml
            );
        }

        var focusMessage = BuildFocusSystemMessage(request.Focus, aliasMappingService);
        if (!string.IsNullOrWhiteSpace(focusMessage))
        {
            chatHistory.AddSystemMessage(focusMessage);
        }

        if (!string.IsNullOrWhiteSpace(request.Summary))
        {
            chatHistory.AddSystemMessage(
                "## SESSION SUMMARY\n"
                    + "Condensed state of the earlier turns in this conversation "
                    + "(decisions, entities the user is working on, open questions). "
                    + "Treat as authoritative background context:\n\n"
                    + request.Summary
            );
        }

        if (request.History is not null)
        {
            foreach (var turn in request.History)
            {
                if (string.IsNullOrWhiteSpace(turn.Content))
                {
                    continue;
                }
                if (turn.Role == "assistant")
                {
                    chatHistory.AddAssistantMessage(turn.Content);
                }
                else
                {
                    chatHistory.AddUserMessage(turn.Content);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(imageDescription))
        {
            chatHistory.AddSystemMessage(
                "The user attached one or more images. A vision model transcribed them as "
                    + "follows; treat this as the content of the attached images:\n"
                    + imageDescription
            );
        }

        chatHistory.AddUserMessage(request.Message);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        var response = await chatCompletion.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel,
            cancellationToken
        );

        var usage = TokenUsageReader.Read(response);

        var reply = SanitizeReply(response.Content ?? "", toolCallLimitFilter.LimitReached);

        var priorAssistantTurns = request.History?.Count(turn => turn.Role == "assistant") ?? 0;
        var totalAssistantTurns = priorAssistantTurns + 1;
        string? updatedSummary = null;
        if (
            sessionSummarizerService.IsConfigured
            && totalAssistantTurns > 0
            && totalAssistantTurns % 5 == 0
        )
        {
            updatedSummary = await sessionSummarizerService.SummarizeAsync(
                request.Summary,
                request.History ?? Array.Empty<ChatTurn>(),
                request.Message,
                reply,
                cancellationToken
            );
        }

        return Results.Ok(
            new ChatResponse(
                reply,
                functionTracker.InvokedFunctions,
                usage.PromptTokens,
                usage.CompletionTokens,
                usage.TotalTokens,
                functionTracker.AffectedNodes,
                functionTracker.ModelChanged,
                updatedSummary
            )
        );
    }
);

app.Run();

static string SanitizeReply(string content, bool limitReached)
{
    if (limitReached)
    {
        return "I reached the tool-call limit for this turn, so I stopped before writing a "
            + "summary. The steps shown above were executed - tell me to continue and I will "
            + "finish and summarize what was done.";
    }

    return content;
}

static string BuildFocusSystemMessage(
    ChatFocus? focus,
    Origam.AI.Function.Calling.Services.AliasMappingService aliasMappingService
)
{
    if (focus is null)
    {
        return string.Empty;
    }

    var builder = new System.Text.StringBuilder();
    builder.AppendLine("## FOCUS");
    builder.AppendLine(
        "What the user currently has open and highlighted in the ORIGAM Architect. "
            + "Prefer these ids when the user refers to something by name."
    );

    if (focus.ActiveEditor is not null)
    {
        builder.Append("Active editor: ").Append(focus.ActiveEditor.Label);
        if (!string.IsNullOrWhiteSpace(focus.ActiveEditor.ItemTypeName))
        {
            builder.Append(" (").Append(focus.ActiveEditor.ItemTypeName).Append(')');
        }
        if (!string.IsNullOrWhiteSpace(focus.ActiveEditor.OrigamId))
        {
            builder
                .Append(" — id: ")
                .Append(aliasMappingService.GetOrAddAlias(focus.ActiveEditor.OrigamId));
        }
        builder.AppendLine();
    }

    if (focus.ActiveNode is not null)
    {
        builder.Append("Active tree node: ").Append(focus.ActiveNode.Label);
        if (!string.IsNullOrWhiteSpace(focus.ActiveNode.ItemTypeName))
        {
            builder.Append(" (").Append(focus.ActiveNode.ItemTypeName).Append(')');
        }
        if (!string.IsNullOrWhiteSpace(focus.ActiveNode.OrigamId))
        {
            builder
                .Append(" — id: ")
                .Append(aliasMappingService.GetOrAddAlias(focus.ActiveNode.OrigamId));
        }
        builder.AppendLine();
    }

    if (focus.OpenTabs is { Count: > 0 })
    {
        builder.AppendLine("Open tabs:");
        foreach (var tab in focus.OpenTabs)
        {
            builder.Append("  - ").Append(tab.Label);
            if (!string.IsNullOrWhiteSpace(tab.ItemTypeName))
            {
                builder.Append(" (").Append(tab.ItemTypeName).Append(')');
            }
            if (!string.IsNullOrWhiteSpace(tab.OrigamId))
            {
                builder.Append(" — id: ").Append(aliasMappingService.GetOrAddAlias(tab.OrigamId));
            }
            builder.AppendLine();
        }
    }

    if (focus.VisibleNodes is { Count: > 0 })
    {
        builder.AppendLine("Other visible tree nodes (name — id — path):");
        foreach (var node in focus.VisibleNodes)
        {
            builder.Append("  - ").Append(node.Label);
            if (!string.IsNullOrWhiteSpace(node.ItemTypeName))
            {
                builder.Append(" (").Append(node.ItemTypeName).Append(')');
            }
            if (!string.IsNullOrWhiteSpace(node.OrigamId))
            {
                builder.Append(" — id: ").Append(aliasMappingService.GetOrAddAlias(node.OrigamId));
            }
            if (!string.IsNullOrWhiteSpace(node.Path))
            {
                builder.Append(" — path: ").Append(node.Path);
            }
            builder.AppendLine();
        }
    }

    return builder.Length == 0 ? string.Empty : builder.ToString();
}

public record ChatRequest(
    string Message,
    ChatFocus? Focus = null,
    string? Summary = null,
    IReadOnlyList<ChatTurn>? History = null,
    IReadOnlyList<string>? Images = null,
    IReadOnlyList<string>? EnabledSections = null
);

public record ChatFocus(
    FocusItem? ActiveEditor = null,
    FocusItem? ActiveNode = null,
    IReadOnlyList<FocusItem>? OpenTabs = null,
    IReadOnlyList<FocusNode>? VisibleNodes = null
);

public record FocusItem(string Label, string? ItemTypeName = null, string? OrigamId = null);

public record FocusNode(
    string Label,
    string? ItemTypeName = null,
    string? OrigamId = null,
    string? Path = null
);

public record ChatTurn(string Role, string Content);

public record ChatResponse(
    string Reply,
    IReadOnlyList<string> CalledFunctions,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    IReadOnlyList<AffectedNode> AffectedNodes,
    bool ModelChanged,
    string? UpdatedSummary = null
);
