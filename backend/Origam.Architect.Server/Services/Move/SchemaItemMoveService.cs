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

using Origam.Architect.Server.Exceptions;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.OrigamEngine.ModelTransfer;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class SchemaItemMoveService(
    SchemaService schemaService,
    MoveNodeResolver nodeResolver,
    SchemaItemDragDrop dragDrop,
    MoveToPackage moveToPackage,
    ModelTransactionRunner transactionRunner,
    PanelControlFactory panelControlFactory,
    GitNodeStatusService gitNodeStatusService,
    TreeNodeFactory treeNodeFactory,
    TabService tabService
)
{
    public List<MoveVerdictResult> GetMoveVerdicts(
        NodeRefModel sourceReference,
        List<NodeRefModel> targetReferences
    )
    {
        RequireActivePackage();
        var item = nodeResolver.Resolve(sourceReference) as ISchemaItem;
        return (targetReferences ?? [])
            .Where(targetReference => targetReference != null)
            .Select(targetReference =>
            {
                IBrowserNode2 target = item == null ? null : nodeResolver.Resolve(targetReference);
                return new MoveVerdictResult
                {
                    Key = targetReference.Id + targetReference.NodeText,
                    CanMove = target != null && dragDrop.CanDrop(item, target, isCopy: false),
                    CanCopy = target != null && dragDrop.CanDrop(item, target, isCopy: true),
                };
            })
            .ToList();
    }

    public MoveTargetsResult GetMoveTargets(NodeRefModel sourceReference, Guid? packageId)
    {
        Package activePackage = RequireActivePackage();
        IBrowserNode2 source = nodeResolver.Resolve(sourceReference);
        var result = new MoveTargetsResult { Targets = [], Packages = [] };
        if (GetRootProvider(source) is not AbstractSchemaItemProvider provider)
        {
            return result;
        }
        List<Package> packages = moveToPackage.GetPackages();
        Package package = GetPackage(source, packageId, packages);
        result.Packages = packages
            .Select(choice => ToPackageResult(choice, activePackage))
            .ToList();
        result.PackageId = package.Id;
        result.IsSourceInActivePackage = GetPackageId(source) == activePackage.Id;
        if (source is ISchemaItem item && package.Id == item.SchemaExtensionId)
        {
            foreach (IBrowserNode2 candidate in GetDropCandidates(item, provider))
            {
                bool canMove = dragDrop.CanDrop(item, candidate, isCopy: false);
                bool canCopy = dragDrop.CanDrop(item, candidate, isCopy: true);
                if (canMove || canCopy)
                {
                    result.Targets.Add(
                        ToMoveTarget(
                            source,
                            candidate,
                            provider,
                            canMove,
                            canCopy,
                            packages,
                            activePackage.Id
                        )
                    );
                }
            }
            return result;
        }
        result.PackageRefusal = moveToPackage.GetRefusal(source, package);
        foreach (IBrowserNode2 candidate in GetPackageTargets(source, package))
        {
            bool canCopy =
                source is ISchemaItem sourceItem
                && dragDrop.CanDrop(sourceItem, candidate, isCopy: true);
            result.Targets.Add(
                ToMoveTarget(
                    source,
                    candidate,
                    provider,
                    canMove: result.PackageRefusal == null,
                    canCopy,
                    packages,
                    activePackage.Id
                )
            );
        }
        return result;
    }

    public MoveNodeResult Move(
        NodeRefModel sourceReference,
        NodeRefModel targetReference,
        bool isCopy,
        Guid? packageId
    )
    {
        Package activePackage = RequireActivePackage();
        IBrowserNode2 source =
            nodeResolver.Resolve(sourceReference)
            ?? throw new UserOrigamException(Strings.Move_SourceNotFound);
        IBrowserNode2 target =
            nodeResolver.Resolve(targetReference)
            ?? throw new UserOrigamException(Strings.Move_TargetNotFound);
        if (
            GetRootProvider(source) is not AbstractSchemaItemProvider provider
            || (isCopy && source is not ISchemaItem)
        )
        {
            throw NotAllowed(source, target);
        }
        List<Package> packages = moveToPackage.GetPackages();
        MoveLocationResult sourceLocation = DescribeLocation(source, provider, packages);
        MovePackageResult sourcePackage = ToPackageResult(
            packages.FirstOrDefault(package => package.Id == GetPackageId(source)),
            activePackage
        );
        List<ISchemaItem> changedRoots = GetRootItems(source);
        IBrowserNode2 result =
            source is ISchemaItem item
            && (isCopy || (packageId ?? item.SchemaExtensionId) == item.SchemaExtensionId)
                ? Drop(item, target, isCopy, activePackage)
                : ChangePackage(source, target, GetPackage(source, packageId, packages));
        gitNodeStatusService.ClearCache();
        if (!isCopy)
        {
            foreach (
                ISchemaItem root in changedRoots
                    .Concat(GetRootItems(result))
                    .DistinctBy(root => root.Id)
            )
            {
                tabService.InvalidateTabsInRoot(root, changedByTabId: null);
            }
        }
        return new MoveNodeResult
        {
            Node = treeNodeFactory.Create(result),
            SourceLocation = sourceLocation,
            SourcePackage = sourcePackage,
            ParentNodeIds = GetParentNodeIds(result),
        };
    }

    private ISchemaItem Drop(
        ISchemaItem item,
        IBrowserNode2 target,
        bool isCopy,
        Package activePackage
    )
    {
        if (!isCopy && !schemaService.CanEditItem(item))
        {
            throw new UserOrigamException(
                string.Format(Strings.Move_NotInActivePackage, item.Name, item.PackageName)
            );
        }
        ISchemaItem result = dragDrop.Drop(item, target, isCopy) ?? throw NotAllowed(item, target);
        if (isCopy && result is PanelControlSet { PanelControl: null } panelControlSet)
        {
            transactionRunner.Run(() =>
                panelControlFactory.Create(panelControlSet, activePackage.Id)
            );
        }
        return result;
    }

    private IBrowserNode2 ChangePackage(IBrowserNode2 source, IBrowserNode2 target, Package package)
    {
        if (!GetPackageTargets(source, package).Any(candidate => candidate.NodeId == target.NodeId))
        {
            throw NotAllowed(source, target);
        }
        moveToPackage.Move(source, package, target as SchemaItemGroup);
        return source;
    }

    private IEnumerable<IBrowserNode2> GetPackageTargets(IBrowserNode2 source, Package package)
    {
        return moveToPackage.GetTargets(source, package);
    }

    private static IEnumerable<IBrowserNode2> GetDropCandidates(
        ISchemaItem item,
        AbstractSchemaItemProvider provider
    )
    {
        yield return provider;
        foreach (
            SchemaItemGroup group in provider.ChildGroups.SelectMany(group =>
                group.ChildGroupsRecursive.Prepend(group)
            )
        )
        {
            yield return group;
        }
        if (
            item.GetType().GetMethod(nameof(ISchemaItem.CanMove))?.DeclaringType
            == typeof(AbstractSchemaItem)
        )
        {
            yield break;
        }
        foreach (ISchemaItem candidate in GetItemsRecursive(provider.ChildItems))
        {
            yield return candidate;
        }
    }

    private static IEnumerable<ISchemaItem> GetItemsRecursive(IEnumerable<ISchemaItem> items)
    {
        return items
            .Where(item => item.DerivedFrom == null)
            .SelectMany(item => GetItemsRecursive(item.ChildItems).Prepend(item));
    }

    private static MoveTargetResult ToMoveTarget(
        IBrowserNode2 source,
        IBrowserNode2 candidate,
        AbstractSchemaItemProvider provider,
        bool canMove,
        bool canCopy,
        List<Package> packages,
        Guid activePackageId
    )
    {
        Guid packageId = GetOwningPackageId(candidate) ?? activePackageId;
        (string path, int depth) = GetLocation(candidate, provider);
        return new MoveTargetResult
        {
            Id = candidate.NodeId,
            NodeText = TreeNode.ToNodeText(candidate),
            Key = TreeNode.ToTreeNodeId(candidate),
            Path = path,
            Depth = depth,
            PackageName = GetPackageName(packageId, packages),
            IsInActivePackage = packageId == activePackageId,
            IsCurrentLocation = IsCurrentLocation(source, candidate),
            CanMove = canMove,
            CanCopy = canCopy,
        };
    }

    private static bool IsCurrentLocation(IBrowserNode2 source, IBrowserNode2 candidate)
    {
        return (source, candidate) switch
        {
            (SchemaItemGroup group, SchemaItemGroup target) => group.ParentGroup?.Id == target.Id,
            (ISchemaItem item, AbstractSchemaItemProvider) => item.ParentItem == null
                && item.Group == null,
            (ISchemaItem item, SchemaItemGroup target) => item.ParentItem == null
                && item.Group?.Id == target.Id,
            (ISchemaItem item, ISchemaItem target) => item.ParentItem?.Id == target.Id,
            _ => false,
        };
    }

    private static MoveLocationResult DescribeLocation(
        IBrowserNode2 source,
        AbstractSchemaItemProvider provider,
        List<Package> packages
    )
    {
        IBrowserNode2 location = source switch
        {
            ISchemaItem item => (IBrowserNode2)item.ParentItem
                ?? (IBrowserNode2)item.Group
                ?? provider,
            SchemaItemGroup group => (IBrowserNode2)group.ParentGroup ?? provider,
            _ => provider,
        };
        Guid? packageId = GetOwningPackageId(location);
        return new MoveLocationResult
        {
            Id = location.NodeId,
            NodeText = TreeNode.ToNodeText(location),
            Path = GetLocation(location, provider).Path,
            PackageName = packageId == null ? "" : GetPackageName(packageId.Value, packages),
        };
    }

    private static (string Path, int Depth) GetLocation(
        IBrowserNode2 node,
        AbstractSchemaItemProvider provider
    )
    {
        var segments = new List<string>();
        var group = node as SchemaItemGroup;
        for (var item = node as ISchemaItem; item != null; item = item.ParentItem)
        {
            segments.Add(item.Name);
            group = item.Group;
        }
        for (; group != null; group = group.ParentGroup)
        {
            segments.Add(group.Name);
        }
        segments.Add(provider.NodeText);
        segments.Reverse();
        return (string.Join(separator: "/", segments), segments.Count - 1);
    }

    private static List<string> GetParentNodeIds(IBrowserNode2 node)
    {
        if (node is SchemaItemGroup group)
        {
            if (group.RootProvider is not AbstractSchemaItemProvider provider)
            {
                return [];
            }
            var ids = new List<string>();
            for (
                SchemaItemGroup parent = group.ParentGroup;
                parent != null;
                parent = parent.ParentGroup
            )
            {
                ids.Add(parent.Id.ToString());
            }
            ids.Add(provider.NodeId);
            ids.Add(provider.Group);
            ids.Reverse();
            return ids;
        }
        try
        {
            var item = (ISchemaItem)node;
            return SchemaItemTreePath.GetParentNodeIds(item, SchemaItemTreePath.GetRoot(item));
        }
        catch (OrphanedSchemaReferenceException)
        {
            return [];
        }
    }

    private static List<ISchemaItem> GetRootItems(IBrowserNode2 node)
    {
        List<ISchemaItem> roots = node switch
        {
            ISchemaItem item => [item.RootItem],
            SchemaItemGroup group => group
                .ChildGroupsRecursive.Prepend(group)
                .SelectMany(childGroup => childGroup.ChildItems)
                .ToList(),
            _ => [],
        };
        return
        [
            .. roots,
            .. roots
                .OfType<PanelControlSet>()
                .Select(section => section.PanelControl)
                .Where(control => control != null),
        ];
    }

    private static ISchemaItemProvider GetRootProvider(IBrowserNode2 node)
    {
        return node switch
        {
            ISchemaItem { IsPersisted: true } item => item.RootProvider,
            SchemaItemGroup group => group.RootProvider,
            _ => null,
        };
    }

    private static Guid GetPackageId(IBrowserNode2 node)
    {
        return node is SchemaItemGroup group
            ? group.SchemaExtensionId
            : ((ISchemaItem)node).SchemaExtensionId;
    }

    private static Guid? GetOwningPackageId(IBrowserNode2 node)
    {
        return node switch
        {
            SchemaItemGroup group => group.SchemaExtensionId,
            ISchemaItem item => item.SchemaExtensionId,
            _ => null,
        };
    }

    private static Package GetPackage(IBrowserNode2 source, Guid? packageId, List<Package> packages)
    {
        Guid chosenPackageId = packageId ?? GetPackageId(source);
        return packages.FirstOrDefault(package => package.Id == chosenPackageId)
            ?? throw new UserOrigamException(
                string.Format(
                    Strings.Move_PackageNotAvailable,
                    TreeNode.ToNodeText(source),
                    chosenPackageId
                )
            );
    }

    private static string GetPackageName(Guid packageId, List<Package> packages)
    {
        return packages.FirstOrDefault(package => package.Id == packageId)?.Name ?? "";
    }

    private MovePackageResult ToPackageResult(Package package, Package activePackage)
    {
        return package == null
            ? null
            : new MovePackageResult
            {
                Id = package.Id,
                Name = package.Name,
                IsActive = package.Id == activePackage.Id,
            };
    }

    private Package RequireActivePackage()
    {
        return schemaService.ActiveExtension
            ?? throw new UserOrigamException(Strings.Move_NoActivePackage);
    }

    private static UserOrigamException NotAllowed(IBrowserNode2 source, IBrowserNode2 target)
    {
        return new UserOrigamException(
            string.Format(
                Strings.Move_NotAllowed,
                TreeNode.ToNodeText(source),
                TreeNode.ToNodeText(target)
            )
        );
    }
}
