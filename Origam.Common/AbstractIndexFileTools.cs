using System;
using System.IO;

namespace Origam
{
    public class AbstractIndexFileTools<T> where T : IndexFileTools, new()
    {
        public static T GetInstance()
        {
            return new T();
        }

        public void AddEntryToIndexFile(string indexFile, string entry)
        {
            File.AppendAllText(indexFile, entry + Environment.NewLine);
        }
    }
}
