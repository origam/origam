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

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Server.Configuration;
using Origam.Server.OpenApi;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.ServerTests.OpenApi;

public class ModeledOpenApiDocumentProviderTests
{
    [Test]
    public void GetDocumentReturnsCachedModeledApiDocument()
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
        var provider = new ModeledOpenApiDocumentProvider(generator);

        string firstDocument = provider.GetDocument();
        string secondDocument = provider.GetDocument();

        Assert.That(secondDocument, Is.SameAs(firstDocument));
        using JsonDocument document = JsonDocument.Parse(firstDocument);
        JsonElement root = document.RootElement;
        Assert.That(
            root.GetProperty("info").GetProperty("title").GetString(),
            Is.EqualTo("Origam Modeled API")
        );
        Assert.That(root.GetProperty("info").GetProperty("version").GetString(), Is.EqualTo("1.0"));
        JsonElement operation = root.GetProperty("paths")
            .GetProperty("/reports/sales")
            .GetProperty("get");
        Assert.That(
            operation.GetProperty("operationId").GetString(),
            Is.EqualTo("Sales_report_get")
        );
        Assert.That(operation.GetProperty("summary").GetString(), Is.EqualTo("Sales report"));
        Assert.That(
            operation.TryGetProperty(propertyName: "security", out JsonElement _),
            Is.False
        );
        JsonElement authenticationScheme = root.GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("OrigamAuthentication");
        Assert.That(authenticationScheme.GetProperty("type").GetString(), Is.EqualTo("http"));
        Assert.That(authenticationScheme.GetProperty("scheme").GetString(), Is.EqualTo("bearer"));
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
