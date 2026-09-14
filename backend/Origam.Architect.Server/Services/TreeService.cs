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
using Origam.Schema;

namespace Origam.Architect.Server.Services;

public class TreeService(TabService tabService, PropertyEditorService propertyEditorService)
{
    public NewItemResult CreateNode(NewItemModel input)
    {
        TabData tab = tabService.OpenTabWithNewItem(input.NodeId, input.NewTypeName);

        try
        {
            if (input.Changes is { Count: > 0 })
            {
                tabService.ChangesToTabData(
                    new ChangesModel { SchemaItemId = tab.Item.Id, Changes = input.Changes }
                );
            }

            if (input.Persist)
            {
                if (HasRuleErrors(tab.Item))
                {
                    return new NewItemResult(tab, Discarded: true, CloseWhenDone: true);
                }

                if (tabService.CanPersist(tab.Item))
                {
                    tabService.PersistItem(tab);
                }
            }
        }
        catch
        {
            if (!tab.Item.IsPersisted)
            {
                tabService.CloseTab(tab.Id);
            }

            throw;
        }

        return new NewItemResult(
            tab,
            Discarded: false,
            CloseWhenDone: input.Persist && tab.Item.IsPersisted
        );
    }

    private bool HasRuleErrors(ISchemaItem item)
    {
        return propertyEditorService
            .GetEditorPropertiesWithErrors(item)
            .Any(property => property.Errors is { Count: > 0 });
    }
}
