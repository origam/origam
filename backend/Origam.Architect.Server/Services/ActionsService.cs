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
using Origam.Architect.Server.Interfaces.Services;
using Origam.Architect.Server.Models.Requests.Actions;
using Origam.Architect.Server.Models.Responses.Actions;
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

public class ActionsService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : IFilterActions, IScreenActions, IWorkQueueActions, ISqlActions, IMenuActions, ILookupActions
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    public CreateActionResult CreateFilter(CreateFilterModel input)
    {
        var column =
            persistenceProvider.RetrieveInstance<IDataEntityColumn>(input.ColumnId)
            ?? throw new UserOrigamException($"Column {input.ColumnId} not found");

        if (column.ParentItem is not IDataEntity)
        {
            throw new UserOrigamException("Column does not belong to an entity.");
        }

        var generated = new List<ISchemaItem>();
        _ = transaction.Run(() =>
            input.FilterType switch
            {
                CreateFilterType.Equal => EntityHelper.CreateFilter(
                    field: column,
                    functionName: "Equal",
                    filterPrefix: "GetBy",
                    createParameter: false,
                    generatedElements: generated
                ),
                CreateFilterType.EqualParam => EntityHelper.CreateFilter(
                    field: column,
                    functionName: "Equal",
                    filterPrefix: "GetBy",
                    createParameter: true,
                    generatedElements: generated
                ),
                CreateFilterType.Like => EntityHelper.CreateFilter(
                    field: column,
                    functionName: "Like",
                    filterPrefix: "GetLike",
                    createParameter: false,
                    generatedElements: generated
                ),
                CreateFilterType.LikeParam => EntityHelper.CreateFilter(
                    field: column,
                    functionName: "Like",
                    filterPrefix: "GetLike",
                    createParameter: true,
                    generatedElements: generated
                ),
                CreateFilterType.InList => EntityHelper.CreateFilter(
                    field: column,
                    functionName: "In",
                    filterPrefix: "GetBy",
                    createParameter: true,
                    generatedElements: generated
                ),
                CreateFilterType.Between => CreateBetweenFilter(column, generated),
                _ => throw new UserOrigamException($"Unknown filter type '{input.FilterType}'."),
            }
        );

        return new CreateActionResult { SearchResults = searchService.BuildResults(generated) };
    }

    private static EntityFilter CreateBetweenFilter(
        IDataEntityColumn field,
        IList<ISchemaItem> generated
    )
    {
        if (string.IsNullOrEmpty(field.Name))
        {
            throw new ArgumentException("Field Name is not set.");
        }
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var entity = (IDataEntity)field.ParentItem;
        var baseName = field.Name.StartsWith("ref") ? field.Name.Substring(3) : field.Name;

        var paramFrom = entity.NewItem<DatabaseParameter>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        paramFrom.DataType = field.DataType;
        paramFrom.DataLength = field.DataLength;
        paramFrom.Name = "par" + baseName + "From";
        paramFrom.Persist();
        generated.Add(paramFrom);

        var paramTo = entity.NewItem<DatabaseParameter>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        paramTo.DataType = field.DataType;
        paramTo.DataLength = field.DataLength;
        paramTo.Name = "par" + baseName + "To";
        paramTo.Persist();
        generated.Add(paramTo);

        var filter = entity.NewItem<EntityFilter>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        filter.Name = "GetBetween" + baseName;
        filter.Persist();
        generated.Add(filter);

        var call = filter.NewItem<FunctionCall>(
            schemaExtensionId: schemaService.ActiveSchemaExtensionId,
            group: null
        );
        var functionProvider = schemaService.GetProvider<FunctionSchemaItemProvider>();
        var betweenFunction = (Function)
            functionProvider.GetChildByName(name: "Between", itemType: Function.CategoryConst);
        if (betweenFunction == null)
        {
            throw new Exception("Between function not found. Cannot create filter.");
        }
        call.Function = betweenFunction;
        call.Name = "Between";
        call.Persist();

        var expressionRef = call.GetChildByName(name: "Expression")
            .NewItem<EntityColumnReference>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
        expressionRef.Field = field;
        expressionRef.Persist();

        var leftRef = call.GetChildByName(name: "Left")
            .NewItem<ParameterReference>(
                schemaExtensionId: schemaService.ActiveSchemaExtensionId,
                group: null
            );
        leftRef.Parameter = paramFrom;
        leftRef.Persist();

        var rightRef = call.GetChildByName(name: "Right")
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
            ?? throw new UserOrigamException($"Entity {entityId} not found");

        var columns = entity
            .EntityColumns.Where(c => !string.IsNullOrEmpty(c.ToString()))
            .OrderBy(c => c.Name)
            .Select(c => new ScreenWizardColumn
            {
                Id = c.Id,
                Name = c.Name,
                IsPrimaryKey = c.IsPrimaryKey,
            })
            .ToList();

        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var existingNames =
            dsProvider
                ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
                .Select(x => x.Name)
                .ToList() ?? new List<string>();

        return new ScreenWizardData
        {
            EntityName = entity.Name,
            Columns = columns,
            ExistingDataStructureNames = existingNames,
        };
    }

    public CreateActionResult CreateScreen(CreateScreenModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException("Screen name is required.");
        }

        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException($"Entity {input.EntityId} not found");

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            throw new UserOrigamException("At least one field must be selected.");
        }

        var trimmedName = input.Name.Trim();
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var duplicate = dsProvider
            ?.ChildItemsByType<ISchemaItem>(AbstractDataStructure.CategoryConst)
            .FirstOrDefault(x =>
                string.Equals(x.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
            );
        if (duplicate != null)
        {
            throw new UserOrigamException(
                $"A DataStructure named \"{trimmedName}\" already exists."
            );
        }

        var selectedNames = new Hashtable();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column =
                entity.EntityColumns.FirstOrDefault(c => c.Id == fieldId)
                ?? throw new UserOrigamException($"Field {fieldId} not found on entity.");
            selectedNames[column.Name] = true;
        }

        var groupName = entity.Group?.Name;

        var (dataStructure, panel, form) = transaction.Run(() =>
        {
            var ds = EntityHelper.CreateDataStructure(entity, input.Name, persist: true);
            var p = GuiHelper.CreatePanel(groupName, entity, selectedNames, input.Name);
            var f = GuiHelper.CreateForm(ds, groupName, p);
            return (ds, p, f);
        });

        if (!string.IsNullOrWhiteSpace(input.Caption) && panel.ChildItems.Count > 0)
        {
            transaction.Run(() =>
            {
                var rootControl = panel.ChildItems[0];
                var titleProp = rootControl
                    .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
                    .FirstOrDefault(p => p.ControlPropertyItem?.Name == "PanelTitle");
                if (titleProp != null)
                {
                    titleProp.Value = input.Caption.Trim();
                    titleProp.Persist();
                }
            });
        }

        return new CreateActionResult
        {
            SearchResults = searchService.BuildResults([dataStructure, panel, form]),
        };
    }

    public CreateActionResult CreateWorkQueueClass(CreateWorkQueueModel input)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException($"Entity {input.EntityId} not found");

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            throw new UserOrigamException("At least one field must be selected.");
        }

        var selectedColumns = new ArrayList();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column =
                entity.EntityColumns.FirstOrDefault(c => c.Id == fieldId)
                ?? throw new UserOrigamException($"Field {fieldId} not found on entity.");
            selectedColumns.Add(column);
        }

        var generated = new List<ISchemaItem>();
        var workQueueClass = transaction.Run(() =>
            WorkflowHelper.CreateWorkQueueClass(entity, selectedColumns, generated)
        );

        var generatedAll = new List<ISchemaItem> { workQueueClass };
        generatedAll.AddRange(generated);
        return new CreateActionResult { SearchResults = searchService.BuildResults(generatedAll) };
    }

    public GetDataStructureSqlResult GetDataStructureSql(Guid dataStructureId)
    {
        var dataStructure =
            persistenceProvider.RetrieveInstance<DataStructure>(dataStructureId)
            ?? throw new UserOrigamException($"DataStructure {dataStructureId} not found");

        if (DataServiceFactory.GetDataService() is not AbstractSqlDataService dataService)
        {
            throw new UserOrigamException("Active data service is not SQL-based.");
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
            string tmpTable = $"tmptable{Guid.NewGuid()}";
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

    public CreateActionResult CreateMenuItem(CreateMenuItemModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Caption))
        {
            throw new UserOrigamException("Menu caption is required.");
        }

        var form =
            persistenceProvider.RetrieveInstance<FormControlSet>(input.FormId)
            ?? throw new UserOrigamException($"Screen (FormControlSet) {input.FormId} not found");

        var role = string.IsNullOrWhiteSpace(input.Role) ? "*" : input.Role.Trim();

        var generated = new List<ISchemaItem>();
        _ = transaction.Run(() =>
        {
            var item = MenuHelper.CreateMenuItem(input.Caption.Trim(), role, form);
            generated.Add(item);

            var schemaService = ServiceManager.Services.GetService<ISchemaService>();
            var deploymentProvider = schemaService?.GetProvider<DeploymentSchemaItemProvider>();
            var hasCurrentVersion = deploymentProvider?.CurrentVersion() != null;

            if (role != "*" && role != "" && hasCurrentVersion)
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

        return new CreateActionResult { SearchResults = searchService.BuildResults(generated) };
    }

    public LookupWizardData GetLookupWizardData(Guid entityId)
    {
        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(entityId)
            ?? throw new UserOrigamException($"Entity {entityId} not found");

        var primaryKey =
            entity.EntityColumns.FirstOrDefault(c => c.IsPrimaryKey && !c.ExcludeFromAllFields)
            ?? throw new UserOrigamException(
                "Entity has no primary key defined. Cannot create lookup."
            );

        var columns = entity
            .EntityColumns.Where(c => !string.IsNullOrEmpty(c.ToString()))
            .OrderBy(c => c.Name)
            .Select(c => new IdName { Id = c.Id, Name = c.Name })
            .ToList();

        var filters = entity
            .EntityFilters.OrderBy(f => f.Name)
            .Select(f => new IdName { Id = f.Id, Name = f.Name })
            .ToList();

        var defaultDisplay = entity.EntityColumns.FirstOrDefault(c => c.Name == "Name");

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

    public CreateActionResult CreateLookup(CreateLookupModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserOrigamException("Lookup name is required.");
        }

        var entity =
            persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId)
            ?? throw new UserOrigamException($"Entity {input.EntityId} not found");

        var idColumn =
            entity.EntityColumns.FirstOrDefault(c => c.IsPrimaryKey && !c.ExcludeFromAllFields)
            ?? throw new UserOrigamException("Entity has no primary key defined.");

        var displayColumn =
            entity.EntityColumns.FirstOrDefault(c => c.Id == input.DisplayFieldId)
            ?? throw new UserOrigamException("Display field not found on entity.");

        var idFilter =
            entity.EntityFilters.FirstOrDefault(f => f.Id == input.IdFilterId)
            ?? throw new UserOrigamException("Id filter not found on entity.");

        EntityFilter listFilter = null;
        if (input.ListFilterId.HasValue && input.ListFilterId.Value != Guid.Empty)
        {
            listFilter =
                entity.EntityFilters.FirstOrDefault(f => f.Id == input.ListFilterId.Value)
                ?? throw new UserOrigamException("List filter not found on entity.");
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
            return new CreateActionResult
            {
                SearchResults = searchService.BuildResults([lookup, lookup.ListDataStructure]),
            };
        });
    }
}
