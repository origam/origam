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
using System.Reflection;

namespace Origam.Git;

public static class IgnoreFileTools
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );

    public static void TryAdd(string ignoreFileDir, string ignoreFileEntry)
    {
        string pathToIgnoreFile = Path.Combine(path1: ignoreFileDir, path2: ".gitignore");
        try
        {
            var lines = File.Exists(path: pathToIgnoreFile)
                ? File.ReadAllLines(path: pathToIgnoreFile).ToList()
                : new List<string>();
            if (lines.Any(predicate: line => line.Trim() == ignoreFileEntry))
            {
                return;
            }
            lines.Add(item: ignoreFileEntry);
            File.WriteAllLines(path: pathToIgnoreFile, contents: lines);
        }
        catch (IOException ex)
        {
            log.Warn(message: $"Could not write to \"{pathToIgnoreFile}\"", exception: ex);
        }
    }
}
