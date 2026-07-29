using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Server.Configuration;
using Origam.Server.OpenApi;
using Origam.Services;
using Origam.Workbench.Services;
using Xunit;

namespace Origam.ServerTests.OpenApi;

public class ModeledOpenApiDocumentProviderTests
{
    [Fact]
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
            new ModeledOpenApiExampleFactory(documentationService: null)
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

        Assert.Same(firstDocument, secondDocument);
        using JsonDocument document = JsonDocument.Parse(firstDocument);
        JsonElement root = document.RootElement;
        Assert.Equal(
            expected: "Origam Modeled API",
            actual: root.GetProperty("info").GetProperty("title").GetString()
        );
        Assert.Equal(
            expected: "1.0",
            actual: root.GetProperty("info").GetProperty("version").GetString()
        );
        JsonElement operation = root.GetProperty("paths")
            .GetProperty("/reports/sales")
            .GetProperty("get");
        Assert.Equal(
            expected: "Sales_report_get",
            actual: operation.GetProperty("operationId").GetString()
        );
        Assert.Equal(
            expected: "Sales report",
            actual: operation.GetProperty("summary").GetString()
        );
        Assert.False(operation.TryGetProperty(propertyName: "security", out JsonElement _));
        JsonElement authenticationScheme = root.GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("OrigamAuthentication");
        Assert.Equal(
            expected: "http",
            actual: authenticationScheme.GetProperty("type").GetString()
        );
        Assert.Equal(
            expected: "bearer",
            actual: authenticationScheme.GetProperty("scheme").GetString()
        );
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
