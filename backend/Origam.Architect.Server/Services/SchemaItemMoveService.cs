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
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public enum DropKind
{
    None,
    ToRootProvider,
    ToGroup,
    ToParentNode,
}

public class DropDecision
{
    public DropKind Kind { get; init; }
    public string ErrorMessage { get; init; }
    public SchemaItemGroup Group { get; init; }
    public ISchemaItem TargetItem { get; init; }

    public bool IsAllowed => Kind != DropKind.None;

    public static DropDecision Rejected(string message) =>
        new() { Kind = DropKind.None, ErrorMessage = message };

    public static DropDecision ToProvider() => new() { Kind = DropKind.ToRootProvider };

    public static DropDecision ToGroup(SchemaItemGroup group) =>
        new() { Kind = DropKind.ToGroup, Group = group };

    public static DropDecision ToParent(ISchemaItem targetItem) =>
        new() { Kind = DropKind.ToParentNode, TargetItem = targetItem };
}

// The drop rules follow the WinForms architect in Origam.Workbench/ExpressionBrowser.cs.
public class SchemaItemMoveService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    ModelTransactionRunner transactionRunner,
    TreeNodeFactory treeNodeFactory,
    IDocumentationService documentationService
)
{
    private const int MaxParentWalkDepth = 200;

    private IPersistenceProvider PersistenceProvider => persistenceService.SchemaProvider;

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

    // Non persistent folder nodes carry the NodeId of their owning item.
    public IBrowserNode2 Resolve(NodeRefModel reference)
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

    public List<DropTargetResult> GetDropTargets(
        NodeRefModel sourceReference,
        List<NodeRefModel> targetReferences
    )
    {
        IBrowserNode2 source = Resolve(sourceReference);
        var results = new List<DropTargetResult>();
        if (targetReferences == null)
        {
            return results;
        }

        foreach (NodeRefModel targetReference in targetReferences)
        {
            IBrowserNode2 target = source == null ? null : Resolve(targetReference);
            results.Add(
                new DropTargetResult
                {
                    Id = ToNodeKey(targetReference),
                    CanMove = target != null && Evaluate(source, target, isCopy: false).IsAllowed,
                    CanCopy = target != null && Evaluate(source, target, isCopy: true).IsAllowed,
                }
            );
        }

        return results;
    }

    public MoveNodeResult Move(
        NodeRefModel sourceReference,
        NodeRefModel targetReference,
        bool isCopy
    )
    {
        if (schemaService.ActiveExtension == null)
        {
            throw new UserOrigamException(Strings.Move_NoActivePackage);
        }

        IBrowserNode2 source =
            Resolve(sourceReference) ?? throw new UserOrigamException(Strings.Move_SourceNotFound);
        IBrowserNode2 target =
            Resolve(targetReference) ?? throw new UserOrigamException(Strings.Move_TargetNotFound);

        // GetDropTargets is only advisory, the model may have changed since then.
        DropDecision decision = Evaluate(source, target, isCopy);
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

    public DropDecision Evaluate(IBrowserNode2 source, IBrowserNode2 target, bool isCopy)
    {
        if (source is not ISchemaItem item)
        {
            return DropDecision.Rejected(Strings.Move_SourceNotMovable);
        }

        if (!item.IsPersisted)
        {
            return DropDecision.Rejected(string.Format(Strings.Move_SourceNotPersisted, item.Name));
        }

        if (target == null)
        {
            return DropDecision.Rejected(Strings.Move_TargetNotFound);
        }

        // A copy always lands in the active package, a move keeps the original one.
        if (!isCopy && !schemaService.CanEditItem(item))
        {
            return DropDecision.Rejected(
                string.Format(Strings.Move_NotInActivePackage, item.Name, item.PackageName)
            );
        }

        int selfDepth = GetAncestorDepth(target, item);
        if (selfDepth == 0)
        {
            return DropDecision.Rejected(Strings.Move_TargetIsSource);
        }

        if (selfDepth > 0)
        {
            return DropDecision.Rejected(
                string.Format(Strings.Move_TargetIsDescendant, item.Name, target.NodeText)
            );
        }

        // ISchemaItem also implements ISchemaItemProvider, hence the concrete base class.
        if (target is AbstractSchemaItemProvider targetProvider)
        {
            return EvaluateProviderDrop(item, targetProvider);
        }

        if (target is SchemaItemGroup targetGroup)
        {
            return EvaluateGroupDrop(item, targetGroup);
        }

        if (target is ISchemaItem targetItem && item.CanMove(target))
        {
            return DropDecision.ToParent(targetItem);
        }

        return DropDecision.Rejected(
            string.Format(Strings.Move_NotAllowed, item.Name, target.NodeText)
        );
    }

    private static DropDecision EvaluateProviderDrop(
        ISchemaItem item,
        AbstractSchemaItemProvider provider
    )
    {
        if (item.RootProvider == null || provider.NodeId != item.RootProvider.NodeId)
        {
            return DropDecision.Rejected(
                string.Format(Strings.Move_NotAllowed, item.Name, provider.NodeText)
            );
        }

        if (item.ParentItem != null)
        {
            return DropDecision.Rejected(
                string.Format(Strings.Move_ProviderRequiresTopLevelItem, provider.NodeText)
            );
        }

        return DropDecision.ToProvider();
    }

    private static DropDecision EvaluateGroupDrop(ISchemaItem item, SchemaItemGroup group)
    {
        // Group.RootProvider is transient and empty here, RootItemType is persisted.
        if (
            item.ParentItem != null
            || item.RootProvider is not AbstractSchemaItemProvider provider
            || !string.Equals(group.RootItemType, provider.RootItemType, StringComparison.Ordinal)
        )
        {
            return DropDecision.Rejected(
                string.Format(Strings.Move_NotAllowed, item.Name, group.NodeText)
            );
        }

        return DropDecision.ToGroup(group);
    }

    // 0 when the target is the item itself, positive for its descendants, -1 otherwise.
    private static int GetAncestorDepth(IBrowserNode2 target, ISchemaItem item)
    {
        var current = target as ISchemaItem;
        for (int depth = 0; current != null && depth < MaxParentWalkDepth; depth++)
        {
            if (current.Id == item.Id)
            {
                return depth;
            }
            current = current.ParentItem;
        }

        return -1;
    }

    private ISchemaItem MoveExisting(ISchemaItem item, DropDecision decision)
    {
        transactionRunner.Run(() =>
        {
            ApplyDecision(item, decision);
            item.Persist();
        });
        return item;
    }

    private ISchemaItem Copy(ISchemaItem original, DropDecision decision)
    {
        ISchemaItemProvider rootProvider = original.RootProvider;
        // Clone() adds a top level clone straight into RootProvider.ChildItems, so the
        // provider cache has to be reset once the clone found its real parent.
        var clone = (ISchemaItem)original.Clone();
        bool wasTopLevel = clone.ParentItem == null;
        try
        {
            return transactionRunner.Run(() =>
            {
                ApplyDecision(clone, decision);
                clone.SetExtensionRecursive(schemaService.ActiveExtension);
                clone.Name = GetUniqueName(clone);
                PersistClone(clone);
                if (wasTopLevel && decision.Kind == DropKind.ToParentNode)
                {
                    rootProvider?.ClearCache();
                }
                return clone;
            });
        }
        catch
        {
            rootProvider?.ClearCache();
            decision.TargetItem?.ClearCache();
            throw;
        }
    }

    private static void ApplyDecision(ISchemaItem item, DropDecision decision)
    {
        switch (decision.Kind)
        {
            case DropKind.ToRootProvider:
            {
                item.Group = null;
                break;
            }
            case DropKind.ToGroup:
            {
                item.Group = decision.Group;
                break;
            }
            case DropKind.ToParentNode:
            {
                item.ParentNode = decision.TargetItem;
                if (decision.TargetItem.IsAbstract && !item.IsAbstract)
                {
                    item.IsAbstract = true;
                }
                break;
            }
        }
    }

    // Two passes, the second one stores the model after UpdateReferences repointed
    // the references from the original to the clone.
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
