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

using Origam.Server.Attributes;

namespace Origam.Architect.Server.Models;

public class ChatThreadModel
{
    [RequiredNonDefault]
    public Guid Id { get; set; }

    public string Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public int TokensUsed { get; set; }

    public string Summary { get; set; }

    public List<ChatMessageModel> Messages { get; set; } = new();
}

public class ChatMessageModel
{
    public string Id { get; set; }

    public string Role { get; set; }

    public string Text { get; set; }

    public List<string> CalledFunctions { get; set; }

    public List<ChatToolCallModel> ToolCalls { get; set; }

    public List<ChatAffectedNodeModel> AffectedNodes { get; set; }

    public List<ChatImageModel> Images { get; set; }

    public int? TotalTokens { get; set; }
}

public class ChatToolCallModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Arguments { get; set; }

    public string Result { get; set; }
}

public class ChatImageModel
{
    public string Id { get; set; }

    public string MimeType { get; set; }
}

public record ChatImageRequest(Guid ThreadId, Guid ImageId, string MimeType, string Data);

public record ChatImageContent(byte[] Content, string MimeType);

public class ChatAffectedNodeModel
{
    public string OrigamId { get; set; }

    public string Label { get; set; }

    public string ItemTypeName { get; set; }

    public string Action { get; set; }
}
