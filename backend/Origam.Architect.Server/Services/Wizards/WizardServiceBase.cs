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

using System.Collections;
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Services.Wizards;

public abstract class WizardServiceBase(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
)
{
    protected const string AllRoles = "*";

    protected IPersistenceProvider PersistenceProvider { get; } = persistenceService.SchemaProvider;

    protected ModelTransactionRunner Transaction { get; } = transaction;

    protected CreateWizardResult BuildResult(IEnumerable<ISchemaItem> generatedItems)
    {
        return new CreateWizardResult
        {
            SearchResults = searchService.BuildResults(generatedItems),
        };
    }

    protected T Retrieve<T>(Guid id, string notFoundMessage)
        where T : class
    {
        return PersistenceProvider.RetrieveInstance<T>(id)
            ?? throw new UserOrigamException(string.Format(notFoundMessage, id));
    }

    protected IDataEntity RetrieveEntity(Guid entityId)
    {
        var instance = Retrieve<IPersistent>(entityId, Strings.Wizard_EntityNotFound);

        if (instance is IDataEntity entity)
        {
            return entity;
        }

        if (
            instance is DataStructureEntity dataStructureEntity
            && dataStructureEntity.Entity is IDataEntity or IAssociation
        )
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.Wizard_DataStructureEntityUsedAsEntity,
                    entityId,
                    dataStructureEntity.EntityDefinition.Id
                )
            );
        }

        throw new UserOrigamException(
            string.Format(Strings.Wizard_NotAnEntity, entityId, instance.GetType().Name)
        );
    }

    protected static string RequireName(string name, string nameRequiredMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new UserOrigamException(nameRequiredMessage);
        }
        return name.Trim();
    }

    protected static void RequireSelectedFields(ICollection<Guid> selectedFieldIds)
    {
        if (selectedFieldIds == null || selectedFieldIds.Count == 0)
        {
            throw new UserOrigamException(Strings.Wizard_AtLeastOneFieldRequired);
        }
    }

    protected static IDataEntityColumn RetrieveColumn(IDataEntity entity, Guid fieldId)
    {
        return entity.EntityColumns.FirstOrDefault(column => column.Id == fieldId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_FieldNotFoundOnEntity, fieldId)
            );
    }

    protected static ArrayList RetrieveColumns(IDataEntity entity, IEnumerable<Guid> fieldIds)
    {
        return new ArrayList(fieldIds.Select(fieldId => RetrieveColumn(entity, fieldId)).ToList());
    }

    protected static List<string> GetDataStructureNames()
    {
        return GetItemNames<DataStructureSchemaItemProvider>(AbstractDataStructure.CategoryConst);
    }

    protected static List<string> GetScreenSectionNames()
    {
        return GetItemNames<PanelSchemaItemProvider>(PanelControlSet.CategoryConst);
    }

    protected static void RequireUniqueDataStructureName(string name)
    {
        if (ContainsName(GetDataStructureNames(), name))
        {
            throw new UserOrigamException(
                string.Format(Strings.Wizard_DataStructureAlreadyExists, name)
            );
        }
    }

    protected static void RequireUniqueScreenSectionName(string name)
    {
        if (ContainsName(GetScreenSectionNames(), name))
        {
            throw new UserOrigamException(
                string.Format(Strings.Wizard_ScreenSectionAlreadyExists, name)
            );
        }
    }

    protected static bool HasCurrentDeploymentVersion()
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var deploymentProvider = schemaService?.GetProvider<DeploymentSchemaItemProvider>();
        return deploymentProvider?.CurrentVersion() != null;
    }

    protected static void CreateSystemRoleActivities(string role, IList<ISchemaItem> generated)
    {
        foreach (var sqlDataService in GetDeploymentSqlDataServices())
        {
            generated.Add(DeploymentHelper.CreateSystemRole(role, sqlDataService));
        }
    }

    protected static IEnumerable<AbstractSqlDataService> GetDeploymentSqlDataServices()
    {
        var settings = ConfigurationManager.GetActiveConfiguration();
        var platformDataServices = (settings.DeployPlatforms ?? [])
            .Select(platform => DataServiceFactory.GetDataService(platform))
            .OfType<AbstractSqlDataService>();
        foreach (var platformDataService in platformDataServices)
        {
            yield return platformDataService;
        }
        if (DataServiceFactory.GetDataService() is AbstractSqlDataService activeDataService)
        {
            yield return activeDataService;
        }
    }

    private static bool ContainsName(IEnumerable<string> existingNames, string name)
    {
        return existingNames.Any(existingName =>
            string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static List<string> GetItemNames<TProvider>(string category)
        where TProvider : class, ISchemaItemProvider
    {
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var provider = schemaService?.GetProvider<TProvider>();
        return provider?.ChildItemsByType<ISchemaItem>(category).Select(item => item.Name).ToList()
            ?? [];
    }
}
