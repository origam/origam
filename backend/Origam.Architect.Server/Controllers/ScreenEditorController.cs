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
        if (editor.Item is PanelControlSet screenSection)
        {
            screenSection.Name = input.Name;
            screenSection.DataSourceId = input.SelectedDataSourceId;
            foreach (var changes in input.ModelChanges)
            {
                ControlSetItem itemToUpdate = screenSection.PanelControl.PanelControlSet.GetChildByIdRecursive(changes.SchemaItemId) as ControlSetItem;
                if (itemToUpdate == null)
                {
                    return BadRequest(
                        $"item id: {changes.SchemaItemId} is not in the PanelControlSet");
                }
                foreach (var propertyChange in changes.Changes)
                {
                    PropertyValueItem valueItem = itemToUpdate.ChildItems
                        .OfType<PropertyValueItem>()
                        .FirstOrDefault(item =>
                            item.ControlPropertyId ==
                            propertyChange.ControlPropertyId);
                    if (valueItem != null)
                    {
                        if (valueItem.Value != propertyChange.Value)
                        {
                            editor.IsDirty = true;
                        }
                        valueItem.Value = propertyChange.Value;
                    }
                }
            }

            SectionEditorData editorData = sectionService.GetSectionEditorData(screenSection);
            return Ok(new SectionEditorModel
            {
                Data = editorData,
                IsDirty = editor.IsDirty
            });
        }

        return BadRequest(
            $"item id: {input.SchemaItemId} is not a PanelControlSet");
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
        ISchemaItem editor = editorService.OpenEditor(itemModelData.EditorSchemaItemId).Item;
        if (editor is PanelControlSet screenSection)
        {
            ApiControl apiControl = sectionService.CreateNewItem(itemModelData, screenSection);
            return Ok(apiControl);
        }
        return BadRequest(
            $"item id: {itemModelData.EditorSchemaItemId} is not a PanelControlSet");
    }
}