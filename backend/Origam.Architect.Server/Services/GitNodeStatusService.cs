#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Collections.Concurrent;
using System.Linq;
using LibGit2Sharp;
using Origam.DA.ObjectPersistence;

namespace Origam.Architect.Server.Services;

public class GitNodeStatusService
{
    private readonly string sourcePath;
    private readonly string repoPath;
    private readonly string workingDirectory;
    private readonly bool gitSupported;
    private readonly ConcurrentDictionary<string, bool> dirtyCache = new();

    public GitNodeStatusService()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        sourcePath = settings?.ModelSourceControlLocation;
        if (string.IsNullOrEmpty(sourcePath))
        {
            gitSupported = false;
            return;
        }
        repoPath = Repository.Discover(sourcePath);
        if (repoPath == null)
        {
            gitSupported = false;
            return;
        }
        using var repo = new Repository(repoPath);
        workingDirectory = repo.Info.WorkingDirectory;
        gitSupported = true;
    }

    public bool IsFileDirty(IPersistent item)
    {
        if (!gitSupported || item?.Files == null || item.Files.Count == 0)
        {
            return false;
        }
        string cacheKey = NormalizePath(item.Files.First());
        if (dirtyCache.TryGetValue(cacheKey, out bool status))
        {
            return status;
        }
        try
        {
            using var repo = new Repository(repoPath);
            foreach (string absolutePath in item.Files.Select(file => Path.Combine(sourcePath, file)))
            {
                if (!File.Exists(absolutePath))
                {
                    continue;
                }
                string relativeToRepo = Path.GetRelativePath(workingDirectory, absolutePath);
                string gitPath = NormalizePath(relativeToRepo);
                FileStatus fileStatus = repo.RetrieveStatus(gitPath);
                if (fileStatus != FileStatus.Unaltered && fileStatus != FileStatus.Nonexistent)
                {
                    dirtyCache[cacheKey] = true;
                    return true;
                }
            }
        }
        catch (LibGit2SharpException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        dirtyCache[cacheKey] = false;
        return false;
    }

    public void InvalidateCache(IEnumerable<string> files)
    {
        if (files == null)
        {
            return;
        }
        foreach (string file in files)
        {
            dirtyCache.TryRemove(NormalizePath(file), out _);
        }
    }

    public void ClearCache()
    {
        dirtyCache.Clear();
    }

    private static string NormalizePath(string path)
    {
        return path?.Replace(oldChar: '\\', newChar: '/');
    }
}
