using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
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
    PropertyParser propertyParser)
    : ControllerBase
{
    private readonly IPersistenceProvider persistenceProvider =
        persistenceService.SchemaProvider;

    [HttpPost("CreateNew")]
    public ActionResult<IEnumerable<EditorProperty>> CreateNew(
        [Required] [FromBody] NewItemModel input)
    {
        IBrowserNode2 parentItem = persistenceProvider
            .RetrieveInstance<IBrowserNode2>(input.NodeId);
        var factory = (ISchemaItemFactory)parentItem;

        Type newItemType = Reflector.GetTypeByName(input.NewTypeName);
        object result = factory
            .GetType()
            .GetMethod("NewItem")
            .MakeGenericMethod(newItemType)
            .Invoke(factory,
                new object[] { schemaService.ActiveSchemaExtensionId, null });

        ISchemaItem item = (ISchemaItem)result;

        return GetEditorProperties(item);
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

        return GetEditorProperties(item);
    }

    private ActionResult<IEnumerable<EditorProperty>> GetEditorProperties(
        ISchemaItem item)
    {
        if (item is XslTransformation xsltTransformation)
        {
            var xsltProperties =
                GetEditorPropertiesByName(
                    xsltTransformation,
                    new[] { "Id", "Package", "TextStore", "XsltEngineType" }
                );
            return Ok(xsltProperties);
        }

        if (item is XslRule xslRule)
        {
            var xsltProperties =
                GetEditorPropertiesByName(xslRule, new[] { "Xsl" });
            return Ok(xsltProperties);
        }

        var properties = item.GetType()
            .GetProperties()
            .Select(
                prop => propertyFactory.CreateIfMarkedAsEditable(prop, item))
            .Where(x => x != null);
        return Ok(properties);
    }

    [HttpPost("CheckRules")]
    public IEnumerable<RuleErrors> CheckRules([FromBody] ChangesModel changes)
    {
        ISchemaItem item = ChangesToSchemaItem(changes);
        return item.GetType()
            .GetProperties()
            .Select(property =>
            {
                List<string> ruleErrors = GetRuleErrors(item, property);
                if (ruleErrors.Count == 0)
                {
                    return null;
                }

                return new RuleErrors
                {
                    Name = property.Name,
                    Errors = ruleErrors
                };
            })
            .Where(errors => errors != null);
    }

    private List<string> GetRuleErrors( ISchemaItem item, PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttributes()
            .OfType<IModelElementRule>()
            .Select(rule => rule.CheckRule(item, propertyInfo.Name)?.Message)
            .Where(message => message != null)
            .ToList();
    }
    
    [HttpPost("PersistChanges")]
    public ActionResult PersistChanges([FromBody] ChangesModel input)
    {
        ISchemaItem item = ChangesToSchemaItem(input);
        persistenceService.SchemaProvider.Persist(item);
        return Ok();
    }

    private ISchemaItem ChangesToSchemaItem(ChangesModel input)
    {
        ISchemaItem item = persistenceService.SchemaProvider
            .RetrieveInstance<ISchemaItem>(input.SchemaItemId, false);

        PropertyInfo[] properties = item.GetType().GetProperties();
        foreach (var change in input.Changes)
        {
            PropertyInfo propertyToChange = properties
                .FirstOrDefault(prop => prop.Name == change.Name);

            if (propertyToChange == null)
            {
                throw new Exception(
                    $"Property {change.Name} not found on type {item.GetType().Name}");
            }

            object value = propertyParser.Parse(propertyToChange, change.Value);
            propertyToChange.SetValue(item, value);
        }

        return item;
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