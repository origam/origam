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
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public class LocalizationChildEntityWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    public LocalizationChildEntityWizardData GetWizardData(Guid entityId)
    {
        var entity = RetrieveTableEntity(entityId);

        var columns = entity
            .EntityColumns.Where(column => !string.IsNullOrEmpty(column.ToString()))
            .Where(column =>
                column.DataType == OrigamDataType.String || column.DataType == OrigamDataType.Memo
            )
            .OrderBy(column => column.Name)
            .Select(column => new IdName { Id = column.Id, Name = column.Name })
            .ToList();

        return new LocalizationChildEntityWizardData
        {
            EntityName = entity.Name,
            TranslationEntityName = EntityHelper.LanguageTranslationChildEntityName(entity),
            Columns = columns,
        };
    }

    public CreateWizardResult CreateLocalizationChildEntity(
        CreateLocalizationChildEntityModel input
    )
    {
        var entity = RetrieveTableEntity(input.EntityId);
        var selectedColumns = RetrieveColumns(entity, input.SelectedFieldIds);

        var generated = new List<ISchemaItem>();
        Transaction.Run(() =>
        {
            var translationEntity = EntityHelper.CreateLanguageTranslationChildEntity(
                entity,
                selectedColumns,
                generated
            );

            if (HasCurrentDeploymentVersion())
            {
                CreateEntityDdlActivities(translationEntity, generated);
            }

            return translationEntity;
        });

        return BuildResult(generated);
    }

    private static void CreateEntityDdlActivities(
        TableMappingItem translationEntity,
        IList<ISchemaItem> generated
    )
    {
        foreach (var sqlDataService in GetDeploymentSqlDataServices())
        {
            var script = sqlDataService.EntityDdl(translationEntity.Id);
            var activity = DeploymentHelper.CreateDatabaseScript(
                translationEntity.Name,
                script,
                sqlDataService.PlatformName
            );
            generated.Add(activity);
        }
    }

    private TableMappingItem RetrieveTableEntity(Guid entityId)
    {
        return Retrieve<TableMappingItem>(entityId, Strings.Wizard_TableEntityNotFound);
    }
}
