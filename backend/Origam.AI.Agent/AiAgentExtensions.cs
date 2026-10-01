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

using Origam.AI.Agent.Api;
using Origam.AI.Agent.Chat;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Handlers;
using Origam.AI.Agent.Models;
using Origam.AI.Agent.Strategy;
using Origam.AI.Agent.Strategy.Architect;
using Origam.AI.Agent.Strategy.Architect.Api;
using Origam.AI.Agent.Testing;
using Origam.AI.Agent.Tools;

namespace Origam.AI.Agent;

public static class AiAgentExtensions
{
    public static IServiceCollection AddOrigamAiAgent(this IServiceCollection services)
    {
        services.AddOptions<AiOptions>().BindConfiguration(AiOptions.SectionName);

        AddHttpClients(services);

        services.AddSingleton(new PromptPack(PromptPack.SharedPackName));
        services.AddSingleton<CustomInstructionsFile>();
        services.AddSingleton<AiScriptStore>();
        services.AddSingleton<SessionSummarizerService>();
        services.AddSingleton<ChatHistoryService>();

        services.AddSingleton<ArchitectBaseUrlProvider>();
        services.AddSingleton<IAgentTargetStrategy>(ArchitectTargetStrategy.Create);

        services.AddControllers().AddApplicationPart(typeof(AiAgentExtensions).Assembly);
        services.AddSingleton<IStartupFilter, AgentStreamStartupFilter>();
        services.AddAGUIServer();

        return services;
    }

    private static void AddHttpClients(IServiceCollection services)
    {
        services.AddTransient<RateLimitRetryHandler>();
        services.AddTransient<OpenAiTrafficLogHandler>();

        services
            .AddHttpClient(
                ArchitectApiClient.HttpClientName,
                client => client.DefaultRequestHeaders.Add(AgentRequestHeader.Name, value: "true")
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                }
            );

        services.AddHttpClient(
            CommunityWebSearchTool.HttpClientName,
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OrigamArchitectAgent/1.0");
            }
        );

        services
            .AddHttpClient(
                OrigamAgentFactory.HttpClientName,
                client => client.Timeout = TimeSpan.FromMinutes(10)
            )
            .AddHttpMessageHandler<OpenAiTrafficLogHandler>()
            .AddHttpMessageHandler<RateLimitRetryHandler>();
    }
}
