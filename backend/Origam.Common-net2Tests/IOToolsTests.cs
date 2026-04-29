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
    [TestCase(arg1: @"c:\foo", arg2: @"c:\", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo", arg2: @"c:\foo", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo", arg2: @"c:\foo\", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo\", arg2: @"c:\foo", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo\bar\", arg2: @"c:\foo\", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo\bar", arg2: @"c:\foo\", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo\a.txt", arg2: @"c:\foo", ExpectedResult = true)]
    [TestCase(arg1: @"c:\FOO\a.txt", arg2: @"c:\foo", ExpectedResult = false)]
    [TestCase(arg1: @"c:/foo/a.txt", arg2: @"c:\foo", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foobar", arg2: @"c:\foo", ExpectedResult = false)]
    [TestCase(arg1: @"c:\foobar\a.txt", arg2: @"c:\foo", ExpectedResult = false)]
    [TestCase(arg1: @"c:\foobar\a.txt", arg2: @"c:\foo\", ExpectedResult = false)]
    [TestCase(arg1: @"c:\foo\a.txt", arg2: @"c:\foobar", ExpectedResult = false)]
    [TestCase(arg1: @"c:\foo\a.txt", arg2: @"c:\foobar\", ExpectedResult = false)]
    [TestCase(arg1: @"c:\foo\..\bar\baz", arg2: @"c:\foo", ExpectedResult = false)]
    [TestCase(arg1: @"c:\foo\..\bar\baz", arg2: @"c:\bar", ExpectedResult = true)]
    [TestCase(arg1: @"c:\foo\..\bar\baz", arg2: @"c:\barr", ExpectedResult = false)]
    [TestCase(arg1: @"\foo", arg2: @"\foo", ExpectedResult = true)]
    [TestCase(arg1: @"\foo", arg2: @"\foo\", ExpectedResult = true)]
    [TestCase(arg1: @"\foo\", arg2: @"\foo", ExpectedResult = true)]
    [TestCase(arg1: @"\foo\bar\", arg2: @"\foo\", ExpectedResult = true)]
    [TestCase(arg1: @"\foo\bar", arg2: @"\foo\", ExpectedResult = true)]
    [TestCase(arg1: @"\foo\a.txt", arg2: @"\foo", ExpectedResult = true)]
    [TestCase(arg1: @"\FOO\a.txt", arg2: @"\foo", ExpectedResult = false)]
    [TestCase(arg1: @"/foo/a.txt", arg2: @"\foo", ExpectedResult = true)]
    [TestCase(arg1: @"\foobar", arg2: @"\foo", ExpectedResult = false)]
    [TestCase(arg1: @"\foobar\a.txt", arg2: @"\foo", ExpectedResult = false)]
    [TestCase(arg1: @"\foobar\a.txt", arg2: @"\foo\", ExpectedResult = false)]
    [TestCase(arg1: @"\foo\a.txt", arg2: @"\foobar", ExpectedResult = false)]
    [TestCase(arg1: @"\foo\a.txt", arg2: @"\foobar\", ExpectedResult = false)]
    [TestCase(arg1: @"\foo\..\bar\baz", arg2: @"\foo", ExpectedResult = false)]
    [TestCase(arg1: @"\foo\..\bar\baz", arg2: @"\bar", ExpectedResult = true)]
    [TestCase(arg1: @"\foo\..\bar\baz", arg2: @"\barr", ExpectedResult = false)]
    public bool IsSubPathOfTest(string path, string baseDirPath)
    {
        return IOTools.IsSubPathOf(
            path: path.Replace(oldChar: '\\', newChar: Path.DirectorySeparatorChar),
            basePath: baseDirPath.Replace(oldChar: '\\', newChar: Path.DirectorySeparatorChar)
        );
    }
}
