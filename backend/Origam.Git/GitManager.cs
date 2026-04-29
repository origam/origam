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
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace Origam.Git;

public class GitManager
{
    public Repository Repo { get; private set; }
    public string repositoryPath { get; private set; }
    public string CompareFile { get; private set; }
    private string pathOfgitConfig;
    private string patname = @"name\s*=\s*(.*)";
    private string patemail = @"email\s*=\s*(.*)";
    private static Dictionary<string, bool> gitPersistObjectsCache = new Dictionary<string, bool>();

    public GitManager(string path)
    {
        repositoryPath = Repository.Discover(startingPath: path);
        if (repositoryPath == null)
        {
            throw new Exception(message: "Not a valid git directory " + path);
        }
        Repo = new Repository(path: repositoryPath);
        InitValues();
    }

    public GitManager()
    {
        InitValues();
    }

    public void CloneRepository(
        string gitRepositoryLink,
        string modelFolder,
        string repositoryUsername,
        string repositoryPassword
    )
    {
        CloneOptions co = new CloneOptions();
        Credentials ca = new UsernamePasswordCredentials()
        {
            Username = repositoryUsername,
            Password = repositoryPassword,
        };
        co.FetchOptions.CredentialsProvider = (_url, _user, _cred) => ca;
        Repository.Clone(sourceUrl: gitRepositoryLink, workdirPath: modelFolder, options: co);
    }

    public bool IsValidUrl(string url, string gitUsername, string gitPassword)
    {
        try
        {
            var pushOptions = new PushOptions()
            {
                CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = gitUsername,
                        Password = gitPassword,
                    },
            };
            Repository.ListRemoteReferences(
                url: url,
                credentialsProvider: pushOptions.CredentialsProvider
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValid(string modelSourceControlLocation)
    {
        return Repository.Discover(startingPath: modelSourceControlLocation) != null;
    }

    public static string GetRepositoryPath(string modelSourceControlLocation)
    {
        return Repository.Discover(startingPath: modelSourceControlLocation);
    }

    public static void PersistPath(List<string> files)
    {
        foreach (string path in files)
        {
            gitPersistObjectsCache.Remove(key: path);
        }
    }

    public static Dictionary<string, bool> GetCache()
    {
        return gitPersistObjectsCache;
    }

    public static void RemoveRepository(string sourcesFolder)
    {
        string path = Path.Combine(path1: sourcesFolder, path2: ".git");
        DeleteDirectory(directoryPath: path);
    }

    private void InitValues()
    {
        pathOfgitConfig = Path.Combine(
            path1: Environment.GetEnvironmentVariable(variable: "HOMEDRIVE")
                + FixSlash(file: Environment.GetEnvironmentVariable(variable: "HOMEPATH")),
            path2: ".gitconfig"
        );
    }

    public static void CreateRepository(string modelSourceControlLocation)
    {
        Repository.Init(path: modelSourceControlLocation);
    }

    public void Init(string gitusername, string gitemail)
    {
        List<string> rules = new List<string> { "/index.bin", "scripts/" };
        Repo.Ignore.AddTemporaryRules(rules: rules);
        Commands.Stage(repository: Repo, path: "*");
        Signature author = new Signature(name: gitusername, email: gitemail, when: DateTime.Now);
        Signature committer = author;

        Repo.Commit(message: "Initial commit", author: author, committer: committer);
        CreateGitConfig(gitusername: gitusername, gitemail: gitemail);
    }

    private void CreateGitConfig(string gitusername, string gitemail)
    {
        if (!IsGitConfig())
        {
            string[] lines =
            {
                "[user]",
                "     name = " + gitusername,
                "     email = " + gitemail,
                "[credential]",
                "     helper = manager",
            };
            File.WriteAllLines(path: pathOfgitConfig, contents: lines);
        }
    }

    public Commit GetLastCommit()
    {
        return Repo.Head.Tip;
    }

    public bool HasChanges(string filePath) =>
        Repo.RetrieveStatus(filePath: filePath) != FileStatus.Unaltered;

    private string FixSlash(string file)
    {
        return file == null ? "" : file.Replace(oldValue: "\\", newValue: "/");
    }

    public string GetModifiedChanges()
    {
        return Repo.Diff.Compare<Patch>(paths: new List<string>() { CompareFile });
    }

    public string getCompareFileName()
    {
        return CompareFile.Split(separator: '/').LastOrDefault();
    }

    public void SetFile(string file)
    {
        CompareFile = FixSlash(file: file);
    }

    public bool IsGitConfig()
    {
        return File.Exists(path: pathOfgitConfig);
    }

    public string[] GitConfig()
    {
        string[] output = new string[2];
        if (IsGitConfig())
        {
            string gitFiletext = File.ReadAllText(path: pathOfgitConfig);
            Regex rname = new Regex(pattern: patname, options: RegexOptions.IgnoreCase);
            Regex remail = new Regex(pattern: patemail, options: RegexOptions.IgnoreCase);
            Match memail = remail.Match(input: gitFiletext);
            Match mname = rname.Match(input: gitFiletext);
            if (mname.Success)
            {
                output[0] = mname.Groups[groupnum: 1].Value;
                if (memail.Success)
                {
                    output[1] = memail.Groups[groupnum: 1].Value;
                    return output;
                }
            }
        }
        return null;
    }

    public static void DeleteDirectory(string directoryPath)
    {
        if (!Directory.Exists(path: directoryPath))
        {
            return;
        }
        var files = Directory.GetFiles(path: directoryPath);
        var directories = Directory.GetDirectories(path: directoryPath);
        foreach (var file in files)
        {
            // delete/clear hidden attribute
            File.SetAttributes(
                path: file,
                fileAttributes: File.GetAttributes(path: file) & ~FileAttributes.Hidden
            );
            // delete/clear archive and read only attributes
            File.SetAttributes(
                path: file,
                fileAttributes: File.GetAttributes(path: file)
                    & ~(FileAttributes.Archive | FileAttributes.ReadOnly)
            );
            File.Delete(path: file);
        }
        foreach (var dir in directories)
        {
            DeleteDirectory(directoryPath: dir);
        }
        File.SetAttributes(path: directoryPath, fileAttributes: FileAttributes.Normal);
        Directory.Delete(path: directoryPath, recursive: false);
    }
}
