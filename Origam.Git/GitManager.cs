using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Origam.Git
{
    public class GitManager
    {
        public Repository Repo { get; private set; }
        public string repositoryPath { get; private set; }
        public string CompareFile { get; private set; }
        private string pathOfgitConfig ;
        private string patname = @"name\s*=\s*(.*)";
        private string patemail = @"email\s*=\s*(.*)";

        public GitManager(string repositoryPath)
        {
            Repo = new Repository(repositoryPath);
            this.repositoryPath = repositoryPath;
            InitValues();
        }
        public GitManager()
        {
            InitValues();
        }
        public static bool IsValid(string modelSourceControlLocation)
        {
            if (!File.Exists(Path.Combine(modelSourceControlLocation, ".git")))
            {
                return false;
            }
            return Repository.IsValid(modelSourceControlLocation);
        }
        private void InitValues()
        {
            pathOfgitConfig = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + 
                FixSlash(Environment.GetEnvironmentVariable("HOMEPATH")), ".gitconfig");
        }
        public static void CreateRepository(string modelSourceControlLocation)
        {
            Repository.Init(modelSourceControlLocation);
        }
        public void Init(string gitusername, string gitemail)
        {
            Commands.Stage(Repo, "*");
            Signature author = new Signature(gitusername, gitemail, DateTime.Now);
            Signature committer = author;
            Repo.Commit("Initial commit", author, committer);
            CreateGitConfig(gitusername,gitemail);
        }
        private void CreateGitConfig(string gitusername, string gitemail)
        {
            if(!IsGitConfig())
            {
                string[] lines = { "[user]", "     name = "+gitusername, "     email = " + gitemail, "[credential]", "     helper = manager" };
                File.WriteAllLines(pathOfgitConfig, lines);
            }
        }
        public Commit GetLastCommit()
        {
            return Repo.Head.Tip;         
        }
        public bool HasChanges(string filePath) 
            => Repo.RetrieveStatus(filePath) != FileStatus.Unaltered;
        private string FixSlash(string file)
        {
            return file==null?"":file.Replace("\\", "/");
        }
        public string GetModifiedChanges()
        {
            return Repo.Diff.Compare<Patch>(new List<string>() { CompareFile });
        }
        public string getCompareFileName()
        {
            return CompareFile.Split('/').LastOrDefault();
        }
        public void SetFile(string file)
        {
            CompareFile = FixSlash(file);
        }
        public bool IsGitConfig()
        {
            return File.Exists(pathOfgitConfig);
        }
        public string[] GitConfig()
        {
            string[] output = new string[2];
            if(IsGitConfig())
            {
                string  gitFiletext = File.ReadAllText(pathOfgitConfig);
                Regex rname = new Regex(patname, RegexOptions.IgnoreCase);
                Regex remail = new Regex(patemail, RegexOptions.IgnoreCase);
                Match memail = remail.Match(gitFiletext);
                Match mname = rname.Match(gitFiletext);
                if (mname.Success)
                {
                    output[0] = mname.Groups[1].Value;
                    if (memail.Success)
                    {
                        output[1] = memail.Groups[1].Value;
                        return output;
                    }
                }
            }
            return null;
        }
    }
}
