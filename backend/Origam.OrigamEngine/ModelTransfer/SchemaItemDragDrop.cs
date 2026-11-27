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
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelTransfer;

public class SchemaItemDragDrop(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    IDocumentationService documentationService
)
{
    public bool CanDrop(ISchemaItem item, IBrowserNode2 dropTarget, bool isCopy)
    {
        if (!isCopy && !schemaService.CanEditItem(item))
        {
            return false;
        }
        if (dropTarget == item.RootProvider)
        {
            return item.ParentItem == null;
        }
        if (dropTarget is SchemaItemGroup group)
        {
            return group.ParentItem == item.ParentItem && group.RootProvider == item.RootProvider;
        }
        return dropTarget is ISchemaItem dropElement
            && item.CanMove(dropElement)
            && (isCopy || !IsItemOrItsDescendant(dropElement, item));
    }

    public ISchemaItem Drop(ISchemaItem originalItem, IBrowserNode2 dropTarget, bool isCopy)
    {
        if (!CanDrop(originalItem, dropTarget, isCopy))
        {
            return null;
        }
        ISchemaItem item = null;
        try
        {
            ModelTransaction.Run(
                persistenceService.SchemaProvider,
                () =>
                {
                    item = isCopy ? (ISchemaItem)originalItem.Clone() : originalItem;
                    Place(item, dropTarget, isCopy);
                }
            );
        }
        catch
        {
            originalItem.RootProvider?.ClearCache();
            if (isCopy && item != null)
            {
                DiscardCopy(item);
            }
            throw;
        }
        return item;
    }

    private void Place(ISchemaItem item, IBrowserNode2 dropTarget, bool isCopy)
    {
        bool renameCopy = true;
        if (dropTarget == item.RootProvider)
        {
            item.Group = null;
        }
        else if (dropTarget is SchemaItemGroup group)
        {
            item.Group = group;
        }
        else
        {
            var dropElement = (ISchemaItem)dropTarget;
            if (item.ParentItem == null)
            {
                item.RootProvider?.ClearCache();
            }
            item.Group = null;
            item.ParentNode = dropElement;
            if (dropElement.IsAbstract && !item.IsAbstract)
            {
                item.IsAbstract = true;
            }
            renameCopy = GetTakenNames(item).Contains(item.Name);
        }
        if (isCopy)
        {
            item.SetExtensionRecursive(schemaService.ActiveExtension);
            if (renameCopy)
            {
                item.Name = GetItemText(item);
            }
            SaveCopy(item);
        }
        else
        {
            item.Persist();
        }
    }

    private static void DiscardCopy(ISchemaItem copy)
    {
        ISchemaItemProvider provider = (ISchemaItemProvider)copy.ParentItem ?? copy.RootProvider;
        if (provider != null && provider.ChildItems.Contains(copy))
        {
            provider.ChildItems.Remove(copy);
        }
    }

    private void SaveCopy(ISchemaItem item)
    {
        Dictionary<Guid, ModelElementKey> oldKeys = item
            .ChildItemsRecursive.Where(child => child.OldPrimaryKey != null)
            .ToDictionary(child => child.Id, child => child.OldPrimaryKey);
        item.ThrowEventOnPersist = false;
        item.Persist();
        RestoreOldKeys(item, oldKeys);
        item.UpdateReferences();
        item.ThrowEventOnPersist = true;
        item.Persist();
        List<ISchemaItem> items = RestoreOldKeys(item, oldKeys);
        items.Add(item);
        documentationService.CloneDocumentation(items);
        item.OldPrimaryKey = null;
    }

    private static List<ISchemaItem> RestoreOldKeys(
        ISchemaItem item,
        Dictionary<Guid, ModelElementKey> oldKeys
    )
    {
        List<ISchemaItem> children = item.ChildItemsRecursive;
        foreach (ISchemaItem child in children)
        {
            if (oldKeys.TryGetValue(child.Id, out ModelElementKey oldKey))
            {
                child.OldPrimaryKey = oldKey;
            }
        }
        return children;
    }

    private static string GetItemText(ISchemaItem item)
    {
        HashSet<string> takenNames = GetTakenNames(item);
        string text = item.Name;
        do
        {
            text = string.Format(Strings.CopyOf, text);
        } while (takenNames.Contains(text));
        return text;
    }

    private static HashSet<string> GetTakenNames(ISchemaItem item)
    {
        IEnumerable<ISchemaItem> siblings =
            item.ParentItem?.ChildItems
            ?? item.RootProvider?.ChildItems
            ?? Enumerable.Empty<ISchemaItem>();
        return siblings
            .Where(sibling => sibling.Id != item.Id && sibling.Name != null)
            .Select(sibling => sibling.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsItemOrItsDescendant(ISchemaItem node, ISchemaItem item)
    {
        for (ISchemaItem current = node; current != null; current = current.ParentItem)
        {
            if (current.Id == item.Id)
            {
                return true;
            }
        }
        return false;
    }
}
