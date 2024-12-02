using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;


[ApiController]
[Route("[controller]")]
public class ScreenEditorController(
    IPersistenceService persistenceService,
    PropertyEditorService propertyService,
    ScreenSectionEditorService sectionService,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService)
    : ControllerBase
{
    [HttpPost("Update")]
    public ActionResult<SectionEditorModel> Update(
        [FromBody] SectionEditorChangesModel input)
    {
        ISchemaItem editorItem = editorService.OpenEditor(input.SchemaItemId);
        if (editorItem is PanelControlSet screenSection)
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
                        valueItem.Value = propertyChange.Value;
                    }
                }
            }
            return Ok(sectionService.GetSectionEditorData(screenSection));
        }

        return BadRequest(
            $"item id: {input.SchemaItemId} is not a PanelControlSet");
    }
    
    [HttpPost("CreateItem")]
    public ActionResult<ApiControl> CreateItem(
        [FromBody] ScreenEditorItem itemData)
    {
        ISchemaItem editorItem = editorService.OpenEditor(itemData.EditorSchemaItemId);
        if (editorItem is PanelControlSet screenSection)
        {
            ApiControl apiControl = sectionService.CreateNewItem(itemData, screenSection);
            return Ok(apiControl);
        }
        return BadRequest(
            $"item id: {itemData.EditorSchemaItemId} is not a PanelControlSet");
    }
}