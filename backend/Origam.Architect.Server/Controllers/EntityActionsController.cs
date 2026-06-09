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
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models.Requests;
using Origam.Architect.Server.Services;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EntityActionsController(
    IPersistenceService persistenceService,
    GitNodeStatusService gitNodeStatusService,
    SearchService searchService
) : ControllerBase
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    /// <summary>
    /// Create a filter on the entity owning the given column. Mirrors WinForms
    /// CreateFilterByFieldCommand family (Equal/Like/In/Between, with or without parameter).
    /// </summary>
    [HttpPost("CreateFilter")]
    public IActionResult CreateFilter([Required] [FromBody] CreateFilterModel input)
    {
        var column = persistenceProvider.RetrieveInstance<IDataEntityColumn>(input.ColumnId);
        if (column == null)
        {
            return NotFound($"Column {input.ColumnId} not found");
        }

        if (column.ParentItem is not IDataEntity)
        {
            return BadRequest("Column does not belong to an entity.");
        }

        try
        {
            persistenceProvider.BeginTransaction();
            var generated = new List<ISchemaItem>();
            EntityFilter filter;
            switch (input.FilterType)
            {
                case CreateFilterType.Equal:
                {
                    filter = EntityHelper.CreateFilter(
                        field: column,
                        functionName: "Equal",
                        filterPrefix: "GetBy",
                        createParameter: false,
                        generatedElements: generated
                    );
                    break;
                }
                case CreateFilterType.EqualParam:
                {
                    filter = EntityHelper.CreateFilter(
                        field: column,
                        functionName: "Equal",
                        filterPrefix: "GetBy",
                        createParameter: true,
                        generatedElements: generated
                    );
                    break;
                }
                case CreateFilterType.Like:
                {
                    filter = EntityHelper.CreateFilter(
                        field: column,
                        functionName: "Like",
                        filterPrefix: "GetLike",
                        createParameter: false,
                        generatedElements: generated
                    );
                    break;
                }
                case CreateFilterType.LikeParam:
                {
                    filter = EntityHelper.CreateFilter(
                        field: column,
                        functionName: "Like",
                        filterPrefix: "GetLike",
                        createParameter: true,
                        generatedElements: generated
                    );
                    break;
                }
                case CreateFilterType.InList:
                {
                    filter = EntityHelper.CreateFilter(
                        field: column,
                        functionName: "In",
                        filterPrefix: "GetBy",
                        createParameter: true,
                        generatedElements: generated
                    );
                    break;
                }
                case CreateFilterType.Between:
                {
                    filter = CreateBetweenFilter(column, generated);
                    break;
                }
                default:
                {
                    persistenceProvider.EndTransactionDontSave();
                    return BadRequest($"Unknown filter type '{input.FilterType}'.");
                }
            }
            persistenceProvider.EndTransaction();
            gitNodeStatusService.ClearCache();
            return Ok(
                new CreateFilterResult
                {
                    FilterId = filter.Id,
                    FilterName = filter.Name,
                    SearchResults = searchService.BuildResults(generated),
                }
            );
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(
                statusCode: 500,
                $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            );
        }
    }

    /// <summary>
    /// Port of WinForms CreateFilterBetweenWithParameterByFieldCommand.Run — creates
    /// two parameters (par&lt;Field&gt;From / par&lt;Field&gt;To), the GetBetween&lt;Field&gt; filter
    /// and a Between function call wired to them.
    /// </summary>
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

    /// <summary>
    /// Returns entity info needed by the Create Screen wizard:
    /// entity name, its columns (id + name), and the names of existing
    /// DataStructures so the wizard can warn before colliding on Name.
    /// </summary>
    [HttpGet("GetScreenWizardData")]
    public IActionResult GetScreenWizardData([FromQuery] [Required] Guid entityId)
    {
        var entity = persistenceProvider.RetrieveInstance<IDataEntity>(entityId);
        if (entity == null)
        {
            return NotFound($"Entity {entityId} not found");
        }

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
                ?.ChildItemsByType<ISchemaItem>(DataStructure.CategoryConst)
                .Select(x => x.Name)
                .ToList() ?? new List<string>();

        return Ok(
            new ScreenWizardData
            {
                EntityName = entity.Name,
                Columns = columns,
                ExistingDataStructureNames = existingNames,
            }
        );
    }

    /// <summary>
    /// Create Screen from an entity: DataStructure + PanelControlSet (Screen Section)
    /// + FormControlSet (Screen). Mirrors WinForms CreateFormFromEntityCommand.Execute.
    /// </summary>
    [HttpPost("CreateScreen")]
    public IActionResult CreateScreen([Required] [FromBody] CreateScreenModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return BadRequest("Screen name is required.");
        }

        var entity = persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId);
        if (entity == null)
        {
            return NotFound($"Entity {input.EntityId} not found");
        }

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            return BadRequest("At least one field must be selected.");
        }

        var trimmedName = input.Name.Trim();
        var schemaService = ServiceManager.Services.GetService<ISchemaService>();
        var dsProvider =
            schemaService?.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var duplicate = dsProvider
            ?.ChildItemsByType<ISchemaItem>(DataStructure.CategoryConst)
            .FirstOrDefault(x =>
                string.Equals(x.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
            );
        if (duplicate != null)
        {
            return Conflict($"A DataStructure named \"{trimmedName}\" already exists.");
        }

        // Resolve selected column ids to names; reject any unknown ids.
        var selectedNames = new Hashtable();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column = entity.EntityColumns.FirstOrDefault(c => c.Id == fieldId);
            if (column == null)
            {
                return BadRequest($"Field {fieldId} not found on entity.");
            }
            selectedNames[column.Name] = true;
        }

        var groupName = entity.Group?.Name;

        try
        {
            persistenceProvider.BeginTransaction();

            var dataStructure = EntityHelper.CreateDataStructure(entity, input.Name, persist: true);
            var panel = GuiHelper.CreatePanel(groupName, entity, selectedNames, input.Name);
            var form = GuiHelper.CreateForm(dataStructure, groupName, panel);

            persistenceProvider.EndTransaction();

            // Optional: override PanelTitle on the freshly-created panel.
            // Done in a separate transaction so a failure here can't roll back the screen.
            if (!string.IsNullOrWhiteSpace(input.Caption) && panel.ChildItems.Count > 0)
            {
                try
                {
                    persistenceProvider.BeginTransaction();
                    var rootControl = panel.ChildItems[0];
                    var titleProp = rootControl
                        .ChildItemsByType<PropertyValueItem>(PropertyValueItem.CategoryConst)
                        .FirstOrDefault(p => p.ControlPropertyItem?.Name == "PanelTitle");
                    if (titleProp != null)
                    {
                        titleProp.Value = input.Caption.Trim();
                        titleProp.Persist();
                    }
                    persistenceProvider.EndTransaction();
                }
                catch
                {
                    persistenceProvider.EndTransactionDontSave();
                    // swallow — Caption override is non-critical; screen is already created.
                }
            }

            gitNodeStatusService.ClearCache();
            return Ok(
                new CreateScreenResult
                {
                    DataStructureId = dataStructure.Id,
                    DataStructureName = dataStructure.Name,
                    PanelId = panel.Id,
                    PanelName = panel.Name,
                    FormId = form.Id,
                    FormName = form.Name,
                    SearchResults = searchService.BuildResults(
                        new ISchemaItem[] { dataStructure, panel, form }
                    ),
                }
            );
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(
                statusCode: 500,
                $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            );
        }
    }

    /// <summary>
    /// Create WorkQueue class for the entity. Mirrors WinForms
    /// CreateWorkQueueClassFromEntityCommand.Execute → WorkflowHelper.CreateWorkQueueClass.
    /// </summary>
    [HttpPost("CreateWorkQueueClass")]
    public IActionResult CreateWorkQueueClass([Required] [FromBody] CreateWorkQueueModel input)
    {
        var entity = persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId);
        if (entity == null)
        {
            return NotFound($"Entity {input.EntityId} not found");
        }

        if (input.SelectedFieldIds == null || input.SelectedFieldIds.Count == 0)
        {
            return BadRequest("At least one field must be selected.");
        }

        // Resolve selected column ids to actual IDataEntityColumn instances.
        var selectedColumns = new ArrayList();
        foreach (var fieldId in input.SelectedFieldIds)
        {
            var column = entity.EntityColumns.FirstOrDefault(c => c.Id == fieldId);
            if (column == null)
            {
                return BadRequest($"Field {fieldId} not found on entity.");
            }
            selectedColumns.Add(column);
        }

        try
        {
            persistenceProvider.BeginTransaction();
            var generated = new List<ISchemaItem>();
            var workQueueClass = WorkflowHelper.CreateWorkQueueClass(
                entity,
                selectedColumns,
                generated
            );
            persistenceProvider.EndTransaction();
            gitNodeStatusService.ClearCache();
            var generatedAll = new List<ISchemaItem> { workQueueClass };
            generatedAll.AddRange(generated);
            return Ok(
                new CreateWorkQueueResult
                {
                    WorkQueueClassId = workQueueClass.Id,
                    WorkQueueClassName = workQueueClass.Name,
                    SearchResults = searchService.BuildResults(generatedAll),
                }
            );
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(
                statusCode: 500,
                $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            );
        }
    }

    /// <summary>
    /// Generate the SQL statements for a DataStructure. Mirrors WinForms
    /// ShowDataStructureSql command — used by the "Show SQL" action.
    /// </summary>
    [HttpGet("GetDataStructureSql")]
    public IActionResult GetDataStructureSql([FromQuery] [Required] Guid dataStructureId)
    {
        var dataStructure = persistenceProvider.RetrieveInstance<DataStructure>(dataStructureId);
        if (dataStructure == null)
        {
            return NotFound($"DataStructure {dataStructureId} not found");
        }

        try
        {
            var dataService = DataServiceFactory.GetDataService() as AbstractSqlDataService;
            if (dataService == null)
            {
                return StatusCode(statusCode: 500, value: "Active data service is not SQL-based.");
            }
            var sqlGenerator = (AbstractSqlCommandGenerator)
                dataService.DbDataAdapterFactory.Clone();
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
                output.AppendLine(
                    "-----------------------------------------------------------------"
                );
                output.AppendLine($"-- {dsEntity.Name}");
                output.AppendLine(
                    "-----------------------------------------------------------------"
                );
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

            return Ok(
                new GetDataStructureSqlResult
                {
                    DataStructureId = dataStructure.Id,
                    DataStructureName = dataStructure.Name,
                    Sql = output.ToString(),
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                statusCode: 500,
                $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            );
        }
    }

    /// <summary>
    /// Create a FormReferenceMenuItem pointing to the given Screen (FormControlSet).
    /// Mirrors WinForms CreateMenuFromFormCommand.Execute → MenuHelper.CreateMenuItem.
    /// </summary>
    [HttpPost("CreateMenuItem")]
    public IActionResult CreateMenuItem([Required] [FromBody] CreateMenuItemModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Caption))
        {
            return BadRequest("Menu caption is required.");
        }

        var form = persistenceProvider.RetrieveInstance<FormControlSet>(input.FormId);
        if (form == null)
        {
            return NotFound($"Screen (FormControlSet) {input.FormId} not found");
        }

        var role = string.IsNullOrWhiteSpace(input.Role) ? "*" : input.Role.Trim();

        try
        {
            persistenceProvider.BeginTransaction();
            var menuItem = MenuHelper.CreateMenuItem(input.Caption.Trim(), role, form);

            var generated = new List<ISchemaItem> { menuItem };

            // Mirror WinForms CreateMenuFromFormCommand.Execute: if Role is a real
            // role name (not "*", not empty), generate AddRole_<role> deployment
            // scripts so the role actually exists in OrigamRole after deploy.
            // Skip silently if no current DeploymentVersion exists in the active
            // package — DeploymentHelper.CreateDatabaseScript would NRE otherwise.
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
                        var platformSqlDataService =
                            DataServiceFactory.GetDataService(platform) as AbstractSqlDataService;
                        if (platformSqlDataService != null)
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

            persistenceProvider.EndTransaction();
            gitNodeStatusService.ClearCache();
            return Ok(
                new CreateMenuItemResult
                {
                    MenuItemId = menuItem.Id,
                    MenuItemName = menuItem.Name,
                    SearchResults = searchService.BuildResults(generated),
                }
            );
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(
                statusCode: 500,
                $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            );
        }
    }
}

public enum CreateFilterType
{
    Equal,
    EqualParam,
    Like,
    LikeParam,
    InList,
    Between,
}

public class CreateFilterModel
{
    [Required]
    public Guid ColumnId { get; set; }

    [Required]
    public CreateFilterType FilterType { get; set; }
}

public class CreateFilterResult
{
    public Guid FilterId { get; set; }
    public string FilterName { get; set; }
    public List<SearchResult> SearchResults { get; set; } = new();
}

public class ScreenWizardData
{
    public string EntityName { get; set; }
    public List<ScreenWizardColumn> Columns { get; set; } = new();
    public List<string> ExistingDataStructureNames { get; set; } = new();
}

public class ScreenWizardColumn
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsPrimaryKey { get; set; }
}

public class CreateScreenModel
{
    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Caption { get; set; }

    [Required]
    public List<Guid> SelectedFieldIds { get; set; } = new();
}

public class CreateScreenResult
{
    public Guid DataStructureId { get; set; }
    public string DataStructureName { get; set; }
    public Guid PanelId { get; set; }
    public string PanelName { get; set; }
    public Guid FormId { get; set; }
    public string FormName { get; set; }
    public List<SearchResult> SearchResults { get; set; } = new();
}

public class CreateWorkQueueModel
{
    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public List<Guid> SelectedFieldIds { get; set; } = new();
}

public class CreateWorkQueueResult
{
    public Guid WorkQueueClassId { get; set; }
    public string WorkQueueClassName { get; set; }
    public List<SearchResult> SearchResults { get; set; } = new();
}

public class CreateMenuItemModel
{
    [Required]
    public Guid FormId { get; set; }

    [Required]
    public string Caption { get; set; }

    public string Role { get; set; }
}

public class CreateMenuItemResult
{
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; }
    public List<SearchResult> SearchResults { get; set; } = new();
}

public class GetDataStructureSqlResult
{
    public Guid DataStructureId { get; set; }
    public string DataStructureName { get; set; }
    public string Sql { get; set; }
}
