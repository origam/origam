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
    [TestCase("5.0")]
    [TestCase("1.111")]
    [TestCase("1.011")]
    [TestCase("1.011.11")]
    [TestCase("")]
    [TestCase(null)]
    public void ShouldParse(string versionCandidate)
    {
        new PackageVersion(versionCandidate);
    }
    
    [TestCase("5.")]
    [TestCase("1.111A")]
    [TestCase("dcsd")]
    [TestCase("1.cdf.11")]
    public void ShouldFailToParse(string versionCandidate)
    {
        Assert.Throws<ArgumentException>(
            () => new PackageVersion(versionCandidate));
    }
    
            
    [TestCase("5.0","4.0")]
    [TestCase("5.0.1", "5.0")]
    [TestCase("5.1","5.0")]
    [TestCase("5.2","5.0.11")]
    public void ShouldRecognizeFirstIsNewer(
        string newerVersionString, string olderVersionString)
    {
        var newerVersion = new PackageVersion(newerVersionString);
        var olderVersion = new PackageVersion(olderVersionString);
        
        Assert.True(newerVersion > olderVersion);
        Assert.True(newerVersion.CompareTo(olderVersion) > 0);
    }
    [TestCase("5.0","5")]
    [TestCase("5.0","5.0")]
    [TestCase("5.0.11","5.0.011")]
    public void ShouldBerecognizedAsEqual(
        string versionString1, string versionString2)
    {
        var version1 = new PackageVersion(versionString1);
        var version2 = new PackageVersion(versionString2); 
        Assert.That(version1, Is.EqualTo(version2));
        Assert.That(version1.GetHashCode(), Is.EqualTo(version2.GetHashCode()));
    }
}
