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
using Origam.Architect.Server.Services;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

// Dev-only helper for the Playwright integration tests. The backend keeps the
// open editor tabs (TabService) and the loaded file model (persistence provider)
// in process-wide singletons, so state from one test run leaks into the next and
// the only reliable reset used to be a full server restart. This endpoint resets
// that in-memory state in-process: callers restore the model files on disk first
// (e.g. `git checkout`/`git clean` of model-tests/model) and then POST here to
// make the running server forget its tabs and re-read the clean model.
[ApiController]
[Route("[controller]")]
public class TestController(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TabService tabService,
    IWebHostEnvironment environment
) : ControllerBase
{
    [HttpPost("Reset")]
    public IActionResult Reset()
    {
        // A full-model reset is destructive (drops every unsaved change), so it
        // must never be reachable outside a development server.
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        SecurityManager.SetServerIdentity();

        // Drop all open editor tabs (kept in the singleton TabService).
        tabService.CloseAllTabs();

        // Re-read the file model from disk. FlushCache/LoadSchema on their own
        // keep the cached file index, so files removed on disk (a git reset of
        // model-tests/model between runs) would otherwise still appear "loaded".
        if (persistenceService is FilePersistenceService filePersistence)
        {
            var loadError = filePersistence.Reload();
            if (loadError.HasValue)
            {
                return StatusCode(statusCode: 500, loadError.Value.Message);
            }
        }

        // Rebuild the schema providers from the freshly reloaded model so the
        // tree and editors reflect the clean baseline.
        Guid activePackageId = schemaService.ActiveSchemaExtensionId;
        if (activePackageId != Guid.Empty)
        {
            schemaService.LoadSchema(activePackageId);
        }

        return Ok();
    }
}
