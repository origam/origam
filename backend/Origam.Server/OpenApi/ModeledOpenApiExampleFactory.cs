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
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiExampleFactory(
    IDocumentationService documentationService,
    ILogger<ModeledOpenApiExampleFactory> logger
)
{
    public string GetInvalidExampleMessage(AbstractPage page)
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
            return Resources.ModeledApiInvalidJsonExample;
        }

        string xmlExample = documentationService.GetDocumentation(
            page.Id,
            DocumentationType.EXAMPLE_XML
        );
        if (!string.IsNullOrWhiteSpace(xmlExample) && !IsXml(xmlExample))
        {
            return Resources.ModeledApiInvalidXmlExample;
        }

        return null;
    }

    public IDictionary<string, OpenApiMediaType> CreateDocumentedExampleContent(AbstractPage page)
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

    private void AddDocumentedExample(
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

    public static void AddExamples(
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

    private bool IsXml(string value)
    {
        try
        {
            XDocument.Parse(value);
            return true;
        }
        catch (System.Xml.XmlException)
        {
            return false;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception: exception,
                message: "Could not validate the modeled API XML example."
            );
            return false;
        }
    }
}
