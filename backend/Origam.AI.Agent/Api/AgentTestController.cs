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
using Origam.AI.Agent.Models.Requests;
using Origam.AI.Agent.Services.OpenApi;
using Origam.AI.Agent.Testing;

namespace Origam.AI.Agent.Api;

[ApiController]
[Route("agent/test")]
[Tags(OpenApiSectionProvider.AgentApiSectionName)]
public sealed class AgentTestController(AiScriptStore scriptStore, IWebHostEnvironment environment)
    : ControllerBase
{
    [HttpPost("script")]
    public IActionResult SetScript([FromBody] AiScript script)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        scriptStore.Script = script;
        return Ok();
    }

    [HttpDelete("script")]
    public IActionResult ClearScript()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        scriptStore.Script = null;
        return Ok();
    }
}
