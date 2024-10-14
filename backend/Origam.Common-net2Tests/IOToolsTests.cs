using System.IO;
using NUnit.Framework;

namespace Origam.Common_net2Tests;


[TestFixture]
public class IOToolsTests
{
    [TestCase(@"c:\foo", @"c:", ExpectedResult = true)]
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