using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Origam.Git
{
    public static class IgnoreFileTools
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void TryAdd(string ignoreFileDir, string ignoreFileEntry)
        {
            string pathToIgnoreFile =
                Path.Combine(ignoreFileDir, ".gitignore");
            try
            {
                var lines = File.Exists(pathToIgnoreFile)
                    ? File.ReadAllLines(pathToIgnoreFile).ToList()
                    : new List<string>();
                if (lines.Any(line => line.Trim() == ignoreFileEntry))
                {
                    return;
                }

                lines.Add(ignoreFileEntry);
                File.WriteAllLines(pathToIgnoreFile, lines);
            }
            catch (IOException ex)
            {
                log.Warn($"Could not write to \"{pathToIgnoreFile}\"", ex); 
            }
        }
    }
}