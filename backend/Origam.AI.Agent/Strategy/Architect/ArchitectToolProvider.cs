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
using Origam.AI.Agent.Models.Responses;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Strategy.Architect.Api;
using Origam.AI.Agent.Tools;

namespace Origam.AI.Agent.Strategy.Architect;

public sealed class ArchitectToolProvider : IToolProvider
{
    private readonly ArchitectApiClient architectApi;
    private readonly AliasMappingService aliasMappingService;
    private readonly YamlSchemaSerializer yamlSerializer;

    public ArchitectToolProvider(
        ArchitectApiClient architectApi,
        AliasMappingService aliasMappingService,
        YamlSchemaSerializer yamlSerializer
    )
    {
        this.architectApi = architectApi;
        this.aliasMappingService = aliasMappingService;
        this.yamlSerializer = yamlSerializer;
    }

    public Task<IReadOnlyList<AITool>> GetToolsAsync(
        IReadOnlyList<string> enabledSections,
        CancellationToken cancellationToken
    )
    {
        var schemaTool = new SchemaExplorationTool(
            architectApi,
            aliasMappingService,
            yamlSerializer
        );

        IReadOnlyList<AITool> tools = new AITool[]
        {
            AIFunctionFactory.Create(schemaTool.ExploreNodeAsync),
            AIFunctionFactory.Create(schemaTool.SearchSchemaAsync),
        };

        return Task.FromResult(tools);
    }

    public Task<IReadOnlyList<SectionInfo>?> GetSectionsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<SectionInfo>?>(Array.Empty<SectionInfo>());
    }
}
