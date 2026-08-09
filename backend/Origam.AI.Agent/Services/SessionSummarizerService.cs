using System.ClientModel;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Origam.AI.Agent.Services;

public class SessionSummarizerService
{
    private readonly AiConnectionSettings settings;

    public SessionSummarizerService(IConfiguration configuration)
    {
        settings = AiConnectionSettings.Read(configuration);
    }

    public bool IsConfigured => settings.HasApiKey;

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
        chatHistory.AddSystemMessage(
            "You maintain a compact running summary of an AI-assisted ORIGAM low-code editing "
                + "session. Given the previous summary (if any) plus the latest conversation "
                + "turns, produce an updated summary in under 200 words. Focus on: which business "
                + "entities/artefacts the user is working on (name them explicitly), what has been "
                + "decided or created, which fields/choices are in play, and any open questions. "
                + "Preserve short aliases (like n_xxxxxxxx) verbatim so future turns can resolve "
                + "them. Do not add greetings, meta commentary, or bullet-list scaffolding — just "
                + "the summary text."
        );

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
