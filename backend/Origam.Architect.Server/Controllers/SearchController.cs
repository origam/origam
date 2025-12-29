#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using Origam.Architect.Server.Models.Requests;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(
    IPersistenceService persistenceService
) : ControllerBase
{
    [HttpGet("Text")]
    public ActionResult Text([FromQuery]string text)
    {
        ISchemaItem[] results = persistenceService.SchemaProvider.FullTextSearch<ISchemaItem>(
            text
        );
        return Ok(results.Select(x => new SearchResult { Name = x.Name, SchemaId = x.Id }));
    }
}