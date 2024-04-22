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
using System.Data;
using System.IO;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

namespace Origam.DA;

public class ByteArrayConverter
{
	public const int MaxByteFileLenghtToStore=50000000;

	public static bool SaveToDataSet (string fullFileName, DataTable table,int RowIndex, string columnName)
	{
			if ( table == null || table.Rows.Count < 1 || (table.Rows.Count + 1) < RowIndex )
				return false;

			return SaveToDataSet (fullFileName, table.Rows[RowIndex], columnName);
		}

	public static bool SaveToDataSet (string fullFileName, DataRow dataRow, string columnName)
	{
			return SaveToDataSet(fullFileName, dataRow, columnName, false);
		}
			
	public static bool SaveToDataSet (string fullFileName, DataRow dataRow, string columnName, bool compress)
	{
			dataRow[columnName] = GetByteArrayFromFile(fullFileName, compress);
			return true;
		}


	private static byte[] GetByteArrayFromFile(string filePath, bool compress)
	{
			FileStream fs = null;
			try
			{				
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

				if(compress)
				{
					return Zip(fs, Path.GetFileName(filePath), File.GetCreationTimeUtc(filePath));
				}
				else
				{
					BinaryReader br = new BinaryReader(fs);
					try
					{
						if(fs.Length > MaxByteFileLenghtToStore)
						{
							throw new Exception (ResourceUtils.GetString("FileTooBig", MaxByteFileLenghtToStore.ToString()));
						}

						return br.ReadBytes((int)fs.Length);
					}
					finally
					{
						br.Close();
					}
				}
			}
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorWhileReading", filePath + Environment.NewLine + ex.Message),ex);
			}
			finally
			{
				if(fs != null) fs.Close();
			}
		}

	public static void SaveFromDataSet (string fullFileName, DataRow dataRow, string columnName, bool compressed)
	{
			byte[] bytes = (byte[])dataRow[columnName];

			if(bytes == null) return;

			if( File.Exists(fullFileName) )
			{
				File.Delete(fullFileName);
			}

			ByteArrayToFile(fullFileName, bytes, compressed);
		}

	public static void ByteArrayToFile (string fileName, byte[] bytes, bool compressed)
	{
			if(compressed)
			{
				Unzip(bytes, fileName);
			}
			else
			{
				try
				{
					FileStream fs = new FileStream(fileName , FileMode.OpenOrCreate, FileAccess.Write);
					StreamTools.Write(fs, bytes);
					fs.Close();
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhileWriting", fileName,ex));
				}
			}
		}

	private static byte[] Zip(Stream input, string fileName, DateTime dateCreated)
	{
			Crc32 crc = new Crc32();
			MemoryStream stream = new MemoryStream();
			ZipOutputStream zipStream = new ZipOutputStream(stream);
			zipStream.SetLevel(9);
			BinaryReader br = new BinaryReader(stream);
			byte[] byteArray;

			try
			{
				byte[] buffer = new byte[input.Length];
				input.Read(buffer, 0, buffer.Length);
				ZipEntry entry = new ZipEntry(@fileName);
				entry.DateTime = dateCreated;
				entry.Comment = fileName;
				entry.ZipFileIndex = 1;

				entry.Size = input.Length;

				crc.Reset();
				crc.Update(buffer);

				entry.Crc = crc.Value;

				zipStream.PutNextEntry(entry);

				zipStream.Write(buffer, 0, buffer.Length);
				zipStream.Finish();

				if(stream.Length > MaxByteFileLenghtToStore)
					throw new Exception (ResourceUtils.GetString("FileTooBig", MaxByteFileLenghtToStore.ToString()));

				stream.Position = 0;
				byteArray = br.ReadBytes((int)stream.Length);
			}
			finally
			{
				zipStream.Close();
				br.Close();
				stream.Close();
				stream = null;
				br = null;
				zipStream = null;
			}

			return byteArray;
		}

	private static void Unzip(byte[] input, string fileName)
	{
			MemoryStream ms = new MemoryStream(input);
			ZipInputStream s = new ZipInputStream(ms); 

			ZipEntry entry = s.GetNextEntry();
			FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);

			try
			{	
				int size;
				byte[] data = new byte[2048];

				do
				{
					size = s.Read(data, 0, data.Length);
					file.Write(data, 0, size);
				} while (size > 0);
			}
			finally
			{
				if(s != null) s.Close();
				if(file != null) file.Close();
				if(ms != null) ms.Close();
			}

			File.SetCreationTime(fileName, entry.DateTime);
		}
}