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
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueFileLoader : WorkQueueLoaderAdapter
{
    private class SplitFileStreamReader : StreamReader
    {
        private int segmentCounter;
        public int SegmentNumber => ++segmentCounter;
        public string ProcessedFilename { get; }
        public string ProcessedFileTitle { get; }
        public string Header { get; private set; }

        public SplitFileStreamReader(
            string processedFilename,
            string processedFileTitle,
            Stream stream
        )
            : base(stream: stream)
        {
            ProcessedFilename = processedFilename;
            ProcessedFileTitle = processedFileTitle;
        }

        public SplitFileStreamReader(
            string processedFilename,
            string processedFileTitle,
            Stream stream,
            Encoding encoding
        )
            : base(stream: stream, encoding: encoding)
        {
            ProcessedFilename = processedFilename;
            ProcessedFileTitle = processedFileTitle;
        }

        public void ProcessHeader()
        {
            if (!EndOfStream)
            {
                Header = ReadLine();
            }
        }
    }

    private int _currentPosition;
    private string[] _filenames;
    private readonly Hashtable _files = new Hashtable();
    private string _transactionId;
    private bool _isLocalTransaction;
    private FileType _mode = FileType.TEXT;
    private ReadType _readType = ReadType.SingleFiles;
    private string _encoding;
    private CompressionType _compression = CompressionType.None;
    private ZipInputStream _currentZipStream;
    private int _splitFileByRows;
    private bool _splitFileAndKeepHeader;
    private SplitFileStreamReader _splitFileStreamReader;

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    private class LimitedReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _limit;
        private long _readTotal;

        public LimitedReadStream(Stream inner, long limit)
        {
            _inner = inner;
            _limit = limit;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int toRead = count;
            long remaining = _limit - _readTotal;
            if (remaining <= 0)
            {
                throw new InvalidOperationException(message: "Stream exceeds allowed size.");
            }
            if (toRead > remaining)
            {
                toRead = (int)remaining;
            }

            int read = _inner.Read(buffer: buffer, offset: offset, count: toRead);
            _readTotal += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }

    public enum FileType
    {
        TEXT = 0,
        BINARY = 1,
    }

    public enum ReadType
    {
        SingleFiles = 0,
        AggregateCompressedFiles = 1,
        AggregateAllFiles = 2,
        SplitByRows = 3,
    }

    public enum CompressionType
    {
        None = 0,
        ZIP = 1,
    }

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
        _transactionId = transactionId;
        if (_transactionId == null)
        {
            _transactionId = Guid.NewGuid().ToString();
            _isLocalTransaction = true;
        }
        string path = null;
        string searchPattern = null;
        if (connection != null)
        {
            var connectionParts = connection.Split(separator: ";".ToCharArray());
            foreach (var part in connectionParts)
            {
                var pair = part.Split(separator: "=".ToCharArray());
                if (pair.Length == 2)
                {
                    try
                    {
                        switch (pair[0])
                        {
                            case "path":
                            {
                                path = pair[1];
                                break;
                            }

                            case "searchPattern":
                            {
                                searchPattern = pair[1];
                                break;
                            }

                            case "mode":
                            {
                                _mode = (FileType)
                                    Enum.Parse(enumType: typeof(FileType), value: pair[1]);
                                break;
                            }

                            case "encoding":
                            {
                                _encoding = pair[1];
                                break;
                            }

                            case "compression":
                            {
                                _compression = (CompressionType)
                                    Enum.Parse(enumType: typeof(CompressionType), value: pair[1]);
                                break;
                            }

                            case "readType":
                            {
                                _readType = (ReadType)
                                    Enum.Parse(enumType: typeof(ReadType), value: pair[1]);
                                break;
                            }

                            case "splitFileByRows":
                            {
                                _splitFileByRows = Convert.ToInt32(value: pair[1]);
                                break;
                            }

                            case "keepHeader":
                            {
                                _splitFileAndKeepHeader = Convert.ToBoolean(value: pair[1]);
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
                    catch (Exception ex)
                    {
                        throw new Exception(
                            message: Strings.ErrorParsingConnectionString + ": " + ex.Message
                        );
                    }
                }
            }
        }
        if (path == null)
        {
            throw new Exception(message: Strings.ErrorNoPath);
        }
        if (searchPattern == null)
        {
            throw new Exception(message: Strings.ErrorNoSearchPattern);
        }
        if (_readType == ReadType.SplitByRows)
        {
            if (_splitFileByRows <= 0)
            {
                throw new Exception(message: Strings.ErrorSplitFileByRows);
            }
            if (_mode == FileType.BINARY)
            {
                throw new Exception(message: Strings.SplitBinaryFilesNotSupported);
            }
        }
        if (_readType == ReadType.AggregateCompressedFiles && _compression == CompressionType.None)
        {
            throw new Exception(message: Strings.AggregateCompressedFilesButNoCompression);
        }
        // lock the files
        _filenames = Directory.GetFiles(path: path, searchPattern: searchPattern);
        foreach (var fileName in _filenames)
        {
            try
            {
                if (log.IsInfoEnabled)
                {
                    log.Info(message: "Locking file " + fileName);
                }
                var fileStream = OpenFile(fileName: fileName);
                _files.Add(key: fileName, value: fileStream);
            }
            catch
            {
                // ignored
            }
        }
        var fileDeleteTransaction =
            ResourceMonitor.GetTransaction(
                transactionId: _transactionId,
                resourceManagerId: connection
            ) as FileDeleteTransaction;
        if (fileDeleteTransaction == null)
        {
            ResourceMonitor.RegisterTransaction(
                transactionId: _transactionId,
                resourceManagerId: connection,
                transaction: new FileDeleteTransaction(files: _files)
            );
        }
    }

    private static FileStream OpenFile(string fileName)
    {
        return File.Open(
            path: fileName,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.None
        );
    }

    public override void Disconnect()
    {
        if (_isLocalTransaction)
        {
            ResourceMonitor.Commit(transactionId: _transactionId);
        }
    }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        var dataTable = CreateFileDataset(mode: _mode).Tables[name: "File"];
        bool result;
        switch (_readType)
        {
            case ReadType.SingleFiles:
            {
                result = RetrieveNextFile(dataTable: dataTable, aggregateCompressedFiles: false);
                break;
            }

            case ReadType.AggregateCompressedFiles:
            {
                return RetrieveAggregate(dataTable: dataTable, compressedOnly: true);
            }
            case ReadType.AggregateAllFiles:
            {
                return RetrieveAggregate(dataTable: dataTable, compressedOnly: false);
            }
            case ReadType.SplitByRows:
            {
                result = RetrieveNextSegment(dataTable: dataTable);
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "readType",
                    actualValue: _readType,
                    message: Strings.UnknownReadType
                );
            }
        }
        return result
            ? new WorkQueueAdapterResult(
                document: DataDocumentFactory.New(dataSet: dataTable.DataSet)
            )
            : null;
    }

    private WorkQueueAdapterResult RetrieveAggregate(DataTable dataTable, bool compressedOnly)
    {
        // retrieve files as multiple records
        while (RetrieveNextFile(dataTable: dataTable, aggregateCompressedFiles: compressedOnly)) { }
        if (dataTable.Rows.Count == 0)
        {
            return null;
        }
        // create an aggregated record with files as Data field
        var aggregatedDataTable = CreateFileDataset(mode: _mode).Tables[name: "File"];
        DataRow dataRow;
        if (compressedOnly)
        {
            string fileName = _filenames[_currentPosition - 1];
            dataRow = CreateAndInitializeDataRow(
                dataTable: aggregatedDataTable,
                filename: fileName,
                title: fileName
            );
        }
        else
        {
            dataRow = aggregatedDataTable.NewRow();
            dataRow[columnName: "Name"] = $"Multiple files {DateTime.Now}";
        }
        dataRow[columnName: "Data"] = dataTable.DataSet.GetXml();
        aggregatedDataTable.Rows.Add(row: dataRow);
        return new WorkQueueAdapterResult(
            document: DataDocumentFactory.New(dataSet: aggregatedDataTable.DataSet)
        );
    }

    private (Stream, string, string) GetNextFileStream(bool aggregateCompressedFilesOnly)
    {
        start:
        if (_currentPosition + 1 > _files.Count)
        {
            return (null, null, null);
        }
        var fileName = _filenames[_currentPosition];
        var title = fileName;
        var fileStream = (FileStream)_files[key: fileName];
        Stream finalStream;
        if (_compression == CompressionType.ZIP)
        {
            var zipStream = _currentZipStream;
            if (zipStream == null)
            {
                zipStream = new ZipInputStream(baseInputStream: fileStream);
                _currentZipStream = zipStream;
            }
            var zipEntry = zipStream.GetNextEntry();
            if (zipEntry != null)
            {
                long maxBytes = WorkQueueConfig.GetMaxUncompressedBytes();
                if (zipEntry.Size >= 0 && zipEntry.Size > maxBytes)
                {
                    throw new InvalidOperationException(message: Strings.ArchiveEntryTooLarge);
                }
                if (zipEntry.CompressedSize > 0 && zipEntry.Size >= 0)
                {
                    double ratio = zipEntry.Size / (double)zipEntry.CompressedSize;
                    if (ratio > WorkQueueConfig.GetMaxCompressionRatio())
                    {
                        throw new InvalidOperationException(
                            message: Strings.ArchiveEntrySuspiciousCompressionRatio
                        );
                    }
                }
            }
            if (zipEntry == null)
            {
                _currentZipStream = null;
                zipStream.Close();
                ((IDisposable)zipStream).Dispose();
                _currentPosition++;
                if (aggregateCompressedFilesOnly)
                {
                    return (null, null, null);
                }

                goto start;
            }
            finalStream = zipStream;
            title += " " + zipEntry.Name;
        }
        else
        {
            finalStream = fileStream;
            _currentPosition++;
        }
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Reading file " + title);
        }
        return (finalStream, fileName, title);
    }

    private bool RetrieveNextSegment(DataTable dataTable)
    {
        if ((_splitFileStreamReader == null) || _splitFileStreamReader.EndOfStream)
        {
            var (stream, filename, title) = GetNextFileStream(aggregateCompressedFilesOnly: false);
            if (stream == null)
            {
                return false;
            }
            _splitFileStreamReader =
                (_encoding == null)
                    ? new SplitFileStreamReader(
                        processedFilename: filename,
                        processedFileTitle: title,
                        stream: stream
                    )
                    : new SplitFileStreamReader(
                        processedFilename: filename,
                        processedFileTitle: title,
                        stream: stream,
                        encoding: Encoding.GetEncoding(name: _encoding)
                    );
            if (_splitFileAndKeepHeader)
            {
                _splitFileStreamReader.ProcessHeader();
            }
        }
        AddTextFileSegmentFromStream(dataTable: dataTable);
        return true;
    }

    private bool RetrieveNextFile(DataTable dataTable, bool aggregateCompressedFiles)
    {
        var (stream, filename, title) = GetNextFileStream(
            aggregateCompressedFilesOnly: aggregateCompressedFiles
        );
        if (stream == null)
        {
            return false;
        }
        AddFileFromStream(
            stream: stream,
            dataTable: dataTable,
            mode: _mode,
            filename: filename,
            title: title,
            encoding: _encoding
        );
        return true;
    }

    private void AddTextFileSegmentFromStream(DataTable dataTable)
    {
        var rowCounter = 0;
        using (var memoryStream = new MemoryStream())
        {
            var streamWriter =
                (_encoding == null)
                    ? new StreamWriter(stream: memoryStream)
                    : new StreamWriter(
                        stream: memoryStream,
                        encoding: Encoding.GetEncoding(name: _encoding)
                    );
            if (_splitFileAndKeepHeader)
            {
                streamWriter.WriteLine(value: _splitFileStreamReader.Header);
            }
            while (!_splitFileStreamReader.EndOfStream && (rowCounter < _splitFileByRows))
            {
                streamWriter.WriteLine(value: _splitFileStreamReader.ReadLine());
                rowCounter++;
            }
            // last parts tend to remain in buffer, we need to flush them
            streamWriter.Flush();
            var dataRow = CreateAndInitializeDataRow(
                dataTable: dataTable,
                filename: _splitFileStreamReader.ProcessedFilename,
                title: _splitFileStreamReader.ProcessedFileTitle
            );
            dataRow[columnName: "SequenceNumber"] = _splitFileStreamReader.SegmentNumber;
            memoryStream.Position = 0;
            var internalStreamReader =
                (_encoding == null)
                    ? new StreamReader(stream: memoryStream)
                    : new StreamReader(
                        stream: memoryStream,
                        encoding: Encoding.GetEncoding(name: _encoding)
                    );
            dataRow[columnName: "Data"] = internalStreamReader.ReadToEnd();
            dataTable.Rows.Add(row: dataRow);
        }
    }

    public static DataSet GetFileFromStream(
        Stream stream,
        FileType mode,
        string filename,
        string title,
        string encoding
    )
    {
        var dataTable = CreateFileDataset(mode: mode).Tables[name: "File"];
        AddFileFromStream(
            stream: stream,
            dataTable: dataTable,
            mode: mode,
            filename: filename,
            title: title,
            encoding: encoding
        );
        return dataTable.DataSet;
    }

    private static void AddFileFromStream(
        Stream stream,
        DataTable dataTable,
        FileType mode,
        string filename,
        string title,
        string encoding
    )
    {
        AddFileToTable(
            stream: stream,
            mode: mode,
            filename: filename,
            title: title,
            encoding: encoding,
            dataTable: dataTable
        );
    }

    private static DataSet CreateFileDataset(FileType mode)
    {
        var dataSet = new DataSet(dataSetName: "ROOT");
        var dataTable = dataSet.Tables.Add(name: "File");
        dataTable.Columns.Add(columnName: "Name", type: typeof(string));
        switch (mode)
        {
            case FileType.TEXT:
            {
                dataTable.Columns.Add(columnName: "Data", type: typeof(string));
                break;
            }

            case FileType.BINARY:
            {
                dataTable.Columns.Add(columnName: "Data", type: typeof(byte[]));
                break;
            }
        }
        // Add file metadata (times)
        dataTable.Columns.Add(columnName: "CreationTime", type: typeof(DateTime));
        dataTable.Columns.Add(columnName: "LastWriteTime", type: typeof(DateTime));
        dataTable.Columns.Add(columnName: "LastAccessTime", type: typeof(DateTime));
        dataTable.Columns.Add(columnName: "SequenceNumber", type: typeof(int));
        return dataSet;
    }

    private static DataRow CreateAndInitializeDataRow(
        DataTable dataTable,
        string filename,
        string title
    )
    {
        var dataRow = dataTable.NewRow();
        if (File.Exists(path: filename))
        {
            // Add file metadata (times)
            var fileInfo = new FileInfo(fileName: filename);
            dataRow[columnName: "CreationTime"] = fileInfo.CreationTime;
            dataRow[columnName: "LastWriteTime"] = fileInfo.LastWriteTime;
            dataRow[columnName: "LastAccessTime"] = fileInfo.LastAccessTime;
        }
        if (!(dataRow[columnName: "CreationTime"] is DateTime))
        {
            dataRow[columnName: "CreationTime"] = DateTime.Now;
        }
        dataRow[columnName: "Name"] = title;
        return dataRow;
    }

    private static void AddFileToTable(
        Stream stream,
        FileType mode,
        string filename,
        string title,
        string encoding,
        DataTable dataTable
    )
    {
        var dataRow = CreateAndInitializeDataRow(
            dataTable: dataTable,
            filename: filename,
            title: title
        );
        switch (mode)
        {
            case FileType.TEXT:
            {
                ReadTextStream(stream: stream, encoding: encoding, dataRow: dataRow);
                break;
            }

            case FileType.BINARY:
            {
                ReadBinaryStream(stream: stream, dataRow: dataRow);
                break;
            }
        }
        dataTable.Rows.Add(row: dataRow);
    }

    private static void ReadBinaryStream(Stream stream, DataRow dataRow)
    {
        long maxBytes = WorkQueueConfig.GetMaxUncompressedBytes();
        byte[] data;
        if (stream.CanSeek)
        {
            if (stream.Length > maxBytes)
            {
                throw new InvalidOperationException(message: Strings.StreamExceedsAllowedSize);
            }
            data = new byte[stream.Length];
            stream.Read(buffer: data, offset: 0, count: Convert.ToInt32(value: stream.Length));
        }
        else
        {
            using (var memoryStream = new MemoryStream())
            {
                int count;
                long total = 0;
                do
                {
                    var buffer = new byte[1024];
                    count = stream.Read(buffer: buffer, offset: 0, count: 1024);
                    if (count > 0)
                    {
                        total += count;
                        if (total > maxBytes)
                        {
                            throw new InvalidOperationException(
                                message: Strings.StreamExceedsAllowedSize
                            );
                        }
                        memoryStream.Write(buffer: buffer, offset: 0, count: count);
                    }
                } while (stream.CanRead && count > 0);
                data = memoryStream.ToArray();
            }
        }
        dataRow[columnName: "Data"] = data;
    }

    private static void ReadTextStream(Stream stream, string encoding, DataRow dataRow)
    {
        long maxBytes = WorkQueueConfig.GetMaxUncompressedBytes();
        Stream effective = stream;
        if (stream.CanSeek)
        {
            if (stream.Length > maxBytes)
            {
                throw new InvalidOperationException(message: Strings.StreamExceedsAllowedSize);
            }
            stream.Position = 0;
        }
        else
        {
            effective = new LimitedReadStream(inner: stream, limit: maxBytes);
        }
        var streamReader =
            (encoding == null)
                ? new StreamReader(stream: effective)
                : new StreamReader(
                    stream: effective,
                    encoding: Encoding.GetEncoding(name: encoding)
                );
        string content = streamReader.ReadToEnd();
        if (content.Length > maxBytes)
        {
            throw new InvalidOperationException(message: Strings.StreamExceedsAllowedSize);
        }
        dataRow[columnName: "Data"] = content;
    }
}
