#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Xml;
using System.Data;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue
{
	/// <summary>
	/// Summary description for WorkQueueFileLoader.
	/// </summary>
	public class WorkQueueFileLoader : WorkQueueLoaderAdapter
	{
		private int _currentPosition = 0;
        private bool _aggregationExecuted = false;
		private string[] _fileNames;
		private Hashtable _files = new Hashtable();
		private string _transactionId;
		private bool _isLocalTransaction = false;
		private string _mode = null;
        private bool _aggregate = false;
        private string _encoding = null;
        public const string MODE_TEXT = "TEXT";
        public const string MODE_BINARY = "BINARY";
        private string _compression = null;
        private const string COMPRESSION_ZIP = "ZIP";
        private ZipInputStream _currentZipStream = null;

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public WorkQueueFileLoader()
		{
		}

        public override void Connect(IWorkQueueService service, Guid queueId, 
            string workQueueClass, string connection, string userName, string password, 
            string transactionId)
		{
			if(log.IsInfoEnabled)
			{
				log.Info("Connecting " + connection);
			}

            _transactionId = transactionId;
            if (_transactionId == null)
            {
                _transactionId = Guid.NewGuid().ToString();
                _isLocalTransaction = true;
            }
            
            string path = null;
			string searchPattern = null;

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
						case "searchPattern":
							searchPattern = pair[1];
							break;
						case "mode":
							_mode = pair[1].ToUpper();

							if(_mode != MODE_TEXT && _mode != MODE_BINARY)
                                throw new ArgumentOutOfRangeException("mode", _mode, 
                                    ResourceUtils.GetString("ErrorUnknownAccessMode"));

							break;
						case "encoding":
							_encoding = pair[1];
							break;
                        case "compression":
                            _compression = pair[1].ToUpper();
                            break;
                        case "aggregate":
                            _aggregate = Convert.ToBoolean(pair[1]);
                            break;

						default:
							throw new ArgumentOutOfRangeException("connectionParameterName", pair[0], ResourceUtils.GetString("ErrorInvalidConnectionString"));
					}
				}
			}

			if(path == null) throw new Exception(ResourceUtils.GetString("ErrorNoPath"));
			if(searchPattern == null) throw new Exception(ResourceUtils.GetString("ErrorNoSearchPattern"));

			// lock the files
			_fileNames = Directory.GetFiles(path, searchPattern);

			foreach(string fileName in _fileNames)
			{
				try
				{
					if(log.IsInfoEnabled)
					{
						log.Info("Locking file " + fileName);
					}

                    FileStream fs = OpenFile(fileName);
                    _files.Add(fileName, fs);
                }
				catch
				{
				}
			}

			FileDeleteTransaction fdt = ResourceMonitor.GetTransaction(_transactionId, connection) as FileDeleteTransaction;

			if(fdt == null)
			{
				ResourceMonitor.RegisterTransaction(_transactionId, connection, new FileDeleteTransaction(_files));
			}
		}

        private static FileStream OpenFile(string fileName)
        {
            return File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
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
            DataTable dt = CreateFileDataset(_mode);

            if(_aggregate)
            {
                if (_aggregationExecuted) return null;

                // retrieve all files as multiple records
                while (RetrieveNextFile(dt))
                {
                }
                if (dt.Rows.Count == 0) return null;
                // create an aggregated record with files as Data field
                DataTable aggregatedDt = CreateFileDataset(_mode);
                DataRow row = aggregatedDt.NewRow();
                row["Name"] = "Multiple files " + DateTime.Now.ToString();
                row["Data"] = dt.DataSet.GetXml();
                aggregatedDt.Rows.Add(row);
                _aggregationExecuted = true;
                return new WorkQueueAdapterResult(new XmlDataDocument(aggregatedDt.DataSet));
            }
            else
            {
                return RetrieveNextSingleFile(dt);
            }
        }

        private WorkQueueAdapterResult RetrieveNextSingleFile(DataTable dt)
        {
            bool result = RetrieveNextFile(dt);
            if (result)
            {
                return new WorkQueueAdapterResult(new XmlDataDocument(dt.DataSet));
            }
            else
            {
                return null;
            }
        }

        private bool RetrieveNextFile(DataTable dt)
        {
        start:
            if (_currentPosition + 1 > _files.Count) return false;
            string fileName = _fileNames[_currentPosition];
            string title = fileName;
            FileStream fs = (FileStream)_files[fileName];
            Stream finalStream;
            if (_compression == COMPRESSION_ZIP)
            {
                ZipInputStream zipStream = _currentZipStream;
                if (zipStream == null)
                {
                    zipStream = new ZipInputStream(fs);
                    _currentZipStream = zipStream;
                }
                ZipEntry zipEntry = zipStream.GetNextEntry();
                if (zipEntry == null)
                {
                    _currentZipStream = null;
                    zipStream.Close();
                    ((IDisposable)zipStream).Dispose();
                    _currentPosition++;
                    goto start;
                }
                finalStream = zipStream;
                title += " " + zipEntry.Name;
            }
            else
            {
                finalStream = fs;
                _currentPosition++;
            }

            if (log.IsInfoEnabled)
            {
                log.Info("Reading file " + title);
            }

            AddFileFromStream(finalStream, dt, _mode, fileName, title, _encoding);
            return true;
        }

		public static DataSet GetFileFromStream(Stream stream, string mode, string fileName, string title, string encoding)
		{
            DataTable dt = CreateFileDataset(mode);
            AddFileFromStream(stream, dt, mode, fileName, title, encoding);
			return dt.DataSet;
		}

        private static void AddFileFromStream(Stream stream, DataTable dt, string mode, string fileName, string title, string encoding)
        {
            AddFileToTable(stream, mode, fileName, title, encoding, dt);
        }

        private static DataTable CreateFileDataset(string mode)
        {
            DataSet ds = new DataSet("ROOT");
            DataTable dt = ds.Tables.Add("File");
            dt.Columns.Add("Name", typeof(string));
            if (mode == MODE_TEXT)
            {
                dt.Columns.Add("Data", typeof(string));
            }
            else if (mode == MODE_BINARY)
            {
                dt.Columns.Add("Data", typeof(byte[]));
            }

            // Add file metadata (times)			
            dt.Columns.Add("CreationTime", typeof(DateTime));
            dt.Columns.Add("LastWriteTime", typeof(DateTime));
            dt.Columns.Add("LastAccessTime", typeof(DateTime));
            return dt;
        }

        private static void AddFileToTable(Stream stream, string mode, string fileName, string title, string encoding, DataTable dt)
        {
            DataRow row = dt.NewRow();

            try
            {
                // Add file metadata (times)			
                FileInfo fInfo = new FileInfo(fileName);
                row["CreationTime"] = fInfo.CreationTime;
                row["LastWriteTime"] = fInfo.LastWriteTime;
                row["LastAccessTime"] = fInfo.LastAccessTime;
            }
            catch
            {
            }

            row["Name"] = title;
            if (mode == MODE_TEXT)
            {
                ReadTextStream(stream, encoding, row);
            }
            else if (mode == MODE_BINARY)
            {
                ReadBinaryStream(stream, row);
            }
            dt.Rows.Add(row);
        }

        private static void ReadBinaryStream(Stream stream, DataRow row)
        {
            byte[] data;
            if (stream.CanSeek)
            {
                data = new byte[stream.Length];
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        byte[] buf = new byte[1024];
                        count = stream.Read(buf, 0, 1024);
                        ms.Write(buf, 0, count);
                    } while (stream.CanRead && count > 0);
                    data = ms.ToArray();
                }
            }
            row["Data"] = data;
        }

        private static void ReadTextStream(Stream stream, string encoding, DataRow row)
        {
            StreamReader sr;
            if (encoding == null)
            {
                sr = new StreamReader(stream);
            }
            else
            {
                sr = new StreamReader(stream, Encoding.GetEncoding(encoding));
            }

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            row["Data"] = sr.ReadToEnd();
        }

        private class FileEntry
        {
            public FileEntry(string fileName, string subFileName)
            {
                FileName = fileName;
                SubFileName = subFileName;
            }

            public string FileName;
            public string SubFileName;
        }
    }
}
