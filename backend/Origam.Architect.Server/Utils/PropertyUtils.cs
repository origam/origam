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
using System.Reflection;
using Origam.Extensions;

namespace Origam.Architect.Server.Utils;

public static class PropertyUtils
{
    public static bool CanBeEdited(PropertyInfo property)
    {
        var browsableAttribute = (BrowsableAttribute)
            Attribute.GetCustomAttribute(property, typeof(BrowsableAttribute), inherit: true);
        return browsableAttribute?.Browsable ?? true;
    }

    public static bool IsUntyped(PropertyInfo property)
    {
        return property.PropertyType == typeof(object);
    }

    public static TypeConverter CreateConverter(PropertyInfo property)
    {
        string converterTypeName = property
            .GetAttribute<TypeConverterAttribute>()
            ?.ConverterTypeName;
        if (converterTypeName == null)
        {
            return null;
        }

        Type type = Type.GetType(converterTypeName);
        if (type == null)
        {
            throw new Exception($"Could not find type {converterTypeName}");
        }

        return Activator.CreateInstance(type) as TypeConverter;
    }

    public static void SetValue(PropertyInfo property, object instance, object value)
    {
        try
        {
            property.SetValue(instance, value);
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException
                    is FormatException
                        or OverflowException
                        or InvalidCastException
                        or ArgumentException
            )
        {
            throw MakeValueNotReadException(property, exception.InnerException);
        }
    }

    public static UserOrigamException MakeValueNotReadException(
        PropertyInfo property,
        Exception exception
    )
    {
        return new UserOrigamException(
            string.Format(Strings.Property_ValueNotRead, property.Name, exception.Message),
            exception.StackTrace,
            exception
        );
    }
}
