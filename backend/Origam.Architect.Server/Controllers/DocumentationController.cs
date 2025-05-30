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
    ILogger<OrigamController> log)
    : OrigamController(log, environment)
{
  
    [HttpPost("OpenEditor")]
    public IActionResult OpenEditor([Required] [FromBody] OpenEditorModel input)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenDocumentationEditor(input.SchemaItemId);
            ISchemaItem item = editor.Item;
            TreeNode treeNode = treeNodeFactory.Create(item);

            editor.DocumentationData = documentationService.LoadDocumentation(item.Id);
            var openEditorData = new OpenEditorData
            (
                editorId: editor.Id,
                isPersisted: true,
                node: treeNode,
                data: documentationHelper.GetData(editor.DocumentationData)
            );
            return Ok(openEditorData);
        });
    }

    [HttpPost("Update")]
    public IActionResult Update(
        [FromBody] ChangesModel changes)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenDocumentationEditor(changes.SchemaItemId);
            documentationHelper.Update(changes, editor);

            return Ok( 
                new UpdatePropertiesResult
                {
                    IsDirty = editor.IsDirty
                }
            );
        });
    }
    
    [HttpPost("PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        return RunWithErrorHandler(() =>
        {
            EditorData editor = editorService.OpenDocumentationEditor(input.SchemaItemId);
            documentationService.SaveDocumentation(editor.DocumentationData, input.SchemaItemId);
            editor.IsDirty = false;
            return Ok();
        });
    }
}