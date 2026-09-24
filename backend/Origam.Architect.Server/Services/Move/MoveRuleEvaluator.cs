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

public class MoveRuleEvaluator(SchemaService schemaService)
{
    public MoveDecision Evaluate(IBrowserNode2 source, IBrowserNode2 target, bool isCopy)
    {
        if (source is not ISchemaItem item)
        {
            return MoveDecision.Rejected(Strings.Move_SourceNotMovable);
        }

        if (!item.IsPersisted)
        {
            return MoveDecision.Rejected(string.Format(Strings.Move_SourceNotPersisted, item.Name));
        }

        if (target == null)
        {
            return MoveDecision.Rejected(Strings.Move_TargetNotFound);
        }

        if (!isCopy && !schemaService.CanEditItem(item))
        {
            return MoveDecision.Rejected(
                string.Format(Strings.Move_NotInActivePackage, item.Name, item.PackageName)
            );
        }

        int? selfDepth = GetAncestorDepth(target, item);
        if (selfDepth == null)
        {
            return MoveDecision.Rejected(
                string.Format(
                    Strings.Move_TargetChainUnknown,
                    item.Name,
                    TreeNode.ToNodeText(target)
                )
            );
        }

        if (selfDepth == 0)
        {
            return MoveDecision.Rejected(Strings.Move_TargetIsSource);
        }

        if (selfDepth > 0 && !isCopy)
        {
            return MoveDecision.Rejected(
                string.Format(
                    Strings.Move_TargetIsDescendant,
                    item.Name,
                    TreeNode.ToNodeText(target)
                )
            );
        }

        MoveDecision decision = EvaluateDestination(item, target);
        if (decision.IsAllowed && !isCopy && IsCurrentLocation(item, target))
        {
            return MoveDecision.Rejected(
                string.Format(
                    Strings.Move_TargetIsCurrentLocation,
                    item.Name,
                    TreeNode.ToNodeText(target)
                )
            );
        }

        return decision;
    }

    public (bool CanMove, bool CanCopy) EvaluateBothModes(
        IBrowserNode2 source,
        IBrowserNode2 target
    )
    {
        // A move is allowed only where a copy is.
        if (!Evaluate(source, target, isCopy: true).IsAllowed)
        {
            return (CanMove: false, CanCopy: false);
        }

        return (CanMove: Evaluate(source, target, isCopy: false).IsAllowed, CanCopy: true);
    }

    public static bool IsCurrentLocation(ISchemaItem item, IBrowserNode2 candidate)
    {
        return candidate switch
        {
            AbstractSchemaItemProvider => item.ParentItem == null && item.Group == null,
            SchemaItemGroup group => item.Group != null && item.Group.Id == group.Id,
            ISchemaItem parent => item.ParentItem != null && item.ParentItem.Id == parent.Id,
            _ => false,
        };
    }

    private static MoveDecision EvaluateDestination(ISchemaItem item, IBrowserNode2 target)
    {
        if (target is AbstractSchemaItemProvider targetProvider)
        {
            return EvaluateProviderTarget(item, targetProvider);
        }

        if (target is SchemaItemGroup targetGroup)
        {
            return EvaluateGroupTarget(item, targetGroup);
        }

        if (target is ISchemaItem targetItem && item.CanMove(target))
        {
            return MoveDecision.ToParentItem(targetItem);
        }

        return MoveDecision.Rejected(
            string.Format(Strings.Move_NotAllowed, item.Name, TreeNode.ToNodeText(target))
        );
    }

    private static MoveDecision EvaluateProviderTarget(
        ISchemaItem item,
        AbstractSchemaItemProvider provider
    )
    {
        if (item.RootProvider == null || provider.NodeId != item.RootProvider.NodeId)
        {
            return MoveDecision.Rejected(
                string.Format(Strings.Move_NotAllowed, item.Name, TreeNode.ToNodeText(provider))
            );
        }

        if (item.ParentItem != null)
        {
            return MoveDecision.Rejected(
                string.Format(
                    Strings.Move_ProviderRequiresTopLevelItem,
                    TreeNode.ToNodeText(provider)
                )
            );
        }

        return MoveDecision.ToRootProvider();
    }

    private static MoveDecision EvaluateGroupTarget(ISchemaItem item, SchemaItemGroup group)
    {
        // Group.RootProvider is transient here, RootItemType is persisted.
        if (
            item.ParentItem != null
            || item.RootProvider is not AbstractSchemaItemProvider provider
            || !string.Equals(group.RootItemType, provider.RootItemType, StringComparison.Ordinal)
        )
        {
            return MoveDecision.Rejected(
                string.Format(Strings.Move_NotAllowed, item.Name, TreeNode.ToNodeText(group))
            );
        }

        return MoveDecision.ToGroup(group);
    }

    // Null means the chain could not be walked.
    private static int? GetAncestorDepth(IBrowserNode2 target, ISchemaItem item)
    {
        var current = target as ISchemaItem;
        for (int depth = 0; current != null; depth++)
        {
            if (depth == MoveLimits.MaxParentWalkDepth)
            {
                return null;
            }
            if (current.Id == item.Id)
            {
                return depth;
            }
            current = current.ParentItem;
        }

        return -1;
    }
}
