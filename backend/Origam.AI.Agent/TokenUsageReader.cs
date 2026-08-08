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

namespace Origam.AI.Agent;

public static class TokenUsageReader
{
    public static RunUsage Read(UsageDetails? usage)
    {
        if (usage is null)
        {
            return new RunUsage(
                PromptTokens: 0,
                CompletionTokens: 0,
                TotalTokens: 0,
                CachedTokens: 0
            );
        }

        return new RunUsage(
            (int)(usage.InputTokenCount ?? 0),
            (int)(usage.OutputTokenCount ?? 0),
            (int)(usage.TotalTokenCount ?? 0),
            ReadCachedTokens(usage)
        );
    }

    private static int ReadCachedTokens(UsageDetails usage)
    {
        if (usage.AdditionalCounts is null)
        {
            return 0;
        }

        foreach (var count in usage.AdditionalCounts)
        {
            if (
                count.Key.Contains(
                    value: "Cached",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return (int)count.Value;
            }
        }

        return 0;
    }
}
