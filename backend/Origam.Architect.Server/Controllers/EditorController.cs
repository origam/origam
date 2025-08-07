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
[Route("[controller]")]
public class EditorController(
    PropertyEditorService propertyService,
    IPersistenceService persistenceService,
    DesignerEditorService sectionService,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService,
    DocumentationHelperService documentationHelper,
    IWebHostEnvironment environment,
    ILogger<OrigamController> log
) : OrigamController(log, environment)
{
    [HttpPost("CreateNode")]
    public OpenEditorData CreateNode([Required] [FromBody] NewItemModel input)
    {
        var editor = editorService.OpenEditorWithNewItem(
            input.NodeId,
            input.NewTypeName
        );

        TreeNode treeNode = treeNodeFactory.Create(editor.Item);
        return new OpenEditorData(
            editorId: editor.Id,
            node: treeNode,
            data: GetData(treeNode, editor.Item),
            isPersisted: false,
            parentNodeId: null,
            isDirty: true
        );
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

                    return editor.Id.Type switch
                    {
                        EditorType.Default => new OpenEditorData(
                            editorId: editor.Id,
                            node: treeNode,
                            data: GetData(treeNode, item),
                            isPersisted: item.IsPersisted,
                            parentNodeId: TreeNode.ToTreeNodeId(
                                item.ParentItem
                            ),
                            isDirty: editor.IsDirty
                        ),
                        EditorType.DocumentationEditor => new OpenEditorData(
                            editorId: editor.Id,
                            isPersisted: item.IsPersisted,
                            node: treeNode,
                            isDirty: editor.IsDirty,
                            data: documentationHelper.GetData(
                                editor.DocumentationData,
                                item.Name
                            )
                        ),
                        _ => throw new Exception(
                            "Unknown editor type: " + editor.Id.Type
                        ),
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
            EditorData editor = editorService.OpenDefaultEditor(
                input.SchemaItemId
            );
            ISchemaItem item = editor.Item;
            TreeNode treeNode = treeNodeFactory.Create(item);

            var openEditorData = new OpenEditorData(
                editorId: editor.Id,
                node: treeNode,
                data: GetData(treeNode, item),
                isPersisted: true
            );
            return Ok(openEditorData);
        });
    }

    private object GetData(TreeNode treeNode, ISchemaItem item)
    {
        object data = treeNode.DefaultEditor switch
        {
            EditorSubType.GridEditor => propertyService.GetEditorProperties(
                item
            ),
            EditorSubType.XsltEditor => propertyService.GetEditorProperties(
                item
            ),
            EditorSubType.ScreenSectionEditor =>
                sectionService.GetSectionEditorData(item),
            EditorSubType.ScreenEditor => sectionService.GetScreenEditorData(
                item
            ),
            _ => null,
        };
        return data;
    }

    [HttpPost("CloseEditor")]
    public void CloseEditor([Required] [FromBody] CloseEditorModel input)
    {
        RunWithErrorHandler(() =>
        {
            editorService.CloseEditor(input.GetTypedEditorId());
            return Ok();
        });
    }

    [HttpPost("PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editorData = editorService.OpenDefaultEditor(
                input.SchemaItemId
            );
            ISchemaItem item = editorData.Item;
            if (
                item is AbstractControlSet controlSet
                && controlSet.DataSourceId == Guid.Empty
            )
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
        });
    }
}
