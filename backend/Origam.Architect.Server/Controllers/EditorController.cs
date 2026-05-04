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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route(template: "[controller]")]
public class EditorController(
    PropertyEditorService propertyService,
    IPersistenceService persistenceService,
    DesignerEditorService sectionService,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService,
    DocumentationHelperService documentationHelper,
    ILogger<OrigamController> log
) : OrigamController(log: log)
{
    [HttpPost(template: "CreateNode")]
    public OpenEditorData CreateNode([Required] [FromBody] NewItemModel input)
    {
        var editor = editorService.OpenEditorWithNewItem(
            parentId: input.NodeId,
            fullTypeName: input.NewTypeName
        );

        TreeNode treeNode = treeNodeFactory.Create(node: editor.Item);
        return new OpenEditorData(
            editorId: editor.Id,
            node: treeNode,
            data: GetData(treeNode: treeNode, item: editor.Item),
            isPersisted: false,
            parentNodeId: null,
            isDirty: true
        );
    }

    [HttpGet(template: "GetOpenEditors")]
    public IActionResult GetOpenEditors()
    {
        var items = editorService
            .GetOpenEditors()
            .Select(selector: editor =>
            {
                var item = editor.Item;
                TreeNode treeNode = treeNodeFactory.Create(node: item);

                return editor.Id.Type switch
                {
                    EditorType.Default => new OpenEditorData(
                        editorId: editor.Id,
                        node: treeNode,
                        data: GetData(treeNode: treeNode, item: item),
                        isPersisted: item.IsPersisted,
                        parentNodeId: TreeNode.ToTreeNodeId(node: item.ParentItem),
                        isDirty: editor.IsDirty
                    ),
                    EditorType.DocumentationEditor => new OpenEditorData(
                        editorId: editor.Id,
                        node: treeNode,
                        data: documentationHelper.GetData(
                            documentationComplete: editor.DocumentationData,
                            label: item.Name
                        ),
                        isPersisted: item.IsPersisted,
                        isDirty: editor.IsDirty
                    ),
                    _ => throw new Exception(message: "Unknown editor type: " + editor.Id.Type),
                };
            })
            .ToList();
        return Ok(value: items);
    }

    [HttpPost(template: "OpenEditor")]
    public IActionResult OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        EditorData editor = editorService.OpenDefaultEditor(schemaItemId: input.SchemaItemId);
        ISchemaItem item = editor.Item;
        TreeNode treeNode = treeNodeFactory.Create(node: item);

        var openEditorData = new OpenEditorData(
            editorId: editor.Id,
            node: treeNode,
            data: GetData(treeNode: treeNode, item: item),
            isPersisted: true
        );
        return Ok(value: openEditorData);
    }

    private object GetData(TreeNode treeNode, ISchemaItem item)
    {
        object data = treeNode.DefaultEditor switch
        {
            EditorSubType.GridEditor => propertyService.GetEditorProperties(item: item),
            EditorSubType.DeploymentScriptsEditor => propertyService.GetEditorProperties(
                item: item
            ),
            EditorSubType.XsltEditor => propertyService.GetEditorProperties(item: item),
            EditorSubType.ScreenSectionEditor => sectionService.GetSectionEditorData(
                editedItem: item
            ),
            EditorSubType.ScreenEditor => sectionService.GetScreenEditorData(editedItem: item),
            _ => null,
        };
        return data;
    }

    [HttpPost(template: "CloseEditor")]
    public IActionResult CloseEditor([Required] [FromBody] CloseEditorModel input)
    {
        editorService.CloseEditor(editorId: input.GetTypedEditorId());
        return Ok();
    }

    [HttpPost(template: "PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        EditorData editorData = editorService.OpenDefaultEditor(schemaItemId: input.SchemaItemId);
        ISchemaItem item = editorData.Item;
        if (item is AbstractControlSet controlSet && controlSet.DataSourceId == Guid.Empty)
        {
            return BadRequest(error: "No Datasource selected can't save");
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
