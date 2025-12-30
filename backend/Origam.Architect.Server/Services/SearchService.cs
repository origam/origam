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
        var results = persistenceService.SchemaProvider.FullTextSearch<ISchemaItem>(text);
        return results.Select(result => GetResult(result, referencePackages));
    }

    public IEnumerable<SearchResult> FindReferences(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        List<Guid> referencePackages = GetReferencePackages();
        return item.GetUsage().Select(result => GetResult(result, referencePackages));
    }

    public IEnumerable<SearchResult> FindDependencies(Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        List<Guid> referencePackages = GetReferencePackages();
        return item.GetDependencies(false).Select(result => GetResult(result, referencePackages));
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
            PackageReference = referencePackages.Contains(item.SchemaExtensionId),
            ParentNodeIds = GetParentNodeIds(item),
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

        AddFolderNameIfAny(ids, item);

        for (var parent = item.ParentItem; parent != null; parent = parent.ParentItem)
        {
            AddFolderNameIfAny(ids, parent);
            ids.Add(parent.Id.ToString());
        }

        for (var group = item.RootItem.Group; group != null; group = group.ParentGroup)
        {
            ids.Add(group.Id.ToString());
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
