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

public sealed class ModelTransferProbe(HttpClient architect)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<MoveTargets> ReadMoveTargetsAsync(string itemId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/Model/GetMoveTargets",
            new { source = new { id = itemId } },
            CancellationToken.None
        );
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"GetMoveTargets for {itemId} answered {(int)response.StatusCode}: {body}"
            );
        }

        return JsonSerializer.Deserialize<MoveTargets>(body, JsonOptions)
            ?? throw new InvalidOperationException($"GetMoveTargets for {itemId} returned {body}");
    }
}
