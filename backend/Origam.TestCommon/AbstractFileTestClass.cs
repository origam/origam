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

using System.IO;
using NUnit.Framework;

namespace Origam.TestCommon;

public abstract class AbstractFileTestClass
{
    protected DirectoryInfo ProjectDir => new DirectoryInfo(TestContext.TestDirectory);
    protected virtual string DirName => "";
    protected abstract TestContext TestContext { get; }
    protected DirectoryInfo TestFilesDir
    {
        get
        {
            string path = Path.Combine(ProjectDir.FullName, DirName, "TestFiles");
            Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }
    }
    protected DirectoryInfo TestProjectDir
    {
        get
        {
            string relativeToFilesDir = DirName + @"\TestProject";

            string path = Path.Combine(ProjectDir.FullName, relativeToFilesDir);
            Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }
    }

    protected void ClearTestDir()
    {
        Directory.Delete(TestFilesDir.FullName, true);
    }
}
