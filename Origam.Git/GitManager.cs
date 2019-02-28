using LibGit2Sharp;
using System.Collections.Generic;
using System.Linq;
namespace Origam.Git
{
    public class GitManager
    {
        public Repository Repo { get; private set; }
        public string CompareFile { get; private set; }
        public GitManager(string repositoryPath)
        {
            Repo = new Repository(repositoryPath);
        }
        public static bool IsValid(string modelSourceControlLocation)
        {
            return Repository.IsValid(modelSourceControlLocation);
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
    }
}
