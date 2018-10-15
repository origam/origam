#region  NandoF library -- Copyright 2005-2006 Nando Florestan
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

namespace NandoF {
	
	#region Imported classes and namespaces
	using ApplicationException  = System.ApplicationException;
	using ArgumentNullException = System.ArgumentNullException;
	using Console               = System.Console;
	using Trace                 = System.Diagnostics.Trace;
	using DateTime              = System.DateTime;
	using Environment           = System.Environment;
	using Exception             = System.Exception;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	#endregion
	
	public class Filez
	{
		/// <summary>Ensures a directory string ends with "\" or current platforms'
		/// directory separator character.</summary>
		/// <param name="folder">Directory string to check (e.g. "C:\Docs")</param>
		/// <returns>e.g. "C:\Docs\"</returns>
		static public string GetDirEndingWithDelimiter(string folder)
		{
			if (folder==null)  throw new ArgumentNullException("folder");
			if (folder[folder.Length-1] == System.IO.Path.DirectorySeparatorChar)
				return folder;
			else return folder + System.IO.Path.DirectorySeparatorChar;
		}
		
		static public string ToValidName(string s)
		{
			if (s==null) throw new ArgumentNullException("s");
			string tmp = s;
			tmp = tmp.Replace("\r\n", "-").Replace("\r", "-").Replace("\n", "-");
			char[] c = new char[9] {'/', '\\', ':', '*', '?', '\"', '<', '>', '|' };
			for (int i = 0; i < c.Length; i++)  tmp = tmp.Replace(c[i], '_');
			tmp = tmp.Replace("&nbsp;", " ");
			return tmp;
		}
		
		static public string ToValidName(string s, char replaceSpaces)
		{
			if (s==null) throw new ArgumentNullException("s");
			string tmp = ToValidName(s);
			if (replaceSpaces!=' ') tmp = tmp.Replace(' ', replaceSpaces);
			return tmp;
		}
		
		/// <summary>If file "fileName" exists in directory "dir",
		/// adds or changes the end of the file title until a name is found
		/// that doesn't yet exist.</summary>
		/// <returns>The new file name (without directory).</returns>
		/// <remarks>For instance, if file "Image (01).jpg" exists,
		/// returns "Image (02).jpg".</remarks>
		static public string FindUniqueName(string dir, string fileName)  {
			if (Text.IsNullOrEmpty(dir))
				throw new ArgumentNullException("dir");
			if (Text.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");
			
			string ext     = Path.GetExtension(fileName);
			string title   = Path.GetFileNameWithoutExtension(fileName);
			string attempt = Path.Combine(dir, fileName);
			Regex   r = new Regex(@" \((?<number>\d{1,5})\)");
			
			while (File.Exists(attempt))  {
				MatchCollection mm = r.Matches(title);
				if (mm.Count == 0)  {
					title = title + " (001)";
				}  else  {
					Match  lastMatch = mm[mm.Count - 1];
					string number    = lastMatch.Groups["number"].Value;
					int    increment = int.Parse(number) + 1;
					number           = " (" + increment.ToString("000") + ")";
					title            = r.Replace(title, number, 1, lastMatch.Index);
				}
				attempt = Path.Combine(dir, title + ext);
			}
			return title + ext;
		}
		
		/// <param name="fileName">Example: "C:/MyHtml.htm"</param>
		/// <param name="addition">Example: " cleaned"</param>
		/// <returns>Example:  "C:/MyHtml cleaned.htm"</returns>
		static public string AddToTitle(string fileName, string addition)  {
			string dir = Path.GetDirectoryName(fileName);
			string tit = Path.GetFileNameWithoutExtension(fileName);
			string ext = Path.GetExtension(fileName);
			return Path.Combine(dir, tit + addition + ext);
		}
		
		static public string TextInFile     (string fileName)  {
			if (fileName==null) throw new ArgumentNullException("fileName");
			// Create an instance of StreamReader to read from a file.
			// The 'using' statement also closes the StreamReader.
			using (StreamReader sr = new StreamReader(fileName, true))  {
				return sr.ReadToEnd();
			}
		}
		
		static public string TextInFile     (string fileName,
		                                     System.Text.Encoding encoding)  {
			if (fileName==null) throw new ArgumentNullException("fileName");
			using (StreamReader sr = new StreamReader(fileName, encoding))  {
				return sr.ReadToEnd();
			}
		}
		
		static public ArrayList  LinesInFile(string fileName, bool allowBlankLines)
		{
			if (fileName==null) throw new ArgumentNullException("fileName");
			ArrayList a = new ArrayList(32);
			using (StreamReader sr = new StreamReader(fileName, true)) {
				while (sr.Peek() != -1) {
					string s = sr.ReadLine();
					if (!allowBlankLines && s==string.Empty) continue;
					a.Add(s);
				}
			}
			return a;
		}
		
		static public void   WriteTextToFile(string   lines, string fileName,
		                                     bool append,  Encoding encoding)   {
			if (fileName==null) throw new ArgumentNullException("fileName");
			if (lines   ==null) throw new ArgumentNullException("lines");
			using (StreamWriter w = new StreamWriter(fileName, append, encoding)) {
				w.Write(lines);
			}
		}
		static public void   WriteTextToFile(string   lines, string fileName,
		                                     bool append)  {
			WriteTextToFile(lines, fileName, append, Encoding.UTF8);
		}
		
		static public void   WriteTextToFile(string[] lines, string fileName,
		                                     bool append,  Encoding encoding)   {
			if (fileName==null) throw new ArgumentNullException("fileName");
			if (lines   ==null) throw new ArgumentNullException("lines");
			using (StreamWriter w = new StreamWriter(fileName, append, encoding)) {
				foreach (string line in lines)   w.WriteLine(line);
			}
		}
		static public void   WriteTextToFile(string[] lines, string fileName,
		                                     bool append)  {
			WriteTextToFile(lines, fileName, append, Encoding.UTF8);
		}
		
		/// <summary>Save byte content to a file</summary>
		/// <param name="file">File to be saved to</param>
		/// <param name="bytContent">Byte array content</param>
		static public void  WriteBytesToFile(byte[] bytes, string file) {
			if (Text.IsNullOrEmpty(file))  throw new ArgumentNullException("file");
			if (bytes == null)        throw new ArgumentNullException("bytes");
			// I don't think this line is necessary:
			// if (File.Exists(file))  File.Delete(file);
			using (FileStream fs = File.Create(file)) {
				fs.Write(bytes, 0, bytes.Length);
			}
		}
		
		static public byte[]   BytesFromFile(string file)  {
			// Read the whole file into a memory buffer
			using (FileStream fs = File.OpenRead(file))  {
				byte[] buffer = new byte[fs.Length];
				fs.Read(buffer, 0, buffer.Length);
				return buffer;
			}
		}
	}
	
}
