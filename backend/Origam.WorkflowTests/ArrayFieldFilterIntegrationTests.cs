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

using log4net.Config;
using NUnit.Framework;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.WorkflowTests;

[TestFixture]
public class ArrayFieldFilterIntegrationTests
{
    private static readonly Guid AllDataTypesDataStructureId = new Guid(
        "31791c3b-7265-439e-ac96-ddd57aa82579"
    );
    private const string RootEntityName = "AllDataTypes";
    private const string ArrayColumnName = "TagInput";
    private const string ScalarColumnName = "Text1";
    private const string IdColumnName = "Id";
    private const string ExistingTagId = "eef55cab-d66f-4828-b3bf-f27c7930858f";
    private const string SecondExistingTagId = "74ba6e7d-6d77-4268-9e73-601a71d8b385";
    private const string UnknownTagId = "11111111-1111-1111-1111-111111111111";

    public ArrayFieldFilterIntegrationTests()
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BasicConfigurator.Configure();
        Origam.OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Origam.OrigamEngine.OrigamEngine.DisconnectRuntime();
    }

    [Test]
    public void ShouldExecuteArrayFieldFilterAgainstRealDatabase()
    {
        List<string> rowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"in\",[\"{ExistingTagId}\"]]"
        );

        Assert.That(rowIds, Is.Not.Null);
    }

    [Test]
    public void ShouldExecuteNegatedArrayFieldFilterAgainstRealDatabase()
    {
        List<string> rowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"nin\",[\"{ExistingTagId}\"]]"
        );

        Assert.That(rowIds, Is.Not.Null);
    }

    [Test]
    public void ShouldExecuteArrayFieldFilterWithMultipleValuesAgainstRealDatabase()
    {
        List<string> rowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"in\",[\"{ExistingTagId}\",\"{SecondExistingTagId}\"]]"
        );

        Assert.That(rowIds, Is.Not.Null);
    }

    [Test]
    public void ShouldOnlyReturnRowsCarryingTheTagForArrayFieldInFilter()
    {
        List<string> allRowIds = LoadRootRowIds("");
        List<string> matchingRowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"in\",[\"{ExistingTagId}\"]]"
        );
        List<string> unknownTagRowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"in\",[\"{UnknownTagId}\"]]"
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                matchingRowIds,
                Is.Not.Empty,
                message: "The array \"in\" filter must return the rows carrying the tag."
            );
            Assert.That(
                matchingRowIds.Count,
                Is.LessThan(allRowIds.Count),
                message: "A correlated EXISTS must return a strict subset, not every row."
            );
            Assert.That(
                unknownTagRowIds,
                Is.Empty,
                message: "A tag that is not assigned to any row must match nothing."
            );
        });
    }

    [Test]
    public void ShouldPartitionRowsBetweenArrayFieldInAndNin()
    {
        HashSet<string> allRowIds = LoadRootRowIds("").ToHashSet();
        HashSet<string> matchingRowIds = LoadRootRowIds(
                $"[\"{ArrayColumnName}\",\"in\",[\"{ExistingTagId}\"]]"
            )
            .ToHashSet();
        HashSet<string> nonMatchingRowIds = LoadRootRowIds(
                $"[\"{ArrayColumnName}\",\"nin\",[\"{ExistingTagId}\"]]"
            )
            .ToHashSet();

        Assert.Multiple(() =>
        {
            Assert.That(
                matchingRowIds.Overlaps(nonMatchingRowIds),
                Is.False,
                message: "A row cannot satisfy both the array \"in\" and \"nin\" filters."
            );
            Assert.That(
                matchingRowIds.Union(nonMatchingRowIds),
                Is.EquivalentTo(allRowIds),
                message: "Together the array \"in\" and \"nin\" filters must cover every unfiltered row."
            );
        });
    }

    [Test]
    public void ShouldExecuteScalarFieldFilterAgainstRealDatabase()
    {
        List<string> rowIds = LoadRootRowIds($"[\"{ScalarColumnName}\",\"eq\",\"sample\"]");

        Assert.That(rowIds, Is.Not.Null);
    }

    [Test]
    public void ShouldExecutePlainSelectWhenNoCustomFilterIsGiven()
    {
        List<string> rowIds = LoadRootRowIds("");

        Assert.That(rowIds, Is.Not.Empty);
    }

    private List<string> LoadRootRowIds(string customFilter)
    {
        var query = new DataStructureQuery(AllDataTypesDataStructureId)
        {
            Entity = RootEntityName,
            CustomFilters = new CustomFilters { Filters = customFilter },
            ColumnsInfo = new ColumnsInfo(
                columnName: IdColumnName,
                renderSqlForDetachedFields: true
            ),
            ForceDatabaseCalculation = true,
        };

        IDataService dataService = DataServiceFactory.GetDataService();

        List<string> rowIds = dataService
            .ExecuteDataReaderReturnPairs(query)
            .Select(row => row[IdColumnName].ToString() ?? string.Empty)
            .ToList();

        TestContext.Out.WriteLine($"Filter '{customFilter}' returned {rowIds.Count} row(s).");
        return rowIds;
    }
}
