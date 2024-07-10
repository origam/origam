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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Origam.DA.Service;
public class FileFilter
{
    private readonly IEnumerable<string> fileExtensionsToIgnore;
    private readonly IEnumerable<FileInfo> filesToIgnore;
    private readonly IEnumerable<string> directoryNamesToIgnore;
    public FileFilter(IEnumerable<string> fileExtensionsToIgnore, IEnumerable<FileInfo> filesToIgnore, IEnumerable<string> directoryNamesToIgnore)
    {
        this.fileExtensionsToIgnore = fileExtensionsToIgnore;
        this.filesToIgnore = filesToIgnore;
        this.directoryNamesToIgnore = directoryNamesToIgnore;
    }
    public bool ShouldPass(string fullPath)
    {
        return 
            !HasIgnoredExtension(fullPath) &&
            !IsIgnoredFile(fullPath) &&
            !IsInIgnoredDirectory(fullPath);
    }
    private bool HasIgnoredExtension(string fullPath)
    {
        string extension = Path.GetExtension(fullPath);
        if (extension.StartsWith("."))
        {
            extension = extension.Substring(1);
        }
        return fileExtensionsToIgnore.Any(ext => ext == extension);
    }
    private bool IsIgnoredFile(string fullPath)
    {
        return filesToIgnore.Any(f => f.FullName == fullPath);
    }
    private bool IsInIgnoredDirectory(string fullPath)
    {
        return fullPath
            .Split(Path.DirectorySeparatorChar)
            .Any(dirName => directoryNamesToIgnore.Contains(dirName));
    }
}
