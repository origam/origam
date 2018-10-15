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

using ApplicationException  = System.ApplicationException;
using ArgumentNullException = System.ArgumentNullException;

namespace NandoF.Data
{
	/// <summary>Represents a name and value pair of strings.
	/// Useful for MANY situations.</summary>
	public class NameVal : System.IComparable
	{
		public  string Name {
			get { return name;  }
			set { name = value; }
		}
		private string name  = string.Empty;
		
		public  string Val {
			get { return val;  }
			set { val = value; }
		}
		private string val   = string.Empty;
		
		/* Constructor */ public NameVal(string name, string val) {
			this.name = name;
			this.val = val;
		}
		/* Constructor */ public NameVal(string name)  {
			this.name = name;
		}
		/* Constructor */ public NameVal()  {}
		
		/// <summary>Divides a string at the first occurrence of a separator,
		/// putting the result in a new NameVal object, with the left part of the
		/// string in Name and the right part in Val.</summary>
		static public NameVal Parse(string divisible, string separator)  {
			if (divisible==null)  throw new ArgumentNullException("divisible");
			if (separator==null)  throw new ArgumentNullException("separator");
			int    pos = divisible.IndexOf(separator);
			if (pos < 0)  throw new ApplicationException
				("String does not contain the separator: " + separator);
			return new NameVal(divisible.Substring(0, pos),
			                   divisible.Substring(pos + separator.Length));
		}
		
		public override string  ToString()  {
			return ToString(" » ");
		}
		public          string  ToString(string separator)  {
			return Name + separator + Val;
		}
		
		public          int    CompareTo(object o)  {
			int c = Name.CompareTo(o);
			if (c != 0)  return c; // Comparing the Name is enough most of the time.
			// But if the result is 0 (equality), try to compare the Val too
			NameVal nv = o as NameVal;
			if (nv != null)  return Val.CompareTo(nv.Val);
			else             return c;
		}
	}
}
