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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi;
using NUnit.Framework;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Server.Configuration;
using Origam.Server.OpenApi;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.ServerTests.OpenApi;

public class ModeledOpenApiDocumentGeneratorCompositionTests
{
    [Test]
    public void GeneratorAddsModeledApiToExistingDocument()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
                    ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
                    ["UserApiOptions:PublicRoutes:0"] = "reports",
                }
            )
            .Build();
        var persistenceProvider = new NullPersistenceProvider();
        var pageProvider = new TestPagesSchemaItemProvider(
            new ReportPage
            {
                Name = "Sales report",
                Url = "reports/sales",
                Roles = "*",
                MimeType = "application/json",
                PersistenceProvider = persistenceProvider,
            }
        );
        pageProvider.PersistenceProvider = persistenceProvider;
        var schemaService = new TestSchemaService(pageProvider);
        var pagePolicy = new ModeledOpenApiPagePolicy(new StartUpConfiguration(configuration));
        var pageDocumenter = new ModeledOpenApiPageDocumenter(
            pagePolicy,
            new ModeledOpenApiSchemaFactory(),
            new ModeledOpenApiExampleFactory(
                documentationService: null,
                NullLogger<ModeledOpenApiExampleFactory>.Instance
            )
        );
        var generator = new ModeledOpenApiDocumentGenerator(
            new OpenIddictConfig(configuration),
            schemaService,
            pagePolicy,
            pageDocumenter
        );
        var document = new OpenApiDocument
        {
            Info = new OpenApiInfo { Title = "Combined API", Version = "1.0" },
            Paths = new OpenApiPaths(),
        };

        generator.AddTo(document);

        Assert.That(document.Info.Title, Is.EqualTo("Combined API"));
        Assert.That(document.Paths.ContainsKey("/reports/sales"), Is.True);
        OpenApiOperation operation = document.Paths["/reports/sales"].Operations![HttpMethod.Get];
        Assert.That(operation.OperationId, Is.EqualTo("Sales_report_get"));
        Assert.That(operation.Summary, Is.EqualTo("Sales report"));
        Assert.That(operation.Security, Is.Null.Or.Empty);
        IOpenApiSecurityScheme authenticationScheme = document.Components!.SecuritySchemes![
            ModeledOpenApiPageDocumenter.AuthenticationSchemeName
        ];
        Assert.That(authenticationScheme.Type, Is.EqualTo(SecuritySchemeType.Http));
        Assert.That(authenticationScheme.Scheme, Is.EqualTo("bearer"));
    }

    private sealed class TestPagesSchemaItemProvider(params AbstractPage[] pages)
        : PagesSchemaItemProvider
    {
        public override List<T> ChildItemsByType<T>(string itemType)
        {
            return pages.OfType<T>().ToList();
        }
    }

    private sealed class TestSchemaService(PagesSchemaItemProvider pageProvider) : ISchemaService
    {
        public Guid ActiveSchemaExtensionId => Guid.Empty;
        public Guid StorageSchemaExtensionId { get; set; }
        public Package ActiveExtension => null!;
        public bool IsSchemaLoaded => true;

        public ISchemaItemProvider GetProvider(Type type)
        {
            return pageProvider;
        }

        public T GetProvider<T>()
            where T : ISchemaItemProvider
        {
            return (T)(ISchemaItemProvider)pageProvider;
        }

        public bool UnloadSchema()
        {
            return true;
        }

        public void InitializeService() { }

        public void UnloadService() { }
    }
}
