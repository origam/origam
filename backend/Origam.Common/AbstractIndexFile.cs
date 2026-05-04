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
using System.Text;

namespace Origam;

public abstract class AbstractIndexFile : IDisposable
{
    private readonly string indexFile;
    private readonly FileStream fileStream;
    private bool disposed;

    protected AbstractIndexFile(string indexFile)
    {
        this.indexFile = indexFile;
        fileStream = File.Open(
            path: indexFile,
            mode: FileMode.OpenOrCreate,
            access: FileAccess.ReadWrite,
            share: FileShare.None
        );
    }

    public void AddEntryToIndexFile(string entry)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(
                objectName: "Dispose method has been already called and file is closed!"
            );
        }

        try
        {
            byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(
                s: entry + Environment.NewLine
            );
            fileStream.Seek(offset: 0, origin: SeekOrigin.End);
            fileStream.Write(array: bytes, offset: 0, count: bytes.Length);
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
    }

    private string ReadAllText()
    {
        fileStream.Seek(offset: 0, origin: SeekOrigin.Begin);
        byte[] bytes = new byte[fileStream.Length];
        int numBytesToRead = (int)fileStream.Length;
        int numBytesRead = 0;
        while (numBytesToRead > 0)
        {
            // Read may return anything from 0 to numBytesToRead.
            int n = fileStream.Read(array: bytes, offset: numBytesRead, count: numBytesToRead);
            // Break when the end of the file is reached.
            if (n == 0)
            {
                break;
            }

            numBytesRead += n;
            numBytesToRead -= n;
        }
        return Encoding.UTF8.GetString(bytes: bytes, index: 0, count: bytes.Length);
    }

    protected IEnumerable<string> ReadAllLines()
    {
        return ReadAllText()
            .Split(separator: '\n')
            .Select(selector: line => line.Trim())
            .Where(predicate: line => line != "");
    }

    public void Dispose()
    {
        if (!disposed)
        {
            fileStream?.Dispose();
            GC.SuppressFinalize(obj: this);
        }
        disposed = true;
    }

    ~AbstractIndexFile()
    {
        Dispose();
    }

    public abstract string GetFirstUnprocessedFile(string path, string mask);
}
