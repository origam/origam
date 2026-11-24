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
using System.Linq;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelTransfer;

public class MoveToPackage(SchemaService schema, IPersistenceService persistenceService)
{
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
        List<ISchemaItem> movingItems = GetMovingItems(activeNode);
        HashSet<Guid> movingIds = movingItems.Select(item => item.Id).ToHashSet();
        List<ISchemaItem> dependenciesInPackagesNotReferencedByTargetPackage = movingItems
            .SelectMany(item => item.GetDependencies(ignoreErrors: true))
            .Where(item =>
                item != null
                && !movingIds.Contains(item.Id)
                && !IsPackageIncluded(targetPackage, item.SchemaExtensionId)
            )
            .DistinctBy(item => item.Id)
            .ToList();
        if (dependenciesInPackagesNotReferencedByTargetPackage.Count != 0)
        {
            return string.Format(
                Strings.ErrorDependenciesInPackagesNotReferencedByTargetPackage,
                targetPackage.Name,
                FormatToIdList(dependenciesInPackagesNotReferencedByTargetPackage)
            );
        }
        if (!ReferenceIndexManager.Initialized)
        {
            return Strings.ErrorReferenceIndexNotReady;
        }
        List<ISchemaItem> usagesInPackagesWhichDontDependOnTargetPackage = movingItems
            .SelectMany(item => item.GetUsage())
            .Where(item =>
                !movingIds.Contains(item.Id) && !IsPackageIncluded(item.Package, targetPackage.Id)
            )
            .DistinctBy(item => item.Id)
            .ToList();
        if (usagesInPackagesWhichDontDependOnTargetPackage.Count != 0)
        {
            return string.Format(
                Strings.ErrorUsagesInPackagesWhichDontDependOnTargetPackage,
                targetPackage.Name,
                FormatToIdList(usagesInPackagesWhichDontDependOnTargetPackage)
            );
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
        MoveSchemaItem(item, targetPackage);
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

    private static List<ISchemaItem> GetMovingItems(IBrowserNode2 activeNode)
    {
        var items = new List<ISchemaItem>();
        if (activeNode is SchemaItemGroup activeGroup)
        {
            foreach (SchemaItemGroup group in activeGroup.ChildGroupsRecursive.Prepend(activeGroup))
            {
                foreach (ISchemaItem child in group.ChildItems)
                {
                    AddItemAndDescendants(child, items);
                }
            }
        }
        else if (activeNode is ISchemaItem activeItem)
        {
            AddItemAndDescendants(activeItem, items);
        }
        return items;
    }

    private static void AddItemAndDescendants(ISchemaItem item, List<ISchemaItem> items)
    {
        items.Add(item);
        items.AddRange(item.ChildItemsRecursive.Where(child => child.DerivedFrom == null));
        if (item is PanelControlSet { PanelControl: { } panelControl })
        {
            items.Add(panelControl);
        }
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

    private static string FormatToIdList(List<ISchemaItem> schemaItems)
    {
        return "[" + string.Join("\n", schemaItems.Select(schemaItem => schemaItem.Id)) + "]";
    }
}
