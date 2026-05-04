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
    public const int MaxByteFileLenghtToStore = 50000000;

    public static bool SaveToDataSet(
        string fullFileName,
        DataTable table,
        int RowIndex,
        string columnName
    )
    {
        if (table == null || table.Rows.Count < 1 || (table.Rows.Count + 1) < RowIndex)
        {
            return false;
        }

        return SaveToDataSet(
            fullFileName: fullFileName,
            dataRow: table.Rows[index: RowIndex],
            columnName: columnName
        );
    }

    public static bool SaveToDataSet(string fullFileName, DataRow dataRow, string columnName)
    {
        return SaveToDataSet(
            fullFileName: fullFileName,
            dataRow: dataRow,
            columnName: columnName,
            compress: false
        );
    }

    public static bool SaveToDataSet(
        string fullFileName,
        DataRow dataRow,
        string columnName,
        bool compress
    )
    {
        dataRow[columnName: columnName] = GetByteArrayFromFile(
            filePath: fullFileName,
            compress: compress
        );
        return true;
    }

    private static byte[] GetByteArrayFromFile(string filePath, bool compress)
    {
        FileStream fs = null;
        try
        {
            fs = new FileStream(path: filePath, mode: FileMode.Open, access: FileAccess.Read);
            if (compress)
            {
                return Zip(
                    input: fs,
                    fileName: Path.GetFileName(path: filePath),
                    dateCreated: File.GetCreationTimeUtc(path: filePath)
                );
            }
            BinaryReader br = new BinaryReader(input: fs);

            try
            {
                if (fs.Length > MaxByteFileLenghtToStore)
                {
                    throw new Exception(
                        message: ResourceUtils.GetString(
                            key: "FileTooBig",
                            args: MaxByteFileLenghtToStore.ToString()
                        )
                    );
                }
                return br.ReadBytes(count: (int)fs.Length);
            }
            finally
            {
                br.Close();
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "ErrorWhileReading",
                    args: filePath + Environment.NewLine + ex.Message
                ),
                innerException: ex
            );
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
        }
    }

    public static void SaveFromDataSet(
        string fullFileName,
        DataRow dataRow,
        string columnName,
        bool compressed
    )
    {
        byte[] bytes = (byte[])dataRow[columnName: columnName];
        if (bytes == null)
        {
            return;
        }

        if (File.Exists(path: fullFileName))
        {
            File.Delete(path: fullFileName);
        }
        ByteArrayToFile(fileName: fullFileName, bytes: bytes, compressed: compressed);
    }

    public static void ByteArrayToFile(string fileName, byte[] bytes, bool compressed)
    {
        if (compressed)
        {
            Unzip(input: bytes, fileName: fileName);
        }
        else
        {
            try
            {
                FileStream fs = new FileStream(
                    path: fileName,
                    mode: FileMode.OpenOrCreate,
                    access: FileAccess.Write
                );
                StreamTools.Write(output: fs, bytes: bytes);
                fs.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorWhileWriting",
                        args: new object[] { fileName, ex }
                    )
                );
            }
        }
    }

    private static byte[] Zip(Stream input, string fileName, DateTime dateCreated)
    {
        Crc32 crc = new Crc32();
        MemoryStream stream = new MemoryStream();
        ZipOutputStream zipStream = new ZipOutputStream(baseOutputStream: stream);
        zipStream.SetLevel(level: 9);
        BinaryReader br = new BinaryReader(input: stream);
        byte[] byteArray;
        try
        {
            byte[] buffer = new byte[input.Length];
            input.Read(buffer: buffer, offset: 0, count: buffer.Length);
            ZipEntry entry = new ZipEntry(name: @fileName);
            entry.DateTime = dateCreated;
            entry.Comment = fileName;
            entry.ZipFileIndex = 1;
            entry.Size = input.Length;
            crc.Reset();
            crc.Update(buffer: buffer);
            entry.Crc = crc.Value;
            zipStream.PutNextEntry(entry: entry);
            zipStream.Write(buffer: buffer, offset: 0, count: buffer.Length);
            zipStream.Finish();
            if (stream.Length > MaxByteFileLenghtToStore)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "FileTooBig",
                        args: MaxByteFileLenghtToStore.ToString()
                    )
                );
            }

            stream.Position = 0;
            byteArray = br.ReadBytes(count: (int)stream.Length);
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
        MemoryStream ms = new MemoryStream(buffer: input);
        ZipInputStream s = new ZipInputStream(baseInputStream: ms);
        ZipEntry entry = s.GetNextEntry();
        FileStream file = new FileStream(
            path: fileName,
            mode: FileMode.Create,
            access: FileAccess.Write
        );
        try
        {
            int size;
            byte[] data = new byte[2048];
            do
            {
                size = s.Read(buffer: data, offset: 0, count: data.Length);
                file.Write(array: data, offset: 0, count: size);
            } while (size > 0);
        }
        finally
        {
            if (s != null)
            {
                s.Close();
            }

            if (file != null)
            {
                file.Close();
            }

            if (ms != null)
            {
                ms.Close();
            }
        }
        File.SetCreationTime(path: fileName, creationTime: entry.DateTime);
    }
}
