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
    ScreenSectionEditorService sectionService,
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
        SectionEditorModel resultModel = sectionService.Update(screenSection, input);
        editor.IsDirty = resultModel.IsDirty;
        return resultModel;
    }
    
    [HttpPost("DeleteItem")]
    public ActionResult<SectionEditorData> DeleteItem(
        [FromBody] ScreenEditorDeleteItemModel input)
    {
        EditorData editor = editorService.OpenEditor(input.EditorSchemaItemId);
        if (editor.Item is PanelControlSet screenSection)
        {
            sectionService.DeleteItem(input.SchemaItemId, screenSection);
            editor.IsDirty = true;
            return Ok(new RootControlModel
            {
                RootControl = sectionService.LoadRootApiControl(screenSection)
            });
        }

        return BadRequest(
            $"item id: {input.EditorSchemaItemId} is not a PanelControlSet");
    }
    
    [HttpPost("CreateItem")]
    public ActionResult<ApiControl> CreateItem(
        [FromBody] ScreenEditorItemModel itemModelData)
    {
        EditorData editor = editorService.OpenEditor(itemModelData.EditorSchemaItemId);
        ISchemaItem item = editor.Item;
        if (item is PanelControlSet screenSection)
        {
            ApiControl apiControl = sectionService.CreateNewItem(itemModelData, screenSection);
            editor.IsDirty = true;
            return Ok(apiControl);
        }
        return BadRequest(
            $"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet");
    }
}