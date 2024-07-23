using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Extensions;
using Origam.Architect.Server.Utils;
using Origam.Extensions;
using Origam.Schema;
using Origam.UI;

namespace Origam.Architect.Server.Controllers;

public class EditorProperty(
    string Name,
    string Type,
    object Value,
    string Category,
    string Description,
    bool ReadOnly)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public object Value { get; } = Value;
    public string Category { get; } = Category;
    public string Description { get; } = Description;
    public bool ReadOnly { get; } = ReadOnly;
}

public class EditorPropertyFactory
{
    public EditorProperty Create(PropertyInfo property, ISchemaItem item)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        if (category == null || !PropertyUtils.CanBeEdited(property))
        {
            return null;
        }

        string description = property.GetAttribute<DescriptionAttribute>()?.Description;

        object value = property.GetValue(item);
        object editorValue = value;
        if (value is ISchemaItem schemaItem)
        {
            editorValue = schemaItem.Id;
        }

        return new EditorProperty(
            Name: property.Name, 
            Type: property.PropertyType.Name,
            Value: editorValue,
            Category: category,
            Description: description,
            ReadOnly: property.GetSetMethod() == null);
    }
}