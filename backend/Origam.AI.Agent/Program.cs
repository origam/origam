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

using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using Origam.AI.Agent;
using Origam.AI.Agent.Services;

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

builder.Services.AddSingleton<OpenApiSectionProvider>();
builder.Services.AddSingleton<AliasMappingService>();
builder.Services.AddSingleton<YamlSchemaSerializer>();
builder.Services.AddSingleton<ModelIndexService>();
builder.Services.AddSingleton<SessionSummarizerService>();
builder.Services.AddSingleton<NewItemTypeCatalogService>();

builder.Services.AddAGUIServer();

var app = builder.Build();

app.UseCors(frontendCorsPolicy);

var aiSettings = app.Configuration.GetSection("Ai");
var endpoint = aiSettings["Endpoint"] ?? "https://integrate.api.nvidia.com/v1";
var model = aiSettings["Model"] ?? "z-ai/glm-5.2";
var apiKey = aiSettings["ApiKey"] ?? "";

app.MapGet(
    pattern: "/agent/health",
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

var openAiClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint),
        Transport = new HttpClientPipelineTransport(
            app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("openai")
        ),
    }
);

var baseAgent = openAiClient
    .GetChatClient(model)
    .AsIChatClient()
    .AsAIAgent(
        new ChatClientAgentOptions { Name = "OrigamArchitect", UseProvidedChatClientAsIs = true }
    );

var architectAgent = new ArchitectAgent(
    baseAgent,
    app.Services.GetRequiredService<OpenApiSectionProvider>(),
    app.Services.GetRequiredService<AliasMappingService>(),
    app.Services.GetRequiredService<YamlSchemaSerializer>(),
    app.Services.GetRequiredService<ModelIndexService>(),
    app.Services.GetRequiredService<SessionSummarizerService>(),
    app.Services.GetRequiredService<NewItemTypeCatalogService>(),
    app.Services.GetRequiredService<IHttpClientFactory>(),
    app.Configuration,
    app.Services.GetRequiredService<ILogger<ToolErrorFilter>>()
);

app.MapAGUIServer(pattern: "/agent/architect", architectAgent);

app.Run();
