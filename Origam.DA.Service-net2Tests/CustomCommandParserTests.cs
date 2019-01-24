using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Origam.DA.Service.Generators;

namespace Origam.DA.Service_net2Tests
{
    [TestFixture]
    class CustomCommandParserTests
    {
        [TestCase(
            "[\"name\",\"gt\",\"John Doe\"]",
            "(name > 'JohnDoe')")]
        [TestCase(
            "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"%Wash%\"],[\"name\",\"like\",\"%Smith%\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
            "(((city_name LIKE '%Wash%')) OR ((name LIKE '%Smith%'))) AND ((age >= 18)) AND (id IN ('f2', 'f3', 'f4'))")]
        [TestCase("", "")]
        public void ShouldParseFilter(string filter, string expectedSqlWhere )
        {
            var sqlWhere = new CustomCommandParser("","").ToSqlWhere(filter);
            Assert.That(sqlWhere, Is.EqualTo(expectedSqlWhere));
        }

        [TestCase(null)]
        [TestCase("bla")]
        [TestCase("\"name\",\"gt\",\"John Doe\"]")] // "[" is missing
        [TestCase("[\"name\",\"gt\",\"John Doe\"")] // "]" is missing
        [TestCase("[\"name\"\"gt\",\"John Doe\"")] // "," is missing
        public void ShouldThrowArgumentExceptionWhenParsingFilter(string filter)
        {
            Assert.Throws<ArgumentException>(() => new CustomCommandParser("","").ToSqlWhere(filter));
        }

        [Test]
        public void ShouldParseOrderBy()
        {
            List<Tuple<string, string>> ordering = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("col1", "desc"),
                new Tuple<string, string>("col2", "asc")
            };
            string orderBy = new CustomCommandParser("","").ToSqlOrderBy(ordering);
            Assert.That(orderBy, Is.EqualTo("col1 DESC, col2 ASC"));
        }

        [TestCase(null)]
        [TestCase("col1, ")]
        [TestCase("col1,")]
        [TestCase(" ,desc")]
        [TestCase(",desc")]
        public void ShouldThrowArgumentExceptionWhenParsingOrderBy(
            string orderingStr)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new CustomCommandParser("","").ToSqlOrderBy(ToListOfTuples(orderingStr));
            });
        }

        private List<Tuple<string, string>> ToListOfTuples(string orderingStr)
        {
            if (orderingStr == null) return null;
            string[] strings = orderingStr.Split(',');
            return new List<Tuple<string, string>>
            {
                new Tuple<string, string>(strings[0], strings [1])
            };
        }
    }
}
