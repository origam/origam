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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Moq;
using NUnit.Framework;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.DA.ServiceTests.Generators;

[TestFixture]
public class SelectSqlArrayFilterTests
{
    private static readonly Guid AllDataTypesDataStructureId = new Guid(
        "31791c3b-7265-439e-ac96-ddd57aa82579"
    );
    private const string RootEntityName = "AllDataTypes";
    private const string ArrayColumnName = "TagInput";
    private const string ScalarColumnName = "Text1";

    private FilePersistenceService persistenceService;
    private DataStructure dataStructure;
    private DataStructureEntity rootEntity;

    [OneTimeSetUp]
    public void LoadModel()
    {
        string modelDirectory = ResolveModelDirectory();
        if (modelDirectory == null)
        {
            Assert.Ignore(
                "model-tests model directory was not found next to the test assembly "
                    + "or in any parent directory."
            );
        }

        ConfigureSecurityContext();

        persistenceService = new FilePersistenceService(
            metaModelUpgradeService: new NullMetaModelUpgradeService(),
            defaultFolders: new List<string>
            {
                CategoryFactory.Create(typeof(Package)),
                CategoryFactory.Create(typeof(SchemaItemGroup)),
            },
            pathToRuntimeModelConfig: "RuntimeModelConfiguration.json",
            basePath: modelDirectory,
            watchFileChanges: false,
            useBinFile: false,
            checkRules: false,
            mode: MetaModelUpgradeMode.Ignore
        );

        IPersistenceProvider provider = persistenceService.SchemaProvider;
        dataStructure = (DataStructure)
            provider.RetrieveInstance(
                typeof(DataStructure),
                new ModelElementKey(AllDataTypesDataStructureId)
            );
        rootEntity = dataStructure
            .ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
            .First(entity => entity.Name == RootEntityName);
    }

    [OneTimeTearDown]
    public void Dispose()
    {
        persistenceService?.Dispose();
        SetAuthorizationProvider(null);
    }

    private static void ConfigureSecurityContext()
    {
        ConfigurationManager.SetActiveConfiguration(new OrigamSettings());
        SecurityManager.SetServerIdentity();

        var authorizationProvider = new Mock<IOrigamAuthorizationProvider>();
        authorizationProvider
            .Setup(provider => provider.Authorize(It.IsAny<IPrincipal>(), It.IsAny<string>()))
            .Returns(false);
        SetAuthorizationProvider(authorizationProvider.Object);
    }

    private static void SetAuthorizationProvider(IOrigamAuthorizationProvider provider)
    {
        typeof(SecurityManager)
            .GetField("_authorizationProvider", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, provider);
    }

    [Test]
    public void Should_render_array_field_filter_as_EXISTS_without_table_alias()
    {
        string sql = GenerateSql(
            $"[\"{ArrayColumnName}\",\"in\",[\"11111111-1111-1111-1111-111111111111\"]]"
        );

        Assert.That(sql, Does.Contain("EXISTS(SELECT * FROM"));
        Assert.That(sql, Does.Contain($"IN (@{ArrayColumnName}_in_0)"));

        string existsFromClause = ExtractArrayExistsFromClause(sql);
        Assert.That(
            existsFromClause,
            Does.Not.Contain(" AS "),
            "The array sub-select must reference the table directly, without an \"AS\" alias. "
                + "Actual FROM clause: "
                + existsFromClause
        );
    }

    [Test]
    public void Should_render_scalar_field_filter_as_plain_predicate()
    {
        string sql = GenerateSql($"[\"{ScalarColumnName}\",\"eq\",\"sample\"]");

        Assert.That(sql, Does.Contain($"[{ScalarColumnName}] = @{ScalarColumnName}_eq"));
        Assert.That(sql, Does.Not.Contain("EXISTS(SELECT * FROM"));
    }

    [Test]
    public void Should_render_plain_select_when_no_custom_filter_is_given()
    {
        string sql = GenerateSql("");

        Assert.That(sql.TrimStart(), Does.StartWith("SELECT"));
        Assert.That(sql, Does.Not.Contain("EXISTS(SELECT * FROM"));
    }

    private string GenerateSql(string customFilter)
    {
        var generator = new MsSqlCommandGenerator();
        var selectParameters = new SelectParameters
        {
            DataStructure = dataStructure,
            Entity = rootEntity,
            Parameters = new Hashtable(),
            CustomFilters = new CustomFilters { Filters = customFilter },
        };
        var adapter = (IDbDataAdapter)generator.GetAdapter();
        generator.BuildCommands(adapter, selectParameters, false);
        return adapter.SelectCommand.CommandText;
    }

    private static string ExtractArrayExistsFromClause(string sql)
    {
        int existsStart = sql.IndexOf("EXISTS(SELECT * FROM", StringComparison.Ordinal);
        int whereStart = sql.IndexOf(" WHERE", existsStart, StringComparison.Ordinal);
        return sql.Substring(existsStart, whereStart - existsStart);
    }

    private static string ResolveModelDirectory()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "model-tests", "model");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
            directory = directory.Parent;
        }
        return null;
    }
}
