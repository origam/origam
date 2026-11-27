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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelTransfer;

public class MoveToPackage(SchemaService schema, IPersistenceService persistenceService)
{
    private const int MaxListedItems = 10;

    public List<Package> GetPackages()
    {
        return schema
            .ActiveExtension.IncludedPackages.Append(schema.ActiveExtension)
            .OrderBy(package => package.Name)
            .ToList();
    }

    public List<IBrowserNode2> GetTargets(IBrowserNode2 activeNode, Package package)
    {
        if (activeNode is ISchemaItem { ParentItem: { } parent })
        {
            return IsPackageIncluded(package, parent.SchemaExtensionId) ? [parent] : [];
        }
        return [.. GetGroups(activeNode, package)];
    }

    private static List<SchemaItemGroup> GetGroups(IBrowserNode2 activeNode, Package package)
    {
        var movedGroup = activeNode as SchemaItemGroup;
        return GetActiveItemRootProvider(activeNode)
            .ChildGroups.OrderBy(group => group.Name)
            .Where(group => group.SchemaExtensionId == package.Id)
            .SelectMany(group => group.ChildGroupsRecursive.Prepend(group))
            .Where(group =>
                IsPackageIncluded(package, group.SchemaExtensionId)
                && (movedGroup == null || !IsGroupOrItsDescendant(group, movedGroup))
            )
            .ToList();
    }

    public string GetRefusal(IBrowserNode2 activeNode, Package targetPackage)
    {
        List<ISchemaItem> topLevelItems = GetTopLevelItems(activeNode);
        List<ISchemaItem> itemsChangingPackage = topLevelItems
            .Where(item => !IsItemOfPackage(item, targetPackage))
            .ToList();
        if (itemsChangingPackage.Count == 0)
        {
            return null;
        }
        HashSet<Guid> movingIds = topLevelItems
            .SelectMany(GetItemAndDescendants)
            .Select(item => item.Id)
            .ToHashSet();
        foreach (ISchemaItem item in itemsChangingPackage)
        {
            List<ISchemaItem> dependenciesInPackagesNotReferencedByTargetPackage =
                item.GetDependencies(ignoreErrors: true)
                    .Where(dependency =>
                        dependency != null
                        && !movingIds.Contains(dependency.Id)
                        && !IsPackageIncluded(targetPackage, dependency.SchemaExtensionId)
                    )
                    .DistinctBy(dependency => dependency.Id)
                    .ToList();
            if (dependenciesInPackagesNotReferencedByTargetPackage.Count != 0)
            {
                return FormatRefusal(
                    Strings.ErrorDependenciesInPackagesNotReferencedByTargetPackage,
                    item,
                    targetPackage,
                    dependenciesInPackagesNotReferencedByTargetPackage
                );
            }
        }
        if (!ReferenceIndexManager.Initialized)
        {
            return Strings.ErrorReferenceIndexNotReady;
        }
        foreach (ISchemaItem item in itemsChangingPackage)
        {
            List<ISchemaItem> usagesInPackagesWhichDontDependOnTargetPackage = item.GetUsage()
                .Where(usage =>
                    !movingIds.Contains(usage.Id)
                    && !IsPackageIncluded(usage.Package, targetPackage.Id)
                )
                .DistinctBy(usage => usage.Id)
                .ToList();
            if (usagesInPackagesWhichDontDependOnTargetPackage.Count != 0)
            {
                return FormatRefusal(
                    Strings.ErrorUsagesInPackagesWhichDontDependOnTargetPackage,
                    item,
                    targetPackage,
                    usagesInPackagesWhichDontDependOnTargetPackage
                );
            }
        }
        return null;
    }

    public void Move(IBrowserNode2 activeNode, Package targetPackage, SchemaItemGroup targetGroup)
    {
        string refusal = GetRefusal(activeNode, targetPackage);
        if (refusal != null)
        {
            throw new UserOrigamException(refusal);
        }
        try
        {
            ModelTransaction.Run(
                persistenceService.SchemaProvider,
                () =>
                {
                    if (activeNode is SchemaItemGroup activeGroup)
                    {
                        MoveGroupRecursive(targetPackage, targetGroup, activeGroup);
                    }
                    else if (activeNode is ISchemaItem activeItem)
                    {
                        MoveSchemaItem(targetPackage, targetGroup, activeItem);
                    }
                }
            );
        }
        catch
        {
            GetActiveItemRootProvider(activeNode)?.ClearCache();
            throw;
        }
    }

    private static ISchemaItemProvider GetActiveItemRootProvider(IBrowserNode2 activeNode)
    {
        return activeNode switch
        {
            ISchemaItem schemaItem => schemaItem.RootProvider,
            SchemaItemGroup group => group.RootProvider,
            _ => throw new ArgumentOutOfRangeException(nameof(activeNode)),
        };
    }

    private static void MoveSchemaItem(
        Package targetPackage,
        SchemaItemGroup targetGroup,
        ISchemaItem item
    )
    {
        if (item.ParentItem == null)
        {
            item.Group = targetGroup;
        }
        if (IsItemOfPackage(item, targetPackage))
        {
            item.Persist();
            return;
        }
        MoveSchemaItem(item, targetPackage);
    }

    private static bool IsItemOfPackage(IBrowserNode2 node, Package package)
    {
        return node is ISchemaItem item && item.SchemaExtensionId == package.Id;
    }

    private static void MoveSchemaItem(ISchemaItem item, Package targetPackage)
    {
        item.SetExtensionRecursive(targetPackage);
        if (item is PanelControlSet { PanelControl: { } panelControl })
        {
            panelControl.SetExtensionRecursive(targetPackage);
            panelControl.Persist();
        }
        item.Persist();
    }

    private static void MoveGroupRecursive(
        Package targetPackage,
        SchemaItemGroup targetGroup,
        SchemaItemGroup group
    )
    {
        group.ParentGroup = targetGroup;
        foreach (SchemaItemGroup childGroup in group.ChildGroupsRecursive.ToList())
        {
            MoveGroupAndTopLevelItems(childGroup, targetPackage);
        }
        MoveGroupAndTopLevelItems(group, targetPackage);
    }

    private static void MoveGroupAndTopLevelItems(SchemaItemGroup group, Package targetPackage)
    {
        foreach (ISchemaItem child in group.ChildItems.ToList())
        {
            MoveSchemaItem(child, targetPackage);
        }
        group.Package = targetPackage;
        group.Persist();
    }

    private static List<ISchemaItem> GetTopLevelItems(IBrowserNode2 activeNode)
    {
        return activeNode switch
        {
            SchemaItemGroup activeGroup => activeGroup
                .ChildGroupsRecursive.Prepend(activeGroup)
                .SelectMany(group => group.ChildItems)
                .ToList(),
            ISchemaItem activeItem => [activeItem],
            _ => [],
        };
    }

    private static IEnumerable<ISchemaItem> GetItemAndDescendants(ISchemaItem item)
    {
        IEnumerable<ISchemaItem> itemAndDescendants = item
            .ChildItemsRecursive.Where(child => child.DerivedFrom == null)
            .Prepend(item);
        return item is PanelControlSet { PanelControl: { } panelControl }
            ? itemAndDescendants.Append(panelControl)
            : itemAndDescendants;
    }

    private static bool IsPackageIncluded(Package package, Guid packageId)
    {
        return package.Id == packageId
            || package.IncludedPackages.Any(included => included.Id == packageId);
    }

    private static bool IsGroupOrItsDescendant(SchemaItemGroup group, SchemaItemGroup ancestor)
    {
        for (SchemaItemGroup current = group; current != null; current = current.ParentGroup)
        {
            if (current.Id == ancestor.Id)
            {
                return true;
            }
        }
        return false;
    }

    private static string FormatRefusal(
        string template,
        ISchemaItem item,
        Package targetPackage,
        List<ISchemaItem> blockingItems
    )
    {
        List<ISchemaItem> sortedItems = blockingItems
            .OrderBy(blockingItem => blockingItem.Package.Name, StringComparer.Ordinal)
            .ThenBy(GetPath, StringComparer.Ordinal)
            .ToList();
        List<string> lines = sortedItems
            .Take(MaxListedItems)
            .Select(blockingItem =>
                string.Format(
                    Strings.ItemInPackage,
                    blockingItem.ModelDescription() ?? blockingItem.ItemType,
                    GetPath(blockingItem),
                    blockingItem.Package.Name
                )
            )
            .ToList();
        if (sortedItems.Count > MaxListedItems)
        {
            lines.Add(string.Format(Strings.MoreItems, sortedItems.Count - MaxListedItems));
        }
        string packageNames = string.Join(
            ", ",
            sortedItems.Select(blockingItem => blockingItem.Package.Name).Distinct()
        );
        return string.Format(template, item.Name, targetPackage.Name, packageNames)
            + "\n"
            + string.Join("\n", lines);
    }

    private static string GetPath(ISchemaItem item)
    {
        try
        {
            return item.Path.Replace(Path.DirectorySeparatorChar, '/');
        }
        catch (Exception)
        {
            return item.Name;
        }
    }
}
