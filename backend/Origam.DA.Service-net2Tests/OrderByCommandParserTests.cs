using System;
using System.Collections.Generic;
using NUnit.Framework;
using Origam.DA.Service.CustomCommandParser;
using Origam.DA.Service.Generators;
#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.DA.Service_net2Tests;

[TestFixture]
class OrderByCommandParserTests
{
    [Test]
    public void ShouldParseOrderBy()
    {
        List<Ordering> ordering = new List<Ordering>
        {
            new Ordering("col1", "desc", 100),
            new Ordering("col2", "asc", 101),
        };
        var sut = new OrderByCommandParser(orderingsInput: ordering);
        sut.SetColumnExpressionsIfMissing("col1", new[] { "[col1]" });
        sut.SetColumnExpressionsIfMissing("col2", new[] { "[col2]" });
        string orderBy = sut.Sql;
        Assert.That(orderBy, Is.EqualTo("[col1] DESC, [col2] ASC"));
    }

    [TestCase("col1, ")]
    [TestCase("col1,")]
    [TestCase(" ,desc")]
    [TestCase(",desc")]
    public void ShouldThrowArgumentExceptionWhenParsingOrderBy(string orderingStr)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new OrderByCommandParser(orderingsInput: ToListOfOrderings(orderingStr));
        });
    }

    private List<Ordering> ToListOfOrderings(string orderingStr)
    {
        if (orderingStr == null)
            return null;
        string[] strings = orderingStr.Split(',');
        return new List<Ordering> { new Ordering(strings[0], strings[1], 100) };
    }
}
