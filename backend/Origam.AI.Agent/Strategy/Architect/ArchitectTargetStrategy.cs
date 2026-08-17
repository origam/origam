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

using Microsoft.Extensions.Options;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Invocation;
using Origam.AI.Agent.Models;
using Origam.AI.Agent.Providers;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Services.OpenApi;
using Origam.AI.Agent.Strategy.Architect.Api;
using Origam.AI.Agent.Strategy.Architect.Filters;
using Origam.AI.Agent.Strategy.Architect.ItemTypes;
using Origam.AI.Agent.Strategy.Architect.ModelIndex;
using Origam.AI.Agent.Tools;

namespace Origam.AI.Agent.Strategy.Architect;

public sealed class ArchitectTargetStrategy(
    TargetOptions options,
    ArchitectPromptPack prompts,
    ArchitectApiClient architectApi,
    NewItemTypeCatalogService newItemTypeCatalogService,
    AliasMappingService aliasMappingService,
    ILogger<ToolErrorFilter> toolErrorLogger,
    IReadOnlyList<IToolProvider> toolProviders,
    IPromptContextProvider contextProvider
) : IAgentTargetStrategy
{
    public const string TargetName = "architect";

    public string Name => TargetName;

    public TargetOptions Options { get; } = options;

    public PromptPack Prompts { get; } = prompts;

    public IReadOnlyList<IToolProvider> ToolProviders { get; } = toolProviders;

    public IPromptContextProvider Context { get; } = contextProvider;

    public IReadOnlyList<IToolInvocationFilter> CreateFilters(ToolCallTracker toolTracker)
    {
        return
        [
            new ToolErrorFilter(toolErrorLogger),
            new ResponseCompactionFilter(newItemTypeCatalogService),
            new AliasArgumentResolver(aliasMappingService),
            new CreateNodeValidationFilter(architectApi, newItemTypeCatalogService, prompts),
            toolTracker,
        ];
    }

    public static ArchitectTargetStrategy Create(IServiceProvider services)
    {
        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        var aiOptions = services.GetRequiredService<IOptions<AiOptions>>().Value;
        var architectOptions = services.GetRequiredService<IOptions<ArchitectOptions>>().Value;
        var customInstructions = services.GetRequiredService<CustomInstructionsFile>();
        var toolErrorLogger = services.GetRequiredService<ILogger<ToolErrorFilter>>();

        var options = ArchitectTargetOptions.Create(architectOptions.BaseUrl);
        var prompts = new ArchitectPromptPack();

        var architectApi = new ArchitectApiClient(httpClientFactory, options.BaseUrl);
        var aliasMappingService = new AliasMappingService(prompts);
        var yamlSerializer = new YamlSchemaSerializer(aliasMappingService);
        var newItemTypeCatalogService = new NewItemTypeCatalogService(architectApi, prompts);
        var modelIndexService = new ModelIndexService(architectApi, aliasMappingService, prompts);
        var sectionProvider = new OpenApiSectionProvider(architectApi, options);

        IReadOnlyList<IToolProvider> toolProviders =
        [
            new ArchitectToolProvider(architectApi, aliasMappingService, yamlSerializer),
            new CommunityToolProvider(
                httpClientFactory,
                aiOptions.Community,
                sectionProvider.TagsFor(CommunityWebSearchTool.SectionName)
            ),
            new OpenApiToolProvider(sectionProvider),
        ];

        var contextProvider = new ArchitectContextProvider(
            modelIndexService,
            newItemTypeCatalogService,
            aliasMappingService,
            customInstructions,
            prompts
        );

        return new ArchitectTargetStrategy(
            options,
            prompts,
            architectApi,
            newItemTypeCatalogService,
            aliasMappingService,
            toolErrorLogger,
            toolProviders,
            contextProvider
        );
    }
}
