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
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiSchemaFactory
{
    public IList<IOpenApiParameter> CreateParameters(AbstractPage page)
    {
        var parameters = new List<IOpenApiParameter>();
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
                        ? new OpenApiSchema { Type = JsonSchemaType.Array, Items = itemSchema }
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
            : new OpenApiSchema { Type = JsonSchemaType.String };
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
            ? new OpenApiSchema { Type = JsonSchemaType.String }
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
            OrigamDataType.Boolean => new OpenApiSchema { Type = JsonSchemaType.Boolean },
            OrigamDataType.Byte => new OpenApiSchema
            {
                Type = JsonSchemaType.Integer,
                Format = "int32",
            },
            OrigamDataType.Currency => new OpenApiSchema
            {
                Type = JsonSchemaType.Number,
                Format = "double",
            },
            OrigamDataType.Date => new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "date-time",
            },
            OrigamDataType.Long => new OpenApiSchema
            {
                Type = JsonSchemaType.Integer,
                Format = "int64",
            },
            OrigamDataType.Float => new OpenApiSchema
            {
                Type = JsonSchemaType.Number,
                Format = "double",
            },
            OrigamDataType.Integer => new OpenApiSchema
            {
                Type = JsonSchemaType.Integer,
                Format = "int32",
            },
            OrigamDataType.UniqueIdentifier => new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid",
            },
            OrigamDataType.Blob => new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "byte",
            },
            OrigamDataType.Array => new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = new OpenApiSchema { Type = JsonSchemaType.String },
            },
            OrigamDataType.Object or OrigamDataType.Xml or OrigamDataType.Geography =>
                new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    AdditionalPropertiesAllowed = true,
                },
            _ => new OpenApiSchema { Type = JsonSchemaType.String },
        };
    }

    private static OpenApiParameter CreateIntegerQueryParameter(string name)
    {
        return new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
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
            Type = JsonSchemaType.String,
            Description = string.Format(Resources.ModeledApiColumnDescription, entity.Name),
            Enum = columns.Select(column => (JsonNode)column.Name).ToList(),
        };
        var orderingSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "columnId", "direction" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["columnId"] = columnNameSchema,
                ["direction"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { "ASC", "DESC" },
                },
                ["lookupId"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String | JsonSchemaType.Null,
                    Format = "uuid",
                },
            },
        };
        string exampleColumn =
            columns.FirstOrDefault()?.Name ?? Resources.ModeledApiExampleColumnName;
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Description = string.Format(
                Resources.ModeledApiFilterInputDescription,
                availableColumns
            ),
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["filter"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = Resources.ModeledApiFilterExpressionDescription,
                    Example = $"[\"{exampleColumn}\",\"eq\",null]",
                },
                ["filterLookups"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Description = Resources.ModeledApiFilterLookupsDescription,
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "uuid",
                    },
                },
                ["ordering"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Description = Resources.ModeledApiOrderingDescription,
                    Items = orderingSchema,
                },
            },
        };
    }

    private static string GetOpenApiTypeName(OpenApiSchema schema)
    {
        string typeName = (schema.Type & ~JsonSchemaType.Null).ToString().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(schema.Format) ? typeName : $"{typeName}/{schema.Format}";
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
        OpenApiSchema mainSchema = new()
        {
            Type = JsonSchemaType.Object,
            Properties = mainProperties.ToDictionary(
                pair => pair.Key,
                pair => (IOpenApiSchema)pair.Value
            ),
        };

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
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema> { ["ROOT"] = mainSchema },
        };
    }

    private static OpenApiSchema CreateEntityCollectionSchema(DataStructureEntity entity)
    {
        OpenApiSchema rowSchema = CreateEntityRowSchema(entity);
        return entity.SerializeAsSingleJsonObject
            ? rowSchema
            : new OpenApiSchema { Type = JsonSchemaType.Array, Items = rowSchema };
    }

    private static OpenApiSchema CreateEntityRowSchema(DataStructureEntity entity)
    {
        var properties = entity
            .Columns.Where(column => !column.HideInOutput && !column.IsWriteOnly)
            .ToDictionary(column => column.Name, CreateColumnSchema);

        foreach (
            DataStructureEntity childEntity in entity
                .ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
                .Where(childEntity => childEntity.Entity is IAssociation { IsParentChild: true })
        )
        {
            properties[childEntity.Name] = CreateEntityCollectionSchema(childEntity);
        }

        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = properties.ToDictionary(
                pair => pair.Key,
                pair => (IOpenApiSchema)pair.Value
            ),
        };
    }

    private static OpenApiSchema CreateColumnSchema(DataStructureColumn column)
    {
        var schema = CreateDataTypeSchema(column.DataType);
        if (column.Field.AllowNulls)
        {
            schema.Type |= JsonSchemaType.Null;
        }
        if (
            schema.Type.HasValue
            && schema.Type.Value.HasFlag(JsonSchemaType.String)
            && column.Field.DataLength > 0
            && column.DataType is not OrigamDataType.Memo and not OrigamDataType.Xml
        )
        {
            schema.MaxLength = column.Field.DataLength;
        }
        return schema;
    }
}
