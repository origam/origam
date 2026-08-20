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

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Origam.AI.Agent.Strategy.Architect;

public sealed class ArchitectBaseUrlProvider(IServer server)
{
    private static readonly string[] WildcardHosts = ["+", "*", "0.0.0.0", "[::]"];

    private string? resolvedBaseUrl;

    public string BaseUrl => resolvedBaseUrl ??= Resolve();

    private string Resolve()
    {
        ICollection<string> serverAddresses =
            server.Features.Get<IServerAddressesFeature>()?.Addresses ?? [];
        var address =
            serverAddresses.FirstOrDefault(candidate =>
                candidate.StartsWith(value: "https://", StringComparison.OrdinalIgnoreCase)
            ) ?? serverAddresses.FirstOrDefault();
        if (address is null)
        {
            throw new InvalidOperationException(
                "The AI agent cannot determine the Architect base url because the server "
                    + "reports no listening address."
            );
        }

        return ReplaceWildcardHost(address.TrimEnd('/'));
    }

    private static string ReplaceWildcardHost(string address)
    {
        foreach (var wildcardHost in WildcardHosts)
        {
            address = address.Replace("://" + wildcardHost + ":", newValue: "://localhost:");
        }

        return address;
    }
}
