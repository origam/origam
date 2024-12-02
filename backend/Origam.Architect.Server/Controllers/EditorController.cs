using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EditorController(
    IPersistenceService persistenceService,
    PropertyEditorService propertyService,
    ScreenSectionEditorService sectionService,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService)
    : ControllerBase
{

    [HttpPost("CreateNode")]
    public EditorData CreateNode(
        [Required] [FromBody] NewItemModel input)
    {
        var item =
            editorService.OpenEditorWithNewItem(input.NodeId,
                input.NewTypeName);

        var editorProperties = propertyService.GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors =
                    propertyService.GetRuleErrorsIfExist(property, item);
            });
        return new EditorData
        {
            IsPersisted = false,
            Node = treeNodeFactory.Create(item),
            Data = editorProperties
        };
    }

    [HttpGet("GetOpenEditors")]
    public IEnumerable<EditorData> GetOpenEditors()
    {
        var items = editorService
            .GetOpenEditors()
            .Select(item =>
            {
                TreeNode treeNode = treeNodeFactory.Create(item);
                return new EditorData
                {
                    ParentNodeId = TreeNode.ToTreeNodeId(item.ParentItem),
                    IsPersisted = item.IsPersisted,
                    Node = treeNode,
                    Data = GetData(treeNode, item)
                };
            });
        return items;
    }

    [HttpPost("OpenEditor")]
    public EditorData OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        ISchemaItem item = editorService.OpenEditor(input.SchemaItemId);
        TreeNode treeNode = treeNodeFactory.Create(item);

        return new EditorData
        {
            IsPersisted = true,
            Node = treeNode,
            Data = GetData(treeNode, item)
        };
    }

    private object GetData(TreeNode treeNode, ISchemaItem item)
    {
        object data = treeNode.EditorType switch
        {
            EditorType.GridEditor => propertyService.GetEditorProperties(item),
            EditorType.XslTEditor => propertyService.GetEditorProperties(item),
            EditorType.ScreenSectionEditor => sectionService
                .GetSectionEditorData(item),
            _ => null
        };
        return data;
    }

    [HttpPost("CloseEditor")]
    public void CloseEditor([Required] [FromBody] CloseEditorModel input)
    {
        editorService.CloseEditor(input.SchemaItemId);
    }

    [HttpPost("UpdateProperties")]
    public IEnumerable<PropertyUpdate> UpdateProperties(
        [FromBody] ChangesModel changes)
    {
        ISchemaItem item = editorService.ChangesToSchemaItem(changes);
        PropertyInfo[] properties = item
            .GetType()
            .GetProperties();
        return propertyService.GetEditorProperties(item)
            .Select(editorProperty =>
            {
                PropertyInfo property = properties
                    .FirstOrDefault(x => x.Name == editorProperty.Name);
                return new PropertyUpdate
                {
                    PropertyName = editorProperty.Name,
                    Errors = propertyService.GetRuleErrors(property, item),
                    DropDownValues = editorProperty
                        .DropDownValues ?? Array.Empty<DropDownValue>()
                };
            });
    }

    [HttpPost("UpdateScreenEditor")]
    public ActionResult<SectionEditorModel> UpdateScreenEditor(
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
    
    [HttpPost("CreateScreenEditorItem")]
    public ActionResult<ApiControl> CreateScreenEditorItem(
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

    [HttpPost("PersistChanges")]
    public ActionResult PersistChanges([FromBody] ChangesModel input)
    {
        ISchemaItem item = editorService.ChangesToSchemaItem(input);
        persistenceService.SchemaProvider.Persist(item);
        return Ok();
    }
}