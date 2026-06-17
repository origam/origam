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
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("wizards")]
public class WizardController(WizardService wizard) : ControllerBase
{
    [HttpPost("filters")]
    public CreateWizardResult Filter(CreateFilterModel input) => wizard.CreateFilter(input);

    [HttpPost("screens")]
    public CreateWizardResult Screen(CreateScreenModel input) => wizard.CreateScreen(input);

    [HttpPost("lookups")]
    public CreateWizardResult Lookup(CreateLookupModel input) => wizard.CreateLookup(input);

    [HttpPost("menu-items")]
    public CreateWizardResult MenuItem(CreateMenuItemModel input) => wizard.CreateMenuItem(input);

    [HttpPost("work-queue-classes")]
    public CreateWizardResult WorkQueue(CreateWorkQueueModel input) =>
        wizard.CreateWorkQueueClass(input);

    [HttpGet("screens/form-data")]
    public ScreenWizardData ScreenFormData([FromQuery] Guid entityId) =>
        wizard.GetScreenWizardData(entityId);

    [HttpGet("lookups/form-data")]
    public LookupWizardData LookupFormData([FromQuery] Guid entityId) =>
        wizard.GetLookupWizardData(entityId);

    [HttpGet("data-structures/{id}/sql")]
    public GetDataStructureSqlResult Sql(Guid id) => wizard.GetDataStructureSql(id);
}
