using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Extensions;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EditorController(
    PropertyEditorService propertyService,
    IPersistenceService persistenceService,
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
            EditorType.ScreenEditor => sectionService
                .GetScreenEditorData(item),
            _ => null
        };
        return data;
    }

    [HttpPost("CloseEditor")]
    public void CloseEditor([Required] [FromBody] CloseEditorModel input)
    {
        editorService.CloseEditor(input.SchemaItemId);
    }
    
    [HttpPost("PersistChanges")]
    public ActionResult PersistChanges([FromBody] PersistModel input)
    {
        EditorData editorData = editorService.OpenEditor(input.SchemaItemId);
        ISchemaItem item = editorData.Item;
        try
        {
            persistenceService.SchemaProvider.BeginTransaction();
            item.Persist();
            editorData.IsDirty = false;
            return Ok();
        }
        finally
        {
            persistenceService.SchemaProvider.EndTransaction();
        }
    }
}