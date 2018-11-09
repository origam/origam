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
    class FilterParserTests
    {
        [TestCase(
            "[\"name\",\"gt\",\"John Doe\"]",
            "(name > 'JohnDoe')")]
        [TestCase(
            "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"%Wash%\"],[\"name\",\"like\",\"%Smith%\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]",
            "(((city_name LIKE '%Wash%')) OR ((name LIKE '%Smith%'))) AND ((age >= 18)) AND (id IN ('f2', 'f3', 'f4'))")]

        public void ShouldParseFilter(string filter, string expectedSqlWhere )
        {
            var sqlWhere = new FilterParser().Parse(filter);
            Assert.That(sqlWhere, Is.Not.Null);
            Assert.That(sqlWhere, Is.Not.Empty);
            Assert.That(sqlWhere, Is.EqualTo(expectedSqlWhere));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("bla")]
        [TestCase("\"name\",\"gt\",\"John Doe\"]")] // "[" is missing
        [TestCase("[\"name\",\"gt\",\"John Doe\"")] // "]" is missing
        [TestCase("[\"name\"\"gt\",\"John Doe\"")] // "," is missing
        public void ShouldThrowArgumentException(string filter)
        {
            Assert.Throws<ArgumentException>(() => new FilterParser().Parse(filter));
        }
    }
}
