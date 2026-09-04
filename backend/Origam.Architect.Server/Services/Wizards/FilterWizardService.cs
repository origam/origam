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
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Wizards;

public class FilterWizardService(
    IPersistenceService persistenceService,
    ModelTransactionRunner transaction,
    SearchService searchService
) : WizardServiceBase(persistenceService, transaction, searchService)
{
    private const string DbParamPrefix = "par";
    private const string RefColumnPrefix = "ref";
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

    public FilterWizardData GetWizardData(Guid entityId)
    {
        var entity = RetrieveEntity(entityId);

        var columns = entity
            .EntityColumns.Where(column => !string.IsNullOrEmpty(column.ToString()))
            .OrderBy(column => column.Name)
            .Select(column => new FilterWizardColumn
            {
                Id = column.Id,
                Name = column.Name,
                IsPrimaryKey = column.IsPrimaryKey,
                DataType = column.DataType.ToString(),
            })
            .ToList();

        return new FilterWizardData
        {
            EntityName = entity.Name,
            Columns = columns,
            ExistingFilters = entity
                .EntityFilters.OrderBy(filter => filter.Name)
                .Select(filter => new IdName { Id = filter.Id, Name = filter.Name })
                .ToList(),
            FilterTypes = Enum.GetNames<CreateFilterType>().ToList(),
        };
    }

    public CreateWizardResult CreateFilter(CreateFilterModel input)
    {
        var column = Retrieve<IDataEntityColumn>(input.ColumnId, Strings.Wizard_ColumnNotFound);

        if (column.ParentItem is not IDataEntity)
        {
            throw new UserOrigamException(Strings.Wizard_ColumnNotInEntity);
        }

        var generated = new List<ISchemaItem>();
        _ = Transaction.Run(() =>
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

        return BuildResult(generated);
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
}
