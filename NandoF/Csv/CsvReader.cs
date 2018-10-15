#region License
/* CsvReader
 * CsvReaderByHeader
 * 
 * Derived from JHLib 1.1, a public domain .NET tool library by Jouni Heikniemi
 * http://www.heikniemi.net/jhlib
 *
 * The class CsvReader, although changed by Nando Florestan,
 * remains in the public domain.
 * The main change: use a more general TextReader, instead of a Stream.
 * 
 * The class CsvReaderByHeader -- at the bottom -- was written from scratch by
 * Nando Florestan and is also donated to the public domain.
 * 
 * http://www.oui.com.br/n/
 * 
*/
#endregion

#region Imported classes and namespaces
using ApplicationException      = System.ApplicationException;
using ArgumentNullException     = System.ArgumentNullException;
using ArgumentException         = System.ArgumentException;
using Exception                 = System.Exception;
using IDisposable               = System.IDisposable;
using Serializable              = System.SerializableAttribute;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
#endregion

namespace NandoF.Csv
{
	/// <summary>A data reader for CSV files</summary>
	public class CsvReader : IDisposable
	{
		private TextReader reader;
		
		/// <summary>Separator character for the fields (usually comma).</summary>
		public  char Separator  {
			get { return separator;  }
			set { separator = value; }
		}
		private char separator = ',';
		
		#region Old Constructors
		/// <summary>Creates a new Csv reader for the given stream.</summary>
		/// <param name="s">The stream to read the CSV from.</param>
//		public CsvReader(Stream s) : this(s, null, ',') { }
//		
//		/// <summary>Creates a new reader for the given stream and separator.
//		/// </summary>
//		/// <param name="s">The stream to read the separator from.</param>
//		/// <param name="separator">The field separator character</param>
//		public CsvReader(Stream s, char separator) : this(s, null, separator) { }
//		
//		/// <summary>Creates a new Csv reader for the given stream and encoding.
//		/// </summary>
//		/// <param name="s">The stream to read the CSV from.</param>
//		/// <param name="enc">The encoding used.</param>
//		public CsvReader(Stream s, Encoding enc) : this(s, enc, ',') { }
//		
//		/// <summary>Creates a new reader for the given stream, encoding and
//		/// separator character.</summary>
//		/// <param name="s">The stream to read the data from.</param>
//		/// <param name="enc">The encoding used.</param>
//		/// <param name="separator">The separator character between the fields</param>
//		public CsvReader(Stream s, Encoding enc, char separator) {
//			
//			this.separator = separator;
//			this.stream = s;
//			if (!s.CanRead) {
//				throw new CsvReaderException("Could not read the given data stream!");
//			}
//			reader = (enc != null) ? new StreamReader(s, enc) : new StreamReader(s);
//		}
//		
//		/// <summary>Creates a new Csv reader for the given text file path.
//		/// </summary>
//		/// <param name="filename">The name of the file to be read.</param>
//		public CsvReader(string filename) : this(filename, null, ',') { }
//		
//		/// <summary>Creates a new reader for the given text file path and
//		/// separator character.</summary>
//		/// <param name="filename">The name of the file to be read.</param>
//		/// <param name="separator">The field separator character</param>
//		public CsvReader(string filename, char separator) : this(filename, null, separator) { }
//		
//		/// <summary>Creates a new Csv reader for the given text file path and
//		/// encoding.</summary>
//		/// <param name="filename">The name of the file to be read.</param>
//		/// <param name="enc">The encoding used.</param>
//		public CsvReader(string filename, Encoding enc)
//			: this(filename, enc, ',') { }
//		
//		/// <summary>Creates a new reader for the given text file path,
//		/// encoding and field separator.</summary>
//		/// <param name="filename">The name of the file to be read.</param>
//		/// <param name="enc">The encoding used.</param>
//		/// <param name="separator">The field separator character.</param>
//		public CsvReader(string filename, Encoding enc, char separator)
//			: this(new FileStream(filename, FileMode.Open), enc, separator) { }
//		
		#endregion
		
		#region Constructors written by Nando Florestan
		public CsvReader(TextReader reader)  {
			if (reader==null)  throw new ArgumentNullException("reader");
			this.reader = reader;
		}
		
		public CsvReader(string fileName, Encoding enc)  {
			if (fileName==null)  throw new ArgumentNullException("fileName");
			if (enc==null)  reader = new StreamReader(fileName, true);
			else            reader = new StreamReader(fileName, enc, true);
		}
		
		public CsvReader(string fileName) : this(fileName, null)  {}
		#endregion
		
		#region Parsing
		/// <summary>Returns the fields for the next row of data
		/// (or null if at end of file).</summary>
		/// <returns>An ArrayList of strings containing the fields,
		/// or null if at the end of file.</returns>
		public  ArrayList GetLine() {
			string data = reader.ReadLine();
			if (data == null) return null;
			// Ignore any empty lines...
			if (data.Length==0)  return new ArrayList(0);
			// ...instead of returning empty data as was done before:
//			if (data.Length == 0) return new string[0];
//			ArrayList result = new ArrayList();
//			ParseCsvFields(result, data);
//			return (string[])result.ToArray(typeof(string));
			return parseCsvFields(data);
		}
		
		// Parses the fields and returns them as an ArrayList
		private ArrayList parseCsvFields(string data)  {
			int pos = -1;
			ArrayList a = new ArrayList();
			while (pos < data.Length)
				a.Add(parseCsvField(data, ref pos)
				           .Replace("«¶»", System.Environment.NewLine));
			// ADDED BY NANDO FLORESTAN: Decoding of "«¶»" as a new line
			return a;
		}
		
		// Parses the field at the given position of the data, modifies pos to match
		// the first unparsed position and returns the parsed field
		private string parseCsvField(string data, ref int startSeparatorPosition)  {
			if (startSeparatorPosition == data.Length - 1) {
				startSeparatorPosition++;
				// The last field is empty
				return "";
			}
			
			int fromPos = startSeparatorPosition + 1;
			
			// Determine if this is a quoted field
			if (data[fromPos] == '"') {
				// If we're at the end of the string, let's consider this a field that
				// only contains the quote
				if (fromPos == data.Length-1) {
					fromPos++;
					return "\"";
				}
				
				// Otherwise, return a string of appropriate length with double quotes collapsed
				// Note that FSQ returns data.Length if no single quote was found
				int nextSingleQuote = findSingleQuote(data, fromPos + 1);
				startSeparatorPosition = nextSingleQuote + 1;
				return data.Substring(fromPos + 1, nextSingleQuote - fromPos - 1)
					.Replace("\"\"", "\"");
			}
			
			// The field ends in the next separator or EOL
			int nextSeparator = data.IndexOf(separator, fromPos);
			if (nextSeparator == -1) {
				startSeparatorPosition = data.Length;
				return data.Substring(fromPos);
			}
			else {
				startSeparatorPosition = nextSeparator;
				return data.Substring(fromPos, nextSeparator - fromPos);
			}
		}
		
		// Returns the index of the next single quote mark in the string
		// (starting from startFrom)
		static private int findSingleQuote(string data, int startFrom)  {
			int i = startFrom-1;
			while (++i < data.Length)
				if (data[i] == '"') {
				// If this is a double quote, bypass the chars
				if (i < data.Length-1 && data[i+1] == '"') {
					i++;
					continue;
				}
				else
					return i;
			}
			// If no quote found, return the end value of i (data.Length)
			return i;
		}
		#endregion
		
		/// <summary>Closes the reader. The underlying stream is closed.</summary>
		public void Dispose() {
			if (reader != null) reader.Close();
		}
	}
	
	
	/// <summary>Exception class for CsvReader exceptions.</summary>
	[Serializable]
	public class CsvReaderException : ApplicationException {
		
		/// <summary>Constructs a new CsvReaderException.</summary>
		public CsvReaderException() : this("CSV Reader error.") { }
		
		/// <summary>Constructs a new exception with the given message.</summary>
		/// <param name="message">The exception message.</param>
		public CsvReaderException(string message) : base(message) { }
		
		/// <summary>Constructs a new exception with the given message and
		/// the inner exception.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="inner">Inner exception that caused this issue.</param>
		public CsvReaderException(string message, Exception inner)
			: base(message, inner) { }
		
		/// <summary>Constructs a new exception with the given
		/// serialization information.</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected CsvReaderException(SerializationInfo info,
		                             StreamingContext context)
			: base(info, context) { }
		
	}
	
	
	/// <summary>Wraps a CsvReader, reads the first line of the CSV file,
	/// stores it as Headers and allows user code to refer to the information
	/// by header. For example:
	/// <code>
	/// CsvReaderByHeader csv = new CsvReaderByHeader(myFile);
	/// while (csv.NextRow())  {
	///   string name = csv["FirstName"] + " " + csv["Surname"];
	///   // Do something with name
	/// }
	/// csv.Dispose();
	/// </code>
	/// </summary>
	public class CsvReaderByHeader : IDisposable
	{
		const string COLSERROR = "{0} columns are expected, but " +
			"row #{1} in CSV file contains {2} columns.";
		
		protected CsvReader csv;
		protected Hashtable heads = new Hashtable(16);
		
		#region Constructors
		public CsvReaderByHeader(string fileName)  {
			csv = new CsvReader(fileName);
			init();
		}
		
		public CsvReaderByHeader(string fileName, Encoding enc)  {
			csv = new CsvReader(fileName, enc);
			init();
		}
		
		public CsvReaderByHeader(TextReader reader)  {
			csv = new CsvReader(reader);
			init();
		}
		#endregion
		
		protected string[]  headers;
		public    string[]  Headers {
			get { return headers; }
		}
		
		virtual protected void init()  {
			// First line of CSV file should contain headers. Remember them
//			headers = csv.GetCsvLine();
			ArrayList a = csv.GetLine();
			headers = a.ToArray(typeof(string)) as string[];
			int index = 0;
			foreach(string header in headers)  {
				if (header != string.Empty)  this.heads.Add(header, index);
				index++;
			}
			columns = index;
		}
		
		protected int       columns;
		public    int       Columns  {
			get  { return columns; }
		}
		
		protected ArrayList currentRow;
		public    ArrayList CurrentRow  {
			get  { return currentRow;  }
		}
		
		public    bool      HasHeader(string h)  {
			foreach (string header in headers)  {
				if (header==h)  return true;
			}
			return false;
		}
		
		protected int       rowNumber = 1;
		
		public    bool      NextRow()  {
			// Read a line from CSV file
			ArrayList vals = csv.GetLine();
			// If there is no line, return false
			if (vals==null)  {
				currentRow = null;
				return false;
			}
			rowNumber++;
			// If there is a line, verify that the number of columns is valid
			if (vals.Count != Columns)  throw new ApplicationException
				(string.Format(COLSERROR, Columns, rowNumber, vals.Count));
			// Write line to CurrentRow and return true
			currentRow = vals;
			return true;
		}
		
		public    string    this[string header]  {
			get  {
				/* if (CurrentRow==null)  throw new ApplicationException
					("Cannot read a value because there is no current row."); */
				if (CurrentRow==null)  return null;
				object obj = heads[header];
				if (obj==null) throw new ArgumentException
					("Header '" + header + "' does not exist in CSV file.");
				int column = (int)obj;
				return CurrentRow[column] as string;
			}
		}
		
		public    void      Dispose()  {
			csv.Dispose();
		}
	}
}
