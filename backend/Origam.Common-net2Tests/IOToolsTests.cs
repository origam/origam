#region license
/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

using System.IO;
using NUnit.Framework;

namespace Origam.Common_net2Tests;

[TestFixture]
public class IOToolsTests
{
    [TestCase(@"c:\foo", @"c:\", ExpectedResult = true)]
    [TestCase(@"c:\foo", @"c:\foo", ExpectedResult = true)]
    [TestCase(@"c:\foo", @"c:\foo\", ExpectedResult = true)]
    [TestCase(@"c:\foo\", @"c:\foo", ExpectedResult = true)]
    [TestCase(@"c:\foo\bar\", @"c:\foo\", ExpectedResult = true)]
    [TestCase(@"c:\foo\bar", @"c:\foo\", ExpectedResult = true)]
    [TestCase(@"c:\foo\a.txt", @"c:\foo", ExpectedResult = true)]
    [TestCase(@"c:\FOO\a.txt", @"c:\foo", ExpectedResult = false)]
    [TestCase(@"c:/foo/a.txt", @"c:\foo", ExpectedResult = true)]
    [TestCase(@"c:\foobar", @"c:\foo", ExpectedResult = false)]
    [TestCase(@"c:\foobar\a.txt", @"c:\foo", ExpectedResult = false)]
    [TestCase(@"c:\foobar\a.txt", @"c:\foo\", ExpectedResult = false)]
    [TestCase(@"c:\foo\a.txt", @"c:\foobar", ExpectedResult = false)]
    [TestCase(@"c:\foo\a.txt", @"c:\foobar\", ExpectedResult = false)]
    [TestCase(@"c:\foo\..\bar\baz", @"c:\foo", ExpectedResult = false)]
    [TestCase(@"c:\foo\..\bar\baz", @"c:\bar", ExpectedResult = true)]
    [TestCase(@"c:\foo\..\bar\baz", @"c:\barr", ExpectedResult = false)]
    [TestCase(@"\foo", @"\foo", ExpectedResult = true)]
    [TestCase(@"\foo", @"\foo\", ExpectedResult = true)]
    [TestCase(@"\foo\", @"\foo", ExpectedResult = true)]
    [TestCase(@"\foo\bar\", @"\foo\", ExpectedResult = true)]
    [TestCase(@"\foo\bar", @"\foo\", ExpectedResult = true)]
    [TestCase(@"\foo\a.txt", @"\foo", ExpectedResult = true)]
    [TestCase(@"\FOO\a.txt", @"\foo", ExpectedResult = false)]
    [TestCase(@"/foo/a.txt", @"\foo", ExpectedResult = true)]
    [TestCase(@"\foobar", @"\foo", ExpectedResult = false)]
    [TestCase(@"\foobar\a.txt", @"\foo", ExpectedResult = false)]
    [TestCase(@"\foobar\a.txt", @"\foo\", ExpectedResult = false)]
    [TestCase(@"\foo\a.txt", @"\foobar", ExpectedResult = false)]
    [TestCase(@"\foo\a.txt", @"\foobar\", ExpectedResult = false)]
    [TestCase(@"\foo\..\bar\baz", @"\foo", ExpectedResult = false)]
    [TestCase(@"\foo\..\bar\baz", @"\bar", ExpectedResult = true)]
    [TestCase(@"\foo\..\bar\baz", @"\barr", ExpectedResult = false)]
    public bool IsSubPathOfTest(string path, string baseDirPath)
    {
        return IOTools.IsSubPathOf(
            path.Replace('\\', Path.DirectorySeparatorChar),
            baseDirPath.Replace('\\', Path.DirectorySeparatorChar)
        );
    }
}
