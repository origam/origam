using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Origam.Architect.Server.Controls;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Services;

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
            type: ToPropertyTypeName(property),
            value: ToSerializableValue(value),
            dropDownValues: GetAvailableValues(property, instance),
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null);
    }

    public EditorProperty Create(PropertyInfo property,
        PropertyBindingInfo bindingInfo, DropDownValue[] dropDownValues)
    {
        // DropDownValue[] dropDownValues = fields
        //     .Select(field => new DropDownValue(field.Name, field.Name))
        //     .ToArray();
        return new EditorProperty(
            name: property.Name,
            controlPropertyId: bindingInfo.ControlPropertyId,
            type: "looukup",
            value: bindingInfo.Value,
            dropDownValues: dropDownValues,
            category: "Data",
            description: "The data bindings for the control.",
            readOnly: property.GetSetMethod() == null);
    }
    public EditorProperty Create(PropertyInfo property,
        PropertyValueItem valueItem)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        string description =
            property.GetAttribute<DescriptionAttribute>()?.Description;
        
        string name = property   
            .GetCustomAttribute<ReferencePropertyAttribute>()?.Name ?? property.Name;
        
        return new EditorProperty(
            name: name,
            controlPropertyId: valueItem.ControlPropertyId,
            type: ToPropertyTypeName(property),
            value: valueItem.TypedValue,
            dropDownValues: GetAvailableValues(property, null),
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

    private DropDownValue[] GetAvailableValues(PropertyInfo property,
        object instance)
    {
        bool isReferenceProperty = property   
            .GetCustomAttribute<ReferencePropertyAttribute>() != null;
        if (!isReferenceProperty && 
            (property.PropertyType == typeof(string) ||
            property.PropertyType == typeof(Guid) ||
            property.PropertyType == typeof(int) ||
            property.PropertyType == typeof(long) ||
            property.PropertyType == typeof(decimal) ||
            property.PropertyType == typeof(double) ||
            property.PropertyType == typeof(float) ||
            property.PropertyType == typeof(bool)))
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
        var context = new Context(instance);
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

    private string ToPropertyTypeName(PropertyInfo property)
    {
        Type type = property.PropertyType;
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

        bool isReferenceProperty = property   
            .GetCustomAttribute<ReferencePropertyAttribute>() != null;
        if (isReferenceProperty || type.IsAssignableTo(typeof(ISchemaItem)))
        {
            return "looukup";
        }

        return "string";
    }
}