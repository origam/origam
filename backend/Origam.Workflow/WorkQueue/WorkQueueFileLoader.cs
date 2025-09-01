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
using System.IO;
using System.Data;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Origam.Workbench.Services;
using Origam.Config;

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
			Stream stream) 
			: base(stream)
		{
			ProcessedFilename = processedFilename;
			ProcessedFileTitle = processedFileTitle;
		}
		public SplitFileStreamReader(
			string processedFilename,
			string processedFileTitle, 
			Stream stream, 
			Encoding encoding) 
			: base(stream, encoding)
		{
			ProcessedFilename = processedFilename;
			ProcessedFileTitle = processedFileTitle;
		}
		public void ProcessHeader()
		{
			if(!EndOfStream)
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
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    // Security limits for unzipping / reading
    private static long GetMaxUncompressedBytes()
    {
        const int defaultMb = 50;
        IConfig config = ConfigFactory.GetConfig();
        long maxZipSizeMb = config.GetValue(new [] { "WorkQueue", "MaxUncompressedMbInZip" }) ?? defaultMb;
        return maxZipSizeMb * 1024L * 1024L;
    }

    private const double MaxCompressionRatio = 100.0; // similar to incremental loader

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
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int toRead = count;
            long remaining = _limit - _readTotal;
            if (remaining <= 0)
            {
                throw new InvalidOperationException("Stream exceeds allowed size.");
            }
            if (toRead > remaining) toRead = (int)remaining;
            int read = _inner.Read(buffer, offset, toRead);
            _readTotal += read;
            return read;
        }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
	public enum FileType
    {
		TEXT = 0,
		BINARY = 1
    }
	public enum ReadType
    {
		SingleFiles = 0,
		AggregateCompressedFiles = 1,
		AggregateAllFiles = 2,
		SplitByRows = 3
    }
	public enum CompressionType
	{
		None = 0,
		ZIP = 1
	}
	public override void Connect(IWorkQueueService service, Guid queueId, 
        string workQueueClass, string connection, string userName, 
		string password, string transactionId)
	{
		if(log.IsInfoEnabled)
		{
			log.Info("Connecting " + connection);
		}
        _transactionId = transactionId;
        if(_transactionId == null)
        {
            _transactionId = Guid.NewGuid().ToString();
            _isLocalTransaction = true;
        }
        string path = null;
		string searchPattern = null;
		if (connection != null)
		{
			var connectionParts = connection.Split(";".ToCharArray());
			foreach (var part in connectionParts)
			{
				var pair = part.Split("=".ToCharArray());
				if (pair.Length == 2)
				{
					try
					{
						switch (pair[0])
						{
							case "path":
								path = pair[1];
								break;
							case "searchPattern":
								searchPattern = pair[1];
								break;
							case "mode":
								_mode = (FileType)Enum.Parse(
									typeof(FileType), pair[1]);
								break;
							case "encoding":
								_encoding = pair[1];
								break;
							case "compression":
								_compression = (CompressionType)Enum.Parse(
									typeof(CompressionType), pair[1]);
								break;
							case "readType":
								_readType = (ReadType)Enum.Parse(
									typeof(ReadType), pair[1]);
								break;
							case "splitFileByRows":
								_splitFileByRows = Convert.ToInt32(pair[1]);
								break;
							case "keepHeader":
								_splitFileAndKeepHeader =
									Convert.ToBoolean(pair[1]);
								break;
							default:
								throw new ArgumentOutOfRangeException(
									"connectionParameterName",
									pair[0],
									ResourceUtils.GetString(
										"ErrorInvalidConnectionString"));
						}
					}
					catch (Exception ex)
                    {
						throw new Exception(
							"An error has occurred while parsing the connection string: "
							+ ex.Message);
                    }
				}
			}
		}
		if(path == null)
		{
			throw new Exception(
				ResourceUtils.GetString("ErrorNoPath"));
		}
		if(searchPattern == null)
		{
			throw new Exception(
				ResourceUtils.GetString("ErrorNoSearchPattern"));
		}
        if (_readType == ReadType.SplitByRows)
        {
			if (_splitFileByRows <= 0)
			{
				throw new Exception(
					ResourceUtils.GetString("ErrorSplitFileByRows"));
			}
			if (_mode == FileType.BINARY)
			{
				throw new Exception(
					ResourceUtils.GetString("SplitBinaryFilesNotSupported"));
			}
		}
		if (_readType == ReadType.AggregateCompressedFiles
			&& _compression == CompressionType.None)
        {
			throw new Exception(
				ResourceUtils.GetString("AggregateCompressedFilesButNoCompression"));
		}
		// lock the files
		_filenames = Directory.GetFiles(path, searchPattern);
		foreach(var fileName in _filenames)
		{
			try
			{
				if(log.IsInfoEnabled)
				{
					log.Info("Locking file " + fileName);
				}
                var fileStream = OpenFile(fileName);
                _files.Add(fileName, fileStream);
            }
			catch
			{
				// ignored
			}
		}
		var fileDeleteTransaction = ResourceMonitor.GetTransaction(
			_transactionId, connection) 
			as FileDeleteTransaction;
		if(fileDeleteTransaction == null)
		{
			ResourceMonitor.RegisterTransaction(
				_transactionId, 
				connection, 
				new FileDeleteTransaction(_files));
		}
	}
    private static FileStream OpenFile(string fileName)
    {
        return File.Open(
            fileName, FileMode.Open, FileAccess.Read, FileShare.None);
    }
	public override void Disconnect()
	{
		if(_isLocalTransaction)
		{
			ResourceMonitor.Commit(_transactionId);
		}
	}
	public override WorkQueueAdapterResult GetItem(string lastState)
	{
		var dataTable = CreateFileDataset(_mode).Tables["File"];
		bool result;
		switch (_readType)
        {
            case ReadType.SingleFiles:
				result = RetrieveNextFile(dataTable, false);
				break;
			case ReadType.AggregateCompressedFiles:
				return RetrieveAggregate(dataTable, true);
            case ReadType.AggregateAllFiles:
				return RetrieveAggregate(dataTable, false);
			case ReadType.SplitByRows:
				result = RetrieveNextSegment(dataTable);
				break;
            default:
				throw new ArgumentOutOfRangeException(
					"readType", _readType, "Unknown ReadType value.");
        }
		return result
			? new WorkQueueAdapterResult(
				DataDocumentFactory.New(dataTable.DataSet))
			: null;
	}
    private WorkQueueAdapterResult RetrieveAggregate(DataTable dataTable,
		bool compressedOnly)
    {
		// retrieve files as multiple records
		while(RetrieveNextFile(dataTable, compressedOnly))	{} 
        if (dataTable.Rows.Count == 0)
        {
            return null;
        }
        // create an aggregated record with files as Data field
        var aggregatedDataTable
            = CreateFileDataset(_mode).Tables["File"];
		DataRow dataRow;
        if (compressedOnly)
        {
			string fileName = _filenames[_currentPosition - 1];
			dataRow = CreateAndInitializeDataRow(
				aggregatedDataTable, fileName, fileName);
		}
		else
        {
			dataRow = aggregatedDataTable.NewRow();
			dataRow["Name"] = $"Multiple files {DateTime.Now}";
		}
		dataRow["Data"] = dataTable.DataSet.GetXml();
        aggregatedDataTable.Rows.Add(dataRow);
        return new WorkQueueAdapterResult(
            DataDocumentFactory.New(aggregatedDataTable.DataSet));
    }
    private (Stream, string, string) GetNextFileStream(
		bool aggregateCompressedFilesOnly)
    {
    start:
		if(_currentPosition + 1 > _files.Count)
		{
			return (null, null, null);
		}
        var fileName = _filenames[_currentPosition];
        var title = fileName;
        var fileStream = (FileStream)_files[fileName];
        Stream finalStream;
        if(_compression == CompressionType.ZIP)
        {
            var zipStream = _currentZipStream;
            if(zipStream == null)
            {
                zipStream = new ZipInputStream(fileStream);
                _currentZipStream = zipStream;
            }
            var zipEntry = zipStream.GetNextEntry();
            if (zipEntry != null)
            {
                long maxBytes = GetMaxUncompressedBytes();
                if (zipEntry.Size >= 0 && zipEntry.Size > maxBytes)
                {
                    throw new InvalidOperationException("Archive entry too large.");
                }
                if (zipEntry.CompressedSize > 0 && zipEntry.Size >= 0)
                {
                    double ratio = zipEntry.Size / (double)zipEntry.CompressedSize;
                    if (ratio > MaxCompressionRatio)
                    {
                        throw new InvalidOperationException("Archive entry has suspicious compression ratio.");
                    }
                }
            }
            if(zipEntry == null)
            {
                _currentZipStream = null;
                zipStream.Close();
                ((IDisposable)zipStream).Dispose();
                _currentPosition++;
                if (aggregateCompressedFilesOnly)
                {
					return (null, null, null);
				}
				else
                {
					goto start;
				}
			}
            finalStream = zipStream;
            title += " " + zipEntry.Name;
        }
        else
        {
            finalStream = fileStream;
            _currentPosition++;
        }
        if(log.IsInfoEnabled)
        {
            log.Info("Reading file " + title);
        }
        return (finalStream, fileName, title);
    }
    
    private bool RetrieveNextSegment(DataTable dataTable)
    {
        if((_splitFileStreamReader == null)
		|| _splitFileStreamReader.EndOfStream)
        {
			var (stream, filename, title) = GetNextFileStream(false);
			if(stream == null)
			{
				return false;
			}
			_splitFileStreamReader = (_encoding == null)
				? new SplitFileStreamReader(filename, title, stream)
				: new SplitFileStreamReader(
					filename,
					title,
					stream,
					Encoding.GetEncoding(_encoding));
			if(_splitFileAndKeepHeader)
			{
				_splitFileStreamReader.ProcessHeader();
			}
        }
		AddTextFileSegmentFromStream(dataTable);
        return true;
    }
    private bool RetrieveNextFile(
		DataTable dataTable, bool aggregateCompressedFiles)
    {
        var (stream, filename, title) =
			GetNextFileStream(aggregateCompressedFiles);
        if(stream == null)
        {
	        return false;
        }
        AddFileFromStream(
            stream, dataTable, _mode, filename, title, _encoding);
        return true;
    }
    private void AddTextFileSegmentFromStream(DataTable dataTable)
    {
		var rowCounter = 0;
		using(var memoryStream = new MemoryStream())
		{
			var streamWriter = (_encoding == null)
				? new StreamWriter(memoryStream)
				: new StreamWriter(memoryStream,
					Encoding.GetEncoding(_encoding));
			if(_splitFileAndKeepHeader)
			{
				streamWriter.WriteLine(_splitFileStreamReader.Header);
			}
			while(!_splitFileStreamReader.EndOfStream 
		    && (rowCounter < _splitFileByRows))
			{
				streamWriter.WriteLine(_splitFileStreamReader.ReadLine());
				rowCounter++;
			}
			// last parts tend to remain in buffer, we need to flush them
			streamWriter.Flush();
			var dataRow = CreateAndInitializeDataRow(
				dataTable, _splitFileStreamReader.ProcessedFilename,
				_splitFileStreamReader.ProcessedFileTitle);
			dataRow["SequenceNumber"] 
				= _splitFileStreamReader.SegmentNumber;
			memoryStream.Position = 0;
			var internalStreamReader = (_encoding == null)
				? new StreamReader(memoryStream)
				: new StreamReader(memoryStream,
					Encoding.GetEncoding(_encoding));
			dataRow["Data"] = internalStreamReader.ReadToEnd();
			dataTable.Rows.Add(dataRow);
		}
    }
	public static DataSet GetFileFromStream(
		Stream stream, FileType mode, string filename, 
		string title, string encoding)
	{
        var dataTable = CreateFileDataset(mode).Tables["File"];
        AddFileFromStream(
            stream, dataTable, mode, filename, title, encoding);
		return dataTable.DataSet;
	}
    private static void AddFileFromStream(
        Stream stream, DataTable dataTable, FileType mode, string filename, 
        string title, string encoding)
    {
        AddFileToTable(stream, mode, filename, title, encoding, dataTable);
    }
    private static DataSet CreateFileDataset(FileType mode)
    {
        var dataSet = new DataSet("ROOT");
        var dataTable = dataSet.Tables.Add("File");
        dataTable.Columns.Add("Name", typeof(string));
        switch(mode)
        {
            case FileType.TEXT:
	            dataTable.Columns.Add("Data", typeof(string));
	            break;
            case FileType.BINARY:
	            dataTable.Columns.Add("Data", typeof(byte[]));
	            break;
        }
        // Add file metadata (times)			
        dataTable.Columns.Add("CreationTime", typeof(DateTime));
        dataTable.Columns.Add("LastWriteTime", typeof(DateTime));
        dataTable.Columns.Add("LastAccessTime", typeof(DateTime));
        dataTable.Columns.Add("SequenceNumber", typeof(int));
        return dataSet;
    }
    private static DataRow CreateAndInitializeDataRow(
        DataTable dataTable, string filename, string title)
    {
		var dataRow = dataTable.NewRow();
        if(File.Exists(filename))
        {
            // Add file metadata (times)			
            var fileInfo = new FileInfo(filename);
            dataRow["CreationTime"] = fileInfo.CreationTime;
            dataRow["LastWriteTime"] = fileInfo.LastWriteTime;
            dataRow["LastAccessTime"] = fileInfo.LastAccessTime;
        }
        if(!(dataRow["CreationTime"] is DateTime))
        {
            dataRow["CreationTime"] = DateTime.Now;
        }
        dataRow["Name"] = title;
        return dataRow;
    }
    private static void AddFileToTable(
        Stream stream, FileType mode, string filename, string title, 
        string encoding, DataTable dataTable)
    {
        var dataRow = CreateAndInitializeDataRow(
            dataTable, filename, title);
        switch(mode)
        {
            case FileType.TEXT:
	            ReadTextStream(stream, encoding, dataRow);
	            break;
            case FileType.BINARY:
	            ReadBinaryStream(stream, dataRow);
	            break;
        }
        dataTable.Rows.Add(dataRow);
    }
    private static void ReadBinaryStream(Stream stream, DataRow dataRow)
    {
        long maxBytes = GetMaxUncompressedBytes();
        byte[] data;
        if(stream.CanSeek)
        {
            if (stream.Length > maxBytes)
            {
                throw new InvalidOperationException("Stream exceeds allowed size.");
            }
            data = new byte[stream.Length];
            stream.Read(data, 0, Convert.ToInt32(stream.Length));
        }
        else
        {
            using(var memoryStream = new MemoryStream())
            {
                int count;
                long total = 0;
                do
                {
                    var buffer = new byte[1024];
                    count = stream.Read(buffer, 0, 1024);
                    if (count > 0)
                    {
                        total += count;
                        if (total > maxBytes)
                        {
                            throw new InvalidOperationException("Stream exceeds allowed size.");
                        }
                        memoryStream.Write(buffer, 0, count);
                    }
                } 
                while(stream.CanRead && count > 0);
                data = memoryStream.ToArray();
            }
        }
        dataRow["Data"] = data;
    }
    private static void ReadTextStream(
        Stream stream, string encoding, DataRow dataRow)
    {
        long maxBytes = GetMaxUncompressedBytes();
        Stream effective = stream;
        if (stream.CanSeek)
        {
            if (stream.Length > maxBytes)
            {
                throw new InvalidOperationException("Stream exceeds allowed size.");
            }
            stream.Position = 0;
        }
        else
        {
            effective = new LimitedReadStream(stream, maxBytes);
        }
        var streamReader = (encoding == null) 
	        ? new StreamReader(effective) 
	        : new StreamReader(effective, Encoding.GetEncoding(encoding));
        string content = streamReader.ReadToEnd();
        if (content.Length > maxBytes)
        {
            throw new InvalidOperationException("Stream exceeds allowed size.");
        }
        dataRow["Data"] = content;
    }
}
