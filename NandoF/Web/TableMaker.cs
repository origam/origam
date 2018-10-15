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

using ArgumentNullException       = System.ArgumentNullException;
using Type                        = System.Type;
using System.Collections;
using System.IO;
using System.Reflection;

namespace NandoF
{
	/// <summary>Creates an HTML table from a list of objects of a certain type
	/// and an ordered list of headers (corresponding to properties of the type).
	/// </summary>
	public class TableMaker
	{
		Type       t;
		TextWriter writer;
		string[]   headers;
		ArrayList  props;
		
		public TableMaker(Type t, TextWriter writer, params string[] headers)  {
			if (t==null)       throw new ArgumentNullException("t");
			if (writer==null)  throw new ArgumentNullException("writer");
			if (headers==null || headers.Length < 1)
				throw new ArgumentNullException("headers");
			this.t = t;
			this.writer  = writer;
			this.headers = headers;
			// For each header find and store a corresponding property
			props = new ArrayList(headers.Length);
			foreach (string header in headers)  {
				props.Add(t.GetProperty(header));
			}
		}
		
		virtual public void WriteHeaders()  {
			writer.Write("<thead>");
			foreach (string header in headers)  {
				writer.Write("<th>");
				writer.Write(header);
				writer.Write("</th>");
			}
			writer.WriteLine("</thead>");
		}
		
		virtual public void WriteRow (object o)  {
			// Get public instance properties of derived class.
			// For each property, show its value in a <td>
			// Put <tr> around the result
			writer.Write("<tr>");
			foreach (PropertyInfo prop in props)  {
				writer.Write("<td>");
				object val = prop.GetValue(o, null);
				if (val != null)  writer.Write(val.ToString());
				writer.Write("</td>");
			}
			writer.WriteLine("</tr>");
		}
		
		virtual public void WriteRows(IList  list)  {
			foreach (object o in list)  WriteRow(o);
		}
		
		#region IComparable implementation
		/*
		/// <summary>Comparison allows to sort objects derived from SelfDisplaying.
		/// It works by comparing the values of ToString().</summary>
		/// <param name="obj">The object to compare to</param>
		/// <returns>Less than zero if this instance is less than obj.
		/// Zero if this instance is equal to obj.
		/// Greater than zero if this instance is greater than obj.</returns>
		virtual  public int CompareTo(object obj)  {
			return (this.ToString().CompareTo(obj.ToString()));
		}
		*/
		#endregion
		
	}
}
