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
using Microsoft.Extensions.AI;
using OpenAI;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Strategy;
using Origam.AI.Agent.Testing;

namespace Origam.AI.Agent.Chat;

internal static class OrigamAgentFactory
{
    public const string HttpClientName = "openai";

    public static OrigamAgent Create(
        IServiceProvider services,
        IAgentTargetStrategy target,
        AiOptions options
    )
    {
        var liveChatClient =
            options.HasApiKey && options.HasEndpoint
                ? CreateOpenAiChatClient(options, services)
                : null;
        var chatClient = services.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
            ? new ScriptedChatClient(services.GetRequiredService<AiScriptStore>(), liveChatClient)
            : liveChatClient
                ?? throw new InvalidOperationException(Strings.AgentApiKeyAndEndpointMissing);

        var baseAgent = chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = "Origam" + target.Name,
                UseProvidedChatClientAsIs = true,
            }
        );

        return new OrigamAgent(
            baseAgent,
            target,
            services.GetRequiredService<SessionSummarizerService>()
        );
    }

    private static IChatClient CreateOpenAiChatClient(AiOptions options, IServiceProvider services)
    {
        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(options.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(options.Endpoint),
                Transport = new HttpClientPipelineTransport(
                    services.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)
                ),
            }
        );

#pragma warning disable OPENAI001
        return openAiClient.GetResponsesClient().AsIChatClient(options.Model);
#pragma warning restore OPENAI001
    }
}
