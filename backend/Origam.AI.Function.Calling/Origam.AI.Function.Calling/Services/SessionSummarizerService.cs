using System.ClientModel;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Origam.AI.Function.Calling.Services;

public class SessionSummarizerService
{
    private readonly string endpoint;
    private readonly string model;
    private readonly string apiKey;

    public SessionSummarizerService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Ai");
        endpoint = section["Endpoint"] ?? "https://integrate.api.nvidia.com/v1";
        model = section["Model"] ?? "z-ai/glm-5.2";
        apiKey = section["ApiKey"] ?? "";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(apiKey);

    public async Task<string?> SummarizeAsync(
        string? existingSummary,
        IReadOnlyList<ChatTurn> priorHistory,
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
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
        )
            .GetChatClient(model)
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
            if (string.IsNullOrWhiteSpace(turn.Content))
            {
                continue;
            }
            payload
                .Append(turn.Role.ToUpperInvariant())
                .Append(": ")
                .AppendLine(turn.Content.Trim());
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
