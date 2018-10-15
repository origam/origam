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
	using ArrayList             = System.Collections.ArrayList;
	using ConfigurationSettings = System.Configuration.ConfigurationSettings;
	using DateTime              = System.DateTime;
	using Encoding              = System.Text.Encoding;
	using Environment           = System.Environment;
	using Exception             = System.Exception;
	using StringBuilder         = System.Text.StringBuilder;
	using StreamReader          = System.IO.StreamReader;
	using StreamWriter          = System.IO.StreamWriter;
	using TextWriter            = System.IO.TextWriter;
	using StringCollection      = System.Collections.Specialized.StringCollection;
	using Console               = System.Console;
	using Trace                 = System.Diagnostics.Trace;
	#endregion
	
	/// <summary>Wraps a TextWriter with logging functionality. The constructor
	/// takes a header; this header is written lazily, i.e. only when the
	/// first event is written to the log. If there are no events, the header
	/// does not get written. The Events property can be read at the end to know
	/// the number of logged items.</summary>
	public class Log : System.IDisposable
	{
		protected bool       headerWritten = false;
		protected string     header;
		protected TextWriter writer;
		protected Encoding   enc;
		protected string     fileName;
		
		protected uint       events = 0;
		public    uint       Events  {
			get  { return events; }
		}
		
		public /*constructor*/ Log(string header, TextWriter medium)  {
			if (header==null)  throw new ArgumentNullException("header");
			if (medium==null)  throw new ArgumentNullException("medium");
			this.header = header;
			this.writer = medium;
		}
		
		public /*constructor*/ Log(string header, string fileName, Encoding enc)  {
			if (header==null)  throw new ArgumentNullException("header");
			if (fileName==null || fileName==string.Empty)
				throw new ArgumentNullException("fileName");
			if (enc==null)     throw new ArgumentNullException("enc");
			this.header   = header;
			this.fileName = fileName;
			this.enc      = enc;
		}
		
		protected void WriteHeader()  {
			// First of all, make sure the TextWriter is initialized
			// (Possible lazy instantiation here)
			if (writer==null)  {
				writer = new StreamWriter(fileName, true, enc);
			}
			// Write to log a blank line...
			writer.WriteLine();
			if (header != string.Empty)  {
				// ...then the header, then an under line ===========
				writer.WriteLine(header);
				StringBuilder sb = new StringBuilder(header.Length + 10);
				sb.Insert(0, "=", header.Length);
				sb.Append(" ");
				// Add current date and time at the end
				sb.Append(IsoDate.ToDateTimeString(DateTime.Now));
				writer.WriteLine(sb.ToString());
			}
			headerWritten = true;
		}
		
		/*
		public    void Write      (string text)  {
			if (!headerWritten)  WriteHeader();
			writer.Write(text);
			events++;
		}
		*/
		
		public    void WriteLine  (string line)  {
			if (!headerWritten)  WriteHeader();
			writer.WriteLine(line);
			events++;
		}
		
		public    void Dispose    ()  {
			if (writer != null)  writer.Close();
			// Delete log file if empty
			if (fileName != null && System.IO.File.Exists(fileName))  {
				System.IO.FileInfo fi = new System.IO.FileInfo(fileName);
				if (fi.Length==0)  fi.Delete();
			}
		}
	}
}
