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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.Generators;
using Origam.Schema;

namespace Origam.DA.Service_net2Tests
{
    [TestFixture]
    class CustomCommandParserTests
    {
        [TestCase(
            "[\"name\",\"gt\",\"John Doe\"]",
            "([name] > 'John Doe')")]        
        [TestCase(
            "[\"name\",\"gt\",\"John, Doe\"]",
            "([name] > 'John, Doe')")]
        [TestCase(
            "[\"name\",\"starts\",\"John Doe\"]",
            "([name] LIKE 'John Doe%')")]
        [TestCase(
            "[\"name\",\"nstarts\",\"John Doe\"]",
            "([name] NOT LIKE 'John Doe%')")]
        [TestCase(
            "[\"name\",\"ends\",\"John Doe\"]",
            "([name] LIKE '%John Doe')")]
        [TestCase(
            "[\"name\",\"nends\",\"John Doe\"]",
            "([name] NOT LIKE '%John Doe')")]
        [TestCase(
            "[\"name\",\"contains\",\"John Doe\"]",
            "([name] LIKE '%John Doe%')")]
        [TestCase(
            "[\"name\",\"ncontains\",\"John Doe\"]",
    "([name] NOT LIKE '%John Doe%')")]
        [TestCase(
            "[\"name\",\"gt\",\"John' Doe\"]",
            "([name] > 'John'' Doe')")]        
        [TestCase(
            "[\"name\",\"eq\",null]",
            "[name] IS NULL")]
        [TestCase(
            "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"Wash\"],[\"name\",\"like\",\"Smith\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
            "((([city_name] LIKE '%Wash%') OR ([name] LIKE '%Smith%')) AND ([age] >= 18) AND [id] IN ('f2', 'f3', 'f4'))")]        
        [TestCase(
            "[\"age\",\"between\",[18, 80]]",
            "[age] BETWEEN 18 AND 80")]        
        [TestCase(
            "[\"age\",\"nbetween\",[18, 80]]",
            "[age] NOT BETWEEN 18 AND 80")]         
        [TestCase(
            "[\"Name\",\"in\",[\"Tom\", \"Jane\", \"David\"]]",
            "[Name] IN ('Tom', 'Jane', 'David')")]            
        [TestCase(
            "[\"Name\",\"nin\",[\"Tom\", \"Jane\", \"David\"]]",
            "[Name] NOT IN ('Tom', 'Jane', 'David')")]        
        [TestCase(
            "[\"Timestamp\", \"between\", [\"2020-08-04T00:00:00.000\", \"2020-05-01T00:00:00.000\"]]",
            "[Timestamp] BETWEEN  '2020-08-04 00:00:00'  AND  '2020-05-01 00:00:00' ")]       
        [TestCase(
            "[\"Timestamp\", \"nbetween\", [\"2020-08-04T00:00:00.000\", \"2020-05-01T00:00:00.000\"]]",
            "[Timestamp] NOT BETWEEN  '2020-08-04 00:00:00'  AND  '2020-05-01 00:00:00' ")]
        [TestCase("", null)]
        public void ShouldParseFilter(string filter, string expectedSqlWhere )
        {
            var sut = new CustomCommandParser("[", "]", 
                    new SQLValueFormatter("1", "0",(text) => text.Replace("%", "[%]").Replace("_", "[_]")))
                .Where(filter);
            sut.AddDataType("name", OrigamDataType.String);
            sut.AddDataType("Timestamp", OrigamDataType.Date);
            sut.AddDataType("age", OrigamDataType.Integer);
            sut.AddDataType("city_name", OrigamDataType.String);
            sut.AddDataType("Name", OrigamDataType.String);
            sut.AddDataType("id", OrigamDataType.String);
            
            Assert.That(sut.WhereClause, Is.EqualTo(expectedSqlWhere));
        }

        [TestCase("bla")]
        [TestCase("\"name\",\"gt\",\"John Doe\"]")] // "[" is missing
        [TestCase("[\"name\",\"gt\",\"John Doe\"")] // "]" is missing
        [TestCase("[\"name\"\"gt\",\"John Doe\"")] // "," is missing
        public void ShouldThrowArgumentExceptionWhenParsingFilter(string filter)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                string sql = new CustomCommandParser("[", "]", 
                    new SQLValueFormatter("1", "0",(text) => text.Replace("%", "[%]").Replace("_", "[_]")))
                    .Where(filter)
                    .WhereClause;
            });
    }

        [Test]
        public void ShouldParseOrderBy()
        {
            List<Ordering> ordering = new List<Ordering>
            {
                new Ordering("col1", "desc",100),
                new Ordering("col2", "asc",101)
            };
            string orderBy = new CustomCommandParser("[","]", 
                    new SQLValueFormatter("1", "0",(text) => text.Replace("%", "[%]").Replace("_", "[_]")))
                .OrderBy(ordering)
                .OrderByClause;
            Assert.That(orderBy, Is.EqualTo("[col1] DESC, [col2] ASC"));
        }
        
        [TestCase("col1, ")]
        [TestCase("col1,")]
        [TestCase(" ,desc")]
        [TestCase(",desc")]
        public void ShouldThrowArgumentExceptionWhenParsingOrderBy(
            string orderingStr)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new CustomCommandParser("[","]", 
                    new SQLValueFormatter("1", "0",(text) => text.Replace("%", "[%]").Replace("_", "[_]")))
                    .OrderBy(ToListOfOrderings(orderingStr));
            });
        }

        private List<Ordering> ToListOfOrderings(string orderingStr)
        {
            if (orderingStr == null) return null;
            string[] strings = orderingStr.Split(',');
            return new List<Ordering>
            {
                new Ordering(strings[0], strings [1],100)
            };
        }
    }
}
