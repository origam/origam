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
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class SchemaItemCopier(
    SchemaService schemaService,
    ModelTransactionRunner transactionRunner,
    IDocumentationService documentationService,
    PanelControlFactory panelControlFactory,
    CrossPackageMoveValidator packageValidator
)
{
    public ISchemaItem Copy(ISchemaItem original, MoveDecision decision)
    {
        ISchemaItemProvider rootProvider = original.RootProvider;
        // The root provider has no package of its own.
        Package targetPackage = decision.ResolveTargetPackage(
            MovePreconditions.RequireActivePackage(schemaService)
        );
        if (targetPackage.Id != original.SchemaExtensionId)
        {
            packageValidator.CheckCopyOrThrow(original, targetPackage);
        }
        // Clone() puts a top level clone straight into RootProvider.ChildItems.
        var clone = (ISchemaItem)original.Clone();
        bool wasTopLevel = clone.ParentItem == null;
        try
        {
            return transactionRunner.Run(() =>
            {
                decision.ApplyTo(clone);
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
            decision.ClearAffectedCaches(rootProvider, original.ParentItem);
            throw;
        }
    }

    // PanelControl finds the wrapper by the set's own id, so a clone has none.
    private void CreatePanelControl(ISchemaItem clone, Package targetPackage)
    {
        if (clone is PanelControlSet { PanelControl: null } panelControlSet)
        {
            panelControlFactory.Create(panelControlSet, targetPackage.Id);
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

        // Groups split the provider visually only.
        return item.RootProvider.ChildItems;
    }
}
