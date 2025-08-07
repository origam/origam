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

using System.Reflection;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ArchitectLogic;

public class PropertyParser
{
    private readonly IPersistenceService persistenceService;

    public PropertyParser(IPersistenceService persistenceService)
    {
        this.persistenceService = persistenceService;
    }

    public object Parse(PropertyInfo property, string value)
    {
        if (value == null)
        {
            return null;
        }

        if (property.PropertyType == typeof(string))
        {
            return value;
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
            return persistenceService.SchemaProvider.RetrieveInstance<IPersistent>(
                id
            );
        }

        throw new Exception(
            $"Type {property.PropertyType.Name} of property {property.Name} cannot be parsed."
        );
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
