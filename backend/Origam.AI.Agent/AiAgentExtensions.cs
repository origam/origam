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

using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Tools;

namespace Origam.AI.Agent;

public static class AiAgentExtensions
{
    public static IServiceCollection AddOrigamAiAgent(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                name: "architect",
                client => client.DefaultRequestHeaders.Add(AgentRequestHeader.Name, value: "true")
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                }
            );
        services.AddTransient<RateLimitRetryHandler>();
        services.AddTransient<OpenAiTrafficLogHandler>();
        services.AddHttpClient(
            name: "community",
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OrigamArchitectAgent/1.0");
            }
        );
        services
            .AddHttpClient(name: "openai", client => client.Timeout = TimeSpan.FromMinutes(10))
            .AddHttpMessageHandler<OpenAiTrafficLogHandler>()
            .AddHttpMessageHandler<RateLimitRetryHandler>();

        services.AddSingleton<AiScriptStore>();
        services.AddSingleton<OpenApiSectionProvider>();
        services.AddSingleton<AliasMappingService>();
        services.AddSingleton<YamlSchemaSerializer>();
        services.AddSingleton<ModelIndexService>();
        services.AddSingleton<SessionSummarizerService>();
        services.AddSingleton<NewItemTypeCatalogService>();
        services.AddSingleton<ArchitectAgent>(CreateArchitectAgent);
        services.AddAGUIServer();

        return services;
    }

    public static IEndpointRouteBuilder MapOrigamAiAgent(this IEndpointRouteBuilder endpoints)
    {
        var settings = AiConnectionSettings.Read(
            endpoints.ServiceProvider.GetRequiredService<IConfiguration>()
        );
        var isDevelopment = endpoints
            .ServiceProvider.GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        endpoints
            .MapGet(
                pattern: "/agent/health",
                () =>
                    Results.Ok(
                        new
                        {
                            status = "ok",
                            endpoint = settings.Endpoint,
                            router = settings.Router,
                            model = settings.Model,
                            hasApiKey = settings.HasApiKey,
                        }
                    )
            )
            .WithTags(OpenApiSectionProvider.AgentApiSectionName);

        endpoints
            .MapGet(
                pattern: "/agent/prompt/custom",
                (IConfiguration configuration) =>
                    Results.Ok(new { text = CustomInstructionsFile.Read(configuration) })
            )
            .WithTags(OpenApiSectionProvider.AgentApiSectionName);

        endpoints
            .MapPost(
                pattern: "/agent/prompt/custom",
                (CustomInstructionsUpdate update, IConfiguration configuration) =>
                {
                    CustomInstructionsFile.Write(configuration, update.Text);
                    return Results.Ok();
                }
            )
            .WithTags(OpenApiSectionProvider.AgentApiSectionName);

        endpoints
            .MapGet(
                pattern: "/agent/architect/sections",
                async (
                    OpenApiSectionProvider sectionProvider,
                    IConfiguration configuration,
                    CancellationToken cancellationToken
                ) =>
                {
                    var pluginSections = CommunityWebSearchTool.GetSectionInfo(configuration)
                        is { } communitySection
                        ? new[] { communitySection }
                        : Array.Empty<SectionInfo>();
                    var apiSections = await sectionProvider.GetSectionsAsync(cancellationToken);
                    var allSections = apiSections is null
                        ? pluginSections
                        : pluginSections.Concat(apiSections);

                    return Results.Ok(
                        new
                        {
                            available = apiSections is not null,
                            baseUrl = sectionProvider.BaseUrl,
                            error = apiSections is null
                                ? sectionProvider.LastError
                                    ?? "Architect server unreachable (is it running with Swagger in Development?)."
                                : null,
                            defaultSections = OpenApiSectionProvider.SafeDefaultSections,
                            sections = allSections.Select(section => new
                            {
                                name = section.Name,
                                description = section.Description,
                                tags = section.Tags,
                                functionCount = section.FunctionCount,
                                functions = section.Functions,
                                enabledByDefault = section.EnabledByDefault,
                            }),
                        }
                    );
                }
            )
            .WithTags(OpenApiSectionProvider.AgentApiSectionName);

        if (settings.HasApiKey || isDevelopment)
        {
            endpoints
                .MapAGUIServer(
                    pattern: "/agent/architect",
                    endpoints.ServiceProvider.GetRequiredService<ArchitectAgent>()
                )
                .WithTags(OpenApiSectionProvider.AgentApiSectionName);
        }

        if (isDevelopment)
        {
            endpoints
                .MapPost(
                    pattern: "/agent/test/script",
                    (AiScript script, AiScriptStore scriptStore) =>
                    {
                        scriptStore.Script = script;
                        return Results.Ok();
                    }
                )
                .WithTags(OpenApiSectionProvider.AgentApiSectionName)
                .WithSummary("Tell the agent what to answer, instead of asking the real model.")
                .WithDescription("This exists for the automated tests.");

            endpoints
                .MapDelete(
                    pattern: "/agent/test/script",
                    (AiScriptStore scriptStore) =>
                    {
                        scriptStore.Script = null;
                        return Results.Ok();
                    }
                )
                .WithTags(OpenApiSectionProvider.AgentApiSectionName)
                .WithSummary("Throw the queued answer away and go back to the real model.")
                .WithDescription(
                    "Undoes POST /agent/test/script. Every test has to call this when it is "
                        + "finished: a script left queued would keep answering in the developer's "
                        + "own chat instead of the model. Available only on a server running in "
                        + "Development."
                );
        }

        return endpoints;
    }

    private static ArchitectAgent CreateArchitectAgent(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var settings = AiConnectionSettings.Read(configuration);
        var liveChatClient = settings.HasApiKey ? CreateOpenAiChatClient(settings, services) : null;
        var chatClient = services.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
            ? new ScriptedChatClient(services.GetRequiredService<AiScriptStore>(), liveChatClient)
            : liveChatClient
                ?? throw new InvalidOperationException(
                    "The AI agent needs Ai:ApiKey to be configured."
                );

        var baseAgent = chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = "OrigamArchitect",
                UseProvidedChatClientAsIs = true,
            }
        );

        return new ArchitectAgent(
            baseAgent,
            services.GetRequiredService<OpenApiSectionProvider>(),
            services.GetRequiredService<AliasMappingService>(),
            services.GetRequiredService<YamlSchemaSerializer>(),
            services.GetRequiredService<ModelIndexService>(),
            services.GetRequiredService<SessionSummarizerService>(),
            services.GetRequiredService<NewItemTypeCatalogService>(),
            services.GetRequiredService<IHttpClientFactory>(),
            configuration,
            services.GetRequiredService<ILogger<ToolErrorFilter>>()
        );
    }

    private static IChatClient CreateOpenAiChatClient(
        AiConnectionSettings settings,
        IServiceProvider services
    )
    {
        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(settings.Endpoint),
                Transport = new HttpClientPipelineTransport(
                    services.GetRequiredService<IHttpClientFactory>().CreateClient("openai")
                ),
            }
        );

#pragma warning disable OPENAI001
        return openAiClient.GetResponsesClient().AsIChatClient(settings.Model);
#pragma warning restore OPENAI001
    }
}
