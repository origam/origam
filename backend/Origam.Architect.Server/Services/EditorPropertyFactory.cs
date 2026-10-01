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
using System.Globalization;
using System.Reflection;
using Origam.Architect.Server.Attributes;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.Architect.Server.Services;

public class EditorPropertyFactory
{
    // Spelling kept as is, the frontend expects this exact name.
    private const string LookupTypeName = "looukup";
    private const string UntypedTypeName = "untyped";

    public EditorProperty CreateIfMarkedAsEditable(PropertyInfo property, ISchemaItem item)
    {
        if (!PropertyUtils.CanBeEdited(property))
        {
            return null;
        }

        return Create(property, item);
    }

    public EditorProperty Create(PropertyInfo property, object instance)
    {
        string category = property.GetAttribute<CategoryAttribute>()?.Category;
        string description = property.GetAttribute<DescriptionAttribute>()?.Description;

        var context = new Context(instance);
        (DropDownValue[] dropDownValues, TypeConverter converter) = GetAvailableValues(
            property,
            context
        );
        object value = property.GetValue(instance);

        bool untyped = PropertyUtils.IsUntyped(property);
        // An untyped property is edited in its converter's text space.
        bool editedAsText = untyped && dropDownValues.Length > 0;

        return new EditorProperty(
            name: property.Name,
            controlPropertyId: null,
            type: editedAsText ? LookupTypeName
                : untyped ? UntypedTypeName
                : ToPropertyTypeName(property),
            value: editedAsText
                ? converter.ConvertToString(context, CultureInfo.InvariantCulture, value)
                : ToSerializableValue(value, property, instance),
            dropDownValues: dropDownValues,
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null
        );
    }

    public EditorProperty CreateBoundProperty(
        PropertyInfo property,
        Guid controlPropertyId,
        string boundFieldName,
        DropDownValue[] dropDownValues
    )
    {
        return new EditorProperty(
            name: property.Name,
            controlPropertyId: controlPropertyId,
            type: LookupTypeName,
            value: boundFieldName,
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
            type: ToPropertyTypeName(property),
            value: typedValue,
            dropDownValues: GetAvailableValues(property, new Context(instance: null)).Values,
            category: category,
            description: description,
            readOnly: property.GetSetMethod() == null
        );
    }

    private object ToSerializableValue(object value, PropertyInfo property, object instance)
    {
        // Shown in the XML format PropertyUtils.SetValue reads, a JSON number would lose it.
        if (instance is DataConstant dataConstant && property.Name == nameof(DataConstant.Value))
        {
            return dataConstant.XmlValue;
        }
        if (value is ISchemaItem schemaItem && property.GetSetMethod() == null)
        {
            return schemaItem.ToString();
        }
        if (value is IPersistent persistentObject)
        {
            return persistentObject.Id;
        }
        if (value is ICollection collection)
        {
            var editorValue = new List<object>();
            foreach (var item in collection)
            {
                editorValue.Add(item is IPersistent persistentValue ? persistentValue.Id : value);
            }

            return editorValue;
        }

        return value;
    }

    private (DropDownValue[] Values, TypeConverter Converter) GetAvailableValues(
        PropertyInfo property,
        ITypeDescriptorContext context
    )
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
            return ([], null);
        }

        if (property.PropertyType.IsEnum)
        {
            return (
                Enum.GetValues(property.PropertyType)
                    .Cast<object>()
                    .Select(x => new DropDownValue(x.ToString(), (int)x))
                    .ToArray(),
                null
            );
        }

        TypeConverter converter = PropertyUtils.CreateConverter(property);
        if (converter == null)
        {
            return ([], null);
        }

        TypeConverter.StandardValuesCollection values = converter.GetStandardValues(context);
        if (values == null || values.Count == 0)
        {
            return ([], converter);
        }

        return (
            values
                .Cast<object>()
                .Select(value => ToDropDownValue(value, converter, context))
                .ToArray(),
            converter
        );
    }

    // Plain values are identified by their display text.
    private static DropDownValue ToDropDownValue(
        object value,
        TypeConverter converter,
        ITypeDescriptorContext context
    )
    {
        if (value is ISchemaItem schemaItem)
        {
            return new DropDownValue(schemaItem.Name, schemaItem.Id);
        }
        if (value == null)
        {
            return new DropDownValue(Name: "", Value: null);
        }
        string text = converter.ConvertToString(context, CultureInfo.InvariantCulture, value) ?? "";
        return new DropDownValue(text, text);
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
        if (isReferenceProperty || type.IsAssignableTo(typeof(ISchemaItem)))
        {
            return property.GetSetMethod() == null ? "string" : LookupTypeName;
        }

        return "string";
    }
}
