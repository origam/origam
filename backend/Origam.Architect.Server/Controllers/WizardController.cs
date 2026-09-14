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
    private const string WizardIdRule =
        "Every id argument is a GUID that must be copied character for character out of a tool "
        + "response in this conversation - a name such as 'Name' or 'GetId' is rejected, and so "
        + "is a GUID you produced yourself. If the id is no longer in front of you, call the "
        + "wizard-data endpoint again. Only the Name property is free text. ";

    private const string WizardConfirmationRule =
        "Before calling this, show the user the available choices from the wizard-data response "
        + "as 'name (id)' and wait for explicit confirmation; never create with assumed or "
        + "default selections. A confirmation the user has already given in this conversation "
        + "keeps its value: once they have picked what this wizard needs, create it. Reading "
        + "wizard-data again to recover the ids does not undo that choice and is never a reason "
        + "to ask the same question a second time.";

    [HttpPost("filters")]
    [EndpointDescription(
        "Creates a filter. Call GET /wizards/filters/wizard-data first and take columnId and the "
            + "filter type from its response. "
            + WizardIdRule
            + WizardConfirmationRule
    )]
    public IActionResult CreateFilter([FromBody] CreateFilterModel input) =>
        Ok(filterWizard.CreateFilter(input));

    [HttpPost("screens")]
    [EndpointDescription(
        "Creates a screen. Call GET /wizards/screens/wizard-data first and take the field ids, "
            + "displayFieldId and the filter ids from its response. Leave primary-key fields out "
            + "of the selection unless the user explicitly asks for one. "
            + WizardIdRule
            + WizardConfirmationRule
    )]
    public IActionResult CreateScreen([FromBody] CreateScreenModel input) =>
        Ok(screenWizard.CreateScreen(input));

    [HttpPost("lookups")]
    [EndpointDescription(
        "Creates a lookup. Call GET /wizards/lookups/wizard-data first and take displayFieldId, "
            + "idFilterId and listFilterId from its response. "
            + WizardIdRule
            + WizardConfirmationRule
    )]
    public IActionResult CreateLookup([FromBody] CreateLookupModel input) =>
        Ok(lookupWizard.CreateLookup(input));

    [HttpPost("menu-items")]
    [EndpointDescription(
        "Creates a menu item for an existing screen. " + WizardIdRule + WizardConfirmationRule
    )]
    public IActionResult CreateMenuItem([FromBody] CreateMenuItemModel input) =>
        Ok(menuItemWizard.CreateMenuItem(input));

    [HttpPost("workflow-menu-items")]
    public IActionResult CreateWorkflowMenuItem([FromBody] CreateWorkflowMenuItemModel input) =>
        Ok(menuItemWizard.CreateWorkflowMenuItem(input));

    [HttpPost("roles")]
    [EndpointDescription(
        "Creates the deployment activities that add a role to the database. There is no "
            + "wizard-data endpoint for this wizard: itemId is the id of an existing model item "
            + "that already carries one specific role, typically a menu item whose roles property "
            + "names a single role. An item with no role, or with the role '*', is rejected - ask "
            + "the user for a menu item with a concrete role, or create one first. The package "
            + "must also have a current deployment version. The id must be a GUID copied "
            + "character for character out of a tool response in this conversation - a name is "
            + "rejected, and so is a GUID you produced yourself. Show the user the item you are "
            + "about to use as 'name (id)' and wait for explicit confirmation before calling this."
    )]
    public IActionResult CreateRole([FromBody] CreateRoleModel input) =>
        Ok(roleWizard.CreateRole(input));

    [HttpPost("work-queue-classes")]
    [EndpointDescription(
        "Creates a work queue class. Pass Name to control what the class is called; leave it out "
            + "and the entity name is used. "
            + WizardIdRule
            + WizardConfirmationRule
    )]
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

    [HttpGet("filters/wizard-data")]
    [EndpointDescription(
        "The read-only step before creating a filter with POST /wizards/filters. Give it an "
            + "entity id and it returns that entity's name, its columns (id, name, isPrimaryKey, "
            + "dataType), the filters the entity already has, and the filter types that may be "
            + "requested. Pick the column and the filter type from here and pass the column's id "
            + "as columnId to POST /wizards/filters. This is the wizard-data endpoint for "
            + "filters; the screen and lookup ones describe different wizards and must not be "
            + "used to look up a column for a filter."
    )]
    public IActionResult GetFilterWizardData([FromQuery] Guid entityId) =>
        Ok(filterWizard.GetWizardData(entityId));

    [HttpGet("screens/wizard-data")]
    [EndpointDescription(
        "The read-only step before creating a screen with POST /wizards/screens. Give it an "
            + "entity id and it returns the fields that can be put on the screen (id, name, "
            + "isPrimaryKey) together with the available filters. Exclude the fields marked "
            + "isPrimaryKey unless the user explicitly asks for one: ORIGAM cannot generate a "
            + "form control for an identifier field that has no lookup and the create then fails "
            + "with 'Lookup not set for <entity>/<field>'. If that error does come back, explain "
            + "that the field is a GUID without a lookup and offer to retry without it. This is "
            + "the wizard-data endpoint for screens; the filter and lookup ones describe "
            + "different wizards and must not be used to look up a field for a screen."
    )]
    public IActionResult GetScreenWizardData([FromQuery] Guid entityId) =>
        Ok(screenWizard.GetWizardData(entityId));

    [HttpGet("lookups/wizard-data")]
    [EndpointDescription(
        "The read-only step before creating a lookup with POST /wizards/lookups. Give it an "
            + "entity id and it returns the entity's fields and filters, each with an id and a "
            + "name; pick displayFieldId, idFilterId and listFilterId from here and pass those "
            + "ids to POST /wizards/lookups. This is the wizard-data endpoint for lookups; the "
            + "filter and screen ones describe different wizards and must not be used to look up "
            + "a field for a lookup."
    )]
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
