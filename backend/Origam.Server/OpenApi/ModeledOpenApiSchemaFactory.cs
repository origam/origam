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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiSchemaFactory
{
    public IList<OpenApiParameter> CreateParameters(AbstractPage page)
    {
        var parameters = new List<OpenApiParameter>();
        var mappingGroups = page.ChildItemsByType<PageParameterMapping>(
                PageParameterMapping.CategoryConst
            )
            .Where(mapping =>
                mapping is not PageParameterFileMapping
                && !string.IsNullOrWhiteSpace(mapping.MappedParameter)
                && !mapping.MappedParameter.StartsWith(
                    value: "Server_",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
            )
            .GroupBy(mapping => mapping.MappedParameter, StringComparer.OrdinalIgnoreCase);
        foreach (IGrouping<string, PageParameterMapping> mappingGroup in mappingGroups)
        {
            PageParameterMapping firstMapping = mappingGroup.First();
            bool isPathParameter = page.Url.Contains(
                $"{{{firstMapping.MappedParameter}}}",
                StringComparison.OrdinalIgnoreCase
            );
            bool allMappingsAreLists = mappingGroup.All(mapping => mapping.IsList);
            OpenApiSchema itemSchema = CreateCommonParameterSchema(page, mappingGroup);
            string[] modeledParameterNames = mappingGroup
                .Select(mapping => mapping.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            parameters.Add(
                new OpenApiParameter
                {
                    Name = firstMapping.MappedParameter,
                    In = isPathParameter ? ParameterLocation.Path : ParameterLocation.Query,
                    Required = isPathParameter,
                    Description =
                        modeledParameterNames.Length == 1
                            ? string.Format(
                                Resources.ModeledApiParameterMapping,
                                modeledParameterNames[0]
                            )
                            : string.Format(
                                Resources.ModeledApiParametersMapping,
                                string.Join(
                                    separator: ", ",
                                    values: modeledParameterNames.Select(name => $"'{name}'")
                                )
                            ),
                    Schema = allMappingsAreLists
                        ? new OpenApiSchema { Type = "array", Items = itemSchema }
                        : itemSchema,
                    Style = allMappingsAreLists ? ParameterStyle.Form : null,
                    Explode = false,
                }
            );
        }

        if (page is XsltDataPage { AllowCustomFilters: true })
        {
            parameters.Add(CreateIntegerQueryParameter("_pageSize"));
            parameters.Add(CreateIntegerQueryParameter("_pageNumber"));
        }

        return parameters;
    }

    private static OpenApiSchema CreateCommonParameterSchema(
        AbstractPage page,
        IEnumerable<PageParameterMapping> mappings
    )
    {
        OpenApiSchema[] schemas = mappings
            .Select(mapping => CreateParameterSchema(page, mapping))
            .ToArray();
        return schemas.All(schema =>
            schema.Type == schemas[0].Type && schema.Format == schemas[0].Format
        )
            ? schemas[0]
            : new OpenApiSchema { Type = "string" };
    }

    private static OpenApiSchema CreateParameterSchema(
        AbstractPage page,
        PageParameterMapping mapping
    )
    {
        OrigamDataType? dataType = ResolveParameterDataType(page, mapping.Name);
        if (dataType == null && mapping.DefaultValue != null)
        {
            dataType = mapping.DefaultValue.DataType;
        }
        return dataType == null
            ? new OpenApiSchema { Type = "string" }
            : CreateDataTypeSchema(dataType.Value);
    }

    private static OrigamDataType? ResolveParameterDataType(AbstractPage page, string parameterName)
    {
        Origam.Schema.WorkflowModel.Workflow workflow = page switch
        {
            WorkflowPage workflowPage => workflowPage.Workflow,
            XsltDataPage { Method: DataStructureWorkflowMethod workflowMethod } =>
                workflowMethod.LoadWorkflow,
            FileDownloadPage { Method: DataStructureWorkflowMethod workflowMethod } =>
                workflowMethod.LoadWorkflow,
            _ => null,
        };
        ContextStore contextStore = workflow
            ?.ChildItemsByType<ContextStore>(ContextStore.CategoryConst)
            .FirstOrDefault(context =>
                string.Equals(context.Name, parameterName, StringComparison.OrdinalIgnoreCase)
            );
        if (contextStore != null)
        {
            return contextStore.DataType;
        }

        IEnumerable<Dictionary<string, ParameterReference>> parameterReferenceSets = page switch
        {
            XsltDataPage dataPage => new[]
            {
                dataPage.Method?.ParameterReferences,
                dataPage.DataStructure?.ParameterReferences,
            },
            FileDownloadPage downloadPage => new[]
            {
                downloadPage.Method?.ParameterReferences,
                downloadPage.DataStructure?.ParameterReferences,
            },
            ReportPage reportPage => new[] { reportPage.Report?.ParameterReferences },
            _ => Array.Empty<Dictionary<string, ParameterReference>>(),
        };
        foreach (
            Dictionary<
                string,
                ParameterReference
            > parameterReferences in parameterReferenceSets.Where(references => references != null)
        )
        {
            ParameterReference parameterReference = parameterReferences
                .FirstOrDefault(pair =>
                    string.Equals(pair.Key, parameterName, StringComparison.OrdinalIgnoreCase)
                )
                .Value;
            if (parameterReference?.Parameter != null)
            {
                return parameterReference.Parameter.DataType;
            }
        }
        return null;
    }

    private static OpenApiSchema CreateDataTypeSchema(OrigamDataType dataType)
    {
        return dataType switch
        {
            OrigamDataType.Boolean => new OpenApiSchema { Type = "boolean" },
            OrigamDataType.Byte => new OpenApiSchema { Type = "integer", Format = "int32" },
            OrigamDataType.Currency => new OpenApiSchema { Type = "number", Format = "double" },
            OrigamDataType.Date => new OpenApiSchema { Type = "string", Format = "date-time" },
            OrigamDataType.Long => new OpenApiSchema { Type = "integer", Format = "int64" },
            OrigamDataType.Float => new OpenApiSchema { Type = "number", Format = "double" },
            OrigamDataType.Integer => new OpenApiSchema { Type = "integer", Format = "int32" },
            OrigamDataType.UniqueIdentifier => new OpenApiSchema
            {
                Type = "string",
                Format = "uuid",
            },
            OrigamDataType.Blob => new OpenApiSchema { Type = "string", Format = "byte" },
            OrigamDataType.Array => new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema { Type = "string" },
            },
            OrigamDataType.Object or OrigamDataType.Xml or OrigamDataType.Geography =>
                new OpenApiSchema { Type = "object", AdditionalPropertiesAllowed = true },
            _ => new OpenApiSchema { Type = "string" },
        };
    }

    private static OpenApiParameter CreateIntegerQueryParameter(string name)
    {
        return new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Required = false,
            Schema = new OpenApiSchema { Type = "integer", Format = "int32" },
        };
    }

    public OpenApiSchema CreateCustomFilterSchema(XsltDataPage page)
    {
        DataStructureEntity entity = page.DataStructure.Entities.First();
        List<DataStructureColumn> columns = entity
            .Columns.OrderBy(column => column.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        string availableColumns = string.Join(
            separator: ", ",
            values: columns.Select(column =>
                $"{column.Name} ({GetOpenApiTypeName(CreateColumnSchema(column))})"
            )
        );
        var columnNameSchema = new OpenApiSchema
        {
            Type = "string",
            Description = string.Format(Resources.ModeledApiColumnDescription, entity.Name),
            Enum = columns.Select(column => (IOpenApiAny)new OpenApiString(column.Name)).ToList(),
        };
        var orderingSchema = new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "columnId", "direction" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["columnId"] = columnNameSchema,
                ["direction"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("ASC"),
                        new OpenApiString("DESC"),
                    },
                },
                ["lookupId"] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid",
                    Nullable = true,
                },
            },
        };
        string exampleColumn =
            columns.FirstOrDefault()?.Name ?? Resources.ModeledApiExampleColumnName;
        return new OpenApiSchema
        {
            Type = "object",
            Description = string.Format(
                Resources.ModeledApiFilterInputDescription,
                availableColumns
            ),
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["filter"] = new OpenApiSchema
                {
                    Type = "string",
                    Description = Resources.ModeledApiFilterExpressionDescription,
                    Example = new OpenApiString($"[\"{exampleColumn}\",\"eq\",null]"),
                },
                ["filterLookups"] = new OpenApiSchema
                {
                    Type = "object",
                    Description = Resources.ModeledApiFilterLookupsDescription,
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = new OpenApiSchema { Type = "string", Format = "uuid" },
                },
                ["ordering"] = new OpenApiSchema
                {
                    Type = "array",
                    Description = Resources.ModeledApiOrderingDescription,
                    Items = orderingSchema,
                },
            },
        };
    }

    private static string GetOpenApiTypeName(OpenApiSchema schema)
    {
        return string.IsNullOrWhiteSpace(schema.Format)
            ? schema.Type
            : $"{schema.Type}/{schema.Format}";
    }

    public OpenApiSchema CreateDataStructureSchema(
        DataStructure dataStructure,
        bool omitRootElement,
        bool omitMainElement
    )
    {
        var rootEntities = dataStructure
            .ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
            .ToList();
        var mainProperties = rootEntities.ToDictionary(
            entity => entity.Name,
            CreateEntityCollectionSchema
        );
        OpenApiSchema mainSchema = new() { Type = "object", Properties = mainProperties };

        if (omitMainElement && rootEntities.Count == 1)
        {
            mainSchema = CreateEntityCollectionSchema(rootEntities[0]);
        }
        if (omitRootElement)
        {
            return mainSchema;
        }

        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema> { ["ROOT"] = mainSchema },
        };
    }

    private static OpenApiSchema CreateEntityCollectionSchema(DataStructureEntity entity)
    {
        OpenApiSchema rowSchema = CreateEntityRowSchema(entity);
        return entity.SerializeAsSingleJsonObject
            ? rowSchema
            : new OpenApiSchema { Type = "array", Items = rowSchema };
    }

    private static OpenApiSchema CreateEntityRowSchema(DataStructureEntity entity)
    {
        var properties = entity
            .Columns.Where(column => !column.HideInOutput && !column.IsWriteOnly)
            .ToDictionary(column => column.Name, CreateColumnSchema);

        foreach (
            DataStructureEntity childEntity in entity.ChildItemsByType<DataStructureEntity>(
                DataStructureEntity.CategoryConst
            )
        )
        {
            if (childEntity.Entity is IAssociation { IsParentChild: true })
            {
                properties[childEntity.Name] = CreateEntityCollectionSchema(childEntity);
            }
        }

        return new OpenApiSchema { Type = "object", Properties = properties };
    }

    private static OpenApiSchema CreateColumnSchema(DataStructureColumn column)
    {
        var schema = CreateDataTypeSchema(column.DataType);
        schema.Nullable = column.Field.AllowNulls;
        if (
            schema.Type == "string"
            && column.Field.DataLength > 0
            && column.DataType is not OrigamDataType.Memo and not OrigamDataType.Xml
        )
        {
            schema.MaxLength = column.Field.DataLength;
        }
        return schema;
    }
}
