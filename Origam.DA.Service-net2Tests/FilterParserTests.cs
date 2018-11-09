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
        [Test]
        public void ShouldParseFilter()
        {
            var strFilter = "[\"$AND\", [\"$OR\",[\"city_name\",\"like\",\"%Wash%\"],[\"name\",\"like\",\"%Smith%\"]], [\"age\",\"gte\",18],[\"id\",\"in\",[\"f2\",\"f3\",\"f4\"]]";
            var sqlWhere = new FilterParser().Parse(strFilter);
            Assert.That(sqlWhere, Is.Not.Null);
            Assert.That(sqlWhere, Is.Not.Empty);
            Assert.That(sqlWhere, Is.EqualTo("(((city_name LIKE %Wash%)) OR ((name LIKE %Smith%))) AND ((age >= 18)) AND (IN (f2,f3,f4))"));
        }
    }
}
