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
    public ActionResult<SectionEditorModel> Update([FromBody] SectionEditorChangesModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.SchemaItemId);
        if (tab.Item is not PanelControlSet screenSection)
        {
            return BadRequest($"item id: {input.SchemaItemId} is not a PanelControlSet");
        }
        tab.IsDirty = designerEditorService.Update(screenSection, input);
        var editorData = designerEditorService.GetSectionEditorData(screenSection);
        return Ok(new SectionEditorModel { Data = editorData, IsDirty = tab.IsDirty });
    }

    [HttpPost("Delete")]
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

    [HttpPost("CreateItem")]
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
    public ActionResult<Dictionary<Guid, ApiControl>> Save([FromBody] PersistModel input)
    {
        TabData tabData = tabService.OpenDefaultTab(input.SchemaItemId);
        ISchemaItem item = tabData.Item;
        if (item is PanelControlSet screenSection)
        {
            tabData.IsDirty = designerEditorService.SaveScreenSection(screenSection);
            return Ok();
        }

        return BadRequest($"item id: {input.SchemaItemId} is not a PanelControlSet");
    }
}
