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

using System;
using System.IO;
using System.Linq;

namespace Origam;

public static class IOTools
{
    public static bool IsSubPathOf(string path, string basePath)
    {
        if (
            Path.IsPathRooted(path) && Path.IsPathRooted(basePath)
            || !Path.IsPathRooted(path) && !Path.IsPathRooted(basePath)
        )
        {
            string fullPath = Normalize(path);
            string fullBasePath = Normalize(basePath);
            string[] baseDirNames = fullBasePath.Split(Path.DirectorySeparatorChar);
            string[] dirNames = fullPath.Split(Path.DirectorySeparatorChar);
            if (baseDirNames.Length > dirNames.Length)
            {
                return false;
            }
            return !baseDirNames.Where((dir, i) => dir != dirNames[i]).Any();
        }
        else
        {
            // The paths cannot be really compared.
            return false;
        }
    }

    private static string Normalize(string path)
    {
        if (path.Length == 2 && Char.IsLetter(path[0]) && path[1] == ':')
        {
            return path;
        }
        // The paths have to be converted to full paths to deal with the /../ relative navigation
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar).Trim();
    }
}
