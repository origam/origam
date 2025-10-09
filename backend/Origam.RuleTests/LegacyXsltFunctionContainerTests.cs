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
using NUnit.Framework;
using Origam.Rule.XsltFunctions;

namespace Origam.RuleTests;

[TestFixture]
class LegacyXsltFunctionContainerTests
{
    [TestCase("0000000000{", 2, 0.0)]
    [TestCase("5545A", 2, 554.51)]
    [TestCase("45a", 0, 451)]
    [TestCase("10}", 2, -1.00)]
    [TestCase("45D", 3, 0.454)]
    [TestCase("45d", 0, 454.0)]
    [TestCase("21M", 1, -21.4)]
    [TestCase("21}", 0, -210.0)]
    [TestCase("21{", 0, 210.0)]
    public void ShouldDecodeSignedOverpunch(
        string signedOverpunchVal,
        int decimalPlaces,
        double expectedNumber
    )
    {
        var decodedNum = LegacyXsltFunctionContainer.DecodeSignedOverpunch(
            signedOverpunchVal,
            decimalPlaces
        );
        Assert.That(decodedNum, Is.EqualTo(expectedNumber).Within(0.000000000000001));
    }

    [TestCase("21+", 1)]
    [TestCase("21535", 2)]
    [TestCase("dvdb", 1)]
    [TestCase("dvdbA", 1)]
    [TestCase("", 0)]
    [TestCase("1", 0)]
    [TestCase(null, 1)]
    [TestCase("45D", 5)]
    public void ShouldFailToDecodeSignedOverpunch(string invalidOverpunchVal, int decimalPlaces)
    {
        Assert.Throws<ArgumentException>(() =>
            LegacyXsltFunctionContainer.DecodeSignedOverpunch(invalidOverpunchVal, decimalPlaces)
        );
    }
}
