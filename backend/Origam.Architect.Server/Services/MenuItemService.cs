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
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class MenuItemService(
    IPersistenceService persistenceService,
    TreeNodeFactory treeNodeFactory
)
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    public IEnumerable<MenuItemInfo> GetMenuItems(
        string id,
        bool isNonPersistentItem,
        string nodeText
    )
    {
        if (!Guid.TryParse(id, out Guid schemaItemId))
        {
            ISchemaItemProvider provider = treeNodeFactory.FindRootProvider(id);
            if (provider == null)
            {
                return [];
            }

            return provider.NewItemTypes.Select(type => CreateMenuItemInfo(type, name: null));
        }

        IBrowserNode2 instance = persistenceProvider.RetrieveInstance<IBrowserNode2>(schemaItemId);

        ISchemaItemFactory factory = isNonPersistentItem
            ? new NonpersistentSchemaItemNode { NodeText = nodeText, ParentNode = instance }
            : (ISchemaItemFactory)instance;

        List<string> unusedNames = GetUnusedNames(factory, instance as ISchemaItem).ToList();
        Type[] nameableTypes =
            unusedNames.Count > 0
                ? factory.NewItemTypes.Intersect(factory.NameableTypes).ToArray()
                : [];
        List<MenuItemInfo> menuItems = factory
            .NewItemTypes.Where(type => !nameableTypes.Contains(type))
            .Select(type => CreateMenuItemInfo(type, name: null))
            .ToList();
        foreach (string name in unusedNames)
        {
            menuItems.AddRange(nameableTypes.Select(type => CreateMenuItemInfo(type, name)));
        }
        return menuItems;
    }

    private static IEnumerable<string> GetUnusedNames(
        ISchemaItemFactory factory,
        ISchemaItem parentItem
    )
    {
        HashSet<string> existingNames =
            parentItem?.ChildItems.Select(child => child.Name).ToHashSet() ?? [];
        return factory.NewTypeNames.Distinct().Where(name => !existingNames.Contains(name));
    }

    private static MenuItemInfo CreateMenuItemInfo(Type type, string name)
    {
        SchemaItemDescriptionAttribute attr = type.SchemaItemDescription();
        if (attr is null)
        {
            return new MenuItemInfo(
                caption: type.Name,
                typeName: type.FullName,
                iconName: null,
                iconIndex: null,
                name: name
            );
        }

        return new MenuItemInfo(
            caption: attr.Name,
            typeName: type.FullName,
            iconName: attr.Icon is string iconName ? iconName : null,
            iconIndex: attr.Icon is int iconIndex ? iconIndex : null,
            name: name
        );
    }
}
