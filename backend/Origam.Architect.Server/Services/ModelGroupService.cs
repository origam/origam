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

public class ModelGroupService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TreeNodeFactory treeNodeFactory,
    ModelTransactionRunner transactionRunner
)
{
    private static readonly HashSet<string> ReservedDeviceNames = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "CON",
        "PRN",
        "AUX",
        "NUL",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9",
    };

    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    public TreeNode Create(CreateGroupModel input)
    {
        RequireActiveExtension();
        string name = ValidateName(input.Name);

        ISchemaItemFactory factory = ResolveParent(input.NodeId);
        if (factory == null)
        {
            throw new UserOrigamException(Strings.Group_NotFound);
        }

        // Pre-check only; a concurrent create is harmless - NewGroup auto-numbers collisions.
        bool nameTaken =
            (factory as ISchemaItemProvider)?.ChildGroups.Any(existing =>
                string.Equals(existing.NodeText?.Trim(), name, StringComparison.OrdinalIgnoreCase)
            ) ?? false;
        if (nameTaken)
        {
            throw new UserOrigamException(Strings.Group_NameDuplicate);
        }

        SchemaItemGroup group = transactionRunner.Run(() =>
        {
            SchemaItemGroup created = factory.NewGroup(schemaService.ActiveSchemaExtensionId, name);
            if (created.Name != name)
            {
                // NewGroup auto-numbers when the name is a substring of a sibling group;
                // the name is already validated as unique, so keep what the user typed.
                created.NodeText = name;
            }
            return created;
        });

        return treeNodeFactory.Create(group);
    }

    public TreeNode Rename(RenameGroupModel input)
    {
        RequireActiveExtension();
        string name = ValidateName(input.Name);

        SchemaItemGroup group = ResolveGroup(input.NodeId);
        if (!schemaService.IsItemFromExtension(group))
        {
            throw new UserOrigamException(Strings.Group_RenameOutsideActivePackage);
        }
        if (!group.CanRenameTo(name))
        {
            throw new UserOrigamException(Strings.Group_NameDuplicate);
        }

        // NodeText persists immediately and physically moves the folder on disk (not undone on
        // rollback), so keep the transaction body minimal - nothing may fail after the move.
        transactionRunner.Run(() =>
        {
            group.NodeText = name;
        });

        return treeNodeFactory.Create(group);
    }

    public DeleteGroupResult Delete(DeleteGroupModel input)
    {
        RequireActiveExtension();

        SchemaItemGroup group = ResolveGroup(input.NodeId);
        if (!schemaService.CanDeleteItem(group))
        {
            throw new UserOrigamException(Strings.Group_DeleteOutsideActivePackage);
        }

        List<string> deletedSchemaItemIds = CollectSchemaItemIds(group).Distinct().ToList();

        try
        {
            transactionRunner.Run(() => group.Delete());
        }
        catch (InvalidOperationException ex)
        {
            throw new UserOrigamException(ex.Message);
        }

        return new DeleteGroupResult { DeletedSchemaItemIds = deletedSchemaItemIds };
    }

    private void RequireActiveExtension()
    {
        if (schemaService.ActiveExtension == null)
        {
            throw new UserOrigamException(Strings.Group_NoActivePackage);
        }
    }

    private ISchemaItemFactory ResolveParent(string nodeId)
    {
        if (Guid.TryParse(nodeId, out Guid schemaItemId))
        {
            return persistenceProvider.RetrieveInstance<IBrowserNode2>(schemaItemId)
                as ISchemaItemFactory;
        }
        return treeNodeFactory.FindRootProvider(nodeId) as ISchemaItemFactory;
    }

    private SchemaItemGroup ResolveGroup(Guid groupId)
    {
        if (
            persistenceProvider.RetrieveInstance<IBrowserNode2>(groupId)
            is not SchemaItemGroup group
        )
        {
            throw new UserOrigamException(Strings.Group_NotFound);
        }

        // A group fetched by id has no provider wiring; restore RootProvider so ChildItems works.
        if (group.RootProvider == null && group.ParentItem == null)
        {
            group.RootProvider = schemaService
                .Providers.OfType<AbstractSchemaItemProvider>()
                .FirstOrDefault(provider => provider.RootItemType == group.RootItemType);
        }

        return group;
    }

    private static string ValidateName(string rawName)
    {
        string name = rawName?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new UserOrigamException(Strings.Group_NameEmpty);
        }
        if (
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || name.Contains('/')
            || name.Contains('\\')
        )
        {
            throw new UserOrigamException(Strings.Group_NameInvalidChars);
        }
        if (IsReservedOrUnsafeName(name))
        {
            throw new UserOrigamException(Strings.Group_NameReserved);
        }
        return name;
    }

    private static bool IsReservedOrUnsafeName(string name)
    {
        if (name == "." || name == "..")
        {
            return true;
        }
        if (name.EndsWith("."))
        {
            return true;
        }
        string baseName = name.Split('.')[0];
        return ReservedDeviceNames.Contains(baseName);
    }

    // ChildItemsRecursive skips subgroups; Delete() cascades into them, so recurse.
    private static IEnumerable<string> CollectSchemaItemIds(SchemaItemGroup group)
    {
        foreach (ISchemaItem item in group.ChildItemsRecursive)
        {
            yield return item.Id.ToString();
        }
        foreach (SchemaItemGroup subGroup in group.ChildGroups)
        {
            foreach (string id in CollectSchemaItemIds(subGroup))
            {
                yield return id;
            }
        }
    }
}
