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
using System.Reflection;
using System.Text;
using log4net;
using Origam.Extensions;
using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueIataBspFileLoader : WorkQueueLoaderAdapter
{
    private class PeekableStreamReaderAdapter
    {
        private StreamReader streamReader;
        private Queue<string> buffer;

        public PeekableStreamReaderAdapter(StreamReader streamReader)
        {
            this.streamReader = streamReader;
            buffer = new Queue<string>();
        }

        public string PeekLine()
        {
            string line = streamReader.ReadLine();
            if (line == null)
            {
                return null;
            }
            buffer.Enqueue(item: line);
            return line;
        }

        public string ReadLine()
        {
            if (buffer.Count > 0)
            {
                return buffer.Dequeue();
            }
            return streamReader.ReadLine();
        }
    }

    private static readonly ILog log = LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );
    private string transactionId;
    private bool isLocalTransaction = false;
    private string path = null;
    private string indexFile = null;
    private string searchPattern = null;
    private string segmentMarker = null;
    private string footerMarker = null;
    private string lastRowMarker = null;
    private bool addLastRowToQueue = false;
    private StreamReader streamReader = null;
    private PeekableStreamReaderAdapter adapter = null;
    private FileInfo fileInfo = null;
    private int segmentsCounter = 0;
    private bool lastRowMarkerReached = false;
    private HashIndexFile hashIndexFile;

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
        SetupTransaction(transactionId: transactionId);
        ParseParameters(connection: connection);
        InitializeStream(connection: connection);
    }

    private void InitializeStream(string connection)
    {
        try
        {
            hashIndexFile = new HashIndexFile(indexFile: indexFile);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: "Failed to open hash index file.", ex: ex);
            }
            throw;
        }
        string fileName = hashIndexFile.GetFirstUnprocessedFile(path: path, mask: searchPattern);
        if (fileName != null)
        {
            fileInfo = new FileInfo(fileName: fileName);
            streamReader = new StreamReader(stream: OpenFileExclusively(fileName: fileName));
            adapter = new PeekableStreamReaderAdapter(streamReader: streamReader);
            MoveToFirstTransaction();
        }
    }

    private Stream OpenFileExclusively(string fileName)
    {
        return File.Open(
            path: fileName,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.None
        );
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

                    case "segmentMarker":
                    {
                        segmentMarker = pair[1];
                        break;
                    }

                    case "footerMarker":
                    {
                        footerMarker = pair[1];
                        break;
                    }

                    case "lastRowMarker":
                    {
                        lastRowMarker = pair[1];
                        break;
                    }

                    case "addLastRowToQueue":
                    {
                        addLastRowToQueue = pair[1] == "true";
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "connectionParameterName",
                            actualValue: pair[0],
                            message: ResourceUtils.GetString(key: "ErrorInvalidConnectionString")
                        );
                    }
                }
            }
        }
        if (path == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoPath"));
        }
        if (indexFile == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoIndexFile"));
        }
        if (searchPattern == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoSearchPattern"));
        }
        if (segmentMarker == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoSegmentMarker"));
        }
        if (footerMarker == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoFooterMarker"));
        }
        if (lastRowMarker == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoLastRowMarker"));
        }
    }

    private void SetupTransaction(string transactionId)
    {
        this.transactionId = transactionId;
        if (this.transactionId == null)
        {
            this.transactionId = Guid.NewGuid().ToString();
            isLocalTransaction = true;
        }
    }

    public override void Disconnect()
    {
        if (fileInfo != null)
        {
            CheckLastRowMarker();
            hashIndexFile.AddEntryToIndexFile(
                entry: hashIndexFile.CreateIndexFileEntry(filename: fileInfo.FullName)
                    + $"|{segmentsCounter}"
            );
        }
        if (isLocalTransaction)
        {
            ResourceMonitor.Commit(transactionId: transactionId);
        }
        hashIndexFile.Dispose();
    }

    private void CheckLastRowMarker()
    {
        if ((segmentsCounter > 0) && !lastRowMarkerReached)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorLastRowMarkerNotReached")
            );
        }
    }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if (streamReader == null)
        {
            return null;
        }
        string firstLine = adapter.ReadLine();
        while ((firstLine != null) && !firstLine.StartsWith(value: segmentMarker))
        {
            if (firstLine.StartsWith(value: lastRowMarker))
            {
                lastRowMarkerReached = true;
                if (addLastRowToQueue)
                {
                    return ReadSegmentAndReturnAsWQResult(firstLine: firstLine);
                }
            }
            firstLine = adapter.ReadLine();
        }
        if (firstLine == null)
        {
            streamReader.Close();
            CheckLastRowMarker();
            return null;
        }
        return ReadSegmentAndReturnAsWQResult(firstLine: firstLine);
    }

    private WorkQueueAdapterResult ReadSegmentAndReturnAsWQResult(string firstLine)
    {
        string fileSegment = ReadFileSegment(firstLine: firstLine);
        DataTable dataTable = CreateFileDataset();
        DataRow row = dataTable.NewRow();
        try
        {
            // Add file metadata (times)
            row[columnName: "CreationTime"] = fileInfo.CreationTime;
            row[columnName: "LastWriteTime"] = fileInfo.LastWriteTime;
            row[columnName: "LastAccessTime"] = fileInfo.LastAccessTime;
        }
        catch { }
        row[columnName: "Name"] = fileInfo.Name;
        row[columnName: "Data"] = fileSegment;
        row[columnName: "SequenceNumber"] = segmentsCounter++;
        dataTable.Rows.Add(row: row);
        return new WorkQueueAdapterResult(
            document: DataDocumentFactory.New(dataSet: dataTable.DataSet)
        );
    }

    private string ReadFileSegment(string firstLine)
    {
        StringBuilder output = new StringBuilder();
        output.AppendLine(value: firstLine);
        string line = adapter.PeekLine();
        while (
            (line != null)
            && !line.StartsWith(value: segmentMarker)
            && !line.StartsWith(value: footerMarker)
        )
        {
            output.AppendLine(value: adapter.ReadLine());
            line = adapter.PeekLine();
        }
        return output.ToString();
    }

    private void MoveToFirstTransaction()
    {
        string line = adapter.PeekLine();
        while ((line != null) && !(line.StartsWith(value: segmentMarker)))
        {
            line = adapter.ReadLine();
            line = adapter.PeekLine();
        }
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
        dataTable.Columns.Add(columnName: "SequenceNumber", type: typeof(int));
        return dataTable;
    }
}
