using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Origam
{
    public class FilenameIndexFileTools 
        : AbstractIndexFileTools<FilenameIndexFileTools>, IndexFileTools
    {
        public string GetFirstUnprocessedFile(
            string path, string mask, string indexFile)
        {
			string[] filenames = Directory.GetFiles(path, mask);
            List<string> processedFiles = GetProcessedFiles(indexFile);
            foreach(string filename in filenames)
            {
                if(!processedFiles.Any(str => str == filename))
                {
                    return filename;
                }
            }
            return null;
        }
        private List<string> GetProcessedFiles(string indexFile)
        {
            if(File.Exists(indexFile))
            {
                return File.ReadAllLines(indexFile).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
