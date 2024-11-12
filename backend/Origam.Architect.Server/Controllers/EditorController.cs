using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EditorController(
    IPersistenceService persistenceService,
    EditorPropertyFactory propertyFactory,
    TreeNodeFactory treeNodeFactory,
    EditorService editorService)
    : ControllerBase
{
    [HttpPost("CreateNew")]
    public EditorData CreateNew(
        [Required] [FromBody] NewItemModel input)
    {
        var item =
            editorService.OpenEditorWithNewItem(input.NodeId,
                input.NewTypeName);

        var editorProperties = GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors = GetRuleErrorsIfExist(property, item);
            });
        return new EditorData
        {
            IsPersisted = false,
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
        return GetRuleErrors(propertyInfo, item);
    }

    [HttpGet("GetOpenEditors")]
    public IEnumerable<EditorData> GetOpenEditors()
    {
        var items = editorService
            .GetOpenEditors()
            .Select(item => new EditorData
            {
                ParentNodeId = TreeNode.ToTreeNodeId(item.ParentItem),
                IsPersisted = item.IsPersisted,
                Node = treeNodeFactory.Create(item),
                Properties = GetEditorPropertiesWithErrors(item)
            });
        return items;
    }

    [HttpPost("OpenEditor")]
    public ActionResult<IEnumerable<EditorProperty>> OpenEditor(
        [Required] [FromBody] OpenEditorModel input)
    {
        ISchemaItem item = editorService.OpenEditor(input.SchemaItemId);
        return Ok(GetEditorProperties(item));
    }

    [HttpPost("CloseEditor")]
    public void CloseEditor([Required] [FromBody] CloseEditorModel input)
    {
        editorService.CloseEditor(input.SchemaItemId);
    }

    private IEnumerable<EditorProperty> GetEditorPropertiesWithErrors(
        ISchemaItem item)
    {
        var editorProperties = GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors = GetRuleErrorsIfExist(property, item);
            });
        return editorProperties;
    }

    private IEnumerable<EditorProperty> GetEditorProperties(ISchemaItem item)
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

    [HttpPost("UpdateProperties")]
    public IEnumerable<PropertyUpdate> UpdateProperties(
        [FromBody] ChangesModel changes)
    {
        ISchemaItem item = editorService.ChangesToSchemaItem(changes);
        PropertyInfo[] properties = item
            .GetType()
            .GetProperties();
        return GetEditorProperties(item)
            .Select(editorProperty =>
            {
                PropertyInfo property = properties
                    .FirstOrDefault(x => x.Name == editorProperty.Name);
                return new PropertyUpdate
                {
                    PropertyName = editorProperty.Name,
                    Errors = GetRuleErrors(property, item),
                    DropDownValues = editorProperty
                        .DropDownValues ?? Array.Empty<DropDownValue>()
                };
            });
    }

    private List<string> GetRuleErrors(PropertyInfo property, ISchemaItem item)
    {
        List<string> ruleErrors = property.GetCustomAttributes()
            .OfType<IModelElementRule>()
            .Select(rule => rule.CheckRule(item, property.Name)?.Message)
            .Where(message => message != null)
            .ToList();
        return ruleErrors.Count == 0
            ? null
            : ruleErrors;
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