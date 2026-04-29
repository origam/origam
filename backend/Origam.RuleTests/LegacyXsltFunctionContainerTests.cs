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
    [TestCase(arg1: "0000000000{", arg2: 2, arg3: 0.0)]
    [TestCase(arg1: "5545A", arg2: 2, arg3: 554.51)]
    [TestCase(arg1: "45a", arg2: 0, arg3: 451)]
    [TestCase(arg1: "10}", arg2: 2, arg3: -1.00)]
    [TestCase(arg1: "45D", arg2: 3, arg3: 0.454)]
    [TestCase(arg1: "45d", arg2: 0, arg3: 454.0)]
    [TestCase(arg1: "21M", arg2: 1, arg3: -21.4)]
    [TestCase(arg1: "21}", arg2: 0, arg3: -210.0)]
    [TestCase(arg1: "21{", arg2: 0, arg3: 210.0)]
    public void ShouldDecodeSignedOverpunch(
        string signedOverpunchVal,
        int decimalPlaces,
        double expectedNumber
    )
    {
        var decodedNum = LegacyXsltFunctionContainer.DecodeSignedOverpunch(
            stringToDecode: signedOverpunchVal,
            decimalPlaces: decimalPlaces
        );
        Assert.That(
            actual: decodedNum,
            expression: Is.EqualTo(expected: expectedNumber).Within(amount: 0.000000000000001)
        );
    }

    [TestCase(arg1: "21+", arg2: 1)]
    [TestCase(arg1: "21535", arg2: 2)]
    [TestCase(arg1: "dvdb", arg2: 1)]
    [TestCase(arg1: "dvdbA", arg2: 1)]
    [TestCase(arg1: "", arg2: 0)]
    [TestCase(arg1: "1", arg2: 0)]
    [TestCase(arg1: null, arg2: 1)]
    [TestCase(arg1: "45D", arg2: 5)]
    public void ShouldFailToDecodeSignedOverpunch(string invalidOverpunchVal, int decimalPlaces)
    {
        Assert.Throws<ArgumentException>(code: () =>
            LegacyXsltFunctionContainer.DecodeSignedOverpunch(
                stringToDecode: invalidOverpunchVal,
                decimalPlaces: decimalPlaces
            )
        );
    }
}
