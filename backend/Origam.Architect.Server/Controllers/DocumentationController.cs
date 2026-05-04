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
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route(template: "[controller]")]
public class DocumentationController(
    TreeNodeFactory treeNodeFactory,
    EditorService editorService,
    IDocumentationService documentationService,
    DocumentationHelperService documentationHelper,
    ILogger<OrigamController> log
) : OrigamController(log: log)
{
    [HttpPost(template: "OpenEditor")]
    public IActionResult OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        EditorData editor = editorService.OpenDocumentationEditor(schemaItemId: input.SchemaItemId);
        ISchemaItem item = editor.Item;
        TreeNode treeNode = treeNodeFactory.Create(node: item);

        editor.DocumentationData = documentationService.LoadDocumentation(schemaItemId: item.Id);
        var openEditorData = new OpenEditorData(
            editorId: editor.Id,
            node: treeNode,
            data: documentationHelper.GetData(
                documentationComplete: editor.DocumentationData,
                label: item.Name
            ),
            isPersisted: true
        );
        return Ok(value: openEditorData);
    }

    [HttpPost(template: "Update")]
    public IActionResult Update([FromBody] ChangesModel changes)
    {
        EditorData editor = editorService.OpenDocumentationEditor(
            schemaItemId: changes.SchemaItemId
        );
        documentationHelper.Update(changes: changes, editor: editor);

        return Ok(value: new UpdatePropertiesResult { IsDirty = editor.IsDirty });
    }

    [HttpPost(template: "PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        EditorData editor = editorService.OpenDocumentationEditor(schemaItemId: input.SchemaItemId);
        documentationService.SaveDocumentation(
            documentationData: editor.DocumentationData,
            schemaItemId: input.SchemaItemId
        );
        editor.IsDirty = false;
        return Ok();
    }
}
