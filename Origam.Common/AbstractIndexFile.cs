using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Origam
{
    public abstract class AbstractIndexFile:  IDisposable
    {
        private readonly string indexFile;
        private readonly FileStream fileStream;
        private bool disposed;

        protected AbstractIndexFile(string indexFile)
        {
            this.indexFile = indexFile;
            fileStream = File.Open(indexFile, FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None);
        }

        public void AddEntryToIndexFile(string entry)
        {
            if (disposed) throw new ObjectDisposedException("Dispose method has been already called and file is closed!");
            try
            {
                byte[] bytes = new UTF8Encoding(true)
                    .GetBytes(entry + Environment.NewLine);
                fileStream.Seek(0, SeekOrigin.End);
                fileStream.Write(
                    array: bytes, 
                    offset: 0, 
                    count: bytes.Length);
            } 
            catch(Exception)
            {
                Dispose();
                throw;
            }
        }
    
        private string ReadAllText()
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[fileStream.Length];
            int numBytesToRead = (int)fileStream.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = fileStream.Read(bytes, numBytesRead, numBytesToRead);

                // Break when the end of the file is reached.
                if (n == 0)
                    break;

                numBytesRead += n;
                numBytesToRead -= n;
            }
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        protected IEnumerable<string> ReadAllLines()
        {
            return ReadAllText()
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line != "");
        }
    
        public void Dispose()
        {
            if (!disposed)
            {
                fileStream?.Dispose();
                GC.SuppressFinalize(this);
            }
            disposed = true;
        }
    
        ~AbstractIndexFile() 
        {
            Dispose();
        }
    
        public abstract string GetFirstUnprocessedFile(string path, string mask);
    }
}
