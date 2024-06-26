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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Origam.Extensions;
public static class IOExtensions
{
    public static byte[] GetFileHash(this FileInfo fileInfo)
    {
        using (MD5 md5 = MD5.Create())
        {
            Exception lastException = null;
            Stream stream = null;
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    stream = fileInfo.OpenRead();
                    return md5.ComputeHash(stream);
                }
                catch (IOException ex)
                {
                    lastException = ex;
                    Thread.Sleep(100);
                }
                finally
                {
                    stream?.Dispose();
                }
            }
            throw new Exception("Could not get hash of: "+fileInfo,
                lastException);
        }
    }
    public static bool ExistsNow(this FileInfo file)
    {
        return File.Exists(file.FullName);
    }
    public static string GetFileBase64Hash(this FileInfo fileInfo)
    {
        return Convert.ToBase64String(GetFileHash(fileInfo));
    }
    public static IEnumerable<DirectoryInfo> GetAllSubDirectories(
        this DirectoryInfo directory)
    {
        foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
        {
            yield return directoryInfo;
            foreach (DirectoryInfo subDirInfo in directoryInfo
                .GetAllSubDirectories())
            {
                yield return subDirInfo;
            }
        }
    }
    public static bool DoesNotContain(this DirectoryInfo directory, string fileName)
    {
        return !directory.Contains(fileName);
    }
    public static bool Contains(this DirectoryInfo directory, string fileName)
    {
        return directory.Contains(file => file.Name == fileName);
    }
    public static bool Contains(this DirectoryInfo directory, Func<FileInfo,bool> predicate)
    {
        return directory
            .GetFiles()
            .Any(predicate);
    }
    public static IEnumerable<FileInfo> GetAllFilesInSubDirectories(
        this DirectoryInfo directory)
    {
        foreach (FileInfo fileInfo in directory.GetFiles())
        {
            yield return fileInfo;
        }
        foreach (DirectoryInfo subDirinfo in
            directory.GetAllSubDirectories())
        {
            foreach (FileInfo fileInfo in subDirinfo.GetFiles())
            {
                yield return fileInfo;
            }
        }
    }
    public static bool IsOnPathOf(this DirectoryInfo thisDirInfo,
        DirectoryInfo other)
    {
        return IsOnPathOf(thisDirInfo.FullName, other.FullName);
    }
    public static bool IsOnPathOf(this DirectoryInfo thisDirInfo,
        string otherPath)
    {
        return IsOnPathOf(thisDirInfo.FullName, otherPath);
    }
    private static bool IsOnPathOf(string path,
        string otherPath)
    {
        string[] otherDirNames =
            otherPath.Split(Path.DirectorySeparatorChar);
        string[] thisDirNames = path
            .Split(Path.DirectorySeparatorChar);
        if (thisDirNames.Length > otherDirNames.Length) return false;
        return !thisDirNames
            .Where((dir, i) => dir != otherDirNames[i])
            .Any();
    }
    public static void DeleteAllIncludingReadOnly(this DirectoryInfo dir)
    {
        if (!dir.Exists) return;
        foreach (FileInfo file in dir.GetAllFilesInSubDirectories())
        {
            File.SetAttributes(file.FullName, FileAttributes.Normal);
        }
        
        dir.Attributes = dir.Attributes & ~FileAttributes.ReadOnly;
        dir.Delete(true);
    }
    public static FileInfo MakeNew( this FileInfo file, string newExtension)
    {
        int extensionLength = file.Extension.Length;
        int fullNameLength = file.FullName.Length;
        string baseName = file.FullName.Substring(0,
            fullNameLength - extensionLength);
        return new FileInfo(baseName + "." + newExtension);  
    }
}
