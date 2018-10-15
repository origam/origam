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
using System.Text.RegularExpressions;

namespace NandoF.Data
{
	public class NamedEmail
	{
		public   string Name {
			get { return name;  }
			set {
				if (value==null)  {
					name = "";
					return;
				}
				if (value.Length > 100)  throw new
					ApplicationException("O nome pode ter no máximo 100 caracteres.");
				name = value;
			}
		}
		private  string name  = string.Empty;
		
		public   string Email {
			get { return email;  }
			set {
				if (value==null)  {
					email = "";
					return;
				}
				if (value.Length > 100)  throw new
					ApplicationException("O e-mail pode ter no máximo 100 caracteres.");
				email = value.ToLower();
			}
		}
		private  string email = string.Empty;
		
		override public string ToString()  {
			if (Email==string.Empty)  return string.Empty;
			if (Name ==string.Empty)  return Email;
			return "\"" + Name + "\" <" + Email + ">";
		}
		
		public   bool   IsValid  {
			get  { return EmailAddressRegex.IsValidEmail(Email); }
		}
		
		/* Constructor*/ public NamedEmail(string email, string name) {
			Email = email;
			Name  = name;
		}
		/* Constructor*/ public NamedEmail(string email)  {
			Email = email;
		}
		
		static public NamedEmail Parse(string candidate)  {
			if (Text.IsNullOrEmpty(candidate))
				throw new ArgumentNullException("candidate");
			string c = candidate.Trim();
			// First, try with more complex regular expression
			Match m = QuotedNamedEmailRegex.Match(c);
			if (m.Success) return new NamedEmail(m.Groups["Email"].Value,
			                                     m.Groups["Name"].Value);
			// Secondly try to match only the e-mail
			m = EmailNoNameRegex.Match(c);
			if (m.Success) return new NamedEmail(m.Groups["Email"].Value);
			// Finally, try an incorrect but frequent form (without quotes)
			m = NamedEmailWithoutQuotesRegex.Match(c);
			if (m.Success) return new NamedEmail(m.Groups["Email"].Value,
			                                     m.Groups["Name"].Value);
			// Out of luck?
			throw new ApplicationException("Could not parse named e-mail: " + c);
		}
		// Captures name and e-mail from text like:  "Name" <e-mail>
		static public Regex  QuotedNamedEmailRegex = new Regex(WITH_QUOTES);
		public const  string WITH_QUOTES =
			@"^""(?<Name>[^""]+)""\s?<(?<Email>[\w\d.\-_]+@[\w\d.\-]+)>$";
		static public Regex  NamedEmailWithoutQuotesRegex =
			new Regex(WITHOUT_QUOTES);
		public const  string WITHOUT_QUOTES =
			@"^(?<Name>[^<^>]+)\s<(?<Email>[\w\d.\-_]+@[\w\d.\-]+)>$";
		//@"^(?<Name>[\w\d\s-,!_@\.]+)\s<(?<Email>[\w\d.\-_]+@[\w\d.\-]+)>$";
	//	@"^""?(?<Name>[\w\d\s-,_@\.]+)""?\s?<(?<Email>[\w\d.-_]+@[\w\d.-]+)>$";
	//  @"(""?(?<Name>.+?)""?\s*)?<(?<Email>[\w.-]+@[\w.-]+)>";
		public const string EMAIL_NO_NAME = @"^<?(?<Email>[\w.\-_]+@[\w.\-]+)>?$";
		static public Regex EmailNoNameRegex = new Regex(EMAIL_NO_NAME);
	}
}
