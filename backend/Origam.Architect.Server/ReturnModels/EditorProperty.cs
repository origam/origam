using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Extensions;
using Origam.Extensions;
using Origam.Schema;
using Origam.UI;

namespace Origam.Architect.Server.Controllers;

public class EditorProperty(
    string Name,
    string Type,
    object Value,
    string Category,
    string Description)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public object Value { get; } = Value;
    public string Category { get; } = Category;
    public string Description { get; } = Description;
}

public class EditorPropertyFactory
{
    public EditorProperty Create(PropertyInfo property, ISchemaItem node)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        if (category == null || !CanBeEdited(property))
        {
            return null;
        }

        string description = property.GetAttribute<DescriptionAttribute>()?.Description;

        return new EditorProperty(
            Name: property.Name, 
            Type: property.PropertyType.Name,
            Value: property.GetValue(node),
            Category: category,
            Description: description);
    }
    
    private bool CanBeEdited(PropertyInfo property)
    {        
        var browsableAttribute = property.GetAttribute<BrowsableAttribute>();
        if (browsableAttribute == null)
        {
            return true;
        }

        return browsableAttribute.Browsable;
    }
}