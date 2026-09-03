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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Origam.Server.Configuration;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Server.OpenApi;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services,
        StartUpConfiguration startUpConfiguration
    )
    {
        var extensionControllerPolicy = new ExtensionControllerOpenApiPolicy(startUpConfiguration);
        services.AddSingleton(extensionControllerPolicy);
        services.AddSingleton(_ => ServiceManager.Services.GetService<ISchemaService>());
        services.AddSingleton(_ => ServiceManager.Services.GetService<IDocumentationService>());
        services.AddSingleton<ModeledOpenApiPagePolicy>();
        services.AddSingleton<ModeledOpenApiSchemaFactory>();
        services.AddSingleton<ModeledOpenApiExampleFactory>();
        services.AddSingleton<ModeledOpenApiPageDocumenter>();
        services.AddSingleton<ModeledOpenApiDocumentGenerator>();
        services.AddSingleton<OpenApiDocumentProvider>();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                OpenApiDocumentProvider.DocumentName,
                new OpenApiInfo
                {
                    Title = Resources.OpenApiTitle,
                    Version = "1.0",
                    Description = Resources.OpenApiDescription,
                }
            );
            options.DocInclusionPredicate(
                (_, apiDescription) => extensionControllerPolicy.IsDocumented(apiDescription)
            );
            options.TagActionsBy(apiDescription =>
                [extensionControllerPolicy.GetTagName(apiDescription)]
            );
            options.CustomSchemaIds(type => type.FullName ?? type.Name);
            options.DocumentFilter<ModeledOpenApiDocumentFilter>();
        });
        return services;
    }
}
