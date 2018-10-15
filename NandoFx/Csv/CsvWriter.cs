/* Adapted from JHLib 1.1, a public domain .NET tool library by Jouni Heikniemi
 * http://www.heikniemi.net/jhlib
 */

using ArgumentNullException   = System.ArgumentNullException;
using IDisposable             = System.IDisposable;
using TextWriter              = System.IO.TextWriter;
using IList                   = System.Collections.IList;

namespace NandoF.Csv
{
	/// <summary>Component for writing Csv and other char-separated field files.
	/// </summary>
	public class CsvWriter : IDisposable {
		
		protected readonly TextWriter writer;
		protected readonly char[]     problemChars;
		protected readonly char       separator;
		/// <summary>Character that separates the fields; usually comma.</summary>
		public char Separator  {
			get { return separator; }
		}
		
		public void Dispose()  { writer.Close(); }
		
		#region Constructors
		/// <param name="writer">Anything derived from TextWriter,
		/// such as StreamWriter and StringWriter.</param>
		/// <param name="separator">Character that separates the fields.</param>
		public CsvWriter(TextWriter writer, char separator)  {
			if (writer==null)     throw new ArgumentNullException("writer");
			if (separator=='\"')  throw new System.ArgumentOutOfRangeException
				("separator", "The separator cannot be a quote.");
//			if (fields <= 0)      throw new System.ArgumentOutOfRangeException
//				("fields", "The number of CSV fields must be higher than zero.");
			this.writer    = writer;
			this.separator = separator;
			problemChars   = new char[] { Separator, '"' };
		}
		
		public CsvWriter(TextWriter writer)
			: this(writer, ',')  {}
		#endregion
		
		protected void   writeField(object o)  {
			string s = (o != null ? o.ToString().Trim() : string.Empty);
			if (s.IndexOfAny(problemChars) >= 0)
				// We have to quote the string
				s = "\"" + s.Replace("\"", "\"\"") + "\"";
			// The field cannot contain new line characters either
			s = s.Replace("\r\n", "«¶»").Replace("\r", "«¶»").Replace("\n", "«¶»");
			writer.Write(s);
		}
		
		protected int    fields = 0;
		
		protected void   checkFieldCount(int count)  {
			if (count < 1)  throw new System.ArgumentException
				("Cannot write a CSV line with zero fields.");
			if (fields==0)  fields = count;
			else  {
				if (count != fields)  throw new System.ArgumentException
					(fields.ToString() + " fields expected, got " +
					 count.ToString());
			}
		}
		
		/// <summary>Writes a CSV line, separating the given fields, quoting them
		/// when necessary, ensuring the number of fields is always the same,
		/// and substituting any new-line-chars for «¶»</summary>
		public    void   WriteLine (params object[] content)  {
			if (content==null)       throw new ArgumentNullException("content");
			checkFieldCount(content.Length);
			for (int i = 0; i < content.Length; ++i)  {
				writeField(content[i]);
				// Write the separator unless we're at the last position
				if (i < content.Length-1)  writer.Write(Separator);
			}
			writer.Write(writer.NewLine);
		}
		
		public    void   WriteLine (IList content)  {
			if (content==null)       throw new ArgumentNullException("content");
			checkFieldCount(content.Count);
			for (int i = 0; i < content.Count; ++i)   {
				writeField(content[i]);
				// Write the separator unless we're at the last position
				if (i < content.Count-1)  writer.Write(Separator);
			}
			writer.Write(writer.NewLine);
		}
		
		/// <summary>Writes a line directly; useful for writing the CSV header.
		/// </summary>
		public    void   WriteLine (string line)  {
			writer.WriteLine(line);
		}
		
	}
}
