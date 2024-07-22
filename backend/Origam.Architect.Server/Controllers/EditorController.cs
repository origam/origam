using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EditorController : ControllerBase
{
    private readonly IPersistenceService persistenceService;
    private readonly EditorPropertyFactory editorPropertyFactory;

    public EditorController(IPersistenceService persistenceService,
        EditorPropertyFactory editorPropertyFactory)
    {
        this.persistenceService = persistenceService;
        this.editorPropertyFactory = editorPropertyFactory;
    }

    [HttpGet("EditableProperties")]
    public ActionResult<IEnumerable<EditorProperty>> EditableProperties(
        [FromQuery] Guid schemaItemId)
    {
        ISchemaItem item = persistenceService.SchemaProvider
            .RetrieveInstance<ISchemaItem>(schemaItemId);
        if (item == null)
        {
            return NotFound($"SchemaItem with id \"{schemaItemId}\" not found");
        }

        var properties = item.GetType()
            .GetProperties()
            .Select(prop => editorPropertyFactory.Create(prop, item))
            .Where(x => x != null);
        return Ok(properties);
    }
}