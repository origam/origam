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

using System.Text.RegularExpressions;
using LibGit2Sharp;
using Origam.Composer.Interfaces.Services;
using Spectre.Console;

namespace Origam.Composer.Services;

public class GitService : IGitService
{
    private Repository Repo;
    private readonly List<string> IgnoreRules = ["/index.bin", "scripts/"];
    private readonly string GitConfigPath;

    public GitService()
    {
        GitConfigPath = Path.Combine(
            path1: Environment.GetEnvironmentVariable(variable: "HOMEDRIVE")
                + FixSlash(file: Environment.GetEnvironmentVariable(variable: "HOMEPATH")),
            path2: ".gitconfig"
        );
    }

    public void CreateRepository(string path)
    {
        if (string.IsNullOrWhiteSpace(value: path))
        {
            throw new ArgumentException(
                message: Strings.Cannot_be_null_or_empty,
                paramName: nameof(path)
            );
        }
        if (!Directory.Exists(path: path))
        {
            Directory.CreateDirectory(path: path);
        }
        if (Repository.IsValid(path: path))
        {
            throw new InvalidOperationException(
                message: string.Format(format: Strings.Repository_already_exists, arg0: path)
            );
        }

        Repo?.Dispose();
        Repo = new Repository(path: Repository.Init(path: path));
        AnsiConsole.MarkupLine(
            value: string.Format(format: Strings.Repository_created, arg0: path)
        );
    }

    public void InitCommit(string username, string userEmail)
    {
        if (Repo == null)
        {
            throw new InvalidOperationException(message: Strings.Repository_not_initialized);
        }
        if (string.IsNullOrWhiteSpace(value: username))
        {
            throw new ArgumentException(
                message: Strings.Cannot_be_null_or_empty,
                paramName: nameof(username)
            );
        }
        if (string.IsNullOrWhiteSpace(value: userEmail))
        {
            throw new ArgumentException(
                message: Strings.Cannot_be_null_or_empty,
                paramName: nameof(userEmail)
            );
        }

        Repo.Ignore.AddTemporaryRules(rules: IgnoreRules);

        LibGit2Sharp.Commands.Stage(repository: Repo, path: "*");

        var signature = new Signature(name: username, email: userEmail, when: DateTime.Now);
        Repo.Commit(message: Strings.Initial_commit, author: signature, committer: signature);
        AnsiConsole.MarkupLine(
            value: string.Format(format: Strings.Init_commit_message, arg0: DateTime.Now)
        );

        Configuration config = Repo.Config;
        config.Set(key: "user.name", value: username);
        config.Set(key: "user.email", value: userEmail);
        AnsiConsole.MarkupLine(
            value: string.Format(format: Strings.Git_config_set, arg0: username, arg1: userEmail)
        );
    }

    public string[] FetchGitUserFromGlobalConfig()
    {
        if (!File.Exists(path: GitConfigPath))
        {
            return null;
        }

        var output = new string[2];
        string gitFileText = File.ReadAllText(path: GitConfigPath);
        var nameRegex = new Regex(pattern: @"name\s*=\s*(.*)", options: RegexOptions.IgnoreCase);
        var emailRegex = new Regex(pattern: @"email\s*=\s*(.*)", options: RegexOptions.IgnoreCase);

        Match nameMatch = nameRegex.Match(input: gitFileText);
        Match emailMatch = emailRegex.Match(input: gitFileText);

        if (nameMatch.Success)
        {
            output[0] = nameMatch.Groups[groupnum: 1].Value;
            if (emailMatch.Success)
            {
                output[1] = emailMatch.Groups[groupnum: 1].Value;
                return output;
            }
        }
        return null;
    }

    private string FixSlash(string file)
    {
        return file == null ? "" : file.Replace(oldValue: "\\", newValue: "/");
    }
}
