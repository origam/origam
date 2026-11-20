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

using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class PackageChangeChecker(SchemaService schemaService)
{
    private const int MaxListedItems = 5;

    public static List<ISchemaItem> CollectItemsChangingPackage(ISchemaItem item)
    {
        var items = new List<ISchemaItem>();
        CollectPackageMembers(item, item.SchemaExtensionId, items);
        if (
            item is PanelControlSet { PanelControl: { } panelControl }
            && panelControl.SchemaExtensionId == item.SchemaExtensionId
        )
        {
            CollectPackageMembers(panelControl, item.SchemaExtensionId, items);
        }
        return items;
    }

    public string Check(ISchemaItem item, Package package)
    {
        List<ISchemaItem> movingItems = CollectItemsChangingPackage(item);
        HashSet<Guid> movingIds = movingItems.Select(moving => moving.Id).ToHashSet();
        Dictionary<Guid, Package> allPackages = schemaService.AllPackages.ToDictionary(candidate =>
            candidate.Id
        );
        HashSet<Guid> packagesSeenByTarget = package
            .IncludedPackages.Select(included => included.Id)
            .Append(package.Id)
            .ToHashSet();

        List<ISchemaItem> missingDependencies = movingItems
            .SelectMany(moving => moving.GetDependencies(ignoreErrors: true))
            .Where(dependency =>
                dependency != null
                && !movingIds.Contains(dependency.Id)
                && !packagesSeenByTarget.Contains(dependency.SchemaExtensionId)
            )
            .DistinctBy(dependency => dependency.Id)
            .ToList();
        if (missingDependencies.Count > 0)
        {
            return string.Format(
                Strings.Move_PackageMissingDependencies,
                item.Name,
                package.Name,
                Describe(missingDependencies, allPackages)
            );
        }

        if (!ReferenceIndexManager.Initialized)
        {
            return Strings.Move_ReferenceIndexNotReady;
        }

        HashSet<Guid> packagesSeeingTarget = allPackages
            .Values.Where(candidate =>
                candidate.Id == package.Id
                || candidate.IncludedPackages.Any(included => included.Id == package.Id)
            )
            .Select(candidate => candidate.Id)
            .ToHashSet();
        List<ISchemaItem> blockingUsers = movingItems
            .SelectMany(moving =>
                moving.GetUsage().Concat(GetChildrenOfOtherPackages(moving, item.SchemaExtensionId))
            )
            .Where(user =>
                !movingIds.Contains(user.Id)
                && !packagesSeeingTarget.Contains(user.SchemaExtensionId)
            )
            .DistinctBy(user => user.Id)
            .ToList();
        if (blockingUsers.Count > 0)
        {
            return string.Format(
                Strings.Move_PackageUsedOutside,
                item.Name,
                package.Name,
                Describe(blockingUsers, allPackages)
            );
        }

        return null;
    }

    private static void CollectPackageMembers(
        ISchemaItem node,
        Guid packageId,
        List<ISchemaItem> items
    )
    {
        items.Add(node);
        foreach (ISchemaItem child in node.ChildItems)
        {
            if (child.DerivedFrom == null && child.SchemaExtensionId == packageId)
            {
                CollectPackageMembers(child, packageId, items);
            }
        }
    }

    private static IEnumerable<ISchemaItem> GetChildrenOfOtherPackages(
        ISchemaItem node,
        Guid packageId
    )
    {
        return node.ChildItems.Where(child =>
            child.DerivedFrom == null && child.SchemaExtensionId != packageId
        );
    }

    private string Describe(List<ISchemaItem> items, Dictionary<Guid, Package> allPackages)
    {
        HashSet<Guid> loadedPackageIds = schemaService
            .LoadedPackages.Select(loaded => loaded.Id)
            .ToHashSet();
        IEnumerable<string> listed = items
            .Take(MaxListedItems)
            .Select(listedItem =>
                string.Format(
                    Strings.Move_ItemInPackage,
                    loadedPackageIds.Contains(listedItem.SchemaExtensionId)
                        ? listedItem.Path
                        : listedItem.Name,
                    allPackages.GetValueOrDefault(listedItem.SchemaExtensionId)?.Name
                )
            );
        if (items.Count > MaxListedItems)
        {
            listed = listed.Append("…");
        }
        return string.Join(separator: ", ", listed);
    }
}
