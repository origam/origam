using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EditorController(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    EditorPropertyFactory propertyFactory,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService,
    PropertyParser propertyParser)
    : ControllerBase
{
    private readonly IPersistenceProvider persistenceProvider =
        persistenceService.SchemaProvider;

    [HttpPost("CreateNew")]
    public NewEditorData CreateNew(
        [Required] [FromBody] NewItemModel input)
    {
        var item = editorService.CreateSchemaItem(input.NodeId, input.NewTypeName);

        var editorProperties = GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors = GetRuleErrorsIfExist(property, item);
            });
        return new NewEditorData
        {
            Node = treeNodeFactory.Create(item),
            Properties = editorProperties
        };
    }
    
    private List<string> GetRuleErrorsIfExist(EditorProperty editorProperty,
        ISchemaItem item)
    {
        Type type = item.GetType();
        PropertyInfo[] properties = type.GetProperties();
        PropertyInfo propertyInfo =
            properties.First(property => property.Name == editorProperty.Name);
        return GetRuleErrors(propertyInfo, item)?.Errors;
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

        return Ok(GetEditorProperties(item));
    }

    private IEnumerable<EditorProperty> GetEditorProperties(
        ISchemaItem item)
    {
        if (item is XslTransformation xsltTransformation)
        {
            var xsltProperties =
                GetEditorPropertiesByName(
                    xsltTransformation,
                    new[] { "Id", "Package", "TextStore", "XsltEngineType" }
                );
            return xsltProperties;
        }

        if (item is XslRule xslRule)
        {
            var xsltProperties =
                GetEditorPropertiesByName(xslRule, new[] { "Xsl" });
            return xsltProperties;
        }

        var properties = item.GetType()
            .GetProperties()
            .Select(
                prop => propertyFactory.CreateIfMarkedAsEditable(prop, item))
            .Where(x => x != null);
        return properties;
    }

    [HttpPost("CheckRules")]
    public IEnumerable<RuleErrors> CheckRules([FromBody] ChangesModel changes)
    {
        return editorService.ChangesToSchemaItem(changes)
            .GetType()
            .GetProperties()
            .Select(property =>
                GetRuleErrors(property, editorService.ChangesToSchemaItem(changes)))
            .Where(errors => errors != null);
    }

    private RuleErrors GetRuleErrors(PropertyInfo property, ISchemaItem item)
    {
        List<string> ruleErrors = property.GetCustomAttributes()
            .OfType<IModelElementRule>()
            .Select(rule => rule.CheckRule(item, property.Name)?.Message)
            .Where(message => message != null)
            .ToList();
        return ruleErrors.Count == 0
            ? null
            : new RuleErrors { Name = property.Name, Errors = ruleErrors };
    }

    [HttpPost("PersistChanges")]
    public ActionResult PersistChanges([FromBody] ChangesModel input)
    {
        ISchemaItem item = editorService.ChangesToSchemaItem(input);
        persistenceService.SchemaProvider.Persist(item);
        return Ok();
    }

  
    private IEnumerable<EditorProperty> GetEditorPropertiesByName(
        ISchemaItem item, string[] names)
    {
        var properties = item.GetType()
            .GetProperties()
            .Where(prop => names.Contains(prop.Name))
            .Select(prop => propertyFactory.Create(prop, item));
        return properties;
    }
}