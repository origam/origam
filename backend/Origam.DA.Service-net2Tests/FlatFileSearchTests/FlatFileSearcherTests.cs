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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.TestCommon;

namespace Origam.DA.Service_net2Tests;

[TestFixture]
public class FlatFileSearcherTests : AbstractFileTestClass
{
    protected override TestContext TestContext => TestContext.CurrentContext;

    /// [Test]
    public void ShouldThrowBecauseIdInFoundElementIsMissing()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            FindStrings(stringToFind: "this item has no id");
        });
        StringAssert.Contains("is malformed", exception.Message);
    }

    [Test]
    public void ShouldFindAllItemsContainingKeyword()
    {
        string stringToFind = "blable";

        var flatFileSearcher = new FlatFileSearcher(stringToFind);
        List<Guid> foundElementIds = flatFileSearcher.SearchIn(
            new List<DirectoryInfo> { TestFilesDir }
        );

        Assert.That(foundElementIds.Count, Is.EqualTo(2));
    }

    /// <summary>
    /// Takes 300 ms on 1100 files.
    /// </summary>
    /// [Test]
    public void SpeedTest()
    {
        string pathToLargeNumberOfTestFiles = @"C:\Bordel\Serialization";
        string stringToFind = "calendar";
        IEnumerable<DirectoryInfo> packageDirs = Directory
            .EnumerateDirectories(pathToLargeNumberOfTestFiles)
            .Select(path => new DirectoryInfo(path));
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var flatFileSearcher = new FlatFileSearcher(stringToFind);
        List<Guid> foundElementIds = flatFileSearcher.SearchIn(packageDirs);

        sw.Stop();
        Console.WriteLine($"FlatFile search took: {sw.Elapsed}");

        Assert.That(foundElementIds.Count, Is.EqualTo(5));
    }

    [TestCase("Test blable string", new[] { "221bf117-1afb-462e-8c38-5b66ad84c347" })]
    [TestCase("Test file blable", new[] { "8afba476-ce1e-4c8c-b5e8-d3326b0e658d" })]
    [TestCase(
        "blable",
        new[] { "221bf117-1afb-462e-8c38-5b66ad84c347", "8afba476-ce1e-4c8c-b5e8-d3326b0e658d" }
    )]
    [TestCase(
        "blabl*",
        new[] { "221bf117-1afb-462e-8c38-5b66ad84c347", "8afba476-ce1e-4c8c-b5e8-d3326b0e658d" }
    )]
    [TestCase(
        "*lable",
        new[] { "221bf117-1afb-462e-8c38-5b66ad84c347", "8afba476-ce1e-4c8c-b5e8-d3326b0e658d" }
    )]
    [TestCase(
        "*labl*",
        new[] { "221bf117-1afb-462e-8c38-5b66ad84c347", "8afba476-ce1e-4c8c-b5e8-d3326b0e658d" }
    )]
    public void ShouldFindStringInFiles(string stringToFind, string[] expectedResultIdStr)
    {
        List<Guid> foundElementIds = FindStrings(stringToFind);
        List<Guid> expectedGuids = expectedResultIdStr.Select(x => new Guid(x)).ToList();
        Assert.That(foundElementIds.Count, Is.EqualTo(expectedGuids.Count));
        foreach (Guid expectedGuid in expectedGuids)
        {
            Assert.That(foundElementIds, Contains.Item(expectedGuid));
        }
    }

    [TestCase("blabl")]
    [TestCase("lable")]
    [TestCase("labl")]
    public void ShouldFailToFindFindStringInFiles(string stringToFind)
    {
        List<Guid> foundElementIds = FindStrings(stringToFind);
        Assert.That(foundElementIds.Count, Is.EqualTo(0));
    }

    private List<Guid> FindStrings(string stringToFind)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var flatFileSearcher = new FlatFileSearcher(stringToFind);
        List<Guid> foundElementIds = flatFileSearcher.SearchIn(
            new List<DirectoryInfo> { TestFilesDir }
        );
        sw.Stop();
        Console.WriteLine($"FlatFile search took: {sw.Elapsed}");
        return foundElementIds;
    }

    protected override string DirName => "FlatFileSearchTests";
}
