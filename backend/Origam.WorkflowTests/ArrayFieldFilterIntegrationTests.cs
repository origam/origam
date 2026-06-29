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

using System.Data;
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
    private const string ArrayColumnName = "TagInput";
    private const string ScalarColumnName = "Text1";
    private const string IdColumnName = "Id";
    private const string FirstTagId = "11111111-1111-1111-1111-111111111111";
    private const string SecondTagId = "22222222-2222-2222-2222-222222222222";

    public ArrayFieldFilterIntegrationTests()
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BasicConfigurator.Configure();
    }

    [TearDown]
    public void TearDown()
    {
        Origam.OrigamEngine.OrigamEngine.DisconnectRuntime();
    }

    [Test]
    public void ShouldExecuteArrayFieldFilterAgainstRealDatabase()
    {
        DataSet result = ExecuteWithCustomFilter(
            $"[\"{ArrayColumnName}\",\"in\",[\"11111111-1111-1111-1111-111111111111\"]]"
        );

        LogResult(nameof(ShouldExecuteArrayFieldFilterAgainstRealDatabase), result);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldExecuteNegatedArrayFieldFilterAgainstRealDatabase()
    {
        DataSet result = ExecuteWithCustomFilter(
            $"[\"{ArrayColumnName}\",\"nin\",[\"{FirstTagId}\"]]"
        );

        LogResult(nameof(ShouldExecuteNegatedArrayFieldFilterAgainstRealDatabase), result);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldExecuteArrayFieldFilterWithMultipleValuesAgainstRealDatabase()
    {
        DataSet result = ExecuteWithCustomFilter(
            $"[\"{ArrayColumnName}\",\"in\",[\"{FirstTagId}\",\"{SecondTagId}\"]]"
        );

        LogResult(
            nameof(ShouldExecuteArrayFieldFilterWithMultipleValuesAgainstRealDatabase),
            result
        );
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldPartitionRowsBetweenArrayFieldInAndNin()
    {
        HashSet<string> allRowIds = LoadRootRowIds("");
        HashSet<string> matchingRowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"in\",[\"{FirstTagId}\"]]"
        );
        HashSet<string> nonMatchingRowIds = LoadRootRowIds(
            $"[\"{ArrayColumnName}\",\"nin\",[\"{FirstTagId}\"]]"
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                matchingRowIds.Overlaps(nonMatchingRowIds),
                Is.False,
                "A row cannot satisfy both the array \"in\" and \"nin\" filters."
            );
            Assert.That(
                matchingRowIds.Union(nonMatchingRowIds),
                Is.EquivalentTo(allRowIds),
                "Together the array \"in\" and \"nin\" filters must cover every unfiltered row."
            );
        });
    }

    [Test]
    public void ShouldExecuteScalarFieldFilterAgainstRealDatabase()
    {
        DataSet result = ExecuteWithCustomFilter($"[\"{ScalarColumnName}\",\"eq\",\"sample\"]");

        LogResult(nameof(ShouldExecuteScalarFieldFilterAgainstRealDatabase), result);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ShouldExecutePlainSelectWhenNoCustomFilterIsGiven()
    {
        DataSet result = ExecuteWithCustomFilter("");

        LogResult(nameof(ShouldExecutePlainSelectWhenNoCustomFilterIsGiven), result);
        Assert.That(result, Is.Not.Null);
    }

    private DataSet ExecuteWithCustomFilter(string customFilter)
    {
        Origam.OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );

        var query = new DataStructureQuery(AllDataTypesDataStructureId)
        {
            CustomFilters = new CustomFilters { Filters = customFilter },
        };

        IDataService dataService = DataServiceFactory.GetDataService();

        return dataService.LoadDataSet(
            query,
            SecurityManager.CurrentPrincipal,
            transactionId: null
        );
    }

    private HashSet<string> LoadRootRowIds(string customFilter)
    {
        DataSet result = ExecuteWithCustomFilter(customFilter);
        DataTable rootTable = result
            .Tables.Cast<DataTable>()
            .First(table => table.Columns.Contains(IdColumnName));
        return rootTable
            .Rows.Cast<DataRow>()
            .Select(row => row[IdColumnName].ToString() ?? string.Empty)
            .ToHashSet();
    }

    private static void LogResult(string scenario, DataSet result)
    {
        TestContext.Out.WriteLine($"--- {scenario} ---");
        TestContext.Out.WriteLine($"Tables: {result.Tables.Count}");
        foreach (DataTable table in result.Tables)
        {
            TestContext.Out.WriteLine($"Table '{table.TableName}': {table.Rows.Count} row(s)");
            int rowIndex = 0;
            foreach (DataRow row in table.Rows)
            {
                if (rowIndex >= 20)
                {
                    TestContext.Out.WriteLine("  ... (truncated)");
                    break;
                }
                string values = string.Join(
                    separator: ", ",
                    table
                        .Columns.Cast<DataColumn>()
                        .Select(column => $"{column.ColumnName}={row[column]}")
                );
                TestContext.Out.WriteLine($"  [{rowIndex}] {values}");
                rowIndex++;
            }
        }
    }
}
