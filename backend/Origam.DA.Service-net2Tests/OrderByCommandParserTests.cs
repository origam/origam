using System;
using System.Collections.Generic;
using NUnit.Framework;
using Origam.DA.Service.CustomCommandParser;
using Origam.DA.Service.Generators;

namespace Origam.DA.Service_net2Tests;
[TestFixture]
class OrderByCommandParserTests
{       
    [Test]
    public void ShouldParseOrderBy()
    {
        List<Ordering> ordering = new List<Ordering>
        {
            new Ordering("col1", "desc",100),
            new Ordering("col2", "asc",101)
        };
        var sut = new OrderByCommandParser(orderingsInput: ordering);
        sut.SetColumnExpressionsIfMissing("col1", new []{"[col1]"});
        sut.SetColumnExpressionsIfMissing("col2", new []{"[col2]"});
        string orderBy = sut.Sql;
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
            new OrderByCommandParser(
                orderingsInput: ToListOfOrderings(orderingStr));
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
