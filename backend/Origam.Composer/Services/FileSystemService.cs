#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Composer.Interfaces.Services;

namespace Origam.Composer.Services;

public class FileSystemService : IFileSystemService
{
    public void DeleteDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        var files = Directory.GetFiles(directoryPath);
        var directories = Directory.GetDirectories(directoryPath);
        foreach (var file in files)
        {
            // Delete hidden attribute + archive and read only attributes
            File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.Hidden);
            File.SetAttributes(
                file,
                File.GetAttributes(file) & ~(FileAttributes.Archive | FileAttributes.ReadOnly)
            );
            File.Delete(file);
        }

        foreach (var dir in directories)
        {
            DeleteDirectory(dir);
        }
        File.SetAttributes(directoryPath, FileAttributes.Normal);
        Directory.Delete(directoryPath, false);
    }
}
