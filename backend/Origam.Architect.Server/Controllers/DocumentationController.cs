using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using static Origam.Workbench.Services.DocumentationComplete;

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
            foreach (PropertyChange propertyChange in changes.Changes)
            {
                DocumentationDataTable table = editor.DocumentationData.Documentation;
                DocumentationRow row = table.Rows
                    .Cast<DocumentationRow>()
                    .FirstOrDefault(row => row.Category == propertyChange.Name);
                if (string.IsNullOrEmpty(propertyChange.Value))
                {
                    if (row != null)
                    {
                        table.RemoveDocumentationRow(row);
                        editor.IsDirty = true;
                    }

                    continue;
                }

                if (row == null)
                {
                    row = table.NewDocumentationRow();
                    row.Category = propertyChange.Name;
                    row.Data = propertyChange.Value;
                    row.refSchemaItemId = changes.SchemaItemId;
                    row.Id = Guid.NewGuid();
                    table.Rows.Add(row);
                }
                else
                {
                    row.Data = propertyChange.Value;
                }

                editor.IsDirty = true;
            }

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