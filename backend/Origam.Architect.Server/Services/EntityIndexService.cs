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

        var allItems = schemaService
            .Providers.SelectMany(provider => provider.ChildItemsRecursive)
            .Where(item => item.IsPersisted)
            .ToList();

        var entities = allItems.OfType<IDataEntity>().ToList();

        var structuresByEntityId = new Dictionary<Guid, List<DataStructure>>();
        foreach (DataStructure structure in allItems.OfType<DataStructure>())
        {
            foreach (DataStructureEntity dse in structure.Entities)
            {
                IDataEntity linked = dse.EntityDefinition;
                if (linked == null)
                {
                    continue;
                }
                Guid entityId = (Guid)linked.PrimaryKey["Id"];
                if (!structuresByEntityId.TryGetValue(entityId, out var list))
                {
                    list = new List<DataStructure>();
                    structuresByEntityId[entityId] = list;
                }
                if (!list.Any(existing => existing.Id == structure.Id))
                {
                    list.Add(structure);
                }
            }
        }

        var screensByStructureId = allItems
            .OfType<FormControlSet>()
            .Where(screen => screen.DataSourceId != Guid.Empty)
            .GroupBy(screen => screen.DataSourceId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var panelsByEntityId = allItems
            .OfType<PanelControlSet>()
            .Where(panel => panel.DataSourceId != Guid.Empty)
            .GroupBy(panel => panel.DataSourceId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var lookupsByStructureId = new Dictionary<Guid, List<AbstractDataLookup>>();
        foreach (AbstractDataLookup lookup in allItems.OfType<AbstractDataLookup>())
        {
            if (lookup.ListDataStructureId == Guid.Empty)
            {
                continue;
            }
            if (!lookupsByStructureId.TryGetValue(lookup.ListDataStructureId, out var list))
            {
                list = new List<AbstractDataLookup>();
                lookupsByStructureId[lookup.ListDataStructureId] = list;
            }
            list.Add(lookup);
        }

        var workQueuesByEntityId = allItems
            .OfType<WorkQueueClass>()
            .Where(workQueue => workQueue.EntityId != Guid.Empty)
            .GroupBy(workQueue => workQueue.EntityId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var cards = new List<EntityCard>(entities.Count);
        foreach (IDataEntity entity in entities.OrderBy(entity => entity.Name))
        {
            var entityItem = (ISchemaItem)entity;
            Guid entityId = (Guid)entityItem.PrimaryKey["Id"];

            structuresByEntityId.TryGetValue(entityId, out var structures);
            structures ??= new List<DataStructure>();

            var screens = structures
                .SelectMany(structure =>
                    screensByStructureId.TryGetValue(structure.Id, out var list)
                        ? list
                        : Enumerable.Empty<FormControlSet>()
                )
                .Distinct()
                .ToList();

            var lookups = structures
                .SelectMany(structure =>
                    lookupsByStructureId.TryGetValue(structure.Id, out var list)
                        ? list
                        : Enumerable.Empty<AbstractDataLookup>()
                )
                .Distinct()
                .ToList();

            panelsByEntityId.TryGetValue(entityId, out var panels);
            workQueuesByEntityId.TryGetValue(entityId, out var workQueues);

            var fields = entity
                .EntityColumns.Where(column => column.Name != null)
                .OrderBy(column => column.Name)
                .Select(column => column.Name)
                .ToList();

            var primaryKey = entity
                .EntityColumns.Where(column => column.IsPrimaryKey && column.Name != null)
                .OrderBy(column => column.Name)
                .Select(column => new RelatedItem(column.Id.ToString("D"), column.Name))
                .ToList();

            cards.Add(
                new EntityCard(
                    Id: entityId.ToString("D"),
                    Name: entity.Name,
                    Kind: entity.GetType().SchemaItemDescription()?.Name ?? entity.GetType().Name,
                    Package: entityItem.Group?.Name,
                    Fields: fields,
                    PrimaryKey: primaryKey,
                    Structures: structures
                        .Select(structure => new RelatedItem(
                            structure.Id.ToString("D"),
                            structure.Name
                        ))
                        .ToList(),
                    Screens: screens
                        .Select(screen => new RelatedItem(screen.Id.ToString("D"), screen.Name))
                        .ToList(),
                    Panels: (panels ?? new List<PanelControlSet>())
                        .Select(panel => new RelatedItem(panel.Id.ToString("D"), panel.Name))
                        .ToList(),
                    Lookups: lookups
                        .Select(lookup => new RelatedItem(lookup.Id.ToString("D"), lookup.Name))
                        .ToList(),
                    WorkQueues: (workQueues ?? new List<WorkQueueClass>())
                        .Select(workQueue => new RelatedItem(
                            workQueue.Id.ToString("D"),
                            workQueue.Name
                        ))
                        .ToList()
                )
            );
        }

        return cards;
    }
}
