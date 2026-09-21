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
using Origam.AI.Agent.Extensions;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ScreenEditorController(DesignerEditorService designerService, TabService tabService)
    : ControllerBase
{
    [HttpPost("Update")]
    [EndpointDescription(
        "Reads or edits a screen (FormControlSet) in its designer. To read a screen call it "
            + "with schemaItemId = the screen id and modelChanges = [] and leave name and "
            + "selectedDataSourceId out: the response carries dataSourceId (the screen's data "
            + "structure), dataMembers (the entity paths of that data structure a widget can "
            + "show), root (the form, with every widget on the screen nested in it) and "
            + "warnings. A new screen is created with POST /Tab/CreateNode (nodeId "
            + "Origam.Schema.GuiModel.FormSchemaItemProvider, newTypeName Screen, changes Name) "
            + "and then gets its data structure here with selectedDataSourceId = the data "
            + "structure id; a screen without one cannot be saved. To change widgets pass "
            + "modelChanges = [{schemaItemId: the widget id, changes: [{name, value}]}] using "
            + "the property names from the read response; parentSchemaItemId moves the widget "
            + "into another container and is otherwise left out. Properties that matter: "
            + "DataMember of every screen section, AsTree and plugin = one of dataMembers (the "
            + "root entity for a master section, Parent.Child for a detail); Text = the caption "
            + "of a TabPage or a Label, or the name a plugin is registered under in the client; "
            + "Orientation of a SplitPanel: 0 (Horizontal) puts its two children one above the "
            + "other, 1 (Vertical) puts them side by side; TabIndex orders the children of a "
            + "SplitPanel, 0 = the upper or left one; AsTree needs IDColumn, ParentIDColumn and "
            + "NameColumn = field names of its entity; a plugin also shows the custom "
            + "properties of its widget. The server positions and sizes the children of a "
            + "SplitPanel and the only widget of the screen or of a tab page, so set on them "
            + "only the Height (Horizontal) or Width (Vertical) of the first child of a "
            + "SplitPanel to move the splitter; Width and Height of root make the screen "
            + "bigger. Changes stay in the editor until ScreenEditor/Save is called with the "
            + "screen id."
    )]
    public ActionResult<ScreenEditorData> Update([FromBody] SectionEditorChangesModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.SchemaItemId);
        if (tab.Item is not FormControlSet screenSection)
        {
            return BadRequest($"item id: {input.SchemaItemId} is not a PanelControlSet");
        }

        tab.IsDirty |= designerService.Update(screenSection, input);
        if (Request.IsFromAIAgent())
        {
            designerService.ArrangeChanged(screenSection, input);
        }
        var editorData = designerService.GetScreenEditorData(screenSection);
        return Ok(new ScreenEditorModel { Data = editorData, IsDirty = tab.IsDirty });
    }

    [HttpPost("Delete")]
    [EndpointDescription(
        "Removes widgets from a screen in its designer, together with everything inside them. "
            + "editorSchemaItemId is the screen id, schemaItemIds are the ids of the widgets to "
            + "remove as shown by the read response of ScreenEditor/Update. Call "
            + "ScreenEditor/Save afterwards."
    )]
    public ActionResult<ScreenEditorModel> Delete([FromBody] ScreenEditorDeleteItemModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.EditorSchemaItemId);
        if (tab.Item is FormControlSet screenSection)
        {
            designerService.DeleteItem(input.SchemaItemIds, screenSection);
            tab.IsDirty = true;
            var editorData = designerService.GetScreenEditorData(screenSection);
            return new ScreenEditorModel { Data = editorData, IsDirty = true };
        }

        return BadRequest($"item id: {input.EditorSchemaItemId} is not a PanelControlSet");
    }

    [HttpPost("CreateItem")]
    [EndpointDescription(
        "Adds one widget to a screen in its designer and returns it. Read the screen first "
            + "with ScreenEditor/Update and modelChanges = []. editorSchemaItemId is the screen "
            + "id. controlName is what to add: SplitPanel, TabControl, Panel, Label, AsTree, "
            + "the name of a plugin widget, the name of a screen section, or TabPage for one "
            + "more page of a TabControl; leave controlItemId out. parentControlSetItemId is "
            + "the id of the container the widget goes into: leave it out for the screen "
            + "itself, otherwise the id of a SplitPanel, a TabPage or a Panel (for a TabPage "
            + "the id of the TabControl). The client shows only one widget placed directly on "
            + "the screen, so a screen with more than one part starts with a SplitPanel or a "
            + "TabControl and everything else goes inside it; containers nest. A SplitPanel "
            + "holds exactly two children, the first created is the upper or left one; set its "
            + "Orientation before adding them. A new TabControl comes with two pages (its "
            + "children in the response), widgets go into a page, not into the TabControl. "
            + "Screen sections, AsTree and plugins show data, so set their DataMember with "
            + "ScreenEditor/Update right after creating them. top and left are pixels inside "
            + "the parent and matter only for a Label or for widgets inside a Panel, everything "
            + "else is positioned by the server. The widget exists only in the editor until "
            + "ScreenEditor/Save is called with the screen id."
    )]
    public ActionResult<ScreenEditorItem> CreateItem([FromBody] ScreenEditorItemModel itemModelData)
    {
        TabData tab = tabService.OpenDefaultTab(itemModelData.EditorSchemaItemId);
        ISchemaItem item = tab.Item;
        if (item is FormControlSet screenSection)
        {
            ScreenEditorItem newItem = designerService.CreateNewItem(
                itemModelData,
                screenSection,
                fitToParent: Request.IsFromAIAgent()
            );
            tab.IsDirty = true;
            return Ok(newItem);
        }

        return BadRequest($"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet");
    }

    [HttpPost("Save")]
    [EndpointDescription(
        "Writes a screen with every widget added, changed or removed in its designer to disk. "
            + "schemaItemId is the screen id. Call it once after the CreateItem, Update and "
            + "Delete calls on that screen and before you reply. The response lists warnings "
            + "about what stops the client from showing the screen; each says how to fix it, "
            + "so do that, save again and only then reply. A user opens a screen in the client "
            + "through a menu item, created with POST /wizards/menu-items."
    )]
    public ActionResult<SectionSaveResult> Save([FromBody] PersistModel input)
    {
        TabData tabData = tabService.OpenDefaultTab(input.SchemaItemId);
        if (tabData.Item is not FormControlSet screen)
        {
            return BadRequest($"item id: {input.SchemaItemId} is not a FormControlSet");
        }

        if (!tabService.CanPersist(screen))
        {
            return BadRequest(Strings.ScreenEditor_NoDataSource);
        }

        tabService.PersistItem(tabData);
        if (Request.IsFromAIAgent())
        {
            tabService.CloseTab(tabData.Id);
        }

        return Ok(new SectionSaveResult { Warnings = designerService.FindScreenWarnings(screen) });
    }

    [HttpGet("GetSections")]
    public ActionResult<Dictionary<Guid, ApiControl>> GetSections(
        [FromQuery(Name = "sectionIds[]")] Guid[] sectionIds,
        [FromQuery] Guid editorSchemaItemId
    )
    {
        TabData tab = tabService.OpenDefaultTab(editorSchemaItemId);
        ISchemaItem item = tab.Item;
        if (item is FormControlSet screenSection)
        {
            return designerService.LoadSections(screenSection, sectionIds);
        }

        return BadRequest($"item id: {editorSchemaItemId} is not a PanelControlSet");
    }
}
