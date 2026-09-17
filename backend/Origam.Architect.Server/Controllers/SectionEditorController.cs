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
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SectionEditorController(
    DesignerEditorService designerEditorService,
    TabService tabService
) : ControllerBase
{
    [HttpPost("Update")]
    [EndpointDescription(
        "Reads or edits a screen section (PanelControlSet) in its designer. To read a section "
            + "call it with schemaItemId = the section id and modelChanges = [] and leave name "
            + "and selectedDataSourceId out: the response carries the fields of the section's "
            + "entity with their data types, the root panel (rootControl, its id is the "
            + "parentControlSetItemId for new widgets) and every widget already on it with its "
            + "bound field and its Top, Left, Width and Height, plus warnings for bound fields "
            + "missing from the data structure of a screen showing the section. To change widgets pass "
            + "modelChanges = [{schemaItemId: the widget id, changes: [{name, value}]}] using "
            + "the property names from the read response; parentSchemaItemId moves the widget "
            + "into another container and is otherwise left out. Pass name only to rename the "
            + "section and selectedDataSourceId only to change its entity. Changes stay in the "
            + "editor until SectionEditor/Save is called with the section id."
    )]
    public ActionResult<SectionEditorModel> Update([FromBody] SectionEditorChangesModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.SchemaItemId);
        if (tab.Item is not PanelControlSet screenSection)
        {
            return BadRequest($"item id: {input.SchemaItemId} is not a PanelControlSet");
        }
        tab.IsDirty |= designerEditorService.Update(screenSection, input);
        var editorData = designerEditorService.GetSectionEditorData(screenSection);
        return Ok(new SectionEditorModel { Data = editorData, IsDirty = tab.IsDirty });
    }

    [HttpPost("Delete")]
    [EndpointDescription(
        "Removes widgets from a screen section in its designer. editorSchemaItemId is the "
            + "section id, schemaItemIds are the ids of the widgets to remove as shown by the "
            + "read response of SectionEditor/Update. Call SectionEditor/Save afterwards."
    )]
    public ActionResult<SectionEditorModel> Delete([FromBody] ScreenEditorDeleteItemModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.EditorSchemaItemId);
        if (tab.Item is PanelControlSet screenSection)
        {
            designerEditorService.DeleteItem(input.SchemaItemIds, screenSection);
            tab.IsDirty = true;
            var editorData = designerEditorService.GetSectionEditorData(screenSection);
            return new SectionEditorModel { Data = editorData, IsDirty = true };
        }

        return BadRequest($"item id: {input.EditorSchemaItemId} is not a PanelControlSet");
    }

    //TODO: Its is very ugly description but for beta unstable Origam AI Support maybe it's okay for now later its needs some improvements
    [HttpPost("CreateItem")]
    [EndpointDescription(
        "Adds one widget to an existing screen section in its designer and returns it; a new "
            + "section for an entity is created with POST /wizards/screen-sections instead, and "
            + "a screen showing a section with POST /wizards/screens-from-section. Read the "
            + "section first with Tab/Open on the section id or SectionEditor/Update with "
            + "modelChanges = []. editorSchemaItemId is the section id. Leave "
            + "parentControlSetItemId out to put the widget on the section's root panel; pass "
            + "the id of a GroupBox or MultiColumnAdapterFieldWrapper already on the section "
            + "to place the widget inside it. componentType is the widget type: "
            + "Origam.Gui.Win.AsTextBox for text, memo "
            + "and number fields, Origam.Gui.Win.AsDateBox for dates, Origam.Gui.Win.AsCheckBox "
            + "for booleans, Origam.Gui.Win.AsDropDown for foreign keys and other fields with a "
            + "lookup, Origam.Gui.Win.TagInput or Origam.Gui.Win.Checklist for array fields, "
            + "Origam.Gui.Win.BlobControl for file uploads, Origam.Gui.Win.ImageBox for "
            + "images, Origam.Gui.Win.ColorPicker for an integer colour, "
            + "Origam.Gui.Win.AsRadioButton, Origam.Gui.Win.MultiColumnAdapterFieldWrapper, "
            + "Origam.Gui.Win.GroupBoxWithChamfer for a container and "
            + "System.Windows.Forms.Label for static text. fieldName is the name of the entity "
            + "field the widget shows, taken from the fields list of the read response; it is "
            + "left out only for GroupBox and Label. A field may be bound by only one widget of "
            + "the section, so leave out fields that already have one and say so. top and left "
            + "are pixels inside the parent; a new widget is 100 wide and 20 high. Widgets must "
            + "never overlap: use the Left of the existing widgets and a top below the lowest "
            + "one (its Top + Height + 10) - the root panel grows by itself - and to place a "
            + "widget between existing ones first move the lower ones down with "
            + "SectionEditor/Update. What Save requires per widget, set with "
            + "SectionEditor/Update before saving: AsDropDown, TagInput and Checklist need "
            + "DataLookup, filled from the field's default lookup when it has one and otherwise "
            + "the id of a lookup; TagInput and Checklist take Array fields only; ColorPicker "
            + "takes an Integer field; AsRadioButton is bound to the field it sets and needs "
            + "Text and ValueConstant = the id of a Data Constant holding the value that button "
            + "selects (create it with POST /Tab/CreateNode when it does not exist), one radio "
            + "button per value; BlobControl is bound to the String field holding the file name "
            + "and needs BlobMember = the name of the Blob field; ImageBox is bound to the Blob "
            + "field; MultiColumnAdapterFieldWrapper is bound to the field whose value decides "
            + "which child is shown, its children are created with parentControlSetItemId = "
            + "the wrapper id and each child needs MappingCondition = the id of a Data Constant "
            + "equal to the value that shows it; GroupBox and Label are not bound and Text is "
            + "their caption. The widget exists only in the editor until SectionEditor/Save is "
            + "called with the section id."
    )]
    public ActionResult<ApiControl> CreateItem([FromBody] SectionEditorItemModel itemModelData)
    {
        TabData tab = tabService.OpenDefaultTab(itemModelData.EditorSchemaItemId);
        ISchemaItem item = tab.Item;
        if (item is PanelControlSet screenSection)
        {
            ApiControl apiControl = designerEditorService.CreateNewItem(
                itemModelData,
                screenSection
            );
            tab.IsDirty = true;
            return Ok(apiControl);
        }
        return BadRequest($"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet");
    }

    [HttpPost("Save")]
    [EndpointDescription(
        "Writes a screen section with every widget added, changed or removed in its designer "
            + "to disk. schemaItemId is the section id. Call it once after the CreateItem, "
            + "Update and Delete calls on that section and before you reply; the section is "
            + "validated here, so an error means nothing was saved and names what to fix. "
            + "The response lists warnings for bound fields that are missing from the data "
            + "structure of a screen showing the section - such a screen fails to open until "
            + "the field is added there; each warning says how to add it, so do that before "
            + "you reply and tell the user which screens you repaired."
    )]
    public ActionResult<SectionSaveResult> Save([FromBody] PersistModel input)
    {
        TabData tabData = tabService.OpenDefaultTab(input.SchemaItemId);
        ISchemaItem item = tabData.Item;
        if (item is PanelControlSet screenSection)
        {
            tabData.IsDirty = designerEditorService.SaveScreenSection(screenSection);
            return Ok(
                new SectionSaveResult
                {
                    Warnings = designerEditorService.FindDataStructureWarnings(screenSection),
                }
            );
        }

        return BadRequest($"item id: {input.SchemaItemId} is not a PanelControlSet");
    }
}
