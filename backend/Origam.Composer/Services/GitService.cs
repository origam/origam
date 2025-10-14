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
    private Repository? Repo;
    private readonly List<string> IgnoreRules = ["/index.bin", "scripts/"];
    private readonly string GitConfigPath;

    public GitService()
    {
        GitConfigPath = Path.Combine(
            Environment.GetEnvironmentVariable("HOMEDRIVE")
                + FixSlash(Environment.GetEnvironmentVariable("HOMEPATH")),
            ".gitconfig"
        );
    }

    public void CreateRepository(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(Strings.Cannot_be_null_or_empty, nameof(path));
        }
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (Repository.IsValid(path))
        {
            throw new InvalidOperationException(
                string.Format(Strings.Repository_already_exists, path)
            );
        }

        Repo?.Dispose();
        Repo = new Repository(Repository.Init(path));
        AnsiConsole.MarkupLine(string.Format(Strings.Repository_created, path));
    }

    public void InitCommit(string? username, string? userEmail)
    {
        if (Repo == null)
        {
            throw new InvalidOperationException(Strings.Repository_not_initialized);
        }
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException(Strings.Cannot_be_null_or_empty, nameof(username));
        }
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentException(Strings.Cannot_be_null_or_empty, nameof(userEmail));
        }

        Repo.Ignore.AddTemporaryRules(IgnoreRules);

        LibGit2Sharp.Commands.Stage(Repo, "*");

        var signature = new Signature(username, userEmail, DateTime.Now);
        Repo.Commit(Strings.Initial_commit, signature, signature);
        AnsiConsole.MarkupLine(string.Format(Strings.Init_commit_message, DateTime.Now));

        Configuration config = Repo.Config;
        config.Set("user.name", username);
        config.Set("user.email", userEmail);
        AnsiConsole.MarkupLine(string.Format(Strings.Git_config_set, username, userEmail));
    }

    public string[]? FetchGitUserFromGlobalConfig()
    {
        if (!File.Exists(GitConfigPath))
        {
            return null;
        }

        var output = new string[2];
        string gitFileText = File.ReadAllText(GitConfigPath);
        var nameRegex = new Regex(@"name\s*=\s*(.*)", RegexOptions.IgnoreCase);
        var emailRegex = new Regex(@"email\s*=\s*(.*)", RegexOptions.IgnoreCase);

        Match nameMatch = nameRegex.Match(gitFileText);
        Match emailMatch = emailRegex.Match(gitFileText);

        if (nameMatch.Success)
        {
            output[0] = nameMatch.Groups[1].Value;
            if (emailMatch.Success)
            {
                output[1] = emailMatch.Groups[1].Value;
                return output;
            }
        }
        return null;
    }

    private string FixSlash(string? file)
    {
        return file == null ? "" : file.Replace("\\", "/");
    }
}
