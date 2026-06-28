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
    public void Should_execute_array_field_filter_against_real_database()
    {
        DataSet result = ExecuteWithCustomFilter(
            $"[\"{ArrayColumnName}\",\"in\",[\"11111111-1111-1111-1111-111111111111\"]]"
        );

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Should_execute_scalar_field_filter_against_real_database()
    {
        DataSet result = ExecuteWithCustomFilter($"[\"{ScalarColumnName}\",\"eq\",\"sample\"]");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Should_execute_plain_select_when_no_custom_filter_is_given()
    {
        DataSet result = ExecuteWithCustomFilter("");

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
}
