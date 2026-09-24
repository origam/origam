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

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public static class InProcessArchitect
{
    public const string PackageVariable = "ORIGAM_TEST_PACKAGE";

    private static readonly TimeSpan AgentRunTimeout = TimeSpan.FromMinutes(10);

    private static readonly SemaphoreSlim StartGate = new(initialCount: 1, maxCount: 1);

    private static ArchitectApplicationFactory? factory;
    private static HttpClient? httpClient;

    public static string? ActivePackageName { get; private set; }

    public static async Task<HttpClient> GetClientAsync(CancellationToken cancellationToken)
    {
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

    public static async Task<bool> ActivatePackageAsync(string packageName)
    {
        if (httpClient is null)
        {
            return false;
        }

        var packages = await ReadPackagesAsync(httpClient, CancellationToken.None);
        var index = packages.FindIndex(entry =>
            string.Equals(entry.Name, packageName, StringComparison.OrdinalIgnoreCase)
        );
        if (index < 0)
        {
            return false;
        }

        await SetActivePackageAsync(httpClient, packages[index].Id, CancellationToken.None);
        ActivePackageName = packages[index].Name;
        return true;
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
        var packages = await ReadPackagesAsync(client, cancellationToken);
        if (packages.Count == 0)
        {
            return null;
        }

        var requestedName = Environment.GetEnvironmentVariable(PackageVariable);
        var index = packages.FindIndex(entry =>
            string.Equals(entry.Name, requestedName, StringComparison.OrdinalIgnoreCase)
        );
        var chosen = packages[index < 0 ? 0 : index];

        await SetActivePackageAsync(client, chosen.Id, cancellationToken);
        return chosen.Name;
    }

    private static async Task SetActivePackageAsync(
        HttpClient client,
        string packageId,
        CancellationToken cancellationToken
    )
    {
        using var response = await client.PostAsJsonAsync(
            requestUri: "/Package/SetActive",
            new { id = packageId },
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    private static async Task<List<(string Id, string Name)>> ReadPackagesAsync(
        HttpClient client,
        CancellationToken cancellationToken
    )
    {
        var body = await client.GetStringAsync(requestUri: "/Package/GetAll", cancellationToken);
        using var document = JsonDocument.Parse(body);
        return document
            .RootElement.GetProperty("packages")
            .EnumerateArray()
            .Select(entry =>
                (
                    Id: entry.GetProperty("id").GetString(),
                    Name: entry.GetProperty("name").GetString()
                )
            )
            .Where(entry => entry.Id is not null)
            .Select(entry => (Id: entry.Id!, Name: entry.Name ?? string.Empty))
            .ToList();
    }
}
