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
using LibGit2Sharp;
using Origam.Extensions;

namespace Origam.Git;

public class GitFileComparer
{
    private readonly Signature autor;
    private readonly Signature committer;
    private readonly string internalFileName;
    private Repository repo;
    private readonly DirectoryInfo repoDir;

    public GitFileComparer()
    {
            autor = new Signature("GitFileComparer", "@GitFileComparer",
                DateTime.Now);
            committer = this.autor;
            internalFileName = "fileToCommit.txt";
            repoDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "GitCompare"));
        }

    public GitDiff GetGitDiff(FileInfo oldFile, FileInfo newFile)
    {
            if (FilesAreIdentical(newFile, oldFile))
            {
                return new GitDiff(oldFile, newFile, "");
            }
            
            InitRepo();

            string internalFilePath = Path.Combine(repo.Info.WorkingDirectory,
                internalFileName);
            
            File.Copy(oldFile.FullName, internalFilePath);

            Commit("Old");

            string fileSysXmlText = File.ReadAllText(newFile.FullName);
            File.WriteAllText(internalFilePath, fileSysXmlText);

            Commit("New");

            string diff = GetDiff();
            repoDir.DeleteAllIncludingReadOnly();
            repo.Dispose();
            return new GitDiff(oldFile, newFile, diff);
        }

    private bool FilesAreIdentical(FileInfo oldFile, FileInfo newFile)
    {
            return oldFile.GetFileBase64Hash() == newFile.GetFileBase64Hash();
        }

    private string GetDiff()
    {
            List<Commit> CommitList = repo.Commits.QueryBy(internalFileName)
                .Select(entry => entry.Commit)
                .ToList();

            int ChangeDesired = 0; // Change difference desired
            Patch repoDifferences = repo.Diff.Compare<Patch>(
                (Equals(CommitList[ChangeDesired + 1], null))
                    ? null
                    : CommitList[ChangeDesired + 1].Tree,
                (Equals(CommitList[ChangeDesired], null))
                    ? null
                    : CommitList[ChangeDesired].Tree);
            return repoDifferences.First(e => e.Path == internalFileName).Patch;
        }

    private void Commit(string message)
    {
            repo.Index.Add(internalFileName);
            repo.Commit(message, autor, committer);
        }

    private void InitRepo()
    {
            repoDir.DeleteAllIncludingReadOnly();
            repoDir.Create();
            Repository.Init(repoDir.FullName);
            repo = new Repository(repoDir.FullName);
        }
}

public class GitDiff
{
    public FileInfo OldFile { get; }
    public FileInfo NewFile { get;  }
    public string Text { get; }
    public bool IsEmpty => string.IsNullOrEmpty(Text);

    public GitDiff(FileInfo oldFile, FileInfo newFile, string text)
    {
            OldFile = oldFile;
            NewFile = newFile;
            Text = text;
        }
}