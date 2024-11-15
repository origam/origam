using System.Reflection;
using Origam.Architect.Server.ReturnModels;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;

namespace Origam.Architect.Server.Services;

public class PropertyEditorService(EditorPropertyFactory propertyFactory)
{
    private IEnumerable<EditorProperty> GetEditorPropertiesByName(
        ISchemaItem item, string[] names)
    {
        var properties = item.GetType()
            .GetProperties()
            .Where(prop => names.Contains(prop.Name))
            .Select(prop => propertyFactory.Create(prop, item));
        return properties;
    }

    public List<string> GetRuleErrors(PropertyInfo property, ISchemaItem item)
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

    public IEnumerable<EditorProperty> GetEditorProperties(ISchemaItem item)
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

    public IEnumerable<EditorProperty> GetEditorPropertiesWithErrors(
        ISchemaItem item)
    {
        var editorProperties = GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors = GetRuleErrorsIfExist(property, item);
            });
        return editorProperties;
    }

    public List<string> GetRuleErrorsIfExist(EditorProperty editorProperty,
        ISchemaItem item)
    {
        Type type = item.GetType();
        PropertyInfo[] properties = type.GetProperties();
        PropertyInfo propertyInfo =
            properties.First(property => property.Name == editorProperty.Name);
        return GetRuleErrors(propertyInfo, item);
    }
}