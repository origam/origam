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

namespace Origam.AI.Agent;

public static class AiAgentExtensions
{
    public static IServiceCollection AddOrigamAiAgent(this IServiceCollection services)
    {
        services
            .AddHttpClient("architect")
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                }
            );
        services.AddTransient<RateLimitRetryHandler>();
        services.AddTransient<OpenAiTrafficLogHandler>();
        services
            .AddHttpClient(name: "openai", client => client.Timeout = TimeSpan.FromMinutes(10))
            .AddHttpMessageHandler<OpenAiTrafficLogHandler>()
            .AddHttpMessageHandler<RateLimitRetryHandler>();

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

        endpoints.MapGet(
            pattern: "/agent/health",
            () =>
                Results.Ok(
                    new
                    {
                        status = "ok",
                        endpoint = settings.Endpoint,
                        model = settings.Model,
                        hasApiKey = settings.HasApiKey,
                    }
                )
        );

        endpoints.MapGet(
            pattern: "/agent/architect/sections",
            async (OpenApiSectionProvider sectionProvider, CancellationToken cancellationToken) =>
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
                            defaultSections = OpenApiSectionProvider.SafeDefaultSections,
                            sections = Array.Empty<object>(),
                        }
                    );
                }
                return Results.Ok(
                    new
                    {
                        available = true,
                        baseUrl = sectionProvider.BaseUrl,
                        defaultSections = OpenApiSectionProvider.SafeDefaultSections,
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

        if (settings.HasApiKey)
        {
            endpoints.MapAGUIServer(
                pattern: "/agent/architect",
                endpoints.ServiceProvider.GetRequiredService<ArchitectAgent>()
            );
        }

        return endpoints;
    }

    private static ArchitectAgent CreateArchitectAgent(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var settings = AiConnectionSettings.Read(configuration);
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
        var baseAgent = openAiClient
            .GetResponsesClient()
            .AsIChatClient(settings.Model)
            .AsAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "OrigamArchitect",
                    UseProvidedChatClientAsIs = true,
                }
            );
#pragma warning restore OPENAI001

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
}
