#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using Origam.Architect.Server.Attributes;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ItemTypeCatalogService(SchemaService schemaService)
{
    private readonly object buildLock = new();
    private ItemTypeCatalog cachedCatalog;

    public ItemTypeCatalog Get()
    {
        lock (buildLock)
        {
            if (cachedCatalog != null)
            {
                return cachedCatalog;
            }

            ItemTypeCatalog catalog = Build();
            if (catalog.Types.Length > 0)
            {
                cachedCatalog = catalog;
            }
            return catalog;
        }
    }

    private ItemTypeCatalog Build()
    {
        var providers = new List<ItemTypeProviderInfo>();
        var typesToVisit = new Queue<Type>();
        var visitedTypes = new HashSet<Type>();
        var types = new List<ItemTypeInfo>();

        foreach (
            ISchemaItemProvider provider in schemaService.Providers.OrderBy(
                schemaItemProvider => schemaItemProvider.NodeText,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            Type[] newItemTypes = GetNewItemTypes(provider);
            providers.Add(
                new ItemTypeProviderInfo(
                    provider.GetType().FullName,
                    provider.NodeText,
                    newItemTypes.Select(GetCaption).ToArray()
                )
            );
            EnqueueAll(newItemTypes, typesToVisit, visitedTypes);
        }

        while (typesToVisit.Count > 0)
        {
            Type type = typesToVisit.Dequeue();
            Type[] childTypes = GetChildTypes(type);
            EnqueueAll(childTypes, typesToVisit, visitedTypes);

            types.Add(
                new ItemTypeInfo(
                    Caption: GetCaption(type),
                    TypeName: type.FullName,
                    FolderName: type.SchemaItemDescription()?.FolderName,
                    Children: childTypes.Select(GetCaption).ToArray(),
                    Properties: GetProperties(type)
                )
            );
        }

        return new ItemTypeCatalog(
            providers.ToArray(),
            types.OrderBy(type => type.Caption, StringComparer.OrdinalIgnoreCase).ToArray()
        );
    }

    private static void EnqueueAll(
        Type[] types,
        Queue<Type> typesToVisit,
        HashSet<Type> visitedTypes
    )
    {
        foreach (Type type in types)
        {
            if (visitedTypes.Add(type))
            {
                typesToVisit.Enqueue(type);
            }
        }
    }

    private static Type[] GetNewItemTypes(ISchemaItemFactory factory)
    {
        try
        {
            return factory.NewItemTypes ?? [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static Type[] GetChildTypes(Type type)
    {
        if (type.IsAbstract)
        {
            return [];
        }

        try
        {
            object instance = Activator.CreateInstance(type, Guid.Empty);
            return instance is ISchemaItemFactory factory ? GetNewItemTypes(factory) : [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string GetCaption(Type type)
    {
        return type.SchemaItemDescription()?.Name ?? type.Name;
    }

    private static ItemTypePropertyInfo[] GetProperties(Type type)
    {
        return type.GetProperties()
            .Where(IsSettableInEditor)
            .Select(ToPropertyInfo)
            .OrderBy(property => property.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsSettableInEditor(PropertyInfo property)
    {
        if (property.GetSetMethod() == null || !PropertyUtils.CanBeEdited(property))
        {
            return false;
        }

        return property.DeclaringType != typeof(AbstractSchemaItem) || property.Name == "Name";
    }

    private static ItemTypePropertyInfo ToPropertyInfo(PropertyInfo property)
    {
        Type propertyType =
            Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        return propertyType.IsEnum
            ? new ItemTypePropertyInfo(
                Name: property.Name,
                Type: "enum",
                Values: Enum.GetNames(propertyType)
            )
            : new ItemTypePropertyInfo(
                Name: property.Name,
                Type: GetPropertyTypeName(property, propertyType),
                Values: []
            );
    }

    private static string GetPropertyTypeName(PropertyInfo property, Type propertyType)
    {
        if (propertyType == typeof(bool))
        {
            return "boolean";
        }

        if (propertyType == typeof(int) || propertyType == typeof(long))
        {
            return "integer";
        }

        if (
            propertyType == typeof(decimal)
            || propertyType == typeof(double)
            || propertyType == typeof(float)
        )
        {
            return "float";
        }

        if (propertyType == typeof(DateTime))
        {
            return "date";
        }

        if (
            property.GetCustomAttribute<ReferencePropertyAttribute>() != null
            || propertyType.IsAssignableTo(typeof(ISchemaItem))
        )
        {
            return "reference";
        }

        return "string";
    }
}
