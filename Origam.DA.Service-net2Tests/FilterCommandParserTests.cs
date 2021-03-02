#region license

/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

namespace Origam.DA.ServiceTests
{
    [TestFixture]
    class FilterCommandParserTests
    {
        
        static object[] filterCases =
        {
            new object[] {
                "[\"name\",\"gt\",\"John Doe\"]",
                "([name] > @name)",
                new List<ParameterData> {
                   new ParameterData
                   (
                       columnName: "name", 
                       parameterName: "name", 
                       value: "John Doe",
                       dataType: OrigamDataType.String
                   )
                } 
            },
            new object[] {
                "[\"name\",\"gt\",\"John, Doe\"]",
                "([name] > @name)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John, Doe",
                        dataType: OrigamDataType.String
                    )
                }
            },
            new object[] {
                "[\"name\",\"starts\",\"John Doe\"]",
                "([name] LIKE @name+'%')",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John Doe",
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "[\"name\",\"nstarts\",\"John Doe\"]",
                "([name] NOT LIKE @name+'%')",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John Doe",
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "[\"name\",\"ends\",\"John Doe\"]",
                "([name] LIKE '%'+@name)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John Doe",
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "[\"name\",\"nends\",\"John Doe\"]",
                "([name] NOT LIKE '%'+@name)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John Doe",
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "[\"name\",\"contains\",\"John Doe\"]",
                "([name] LIKE '%'+@name+'%')",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John Doe",
                        dataType: OrigamDataType.String
                    )
                }  
            },
            new object[] {
                "[\"name\",\"ncontains\",\"John Doe\"]",
                "([name] NOT LIKE '%'+@name+'%')",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John Doe",
                        dataType: OrigamDataType.String
                    )
                }  
            },
            new object[] {
                "[\"name\",\"gt\",\"John' Doe\"]",
                "([name] > @name)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "John' Doe",
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "[\"name\",\"eq\",null]",
                "[name] IS NULL",
                new List<ParameterData>()
            },
            new object[] {
                "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"Wash\"],[\"name\",\"like\",\"Smith\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
                "((([city_name] LIKE '%'+@city_name+'%') OR ([name] LIKE '%'+@name+'%')) AND ([age] >= @age) AND [id] IN (@id_0, @id_1, @id_2))",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "city_name", 
                        parameterName: "city_name", 
                        value: "Wash",
                        dataType: OrigamDataType.String
                    ),
                    new ParameterData
                    (
                        columnName: "name", 
                        parameterName: "name", 
                        value: "Smith",
                        dataType: OrigamDataType.String
                    ),
                    new ParameterData
                    (
                        columnName: "age", 
                        parameterName: "age", 
                        value: 18,
                        dataType: OrigamDataType.String
                    ),
                    new ParameterData
                    (
                        columnName: "id", 
                        parameterName: "id_0", 
                        value: "f2",
                        dataType: OrigamDataType.String
                    ),
                    new ParameterData
                    (
                        columnName: "id", 
                        parameterName: "id_1", 
                        value: "f3",
                        dataType: OrigamDataType.String
                    ),
                    new ParameterData
                    (
                        columnName: "id", 
                        parameterName: "id_2", 
                        value: "f4",
                        dataType: OrigamDataType.String
                    ),
                },
            },
            new object[] {
                "[\"age\",\"between\",[18, 80]]",
                "[age] BETWEEN @age_0 AND @age_1",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "age", 
                        parameterName: "age_0", 
                        value: 18,
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "age", 
                        parameterName: "age_1", 
                        value: 80,
                        dataType: OrigamDataType.String
                    )
                }
            },
            new object[] {
                "[\"age\",\"nbetween\",[18, 80]]",
                "[age] NOT BETWEEN @age_0 AND @age_1",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "age", 
                        parameterName: "age_0", 
                        value: 18,
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "age", 
                        parameterName: "age_1", 
                        value: 80,
                        dataType: OrigamDataType.String
                    )
                }
            },
            new object[] {
                "[\"Name\",\"in\",[\"Tom\", \"Jane\", \"David\", \"Ben\"]]",
                "[Name] IN (@Name_0, @Name_1, @Name_2, @Name_3)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_0", 
                        value: "Tom",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_1", 
                        value: "Jane",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_2", 
                        value: "David",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_3", 
                        value: "Ben",
                        dataType: OrigamDataType.String
                    )
                }
            },
            new object[] {
                "[\"Name\",\"in\",[\"Tom\", \"Jane\", \"David\"]]",
                "[Name] IN (@Name_0, @Name_1, @Name_2)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_0", 
                        value: "Tom",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_1", 
                        value: "Jane",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_2", 
                        value: "David",
                        dataType: OrigamDataType.String
                    ),
                }
            },
            new object[] {
                "[\"Name\",\"nin\",[\"Tom\", \"Jane\", \"David\"]]",
                "[Name] NOT IN (@Name_0, @Name_1, @Name_2)",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_0", 
                        value: "Tom",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_1", 
                        value: "Jane",
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Name", 
                        parameterName: "Name_2", 
                        value: "David",
                        dataType: OrigamDataType.String
                    ),
                }
            },
            new object[] {
                "[\"Timestamp\", \"between\", [\"2020-08-04T00:00:00.000\", \"2020-05-01T00:00:00.000\"]]",
                "[Timestamp] BETWEEN @Timestamp_0 AND @Timestamp_1",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "Timestamp", 
                        parameterName: "Timestamp_0", 
                        value: DateTime.Parse("2020-08-04 00:00:00"),
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Timestamp", 
                        parameterName: "Timestamp_1", 
                        value: DateTime.Parse("2020-05-01 00:00:00"),
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "[\"Timestamp\", \"nbetween\", [\"2020-08-04T00:00:00.000\", \"2020-05-01T00:00:00.000\"]]",
                "[Timestamp] NOT BETWEEN @Timestamp_0 AND @Timestamp_1",
                new List<ParameterData> {
                    new ParameterData
                    (
                        columnName: "Timestamp", 
                        parameterName: "Timestamp_0", 
                        value: DateTime.Parse("2020-08-04 00:00:00"),
                        dataType: OrigamDataType.String
                    ),                    
                    new ParameterData
                    (
                        columnName: "Timestamp", 
                        parameterName: "Timestamp_1", 
                        value: DateTime.Parse("2020-05-01 00:00:00"),
                        dataType: OrigamDataType.String
                    )
                } 
            },
            new object[] {
                "",
                null,
                new List<ParameterData>()
            },
        };
        
        [Test, TestCaseSource(nameof(filterCases))]
        public void ShouldParseFilter(string filter, string expectedSqlWhere,
            List<ParameterData> expectedParameters)
        {
            var sut = new FilterCommandParser(
                nameLeftBracket: "[",
                nameRightBracket: "]",
                filterRenderer: new MsSqlFilterRenderer(),
                whereFilterInput: filter, 
                parameterReferenceChar: "@");
            sut.AddDataType("name", OrigamDataType.String);
            sut.AddDataType("Timestamp", OrigamDataType.Date);
            sut.AddDataType("age", OrigamDataType.Integer);
            sut.AddDataType("city_name", OrigamDataType.String);
            sut.AddDataType("Name", OrigamDataType.String);
            sut.AddDataType("id", OrigamDataType.String);

            Assert.That(sut.Sql, Is.EqualTo(expectedSqlWhere));
            Assert.That(sut.ParameterDataList, Has.Count.EqualTo(expectedParameters.Count));
            foreach (var parameterData in sut.ParameterDataList)
            {
                var expectedData = expectedParameters.Find(data =>
                    data.ParameterName == parameterData.ParameterName);
                Assert.That(parameterData.ColumnName, Is.EqualTo(expectedData.ColumnName));
                Assert.That(parameterData.Value, Is.EqualTo(expectedData.Value));
            }
        }

        [TestCase(
            "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"Wash\"],[\"name\",\"like\",\"Smith\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
            new [] {"city_name", "name", "age", "id"})]
        public void ShouldParseColumnNames(string filter,
            string[] expectedColumnNames)
        {
            var sut = new FilterCommandParser(
                nameLeftBracket: "[",
                nameRightBracket: "]",
                filterRenderer: new MsSqlFilterRenderer(),
                whereFilterInput: filter,
                parameterReferenceChar: "@");
            sut.AddDataType("name", OrigamDataType.String);
            sut.AddDataType("Timestamp", OrigamDataType.Date);
            sut.AddDataType("age", OrigamDataType.Integer);
            sut.AddDataType("city_name", OrigamDataType.String);
            sut.AddDataType("Name", OrigamDataType.String);
            sut.AddDataType("id", OrigamDataType.String);

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
                        nameLeftBracket: "[",
                        nameRightBracket: "]",
                        filterRenderer: new MsSqlFilterRenderer(),
                        whereFilterInput: filter,
                        parameterReferenceChar: "@")
                    .Sql;
            });
        }
    }
}