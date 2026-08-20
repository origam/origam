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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Models.Requests;
using Origam.AI.Agent.Models.Responses;
using Origam.AI.Agent.Providers;
using Origam.AI.Agent.Services.OpenApi;
using Origam.AI.Agent.Strategy;

namespace Origam.AI.Agent.Api;

[ApiController]
[Route("agent")]
[Tags(OpenApiSectionProvider.AgentApiSectionName)]
public sealed class AgentController(
    IOptions<AiOptions> options,
    CustomInstructionsFile customInstructions,
    IEnumerable<IAgentTargetStrategy> targets
) : ControllerBase
{
    private const string UnreachableError =
        "Architect server unreachable (is it running with Swagger in Development?).";

    [HttpGet("health")]
    public AgentHealth Health()
    {
        var settings = options.Value;
        return new AgentHealth(
            Status: "ok",
            settings.Endpoint,
            settings.Router,
            settings.Model,
            settings.HasApiKey
        );
    }

    [HttpGet("prompt/custom")]
    public CustomInstructions GetCustomInstructions()
    {
        return new CustomInstructions(customInstructions.Read());
    }

    [HttpPost("prompt/custom")]
    public IActionResult SaveCustomInstructions([FromBody] CustomInstructionsUpdate update)
    {
        customInstructions.Write(update.Text);
        return Ok();
    }

    [HttpGet("{target}/sections")]
    public async Task<ActionResult<AgentSections>> Sections(
        string target,
        CancellationToken cancellationToken
    )
    {
        var strategy = targets.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, target, StringComparison.OrdinalIgnoreCase)
        );
        if (strategy is null)
        {
            return NotFound();
        }

        var sectionLists = new List<IReadOnlyList<SectionInfo>?>();
        foreach (var toolProvider in strategy.ToolProviders)
        {
            sectionLists.Add(await toolProvider.GetSectionsAsync(cancellationToken));
        }

        var openApiProvider = strategy.ToolProviders.OfType<OpenApiToolProvider>().FirstOrDefault();
        var available = sectionLists.All(sections => sections is not null);

        return new AgentSections(
            available,
            openApiProvider?.BaseUrl ?? strategy.Options.BaseUrl(),
            available ? null : openApiProvider?.LastError ?? UnreachableError,
            strategy.Options.DefaultSections,
            sectionLists
                .Where(sections => sections is not null)
                .SelectMany(sections => sections!)
                .ToList()
        );
    }
}
