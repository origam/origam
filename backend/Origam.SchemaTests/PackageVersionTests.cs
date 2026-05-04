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
using Origam.Schema;

namespace Origam.SchemaTests;

[TestFixture]
public class PackageVersionTests
{
    [TestCase(arg: "5.0")]
    [TestCase(arg: "1.111")]
    [TestCase(arg: "1.011")]
    [TestCase(arg: "1.011.11")]
    [TestCase(arg: "")]
    [TestCase(arguments: null)]
    public void ShouldParse(string versionCandidate)
    {
        new PackageVersion(completeVersionString: versionCandidate);
    }

    [TestCase(arg: "5.")]
    [TestCase(arg: "1.111A")]
    [TestCase(arg: "dcsd")]
    [TestCase(arg: "1.cdf.11")]
    public void ShouldFailToParse(string versionCandidate)
    {
        Assert.Throws<ArgumentException>(code: () =>
            new PackageVersion(completeVersionString: versionCandidate)
        );
    }

    [TestCase(arg1: "5.0", arg2: "4.0")]
    [TestCase(arg1: "5.0.1", arg2: "5.0")]
    [TestCase(arg1: "5.1", arg2: "5.0")]
    [TestCase(arg1: "5.2", arg2: "5.0.11")]
    public void ShouldRecognizeFirstIsNewer(string newerVersionString, string olderVersionString)
    {
        var newerVersion = new PackageVersion(completeVersionString: newerVersionString);
        var olderVersion = new PackageVersion(completeVersionString: olderVersionString);

        Assert.True(condition: newerVersion > olderVersion);
        Assert.True(condition: newerVersion.CompareTo(other: olderVersion) > 0);
    }

    [TestCase(arg1: "5.0", arg2: "5")]
    [TestCase(arg1: "5.0", arg2: "5.0")]
    [TestCase(arg1: "5.0.11", arg2: "5.0.011")]
    public void ShouldBerecognizedAsEqual(string versionString1, string versionString2)
    {
        var version1 = new PackageVersion(completeVersionString: versionString1);
        var version2 = new PackageVersion(completeVersionString: versionString2);
        Assert.That(actual: version1, expression: Is.EqualTo(expected: version2));
        Assert.That(
            actual: version1.GetHashCode(),
            expression: Is.EqualTo(expected: version2.GetHashCode())
        );
    }
}
