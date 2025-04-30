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
    EditorService editorService)
    : ControllerBase
{
    [HttpPost("Update")]
    public ActionResult<SectionEditorModel> Update(
        [FromBody] SectionEditorChangesModel input)
    {
        EditorData editor = editorService.OpenEditor(input.SchemaItemId);
        if (editor.Item is not PanelControlSet screenSection)
        {
            return BadRequest(
                $"item id: {input.SchemaItemId} is not a PanelControlSet");
        }
        editor.IsDirty = designerEditorService.Update(screenSection, input);
        var editorData = designerEditorService.GetSectionEditorData(screenSection);
        return Ok(
            new SectionEditorModel
            {
                Data = editorData,
                IsDirty = editor.IsDirty
            }
        );
    }
    
    [HttpPost("Delete")]
    public ActionResult<SectionEditorModel> Delete(
        [FromBody] ScreenEditorDeleteItemModel input)
    {
        EditorData editor = editorService.OpenEditor(input.EditorSchemaItemId);
        if (editor.Item is PanelControlSet screenSection)
        {
            designerEditorService.DeleteItem(input.SchemaItemIds, screenSection);
            editor.IsDirty = true;
            var editorData = designerEditorService.GetSectionEditorData(screenSection);
            return new SectionEditorModel
            {
                Data = editorData,
                IsDirty = true
            };
        }

        return BadRequest(
            $"item id: {input.EditorSchemaItemId} is not a PanelControlSet");
    }
    
    [HttpPost("CreateItem")]
    public ActionResult<ApiControl> CreateItem(
        [FromBody] SectionEditorItemModel itemModelData)
    {
        EditorData editor = editorService.OpenEditor(itemModelData.EditorSchemaItemId);
        ISchemaItem item = editor.Item;
        if (item is PanelControlSet screenSection)
        {
            ApiControl apiControl = designerEditorService.CreateNewItem(itemModelData, screenSection);
            editor.IsDirty = true;
            return Ok(apiControl);
        }
        return BadRequest(
            $"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet");
    }
}