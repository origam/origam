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
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using log4net;
using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueIncrementalFileLoader : WorkQueueLoaderAdapter
{
    private static readonly ILog log = LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );
    private string transactionId;
    private bool isLocalTransaction = false;
    private string path = null;
    private string indexFile = null;
    private string searchPattern = null;
    private bool compressedArchivesAsSubfolders = false;
    private readonly List<string> filesToProcess = new List<string>();
    private int position = 0;

    public override void Connect(
        IWorkQueueService service,
        Guid queueId,
        string workQueueClass,
        string connection,
        string userName,
        string password,
        string transactionId
    )
    {
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Connecting " + connection);
        }
        ParseParameters(connection: connection);
        InitializeFileList();
        SetupTransaction(transactionId: transactionId, connection: connection);
    }

    public override void Disconnect()
    {
        if (isLocalTransaction)
        {
            ResourceMonitor.Commit(transactionId: transactionId);
        }
    }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if (filesToProcess.Count == position)
        {
            return null;
        }
        string filename = filesToProcess[index: position];
        DataTable dataTable = CreateFileDataset();
        DataRow row = dataTable.NewRow();
        row[columnName: "Name"] = filename;
        row[columnName: "Data"] = GetFileContent(filename: filename);
        dataTable.Rows.Add(row: row);
        position++;
        return new WorkQueueAdapterResult(
            document: DataDocumentFactory.New(dataSet: dataTable.DataSet)
        );
    }

    private string GetFileContent(string filename)
    {
        string[] filenameSegments = filename.Split(separator: '|');
        if (filenameSegments.Length == 1)
        {
            // Non-zip file: enforce maximum size before reading
            long maxUncompressedBytes = WorkQueueConfig.GetMaxUncompressedBytes();
            string path = filenameSegments[0];
            FileInfo fi = new FileInfo(fileName: path);
            if (fi.Exists && fi.Length > maxUncompressedBytes)
            {
                throw new InvalidOperationException(message: Strings.StreamExceedsAllowedSize);
            }
            string content = File.ReadAllText(path: path);
            if (content.Length > maxUncompressedBytes)
            {
                throw new InvalidOperationException(message: Strings.StreamExceedsAllowedSize);
            }
            return content;
        }

        return GetContentFromZipArchive(
            archiveName: filenameSegments[0],
            filename: filenameSegments[1]
        );
    }

    private string GetContentFromZipArchive(string archiveName, string filename)
    {
        // Harden against zip-bomb and invalid entry issues
        long maxUncompressedBytes = WorkQueueConfig.GetMaxUncompressedBytes();

        using (
            FileStream fileStream = new FileStream(
                path: archiveName,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.Read
            )
        )
        using (
            ZipArchive archive = new ZipArchive(
                stream: fileStream,
                mode: ZipArchiveMode.Read,
                leaveOpen: false
            )
        )
        {
            // Zip spec uses forward slashes for separators; normalize for lookup
            string normalizedEntryName = filename.Replace(oldChar: '\\', newChar: '/');
            ZipArchiveEntry entry = archive.GetEntry(entryName: normalizedEntryName);
            if (entry == null)
            {
                throw new InvalidOperationException(
                    message: string.Format(
                        format: Strings.EntryNotFoundInArchive,
                        arg0: filename,
                        arg1: archiveName
                    )
                );
            }

            // Guard against zip bombs: excessive uncompressed size or ratio
            if (entry.Length > maxUncompressedBytes)
            {
                throw new InvalidOperationException(message: Strings.ArchiveEntryTooLarge);
            }
            if (
                entry.CompressedLength > 0
                && (entry.Length / (double)entry.CompressedLength)
                    > WorkQueueConfig.GetMaxCompressionRatio()
            )
            {
                throw new InvalidOperationException(
                    message: Strings.ArchiveEntrySuspiciousCompressionRatio
                );
            }

            using (Stream stream = entry.Open())
            using (StreamReader streamReader = new StreamReader(stream: stream))
            {
                string content = streamReader.ReadToEnd();
                if (content.Length > maxUncompressedBytes)
                {
                    throw new InvalidOperationException(
                        message: ResourceUtils.GetString(key: "StreamExceedsAllowedSize")
                    );
                }
                return content;
            }
        }
    }

    private void SetupTransaction(string transactionId, string connection)
    {
        this.transactionId = transactionId;
        if (this.transactionId == null)
        {
            this.transactionId = Guid.NewGuid().ToString();
            isLocalTransaction = true;
        }
        HashIndexFileTransaction transaction =
            ResourceMonitor.GetTransaction(
                transactionId: transactionId,
                resourceManagerId: connection
            ) as HashIndexFileTransaction;
        if (transaction == null)
        {
            ResourceMonitor.RegisterTransaction(
                transactionId: transactionId,
                resourceManagerId: connection,
                transaction: new HashIndexFileTransaction(
                    indexFile: indexFile,
                    processedFiles: filesToProcess
                )
            );
        }
    }

    private void ParseParameters(string connection)
    {
        string[] cnParts = connection.Split(separator: ";".ToCharArray());
        foreach (string part in cnParts)
        {
            string[] pair = part.Split(separator: "=".ToCharArray());
            if (pair.Length == 2)
            {
                switch (pair[0])
                {
                    case "path":
                    {
                        path = pair[1];
                        break;
                    }

                    case "indexFile":
                    {
                        indexFile = pair[1];
                        break;
                    }

                    case "searchPattern":
                    {
                        searchPattern = pair[1];
                        break;
                    }

                    case "compressedArchivesAsSubfolders":
                    {
                        compressedArchivesAsSubfolders = Convert.ToBoolean(value: pair[1]);
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "connectionParameterName",
                            actualValue: pair[0],
                            message: Strings.ErrorInvalidConnectionString
                        );
                    }
                }
            }
        }
        if (path == null)
        {
            throw new Exception(message: Strings.ErrorNoPath);
        }
        if (indexFile == null)
        {
            throw new Exception(message: Strings.ErrorNoIndexFile);
        }
        if (searchPattern == null)
        {
            throw new Exception(message: Strings.ErrorNoSearchPattern);
        }
    }

    private void InitializeFileList()
    {
        using (HashIndexFile hashIndexFile = new HashIndexFile(indexFile: indexFile))
        {
            if (compressedArchivesAsSubfolders)
            {
                AddUnprocessedZipArchivesToFileList(hashIndexFile: hashIndexFile);
            }
            AddUnprocessedFilesToFileList(hashIndexFile: hashIndexFile);
        }
    }

    private void AddUnprocessedFilesToFileList(HashIndexFile hashIndexFile)
    {
        string[] filenames = Directory.GetFiles(path: path, searchPattern: searchPattern);
        foreach (string filename in filenames)
        {
            if (!hashIndexFile.IsFileProcessed(filename: filename))
            {
                filesToProcess.Add(item: filename);
            }
        }
    }

    private void AddUnprocessedZipArchivesToFileList(HashIndexFile hashIndexFile)
    {
        string[] filenames = Directory.GetFiles(path: path, searchPattern: "*.zip");
        foreach (string filename in filenames)
        {
            using (FileStream fileStream = new FileStream(path: filename, mode: FileMode.Open))
            using (ZipArchive archive = new ZipArchive(stream: fileStream))
            {
                string fullDestinationDirPath = Path.GetFullPath(
                    path: path + Path.DirectorySeparatorChar
                );
                foreach (ZipArchiveEntry archiveEntry in archive.Entries)
                {
                    string destinationFileName = Path.GetFullPath(
                        path: Path.Combine(path1: path, path2: archiveEntry.FullName)
                    );
                    var comparison = RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows)
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;
                    if (
                        !destinationFileName.StartsWith(
                            value: fullDestinationDirPath,
                            comparisonType: comparison
                        )
                    )
                    {
                        throw new InvalidOperationException(
                            message: string.Format(
                                format: ResourceUtils.GetString(key: "EntryOutsideTargetDir"),
                                arg0: destinationFileName
                            )
                        );
                    }

                    string sanitizedEntryName = destinationFileName.Substring(
                        startIndex: fullDestinationDirPath.Length
                    );

                    if (
                        FitsMask(filename: archiveEntry.Name)
                        && !hashIndexFile.IsZipArchiveEntryProcessed(archiveEntry: archiveEntry)
                    )
                    {
                        filesToProcess.Add(item: filename + "|" + sanitizedEntryName);
                    }
                }
            }
        }
    }

    private bool FitsMask(string filename)
    {
        Regex mask = new Regex(
            pattern: '^'
                + searchPattern
                    .Replace(oldValue: ".", newValue: "[.]")
                    .Replace(oldValue: "*", newValue: ".*")
                    .Replace(oldValue: "?", newValue: ".")
                + '$',
            options: RegexOptions.IgnoreCase
        );
        return mask.IsMatch(input: filename);
    }

    private DataTable CreateFileDataset()
    {
        DataSet dataSet = new DataSet(dataSetName: "ROOT");
        DataTable dataTable = dataSet.Tables.Add(name: "File");
        dataTable.Columns.Add(columnName: "Name", type: typeof(string));
        dataTable.Columns.Add(columnName: "Data", type: typeof(string));
        // Add file metadata (times)
        dataTable.Columns.Add(columnName: "CreationTime", type: typeof(DateTime));
        dataTable.Columns.Add(columnName: "LastWriteTime", type: typeof(DateTime));
        dataTable.Columns.Add(columnName: "LastAccessTime", type: typeof(DateTime));
        return dataTable;
    }
}
