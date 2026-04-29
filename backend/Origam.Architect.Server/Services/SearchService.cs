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
        return results.Where(x => x != null).Select(result => GetResult(result, referencePackages));
    }

    public IEnumerable<SearchResult> FindReferences(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
            instanceId: schemaItemId
        );
        List<Guid> referencePackages = GetReferencePackages();
        List<ISchemaItem> schemaItems = item.GetUsage();
        if (schemaItems == null)
        {
            return [];
        }
        return schemaItems
            .Where(predicate: x => x != null)
            .Select(selector: result =>
                GetResult(item: result, referencePackages: referencePackages)
            );
    }

    public IEnumerable<SearchResult> FindDependencies(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(
            instanceId: schemaItemId
        );
        List<Guid> referencePackages = GetReferencePackages();
        return item.GetDependencies(ignoreErrors: false)
            .Where(predicate: x => x != null)
            .Select(selector: result =>
                GetResult(item: result, referencePackages: referencePackages)
            );
    }

    private List<Guid> GetReferencePackages()
    {
        var referencePackages = schemaService
            .ActiveExtension.IncludedPackages.Select(selector: x => x.Id)
            .Append(element: schemaService.ActiveExtension.Id)
            .ToList();
        return referencePackages;
    }

    private SearchResult GetResult(ISchemaItem item, List<Guid> referencePackages)
    {
        try
        {
            ISchemaItem root = GetRoot(item);
            return new SearchResult
            {
                SchemaId = item.Id,
                Type = item.ModelDescription() ?? item.ItemType,
                RootType = root.ModelDescription() ?? root.ItemType,
                FoundIn = item.Path,
                Folder = root.Group?.Path ?? "",
                Package = item.PackageName,
                PackageReference = referencePackages.Contains(item.SchemaExtensionId),
                ParentNodeIds = GetParentNodeIds(item, root),
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

    private static List<string> GetParentNodeIds(ISchemaItem item, ISchemaItem root)
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

            for (SchemaItemGroup group = root.Group; group != null; group = group.ParentGroup)
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
            if (!string.IsNullOrWhiteSpace(value: folderName))
            {
                target.Add(item: folderName);
            }
        }
    }
}
