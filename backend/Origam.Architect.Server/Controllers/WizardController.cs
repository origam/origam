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
using Origam.Architect.Server.Services.Wizards;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("wizards")]
public class WizardController(
    FilterWizardService filterWizard,
    ScreenWizardService screenWizard,
    ScreenSectionWizardService screenSectionWizard,
    ScreenFromSectionWizardService screenFromSectionWizard,
    DataStructureWizardService dataStructureWizard,
    DataStructureSqlWizardService dataStructureSqlWizard,
    LookupWizardService lookupWizard,
    MenuItemWizardService menuItemWizard,
    RoleWizardService roleWizard,
    WorkQueueWizardService workQueueWizard,
    LocalizationChildEntityWizardService localizationChildEntityWizard
) : ControllerBase
{
    [HttpPost("filters")]
    public IActionResult CreateFilter([FromBody] CreateFilterModel input) =>
        Ok(filterWizard.CreateFilter(input));

    [HttpPost("screens")]
    public IActionResult CreateScreen([FromBody] CreateScreenModel input) =>
        Ok(screenWizard.CreateScreen(input));

    [HttpPost("lookups")]
    public IActionResult CreateLookup([FromBody] CreateLookupModel input) =>
        Ok(lookupWizard.CreateLookup(input));

    [HttpPost("menu-items")]
    public IActionResult CreateMenuItem([FromBody] CreateMenuItemModel input) =>
        Ok(menuItemWizard.CreateMenuItem(input));

    [HttpPost("workflow-menu-items")]
    public IActionResult CreateWorkflowMenuItem([FromBody] CreateWorkflowMenuItemModel input) =>
        Ok(menuItemWizard.CreateWorkflowMenuItem(input));

    [HttpPost("roles")]
    public IActionResult CreateRole([FromBody] CreateRoleModel input) =>
        Ok(roleWizard.CreateRole(input));

    [HttpPost("work-queue-classes")]
    public IActionResult CreateWorkQueueClass([FromBody] CreateWorkQueueModel input) =>
        Ok(workQueueWizard.CreateWorkQueueClass(input));

    [HttpPost("data-structures")]
    public IActionResult CreateDataStructure([FromBody] CreateDataStructureModel input) =>
        Ok(dataStructureWizard.CreateDataStructure(input));

    [HttpGet("data-structures/wizard-data")]
    public IActionResult GetDataStructureWizardData([FromQuery] Guid entityId) =>
        Ok(dataStructureWizard.GetWizardData(entityId));

    [HttpPost("screens-from-section")]
    public IActionResult CreateScreenFromSection([FromBody] CreateScreenFromSectionModel input) =>
        Ok(screenFromSectionWizard.CreateScreenFromSection(input));

    [HttpGet("screens-from-section/wizard-data")]
    public IActionResult GetScreenFromSectionWizardData([FromQuery] Guid screenSectionId) =>
        Ok(screenFromSectionWizard.GetWizardData(screenSectionId));

    [HttpGet("screens/wizard-data")]
    public IActionResult GetScreenWizardData([FromQuery] Guid entityId) =>
        Ok(screenWizard.GetWizardData(entityId));

    [HttpGet("lookups/wizard-data")]
    public IActionResult GetLookupWizardData([FromQuery] Guid entityId) =>
        Ok(lookupWizard.GetWizardData(entityId));

    [HttpGet("data-structures/{id}/sql")]
    public IActionResult GetDataStructureSql(Guid id) =>
        Ok(dataStructureSqlWizard.GetDataStructureSql(id));

    [HttpGet("localization-child-entities/wizard-data")]
    public IActionResult GetLocalizationChildEntityWizardData([FromQuery] Guid entityId) =>
        Ok(localizationChildEntityWizard.GetWizardData(entityId));

    [HttpPost("localization-child-entities")]
    public IActionResult CreateLocalizationChildEntity(
        [FromBody] CreateLocalizationChildEntityModel input
    ) => Ok(localizationChildEntityWizard.CreateLocalizationChildEntity(input));

    [HttpGet("screen-sections/wizard-data")]
    public IActionResult GetScreenSectionWizardData([FromQuery] Guid entityId) =>
        Ok(screenSectionWizard.GetWizardData(entityId));

    [HttpPost("screen-sections")]
    public IActionResult CreateScreenSection([FromBody] CreateScreenSectionModel input) =>
        Ok(screenSectionWizard.CreateScreenSection(input));
}
