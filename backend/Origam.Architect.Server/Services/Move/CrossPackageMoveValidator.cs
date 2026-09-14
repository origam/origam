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

using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class CrossPackageMoveValidator(SchemaService schemaService)
{
    public void CheckMoveOrThrow(ISchemaItem item, Package targetPackage)
    {
        List<ISchemaItem> moved = GetItemsToMove(item);
        CheckDependenciesOrThrow(item, moved, targetPackage);
        CheckUsagesOrThrow(item, moved, targetPackage);
    }

    // Usages of a copy do not exist yet.
    public void CheckCopyOrThrow(ISchemaItem original, Package targetPackage)
    {
        CheckDependenciesOrThrow(original, GetItemsToMove(original), targetPackage);
    }

    private static void CheckDependenciesOrThrow(
        ISchemaItem item,
        List<ISchemaItem> moved,
        Package targetPackage
    )
    {
        HashSet<Guid> movedIds = moved.Select(movedItem => movedItem.Id).ToHashSet();
        HashSet<Guid> reachableFromTarget = GetReachablePackageIds(targetPackage);
        List<ISchemaItem> unreachable = moved
            .SelectMany(movedItem => movedItem.GetDependencies(ignoreErrors: true))
            .Where(dependency =>
                dependency != null
                && !movedIds.Contains(dependency.Id)
                && !reachableFromTarget.Contains(dependency.SchemaExtensionId)
            )
            .ToList();
        if (unreachable.Count > 0)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.Move_DependenciesOutsideTargetPackage,
                    item.Name,
                    targetPackage.Name,
                    FormatItemList(unreachable)
                )
            );
        }
    }

    private void CheckUsagesOrThrow(
        ISchemaItem item,
        List<ISchemaItem> moved,
        Package targetPackage
    )
    {
        HashSet<Guid> movedIds = moved.Select(movedItem => movedItem.Id).ToHashSet();
        Dictionary<Guid, HashSet<Guid>> reachablePackages =
            schemaService.LoadedPackages.ToDictionary(
                package => package.Id,
                GetReachablePackageIds
            );
        List<ISchemaItem> unreachable = GetUsages(item, moved, targetPackage)
            .Where(usage =>
                usage != null
                && !movedIds.Contains(usage.Id)
                && !(
                    reachablePackages.TryGetValue(
                        usage.SchemaExtensionId,
                        out HashSet<Guid> reachable
                    ) && reachable.Contains(targetPackage.Id)
                )
            )
            .ToList();
        if (unreachable.Count > 0)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.Move_UsagesCannotReachTargetPackage,
                    item.Name,
                    targetPackage.Name,
                    FormatItemList(unreachable)
                )
            );
        }
    }

    // GetUsage throws a plain exception when the reference index is missing.
    private static List<ISchemaItem> GetUsages(
        ISchemaItem item,
        List<ISchemaItem> moved,
        Package targetPackage
    )
    {
        try
        {
            return moved.SelectMany(movedItem => movedItem.GetUsage()).ToList();
        }
        catch (Exception exception)
        {
            throw new UserOrigamException(
                string.Format(Strings.Move_UsagesNotChecked, item.Name, targetPackage.Name),
                exception.Message,
                exception
            );
        }
    }

    private static List<ISchemaItem> GetItemsToMove(ISchemaItem item)
    {
        List<ISchemaItem> items = item.ChildItemsRecursive;
        items.Add(item);
        return items;
    }

    private static HashSet<Guid> GetReachablePackageIds(Package package)
    {
        return package
            .IncludedPackages.Select(included => included.Id)
            .Append(package.Id)
            .ToHashSet();
    }

    private static string FormatItemList(IEnumerable<ISchemaItem> items)
    {
        return string.Join(separator: ", ", items.Select(item => item.Name).Distinct());
    }
}
