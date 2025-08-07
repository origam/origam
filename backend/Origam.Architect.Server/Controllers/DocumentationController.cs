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
[Route("[controller]")]
public class DocumentationController(
    TreeNodeFactory treeNodeFactory,
    EditorService editorService,
    IWebHostEnvironment environment,
    IDocumentationService documentationService,
    DocumentationHelperService documentationHelper,
    ILogger<OrigamController> log
) : OrigamController(log, environment)
{
    [HttpPost("OpenEditor")]
    public IActionResult OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenDocumentationEditor(
                input.SchemaItemId
            );
            ISchemaItem item = editor.Item;
            TreeNode treeNode = treeNodeFactory.Create(item);

            editor.DocumentationData = documentationService.LoadDocumentation(
                item.Id
            );
            var openEditorData = new OpenEditorData(
                editorId: editor.Id,
                isPersisted: true,
                node: treeNode,
                data: documentationHelper.GetData(
                    editor.DocumentationData,
                    item.Name
                )
            );
            return Ok(openEditorData);
        });
    }

    [HttpPost("Update")]
    public IActionResult Update([FromBody] ChangesModel changes)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenDocumentationEditor(
                changes.SchemaItemId
            );
            documentationHelper.Update(changes, editor);

            return Ok(new UpdatePropertiesResult { IsDirty = editor.IsDirty });
        });
    }

    [HttpPost("PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenDocumentationEditor(
                input.SchemaItemId
            );
            documentationService.SaveDocumentation(
                editor.DocumentationData,
                input.SchemaItemId
            );
            editor.IsDirty = false;
            return Ok();
        });
    }
}
