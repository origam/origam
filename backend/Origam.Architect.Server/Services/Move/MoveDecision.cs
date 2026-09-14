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

namespace Origam.Architect.Server.Services.Move;

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

    public Package ResolveTargetPackage(Package fallback)
    {
        return Kind switch
        {
            MoveDestination.Group => Group.Package,
            MoveDestination.ParentItem => TargetItem.Package,
            _ => fallback,
        };
    }

    public void ApplyTo(ISchemaItem item)
    {
        switch (Kind)
        {
            case MoveDestination.RootProvider:
            {
                item.Group = null;
                break;
            }
            case MoveDestination.Group:
            {
                item.Group = Group;
                break;
            }
            case MoveDestination.ParentItem:
            {
                item.Group = null;
                item.ParentNode = TargetItem;
                if (TargetItem.IsAbstract && !item.IsAbstract)
                {
                    item.IsAbstract = true;
                }
                break;
            }
        }
    }

    public void ClearAffectedCaches(ISchemaItemProvider rootProvider, ISchemaItem oldParent)
    {
        rootProvider?.ClearCache();
        oldParent?.ClearCache();
        TargetItem?.ClearCache();
    }
}
