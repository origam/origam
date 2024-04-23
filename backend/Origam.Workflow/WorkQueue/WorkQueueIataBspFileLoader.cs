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
using System.Xml;
using Origam.Workbench.Services;
using log4net;
using Origam.Extensions;

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
                if(line == null)
                {
                    return null;
                }
                buffer.Enqueue(line);
                return line;
            }

        public string ReadLine()
        {
                if(buffer.Count > 0)
                {
                    return buffer.Dequeue();
                }
                return streamReader.ReadLine();
            }
    }

    private static readonly ILog log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType);
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
        IWorkQueueService service, Guid queueId, 
        string workQueueClass, string connection, 
        string userName, string password, 
        string transactionId)
    {
            if(log.IsInfoEnabled)
            {
                log.Info("Connecting " + connection);
            }
            SetupTransaction(transactionId);
            ParseParameters(connection);
            InitializeStream(connection);
        }

    private void InitializeStream(string connection)
    {
            try
            {
                hashIndexFile = new HashIndexFile(indexFile);
            }
            catch(Exception ex)
            {
                if(log.IsErrorEnabled)
                {
                    log.LogOrigamError("Failed to open hash index file.", ex);
                }
                throw;
            }
            string fileName = hashIndexFile
                .GetFirstUnprocessedFile(path, searchPattern);
            if(fileName != null)
            {
                fileInfo = new FileInfo(fileName);
                streamReader = new StreamReader(OpenFileExclusively(fileName));
                adapter = new PeekableStreamReaderAdapter(streamReader);
                MoveToFirstTransaction();
            }
        }


    private Stream OpenFileExclusively(string fileName)
    {
            return File.Open(
                fileName, FileMode.Open, FileAccess.Read, FileShare.None);
        }

    private void ParseParameters(string connection)
    {
            string[] cnParts = connection.Split(";".ToCharArray());
            foreach(string part in cnParts)
            {
                string[] pair = part.Split("=".ToCharArray());
                if(pair.Length == 2)
                {
                    switch(pair[0])
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
                        case "segmentMarker":
                            segmentMarker = pair[1];
                            break;
                        case "footerMarker":
                            footerMarker = pair[1];
                            break;
                        case "lastRowMarker":
                            lastRowMarker = pair[1];
                            break;
                        case "addLastRowToQueue":
                            addLastRowToQueue = pair[1] == "true";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(
                                "connectionParameterName", pair[0], 
                                ResourceUtils.GetString(
                                    "ErrorInvalidConnectionString"));
                    }
                }
            }
            if(path == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorNoPath"));
            }
            if(indexFile == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorNoIndexFile"));
            }
            if(searchPattern == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorNoSearchPattern"));
            }
            if(segmentMarker == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorNoSegmentMarker"));
            }
            if(footerMarker == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorNoFooterMarker"));
            }
            if(lastRowMarker == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorNoLastRowMarker"));
            }
        }

    private void SetupTransaction(string transactionId)
    {
            this.transactionId = transactionId;
            if(this.transactionId == null)
            {
                this.transactionId = Guid.NewGuid().ToString();
                isLocalTransaction = true;
            }
        }

    public override void Disconnect()
    {
            if(fileInfo != null)
            {
                CheckLastRowMarker();
                hashIndexFile.AddEntryToIndexFile(
                    hashIndexFile.CreateIndexFileEntry(
                        fileInfo.FullName) + $"|{segmentsCounter}");
            }
            if(isLocalTransaction)
            {
                ResourceMonitor.Commit(transactionId);
            }
            hashIndexFile.Dispose();
        }

    private void CheckLastRowMarker()
    {
            if((segmentsCounter > 0) && !lastRowMarkerReached)
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorLastRowMarkerNotReached"));
            }
        }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
            if(streamReader == null)
            {
                return null;
            }
            string firstLine = adapter.ReadLine();
            while((firstLine != null) && !firstLine.StartsWith(segmentMarker))
            {
                if(firstLine.StartsWith(lastRowMarker))
                {
                    lastRowMarkerReached = true;
                    if(addLastRowToQueue)
                    {
                        return ReadSegmentAndReturnAsWQResult(firstLine);
                    }
                }
                firstLine = adapter.ReadLine();
            }
            if(firstLine == null)
            {
                streamReader.Close();
                CheckLastRowMarker();
                return null;
            }
            return ReadSegmentAndReturnAsWQResult(firstLine);
        }

    private WorkQueueAdapterResult ReadSegmentAndReturnAsWQResult(
        string firstLine)
    {
            string fileSegment = ReadFileSegment(firstLine);
            DataTable dataTable = CreateFileDataset();
            DataRow row = dataTable.NewRow();
            try
            {
                // Add file metadata (times)			     row["CreationTime"] = fileInfo.CreationTime;
                row["LastWriteTime"] = fileInfo.LastWriteTime;
                row["LastAccessTime"] = fileInfo.LastAccessTime;
            }
            catch
            {
            }
            row["Name"] = fileInfo.Name;
            row["Data"] = fileSegment;
            row["SequenceNumber"] = segmentsCounter++;
            dataTable.Rows.Add(row);
            return new WorkQueueAdapterResult(
                DataDocumentFactory.New(dataTable.DataSet));
        }

    private string ReadFileSegment(string firstLine)
    {
            StringBuilder output = new StringBuilder();
            output.AppendLine(firstLine);
            string line = adapter.PeekLine();
            while((line != null) 
            && !line.StartsWith(segmentMarker)
            && !line.StartsWith(footerMarker))
            {
                output.AppendLine(adapter.ReadLine());
                line = adapter.PeekLine();
            }
            return output.ToString();
        }

    private void MoveToFirstTransaction()
    {
            string line = adapter.PeekLine();
            while((line != null) 
            && !(line.StartsWith(segmentMarker)))
            {
                line = adapter.ReadLine();
                line = adapter.PeekLine();
            }
        }

    private DataTable CreateFileDataset()
    {
            DataSet dataSet = new DataSet("ROOT");
            DataTable dataTable = dataSet.Tables.Add("File");
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Data", typeof(string));
            // Add file metadata (times)			     dataTable.Columns.Add("CreationTime", typeof(DateTime));
            dataTable.Columns.Add("LastWriteTime", typeof(DateTime));
            dataTable.Columns.Add("LastAccessTime", typeof(DateTime));
            dataTable.Columns.Add("SequenceNumber", typeof(int));
            return dataTable;
        }
}