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
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class EntityCardService(IPersistenceService persistenceService, SchemaService schemaService)
{
    public List<EntityCard> Get()
    {
        if (schemaService.ActiveExtension == null)
        {
            return [];
        }

        HashSet<Guid> loadedPackages = schemaService
            .ActiveExtension.IncludedPackages.Select(package => package.Id)
            .Append(schemaService.ActiveExtension.Id)
            .ToHashSet();

        return persistenceService
            .SchemaProvider.RetrieveList<IDataEntity>()
            .OrderBy(entity => entity.Name)
            .Select(entity => CreateCard(entity, loadedPackages))
            .ToList();
    }

    private static EntityCard CreateCard(IDataEntity entity, HashSet<Guid> loadedPackages)
    {
        List<ISchemaItem> directUsers = GetRootUsers(entity, loadedPackages);
        IEnumerable<ISchemaItem> indirectUsers = directUsers.SelectMany(user =>
            GetRootUsers(user, loadedPackages)
        );

        return new EntityCard(
            Id: entity.Id.ToString("D"),
            Name: entity.Name,
            Kind: KindOf(entity),
            Package: entity.Group?.Name,
            Fields: entity
                .EntityColumns.Where(column => column.Name != null)
                .OrderBy(column => column.Name)
                .Select(column => column.Name)
                .ToList(),
            PrimaryKey: ToRelatedItems(
                entity.EntityPrimaryKey.Where(column => column.Name != null)
            ),
            UsedBy: ToRelatedItems(directUsers.Concat(indirectUsers).DistinctBy(user => user.Id))
        );
    }

    private static List<ISchemaItem> GetRootUsers(ISchemaItem item, HashSet<Guid> loadedPackages)
    {
        return item.GetUsage()
            .Where(user => user != null && loadedPackages.Contains(user.SchemaExtensionId))
            .Select(user => user.RootItem)
            .Where(root => root.Id != item.Id && root is not IDataEntity)
            .DistinctBy(root => root.Id)
            .ToList();
    }

    private static List<RelatedItem> ToRelatedItems(IEnumerable<ISchemaItem> items)
    {
        return items
            .Select(item => new RelatedItem(item.Id.ToString("D"), item.Name, KindOf(item)))
            .OrderBy(item => item.Kind)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private static string KindOf(ISchemaItem item)
    {
        return item.ModelDescription() ?? item.GetType().Name;
    }
}
