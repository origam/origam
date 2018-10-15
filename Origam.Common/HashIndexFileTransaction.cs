using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Origam
{
    public class HashIndexFileTransaction : OrigamTransaction
    {
        private readonly string indexFile;
        private readonly List<string> processedFiles;
        private readonly HashIndexFile hashIndexFile;

        public HashIndexFileTransaction(
            string indexFile, List<string> processedFiles)
        {
            this.indexFile = indexFile;
            this.processedFiles = processedFiles;
            hashIndexFile = new HashIndexFile(indexFile);
        }
        
        public override void Commit()
        {
            try
            {
                CommitProcessedFiles();
            } 
            finally
            {
                hashIndexFile.Dispose();
            }
        }

        private void CommitProcessedFiles()
        {
            foreach (string filename in processedFiles)
            {
                string[] filenameSegments = filename.Split('|');
                if (filenameSegments.Length == 1)
                {
                    hashIndexFile.AddEntryToIndexFile(
                        hashIndexFile.CreateIndexFileEntry(
                            filenameSegments[0]));
                } else
                {
                    using (FileStream fileStream
                        = new FileStream(filenameSegments[0], FileMode.Open))
                    using (ZipArchive archive = new ZipArchive(fileStream))
                    {
                        hashIndexFile.AddEntryToIndexFile(
                            hashIndexFile.CreateIndexFileEntry(
                                filenameSegments[0],
                                archive.GetEntry(filenameSegments[1])));
                    }
                }
            }
        }

        public override void Rollback()
        {
        }
    }
}
