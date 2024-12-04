using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PropertyEditorController(
    PropertyEditorService propertyService,
    EditorService editorService)
    : ControllerBase
{
    [HttpPost("Update")]
    public UpdatePropertiesResult Update(
        [FromBody] ChangesModel changes)
    {
        EditorData editor = editorService.ChangesToEditorData(changes);
        PropertyInfo[] properties = editor.Item
            .GetType()
            .GetProperties();
        IEnumerable<PropertyUpdate> propertyUpdates = propertyService
            .GetEditorProperties(editor.Item)
            .Select(editorProperty =>
            {
                PropertyInfo property = properties
                    .FirstOrDefault(x => x.Name == editorProperty.Name);
                return new PropertyUpdate
                {
                    PropertyName = editorProperty.Name,
                    Errors =
                        propertyService.GetRuleErrors(property, editor.Item),
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
}