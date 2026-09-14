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
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Models.Responses;
using Origam.AI.Agent.Strategy;
using Origam.AI.Agent.Tools;

namespace Origam.AI.Agent.Providers;

public sealed class CommunityToolProvider(
    IHttpClientFactory httpClientFactory,
    CommunityOptions communityOptions,
    IReadOnlyList<string> sectionTags
) : IToolProvider
{
    public Task<IReadOnlyList<AITool>> GetToolsAsync(
        IReadOnlyList<string> enabledSections,
        CancellationToken cancellationToken
    )
    {
        var tools = enabledSections.Contains(CommunityWebSearchTool.SectionName)
            ? CommunityWebSearchTool.CreateTools(httpClientFactory, communityOptions)
            : Array.Empty<AITool>();

        return Task.FromResult(tools);
    }

    public Task<IReadOnlyList<SectionInfo>?> GetSectionsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<SectionInfo> sections = CommunityWebSearchTool.GetSectionInfo(
            communityOptions,
            sectionTags
        )
            is { } communitySection
            ? new[] { communitySection }
            : Array.Empty<SectionInfo>();

        return Task.FromResult<IReadOnlyList<SectionInfo>?>(sections);
    }
}
