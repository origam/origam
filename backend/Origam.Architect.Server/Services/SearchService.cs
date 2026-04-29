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

public class SearchService(IPersistenceService persistenceService, SchemaService schemaService)
{
    public IEnumerable<SearchResult> SearchByText(string text)
    {
        List<Guid> referencePackages = GetReferencePackages();
        var results = persistenceService.SchemaProvider.FullTextSearch<ISchemaItem>(text: text);
        return results.Select(selector: result =>
            GetResult(item: result, referencePackages: referencePackages)
        );
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
        if (item == null)
        {
            return new SearchResult();
        }
        string name = item.ModelDescription() ?? item.ItemType;
        string rootName = item.RootItem.ModelDescription() ?? item.RootItem.ItemType;
        var searchResult = new SearchResult
        {
            FoundIn = item.Path,
            RootType = rootName,
            Type = name,
            SchemaId = item.Id,
            Folder = item.RootItem.Group == null ? "" : item.RootItem.Group.Path,
            Package = item.PackageName,
            PackageReference = referencePackages.Contains(item: item.SchemaExtensionId),
            ParentNodeIds = GetParentNodeIds(item: item),
        };
        return searchResult;
    }

    private static List<string> GetParentNodeIds(ISchemaItem item)
    {
        if (item?.RootItem?.RootProvider is not AbstractSchemaItemProvider provider)
        {
            return [];
        }

        var ids = new List<string>();

        AddFolderNameIfAny(target: ids, schemaItem: item);

        for (var parent = item.ParentItem; parent != null; parent = parent.ParentItem)
        {
            ids.Add(item: parent.Id.ToString());
            AddFolderNameIfAny(target: ids, schemaItem: parent);
        }

        for (var group = item.RootItem.Group; group != null; group = group.ParentGroup)
        {
            ids.Add(item: group.Id.ToString());
        }

        ids.Add(item: provider.NodeId);
        ids.Add(item: provider.Group);

        ids.Reverse();
        return ids;

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
