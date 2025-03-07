﻿using System.ComponentModel.DataAnnotations;
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
public class EditorController(
    PropertyEditorService propertyService,
    IPersistenceService persistenceService,
    DesignerEditorService sectionService,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService,
    IWebHostEnvironment environment,
    ILogger<OrigamController> log)
    : OrigamController(environment, log)
{
    [HttpPost("CreateNode")]
    public OpenEditorData CreateNode(
        [Required] [FromBody] NewItemModel input)
    {
        var item =
            editorService.OpenEditorWithNewItem(
                input.NodeId, input.NewTypeName).Item;
        
        TreeNode treeNode = treeNodeFactory.Create(item);
        return new OpenEditorData
        {
            IsDirty = true,
            IsPersisted = false,
            Node = treeNode,
            Data = GetData(treeNode, item)
        };
    }

    [HttpGet("GetOpenEditors")]
    public IActionResult GetOpenEditors()
    {
        return RunWithErrorHandler(() =>
        {
            var items = editorService
                .GetOpenEditors()
                .Select(editor =>
                {
                    var item = editor.Item;
                    TreeNode treeNode = treeNodeFactory.Create(item);
                    return new OpenEditorData
                    {
                        ParentNodeId =
                            TreeNode.ToTreeNodeId(item.ParentItem),
                        IsPersisted = item.IsPersisted,
                        Node = treeNode,
                        Data = GetData(treeNode, item),
                        IsDirty = editor.IsDirty
                    };
                })
                .ToList();
            return Ok(items);
        });
    }

    [HttpPost("OpenEditor")]
    public IActionResult OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenEditor(input.SchemaItemId);
            ISchemaItem item = editor.Item;
            TreeNode treeNode = treeNodeFactory.Create(item);

            var openEditorData = new OpenEditorData
            {
                IsPersisted = true,
                Node = treeNode,
                Data = GetData(treeNode, item)
            };
            return Ok(openEditorData);
        });
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
        if (item is AbstractControlSet controlSet && controlSet.DataSourceId == Guid.Empty)
        {
            return BadRequest("No Datasource selected can't save");
        }

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