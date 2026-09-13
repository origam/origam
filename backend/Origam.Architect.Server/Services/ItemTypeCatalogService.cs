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
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ItemTypeCatalogService(SchemaService schemaService)
{
    private const int MinimumSamples = 3;
    private const int MaxCommonValueLength = 60;
    private const double DominantShare = 0.6;

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
        Dictionary<Type, List<ISchemaItem>> existingItems = GroupExistingItemsByType();

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

            List<ISchemaItem> itemsOfType = existingItems.GetValueOrDefault(type) ?? [];
            types.Add(
                new ItemTypeInfo(
                    Caption: GetCaption(type),
                    TypeName: type.FullName,
                    FolderName: type.SchemaItemDescription()?.FolderName,
                    Children: childTypes.Select(GetCaption).ToArray(),
                    Properties: GetProperties(type, itemsOfType),
                    ExistingCount: itemsOfType.Count
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

    private static ItemTypePropertyInfo[] GetProperties(
        Type type,
        IReadOnlyList<ISchemaItem> existingItems
    )
    {
        return type.GetProperties()
            .Where(IsSettableInEditor)
            .Select(property => ToPropertyInfo(property, existingItems))
            .OrderBy(property => property.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private Dictionary<Type, List<ISchemaItem>> GroupExistingItemsByType()
    {
        var itemsByType = new Dictionary<Type, List<ISchemaItem>>();
        foreach (ISchemaItemProvider provider in schemaService.Providers)
        {
            try
            {
                foreach (ISchemaItem item in provider.ChildItems)
                {
                    Collect(item, itemsByType);
                }
            }
            catch (Exception)
            {
                continue;
            }
        }

        return itemsByType;
    }

    private static void Collect(ISchemaItem item, Dictionary<Type, List<ISchemaItem>> itemsByType)
    {
        if (!itemsByType.TryGetValue(item.GetType(), out List<ISchemaItem> items))
        {
            items = [];
            itemsByType[item.GetType()] = items;
        }
        items.Add(item);

        foreach (ISchemaItem child in item.ChildItemsRecursive)
        {
            if (!itemsByType.TryGetValue(child.GetType(), out List<ISchemaItem> childItems))
            {
                childItems = [];
                itemsByType[child.GetType()] = childItems;
            }
            childItems.Add(child);
        }
    }

    private static int CountItemsWithValue(
        PropertyInfo property,
        IReadOnlyList<ISchemaItem> existingItems
    )
    {
        FieldInfo idField = GetReferenceIdField(property);
        if (idField == null)
        {
            return 0;
        }

        return existingItems.Count(item => IsReferenceSet(idField, item));
    }

    private static FieldInfo GetReferenceIdField(PropertyInfo property)
    {
        string idFieldName = property.GetCustomAttribute<XmlReferenceAttribute>()?.IdField;
        return idFieldName == null
            ? null
            : property.DeclaringType?.GetField(
                idFieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );
    }

    private static bool IsReferenceSet(FieldInfo idField, ISchemaItem item)
    {
        try
        {
            return idField.GetValue(item) is Guid value && value != Guid.Empty;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsSettableInEditor(PropertyInfo property)
    {
        if (property.GetSetMethod() == null || !PropertyUtils.CanBeEdited(property))
        {
            return false;
        }

        return property.DeclaringType != typeof(AbstractSchemaItem) || property.Name == "Name";
    }

    private static ItemTypePropertyInfo ToPropertyInfo(
        PropertyInfo property,
        IReadOnlyList<ISchemaItem> existingItems
    )
    {
        Type propertyType =
            Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        bool required = IsRequired(property);
        if (propertyType.IsEnum)
        {
            return new ItemTypePropertyInfo(
                Name: property.Name,
                Type: "enum",
                Values: Enum.GetNames(propertyType),
                Required: required,
                SetOnExisting: 0,
                CommonValue: required ? GetCommonValue(property, existingItems) : null
            );
        }

        string typeName = GetPropertyTypeName(property, propertyType);
        bool isReference = typeName == "reference";
        return new ItemTypePropertyInfo(
            Name: property.Name,
            Type: typeName,
            Values: [],
            Required: required,
            SetOnExisting: isReference ? CountItemsWithValue(property, existingItems) : 0,
            CommonValue: required && !isReference ? GetCommonValue(property, existingItems) : null
        );
    }

    private static string GetCommonValue(
        PropertyInfo property,
        IReadOnlyList<ISchemaItem> existingItems
    )
    {
        if (existingItems.Count < MinimumSamples)
        {
            return null;
        }

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (ISchemaItem item in existingItems)
        {
            string text = ReadScalar(property, item);
            if (string.IsNullOrWhiteSpace(text) || text.Length > MaxCommonValueLength)
            {
                continue;
            }
            counts[text] = counts.GetValueOrDefault(text) + 1;
        }

        if (counts.Count == 0)
        {
            return null;
        }

        KeyValuePair<string, int> mostCommon = counts.MaxBy(pair => pair.Value);
        return
            mostCommon.Value >= MinimumSamples
            && mostCommon.Value >= existingItems.Count * DominantShare
            ? mostCommon.Key
            : null;
    }

    private static string ReadScalar(PropertyInfo property, ISchemaItem item)
    {
        try
        {
            return property.GetValue(item)?.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsRequired(PropertyInfo property)
    {
        return property.GetCustomAttribute<NotNullModelElementRuleAttribute>() != null
            || property.GetCustomAttribute<StringNotEmptyModelElementRuleAttribute>() != null;
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
