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

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Origam;

public class HashIndexFileTransaction : OrigamTransaction
{
    private readonly string indexFile;
    private readonly List<string> processedFiles;
    private readonly HashIndexFile hashIndexFile;

    public HashIndexFileTransaction(string indexFile, List<string> processedFiles)
    {
        this.indexFile = indexFile;
        this.processedFiles = processedFiles;
        hashIndexFile = new HashIndexFile(indexFile: indexFile);
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
            string[] filenameSegments = filename.Split(separator: '|');
            if (filenameSegments.Length == 1)
            {
                hashIndexFile.AddEntryToIndexFile(
                    entry: hashIndexFile.CreateIndexFileEntry(filename: filenameSegments[0])
                );
            }
            else
            {
                using (
                    FileStream fileStream = new FileStream(
                        path: filenameSegments[0],
                        mode: FileMode.Open
                    )
                )
                using (ZipArchive archive = new ZipArchive(stream: fileStream))
                {
                    hashIndexFile.AddEntryToIndexFile(
                        entry: hashIndexFile.CreateIndexFileEntry(
                            archiveName: filenameSegments[0],
                            archiveEntry: archive.GetEntry(entryName: filenameSegments[1])
                        )
                    );
                }
            }
        }
    }

    public override void Rollback() { }
}
