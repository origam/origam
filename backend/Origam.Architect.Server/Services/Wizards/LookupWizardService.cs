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

using Origam.Architect.Server.Models.Requests.Wizards;
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public class LookupWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    private const string DefaultDisplayColumnName = "Name";

    public LookupWizardData GetWizardData(Guid entityId)
    {
        var entity = RetrieveEntity(entityId);

        var primaryKey =
            FindPrimaryKey(entity)
            ?? throw new UserOrigamException(Strings.Wizard_EntityHasNoPrimaryKeyForLookup);

        var columns = entity
            .EntityColumns.Where(column => !string.IsNullOrEmpty(column.ToString()))
            .OrderBy(column => column.Name)
            .Select(column => new IdName { Id = column.Id, Name = column.Name })
            .ToList();

        var filters = entity
            .EntityFilters.OrderBy(filter => filter.Name)
            .Select(filter => new IdName { Id = filter.Id, Name = filter.Name })
            .ToList();

        var defaultDisplay = entity.EntityColumns.FirstOrDefault(column =>
            column.Name == DefaultDisplayColumnName
        );

        return new LookupWizardData
        {
            EntityName = entity.Name,
            PrimaryKeyId = primaryKey.Id,
            PrimaryKeyName = primaryKey.Name,
            DefaultDisplayFieldId = defaultDisplay?.Id ?? primaryKey.Id,
            Columns = columns,
            Filters = filters,
        };
    }

    public CreateWizardResult CreateLookup(CreateLookupModel input)
    {
        RequireName(input.Name, Strings.Wizard_LookupNameRequired);
        var entity = RetrieveEntity(input.EntityId);

        var idColumn =
            FindPrimaryKey(entity)
            ?? throw new UserOrigamException(Strings.Wizard_EntityHasNoPrimaryKey);

        var displayColumn =
            entity.EntityColumns.FirstOrDefault(column => column.Id == input.DisplayFieldId)
            ?? throw new UserOrigamException(Strings.Wizard_DisplayFieldNotFound);

        var idFilter =
            entity.EntityFilters.FirstOrDefault(filter => filter.Id == input.IdFilterId)
            ?? throw new UserOrigamException(Strings.Wizard_IdFilterNotFound);

        EntityFilter listFilter = null;
        if (input.ListFilterId.HasValue && input.ListFilterId.Value != Guid.Empty)
        {
            listFilter =
                entity.EntityFilters.FirstOrDefault(filter => filter.Id == input.ListFilterId.Value)
                ?? throw new UserOrigamException(Strings.Wizard_ListFilterNotFound);
        }

        return Transaction.Run(() =>
        {
            var lookup = LookupHelper.CreateDataServiceLookup(
                input.Name,
                entity,
                idColumn,
                displayColumn,
                codeField: null,
                idFilter,
                listFilter,
                listDisplayMember: null
            );
            return BuildResult([lookup, lookup.ListDataStructure]);
        });
    }

    private static IDataEntityColumn FindPrimaryKey(IDataEntity entity)
    {
        return entity.EntityColumns.FirstOrDefault(column =>
            column.IsPrimaryKey && !column.ExcludeFromAllFields
        );
    }
}
