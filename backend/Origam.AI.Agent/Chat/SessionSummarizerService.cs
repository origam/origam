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
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Extensions;
using Origam.AI.Agent.Models;
using Origam.AI.Agent.Testing;

namespace Origam.AI.Agent.Chat;

public class SessionSummarizerService(
    IOptions<AiOptions> options,
    AiScriptStore scriptStore,
    PromptPack prompts
)
{
    private readonly AiOptions settings = options.Value;

    public bool IsConfigured => settings.HasApiKey && scriptStore.Script is null;

    public async Task<string?> SummarizeAsync(
        string? existingSummary,
        IReadOnlyList<ChatMessage> priorHistory,
        string latestUserMessage,
        string latestAssistantReply,
        CancellationToken cancellationToken
    )
    {
        if (!IsConfigured)
        {
            return existingSummary;
        }

        var chatClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint) }
        )
            .GetChatClient(settings.Model)
            .AsIChatClient();

        var chatHistory = new List<ChatMessage>();
        chatHistory.AddSystemMessage(prompts.SessionSummarizerInstructions);

        var payload = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(existingSummary))
        {
            payload.AppendLine("PREVIOUS SUMMARY:").AppendLine(existingSummary).AppendLine();
        }
        payload.AppendLine("CONVERSATION TURNS:");
        foreach (var turn in priorHistory)
        {
            if (string.IsNullOrWhiteSpace(turn.Text))
            {
                continue;
            }
            payload
                .Append(turn.Role.Value.ToUpperInvariant())
                .Append(": ")
                .AppendLine(turn.Text.Trim());
        }
        payload.Append("USER: ").AppendLine(latestUserMessage.Trim());
        payload.Append("ASSISTANT: ").AppendLine(latestAssistantReply.Trim());

        chatHistory.AddUserMessage(payload.ToString());

        try
        {
            var response = await chatClient.GetResponseAsync(
                chatHistory,
                options: null,
                cancellationToken: cancellationToken
            );
            var text = response.Text?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(text) ? existingSummary : text;
        }
        catch
        {
            return existingSummary;
        }
    }
}
