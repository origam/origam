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
using Microsoft.OpenApi;
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
    public void AddTo(OpenApiDocument document)
    {
        var pageProvider = schemaService.GetProvider<PagesSchemaItemProvider>();
        var pages = pageProvider
            .ChildItems.OfType<AbstractPage>()
            .Where(pagePolicy.IsDocumented)
            .OrderBy(pagePolicy.GetTagName)
            .ThenBy(page => page.Url)
            .ThenBy(page => page.Name)
            .ToList();

        document.Tags ??= new HashSet<OpenApiTag>();
        document.Tags.UnionWith(
            pages
                .Select(pagePolicy.GetTagName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(tagName => new OpenApiTag
                {
                    Name = tagName,
                    Description = string.Format(Resources.ModeledApiTagDescription, tagName),
                })
                .ToHashSet()
        );

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ModeledOpenApiPageDocumenter.AuthenticationSchemeName] =
            CreateAuthenticationScheme();

        foreach (AbstractPage page in pages)
        {
            pageDocumenter.AddPage(document, page);
        }
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
