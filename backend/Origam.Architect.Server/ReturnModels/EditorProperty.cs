using System.Collections;
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
    DropDownValue[] DropDownValues,
    string Category,
    string Description,
    bool ReadOnly)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public object Value { get; } = Value;
    public DropDownValue[] DropDownValues { get; } = DropDownValues;
    public string Category { get; } = Category;
    public string Description { get; } = Description;
    public bool ReadOnly { get; } = ReadOnly;
}

public class DropDownValue(string Name, object Value)
{
    public string Name { get; } = Name;
    public object Value { get; } = Value;
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
            Type: ToPropertyTypeName(property.PropertyType),
            Value: editorValue,
            DropDownValues: GetAvailableValues(property, item),
            Category: category,
            Description: description,
            ReadOnly: property.GetSetMethod() == null);
    }

    private DropDownValue[] GetAvailableValues(PropertyInfo property,
        ISchemaItem item)
    {
        if (property.PropertyType == typeof(string) ||
            property.PropertyType == typeof(Guid) ||
            property.PropertyType == typeof(int) ||
            property.PropertyType == typeof(long) ||
            property.PropertyType == typeof(decimal) ||
            property.PropertyType == typeof(double) ||
            property.PropertyType == typeof(float) ||
            property.PropertyType == typeof(bool))
        {
            return [];
        }

        if (property.PropertyType.IsEnum)
        {
            return Enum
                .GetValues(property.PropertyType)
                .Cast<object>()
                .Select(x => new DropDownValue(x.ToString(), x))
                .ToArray();
        }

        var converterType = property
            .GetAttribute<TypeConverterAttribute>()?.ConverterTypeName;
        if (converterType == null)
        {
            return [];
        }
        
        Type type = Type.GetType(converterType);
        if (type == null)
        {
            throw new Exception($"Could not find type {converterType}");
        }

        object converterInstance = Activator.CreateInstance(type);
        MethodInfo getValuesMethod = type.GetMethod("GetStandardValues",
            new Type[] { typeof(ITypeDescriptorContext) })!;
        var context = new Context(item);
        var values = getValuesMethod.Invoke(converterInstance, new object[] { context }) as TypeConverter.StandardValuesCollection;
        if (values == null || values.Count == 0)
        {
            return [];
        }
        return values.Cast<ISchemaItem>()
            .Select(x => new DropDownValue(x?.Name ?? "", x?.Id))
            .ToArray();
    }
    
    private string ToPropertyTypeName(Type type)
    {
        if (type == typeof(bool))
        {
            return "boolean";
        }
        if (type.IsAssignableTo(typeof(ISchemaItem)))
        {
            return "looukup";
        }
        if (type.IsEnum)
        {
            return "enum";
        }
        return "string";
    }
}

class Context : ITypeDescriptorContext
{

    public Context(object instance)
    {
        Instance = instance;
    }

    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }

    public void OnComponentChanged()
    {
        throw new NotImplementedException();
    }

    public bool OnComponentChanging()
    {
        throw new NotImplementedException();
    }

    public IContainer Container { get; }
    public object Instance { get; }
    public PropertyDescriptor PropertyDescriptor { get; }
}