#region  NandoF library -- © 2006 Nando Florestan
/*
This library is free software; you can redistribute it and/or modify
it under the terms of the Lesser GNU General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This software is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program; if not, see http://www.gnu.org/copyleft/lesser.html
 */
#endregion

using System;
using System.IO;

namespace NandoF.Files
{
	/// <summary>Creates "File IDs" (hashcodes for file contents), stores these
	/// IDs in a binary file, allows user code to AddFileID(), and can answer if
	/// IsFileKnown (whether a certain file ID has already been recorded).
	/// Only file content is considered; file names are irrelevant.
	/// </summary>
	public class FileIDManager : IDisposable
	{
		/// <param name="path">A string containing the path and file name of the
		/// binary database to be used by this instance.</param>
		public FileIDManager(string path)  {
			if (Text.IsNullOrEmpty(path))  throw new ArgumentNullException("path");
			fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			ms = new MemoryStream(24);
			bw = new BinaryWriter(ms);
		}
		
		FileStream    fs; // for writing to and reading from the file
		MemoryStream  ms; // for creating file IDs
		BinaryWriter  bw; // for writing to ms
		byte[]        buffer = new byte[24];
		
		public void   Dispose()  {
			bw.Close();
			fs.Close();
		}
		
		public byte[] GetFileIDFor(byte[] fileContent)  {
			if (fileContent==null)
				throw new ArgumentNullException("fileContent");
//			if (fileContent.Length < 1)
//				throw new ApplicationException("File is empty.");
			ms.Position = 0;
			bw.Write(fileContent.Length);           //  4 bytes
			bw.Write(sha.ComputeHash(fileContent)); // 20 bytes
			bw.Flush();                             // 24 bytes total
			return ms.ToArray();
		}
		
		static private System.Security.Cryptography.SHA1Managed sha =
			new System.Security.Cryptography.SHA1Managed();
		
		public bool IsFileKnown(byte[] fileID)  {
			fs.Position = 0;
			while (fs.Read(buffer, 0, buffer.Length) == buffer.Length)  {
//				if (fileID.Equals(buffer))  return true;
				bool equal = true;
				for (int i = 0;  i < buffer.Length;  ++i)  {
					if (fileID[i] != buffer[i])  {
						equal = false;
						break;
					}
				}
				if (equal)  return true;
			}
			return false;
		}
		
		public void AddFileID  (byte[] fileID)  {
			if (fileID.Length != buffer.Length)  throw new ArgumentOutOfRangeException
				("fileID", "fileID should have " + buffer.Length.ToString() +
				 "bytes but has "                + fileID.Length.ToString() + "bytes.");
			fs.Position = fs.Length;            // go to end of file
			fs.Write(fileID, 0, fileID.Length); // append the ID
			fs.Flush();                         // really write to disk
		}
	}
}
