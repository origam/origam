using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Configuration;
using Origam.Server.OpenApi;
using Origam.Services;
using Origam.Workbench.Services;
using Xunit;

namespace Origam.ServerTests.OpenApi;

public class ModeledOpenApiDocumentGenerationTests
{
    [Fact]
    public void ConfiguredConcretePagesAreTheOnlyDocumentedPages()
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage<ReportPage>(name: "Public", url: "public/public-report");
        fixture.AddPage<ReportPage>(name: "Restricted", url: "restricted/restricted-report");
        fixture.AddPage<ReportPage>(name: "Unconfigured", url: "other/unconfigured-report");
        fixture.AddPage<ReportPage>(
            name: "Abstract",
            url: "public/abstract-report",
            configure: page => page.IsAbstract = true
        );

        JsonElement paths = fixture.Generate().RootElement.GetProperty("paths");

        Assert.Equal(expected: 2, actual: paths.EnumerateObject().Count());
        Assert.True(paths.TryGetProperty(propertyName: "/public/public-report", out _));
        Assert.True(paths.TryGetProperty(propertyName: "/restricted/restricted-report", out _));
        Assert.False(paths.TryGetProperty(propertyName: "/other/unconfigured-report", out _));
        Assert.False(paths.TryGetProperty(propertyName: "/public/abstract-report", out _));
    }

    [Theory]
    [InlineData(PageKind.Xslt, false, "get")]
    [InlineData(PageKind.Xslt, true, "post")]
    [InlineData(PageKind.Workflow, false, "post")]
    [InlineData(PageKind.Report, false, "get")]
    [InlineData(PageKind.Download, false, "get")]
    public void PageTypeDeterminesOperation(
        PageKind pageKind,
        bool allowCustomFilters,
        string expectedOperation
    )
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage(
            kind: pageKind,
            name: "Operation",
            url: "public/operation",
            allowCustomFilters: allowCustomFilters
        );

        JsonElement path = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/public/operation");

        Assert.True(path.TryGetProperty(expectedOperation, out JsonElement _));
        Assert.Single(path.EnumerateObject());
    }

    [Fact]
    public void UpdateAndDeleteFlagsAddOperations()
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage<ReportPage>(
            name: "Mutable",
            url: "public/mutable",
            configure: page =>
            {
                page.AllowPUT = true;
                page.AllowDELETE = true;
            }
        );

        JsonElement path = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/public/mutable");

        Assert.True(path.TryGetProperty(propertyName: "get", out _));
        Assert.True(path.TryGetProperty(propertyName: "put", out _));
        Assert.True(path.TryGetProperty(propertyName: "delete", out _));
        Assert.Equal(expected: 3, actual: path.EnumerateObject().Count());
    }

    [Fact]
    public void RestrictedOperationContainsSecurityRequirement()
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage<ReportPage>(name: "Restricted", url: "restricted/report");

        JsonElement operation = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/restricted/report")
            .GetProperty("get");

        JsonElement requirement = operation.GetProperty("security")[0];
        Assert.True(
            requirement.TryGetProperty(
                propertyName: ModeledOpenApiPageDocumenter.AuthenticationSchemeName,
                out JsonElement scopes
            )
        );
        Assert.Empty(scopes.EnumerateArray());
    }

    [Fact]
    public void XsltJsonDocumentationBecomesResponseExample()
    {
        using var fixture = new ModeledOpenApiFixture();
        XsltDataPage page = fixture.AddPage<XsltDataPage>(
            name: "Transformed",
            url: "public/transformed",
            configure: fixture.ConfigureTransformation
        );
        fixture.Document(
            page: page,
            documentationType: DocumentationType.EXAMPLE_JSON,
            value: """{"result":"documented"}"""
        );

        JsonElement example = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/public/transformed")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal(expected: "documented", actual: example.GetProperty("result").GetString());
    }

    [Fact]
    public void WorkflowJsonDocumentationBecomesRequestExample()
    {
        using var fixture = new ModeledOpenApiFixture();
        WorkflowPage page = fixture.AddPage<WorkflowPage>(name: "Workflow", url: "public/workflow");
        fixture.Document(
            page: page,
            documentationType: DocumentationType.EXAMPLE_JSON,
            value: """{"command":"run"}"""
        );

        JsonElement example = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/public/workflow")
            .GetProperty("post")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal(expected: "run", actual: example.GetProperty("command").GetString());
    }

    public enum PageKind
    {
        Xslt,
        Workflow,
        Report,
        Download,
    }

    private sealed class ModeledOpenApiFixture : IDisposable
    {
        private readonly Guid _dataStructureId = Guid.NewGuid();
        private readonly Guid _transformationId = Guid.NewGuid();
        private readonly Mock<IPersistenceProvider> _persistenceProvider = new();
        private readonly Mock<IDocumentationService> _documentationService = new();
        private readonly List<AbstractPage> _pages = [];

        public ModeledOpenApiFixture()
        {
            var dataStructure = new DataStructure
            {
                PersistenceProvider = _persistenceProvider.Object,
            };
            var entity = new DataStructureEntity
            {
                Name = "Record",
                ParentItemId = dataStructure.Id,
                PersistenceProvider = _persistenceProvider.Object,
            };
            var transformation = new XslTransformation
            {
                PersistenceProvider = _persistenceProvider.Object,
            };
            _persistenceProvider
                .Setup(provider => provider.RetrieveInstance<DataStructure>(_dataStructureId))
                .Returns(dataStructure);
            _persistenceProvider
                .Setup(provider =>
                    provider.RetrieveInstance<AbstractTransformation>(_transformationId)
                )
                .Returns(transformation);
            _persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByParent<ISchemaItem>(
                        It.IsAny<Key>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()
                    )
                )
                .Returns(
                    (
                        Key primaryKey,
                        string parentTableName,
                        string childTableName,
                        bool useCache
                    ) => primaryKey.Equals(dataStructure.PrimaryKey) ? [entity] : []
                );
            _persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByParent<SchemaItemAncestor>(
                        It.IsAny<Key>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()
                    )
                )
                .Returns([]);
            _persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByCategory<PageParameterMapping>(
                        PageParameterMapping.CategoryConst
                    )
                )
                .Returns([]);
            _persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByCategory<DataStructureEntity>(
                        DataStructureEntity.CategoryConst
                    )
                )
                .Returns([entity]);
            _persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByCategory<DataStructureColumn>(
                        DataStructureColumn.CategoryConst
                    )
                )
                .Returns([]);
        }

        public T AddPage<T>(string name, string url, Action<T>? configure = null)
            where T : AbstractPage, new()
        {
            var page = new T
            {
                Name = name,
                Url = url,
                Roles = "*",
                MimeType = "application/json",
                PersistenceProvider = _persistenceProvider.Object,
            };
            configure?.Invoke(page);
            _pages.Add(page);
            return page;
        }

        public AbstractPage AddPage(PageKind kind, string name, string url, bool allowCustomFilters)
        {
            return kind switch
            {
                PageKind.Xslt => AddPage<XsltDataPage>(
                    name,
                    url,
                    page =>
                    {
                        page.AllowCustomFilters = allowCustomFilters;
                        if (allowCustomFilters)
                        {
                            page.DataStructureId = _dataStructureId;
                        }
                    }
                ),
                PageKind.Workflow => AddPage<WorkflowPage>(name, url),
                PageKind.Report => AddPage<ReportPage>(name, url),
                PageKind.Download => AddPage<FileDownloadPage>(name, url),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, message: null),
            };
        }

        public void ConfigureTransformation(XsltDataPage page)
        {
            page.TransformationId = _transformationId;
        }

        public void Document(AbstractPage page, DocumentationType documentationType, string value)
        {
            _documentationService
                .Setup(service => service.GetDocumentation(page.Id, documentationType))
                .Returns(value);
        }

        public JsonDocument Generate()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
                        ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
                        ["UserApiOptions:PublicRoutes:0"] = "public",
                        ["UserApiOptions:RestrictedRoutes:0"] = "restricted",
                    }
                )
                .Build();
            var pagePolicy = new ModeledOpenApiPagePolicy(new StartUpConfiguration(configuration));
            var pageDocumenter = new ModeledOpenApiPageDocumenter(
                pagePolicy,
                new ModeledOpenApiSchemaFactory(),
                new ModeledOpenApiExampleFactory(_documentationService.Object)
            );
            var generator = new ModeledOpenApiDocumentGenerator(
                new OpenIddictConfig(configuration),
                new TestSchemaService(
                    new TestPagesSchemaItemProvider(_pages)
                    {
                        PersistenceProvider = _persistenceProvider.Object,
                    }
                ),
                pagePolicy,
                pageDocumenter
            );

            return JsonDocument.Parse(new ModeledOpenApiDocumentProvider(generator).GetDocument());
        }

        public void Dispose()
        {
            _persistenceProvider.Object.Dispose();
        }
    }

    private sealed class TestPagesSchemaItemProvider(IEnumerable<AbstractPage> pages)
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
