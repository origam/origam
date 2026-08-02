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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Origam.AI.Function.Calling;

public class ImageDescriptionService
{
    private readonly string endpoint;
    private readonly string model;
    private readonly string apiKey;

    public ImageDescriptionService(IConfiguration configuration)
    {
        var aiSettings = configuration.GetSection("Ai");
        endpoint = aiSettings["VisionEndpoint"] ?? "https://integrate.api.nvidia.com/v1";
        model = aiSettings["VisionModel"] ?? "meta/llama-3.2-11b-vision-instruct";
        apiKey = aiSettings["VisionApiKey"] ?? "";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(apiKey);

    public async Task<string> DescribeAsync(
        string userMessage,
        IReadOnlyList<string> imageDataUris,
        CancellationToken cancellationToken
    )
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: model,
            endpoint: new Uri(endpoint),
            apiKey: apiKey
        );
        var kernel = kernelBuilder.Build();
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        var messageItems = new ChatMessageContentItemCollection
        {
            new TextContent(
                "Describe the attached image(s) in detail so a separate text-only assistant can "
                    + "act on them. Transcribe any visible text, ids, labels, table rows and UI "
                    + "elements exactly. Keep it factual. The user's request is: "
                    + userMessage
            ),
        };
        foreach (var imageDataUri in imageDataUris)
        {
            var parsedImage = ImageDataUri.TryParse(imageDataUri);
            if (parsedImage is not null)
            {
                messageItems.Add(
                    new ImageContent(parsedImage.Value.Bytes, parsedImage.Value.MimeType)
                );
            }
        }

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(messageItems);

        var response = await chatCompletion.GetChatMessageContentAsync(
            chatHistory,
            executionSettings: null,
            kernel: kernel,
            cancellationToken: cancellationToken
        );
        return response.Content ?? "";
    }
}
