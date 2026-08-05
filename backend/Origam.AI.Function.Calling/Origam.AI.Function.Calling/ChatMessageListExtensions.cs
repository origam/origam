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

using Microsoft.Extensions.AI;

namespace Origam.AI.Function.Calling;

public static class ChatMessageListExtensions
{
    public static void AddSystemMessage(this List<ChatMessage> messages, string content)
    {
        messages.Add(new ChatMessage(ChatRole.System, content));
    }

    public static void AddUserMessage(this List<ChatMessage> messages, string content)
    {
        messages.Add(new ChatMessage(ChatRole.User, content));
    }

    public static void AddUserMessage(
        this List<ChatMessage> messages,
        string content,
        IReadOnlyList<string>? imageDataUris
    )
    {
        if (imageDataUris is not { Count: > 0 })
        {
            messages.AddUserMessage(content);
            return;
        }

        var messageContent = new List<AIContent> { new TextContent(content) };
        foreach (var imageDataUri in imageDataUris)
        {
            var parsedImage = ImageDataUri.TryParse(imageDataUri);
            if (parsedImage is not null)
            {
                messageContent.Add(
                    new DataContent(parsedImage.Value.Bytes, parsedImage.Value.MimeType)
                );
            }
        }

        messages.Add(new ChatMessage(ChatRole.User, messageContent));
    }

    public static void AddAssistantMessage(this List<ChatMessage> messages, string content)
    {
        messages.Add(new ChatMessage(ChatRole.Assistant, content));
    }
}
