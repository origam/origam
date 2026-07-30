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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OpenApi;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiPageDocumenter(
    ModeledOpenApiPagePolicy pagePolicy,
    ModeledOpenApiSchemaFactory schemaFactory,
    ModeledOpenApiExampleFactory exampleFactory
)
{
    public const string AuthenticationSchemeName = "OrigamAuthentication";

    public void AddPage(OpenApiDocument document, AbstractPage page)
    {
        string path = "/" + page.Url;
        if (!document.Paths.TryGetValue(path, out IOpenApiPathItem existingPathItem))
        {
            existingPathItem = new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>(),
            };
            document.Paths.Add(path, existingPathItem);
        }
        var pathItem = (OpenApiPathItem)existingPathItem;

        switch (page)
        {
            case XsltDataPage dataPage:
            {
                if (dataPage.AllowCustomFilters)
                {
                    AddOperation(document, pathItem, HttpMethod.Post, page);
                }
                else
                {
                    AddOperation(document, pathItem, HttpMethod.Get, page);
                }
                break;
            }

            case WorkflowPage:
            {
                AddOperation(document, pathItem, HttpMethod.Post, page);
                break;
            }

            case ReportPage:
            case FileDownloadPage:
            {
                AddOperation(document, pathItem, HttpMethod.Get, page);
                break;
            }

            default:
            {
                AddUnsupportedPageOperation(document, pathItem, page);
                break;
            }
        }
        if (page.AllowPUT)
        {
            AddOperation(document, pathItem, HttpMethod.Put, page);
        }
        if (page.AllowDELETE)
        {
            AddOperation(document, pathItem, HttpMethod.Delete, page);
        }
    }

    private void AddUnsupportedPageOperation(
        OpenApiDocument document,
        OpenApiPathItem pathItem,
        AbstractPage page
    )
    {
        pathItem.Operations.Add(
            HttpMethod.Get,
            new OpenApiOperation
            {
                OperationId = $"{SanitizeOperationId(page.Name)}_unsupported",
                Summary = string.Format(Resources.ModeledApiUnsupportedPageTypeSummary, page.Name),
                Description = string.Format(
                    Resources.ModeledApiUnsupportedPageTypeDescription,
                    page.GetType().FullName
                ),
                Deprecated = true,
                Tags = new HashSet<OpenApiTagReference>
                {
                    new(pagePolicy.GetTagName(page), document, externalResource: null),
                },
                Responses = new OpenApiResponses
                {
                    ["501"] = new OpenApiResponse
                    {
                        Description = Resources.ModeledApiUnsupportedPageTypeResponse,
                    },
                },
            }
        );
    }

    private void AddOperation(
        OpenApiDocument document,
        OpenApiPathItem pathItem,
        HttpMethod operationType,
        AbstractPage page
    )
    {
        if (pathItem.Operations.ContainsKey(operationType))
        {
            throw new OrigamException(
                string.Format(Resources.ErrorModeledApiDuplicateOperation, operationType, page.Url)
            );
        }

        var operation = new OpenApiOperation
        {
            OperationId =
                $"{SanitizeOperationId(page.Name)}_{operationType.ToString().ToLowerInvariant()}",
            Summary = page.Name,
            Description = string.Format(
                Resources.ModeledApiEndpointDescription,
                page.GetType().Name
            ),
            Tags = new HashSet<OpenApiTagReference>
            {
                new(pagePolicy.GetTagName(page), document, externalResource: null),
            },
            Parameters = schemaFactory.CreateParameters(page),
            Responses = CreateResponses(page),
        };
        if (operationType == HttpMethod.Get && page is XsltDataPage { AllowCustomFilters: true })
        {
            operation.Description += Resources.ModeledApiPostFiltersHint;
        }

        OpenApiRequestBody requestBody = CreateRequestBody(page, operationType);
        if (requestBody != null)
        {
            operation.RequestBody = requestBody;
        }

        if (pagePolicy.RequiresAuthentication(page))
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [
                        new OpenApiSecuritySchemeReference(
                            AuthenticationSchemeName,
                            document,
                            externalResource: null
                        )
                    ] = new List<string>(),
                },
            };
        }

        pathItem.Operations.Add(operationType, operation);
    }

    private OpenApiRequestBody CreateRequestBody(AbstractPage page, HttpMethod operationType)
    {
        if (operationType == HttpMethod.Get || operationType == HttpMethod.Delete)
        {
            return null;
        }

        var mappings = page.ChildItemsByType<PageParameterMapping>(
                PageParameterMapping.CategoryConst
            )
            .ToList();
        IDictionary<string, OpenApiMediaType> documentedExamples =
            page is WorkflowPage ? exampleFactory.CreateDocumentedExampleContent(page) : null;
        var fileMappings = mappings.OfType<PageParameterFileMapping>().ToList();
        if (fileMappings.Count > 0 && operationType == HttpMethod.Post)
        {
            var properties = new Dictionary<string, IOpenApiSchema>();
            foreach (PageParameterFileMapping mapping in fileMappings)
            {
                properties[mapping.MappedParameter] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "binary",
                };
            }
            var multipartContent = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new()
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = properties,
                    },
                },
            };
            ModeledOpenApiExampleFactory.AddExamples(multipartContent, documentedExamples);
            return new OpenApiRequestBody { Content = multipartContent };
        }

        bool hasContentMapping = mappings.Any(mapping =>
            string.IsNullOrWhiteSpace(mapping.MappedParameter)
        );
        bool acceptsCustomFilters =
            page is XsltDataPage { AllowCustomFilters: true } && operationType == HttpMethod.Post;
        if (!hasContentMapping && !acceptsCustomFilters && documentedExamples == null)
        {
            return null;
        }

        OpenApiSchema schema;
        if (acceptsCustomFilters)
        {
            schema = schemaFactory.CreateCustomFilterSchema((XsltDataPage)page);
        }
        else if (
            operationType == HttpMethod.Put
            && page is XsltDataPage dataPage
            && dataPage.DataStructure != null
        )
        {
            schema = schemaFactory.CreateDataStructureSchema(
                dataPage.DataStructure,
                dataPage.OmitJsonRootElement,
                dataPage.OmitJsonMainElement
            );
        }
        else
        {
            schema = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                AdditionalPropertiesAllowed = true,
            };
        }

        var content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new() { Schema = schema },
        };
        if (!acceptsCustomFilters)
        {
            content["text/xml"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            };
        }

        ModeledOpenApiExampleFactory.AddExamples(content, documentedExamples);
        return new OpenApiRequestBody { Required = false, Content = content };
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
            ["404"] = new OpenApiResponse { Description = Resources.ModeledApiNotFoundResponse },
            ["500"] = new OpenApiResponse { Description = Resources.ModeledApiFailedResponse },
        };

        if (pagePolicy.RequiresAuthentication(page))
        {
            responses["401"] = new OpenApiResponse
            {
                Description = Resources.ModeledApiAuthenticationRequiredResponse,
            };
            responses["403"] = new OpenApiResponse
            {
                Description = Resources.ModeledApiAccessDeniedResponse,
            };
        }

        return responses;
    }

    private string GetSuccessfulResponseDescription(AbstractPage page)
    {
        if (page is WorkflowPage)
        {
            return Resources.ModeledApiSuccessfulResponse;
        }

        string invalidExampleMessage = exampleFactory.GetInvalidExampleMessage(page);
        if (invalidExampleMessage != null)
        {
            return Resources.ModeledApiSuccessfulResponse + " " + invalidExampleMessage;
        }

        if (
            page is XsltDataPage { Transformation: not null }
            && exampleFactory.CreateDocumentedExampleContent(page) == null
        )
        {
            return Resources.ModeledApiMissingOutputDocumentationResponse;
        }

        if (page.MimeType != "application/json")
        {
            return Resources.ModeledApiSuccessfulResponse;
        }

        if (page is not XsltDataPage)
        {
            return Resources.ModeledApiUnknownJsonResponse;
        }

        return Resources.ModeledApiSuccessfulResponse;
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
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" },
                },
            };
        }

        string mimeType = string.IsNullOrWhiteSpace(page.MimeType) ? "text/plain" : page.MimeType;
        OpenApiSchema responseSchema = new() { Type = JsonSchemaType.String };
        if (page is XsltDataPage { Transformation: not null })
        {
            return exampleFactory.CreateDocumentedExampleContent(page);
        }
        if (page is XsltDataPage xsltPage && mimeType == "application/json")
        {
            DataStructure dataStructure = xsltPage.DataStructure;
            if (dataStructure == null)
            {
                return new Dictionary<string, OpenApiMediaType> { [mimeType] = new() };
            }
            responseSchema = schemaFactory.CreateDataStructureSchema(
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

    private static string SanitizeOperationId(string name)
    {
        return new string(
            name.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray()
        );
    }
}
