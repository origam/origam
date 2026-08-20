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

using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.Options;
using Origam.AI.Agent.Chat;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Services.OpenApi;
using Origam.AI.Agent.Strategy;

namespace Origam.AI.Agent.Api;

internal sealed class AgentStreamStartupFilter(IServiceProvider services) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseRouting();
            app.UseEndpoints(MapStreams);
            next(app);
        };
    }

    private void MapStreams(IEndpointRouteBuilder endpoints)
    {
        var options = services.GetRequiredService<IOptions<AiOptions>>().Value;
        var isDevelopment = services.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
        var targets = services.GetServices<IAgentTargetStrategy>().ToList();

        if (targets.Count == 0)
        {
            throw new InvalidOperationException("The AI agent has no registered targets.");
        }

        if (!options.HasApiKey && !isDevelopment)
        {
            return;
        }

        foreach (var target in targets)
        {
            endpoints
                .MapAGUIServer(
                    $"/agent/{target.Name}",
                    OrigamAgentFactory.Create(services, target, options)
                )
                .WithTags(OpenApiSectionProvider.AgentApiSectionName);
        }
    }
}
