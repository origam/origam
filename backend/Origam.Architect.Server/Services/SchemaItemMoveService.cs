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

using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public enum MoveDestination
{
    None,
    RootProvider,
    Group,
    ParentItem,
}

public class MoveDecision
{
    public MoveDestination Kind { get; init; }
    public string ErrorMessage { get; init; }
    public SchemaItemGroup Group { get; init; }
    public ISchemaItem TargetItem { get; init; }

    public bool IsAllowed => Kind != MoveDestination.None;

    public static MoveDecision Rejected(string message) =>
        new() { Kind = MoveDestination.None, ErrorMessage = message };

    public static MoveDecision ToRootProvider() => new() { Kind = MoveDestination.RootProvider };

    public static MoveDecision ToGroup(SchemaItemGroup group) =>
        new() { Kind = MoveDestination.Group, Group = group };

    public static MoveDecision ToParentItem(ISchemaItem targetItem) =>
        new() { Kind = MoveDestination.ParentItem, TargetItem = targetItem };
}

public class SchemaItemMoveService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    ModelTransactionRunner transactionRunner,
    TreeNodeFactory treeNodeFactory,
    IDocumentationService documentationService,
    TabService tabService
)
{
    private const int MaxParentWalkDepth = 200;
    private const int MaxMoveTargets = 500;
    private const int MaxMoveCandidates = 5000;
    private const string PanelControlType = "Origam.Gui.Win.AsPanel";

    private IPersistenceProvider PersistenceProvider => persistenceService.SchemaProvider;

    private void RequireActivePackage()
    {
        if (schemaService.ActiveExtension == null)
        {
            throw new UserOrigamException(Strings.Move_NoActivePackage);
        }
    }

    public ISchemaItemProvider GetRootProviderById(string id)
    {
        if (schemaService.ActiveExtension == null)
        {
            return null;
        }

        return schemaService
            .ActiveExtension.ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .SelectMany(group => group.ChildNodes().Cast<ISchemaItemProvider>())
            .FirstOrDefault(provider => provider.NodeId == id);
    }

    public IBrowserNode2 ResolveNode(NodeRefModel reference)
    {
        if (reference == null || string.IsNullOrWhiteSpace(reference.Id))
        {
            return null;
        }

        if (Guid.TryParse(reference.Id, out Guid id))
        {
            var node = PersistenceProvider.RetrieveInstance<IBrowserNode2>(
                id,
                useCache: true,
                throwNotFoundException: false
            );
            if (node == null || !reference.IsNonPersistentItem)
            {
                return node;
            }

            return new NonpersistentSchemaItemNode
            {
                NodeText = reference.NodeText,
                ParentNode = node,
            };
        }

        return GetRootProviderById(reference.Id) as IBrowserNode2;
    }

    public List<MoveVerdictResult> GetMoveVerdicts(
        NodeRefModel sourceReference,
        List<NodeRefModel> targetReferences
    )
    {
        RequireActivePackage();
        IBrowserNode2 source = ResolveNode(sourceReference);
        var results = new List<MoveVerdictResult>();
        if (targetReferences == null)
        {
            return results;
        }

        foreach (NodeRefModel targetReference in targetReferences)
        {
            if (targetReference == null)
            {
                continue;
            }

            IBrowserNode2 target = source == null ? null : ResolveNode(targetReference);
            (bool canMove, bool canCopy) = EvaluateBothModes(source, target);
            results.Add(
                new MoveVerdictResult
                {
                    Key = ToNodeKey(targetReference),
                    CanMove = canMove,
                    CanCopy = canCopy,
                }
            );
        }

        return results;
    }

    public MoveTargetsResult GetMoveTargets(NodeRefModel sourceReference)
    {
        RequireActivePackage();
        var result = new MoveTargetsResult { Targets = [] };
        if (
            ResolveNode(sourceReference) is not ISchemaItem item
            // RootProvider is only set when the item is reached through a provider.
            || item.RootProvider is not AbstractSchemaItemProvider provider
        )
        {
            return result;
        }

        Dictionary<Guid, string> packageNames = schemaService.LoadedPackages.ToDictionary(
            package => package.Id,
            package => package.Name
        );
        int examined = 0;
        foreach (IBrowserNode2 candidate in GetMoveCandidates(item, provider))
        {
            // A provider can hold tens of thousands of items, the walk has to end somewhere.
            if (examined == MaxMoveCandidates)
            {
                result.IsTruncated = true;
                break;
            }
            examined++;

            (bool canMove, bool canCopy) = EvaluateBothModes(item, candidate);
            if (!canMove && !canCopy)
            {
                continue;
            }

            if (result.Targets.Count == MaxMoveTargets)
            {
                result.IsTruncated = true;
                break;
            }

            result.Targets.Add(
                ToMoveTarget(item, candidate, provider, canMove, canCopy, packageNames)
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

        // Without a CanMove override no item can become a parent, so skip the walk entirely.
        if (
            item.GetType().GetMethod(nameof(ISchemaItem.CanMove))?.DeclaringType
            == typeof(AbstractSchemaItem)
        )
        {
            yield break;
        }

        // ChildItems merges inherited items in, one instance shows up under every descendant.
        IEnumerable<ISchemaItem> descendants = WalkChildItems(provider.ChildItems);
        foreach (ISchemaItem candidate in descendants.DistinctBy(node => node.Id))
        {
            yield return candidate;
        }
    }

    // ChildItemsRecursive materializes the whole provider, this stops where the caller does.
    private static IEnumerable<ISchemaItem> WalkChildItems(IEnumerable<ISchemaItem> items)
    {
        foreach (ISchemaItem item in items)
        {
            yield return item;
            foreach (ISchemaItem descendant in WalkChildItems(item.ChildItems))
            {
                yield return descendant;
            }
        }
    }

    private MoveTargetResult ToMoveTarget(
        ISchemaItem item,
        IBrowserNode2 candidate,
        AbstractSchemaItemProvider provider,
        bool canMove,
        bool canCopy,
        Dictionary<Guid, string> packageNames
    )
    {
        Guid packageId = candidate switch
        {
            SchemaItemGroup group => group.SchemaExtensionId,
            ISchemaItem schemaItem => schemaItem.SchemaExtensionId,
            _ => schemaService.ActiveExtension.Id,
        };
        return new MoveTargetResult
        {
            Id = candidate.NodeId,
            NodeText = TreeNode.ToNodeText(candidate),
            Key = TreeNode.ToTreeNodeId(candidate),
            Path = GetTargetPath(candidate, provider),
            PackageName = packageNames.GetValueOrDefault(packageId),
            IsInActivePackage = packageId == schemaService.ActiveExtension.Id,
            IsCurrentLocation = IsCurrentLocation(item, candidate),
            CanMove = canMove,
            CanCopy = canCopy,
        };
    }

    private static string GetTargetPath(
        IBrowserNode2 candidate,
        AbstractSchemaItemProvider provider
    )
    {
        string path = candidate switch
        {
            SchemaItemGroup group => group.Path,
            ISchemaItem schemaItem => schemaItem.Path,
            _ => null,
        };
        if (path == null)
        {
            return provider.NodeText;
        }

        string separator = System.IO.Path.DirectorySeparatorChar.ToString();
        return provider.NodeText + "/" + path.Replace(separator, newValue: "/");
    }

    private static bool IsCurrentLocation(ISchemaItem item, IBrowserNode2 candidate)
    {
        return candidate switch
        {
            AbstractSchemaItemProvider => item.ParentItem == null && item.Group == null,
            SchemaItemGroup group => item.Group != null && item.Group.Id == group.Id,
            ISchemaItem parent => item.ParentItem != null && item.ParentItem.Id == parent.Id,
            _ => false,
        };
    }

    public MoveNodeResult Move(
        NodeRefModel sourceReference,
        NodeRefModel targetReference,
        bool isCopy
    )
    {
        RequireActivePackage();
        IBrowserNode2 source =
            ResolveNode(sourceReference)
            ?? throw new UserOrigamException(Strings.Move_SourceNotFound);
        IBrowserNode2 target =
            ResolveNode(targetReference)
            ?? throw new UserOrigamException(Strings.Move_TargetNotFound);

        MoveDecision decision = Evaluate(source, target, isCopy);
        if (!decision.IsAllowed)
        {
            throw new UserOrigamException(decision.ErrorMessage);
        }

        var original = (ISchemaItem)source;
        ISchemaItem result = isCopy ? Copy(original, decision) : MoveExisting(original, decision);
        return new MoveNodeResult
        {
            Node = treeNodeFactory.Create(result),
            ParentNodeIds = SearchService.GetParentNodeIds(result, SearchService.GetRoot(result)),
        };
    }

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

        // A copy lands in the active package, a move keeps the original one.
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
                string.Format(Strings.Move_TargetChainUnknown, item.Name, target.NodeText)
            );
        }

        if (selfDepth == 0)
        {
            return MoveDecision.Rejected(Strings.Move_TargetIsSource);
        }

        if (selfDepth > 0 && !isCopy)
        {
            return MoveDecision.Rejected(
                string.Format(Strings.Move_TargetIsDescendant, item.Name, target.NodeText)
            );
        }

        MoveDecision decision = EvaluateDestination(item, target);
        if (decision.IsAllowed && !isCopy && IsCurrentLocation(item, target))
        {
            return MoveDecision.Rejected(
                string.Format(Strings.Move_TargetIsCurrentLocation, item.Name, target.NodeText)
            );
        }

        return decision;
    }

    public (bool CanMove, bool CanCopy) EvaluateBothModes(
        IBrowserNode2 source,
        IBrowserNode2 target
    )
    {
        return (
            Evaluate(source, target, isCopy: false).IsAllowed,
            Evaluate(source, target, isCopy: true).IsAllowed
        );
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
            string.Format(Strings.Move_NotAllowed, item.Name, target.NodeText)
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
                string.Format(Strings.Move_NotAllowed, item.Name, provider.NodeText)
            );
        }

        if (item.ParentItem != null)
        {
            return MoveDecision.Rejected(
                string.Format(Strings.Move_ProviderRequiresTopLevelItem, provider.NodeText)
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
                string.Format(Strings.Move_NotAllowed, item.Name, group.NodeText)
            );
        }

        return MoveDecision.ToGroup(group);
    }

    // Null means the parent chain could not be walked and the relation stays unknown.
    private static int? GetAncestorDepth(IBrowserNode2 target, ISchemaItem item)
    {
        var current = target as ISchemaItem;
        for (int depth = 0; current != null; depth++)
        {
            if (depth == MaxParentWalkDepth)
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

    private ISchemaItem MoveExisting(ISchemaItem item, MoveDecision decision)
    {
        ISchemaItemProvider rootProvider = item.RootProvider;
        ISchemaItem oldParent = item.ParentItem;
        SchemaItemGroup oldGroup = item.Group;
        bool oldIsAbstract = item.IsAbstract;
        Package oldPackage = item.Package;
        Package targetPackage = ResolveTargetPackage(decision, oldPackage);
        bool crossPackage = targetPackage != null && targetPackage.Id != oldPackage.Id;
        if (crossPackage)
        {
            CheckDependenciesOrThrow(item, targetPackage);
            CheckUsagesOrThrow(item, targetPackage);
        }
        try
        {
            transactionRunner.Run(() =>
            {
                ApplyDecision(item, decision);
                if (crossPackage)
                {
                    SetPackageRecursive(item, targetPackage);
                }
                item.Persist();
                if (crossPackage)
                {
                    PersistPanelControl(item);
                }
            });
        }
        catch
        {
            // The transaction rolls back the files only, not the in memory item.
            item.ParentItem = oldParent;
            item.Group = oldGroup;
            item.IsAbstract = oldIsAbstract;
            if (crossPackage)
            {
                SetPackageRecursive(item, oldPackage);
            }
            ClearLocationCaches(rootProvider, oldParent, decision.TargetItem);
            throw;
        }

        if (oldParent == null && decision.Kind == MoveDestination.ParentItem)
        {
            rootProvider?.ClearCache();
        }

        return item;
    }

    private ISchemaItem Copy(ISchemaItem original, MoveDecision decision)
    {
        ISchemaItemProvider rootProvider = original.RootProvider;
        // A drop on the root provider has no package of its own, that copy lands in the active one.
        Package targetPackage = ResolveTargetPackage(decision, schemaService.ActiveExtension);
        if (targetPackage.Id != original.SchemaExtensionId)
        {
            // The copy has the same dependencies as the original, usages do not exist yet.
            CheckDependenciesOrThrow(original, targetPackage);
        }
        // Clone() puts a top level clone straight into RootProvider.ChildItems.
        var clone = (ISchemaItem)original.Clone();
        bool wasTopLevel = clone.ParentItem == null;
        try
        {
            return transactionRunner.Run(() =>
            {
                ApplyDecision(clone, decision);
                clone.SetExtensionRecursive(targetPackage);
                clone.Name = GetUniqueName(clone);
                PersistClone(clone);
                CreatePanelControl(clone, targetPackage);
                if (wasTopLevel && decision.Kind == MoveDestination.ParentItem)
                {
                    rootProvider?.ClearCache();
                }
                return clone;
            });
        }
        catch
        {
            ClearLocationCaches(rootProvider, original.ParentItem, decision.TargetItem);
            throw;
        }
    }

    private static void ClearLocationCaches(
        ISchemaItemProvider rootProvider,
        ISchemaItem oldParent,
        ISchemaItem newParent
    )
    {
        rootProvider?.ClearCache();
        oldParent?.ClearCache();
        newParent?.ClearCache();
    }

    private static Package ResolveTargetPackage(MoveDecision decision, Package fallback)
    {
        return decision.Kind switch
        {
            MoveDestination.Group => decision.Group.Package,
            MoveDestination.ParentItem => decision.TargetItem.Package,
            _ => fallback,
        };
    }

    private static void SetPackageRecursive(ISchemaItem item, Package package)
    {
        item.SetExtensionRecursive(package);
        GetPanelControl(item)?.SetExtensionRecursive(package);
    }

    private static void PersistPanelControl(ISchemaItem item)
    {
        GetPanelControl(item)?.Persist();
    }

    // PanelControl finds the wrapper by the set's own id, so a clone has none. The old
    // Architect creates it when the cloned section is saved through ControlSetEditor.
    private void CreatePanelControl(ISchemaItem clone, Package targetPackage)
    {
        if (clone is not PanelControlSet panelControlSet || panelControlSet.PanelControl != null)
        {
            return;
        }

        ControlItem control = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .NewItem<ControlItem>(targetPackage.Id, group: null);
        control.Name = panelControlSet.Name;
        control.IsComplexType = true;
        control.ControlType = typeof(PanelControlSet).ToString();
        control.ControlNamespace = typeof(PanelControlSet).Namespace;
        control.PanelControlSet = panelControlSet;
        control.ControlToolBoxVisibility = ControlToolBoxVisibility.FormDesigner;
        var ancestor = new SchemaItemAncestor
        {
            SchemaItem = control,
            Ancestor = tabService.GetControlByType(PanelControlType),
            PersistenceProvider = control.PersistenceProvider,
        };
        control.ThrowEventOnPersist = false;
        control.Persist();
        ancestor.Persist();
        control.ThrowEventOnPersist = true;
    }

    // A screen section is wrapped by a ControlItem living in another provider and file.
    private static ISchemaItem GetPanelControl(ISchemaItem item)
    {
        return item is PanelControlSet panelControlSet ? panelControlSet.PanelControl : null;
    }

    private static void CheckDependenciesOrThrow(ISchemaItem item, Package targetPackage)
    {
        List<ISchemaItem> moved = GetItemsToMove(item);
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

    private void CheckUsagesOrThrow(ISchemaItem item, Package targetPackage)
    {
        List<ISchemaItem> moved = GetItemsToMove(item);
        HashSet<Guid> movedIds = moved.Select(movedItem => movedItem.Id).ToHashSet();
        Dictionary<Guid, HashSet<Guid>> reachablePackages =
            schemaService.LoadedPackages.ToDictionary(
                package => package.Id,
                GetReachablePackageIds
            );
        List<ISchemaItem> unreachable = moved
            .SelectMany(movedItem => movedItem.GetUsage())
            .Where(usage =>
                usage != null
                && !movedIds.Contains(usage.Id)
                && !(
                    reachablePackages.TryGetValue(usage.SchemaExtensionId, out var reachable)
                    && reachable.Contains(targetPackage.Id)
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

    private static void ApplyDecision(ISchemaItem item, MoveDecision decision)
    {
        switch (decision.Kind)
        {
            case MoveDestination.RootProvider:
            {
                item.Group = null;
                break;
            }
            case MoveDestination.Group:
            {
                item.Group = decision.Group;
                break;
            }
            case MoveDestination.ParentItem:
            {
                item.Group = null;
                item.ParentNode = decision.TargetItem;
                if (decision.TargetItem.IsAbstract && !item.IsAbstract)
                {
                    item.IsAbstract = true;
                }
                break;
            }
        }
    }

    // Two passes, the second one runs after UpdateReferences repointed the clone.
    private void PersistClone(ISchemaItem clone)
    {
        var oldKeys = new Dictionary<Guid, ModelElementKey>();
        foreach (ISchemaItem child in clone.ChildItemsRecursive)
        {
            if (child.OldPrimaryKey != null)
            {
                oldKeys[child.Id] = child.OldPrimaryKey;
            }
        }

        clone.ThrowEventOnPersist = false;
        clone.Persist();

        // Persist() clears OldPrimaryKey, UpdateReferences still needs it.
        foreach (ISchemaItem child in clone.ChildItemsRecursive)
        {
            if (oldKeys.TryGetValue(child.Id, out ModelElementKey oldKey))
            {
                child.OldPrimaryKey = oldKey;
            }
        }

        clone.UpdateReferences();
        clone.ThrowEventOnPersist = true;
        clone.Persist();

        List<ISchemaItem> items = clone.ChildItemsRecursive;
        items.Add(clone);
        documentationService.CloneDocumentation(items);
        clone.OldPrimaryKey = null;
    }

    private static string GetUniqueName(ISchemaItem clone)
    {
        HashSet<string> takenNames = GetSiblings(clone)
            .Where(sibling => sibling.Id != clone.Id)
            .Select(sibling => sibling.Name)
            .Where(name => name != null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (clone.Name == null || !takenNames.Contains(clone.Name))
        {
            return clone.Name;
        }

        string candidate = string.Format(Strings.CopyOfName, clone.Name);
        for (int counter = 2; takenNames.Contains(candidate); counter++)
        {
            candidate = string.Format(Strings.CopyOfNameNumbered, clone.Name, counter);
        }

        return candidate;
    }

    private static IEnumerable<ISchemaItem> GetSiblings(ISchemaItem item)
    {
        if (item.ParentItem != null)
        {
            return item.ParentItem.ChildItems;
        }

        if (item.RootProvider == null)
        {
            return [];
        }

        // Groups split the provider visually only, top level names stay unique.
        return item.RootProvider.ChildItems;
    }

    private static string ToNodeKey(NodeRefModel reference) => reference.Id + reference.NodeText;
}
