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

using Origam.Architect.Server.Models.Requests;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class SearchService(
    IPersistenceService persistenceService,
    SchemaService schemaService,
    ILogger<SearchService> logger
)
{
    public IEnumerable<SearchResult> SearchByText(string text)
    {
        List<Guid> referencePackages = GetReferencePackages();
        var results = persistenceService.SchemaProvider.FullTextSearch<ISchemaItem>(text);
        return results.Where(x => x != null).Select(result => GetResult(result, referencePackages));
    }

    public IEnumerable<SearchResult> FindReferences(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        List<Guid> referencePackages = GetReferencePackages();
        List<ISchemaItem> schemaItems = item.GetUsage();
        if (schemaItems == null)
        {
            return [];
        }
        return schemaItems
            .Where(x => x != null)
            .Select(result => GetResult(result, referencePackages));
    }

    public IEnumerable<SearchResult> FindDependencies(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        List<Guid> referencePackages = GetReferencePackages();
        return item.GetDependencies(false)
            .Where(x => x != null)
            .Select(result => GetResult(result, referencePackages));
    }

    private List<Guid> GetReferencePackages()
    {
        var referencePackages = schemaService
            .ActiveExtension.IncludedPackages.Select(x => x.Id)
            .Append(schemaService.ActiveExtension.Id)
            .ToList();
        return referencePackages;
    }

    private SearchResult GetResult(ISchemaItem item, List<Guid> referencePackages)
    {
        ISchemaItem root = GetRootSafe(item);

        return new SearchResult
        {
            SchemaId = item.Id,
            Type = item.ModelDescription() ?? item.ItemType,
            RootType = root?.ModelDescription() ?? root?.ItemType ?? item.ItemType,
            FoundIn = SafeGet(() => item.Path, nameof(item.Path), item.Id) ?? item.Name ?? "",
            Folder = SafeGet(() => root?.Group?.Path, "Group.Path", item.Id) ?? "",
            Package = SafeGet(() => item.PackageName, nameof(item.PackageName), item.Id) ?? "",
            PackageReference = referencePackages.Contains(item.SchemaExtensionId),
            ParentNodeIds = GetParentNodeIds(item, root),
        };
    }

    /// <summary>
    /// Returns the topmost ancestor of <paramref name="item"/>, or the nearest
    /// reachable one if the parent chain is broken by an orphaned reference.
    /// </summary>
    private ISchemaItem GetRootSafe(ISchemaItem item)
    {
        ISchemaItem root = item;
        try
        {
            // Walk up the parent chain to find the root. Keep the last visited ancestor.
            for (ISchemaItem parent = item.ParentItem; parent != null; parent = parent.ParentItem)
            {
                root = parent;
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    ex,
                    $"Orphaned parent reference while walking root for schema item {item.Id}; using nearest valid ancestor {root.Id}"
                );
            }
        }
        return root;
    }

    private T SafeGet<T>(Func<T> getter, string propertyName, Guid itemId)
    {
        try
        {
            return getter();
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    ex,
                    $"Orphaned reference while evaluating property {propertyName} for schema item {itemId}"
                );
            }
            return default;
        }
    }

    private List<string> GetParentNodeIds(ISchemaItem item, ISchemaItem root)
    {
        if (root?.RootProvider is not AbstractSchemaItemProvider provider)
        {
            return [];
        }

        var ids = new List<string>();
        AddFolderNameIfAny(ids, item);

        try
        {
            for (ISchemaItem parent = item.ParentItem; parent != null; parent = parent.ParentItem)
            {
                ids.Add(parent.Id.ToString());
                AddFolderNameIfAny(ids, parent);
            }

            for (SchemaItemGroup group = root.Group; group != null; group = group.ParentGroup)
            {
                ids.Add(group.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    ex,
                    $"Orphaned reference while building parent node ids for schema item {item.Id}"
                );
            }
        }

        ids.Add(provider.NodeId);
        ids.Add(provider.Group);
        ids.Reverse();

        return ids;

        static void AddFolderNameIfAny(List<string> target, ISchemaItem schemaItem)
        {
            var folderName = schemaItem?.GetType().SchemaItemDescription()?.FolderName;
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                target.Add(folderName);
            }
        }
    }
}
