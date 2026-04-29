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
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route(template: "[controller]")]
public class ScreenEditorController(
    DesignerEditorService designerService,
    EditorService editorService
) : ControllerBase
{
    [HttpPost(template: "Update")]
    public ActionResult<ScreenEditorData> Update([FromBody] SectionEditorChangesModel input)
    {
        EditorData editor = editorService.OpenDefaultEditor(schemaItemId: input.SchemaItemId);
        if (editor.Item is not FormControlSet screenSection)
        {
            return BadRequest(error: $"item id: {input.SchemaItemId} is not a PanelControlSet");
        }

        editor.IsDirty = designerService.Update(screenSection: screenSection, input: input);
        var editorData = designerService.GetScreenEditorData(editedItem: screenSection);
        return Ok(value: new ScreenEditorModel { Data = editorData, IsDirty = editor.IsDirty });
    }

    [HttpPost(template: "Delete")]
    public ActionResult<ScreenEditorModel> Delete([FromBody] ScreenEditorDeleteItemModel input)
    {
        EditorData editor = editorService.OpenDefaultEditor(schemaItemId: input.EditorSchemaItemId);
        if (editor.Item is FormControlSet screenSection)
        {
            designerService.DeleteItem(schemaItemIds: input.SchemaItemIds, rootItem: screenSection);
            editor.IsDirty = true;
            var editorData = designerService.GetScreenEditorData(editedItem: screenSection);
            return new ScreenEditorModel { Data = editorData, IsDirty = true };
        }

        return BadRequest(error: $"item id: {input.EditorSchemaItemId} is not a PanelControlSet");
    }

    [HttpPost(template: "CreateItem")]
    public ActionResult<ScreenEditorItem> CreateItem([FromBody] ScreenEditorItemModel itemModelData)
    {
        EditorData editor = editorService.OpenDefaultEditor(
            schemaItemId: itemModelData.EditorSchemaItemId
        );
        ISchemaItem item = editor.Item;
        if (item is FormControlSet screenSection)
        {
            ScreenEditorItem newItem = designerService.CreateNewItem(
                itemModelData: itemModelData,
                screen: screenSection
            );
            editor.IsDirty = true;
            return Ok(value: newItem);
        }

        return BadRequest(
            error: $"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet"
        );
    }

    [HttpGet(template: "GetSections")]
    public ActionResult<Dictionary<Guid, ApiControl>> GetSections(
        [FromQuery(Name = "sectionIds[]")] Guid[] sectionIds,
        [FromQuery] Guid editorSchemaItemId
    )
    {
        EditorData editor = editorService.OpenDefaultEditor(schemaItemId: editorSchemaItemId);
        ISchemaItem item = editor.Item;
        if (item is FormControlSet screenSection)
        {
            return designerService.LoadSections(
                formControlSet: screenSection,
                sectionIds: sectionIds
            );
        }

        return BadRequest(error: $"item id: {editorSchemaItemId} is not a PanelControlSet");
    }
}
