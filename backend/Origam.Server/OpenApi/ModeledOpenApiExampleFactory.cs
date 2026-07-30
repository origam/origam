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
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
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
            && !TryCreateJsonExample(jsonExample, out JsonNode _)
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
            if (TryCreateJsonExample(example, out JsonNode jsonExample))
            {
                AddExample(content: content, mimeType: "application/json", example: jsonExample);
            }
            else if (IsXml(example))
            {
                AddExample(
                    content: content,
                    mimeType: "text/xml",
                    example: JsonValue.Create(example)
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

        JsonNode openApiExample;
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
            openApiExample = JsonValue.Create(example);
        }
        AddExample(content, mimeType, openApiExample);
    }

    private static void AddExample(
        IDictionary<string, OpenApiMediaType> content,
        string mimeType,
        JsonNode example
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

    private static bool TryCreateJsonExample(string value, out JsonNode example)
    {
        try
        {
            example = JsonNode.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            example = null;
            return false;
        }
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
                message: $"Could not validate the modeled API XML example \"{value}\""
            );
            return false;
        }
    }
}
