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

namespace Origam.AI.Function.Calling;

public static class TokenUsageReader
{
    public static (int PromptTokens, int CompletionTokens, int TotalTokens) Read(
        ChatMessageContent response
    )
    {
        if (
            response.Metadata is null
            || !response.Metadata.TryGetValue(key: "Usage", out var usage)
            || usage is null
        )
        {
            return (0, 0, 0);
        }

        var usageType = usage.GetType();
        int ReadProperty(string propertyName)
        {
            var value = usageType.GetProperty(propertyName)?.GetValue(usage);
            return value is int intValue ? intValue : 0;
        }

        return (
            ReadProperty("InputTokenCount"),
            ReadProperty("OutputTokenCount"),
            ReadProperty("TotalTokenCount")
        );
    }
}
