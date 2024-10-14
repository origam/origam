using System;
using System.IO;
using System.Linq;

namespace Origam;

public static class IOTools
{
    public static bool IsSubPathOf(string path, string basePath)
    {
        if (Path.IsPathRooted(path) && Path.IsPathRooted(basePath) ||
            !Path.IsPathRooted(path) && !Path.IsPathRooted(basePath))
        {
            string fullPath = Normalize(path);
            string fullBasePath = Normalize(basePath);
            string[] baseDirNames =
                fullBasePath.Split(Path.DirectorySeparatorChar);
            string[] dirNames = fullPath
                .Split(Path.DirectorySeparatorChar);
            if (baseDirNames.Length > dirNames.Length) return false;
            return !baseDirNames
                .Where((dir, i) => dir != dirNames[i])
                .Any();
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
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar).Trim();
    }
}