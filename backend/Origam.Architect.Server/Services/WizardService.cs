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
using System.Text;
using Origam.Architect.Server.Models.Requests.Wizards;
using Origam.Architect.Server.Models.Responses.Wizards;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Services;

public class WizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
)
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    private const string AllRoles = "*";
    private const string DbParamPrefix = "par";
    private const string RefColumnPrefix = "ref";
    private const string TempTablePrefix = "tmptable";
    private const string DefaultDisplayColumnName = "Name";
    private const string PanelTitlePropertyName = "PanelTitle";
    private const string TopPropertyName = "Top";
    private const int ScreenSectionFieldTopStart = 36;
    private const int ScreenSectionFieldTopStep = 30;
    private const string BetweenFunctionName = "Between";
    private const string BetweenExpressionChildName = "Expression";
    private const string BetweenLeftChildName = "Left";
    private const string BetweenRightChildName = "Right";

    private record FilterDefinition(string FunctionName, string Prefix, bool CreateParameter);

    private static readonly Dictionary<CreateFilterType, FilterDefinition> FilterDefinitions = new()
    {
        [CreateFilterType.Equal] = new("Equal", "GetBy", CreateParameter: false),
        [CreateFilterType.EqualParam] = new("Equal", "GetBy", CreateParameter: true),
        [CreateFilterType.Like] = new("Like", "GetLike", CreateParameter: false),
        [CreateFilterType.LikeParam] = new("Like", "GetLike", CreateParameter: true),
        [CreateFilterType.InList] = new("In", "GetBy", CreateParameter: true),
    };

    public CreateWizardResult CreateFilter(CreateFilterModel input)
    {
        var column =
            persistenceProvider.RetrieveInstance<IDataEntityColumn>(input.ColumnId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_ColumnNotFound, input.ColumnId)
            );

        if (column.ParentItem is not IDataEntity)
        {
            throw new UserOrigamException(Strings.Wizard_ColumnNotInEntity);
        }

        var generated = new List<ISchemaItem>();
        _ = transaction.Run(() =>
        {
            if (input.FilterType == CreateFilterType.Between)
            {
                return CreateBetweenFilter(column, generated);
            }
            if (!FilterDefinitions.TryGetValue(input.FilterType, out var definition))
            {
                throw new UserOrigamException(
                    string.Format(Strings.Wizard_UnknownFilterType, input.FilterType)
                );
            }
            return EntityHelper.CreateFilter(
                field: column,
                functionName: definition.FunctionName,
                filterPrefix: definition.Prefix,
                createParameter: definition.CreateParameter,
                generatedElements: generated
            );
        });

        return new CreateWizardResult { SearchResults = searchService.BuildResults(generated) };
    }

    private static EntityFilter CreateBetweenFilter(
        IDataEntityColumn field,
        IList<ISchemaItem> generated
    )
    {
        if (string.IsNullOrEmpty(field.Name))
        {
            throw new ArgumentException(Strings.Wizard_FieldNameNotSet);
        }
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var entity = (IDataEntity)field.ParentItem;
        var baseName = field.Name.StartsWith(RefColumnPrefix)
            ? field.Name.Substring(RefColumnPrefix.Length)
            : field.Name;

        var paramFrom = entity.NewItem<DatabaseParameter>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        paramFrom.DataType = field.DataType;
        paramFrom.DataLength = field.DataLength;
        paramFrom.Name = $"{DbParamPrefix}{baseName}From";
        paramFrom.Persist();
        generated.Add(paramFrom);

        var paramTo = entity.NewItem<DatabaseParameter>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        paramTo.DataType = field.DataType;
        paramTo.DataLength = field.DataLength;
        paramTo.Name = $"{DbParamPrefix}{baseName}To";
        paramTo.Persist();
        generated.Add(paramTo);

        var filter = entity.NewItem<EntityFilter>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        filter.Name = $"GetBetween{baseName}";
        filter.Persist();
        generated.Add(filter);

        var call = filter.NewItem<FunctionCall>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        var functionProvider = schemaService.GetProvider<FunctionSchemaItemProvider>();
        var betweenFunction = (Function)
            functionProvider.GetChildByName(
                name: BetweenFunctionName,
                itemType: Function.CategoryConst
            );
        if (betweenFunction == null)
        {
            throw new Exception(Strings.Wizard_BetweenFunctionNotFound);
        }
        call.Function = betweenFunction;
        call.Name = BetweenFunctionName;
        call.Persist();

        var expressionRef = call.GetChildByName(name: BetweenExpressionChildName)
            .NewItem<EntityColumnReference>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
        expressionRef.Field = field;
        expressionRef.Persist();

        var leftRef = call.GetChildByName(name: BetweenLeftChildName)
            .NewItem<ParameterReference>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
        leftRef.Parameter = paramFrom;
        leftRef.Persist();

        var rightRef = call.GetChildByName(name: BetweenRightChildName)
            .NewItem<ParameterReference>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
        rightRef.Parameter = paramTo;
        rightRef.Persist();

        return filter;
    }

    public ScreenWizardData GetScreenWizardData(Guid entityId)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(entityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, entityId)
            );

        var columns = entity
            .EntityColumns.Where(column => !string.IsNullOrEmpty(column.ToString()))
            .OrderBy(column => column.Name)
            .Select(column => new ScreenWizardColumn
            {
                Id = column.Id,
                Name = column.Name,
                IsPrimaryKey = column.IsPrimaryKey,
            })
            .ToList();

        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var existingNames =
            dsProvider
                ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
                .Select(item => item.Name)
                .ToList()
            ?? new List<string>();

        return new ScreenWizardData
        {
            EntityName = entity.Name,
            Columns = columns,
            ExistingDataStructureNames = existingNames,
        };
    }

    public CreateWizardResult CreateScreen(CreateScreenModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException(Strings.Wizard_ScreenNameRequired);
        }

        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, input.EntityId)
            );

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            throw new UserOrigamException(Strings.Wizard_AtLeastOneFieldRequired);
        }

        var trimmedName = input.Name.Trim();
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var duplicate = dsProvider
            ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
            .FirstOrDefault(item =>
                string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
            );
        if (duplicate != null)
        {
            throw new UserOrigamException(
                string.Format(Strings.Wizard_DataStructureAlreadyExists, trimmedName)
            );
        }

        var selectedNames = new Hashtable();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column =
                entity.EntityColumns.FirstOrDefault(column => column.Id == fieldId)
                ?? throw new UserOrigamException(
                    string.Format(Strings.Wizard_FieldNotFoundOnEntity, fieldId)
                );
            selectedNames[column.Name] = true;
        }

        var groupName = entity.Group?.Name;

        var (dataStructure, panel, form) = transaction.Run(() =>
        {
            var newDataStructure = EntityHelper.CreateDataStructure(
                entity,
                input.Name,
                persist: true
            );
            var newPanel = GuiHelper.CreatePanel(groupName, entity, selectedNames, input.Name);
            RelayoutScreenSectionFields(newPanel);
            var newForm = GuiHelper.CreateForm(newDataStructure, groupName, newPanel);
            return (newDataStructure, newPanel, newForm);
        });

        if (!string.IsNullOrWhiteSpace(input.Caption) && panel.ChildItems.Count > 0)
        {
            transaction.Run(() =>
            {
                var rootControl = panel.ChildItems[0];
                var titleProp = rootControl
                    .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
                    .FirstOrDefault(prop =>
                        prop.ControlPropertyItem?.Name == PanelTitlePropertyName
                    );
                if (titleProp != null)
                {
                    titleProp.Value = input.Caption.Trim();
                    titleProp.Persist();
                }
            });
        }

        return new CreateWizardResult
        {
            SearchResults = searchService.BuildResults([dataStructure, panel, form]),
        };
    }

    public DataStructureWizardData GetDataStructureWizardData(Guid entityId)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(entityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, entityId)
            );

        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var existingNames =
            dsProvider
                ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
                .Select(item => item.Name)
                .ToList() ?? new List<string>();

        return new DataStructureWizardData
        {
            EntityName = entity.Name,
            ExistingDataStructureNames = existingNames,
        };
    }

    public CreateWizardResult CreateDataStructure(CreateDataStructureModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException(Strings.Wizard_DataStructureNameRequired);
        }

        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, input.EntityId)
            );

        var trimmedName = input.Name.Trim();
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var duplicate = dsProvider
            ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
            .FirstOrDefault(item =>
                string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
            );
        if (duplicate != null)
        {
            throw new UserOrigamException(
                string.Format(Strings.Wizard_DataStructureAlreadyExists, trimmedName)
            );
        }

        var dataStructure = transaction.Run(() =>
            EntityHelper.CreateDataStructure(entity, trimmedName, persist: true)
        );

        return new CreateWizardResult
        {
            SearchResults = searchService.BuildResults([dataStructure]),
        };
    }

    public ScreenFromSectionWizardData GetScreenFromSectionWizardData(Guid screenSectionId)
    {
        var panel =
            persistenceProvider.RetrieveInstance<PanelControlSet>(screenSectionId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_ScreenSectionNotFound, screenSectionId)
            );

        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var existingNames =
            dsProvider
                ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
                .Select(item => item.Name)
                .ToList() ?? new List<string>();

        return new ScreenFromSectionWizardData
        {
            SectionName = panel.Name,
            ExistingDataStructureNames = existingNames,
        };
    }

    public CreateWizardResult CreateScreenFromSection(CreateScreenFromSectionModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException(Strings.Wizard_ScreenNameRequired);
        }

        var panel =
            persistenceProvider.RetrieveInstance<PanelControlSet>(input.ScreenSectionId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_ScreenSectionNotFound, input.ScreenSectionId)
            );

        var trimmedName = input.Name.Trim();
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var duplicate = dsProvider
            ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
            .FirstOrDefault(item =>
                string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
            );
        if (duplicate != null)
        {
            throw new UserOrigamException(
                string.Format(Strings.Wizard_DataStructureAlreadyExists, trimmedName)
            );
        }

        var groupName = panel.Group?.Name;

        var (dataStructure, form) = transaction.Run(() =>
        {
            var newDataStructure = EntityHelper.CreateDataStructure(
                panel.DataEntity,
                trimmedName,
                persist: true
            );
            var newForm = GuiHelper.CreateForm(newDataStructure, groupName, panel);
            return (newDataStructure, newForm);
        });

        return new CreateWizardResult
        {
            SearchResults = searchService.BuildResults([dataStructure, form]),
        };
    }

    public CreateWizardResult CreateWorkQueueClass(CreateWorkQueueModel input)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, input.EntityId)
            );

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            throw new UserOrigamException(Strings.Wizard_AtLeastOneFieldRequired);
        }

        var selectedColumns = new ArrayList();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column =
                entity.EntityColumns.FirstOrDefault(column => column.Id == fieldId)
                ?? throw new UserOrigamException(
                    string.Format(Strings.Wizard_FieldNotFoundOnEntity, fieldId)
                );
            selectedColumns.Add(column);
        }

        var generated = new List<ISchemaItem>();
        transaction.Run(() =>
            WorkflowHelper.CreateWorkQueueClass(entity, selectedColumns, generated)
        );

        return new CreateWizardResult { SearchResults = searchService.BuildResults(generated) };
    }

    public GetDataStructureSqlResult GetDataStructureSql(Guid dataStructureId)
    {
        var dataStructure =
            persistenceProvider.RetrieveInstance<DataStructure>(dataStructureId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_DataStructureNotFound, dataStructureId)
            );

        if (DataServiceFactory.GetDataService() is not AbstractSqlDataService dataService)
        {
            throw new UserOrigamException(Strings.Wizard_DataServiceNotSql);
        }

        var sqlGenerator = (AbstractSqlCommandGenerator)dataService.DbDataAdapterFactory.Clone();
        sqlGenerator.PrettyFormat = true;
        sqlGenerator.GenerateConsoleUseSyntax = true;

        var output = new StringBuilder();
        output.AppendLine($"-- SQL statements for data structure: {dataStructure.Name}");
        var tmpTables = new List<string>();
        foreach (var dsEntity in dataStructure.Entities)
        {
            if (dsEntity.Columns.Count <= 0)
            {
                continue;
            }
            string tmpTable = $"{TempTablePrefix}{Guid.NewGuid()}";
            tmpTables.Add(tmpTable);
            output.AppendLine(sqlGenerator.CreateOutputTableSql(tmpTable));
            output.AppendLine("-----------------------------------------------------------------");
            output.AppendLine($"-- {dsEntity.Name}");
            output.AppendLine("-----------------------------------------------------------------");
            output.Append(
                sqlGenerator.SelectSql(
                    ds: dataStructure,
                    entity: dsEntity,
                    filter: null,
                    sortSet: null,
                    columnsInfo: ColumnsInfo.Empty,
                    parameters: new Hashtable(),
                    selectParameterReferences: null,
                    paging: false
                )
            );
            output.AppendLine(";");
        }
        output.AppendLine(sqlGenerator.CreateDataStructureFooterSql(tmpTables));

        return new GetDataStructureSqlResult
        {
            DataStructureId = dataStructure.Id,
            DataStructureName = dataStructure.Name,
            Sql = output.ToString(),
        };
    }

    public CreateWizardResult CreateMenuItem(CreateMenuItemModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Caption))
        {
            throw new UserOrigamException(Strings.Wizard_MenuCaptionRequired);
        }

        var form =
            persistenceProvider.RetrieveInstance<FormControlSet>(input.FormId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_FormControlSetNotFound, input.FormId)
            );

        var role = string.IsNullOrWhiteSpace(input.Role) ? AllRoles : input.Role.Trim();

        var generated = new List<ISchemaItem>();
        _ = transaction.Run(() =>
        {
            var item = MenuHelper.CreateMenuItem(input.Caption.Trim(), role, form);
            generated.Add(item);

            var schemaService = ServiceManager.Services.GetService<ISchemaService>();
            var deploymentProvider = schemaService?.GetProvider<DeploymentSchemaItemProvider>();
            var hasCurrentVersion = deploymentProvider?.CurrentVersion() != null;

            if (role != AllRoles && role != "" && hasCurrentVersion)
            {
                var settings = ConfigurationManager.GetActiveConfiguration();
                var activeSqlDataService =
                    DataServiceFactory.GetDataService() as AbstractSqlDataService;
                if (settings.DeployPlatforms != null)
                {
                    foreach (var platform in settings.DeployPlatforms)
                    {
                        if (
                            DataServiceFactory.GetDataService(platform)
                            is AbstractSqlDataService platformSqlDataService
                        )
                        {
                            var platformActivity = DeploymentHelper.CreateSystemRole(
                                role,
                                platformSqlDataService
                            );
                            generated.Add(platformActivity);
                        }
                    }
                }
                if (activeSqlDataService != null)
                {
                    var activity = DeploymentHelper.CreateSystemRole(role, activeSqlDataService);
                    generated.Add(activity);
                }
            }

            return item;
        });

        return new CreateWizardResult { SearchResults = searchService.BuildResults(generated) };
    }

    public CreateWizardResult CreateWorkflowMenuItem(CreateWorkflowMenuItemModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Caption))
        {
            throw new UserOrigamException(Strings.Wizard_MenuCaptionRequired);
        }

        var workflow =
            persistenceProvider.RetrieveInstance<IWorkflow>(input.WorkflowId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_WorkflowNotFound, input.WorkflowId)
            );

        var role = string.IsNullOrWhiteSpace(input.Role) ? AllRoles : input.Role.Trim();

        var generated = new List<ISchemaItem>();
        _ = transaction.Run(() =>
        {
            var item = MenuHelper.CreateMenuItem(input.Caption.Trim(), role, workflow);
            generated.Add(item);

            var schemaService = ServiceManager.Services.GetService<ISchemaService>();
            var deploymentProvider = schemaService?.GetProvider<DeploymentSchemaItemProvider>();
            var hasCurrentVersion = deploymentProvider?.CurrentVersion() != null;

            if (role != AllRoles && role != "" && hasCurrentVersion)
            {
                var settings = ConfigurationManager.GetActiveConfiguration();
                var activeSqlDataService =
                    DataServiceFactory.GetDataService() as AbstractSqlDataService;
                if (settings.DeployPlatforms != null)
                {
                    foreach (var platform in settings.DeployPlatforms)
                    {
                        if (
                            DataServiceFactory.GetDataService(platform)
                            is AbstractSqlDataService platformSqlDataService
                        )
                        {
                            var platformActivity = DeploymentHelper.CreateSystemRole(
                                role,
                                platformSqlDataService
                            );
                            generated.Add(platformActivity);
                        }
                    }
                }
                if (activeSqlDataService != null)
                {
                    var activity = DeploymentHelper.CreateSystemRole(role, activeSqlDataService);
                    generated.Add(activity);
                }
            }

            return item;
        });

        return new CreateWizardResult { SearchResults = searchService.BuildResults(generated) };
    }

    public CreateWizardResult CreateRole(CreateRoleModel input)
    {
        var item =
            persistenceProvider.RetrieveInstance<ISchemaItem>(input.ItemId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_RoleItemNotFound, input.ItemId)
            );

        if (
            item is not IAuthorizationContextContainer roleItem
            || string.IsNullOrEmpty(roleItem.AuthorizationContext)
            || roleItem.AuthorizationContext == AllRoles
        )
        {
            throw new UserOrigamException(Strings.Wizard_RoleNotSpecific);
        }

        var role = roleItem.AuthorizationContext;

        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var deploymentProvider = schemaService?.GetProvider<DeploymentSchemaItemProvider>();
        if (deploymentProvider?.CurrentVersion() == null)
        {
            throw new UserOrigamException(Strings.Wizard_NoCurrentDeploymentVersion);
        }

        var generated = new List<ISchemaItem>();
        _ = transaction.Run(() =>
        {
            var settings = ConfigurationManager.GetActiveConfiguration();
            if (settings.DeployPlatforms != null)
            {
                foreach (var platform in settings.DeployPlatforms)
                {
                    if (
                        DataServiceFactory.GetDataService(platform)
                        is AbstractSqlDataService platformSqlDataService
                    )
                    {
                        var platformActivity = DeploymentHelper.CreateSystemRole(
                            role,
                            platformSqlDataService
                        );
                        generated.Add(platformActivity);
                    }
                }
            }
            if (DataServiceFactory.GetDataService() is AbstractSqlDataService activeSqlDataService)
            {
                var activity = DeploymentHelper.CreateSystemRole(role, activeSqlDataService);
                generated.Add(activity);
            }
            return generated;
        });

        return new CreateWizardResult { SearchResults = searchService.BuildResults(generated) };
    }

    public LookupWizardData GetLookupWizardData(Guid entityId)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(entityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, entityId)
            );

        var primaryKey =
            entity.EntityColumns.FirstOrDefault(column =>
                column.IsPrimaryKey && !column.ExcludeFromAllFields
            ) ?? throw new UserOrigamException(Strings.Wizard_EntityHasNoPrimaryKeyForLookup);

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
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException(Strings.Wizard_LookupNameRequired);
        }

        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, input.EntityId)
            );

        var idColumn =
            entity.EntityColumns.FirstOrDefault(column =>
                column.IsPrimaryKey && !column.ExcludeFromAllFields
            ) ?? throw new UserOrigamException(Strings.Wizard_EntityHasNoPrimaryKey);

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

        return transaction.Run(() =>
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
            return new CreateWizardResult
            {
                SearchResults = searchService.BuildResults([lookup, lookup.ListDataStructure]),
            };
        });
    }

    public LocalizationChildEntityWizardData GetLocalizationChildEntityWizardData(Guid entityId)
    {
        var entity =
            persistenceProvider.RetrieveInstance<TableMappingItem>(entityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_TableEntityNotFound, entityId)
            );

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
            Columns = columns,
        };
    }

    public CreateWizardResult CreateLocalizationChildEntity(
        CreateLocalizationChildEntityModel input
    )
    {
        var entity =
            persistenceProvider.RetrieveInstance<TableMappingItem>(input.EntityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_TableEntityNotFound, input.EntityId)
            );

        var selectedColumns = new ArrayList();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column =
                entity.EntityColumns.FirstOrDefault(c => c.Id == fieldId)
                ?? throw new UserOrigamException(
                    string.Format(Strings.Wizard_FieldNotFoundOnEntity, fieldId)
                );
            selectedColumns.Add(column);
        }

        var generated = new List<ISchemaItem>();
        transaction.Run(() =>
        {
            var translationEntity = EntityHelper.CreateLanguageTranslationChildEntity(
                entity,
                selectedColumns,
                generated
            );

            var schemaService = ServiceManager.Services.GetService<ISchemaService>();
            var deploymentProvider = schemaService?.GetProvider<DeploymentSchemaItemProvider>();
            var hasCurrentVersion = deploymentProvider?.CurrentVersion() != null;

            if (hasCurrentVersion)
            {
                var settings = ConfigurationManager.GetActiveConfiguration();
                if (settings.DeployPlatforms != null)
                {
                    foreach (var platform in settings.DeployPlatforms)
                    {
                        if (
                            DataServiceFactory.GetDataService(platform)
                            is AbstractSqlDataService platformSqlDataService
                        )
                        {
                            var platformScript = platformSqlDataService.EntityDdl(
                                translationEntity.Id
                            );
                            var platformActivity = DeploymentHelper.CreateDatabaseScript(
                                translationEntity.Name,
                                platformScript,
                                platformSqlDataService.PlatformName
                            );
                            generated.Add(platformActivity);
                        }
                    }
                }
                if (
                    DataServiceFactory.GetDataService()
                    is AbstractSqlDataService activeSqlDataService
                )
                {
                    var script = activeSqlDataService.EntityDdl(translationEntity.Id);
                    var activity = DeploymentHelper.CreateDatabaseScript(
                        translationEntity.Name,
                        script,
                        activeSqlDataService.PlatformName
                    );
                    generated.Add(activity);
                }
            }

            return translationEntity;
        });

        return new CreateWizardResult { SearchResults = searchService.BuildResults(generated) };
    }

    public ScreenSectionWizardData GetScreenSectionWizardData(Guid entityId)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(entityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, entityId)
            );

        var columns = entity
            .EntityColumns.Where(column => !string.IsNullOrEmpty(column.ToString()))
            .OrderBy(column => column.Name)
            .Select(column => new ScreenWizardColumn
            {
                Id = column.Id,
                Name = column.Name,
                IsPrimaryKey = column.IsPrimaryKey,
            })
            .ToList();

        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var panelProvider = schemaService?.GetProvider<PanelSchemaItemProvider>();
        var existingNames =
            panelProvider
                ?.ChildItemsByType<ISchemaItem>(PanelControlSet.CategoryConst)
                .Select(item => item.Name)
                .ToList() ?? new List<string>();

        return new ScreenSectionWizardData
        {
            EntityName = entity.Name,
            EntityCaption = entity.Caption,
            Columns = columns,
            ExistingScreenSectionNames = existingNames,
        };
    }

    public CreateWizardResult CreateScreenSection(CreateScreenSectionModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException(Strings.Wizard_ScreenSectionNameRequired);
        }

        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException(
                string.Format(Strings.Wizard_EntityNotFound, input.EntityId)
            );

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            throw new UserOrigamException(Strings.Wizard_AtLeastOneFieldRequired);
        }

        var trimmedName = input.Name.Trim();
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var panelProvider = schemaService?.GetProvider<PanelSchemaItemProvider>();
        var duplicate = panelProvider
            ?.ChildItemsByType<ISchemaItem>(PanelControlSet.CategoryConst)
            .FirstOrDefault(item =>
                string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
            );
        if (duplicate != null)
        {
            throw new UserOrigamException(
                string.Format(Strings.Wizard_ScreenSectionAlreadyExists, trimmedName)
            );
        }

        var selectedNames = new Hashtable();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column =
                entity.EntityColumns.FirstOrDefault(column => column.Id == fieldId)
                ?? throw new UserOrigamException(
                    string.Format(Strings.Wizard_FieldNotFoundOnEntity, fieldId)
                );
            selectedNames[column.Name] = true;
        }

        var groupName = entity.Group?.Name;

        var panel = transaction.Run(() =>
            GuiHelper.CreatePanel(groupName, entity, selectedNames, trimmedName)
        );

        transaction.Run(() =>
        {
            RelayoutScreenSectionFields(panel);
            return panel;
        });

        if (!string.IsNullOrWhiteSpace(input.Caption) && panel.ChildItems.Count > 0)
        {
            transaction.Run(() =>
            {
                var rootControl = panel.ChildItems[0];
                var titleProp = rootControl
                    .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
                    .FirstOrDefault(prop =>
                        prop.ControlPropertyItem?.Name == PanelTitlePropertyName
                    );
                if (titleProp != null)
                {
                    titleProp.Value = input.Caption.Trim();
                    titleProp.Persist();
                }
            });
        }

        return new CreateWizardResult { SearchResults = searchService.BuildResults([panel]) };
    }

    private static void RelayoutScreenSectionFields(PanelControlSet panel)
    {
        if (panel.ChildItems.Count == 0)
        {
            return;
        }
        var rootControl = panel.ChildItems[0];
        var withTops = rootControl
            .ChildItemsByType<ControlSetItem>(ControlSetItem.CategoryConst)
            .Select(control => new
            {
                Control = control,
                TopProp = control
                    .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
                    .FirstOrDefault(prop => prop.ControlPropertyItem?.Name == TopPropertyName),
            })
            .Where(item => item.TopProp != null)
            .OrderBy(item => int.TryParse(item.TopProp.Value, out var top) ? top : int.MaxValue)
            .ToList();

        var y = ScreenSectionFieldTopStart;
        foreach (var item in withTops)
        {
            item.TopProp.Value = y.ToString();
            item.TopProp.Persist();
            y += ScreenSectionFieldTopStep;
        }
    }
}
