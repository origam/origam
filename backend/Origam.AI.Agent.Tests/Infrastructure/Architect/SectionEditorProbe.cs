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

public sealed class SectionEditorProbe(HttpClient architect)
{
    public async Task<DesignerWidget> ReadPersistedAsync(string sectionId)
    {
        using var closeResponse = await architect.PostAsync(
            requestUri: "/Tab/CloseAll",
            content: null,
            CancellationToken.None
        );
        closeResponse.EnsureSuccessStatusCode();
        return await ReadAsync(sectionId);
    }

    public async Task<DesignerWidget> ReadAsync(string sectionId)
    {
        using var response = await architect.PostAsJsonAsync(
            requestUri: "/SectionEditor/Update",
            new { schemaItemId = sectionId, modelChanges = Array.Empty<object>() },
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(body);
        return DesignerWidget.FromJson(
            document.RootElement.GetProperty("data").GetProperty("rootControl")
        );
    }
}
