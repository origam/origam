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

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ArchitectLogic;

public class PropertyParser(IPersistenceService persistenceService)
{
    public object Parse(PropertyInfo property, string value, object instance)
    {
        if (value == null)
        {
            return null;
        }

        if (property.PropertyType == typeof(string))
        {
            return value;
        }

        if (PropertyUtils.IsUntyped(property))
        {
            return ParseUntyped(property, value, instance);
        }

        if (property.PropertyType == typeof(bool))
        {
            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }

            throw MakeCouldNotParseException(property);
        }

        if (property.PropertyType == typeof(int))
        {
            if (int.TryParse(value, out var intValue))
            {
                return intValue;
            }

            throw MakeCouldNotParseException(property);
        }

        if (property.PropertyType == typeof(long))
        {
            if (long.TryParse(value, out var intValue))
            {
                return intValue;
            }

            throw MakeCouldNotParseException(property);
        }

        if (property.PropertyType == typeof(double))
        {
            if (double.TryParse(value, out var doubleValue))
            {
                return doubleValue;
            }

            throw MakeCouldNotParseException(property);
        }

        if (property.PropertyType == typeof(decimal))
        {
            if (decimal.TryParse(value, out var decimalValue))
            {
                return decimalValue;
            }

            throw MakeCouldNotParseException(property);
        }

        if (property.PropertyType.IsEnum)
        {
            if (Enum.TryParse(property.PropertyType, value, out var enumValue))
            {
                return enumValue;
            }

            throw MakeCouldNotParseException(property);
        }

        if (property.PropertyType == typeof(Guid))
        {
            return ParseGuid(value, property);
        }

        if (property.PropertyType.IsAssignableTo(typeof(IPersistent)))
        {
            Guid id = ParseGuid(value, property);
            return persistenceService.SchemaProvider.RetrieveInstance<IPersistent>(id);
        }

        throw new Exception(
            $"Type {property.PropertyType.Name} of property {property.Name} cannot be parsed."
        );
    }

    // Untyped properties are edited in the converter's text space, invariant.
    private static object ParseUntyped(PropertyInfo property, string value, object instance)
    {
        if (value.Length == 0)
        {
            return null;
        }

        TypeConverter converter = PropertyUtils.CreateConverter(property);
        var context = new Context(instance);
        if (converter == null || !converter.CanConvertFrom(context, typeof(string)))
        {
            return value;
        }

        object converted;
        try
        {
            converted = converter.ConvertFrom(context, CultureInfo.InvariantCulture, value);
        }
        catch (Exception exception) when (PropertyUtils.IsRejectedValueException(exception))
        {
            throw PropertyUtils.MakeValueNotReadException(property, exception);
        }

        // A converter answers text it cannot match with null, which would clear the value.
        if (converted == null)
        {
            throw new UserOrigamException(
                string.Format(Strings.Property_ValueNotOffered, property.Name, value)
            );
        }

        if (converted is string text && instance is DataConstant dataConstant)
        {
            return ParseDataConstantValue(property, text, dataConstant.DataType);
        }

        return converted;
    }

    // The model setter parses by the machine culture, the editor shows the XML format.
    private static object ParseDataConstantValue(
        PropertyInfo property,
        string value,
        OrigamDataType dataType
    )
    {
        try
        {
            return dataType switch
            {
                OrigamDataType.Integer => XmlConvert.ToInt32(value),
                OrigamDataType.Currency or OrigamDataType.Float => XmlConvert.ToDecimal(value),
                OrigamDataType.Date => XmlConvert.ToDateTime(
                    value,
                    XmlDateTimeSerializationMode.Unspecified
                ),
                _ => value,
            };
        }
        catch (Exception exception) when (exception is FormatException or OverflowException)
        {
            throw PropertyUtils.MakeValueNotReadException(property, exception);
        }
    }

    private Guid ParseGuid(string value, PropertyInfo property)
    {
        if (Guid.TryParse(value, out var guidValue))
        {
            return guidValue;
        }

        throw MakeCouldNotParseException(property);
    }

    private Exception MakeCouldNotParseException(PropertyInfo property)
    {
        return new Exception(
            $"Could not parse value of property {property.Name} to {property.PropertyType.Name}"
        );
    }
}
