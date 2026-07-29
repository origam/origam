using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Origam.Schema.GuiModel;
using Origam.Server.Configuration;
using Origam.Server.OpenApi;
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
                }
            )
            .Build();
        var schemaService = new SchemaService();
        ServiceManager.Services.AddService(new NullPersistenceService());
        schemaService.AddProvider(
            new PagesSchemaItemProvider { PersistenceProvider = new NullPersistenceProvider() }
        );
        var generator = new ModeledOpenApiDocumentGenerator(
            new StartUpConfiguration(configuration),
            new OpenIddictConfig(configuration),
            schemaService,
            documentationService: null
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
        Assert.Empty(root.GetProperty("paths").EnumerateObject());
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
}
