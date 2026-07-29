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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Configuration;
using Origam.Workbench.Services;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiDocumentGenerator(
    StartUpConfiguration startUpConfiguration,
    OpenIddictConfig openIddictConfig,
    SchemaService schemaService,
    IDocumentationService documentationService
)
{
    private const string AuthenticationSchemeName = "OrigamAuthentication";

    public string Generate()
    {
        var pageProvider = schemaService.GetProvider<PagesSchemaItemProvider>();
        var document = CreateDocument();
        var pages = pageProvider
            .ChildItems.OfType<AbstractPage>()
            .Where(IsDocumentedPage)
            .OrderBy(GetFolderName)
            .ThenBy(page => page.Url)
            .ThenBy(page => page.Name)
            .ToList();

        document.Tags = pages
            .Select(GetFolderName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(folderName => new OpenApiTag
            {
                Name = folderName,
                Description = $"Modeled API endpoints in the '{folderName}' folder.",
            })
            .ToList();

        foreach (AbstractPage page in pages)
        {
            AddPage(document, page);
        }

        using var stringWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(stringWriter);
        document.SerializeAsV3(writer);
        writer.Flush();
        return stringWriter.ToString();
    }

    private OpenApiDocument CreateDocument()
    {
        return new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = "Origam Modeled API",
                Version = "1.0",
                Description = "API endpoints defined in the active Origam application model.",
            },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    [AuthenticationSchemeName] = CreateAuthenticationScheme(),
                },
            },
        };
    }

    private OpenApiSecurityScheme CreateAuthenticationScheme()
    {
        if (openIddictConfig.PrivateApiAuthentication == AuthenticationMethod.Token)
        {
            return new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Origam access token.",
            };
        }

        return new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Name = ".AspNetCore.Identity.Application",
            Description = "Origam authentication cookie.",
        };
    }

    private bool IsDocumentedPage(AbstractPage page)
    {
        return !page.IsAbstract
            && IsRuntimeCompatibleUrl(page.Url)
            && (
                IsRouteIn(page.Url, startUpConfiguration.UserApiPublicRoutes)
                || IsRouteIn(page.Url, startUpConfiguration.UserApiRestrictedRoutes)
            );
    }

    private void AddPage(OpenApiDocument document, AbstractPage page)
    {
        string path = "/" + page.Url;
        if (!document.Paths.TryGetValue(path, out OpenApiPathItem pathItem))
        {
            pathItem = new OpenApiPathItem();
            document.Paths.Add(path, pathItem);
        }

        switch (page)
        {
            case XsltDataPage dataPage:
            {
                if (dataPage.AllowCustomFilters)
                {
                    AddOperation(pathItem, OperationType.Post, page);
                }
                else
                {
                    AddOperation(pathItem, OperationType.Get, page);
                }
                break;
            }

            case WorkflowPage:
            {
                AddOperation(pathItem, OperationType.Post, page);
                break;
            }

            case ReportPage:
            case FileDownloadPage:
            {
                AddOperation(pathItem, OperationType.Get, page);
                break;
            }

            default:
            {
                AddUnsupportedPageOperation(pathItem, page);
                break;
            }
        }
        if (page.AllowPUT)
        {
            AddOperation(pathItem, OperationType.Put, page);
        }
        if (page.AllowDELETE)
        {
            AddOperation(pathItem, OperationType.Delete, page);
        }
    }

    private static void AddUnsupportedPageOperation(OpenApiPathItem pathItem, AbstractPage page)
    {
        pathItem.Operations.Add(
            OperationType.Get,
            new OpenApiOperation
            {
                OperationId = $"{SanitizeOperationId(page.Name)}_unsupported",
                Summary = $"Unsupported page type: {page.Name}",
                Description =
                    $"OpenAPI documentation generation is not implemented for "
                    + $"the modeled page type '{page.GetType().FullName}'.",
                Deprecated = true,
                Tags = new List<OpenApiTag> { new() { Name = GetFolderName(page) } },
                Responses = new OpenApiResponses
                {
                    ["501"] = new OpenApiResponse
                    {
                        Description =
                            "OpenAPI documentation generation is not implemented "
                            + "for this modeled page type.",
                    },
                },
            }
        );
    }

    private void AddOperation(
        OpenApiPathItem pathItem,
        OperationType operationType,
        AbstractPage page
    )
    {
        if (pathItem.Operations.ContainsKey(operationType))
        {
            throw new OrigamException(
                $"The modeled API contains more than one {operationType} operation for '{page.Url}'."
            );
        }

        var operation = new OpenApiOperation
        {
            OperationId =
                $"{SanitizeOperationId(page.Name)}_{operationType.ToString().ToLowerInvariant()}",
            Summary = page.Name,
            Description = $"Modeled {page.GetType().Name} endpoint.",
            Tags = new List<OpenApiTag> { new() { Name = GetFolderName(page) } },
            Parameters = CreateParameters(page),
            Responses = CreateResponses(page),
        };
        if (operationType == OperationType.Get && page is XsltDataPage { AllowCustomFilters: true })
        {
            operation.Description +=
                " Use the POST operation on the same path to send custom filters and ordering.";
        }

        OpenApiRequestBody requestBody = CreateRequestBody(page, operationType);
        if (requestBody != null)
        {
            operation.RequestBody = requestBody;
        }

        if (RequiresAuthentication(page))
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = AuthenticationSchemeName,
                            },
                        }
                    ] = Array.Empty<string>(),
                },
            };
        }

        pathItem.Operations.Add(operationType, operation);
    }

    private static IList<OpenApiParameter> CreateParameters(AbstractPage page)
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
                            ? $"Maps to the modeled parameter '{modeledParameterNames[0]}'."
                            : "Maps to the modeled parameters "
                                + string.Join(
                                    separator: ", ",
                                    values: modeledParameterNames.Select(name => $"'{name}'")
                                )
                                + ".",
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

    private static OpenApiRequestBody CreateRequestBody(
        AbstractPage page,
        OperationType operationType
    )
    {
        if (operationType is OperationType.Get or OperationType.Delete)
        {
            return null;
        }

        var mappings = page.ChildItemsByType<PageParameterMapping>(
                PageParameterMapping.CategoryConst
            )
            .ToList();
        IDictionary<string, OpenApiMediaType> documentedExamples =
            page is WorkflowPage ? CreateDocumentedExampleContent(page) : null;
        var fileMappings = mappings.OfType<PageParameterFileMapping>().ToList();
        if (fileMappings.Count > 0 && operationType == OperationType.Post)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            foreach (PageParameterFileMapping mapping in fileMappings)
            {
                properties[mapping.MappedParameter] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                };
            }
            var multipartContent = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new()
                {
                    Schema = new OpenApiSchema { Type = "object", Properties = properties },
                },
            };
            AddExamples(multipartContent, documentedExamples);
            return new OpenApiRequestBody { Content = multipartContent };
        }

        bool hasContentMapping = mappings.Any(mapping =>
            string.IsNullOrWhiteSpace(mapping.MappedParameter)
        );
        bool acceptsCustomFilters =
            page is XsltDataPage { AllowCustomFilters: true }
            && operationType == OperationType.Post;
        if (!hasContentMapping && !acceptsCustomFilters && documentedExamples == null)
        {
            return null;
        }

        OpenApiSchema schema;
        if (acceptsCustomFilters)
        {
            schema = CreateCustomFilterSchema((XsltDataPage)page);
        }
        else if (
            operationType == OperationType.Put
            && page is XsltDataPage dataPage
            && dataPage.DataStructure != null
        )
        {
            schema = CreateDataStructureSchema(
                dataPage.DataStructure,
                dataPage.OmitJsonRootElement,
                dataPage.OmitJsonMainElement
            );
        }
        else
        {
            schema = new OpenApiSchema { Type = "object", AdditionalPropertiesAllowed = true };
        }

        var content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new() { Schema = schema },
        };
        if (!acceptsCustomFilters)
        {
            content["text/xml"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema { Type = "string" },
            };
        }

        AddExamples(content, documentedExamples);
        return new OpenApiRequestBody { Required = false, Content = content };
    }

    private static OpenApiSchema CreateCustomFilterSchema(XsltDataPage page)
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
            Description = $"A column in '{entity.Name}'.",
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
        string exampleColumn = columns.FirstOrDefault()?.Name ?? "ColumnName";
        return new OpenApiSchema
        {
            Type = "object",
            Description =
                "Custom filtering and ordering input. Available columns: " + availableColumns + ".",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["filter"] = new OpenApiSchema
                {
                    Type = "string",
                    Description =
                        "Origam filter expression. A condition is [\"column\", \"operator\", value]. "
                        + "Combine conditions with [\"$AND\", ...] or [\"$OR\", ...]. "
                        + "Operators include eq, neq, gt, gte, lt, lte, starts, nstarts, ends, "
                        + "nends, contains, ncontains, like, in, nin, between and nbetween.",
                    Example = new OpenApiString($"[\"{exampleColumn}\",\"eq\",null]"),
                },
                ["filterLookups"] = new OpenApiSchema
                {
                    Type = "object",
                    Description =
                        "Optional map from a filter column name to the UUID of the lookup used "
                        + "to resolve that column.",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = new OpenApiSchema { Type = "string", Format = "uuid" },
                },
                ["ordering"] = new OpenApiSchema
                {
                    Type = "array",
                    Description = "Optional custom ordering. Array order determines sort priority.",
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

    private OpenApiResponses CreateResponses(AbstractPage page)
    {
        var responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse
            {
                Description = GetSuccessfulResponseDescription(page),
                Content = CreateResponseContent(page),
            },
            ["404"] = new OpenApiResponse { Description = "Modeled endpoint not found." },
            ["500"] = new OpenApiResponse { Description = "The modeled endpoint failed." },
        };

        if (RequiresAuthentication(page))
        {
            responses["401"] = new OpenApiResponse { Description = "Authentication required." };
            responses["403"] = new OpenApiResponse { Description = "Access denied." };
        }

        return responses;
    }

    private static string GetSuccessfulResponseDescription(AbstractPage page)
    {
        if (page is WorkflowPage)
        {
            return "Successful response.";
        }

        string invalidExampleMessage = GetInvalidExampleMessage(page);
        if (invalidExampleMessage != null)
        {
            return "Successful response. " + invalidExampleMessage;
        }

        if (
            page is XsltDataPage { Transformation: not null }
            && CreateDocumentedExampleContent(page) == null
        )
        {
            return "Successful response. The response schema is not available because "
                + "the output documentation is not filled out. "
                + "Add an EXAMPLE_JSON, EXAMPLE_XML or EXAMPLE documentation entry to expose "
                + "an example response here.";
        }

        if (page.MimeType != "application/json")
        {
            return "Successful response.";
        }

        if (page is not XsltDataPage)
        {
            return "Successful response. This endpoint returns JSON data, but its response "
                + "schema cannot be inferred from the Origam model.";
        }

        return "Successful response.";
    }

    private string GetInvalidExampleMessage(AbstractPage page)
    {
        if (page is not XsltDataPage { Transformation: not null })
        {
            return null;
        }

        if (documentationService == null)
        {
            return null;
        }

        string jsonExample = documentationService.GetDocumentation(
            page.Id,
            DocumentationType.EXAMPLE_JSON
        );
        if (
            !string.IsNullOrWhiteSpace(jsonExample)
            && !TryCreateJsonExample(jsonExample, out IOpenApiAny _)
        )
        {
            return "The EXAMPLE_JSON output documentation entry is not valid JSON "
                + "and cannot be displayed.";
        }

        string xmlExample = documentationService.GetDocumentation(
            page.Id,
            DocumentationType.EXAMPLE_XML
        );
        if (!string.IsNullOrWhiteSpace(xmlExample) && !IsXml(xmlExample))
        {
            return "The EXAMPLE_XML output documentation entry is not valid XML "
                + "and cannot be displayed.";
        }

        return null;
    }

    private IDictionary<string, OpenApiMediaType> CreateResponseContent(AbstractPage page)
    {
        if (page is WorkflowPage)
        {
            return null;
        }

        if (page is FileDownloadPage)
        {
            string contentType = page.MimeType == "?" ? "application/octet-stream" : page.MimeType;
            return new Dictionary<string, OpenApiMediaType>
            {
                [contentType] = new()
                {
                    Schema = new OpenApiSchema { Type = "string", Format = "binary" },
                },
            };
        }

        string mimeType = string.IsNullOrWhiteSpace(page.MimeType) ? "text/plain" : page.MimeType;
        OpenApiSchema responseSchema = new() { Type = "string" };
        if (page is XsltDataPage { Transformation: not null })
        {
            return CreateDocumentedExampleContent(page);
        }
        if (page is XsltDataPage xsltPage && mimeType == "application/json")
        {
            DataStructure dataStructure = xsltPage.DataStructure;
            if (dataStructure == null)
            {
                return new Dictionary<string, OpenApiMediaType> { [mimeType] = new() };
            }
            responseSchema = CreateDataStructureSchema(
                dataStructure,
                xsltPage.OmitJsonRootElement,
                xsltPage.OmitJsonMainElement
            );
        }
        else if (mimeType == "application/json")
        {
            return new Dictionary<string, OpenApiMediaType> { [mimeType] = new() };
        }

        return new Dictionary<string, OpenApiMediaType>
        {
            [mimeType] = new() { Schema = responseSchema },
        };
    }

    private IDictionary<string, OpenApiMediaType> CreateDocumentedExampleContent(AbstractPage page)
    {
        if (documentationService == null)
        {
            return null;
        }

        var content = new Dictionary<string, OpenApiMediaType>();
        AddDocumentedExample(
            content: content,
            mimeType: "application/json",
            example: documentationService.GetDocumentation(page.Id, DocumentationType.EXAMPLE_JSON),
            isJson: true
        );
        AddDocumentedExample(
            content: content,
            mimeType: "text/xml",
            example: documentationService.GetDocumentation(page.Id, DocumentationType.EXAMPLE_XML),
            isJson: false
        );

        string example = documentationService.GetDocumentation(page.Id, DocumentationType.EXAMPLE);
        if (!string.IsNullOrWhiteSpace(example))
        {
            if (TryCreateJsonExample(example, out IOpenApiAny jsonExample))
            {
                AddExample(content: content, mimeType: "application/json", example: jsonExample);
            }
            else if (IsXml(example))
            {
                AddExample(
                    content: content,
                    mimeType: "text/xml",
                    example: new OpenApiString(example)
                );
            }
        }

        return content.Count == 0 ? null : content;
    }

    private static void AddDocumentedExample(
        IDictionary<string, OpenApiMediaType> content,
        string mimeType,
        string example,
        bool isJson
    )
    {
        if (string.IsNullOrWhiteSpace(example))
        {
            return;
        }

        IOpenApiAny openApiExample;
        if (isJson)
        {
            if (!TryCreateJsonExample(example, out openApiExample))
            {
                return;
            }
        }
        else
        {
            if (!IsXml(example))
            {
                return;
            }
            openApiExample = new OpenApiString(example);
        }
        AddExample(content, mimeType, openApiExample);
    }

    private static void AddExample(
        IDictionary<string, OpenApiMediaType> content,
        string mimeType,
        IOpenApiAny example
    )
    {
        if (!content.TryGetValue(mimeType, out OpenApiMediaType mediaType))
        {
            mediaType = new OpenApiMediaType();
            content[mimeType] = mediaType;
        }
        mediaType.Example ??= example;
    }

    private static void AddExamples(
        IDictionary<string, OpenApiMediaType> content,
        IDictionary<string, OpenApiMediaType> documentedExamples
    )
    {
        if (documentedExamples == null)
        {
            return;
        }
        foreach ((string mimeType, OpenApiMediaType documentedMediaType) in documentedExamples)
        {
            AddExample(content, mimeType, documentedMediaType.Example);
        }
    }

    private static bool TryCreateJsonExample(string value, out IOpenApiAny example)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(value);
            example = CreateOpenApiValue(document.RootElement);
            return true;
        }
        catch (JsonException)
        {
            example = null;
            return false;
        }
    }

    private static IOpenApiAny CreateOpenApiValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => CreateOpenApiObject(element),
            JsonValueKind.Array => CreateOpenApiArray(element),
            JsonValueKind.String => new OpenApiString(element.GetString()),
            JsonValueKind.Number when element.TryGetInt32(out int intValue) => new OpenApiInteger(
                intValue
            ),
            JsonValueKind.Number when element.TryGetInt64(out long longValue) => new OpenApiLong(
                longValue
            ),
            JsonValueKind.Number => new OpenApiDouble(element.GetDouble()),
            JsonValueKind.True => new OpenApiBoolean(true),
            JsonValueKind.False => new OpenApiBoolean(false),
            _ => new OpenApiNull(),
        };
    }

    private static OpenApiObject CreateOpenApiObject(JsonElement element)
    {
        var result = new OpenApiObject();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            result[property.Name] = CreateOpenApiValue(property.Value);
        }
        return result;
    }

    private static OpenApiArray CreateOpenApiArray(JsonElement element)
    {
        var result = new OpenApiArray();
        foreach (JsonElement item in element.EnumerateArray())
        {
            result.Add(CreateOpenApiValue(item));
        }
        return result;
    }

    private static bool IsXml(string value)
    {
        try
        {
            XDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static OpenApiSchema CreateDataStructureSchema(
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

    private bool RequiresAuthentication(AbstractPage page)
    {
        return !string.Equals(a: page.Roles, b: "*", comparisonType: StringComparison.Ordinal)
            || IsRouteIn(page.Url, startUpConfiguration.UserApiRestrictedRoutes);
    }

    private static bool IsRouteIn(string pageUrl, IEnumerable<string> configuredRoutes)
    {
        string normalizedPageUrl = "/" + pageUrl.TrimStart('/');
        return configuredRoutes
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Any(route =>
                normalizedPageUrl.StartsWith(
                    "/" + route.TrimStart('/'),
                    StringComparison.OrdinalIgnoreCase
                )
            );
    }

    private static bool IsRuntimeCompatibleUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && !url.StartsWith(value: "/", comparisonType: StringComparison.Ordinal);
    }

    private static string GetFolderName(AbstractPage page)
    {
        return page.Group == null
            ? "Uncategorized"
            : string.Join(
                separator: " / ",
                values: page.Group.Path.Replace(oldValue: "\\", newValue: "/")
                    .Split(separator: '/', options: StringSplitOptions.RemoveEmptyEntries)
                    .AsEnumerable()
            );
    }

    private static string SanitizeOperationId(string name)
    {
        return new string(
            name.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray()
        );
    }
}
