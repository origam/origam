#region license

/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using NUnit.Framework;
using Origam.DA.Service.CustomCommandParser;
using Origam.DA.Service.Generators;
using Origam.Schema;

namespace Origam.DA.ServiceTests;

[TestFixture]
class FilterCommandParserTests
{
    static object[] filterCases =
    {
        new object[]
        {
            "[\"name\",\"gt\",\"John Doe\"]",
            "([name] > @name_gt)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_gt",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"gt\",\"John, Doe\"]",
            "([name] > @name_gt)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_gt",
                    columnName: "name",
                    value: "John, Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"starts\",\"John Doe\"]",
            "([name] LIKE @name_starts+'%')",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_starts",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"nstarts\",\"John Doe\"]",
            "([name] NOT LIKE @name_nstarts+'%')",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_nstarts",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"ends\",\"John Doe\"]",
            "([name] LIKE '%'+@name_ends)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_ends",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"nends\",\"John Doe\"]",
            "([name] NOT LIKE '%'+@name_nends)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_nends",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"contains\",\"John Doe\"]",
            "([name] LIKE '%'+@name_contains+'%')",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_contains",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"ncontains\",\"John Doe\"]",
            "([name] NOT LIKE '%'+@name_ncontains+'%')",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_ncontains",
                    columnName: "name",
                    value: "John Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"name\",\"gt\",\"John' Doe\"]",
            "([name] > @name_gt)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_gt",
                    columnName: "name",
                    value: "John' Doe",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[] { "[\"name\",\"eq\",null]", "[name] IS NULL", new List<ParameterData>() },
        new object[]
        {
            "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"Wash\"],[\"name\",\"like\",\"Smith\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
            "((([city_name] LIKE '%'+@city_name_like+'%') OR ([name] LIKE '%'+@name_like+'%')) AND ([age] >= @age_gte) AND [id] IN (@id_in_0, @id_in_1, @id_in_2))",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "city_name_like",
                    columnName: "city_name",
                    value: "Wash",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "name_like",
                    columnName: "name",
                    value: "Smith",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "age_gte",
                    columnName: "age",
                    value: 18,
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "id_in_0",
                    columnName: "id",
                    value: "f2",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "id_in_1",
                    columnName: "id",
                    value: "f3",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "id_in_2",
                    columnName: "id",
                    value: "f4",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"age\",\"between\",[18, 80]]",
            "[age] BETWEEN @age_between_0 AND @age_between_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "age_between_0",
                    columnName: "age",
                    value: 18,
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "age_between_1",
                    columnName: "age",
                    value: 80,
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"cash\",\"between\",[18.4, 80]]",
            "[cash] BETWEEN @cash_between_0 AND @cash_between_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "cash_between_0",
                    columnName: "cash",
                    value: 18.4,
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "cash_between_1",
                    columnName: "cash",
                    value: 80,
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"age\",\"nbetween\",[18, 80]]",
            "[age] NOT BETWEEN @age_nbetween_0 AND @age_nbetween_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "age_nbetween_0",
                    columnName: "age",
                    value: 18,
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "age_nbetween_1",
                    columnName: "age",
                    value: 80,
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Name\",\"in\",[\"Tom\", \"Jane\", \"David\", \"Ben\"]]",
            "[Name] IN (@Name_in_0, @Name_in_1, @Name_in_2, @Name_in_3)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Name_in_0",
                    columnName: "Name",
                    value: "Tom",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_in_1",
                    columnName: "Name",
                    value: "Jane",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_in_2",
                    columnName: "Name",
                    value: "David",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_in_3",
                    columnName: "Name",
                    value: "Ben",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Name\",\"in\",[\"Tom\", \"Jane\", \"David\"]]",
            "[Name] IN (@Name_in_0, @Name_in_1, @Name_in_2)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Name_in_0",
                    columnName: "Name",
                    value: "Tom",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_in_1",
                    columnName: "Name",
                    value: "Jane",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_in_2",
                    columnName: "Name",
                    value: "David",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Name\",\"nin\",[\"Tom\", \"Jane\", \"David\"]]",
            "[Name] NOT IN (@Name_nin_0, @Name_nin_1, @Name_nin_2)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Name_nin_0",
                    columnName: "Name",
                    value: "Tom",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_nin_1",
                    columnName: "Name",
                    value: "Jane",
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Name_nin_2",
                    columnName: "Name",
                    value: "David",
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Timestamp\", \"between\", [\"2020-04-04T00:00:00.000\", \"2020-05-01T23:59:59.000\"]]",
            "[Timestamp] BETWEEN @Timestamp_between_0 AND @Timestamp_between_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Timestamp_between_0",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-04-04 00:00:00"),
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Timestamp_between_1",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-05-01 23:59:59"),
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Timestamp\", \"between\", [\"2020-04-04T00:00:00.000\", \"2020-05-01T00:00:00.000\"]]",
            "[Timestamp] BETWEEN @Timestamp_between_0 AND @Timestamp_between_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Timestamp_between_0",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-04-04 00:00:00"),
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Timestamp_between_1",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-05-01 23:59:59"),
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Timestamp\", \"nbetween\", [\"2020-08-04T00:00:00.000\", \"2020-05-01T23:59:59.000\"]]",
            "[Timestamp] NOT BETWEEN @Timestamp_nbetween_0 AND @Timestamp_nbetween_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Timestamp_nbetween_0",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-08-04 00:00:00"),
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Timestamp_nbetween_1",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-05-01 23:59:59"),
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[]
        {
            "[\"Timestamp\", \"nbetween\", [\"2020-08-04T00:00:00.000\", \"2020-05-01T00:00:00.000\"]]",
            "[Timestamp] NOT BETWEEN @Timestamp_nbetween_0 AND @Timestamp_nbetween_1",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "Timestamp_nbetween_0",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-08-04 00:00:00"),
                    dataType: OrigamDataType.String
                ),
                new ParameterData(
                    parameterName: "Timestamp_nbetween_1",
                    columnName: "Timestamp",
                    value: DateTime.Parse("2020-05-01 23:59:59"),
                    dataType: OrigamDataType.String
                ),
            },
        },
        new object[] { "", null, new List<ParameterData>() },
    };

    [Test, TestCaseSource(nameof(filterCases))]
    public void ShouldParseFilter(
        string filter,
        string expectedSqlWhere,
        List<ParameterData> expectedParameters
    )
    {
        var sut = new FilterCommandParser(
            filterRenderer: new MsSqlFilterRenderer(),
            whereFilterInput: filter,
            sqlRenderer: new MsSqlRenderer(),
            columns: new List<ColumnInfo>
            {
                new ColumnInfo { Name = "name", DataType = OrigamDataType.String },
                new ColumnInfo { Name = "Timestamp", DataType = OrigamDataType.Date },
                new ColumnInfo { Name = "age", DataType = OrigamDataType.Integer },
                new ColumnInfo { Name = "cash", DataType = OrigamDataType.Currency },
                new ColumnInfo { Name = "city_name", DataType = OrigamDataType.String },
                new ColumnInfo { Name = "Name", DataType = OrigamDataType.String },
                new ColumnInfo { Name = "id", DataType = OrigamDataType.String },
            }
        );
        Assert.That(sut.Sql, Is.EqualTo(expectedSqlWhere));
        Assert.That(sut.ParameterDataList, Has.Count.EqualTo(expectedParameters.Count));
        foreach (var parameterData in sut.ParameterDataList)
        {
            var expectedData = expectedParameters.Find(data =>
                data.ParameterName == parameterData.ParameterName
            );
            Assert.That(parameterData.ColumnName, Is.EqualTo(expectedData.ColumnName));
            Assert.That(parameterData.Value, Is.EqualTo(expectedData.Value));
        }
    }

    private static object[] filterCasesNullableColumns =
    {
        new object[]
        {
            "[\"name\", \"nnull\", null]",
            "[name] IS NOT NULL",
            new List<ParameterData>(),
        },
        new object[] { "[\"name\", \"null\", null]", "[name] IS NULL", new List<ParameterData>() },
        new object[]
        {
            "[\"name\", \"neq\", \"Tom\"]",
            "([name] <> @name_neq OR [name] IS NULL)",
            new List<ParameterData>
            {
                new ParameterData(
                    parameterName: "name_neq",
                    columnName: "name",
                    value: "Tom",
                    dataType: OrigamDataType.String
                ),
            },
        },
    };

    [Test, TestCaseSource(nameof(filterCasesNullableColumns))]
    public void ShouldParseFilterOnNullableColumn(
        string filter,
        string expectedSqlWhere,
        List<ParameterData> expectedParameters
    )
    {
        var sut = new FilterCommandParser(
            filterRenderer: new MsSqlFilterRenderer(),
            whereFilterInput: filter,
            sqlRenderer: new MsSqlRenderer(),
            columns: new List<ColumnInfo>
            {
                new ColumnInfo
                {
                    Name = "name",
                    DataType = OrigamDataType.String,
                    IsNullable = true,
                },
            }
        );
        Assert.That(sut.Sql, Is.EqualTo(expectedSqlWhere));
        Assert.That(sut.ParameterDataList, Has.Count.EqualTo(expectedParameters.Count));
        foreach (var parameterData in sut.ParameterDataList)
        {
            var expectedData = expectedParameters.Find(data =>
                data.ParameterName == parameterData.ParameterName
            );
            Assert.That(parameterData.ColumnName, Is.EqualTo(expectedData.ColumnName));
            Assert.That(parameterData.Value, Is.EqualTo(expectedData.Value));
        }
    }

    [TestCase(
        "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"Wash\"],[\"name\",\"like\",\"Smith\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
        new[] { "city_name", "name", "age", "id" }
    )]
    public void ShouldParseColumnNames(string filter, string[] expectedColumnNames)
    {
        var sut = new FilterCommandParser(
            filterRenderer: new MsSqlFilterRenderer(),
            whereFilterInput: filter,
            sqlRenderer: new MsSqlRenderer(),
            columns: new List<ColumnInfo>
            {
                new ColumnInfo { Name = "name", DataType = OrigamDataType.String },
                new ColumnInfo { Name = "Timestamp", DataType = OrigamDataType.Date },
                new ColumnInfo { Name = "age", DataType = OrigamDataType.Integer },
                new ColumnInfo { Name = "city_name", DataType = OrigamDataType.String },
                new ColumnInfo { Name = "Name", DataType = OrigamDataType.String },
                new ColumnInfo { Name = "id", DataType = OrigamDataType.String },
            }
        );
        Assert.That(sut.Columns, Is.EquivalentTo(expectedColumnNames));
    }

    [TestCase("bla")]
    [TestCase("\"name\",\"gt\",\"John Doe\"]")] // "[" is missing
    [TestCase("[\"name\",\"gt\",\"John Doe\"")] // "]" is missing
    [TestCase("[\"name\"\"gt\",\"John Doe\"")] // "," is missing
    public void ShouldThrowArgumentExceptionWhenParsingFilter(string filter)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var test = new FilterCommandParser(
                filterRenderer: new MsSqlFilterRenderer(),
                whereFilterInput: filter,
                sqlRenderer: new MsSqlRenderer(),
                columns: new List<ColumnInfo>()
            ).Sql;
        });
    }
}
