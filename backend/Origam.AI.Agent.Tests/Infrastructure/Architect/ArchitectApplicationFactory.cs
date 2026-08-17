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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Origam.AI.Agent.Strategy.Architect;
using Origam.AI.Agent.Strategy.Architect.Api;
using Origam.AI.Agent.Tests.Infrastructure.Hosting;
using Origam.Architect.Server;

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed class ArchitectApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting(
            ArchitectOptions.SectionName + ":BaseUrl",
            AgentTestHost.ArchitectBaseUrl
        );
        builder.ConfigureTestServices(services =>
            services
                .AddHttpClient(ArchitectApiClient.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                    ((TestServer)serviceProvider.GetRequiredService<IServer>()).CreateHandler()
                )
        );
    }
}
