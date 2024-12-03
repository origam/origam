using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.GuiModel;
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
    public OpenEditorData CreateNode(
        [Required] [FromBody] NewItemModel input)
    {
        var item =
            editorService.OpenEditorWithNewItem(
                input.NodeId, input.NewTypeName).Item;

        var editorProperties = propertyService.GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors =
                    propertyService.GetRuleErrorsIfExist(property, item);
            });
        return new OpenEditorData
        {
            IsPersisted = false,
            Node = treeNodeFactory.Create(item),
            Data = editorProperties
        };
    }

    [HttpGet("GetOpenEditors")]
    public IEnumerable<OpenEditorData> GetOpenEditors()
    {
        var items = editorService
            .GetOpenEditors()
            .Select(editor =>
            {
                var item = editor.Item; 
                TreeNode treeNode = treeNodeFactory.Create(item);
                return new OpenEditorData
                {
                    ParentNodeId = TreeNode.ToTreeNodeId(item.ParentItem),
                    IsPersisted = item.IsPersisted,
                    Node = treeNode,
                    Data = GetData(treeNode, item),
                    IsDirty = editor.IsDirty
                };
            });
        return items;
    }

    [HttpPost("OpenEditor")]
    public OpenEditorData OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        EditorData editor = editorService.OpenEditor(input.SchemaItemId);
        ISchemaItem item = editor.Item;
        TreeNode treeNode = treeNodeFactory.Create(item);

        return new OpenEditorData
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
    public UpdatePropertiesResult UpdateProperties(
        [FromBody] ChangesModel changes)
    {
        EditorData editor = editorService.ChangesToEditorData(changes);
        PropertyInfo[] properties = editor.Item
            .GetType()
            .GetProperties();
        IEnumerable<PropertyUpdate> propertyUpdates = propertyService.GetEditorProperties(editor.Item)
            .Select(editorProperty =>
            {
                PropertyInfo property = properties
                    .FirstOrDefault(x => x.Name == editorProperty.Name);
                return new PropertyUpdate
                {
                    PropertyName = editorProperty.Name,
                    Errors = propertyService.GetRuleErrors(property, editor.Item),
                    DropDownValues = editorProperty
                        .DropDownValues ?? Array.Empty<DropDownValue>()
                };
            });
        return new UpdatePropertiesResult
        {
            PropertyUpdates = propertyUpdates,
            IsDirty = editor.IsDirty
        };
    }

    [HttpPost("PersistChanges")]
    public ActionResult PersistChanges([FromBody] ChangesModel input)
    {
        EditorData editorData = editorService.ChangesToEditorData(input);
        ISchemaItem item = editorData.Item;
        persistenceService.SchemaProvider.Persist(item);
        editorData.IsDirty = false;
        return Ok();
    }
}