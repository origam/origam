using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;
using Org.BouncyCastle.Utilities;
using Origam.Architect.Server.Utils;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.ReturnModels;

public class EditorProperty
{
    public EditorProperty(
        string name,
        Guid? controlPropertyId,
        string type,
        object value,
        DropDownValue[] dropDownValues,
        string category,
        string description,
        bool readOnly)
    {
    Name  = name;
    Type = type;
    Value = value;
    DropDownValues  = dropDownValues;
    Category= category;
    Description = description;
    ReadOnly = readOnly;
    ControlPropertyId = controlPropertyId;
    }

    public string Name { get; } 
    public string Type { get; }
    public object Value { get; } 
    public DropDownValue[] DropDownValues { get; }
    public string Category { get; } 
    public string Description { get; }
    public bool ReadOnly { get; } 
    public Guid? ControlPropertyId { get; }
    public List<string> Errors { get; set; }
}

public class DropDownValue(string Name, object Value)
{
    public string Name { get; } = Name;
    public object Value { get; } = Value;
}

public class EditorPropertyFactory
{
    public EditorProperty CreateIfMarkedAsEditable(PropertyInfo property,
        ISchemaItem item)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        if (category == null || !PropertyUtils.CanBeEdited(property))
        {
            return null;
        }

        return Create(property, item);
    }

    public EditorProperty Create(PropertyInfo property, object instance)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        string description =
            property.GetAttribute<DescriptionAttribute>()?.Description;

        object value = property.GetValue(instance);

        return new EditorProperty(
            name: property.Name,
            controlPropertyId: null,
            type: ToPropertyTypeName(property.PropertyType),
            value: ToSerializableValue(value),
            dropDownValues: GetAvailableValues(property, instance),
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null);
    }

    public EditorProperty Create(PropertyInfo property,
        PropertyValueItem valueItem)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        string description =
            property.GetAttribute<DescriptionAttribute>()?.Description;
        
        return new EditorProperty(
            name: property.Name,
            controlPropertyId: valueItem.ControlPropertyId,
            type: ToPropertyTypeName(property.PropertyType),
            value: valueItem.TypedValue,
            dropDownValues: Array.Empty<DropDownValue>(),
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null);
    }

    private object ToSerializableValue(object value)
    {
        if (value is IPersistent persistentObject)
        {
            return persistentObject.Id;
        }
        if (value is ICollection collection)
        {
            var editorValue = new List<object>();
            foreach (var item in collection)
            {
                editorValue.Add(item is IPersistent persistentValue
                    ? persistentValue.Id
                    : value);
            }

            return editorValue;
        }

        return value;
    }

    public DropDownValue[] GetAvailableValues(PropertyInfo property,
        object item)
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
                .Select(x => new DropDownValue(x.ToString(), (int)x))
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
        var values =
            getValuesMethod.Invoke(converterInstance, new object[] { context })
                as TypeConverter.StandardValuesCollection;
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

        if (type.IsEnum)
        {
            return "enum";
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return "integer";
        }

        if (type == typeof(decimal) || type == typeof(double) ||
            type == typeof(float))
        {
            return "float";
        }

        if (type.IsAssignableTo(typeof(ISchemaItem)))
        {
            return "looukup";
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