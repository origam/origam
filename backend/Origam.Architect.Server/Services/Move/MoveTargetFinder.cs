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

using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class MoveTargetFinder(SchemaService schemaService, MoveRuleEvaluator ruleEvaluator)
{
    public MoveTargetsResult Find(IBrowserNode2 source)
    {
        Package activePackage = MovePreconditions.RequireActivePackage(schemaService);
        var result = new MoveTargetsResult { Targets = [] };
        if (
            source is not ISchemaItem item
            // RootProvider is only set when the item is reached through a provider.
            || item.RootProvider is not AbstractSchemaItemProvider provider
        )
        {
            return result;
        }

        result.IsSourceInActivePackage = schemaService.CanEditItem(item);
        Dictionary<Guid, string> packageNames = schemaService.LoadedPackages.ToDictionary(
            package => package.Id,
            package => package.Name
        );
        int examined = 0;
        foreach (IBrowserNode2 candidate in GetMoveCandidates(item, provider))
        {
            if (examined == MoveLimits.MaxCandidates)
            {
                result.IsTruncated = true;
                break;
            }
            examined++;

            (bool canMove, bool canCopy) = ruleEvaluator.EvaluateBothModes(item, candidate);
            if (!canMove && !canCopy)
            {
                continue;
            }

            if (result.Targets.Count == MoveLimits.MaxTargets)
            {
                result.IsTruncated = true;
                break;
            }

            result.Targets.Add(
                ToMoveTarget(
                    item,
                    candidate,
                    provider,
                    canMove,
                    canCopy,
                    packageNames,
                    activePackage.Id
                )
            );
        }

        return result;
    }

    private static IEnumerable<IBrowserNode2> GetMoveCandidates(
        ISchemaItem item,
        AbstractSchemaItemProvider provider
    )
    {
        // Providers and groups only ever accept top level items.
        if (item.ParentItem == null)
        {
            yield return provider;
            foreach (SchemaItemGroup group in provider.ChildGroups)
            {
                yield return group;
                foreach (SchemaItemGroup childGroup in group.ChildGroupsRecursive)
                {
                    yield return childGroup;
                }
            }
        }

        // Without a CanMove override no item can become a parent.
        if (
            item.GetType().GetMethod(nameof(ISchemaItem.CanMove))?.DeclaringType
            == typeof(AbstractSchemaItem)
        )
        {
            yield break;
        }

        // ChildItems merges inherited items in.
        IEnumerable<ISchemaItem> descendants = WalkChildItems(provider.ChildItems);
        foreach (ISchemaItem candidate in descendants.DistinctBy(node => node.Id))
        {
            yield return candidate;
        }
    }

    // ChildItemsRecursive materializes the whole provider. The depth cap is what breaks
    // a cycle, DistinctBy is lazy and inheritance can make an item its own descendant.
    private static IEnumerable<ISchemaItem> WalkChildItems(
        IEnumerable<ISchemaItem> items,
        int depth = 0
    )
    {
        if (depth == MoveLimits.MaxChildWalkDepth)
        {
            yield break;
        }

        foreach (ISchemaItem item in items)
        {
            yield return item;
            foreach (ISchemaItem descendant in WalkChildItems(item.ChildItems, depth + 1))
            {
                yield return descendant;
            }
        }
    }

    private static MoveTargetResult ToMoveTarget(
        ISchemaItem item,
        IBrowserNode2 candidate,
        AbstractSchemaItemProvider provider,
        bool canMove,
        bool canCopy,
        Dictionary<Guid, string> packageNames,
        Guid activePackageId
    )
    {
        Guid packageId = candidate switch
        {
            SchemaItemGroup group => group.SchemaExtensionId,
            ISchemaItem schemaItem => schemaItem.SchemaExtensionId,
            _ => activePackageId,
        };
        (string path, int depth) = GetTargetLocation(candidate, provider);
        return new MoveTargetResult
        {
            Id = candidate.NodeId,
            NodeText = TreeNode.ToNodeText(candidate),
            Key = TreeNode.ToTreeNodeId(candidate),
            Path = path,
            Depth = depth,
            PackageName = packageNames.GetValueOrDefault(packageId, defaultValue: ""),
            IsInActivePackage = packageId == activePackageId,
            IsCurrentLocation = MoveRuleEvaluator.IsCurrentLocation(item, candidate),
            CanMove = canMove,
            CanCopy = canCopy,
        };
    }

    // ISchemaItem.Path skips the group, so path and depth come from one walk.
    private static (string Path, int Depth) GetTargetLocation(
        IBrowserNode2 candidate,
        AbstractSchemaItemProvider provider
    )
    {
        var segments = new List<string>();
        SchemaItemGroup group = candidate as SchemaItemGroup;
        for (
            ISchemaItem item = candidate as ISchemaItem;
            item != null && segments.Count < MoveLimits.MaxParentWalkDepth;
            item = item.ParentItem
        )
        {
            segments.Add(item.Name);
            group = item.Group;
        }

        while (group != null && segments.Count < MoveLimits.MaxParentWalkDepth)
        {
            segments.Add(group.Name);
            group = group.ParentGroup;
        }

        segments.Add(provider.NodeText);
        segments.Reverse();
        return (string.Join(separator: "/", segments), segments.Count - 1);
    }
}
