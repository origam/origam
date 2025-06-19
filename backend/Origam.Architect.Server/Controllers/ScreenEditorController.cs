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
public class ScreenEditorController(
    DesignerEditorService designerService,
    EditorService editorService)
    : ControllerBase
{
    [HttpPost("Update")]
    public ActionResult<ScreenEditorData> Update(
        [FromBody] SectionEditorChangesModel input)
    {
        EditorData editor = editorService.OpenDefaultEditor(input.SchemaItemId);
        if (editor.Item is not FormControlSet screenSection)
        {
            return BadRequest(
                $"item id: {input.SchemaItemId} is not a PanelControlSet");
        }

        editor.IsDirty = designerService.Update(screenSection, input);
        var editorData = designerService.GetScreenEditorData(screenSection);
        return Ok(
            new ScreenEditorModel
            {
                Data = editorData,
                IsDirty = editor.IsDirty
            }
        );
    }

    [HttpPost("Delete")]
    public ActionResult<ScreenEditorModel> Delete(
        [FromBody] ScreenEditorDeleteItemModel input)
    {
        EditorData editor = editorService.OpenDefaultEditor(input.EditorSchemaItemId);
        if (editor.Item is FormControlSet screenSection)
        {
            designerService.DeleteItem(input.SchemaItemIds, screenSection);
            editor.IsDirty = true;
            var editorData = designerService.GetScreenEditorData(screenSection);
            return new ScreenEditorModel
            {
                Data = editorData,
                IsDirty = true
            };
        }

        return BadRequest(
            $"item id: {input.EditorSchemaItemId} is not a PanelControlSet");
    }

    [HttpPost("CreateItem")]
    public ActionResult<ScreenEditorItem> CreateItem(
        [FromBody] ScreenEditorItemModel itemModelData)
    {
        EditorData editor =
            editorService.OpenDefaultEditor(itemModelData.EditorSchemaItemId);
        ISchemaItem item = editor.Item;
        if (item is FormControlSet screenSection)
        {
            ScreenEditorItem newItem =
                designerService.CreateNewItem(itemModelData, screenSection);
            editor.IsDirty = true;
            return Ok(newItem);
        }

        return BadRequest(
            $"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet");
    }

    [HttpGet("GetSections")]
    public ActionResult<Dictionary<Guid, ApiControl>> GetSections(
        [FromQuery(Name = "sectionIds[]")] Guid[] sectionIds, [FromQuery] Guid editorSchemaItemId)
    {
        EditorData editor =
            editorService.OpenDefaultEditor(editorSchemaItemId);
        ISchemaItem item = editor.Item;
        if (item is FormControlSet screenSection)
        {
            return designerService.LoadSections(screenSection, sectionIds);
        }

        return BadRequest(
            $"item id: {editorSchemaItemId} is not a PanelControlSet");

    }
}