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

using System.Net.Http.Json;
using System.Text.Json;

namespace Origam.AI.Agent.Tests.Infrastructure;

public static class InProcessArchitect
{
    public const string PackageVariable = "ORIGAM_TEST_PACKAGE";

    private static readonly TimeSpan AgentRunTimeout = TimeSpan.FromMinutes(10);

    private static readonly SemaphoreSlim StartGate = new(initialCount: 1, maxCount: 1);

    private static ArchitectApplicationFactory? factory;
    private static HttpClient? httpClient;

    public static string? ActivePackageName { get; private set; }

    public static IServiceProvider Services =>
        factory?.Services
        ?? throw new InvalidOperationException("Call GetClientAsync before using Services.");

    public static async Task<HttpClient> GetClientAsync(CancellationToken cancellationToken)
    {
        if (httpClient is not null)
        {
            return httpClient;
        }

        await StartGate.WaitAsync(cancellationToken);
        try
        {
            if (httpClient is null)
            {
                var startingFactory = new ArchitectApplicationFactory();
                try
                {
                    var client = startingFactory.CreateClient();
                    client.Timeout = AgentRunTimeout;
                    ActivePackageName = await ActivatePackageAsync(client, cancellationToken);
                    factory = startingFactory;
                    httpClient = client;
                }
                catch
                {
                    startingFactory.Dispose();
                    throw;
                }
            }
            return httpClient;
        }
        finally
        {
            StartGate.Release();
        }
    }

    public static void Shutdown()
    {
        httpClient?.Dispose();
        httpClient = null;
        factory?.Dispose();
        factory = null;
    }

    private static async Task<string?> ActivatePackageAsync(
        HttpClient client,
        CancellationToken cancellationToken
    )
    {
        var body = await client.GetStringAsync(requestUri: "/Package/GetAll", cancellationToken);
        using var document = JsonDocument.Parse(body);
        var packages = document
            .RootElement.GetProperty("packages")
            .EnumerateArray()
            .Select(entry => new
            {
                Id = entry.GetProperty("id").GetString(),
                Name = entry.GetProperty("name").GetString(),
            })
            .Where(entry => entry.Id is not null)
            .ToList();

        if (packages.Count == 0)
        {
            return null;
        }

        var requestedName = Environment.GetEnvironmentVariable(PackageVariable);
        var chosen =
            packages.FirstOrDefault(entry =>
                string.Equals(entry.Name, requestedName, StringComparison.OrdinalIgnoreCase)
            ) ?? packages[0];

        using var response = await client.PostAsJsonAsync(
            requestUri: "/Package/SetActive",
            new { id = chosen.Id },
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
        return chosen.Name;
    }
}
