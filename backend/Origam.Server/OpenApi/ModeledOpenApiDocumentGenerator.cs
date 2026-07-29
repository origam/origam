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
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Origam.Schema.GuiModel;
using Origam.Server.Configuration;
using Origam.Services;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiDocumentGenerator(
    OpenIddictConfig openIddictConfig,
    ISchemaService schemaService,
    ModeledOpenApiPagePolicy pagePolicy,
    ModeledOpenApiPageDocumenter pageDocumenter
)
{
    public string Generate()
    {
        var pageProvider = schemaService.GetProvider<PagesSchemaItemProvider>();
        var document = CreateDocument();
        var pages = pageProvider
            .ChildItems.OfType<AbstractPage>()
            .Where(pagePolicy.IsDocumented)
            .OrderBy(pagePolicy.GetTagName)
            .ThenBy(page => page.Url)
            .ThenBy(page => page.Name)
            .ToList();

        document.Tags = pages
            .Select(pagePolicy.GetTagName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(tagName => new OpenApiTag
            {
                Name = tagName,
                Description = string.Format(Resources.ModeledApiTagDescription, tagName),
            })
            .ToList();

        foreach (AbstractPage page in pages)
        {
            pageDocumenter.AddPage(document, page);
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
                Title = Resources.ModeledApiTitle,
                Version = "1.0",
                Description = Resources.ModeledApiDescription,
            },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    [ModeledOpenApiPageDocumenter.AuthenticationSchemeName] =
                        CreateAuthenticationScheme(),
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
                Description = Resources.ModeledApiAccessTokenDescription,
            };
        }

        return new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Name = ".AspNetCore.Identity.Application",
            Description = Resources.ModeledApiAuthenticationCookieDescription,
        };
    }
}
