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
using System.Text.RegularExpressions;
using System.Xml;
using log4net;
using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueIncrementalFileLoader : WorkQueueLoaderAdapter
{
    private static readonly ILog log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType
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
            log.Info("Connecting " + connection);
        }
        ParseParameters(connection);
        InitializeFileList();
        SetupTransaction(transactionId, connection);
    }

    public override void Disconnect()
    {
        if (isLocalTransaction)
        {
            ResourceMonitor.Commit(transactionId);
        }
    }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if (filesToProcess.Count == position)
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
        return new WorkQueueAdapterResult(DataDocumentFactory.New(dataTable.DataSet));
    }

    private string GetFileContent(string filename)
    {
        string[] filenameSegments = filename.Split('|');
        if (filenameSegments.Length == 1)
        {
            return File.ReadAllText(filenameSegments[0]);
        }

        return GetContentFromZipArchive(filenameSegments[0], filenameSegments[1]);
    }

    private string GetContentFromZipArchive(string archiveName, string filename)
    {
        using (FileStream fileStream = new FileStream(archiveName, FileMode.Open))
        using (ZipArchive archive = new ZipArchive(fileStream))
        using (Stream stream = archive.GetEntry(filename).Open())
        using (StreamReader streamReader = new StreamReader(stream))
        {
            return streamReader.ReadToEnd();
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
            ResourceMonitor.GetTransaction(transactionId, connection) as HashIndexFileTransaction;
        if (transaction == null)
        {
            ResourceMonitor.RegisterTransaction(
                transactionId,
                connection,
                new HashIndexFileTransaction(indexFile, filesToProcess)
            );
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
                        compressedArchivesAsSubfolders = Convert.ToBoolean(pair[1]);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(
                            "connectionParameterName",
                            pair[0],
                            ResourceUtils.GetString("ErrorInvalidConnectionString")
                        );
                }
            }
        }
        if (path == null)
        {
            throw new Exception(ResourceUtils.GetString("ErrorNoPath"));
        }
        if (indexFile == null)
        {
            throw new Exception(ResourceUtils.GetString("ErrorNoIndexFile"));
        }
        if (searchPattern == null)
        {
            throw new Exception(ResourceUtils.GetString("ErrorNoSearchPattern"));
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
        foreach (string filename in filenames)
        {
            if (!hashIndexFile.IsFileProcessed(filename))
            {
                filesToProcess.Add(filename);
            }
        }
    }

    private void AddUnprocessedZipArchivesToFileList(HashIndexFile hashIndexFile)
    {
        string[] filenames = Directory.GetFiles(path, "*.zip");
        foreach (string filename in filenames)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(fileStream))
            {
                string fullDestinationDirPath = Path.GetFullPath(
                    path + Path.DirectorySeparatorChar
                );
                foreach (ZipArchiveEntry archiveEntry in archive.Entries)
                {
                    string destinationFileName = Path.GetFullPath(
                        Path.Combine(path, archiveEntry.FullName)
                    );
                    if (!destinationFileName.StartsWith(fullDestinationDirPath))
                    {
                        throw new InvalidOperationException(
                            "Entry is outside the target dir: " + destinationFileName
                        );
                    }
                    if (
                        FitsMask(archiveEntry.Name)
                        && !hashIndexFile.IsZipArchiveEntryProcessed(archiveEntry)
                    )
                    {
                        filesToProcess.Add(filename + "|" + archiveEntry.FullName);
                    }
                }
            }
        }
    }

    private bool FitsMask(string filename)
    {
        Regex mask = new Regex(
            '^' + searchPattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", ".") + '$',
            RegexOptions.IgnoreCase
        );
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
}
