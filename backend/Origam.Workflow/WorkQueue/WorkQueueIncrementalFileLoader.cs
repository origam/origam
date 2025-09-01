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

using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using Origam.Workbench.Services;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using System.Runtime.InteropServices;
using Origam.Config;

namespace Origam.Workflow.WorkQueue;
public class WorkQueueIncrementalFileLoader : WorkQueueLoaderAdapter
{
    private static readonly ILog log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType);
    private string transactionId;
    private bool isLocalTransaction = false;
    private string path = null;
    private string indexFile = null;
    private string searchPattern = null;
    private bool compressedArchivesAsSubfolders = false;
    private readonly List<string> filesToProcess = new List<string>();
    private int position = 0;
    public override void Connect(
        IWorkQueueService service, Guid queueId, string workQueueClass,
        string connection, string userName, string password,
        string transactionId)
    {
        if (log.IsInfoEnabled)
        {
            log.Info("Connecting " + connection);
        }
        ParseParameters(connection);
        InitializeFileList();
        SetupTransaction(transactionId, connection);
    }
    public override void Disconnect()
    {
        if(isLocalTransaction)
        {
            ResourceMonitor.Commit(transactionId);
        }
    }
    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if(filesToProcess.Count == position)
        {
            return null;
        }
        string filename = filesToProcess[position];
        DataTable dataTable = CreateFileDataset();
        DataRow row = dataTable.NewRow();
        row["Name"] = filename;
        row["Data"] = GetFileContent(filename);
        dataTable.Rows.Add(row);
        position++;
        return new WorkQueueAdapterResult(
            DataDocumentFactory.New(dataTable.DataSet));
    }
    private string GetFileContent(string filename)
    {
        string[] filenameSegments = filename.Split('|');
        if(filenameSegments.Length == 1)
        {
            // Non-zip file: enforce maximum size before reading
            long maxUncompressedBytes = GetMaxUncompressedBytes();
            string path = filenameSegments[0];
            FileInfo fi = new FileInfo(path);
            if (fi.Exists && fi.Length > maxUncompressedBytes)
            {
                throw new InvalidOperationException("File exceeds allowed size.");
            }
            string content = File.ReadAllText(path);
            if (content.Length > maxUncompressedBytes)
            {
                throw new InvalidOperationException("File exceeds allowed size.");
            }
            return content;
        }
        else
        {
            return GetContentFromZipArchive(
                filenameSegments[0], filenameSegments[1]);
        }
    }
    private string GetContentFromZipArchive(
        string archiveName, string filename)
    {
        // Harden against zip-bomb and invalid entry issues
        long maxUncompressedBytes = GetMaxUncompressedBytes();

        using (
            FileStream fileStream = new FileStream(
                archiveName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            )
        )
        using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read, false))
        {
            // Zip spec uses forward slashes for separators; normalize for lookup
            string normalizedEntryName = filename.Replace('\\', '/');
            ZipArchiveEntry entry = archive.GetEntry(normalizedEntryName);
            if (entry == null)
            {
                throw new InvalidOperationException(
                    $"Entry '{filename}' not found in archive '{archiveName}'."
                );
            }

            // Guard against zip bombs: excessive uncompressed size or ratio
            if (entry.Length > maxUncompressedBytes)
            {
                throw new InvalidOperationException("Archive entry too large.");
            }
            if (
                entry.CompressedLength > 0
                && (entry.Length / (double)entry.CompressedLength) > GetMaxCompressionRatio()
            )
            {
                throw new InvalidOperationException(
                    "Archive entry has suspicious compression ratio."
                );
            }

            using (Stream stream = entry.Open())
            using (StreamReader streamReader = new StreamReader(stream))
            {
                string content = streamReader.ReadToEnd();
                if (content.Length > maxUncompressedBytes)
                {
                    throw new InvalidOperationException(
                        "Archive entry exceeds allowed size."
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
		HashIndexFileTransaction transaction 
            = ResourceMonitor.GetTransaction(
                transactionId, connection) as HashIndexFileTransaction;
		if(transaction == null)
		{
		    ResourceMonitor.RegisterTransaction(transactionId, connection, 
                new HashIndexFileTransaction(indexFile, filesToProcess));
		}
    }
    private void ParseParameters(string connection)
    {
        string[] cnParts = connection.Split(";".ToCharArray());
        foreach (string part in cnParts)
        {
            string[] pair = part.Split("=".ToCharArray());
            if (pair.Length == 2)
            {
                switch (pair[0])
                {
                    case "path":
                        path = pair[1];
                        break;
                    case "indexFile":
                        indexFile = pair[1];
                        break;
                    case "searchPattern":
                        searchPattern = pair[1];
                        break;
                    case "compressedArchivesAsSubfolders":
                        compressedArchivesAsSubfolders
                            = Convert.ToBoolean(pair[1]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            "connectionParameterName", pair[0],
                            ResourceUtils.GetString(
                                "ErrorInvalidConnectionString"));
                }
            }
        }
        if (path == null)
        {
            throw new Exception(
                ResourceUtils.GetString("ErrorNoPath"));
        }
        if (indexFile == null)
        {
            throw new Exception(
                ResourceUtils.GetString("ErrorNoIndexFile"));
        }
        if (searchPattern == null)
        {
            throw new Exception(
                ResourceUtils.GetString("ErrorNoSearchPattern"));
        }
    }
    private void InitializeFileList()
    {
        using (HashIndexFile hashIndexFile = new HashIndexFile(indexFile))    
        {
            if (compressedArchivesAsSubfolders)
            {
                AddUnprocessedZipArchivesToFileList(hashIndexFile);
            }
            AddUnprocessedFilesToFileList(hashIndexFile);
        }
    }
    private void AddUnprocessedFilesToFileList(HashIndexFile hashIndexFile)
    {
		string[] filenames = Directory.GetFiles(path, searchPattern);
        foreach(string filename in filenames)
        {
            if(!hashIndexFile.IsFileProcessed(filename))
            {
                filesToProcess.Add(filename);
            }
        }
    }
    private void AddUnprocessedZipArchivesToFileList( HashIndexFile hashIndexFile)
    {
		string[] filenames = Directory.GetFiles(path, "*.zip");
        foreach(string filename in filenames)
        {
            using(FileStream fileStream = new FileStream(
                filename, FileMode.Open))
            using(ZipArchive archive = new ZipArchive(fileStream))
            {
                string fullDestinationDirPath = Path.GetFullPath(path + Path.DirectorySeparatorChar);
                foreach(ZipArchiveEntry archiveEntry in archive.Entries)
                {
                    string destinationFileName = Path.GetFullPath(
                        Path.Combine(path, archiveEntry.FullName)
                    );
                    var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;
                    if (!destinationFileName.StartsWith(fullDestinationDirPath, comparison))
                    {
                        throw new InvalidOperationException("Entry is outside the target dir: " + destinationFileName);
                    }
                    if(FitsMask(archiveEntry.Name) 
                    && !hashIndexFile.IsZipArchiveEntryProcessed(
                        archiveEntry))
                    {
                        filesToProcess.Add(filename + "|" + 
                            archiveEntry.FullName);
                    }
                }
            }
        }
    }
    private bool FitsMask(string filename)
    {
        Regex mask = new Regex(
            '^' +
            searchPattern
                .Replace(".", "[.]")
                .Replace("*", ".*")
                .Replace("?", ".")
            + '$',
            RegexOptions.IgnoreCase);
        return mask.IsMatch(filename);
    }
    private DataTable CreateFileDataset()
    {
        DataSet dataSet = new DataSet("ROOT");
        DataTable dataTable = dataSet.Tables.Add("File");
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Data", typeof(string));
        // Add file metadata (times)			
        dataTable.Columns.Add("CreationTime", typeof(DateTime));
        dataTable.Columns.Add("LastWriteTime", typeof(DateTime));
        dataTable.Columns.Add("LastAccessTime", typeof(DateTime));
        return dataTable;
    }

    private static long GetMaxUncompressedBytes()
    {
        const int defaultMb = 50; // default for Architect and fallback
        IConfig config = ConfigFactory.GetConfig();
        long maxZipSizeMb = config.GetValue(new [] { "WorkQueue", "MaxUncompressedMbInZip" }) ?? defaultMb;
        return maxZipSizeMb * 1024L * 1024L;
    }

    private static double GetMaxCompressionRatio()
    {
        const double defaultRatio = 100.0;
        IConfig config = ConfigFactory.GetConfig();
        double? configured = config.GetValue(new [] { "WorkQueue", "MaxCompressionRatio" });
        return configured ?? defaultRatio;
    }
}
