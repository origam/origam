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

using Origam.Architect.Server.Exceptions;
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
        return results
            .Where(x => x != null)
            .Select(result => BuildResult(result, referencePackages));
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
            .Select(result => BuildResult(result, referencePackages));
    }

    public IEnumerable<SearchResult> FindDependencies(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        List<Guid> referencePackages = GetReferencePackages();
        return item.GetDependencies(false)
            .Where(x => x != null)
            .Select(result => BuildResult(result, referencePackages));
    }

    public List<SearchResult> BuildResults(IEnumerable<ISchemaItem> items)
    {
        List<Guid> referencePackages = GetReferencePackages();
        return items
            .Where(item => item != null)
            .Select(item => BuildResult(item, referencePackages))
            .ToList();
    }

    public List<Guid> GetReferencePackages()
    {
        var referencePackages = schemaService
            .ActiveExtension.IncludedPackages.Select(x => x.Id)
            .Append(schemaService.ActiveExtension.Id)
            .ToList();
        return referencePackages;
    }

    public SearchResult BuildResult(ISchemaItem item, List<Guid> referencePackages)
    {
        try
        {
            ISchemaItem root = GetRoot(item);
            List<SchemaItemGroup> groups = GetGroupChain(root);
            return new SearchResult
            {
                SchemaId = item.Id,
                Type = item.ModelDescription() ?? item.ItemType,
                RootType = root.ModelDescription() ?? root.ItemType,
                FoundIn = item.Path,
                Folder = groups.Count == 0 ? "" : groups[0].Path,
                Package = item.PackageName,
                PackageReference = referencePackages.Contains(item.SchemaExtensionId),
                ParentNodeIds = GetParentNodeIds(item, root, groups),
                IsOrphaned = false,
            };
        }
        catch (OrphanedSchemaReferenceException ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    ex,
                    $"Orphaned reference while building search result for schema item {item.Id}"
                );
            }
            return new SearchResult
            {
                SchemaId = item.Id,
                Type = item.ModelDescription() ?? item.ItemType,
                FoundIn = item.Name ?? item.Id.ToString(),
                IsOrphaned = true,
            };
        }
    }

    private static ISchemaItem GetRoot(ISchemaItem item)
    {
        try
        {
            ISchemaItem root = item;
            for (ISchemaItem parent = item.ParentItem; parent != null; parent = parent.ParentItem)
            {
                root = parent;
            }
            return root;
        }
        catch (Exception ex)
        {
            throw new OrphanedSchemaReferenceException(item.Id, ex);
        }
    }

    // Group and ParentGroup throw when a group id no longer resolves. That is a
    // broken folder, not a broken item, so the chain is dropped and the item is
    // still reported - just without a folder.
    private List<SchemaItemGroup> GetGroupChain(ISchemaItem root)
    {
        var groups = new List<SchemaItemGroup>();
        try
        {
            for (SchemaItemGroup group = root.Group; group != null; group = group.ParentGroup)
            {
                groups.Add(group);
            }
            return groups;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, $"Could not read the group of schema item {root.Id}");
            }
            return [];
        }
    }

    private static List<string> GetParentNodeIds(
        ISchemaItem item,
        ISchemaItem root,
        List<SchemaItemGroup> groups
    )
    {
        try
        {
            if (root.RootProvider is not AbstractSchemaItemProvider provider)
            {
                return [];
            }

            var ids = new List<string>();
            AddFolderNameIfAny(ids, item);

            for (ISchemaItem parent = item.ParentItem; parent != null; parent = parent.ParentItem)
            {
                ids.Add(parent.Id.ToString());
                AddFolderNameIfAny(ids, parent);
            }

            foreach (SchemaItemGroup group in groups)
            {
                ids.Add(group.Id.ToString());
            }

            ids.Add(provider.NodeId);
            ids.Add(provider.Group);
            ids.Reverse();

            return ids;
        }
        catch (Exception ex)
        {
            throw new OrphanedSchemaReferenceException(item.Id, ex);
        }

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
