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

using ApplicationException   = System.ApplicationException;
using ArgumentNullException  = System.ArgumentNullException;
using IDisposable            = System.IDisposable;
using System.IO;
using System.Text;

namespace NandoF.Files
{
	/// <summary>Appears to be a text file that can be sequentially read from and
	/// written to.</summary>
	/// <remarks><para>This class is useful for reading lines from a text file,
	///  replacing lines and adding lines.</para>
	/// In fact, it reads lines from the input file and at the same time
	/// writes them to a temporary file. The interesting thing is that each line
	/// can be substituted with other text by calling ReplaceLine(). There is
	/// also a DeleteLine() method.
	/// <para>At any moment, you can call AddLine().</para>
	/// <para>Finally, when Dispose() is called, the input
	/// file is deleted and the temporary file becomes permanent, renamed to the
	/// input file name.</para>
	/// </remarks>
	public class LinesReaderWriter : IDisposable
	{
		string  file;
		string  tempFile;
		TextReader rd;
		TextWriter wr;
		
		// Constructor
		public LinesReaderWriter(string path, Encoding enc)  {
			if (Text.IsNullOrEmpty(path))  throw new ArgumentNullException("path");
			if (enc == null)               throw new ArgumentNullException("enc");
			this.file     = path;
			this.tempFile = path + ".TEMP";
			// If file doesn't exist, create a blank file
			if (!File.Exists(path))  {
				FileStream fs = File.OpenWrite(path);
				fs.Close();
			}
			rd = new StreamReader(path, enc);
			wr = new StreamWriter(tempFile, false, enc);
		}
		/// <summary>Constructor that uses the UTF-8 encoding for writing.</summary>
		public LinesReaderWriter(string path) : this(path, Encoding.UTF8) {}
		
		string line;
		
		private void  commitLine()  {
			if (line != null)  {
				wr.WriteLine(line);
				line = null;
			}
		}
		
		public string ReadLine()  {
			commitLine();
			// Put line in buffer and return it
			line = rd.ReadLine();
			return line;
		}
		
		/// <summary>Replaces the line that was read last with different text.
		/// </summary>
		public void ReplaceLine(string text)  {
			if (line==null)  throw new ApplicationException
				("Cannot replace line; it probably has already been consumed.");
			wr.WriteLine(text);
			line = null;
		}
		
		/// <summary>This method MUST be called immediately after a successful
		/// ReadLine() call. This means there must be no other calls to this object
		/// in between ReadLine() and DeleteLine().</summary>
		/// <remarks>An exception is thrown if the line has already been consumed.
		/// </remarks>
		public void DeleteLine()  {
			if (line==null)  throw new ApplicationException
				("Cannot delete line; it probably has already been consumed.");
			line = null;
		}
		
		public void AddLine  (string text)  {
			commitLine();
			wr.WriteLine(text);
		}
		
		public void ReadToEnd()  {
			while (ReadLine() != null)  {}
		}
		
		private bool disposed = false;
		
		/// <summary>Closes this object and its files.</summary>
		/// <remarks>If you are in the middle of the file, it will be truncated
		/// at that point. (This is a feature, not a bug.)
		/// To prevent this, ReadToEnd() should be called before
		/// calling Dispose().</remarks>
		public void Dispose()  {
			// Because Dispose() may be called more than once, it is important
			// not to crash here just because an object is already null
			if (!disposed)  {
				commitLine();
				rd.Close();
				rd = null;
				wr.Close();
				wr = null;
				string bak = file + ".bak";
				File.Move(file, bak);
				File.Move(tempFile, file);
				File.Delete(bak);
				disposed = true;
			}
		}
	}
}
