using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using log4net;
using System.Reflection;

namespace Origam
{
   public class HashIndexFile: AbstractIndexFile
    {
		private static readonly ILog log = LogManager.GetLogger(
			MethodBase.GetCurrentMethod().DeclaringType);

        public HashIndexFile(string indexFile) : base(indexFile)
        {
        }
        
		public override string GetFirstUnprocessedFile(string path, string mask)
        {
            HashSet<string> processedFileHashes
				= new HashSet<string>(GetProcessedFileHashes());
            return Directory.GetFiles(path, mask)
                .FirstOrDefault(filename =>
                    !processedFileHashes.Contains(GetHash(filename)));
        }
        
        public IEnumerable<string> GetProcessedFileHashes()
        {
            return ReadAllLines()
                .Select(line => line.Split('|')[0]);
        }

        public string CreateIndexFileEntry(string filename)
        {
            return string.Format("{0}|{1}|{2:yyyyMMddHHmmss}", 
                GetHash(filename), filename, DateTime.Now);
        }
        public string CreateIndexFileEntry(
            string archiveName, ZipArchiveEntry archiveEntry)
        {
            return string.Format("{0}|{1}|{2}|{3:yyyyMMddHHmmss}", 
                GetHash(archiveEntry), archiveName, archiveEntry.FullName, 
                DateTime.Now);
        }
        
        private string GetHash(string filename)
        {
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("Starting to compute hash for file {0}", filename);
			}
            using (FileStream fileStream = new FileStream(
                filename, FileMode.Open))
            {
                return GetHash(fileStream);
            }
        }
        
        private string GetHash(ZipArchiveEntry archiveEntry)
        {
            using (Stream stream = archiveEntry.Open())
            {
                return GetHash(stream);
            }
        }
        private string GetHash(Stream stream)
        {
            using (BufferedStream bufferedStream = new BufferedStream(
                stream))
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                byte[] hash = sha1.ComputeHash(bufferedStream);
                StringBuilder output = new StringBuilder(2 * hash.Length);
                foreach (byte b in hash)
                {
                    output.AppendFormat("{0:X2}", b);
                }
                return output.ToString();
            }
        }

        public bool IsFileProcessed(string filename)
        {
            string fileHash = GetHash(filename);
            return GetProcessedFileHashes()
                .Any(hash => hash == fileHash);
        }
        public bool IsZipArchiveEntryProcessed(ZipArchiveEntry archiveEntry)
        {
            string fileHash = GetHash(archiveEntry);
            return GetProcessedFileHashes()
                .Any(hash => hash == fileHash);
        }
    }
}
