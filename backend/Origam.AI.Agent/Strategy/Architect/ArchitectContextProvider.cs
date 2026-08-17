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
using Origam.AI.Agent.Chat;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Strategy.Architect.ItemTypes;
using Origam.AI.Agent.Strategy.Architect.ModelIndex;

namespace Origam.AI.Agent.Strategy.Architect;

public sealed class ArchitectContextProvider(
    ModelIndexService modelIndexService,
    NewItemTypeCatalogService newItemTypeCatalogService,
    AliasMappingService aliasMappingService,
    CustomInstructionsFile customInstructions,
    ArchitectPromptPack prompts
) : IPromptContextProvider
{
    public async Task<IReadOnlyList<ChatMessage>> GetContextAsync(
        AgentRunSettings settings,
        CancellationToken cancellationToken
    )
    {
        return ArchitectPromptBuilder.Build(
            prompts,
            await modelIndexService.GetContentAsync(cancellationToken),
            await newItemTypeCatalogService.GetPromptSectionsAsync(
                settings.Focus,
                cancellationToken
            ),
            settings,
            aliasMappingService,
            customInstructions.Read()
        );
    }
}
