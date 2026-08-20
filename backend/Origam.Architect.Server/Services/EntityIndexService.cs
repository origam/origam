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
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class EntityIndexService(SchemaService schemaService)
{
    public List<EntityCard> Get()
    {
        if (schemaService.ActiveExtension == null)
        {
            return new List<EntityCard>();
        }

        List<ISchemaItem> allItems = GetPersistedItems();
        EntityUsages usages = IndexUsages(allItems);

        return allItems
            .OfType<IDataEntity>()
            .OrderBy(entity => entity.Name)
            .Select(entity => CreateCard(entity, usages))
            .ToList();
    }

    private List<ISchemaItem> GetPersistedItems()
    {
        return schemaService
            .Providers.SelectMany(provider => provider.ChildItemsRecursive)
            .Where(item => item.IsPersisted)
            .ToList();
    }

    private static EntityUsages IndexUsages(List<ISchemaItem> allItems)
    {
        return new EntityUsages(
            StructuresByEntityId: IndexStructuresByEntityId(allItems),
            ScreensByStructureId: GroupByDataSourceId(allItems.OfType<FormControlSet>()),
            PanelsByEntityId: GroupByDataSourceId(allItems.OfType<PanelControlSet>()),
            LookupsByStructureId: allItems
                .OfType<AbstractDataLookup>()
                .Where(lookup => lookup.ListDataStructureId != Guid.Empty)
                .GroupBy(lookup => lookup.ListDataStructureId)
                .ToDictionary(group => group.Key, group => group.ToList()),
            WorkQueuesByEntityId: allItems
                .OfType<WorkQueueClass>()
                .Where(workQueue => workQueue.EntityId != Guid.Empty)
                .GroupBy(workQueue => workQueue.EntityId)
                .ToDictionary(group => group.Key, group => group.ToList())
        );
    }

    private static Dictionary<Guid, List<DataStructure>> IndexStructuresByEntityId(
        List<ISchemaItem> allItems
    )
    {
        var structuresByEntityId = new Dictionary<Guid, List<DataStructure>>();
        foreach (DataStructure structure in allItems.OfType<DataStructure>())
        {
            foreach (DataStructureEntity structureEntity in structure.Entities)
            {
                IDataEntity entity = structureEntity.EntityDefinition;
                if (entity == null)
                {
                    continue;
                }

                if (
                    !structuresByEntityId.TryGetValue(entity.Id, out List<DataStructure> structures)
                )
                {
                    structures = new List<DataStructure>();
                    structuresByEntityId[entity.Id] = structures;
                }

                if (!structures.Any(existing => existing.Id == structure.Id))
                {
                    structures.Add(structure);
                }
            }
        }

        return structuresByEntityId;
    }

    private static Dictionary<Guid, List<TControlSet>> GroupByDataSourceId<TControlSet>(
        IEnumerable<TControlSet> controlSets
    )
        where TControlSet : AbstractControlSet
    {
        return controlSets
            .Where(controlSet => controlSet.DataSourceId != Guid.Empty)
            .GroupBy(controlSet => controlSet.DataSourceId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    private static EntityCard CreateCard(IDataEntity entity, EntityUsages usages)
    {
        List<DataStructure> structures = GetOrEmpty(usages.StructuresByEntityId, entity.Id);
        List<FormControlSet> screens = structures
            .SelectMany(structure => GetOrEmpty(usages.ScreensByStructureId, structure.Id))
            .Distinct()
            .ToList();
        List<AbstractDataLookup> lookups = structures
            .SelectMany(structure => GetOrEmpty(usages.LookupsByStructureId, structure.Id))
            .Distinct()
            .ToList();

        return new EntityCard(
            Id: entity.Id.ToString("D"),
            Name: entity.Name,
            Kind: entity.GetType().SchemaItemDescription()?.Name ?? entity.GetType().Name,
            Package: entity.Group?.Name,
            Fields: entity
                .EntityColumns.Where(column => column.Name != null)
                .OrderBy(column => column.Name)
                .Select(column => column.Name)
                .ToList(),
            PrimaryKey: ToRelatedItems(
                entity
                    .EntityColumns.Where(column => column.IsPrimaryKey && column.Name != null)
                    .OrderBy(column => column.Name)
            ),
            Structures: ToRelatedItems(structures),
            Screens: ToRelatedItems(screens),
            Panels: ToRelatedItems(GetOrEmpty(usages.PanelsByEntityId, entity.Id)),
            Lookups: ToRelatedItems(lookups),
            WorkQueues: ToRelatedItems(GetOrEmpty(usages.WorkQueuesByEntityId, entity.Id))
        );
    }

    private static List<TItem> GetOrEmpty<TItem>(Dictionary<Guid, List<TItem>> itemsByKey, Guid key)
    {
        return itemsByKey.TryGetValue(key, out List<TItem> items) ? items : new List<TItem>();
    }

    private static List<RelatedItem> ToRelatedItems<TItem>(IEnumerable<TItem> items)
        where TItem : ISchemaItem
    {
        return items.Select(item => new RelatedItem(item.Id.ToString("D"), item.Name)).ToList();
    }

    private record EntityUsages(
        Dictionary<Guid, List<DataStructure>> StructuresByEntityId,
        Dictionary<Guid, List<FormControlSet>> ScreensByStructureId,
        Dictionary<Guid, List<PanelControlSet>> PanelsByEntityId,
        Dictionary<Guid, List<AbstractDataLookup>> LookupsByStructureId,
        Dictionary<Guid, List<WorkQueueClass>> WorkQueuesByEntityId
    );
}
