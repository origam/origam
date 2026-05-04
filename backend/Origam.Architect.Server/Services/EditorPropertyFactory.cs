#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Origam.Architect.Server.Attributes;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Services;

public class EditorPropertyFactory
{
    public EditorProperty CreateIfMarkedAsEditable(PropertyInfo property, ISchemaItem item)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        if (category == null || !PropertyUtils.CanBeEdited(property: property))
        {
            return null;
        }

        return Create(property: property, instance: item);
    }

    public EditorProperty Create(PropertyInfo property, object instance)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        string description = property.GetAttribute<DescriptionAttribute>()?.Description;

        object value = property.GetValue(obj: instance);

        return new EditorProperty(
            name: property.Name,
            controlPropertyId: null,
            type: ToPropertyTypeName(property: property),
            value: ToSerializableValue(value: value),
            dropDownValues: GetAvailableValues(property: property, instance: instance),
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null
        );
    }

    public EditorProperty Create(
        PropertyInfo property,
        PropertyBindingInfo bindingInfo,
        DropDownValue[] dropDownValues
    )
    {
        return new EditorProperty(
            name: property.Name,
            controlPropertyId: bindingInfo.ControlPropertyId,
            type: "looukup",
            value: bindingInfo.Value,
            dropDownValues: dropDownValues,
            category: "Data",
            description: "The data bindings for the control.",
            readOnly: property.GetSetMethod() == null
        );
    }

    public EditorProperty Create(PropertyInfo property, Guid controlPropertyId, object typedValue)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        string description = property.GetAttribute<DescriptionAttribute>()?.Description;

        string name =
            property.GetCustomAttribute<ReferencePropertyAttribute>()?.Name ?? property.Name;

        return new EditorProperty(
            name: name,
            controlPropertyId: controlPropertyId,
            type: ToPropertyTypeName(property: property),
            value: typedValue,
            dropDownValues: GetAvailableValues(property: property, instance: null),
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null
        );
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
                editorValue.Add(
                    item: item is IPersistent persistentValue ? persistentValue.Id : value
                );
            }

            return editorValue;
        }

        return value;
    }

    private DropDownValue[] GetAvailableValues(PropertyInfo property, object instance)
    {
        bool isReferenceProperty =
            property.GetCustomAttribute<ReferencePropertyAttribute>() != null;
        if (
            !isReferenceProperty
            && (
                property.PropertyType == typeof(string)
                || property.PropertyType == typeof(Guid)
                || property.PropertyType == typeof(int)
                || property.PropertyType == typeof(long)
                || property.PropertyType == typeof(decimal)
                || property.PropertyType == typeof(double)
                || property.PropertyType == typeof(float)
                || property.PropertyType == typeof(bool)
            )
        )
        {
            return [];
        }

        if (property.PropertyType.IsEnum)
        {
            return Enum.GetValues(enumType: property.PropertyType)
                .Cast<object>()
                .Select(selector: x => new DropDownValue(Name: x.ToString(), Value: (int)x))
                .ToArray();
        }

        var converterType = property.GetAttribute<TypeConverterAttribute>()?.ConverterTypeName;
        if (converterType == null)
        {
            return [];
        }

        Type type = Type.GetType(typeName: converterType);
        if (type == null)
        {
            throw new Exception(message: $"Could not find type {converterType}");
        }

        object converterInstance = Activator.CreateInstance(type: type);
        MethodInfo getValuesMethod = type.GetMethod(
            name: "GetStandardValues",
            types: new Type[] { typeof(ITypeDescriptorContext) }
        )!;
        var context = new Context(instance: instance);
        var values =
            getValuesMethod.Invoke(obj: converterInstance, parameters: new object[] { context })
            as TypeConverter.StandardValuesCollection;
        if (values == null || values.Count == 0)
        {
            return [];
        }

        return values
            .Cast<ISchemaItem>()
            .Select(selector: x => new DropDownValue(Name: x?.Name ?? "", Value: x?.Id))
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

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return "float";
        }

        bool isReferenceProperty =
            property.GetCustomAttribute<ReferencePropertyAttribute>() != null;
        if (isReferenceProperty || type.IsAssignableTo(targetType: typeof(ISchemaItem)))
        {
            return "looukup";
        }

        return "string";
    }
}
