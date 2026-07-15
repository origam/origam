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
using Origam.Architect.Server.Models.Requests.Wizards;
using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("wizards")]
public class WizardController(WizardService wizard) : ControllerBase
{
    [HttpPost("filters")]
    public IActionResult CreateFilter([FromBody] CreateFilterModel input) =>
        Ok(wizard.CreateFilter(input));

    [HttpPost("screens")]
    public IActionResult CreateScreen([FromBody] CreateScreenModel input) =>
        Ok(wizard.CreateScreen(input));

    [HttpPost("lookups")]
    public IActionResult CreateLookup([FromBody] CreateLookupModel input) =>
        Ok(wizard.CreateLookup(input));

    [HttpPost("menu-items")]
    public IActionResult CreateMenuItem([FromBody] CreateMenuItemModel input) =>
        Ok(wizard.CreateMenuItem(input));

    [HttpPost("work-queue-classes")]
    public IActionResult CreateWorkQueueClass([FromBody] CreateWorkQueueModel input) =>
        Ok(wizard.CreateWorkQueueClass(input));

    [HttpGet("screens/wizard-data")]
    public IActionResult GetScreenWizardData([FromQuery] Guid entityId) =>
        Ok(wizard.GetScreenWizardData(entityId));

    [HttpGet("lookups/wizard-data")]
    public IActionResult GetLookupWizardData([FromQuery] Guid entityId) =>
        Ok(wizard.GetLookupWizardData(entityId));

    [HttpGet("data-structures/{id}/sql")]
    public IActionResult GetDataStructureSql(Guid id) => Ok(wizard.GetDataStructureSql(id));
}
