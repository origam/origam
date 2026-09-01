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
using Microsoft.OpenApi;
using Moq;
using NUnit.Framework;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Configuration;
using Origam.Server.OpenApi;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.ServerTests.OpenApi;

public class ModeledOpenApiDocumentGenerationTests
{
    [TestCase(PageKind.Xslt, false, "get")]
    [TestCase(PageKind.Xslt, true, "post")]
    [TestCase(PageKind.Workflow, false, "post")]
    [TestCase(PageKind.Report, false, "get")]
    [TestCase(PageKind.Download, false, "get")]
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

        Assert.That(path.TryGetProperty(expectedOperation, out JsonElement _), Is.True);
        Assert.That(path.EnumerateObject().Count(), Is.EqualTo(1));
    }

    [TestCase(true, false, "put")]
    [TestCase(false, true, "delete")]
    [TestCase(true, true, "put,delete")]
    public void UpdateAndDeleteFlagsReplaceDefaultOperation(
        bool allowPut,
        bool allowDelete,
        string expectedOperations
    )
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage<ReportPage>(
            name: "Mutable",
            url: "public/mutable",
            configure: page =>
            {
                page.AllowPUT = allowPut;
                page.AllowDELETE = allowDelete;
            }
        );

        JsonElement path = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/public/mutable");

        Assert.That(
            path.EnumerateObject().Select(operation => operation.Name),
            Is.EquivalentTo(expectedOperations.Split(','))
        );
    }

    [Test]
    public void ModeledOperationUsesModeledApiTag()
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage<ReportPage>(name: "Sales", url: "public/sales");

        JsonElement operation = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/public/sales")
            .GetProperty("get");

        Assert.That(
            operation.GetProperty("tags")[0].GetString(),
            Is.EqualTo("Modeled API — Uncategorized")
        );
    }

    [Test]
    public void RestrictedOperationContainsSecurityRequirement()
    {
        using var fixture = new ModeledOpenApiFixture();
        fixture.AddPage<ReportPage>(name: "Restricted", url: "private/report");

        JsonElement operation = fixture
            .Generate()
            .RootElement.GetProperty("paths")
            .GetProperty("/private/report")
            .GetProperty("get");

        JsonElement requirement = operation.GetProperty("security")[0];
        Assert.That(
            requirement.TryGetProperty(
                propertyName: ModeledOpenApiPageDocumenter.AuthenticationSchemeName,
                out JsonElement scopes
            ),
            Is.True
        );
        Assert.That(scopes.EnumerateArray(), Is.Empty);
    }

    [Test]
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

        Assert.That(example.GetProperty("result").GetString(), Is.EqualTo("documented"));
    }

    [Test]
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

        Assert.That(example.GetProperty("command").GetString(), Is.EqualTo("run"));
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
        private readonly Guid dataStructureId = Guid.NewGuid();
        private readonly Guid transformationId = Guid.NewGuid();
        private readonly Mock<IPersistenceProvider> persistenceProvider = new();
        private readonly Mock<IDocumentationService> documentationService = new();
        private readonly List<AbstractPage> pages = [];

        public ModeledOpenApiFixture()
        {
            var dataStructure = new DataStructure
            {
                PersistenceProvider = persistenceProvider.Object,
            };
            var entity = new DataStructureEntity
            {
                Name = "Record",
                ParentItemId = dataStructure.Id,
                PersistenceProvider = persistenceProvider.Object,
            };
            var transformation = new XslTransformation
            {
                PersistenceProvider = persistenceProvider.Object,
            };
            persistenceProvider
                .Setup(provider => provider.RetrieveInstance<DataStructure>(dataStructureId))
                .Returns(dataStructure);
            persistenceProvider
                .Setup(provider =>
                    provider.RetrieveInstance<AbstractTransformation>(transformationId)
                )
                .Returns(transformation);
            persistenceProvider
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
            persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByParent<SchemaItemAncestor>(
                        It.IsAny<Key>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()
                    )
                )
                .Returns([]);
            persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByCategory<PageParameterMapping>(
                        PageParameterMapping.CategoryConst
                    )
                )
                .Returns([]);
            persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByCategory<DataStructureEntity>(
                        DataStructureEntity.CategoryConst
                    )
                )
                .Returns([entity]);
            persistenceProvider
                .Setup(provider =>
                    provider.RetrieveListByCategory<DataStructureColumn>(
                        DataStructureColumn.CategoryConst
                    )
                )
                .Returns([]);
        }

        public T AddPage<T>(string name, string url, Action<T> configure = null)
            where T : AbstractPage, new()
        {
            var page = new T
            {
                Name = name,
                Url = url,
                Roles = "*",
                MimeType = "application/json",
                PersistenceProvider = persistenceProvider.Object,
            };
            configure?.Invoke(page);
            pages.Add(page);
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
                            page.DataStructureId = dataStructureId;
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
            page.TransformationId = transformationId;
        }

        public void Document(AbstractPage page, DocumentationType documentationType, string value)
        {
            documentationService
                .Setup(service => service.GetDocumentation(page.Id, documentationType))
                .Returns(value);
        }

        public JsonDocument Generate()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
                        ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
                        ["UserApiOptions:PublicRoutes:0"] = "public",
                        ["UserApiOptions:RestrictedRoutes:0"] = "private",
                    }
                )
                .Build();
            var pagePolicy = new ModeledOpenApiPagePolicy(new StartUpConfiguration(configuration));
            var pageDocumenter = new ModeledOpenApiPageDocumenter(
                pagePolicy,
                new ModeledOpenApiSchemaFactory(),
                new ModeledOpenApiExampleFactory(
                    documentationService.Object,
                    NullLogger<ModeledOpenApiExampleFactory>.Instance
                )
            );
            var generator = new ModeledOpenApiDocumentGenerator(
                new OpenIddictConfig(configuration),
                new TestSchemaService(
                    new TestPagesSchemaItemProvider(pages)
                    {
                        PersistenceProvider = persistenceProvider.Object,
                    }
                ),
                pagePolicy,
                pageDocumenter
            );

            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo { Title = "Combined API", Version = "1.0" },
                Paths = new OpenApiPaths(),
            };
            generator.AddTo(document);

            using var stringWriter = new StringWriter();
            var writer = new OpenApiJsonWriter(stringWriter);
            document.SerializeAsV3(writer);
            return JsonDocument.Parse(stringWriter.ToString());
        }

        public void Dispose()
        {
            persistenceProvider.Object.Dispose();
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
