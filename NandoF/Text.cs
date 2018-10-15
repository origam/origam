#region Nando Florestan Library: Common Functions
// Copyright (C) 2005  Nando Florestan
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License VERSION 2
// as published by the Free Software Foundation.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, read it on the web:
// http://www.gnu.org/copyleft/gpl.html

/*
 * Created in SharpDevelop:
 *   http://www.icsharpcode.net/OpenSource/SD/
 * Author: Nando Florestan:
 *   http://oui.com.br/n/
 */
#endregion

#region using

using ArgumentNullException = System.ArgumentNullException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using ConfigurationSettings = System.Configuration.ConfigurationSettings;
using Convert               = System.Convert;
using DateTime              = System.DateTime;
using Encoding              = System.Text.Encoding;
using Environment           = System.Environment;
using Exception             = System.Exception;
using Random                = System.Random;
using StringBuilder         = System.Text.StringBuilder;
using StringCollection      = System.Collections.Specialized.StringCollection;
using System.Text.RegularExpressions;
using System.Reflection;
#endregion

namespace NandoF
{
	public class EmailAddressRegex
	{
		static protected string ALLOWED = @"^[\w-.@]*$";
		static protected Regex  allowedCharacters;
		static protected Regex  AllowedCharacters  {
			get  {
				if (allowedCharacters==null)
					allowedCharacters = new Regex(ALLOWED);
				return allowedCharacters;
			}
		}
		static public    bool   HasOnlyEmailChars(string address)  {
			if (address==null) throw new ArgumentNullException("address");
			return AllowedCharacters.IsMatch(address);
		}
		
		static protected string VALIDATOR =
			@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
		static protected Regex  emailAddressValidator;
		static protected Regex  EmailAddressValidator  {
			get  {
				if (emailAddressValidator == null)
					emailAddressValidator = new Regex(VALIDATOR);
				return emailAddressValidator;
			}
		}
		static public    bool   IsValidEmail   (string address)  {
			if (address==null) throw new ArgumentNullException("address");
			return EmailAddressValidator.IsMatch(address);
		}
		
	}
	
	
	public class Text
	{
		/*
		public const  string EMAILREGEX =
			@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
		
		static public bool          IsValidEmail   (string address)  {
			if (address==null) throw new ArgumentNullException("address");
			return Regex.IsMatch(address, EMAILREGEX);
		}
		*/
		
		/// <summary>.NET Framework version 2 introduced the incredibly useful
		/// static method string.IsNullOrEmpty() and I thought previous versions
		/// of the framework also need it...</summary>
		static public bool          IsNullOrEmpty  (string s)  {
			return (s==null || s==string.Empty);
		}
		 
		static public StringBuilder FromStringArray(string[] input)  {
			if (input==null) throw new ArgumentNullException("input");
			StringBuilder sb = new StringBuilder(1000);
			if (input.Length < 1)  return sb;
			foreach (string s in input)  sb.Append(s + Environment.NewLine);
			// It is very important to remove the last NewLine
			sb.Length = sb.Length - Environment.NewLine.Length;
			return sb;
		}
		
		static public string        FromArray      (string[] input)  {
			return FromStringArray(input).ToString();
		}
		
		/// <summary>Separates the lines of a string into a StringCollection.</summary>
		/// <param name="input">The string to be divided.</param>
		/// <returns>A StringCollection containing the lines.</returns>
		static public StringCollection LinesInString (string input)  {
			StringCollection col = new StringCollection();
			if (input==null)  return col;
			int           cursor = 0;
			int       newLinePos = input.IndexOf(System.Environment.NewLine);
			// If there is only one line, add it to the collection and get out
			if (newLinePos == -1)  {
				col.Add(input);
				return col;
			}
			// Else, do the job
			while (newLinePos != -1)  {
				col.Add(input.Substring(cursor, newLinePos - cursor));
				cursor     = newLinePos + System.Environment.NewLine.Length;
				newLinePos = input.IndexOf(Environment.NewLine, cursor);
			}
			col.Add(input.Substring(cursor));
			return col;
			//return input.Split('\r', '\n');
		}
		
		static public string        LeaveOnlyDigits(string s) {
			if (s==null) throw new ArgumentNullException("s");
			StringBuilder sb = new StringBuilder(64);
			foreach(char c in s) {
				if(c<=0 && c>=9) sb.Append(c);
			}
			return sb.ToString();
		}
		
		static public string        CapsFirstChar  (string word)  {
			// Do nothing if empty
			if (word==null  ||  word==string.Empty)  return word;
			// Turn "word" into "Word"
			return word[0].ToString().ToUpper() + word.Substring(1);
		}
		
		static public string        CapsFirstLetter(string word)  {
			// Turn  "- item"  into  "- Item"
			// But do nothing if empty input
			if (word==null  ||  word==string.Empty)  return word;
			StringBuilder sb = new StringBuilder(word.Length);
			int pos = -1;
			// Find first letter (may be after some characters like ")
			foreach (char c in word)  {
				pos++;
				string letter = c.ToString();
				// Turn first LETTER into upper case
				if (Regex.IsMatch(letter, @"^\w{1}$"))  {
					sb.Append(letter.ToUpper());
					break;
				}
				else  sb.Append(c);
			}
			sb.Append(word.Substring(pos + 1));
			return sb.ToString();
		}
		
		static public string        CapsLikeName   (string s)  {
			if (s==null || s==string.Empty)  return s;
			const string delimiters = " -\t\r\n";
			StringBuilder name = new StringBuilder(s.Length);
			StringBuilder word = new StringBuilder(256);
			foreach (char c in s)  {
				if (delimiters.IndexOf(c) == -1)  {
					// Add current letter to word and continue
					word.Append(c);
					continue;
				}
				else  {
					// Delimiter found. Add current word to name and continue
					string piece = intelligentCaps(word.ToString());
					name.Append(piece);
					name.Append(c);
					word.Length = 0;
				}
			}
			string last = intelligentCaps(word.ToString());
			name.Append(last);
			return CapsFirstLetter(name.ToString());
		}
		
		static private string       intelligentCaps(string word)  {
			string piece = word.ToLower();
			if (piece=="de"   || piece=="da"   || piece=="das" || piece=="dos" ||
			    piece=="por"  || piece=="que"  || piece=="porque"              ||
			    piece=="para" || piece=="com"  || piece=="for" ||
			    piece=="em"   || piece=="with" || piece=="sem" || piece=="in"  ||
			    piece=="on"   || piece=="of"   || piece=="to"  || piece=="at"  ||
			    piece=="without"               || piece=="e"   || piece=="and" ||
			    piece=="ou"   || piece=="or"
			   )  return piece;
			else   piece = CapsFirstChar(piece);
			return piece;
		}
		
		/// <summary>Returns AppSettings. Never returns a null.</summary>
		static public string        GetConfig      (string key)  {
			string   val =  ConfigurationSettings.AppSettings[key];
			if (val==null)  return string.Empty;
			else            return val;
		}
		
		/// <summary>Receives an exception object and returns a string containing
		/// all the errors, in the order in which they
		/// happened, one error per line.</summary>
		static public string AllErrorMessages(Exception exception, string separator)
		{
			if (exception==null)  return string.Empty;
			if (separator==null)  separator = Environment.NewLine;
			Exception x = exception;
			string    s = x.Message;
			while(x.InnerException != null)  {
				s = x.InnerException.Message + separator + s;
				x = x.InnerException;
			}
			return s;
		}
		static public string AllErrorMessages(Exception exception)  {
			return AllErrorMessages(exception, Environment.NewLine);
		}
		
		/// <summary>Removes quotes around a string.</summary>
		/// <param name="quoted">Your quoted string.</param>
		/// <returns>The text without quotes.</returns>
		static public string UnQuote(string quoted)  {
			return UnQuote(quoted, '\"');
		}
		
		/// <summary>Removes any quote character around a string.</summary>
		/// <param name="quoted">Your "quoted" string.</param>
		/// <param name="quoteChar">The character to be removed.</param>
		/// <returns>The text without the quoting characters.</returns>
		static public string UnQuote(string quoted, char quoteChar)  {
			if (quoted==null)          return null;
			if (quoted==string.Empty)  return string.Empty;
			string s = quoted;
			if (s[0]==quoteChar)           s = s.Substring(1);
			if (s[s.Length-1]==quoteChar)  s = s.Substring(0, s.Length - 1);
			return s;
		}
		
		/// <param name="text">String to search and trim</param>
		/// <param name="tag">Beginning of the right part to be cut off.</param>
		/// <returns>If tag is found, returns text to the left of it.
		/// Otherwise, the unaltered text.</returns>
		static public string KeepLeft(string text, string tag)  {
			int indexOfTag = text.IndexOf(tag);
			if (indexOfTag != -1)  return text.Substring(0, indexOfTag);
			else                   return text;
		}
		
		/// <param name="text">String to search and trim</param>
		/// <param name="tag">Beginning of the left part to be cut off.</param>
		/// <returns>If tag is found, returns text to the right of it.
		/// Otherwise, the unaltered text.</returns>
		static public string KeepRight(string text, string tag)  {
			int indexOfTag = text.IndexOf(tag);
			if (indexOfTag != -1)  return text.Substring(indexOfTag + 1);
			else                   return text;
		}
		
		static public string GetRandomLetters(short count)  {
			if (count < 1) throw new ArgumentOutOfRangeException("count");
			Random ran = new Random();
			string s = "";
			for (int a = 0; a < count; ++a){
				int i = ran.Next(97, 122);
				s = s + Convert.ToChar(i);
			}
			return s;
		}
		
		/// <summary>Takes an encoding and a byte array and converts it to a string.
		/// This string is without the BOM (byte order mark) of the encoding.
		/// </summary>
		static public string FromBytesWithoutBOM(byte[] buffer, Encoding enc)  {
			if (buffer==null)  throw new ArgumentNullException("buffer");
			if (enc ==  null)  throw new ArgumentNullException("enc");
			// If encoding is a type of Unicode, determine length of byte order mark
			int start = 0;
			byte[] preamble = enc.GetPreamble();
			if (preamble.Length > 0)  {
				bool equals = true;
				for (int i = 0; i < preamble.Length; ++i)  {
					if (buffer[i] != preamble[i])  {
						equals = false;
						break;
					}
				}
				if (equals)  start = preamble.Length;
			}
			// Read text in the correct encoding, starting after BOM if there is one
			if (buffer.Length < start)  return string.Empty;
			return enc.GetString(buffer, start, buffer.Length - start);
		}
		
		/// <summary>Converts the given object to a string in the given format.
		/// </summary>
		/// <param name="o">Object whose properties will be read.</param>
		/// <param name="format">Format string, containing public property
		/// placeholders. For instance: "[Name]'s father is [Father]."</param>
		/// <returns>The format string, with its placeholders replaced with the
		/// corresponding values of the properties of the object.</returns>
		static public string PropsToString(object o, string format)  {
			if (format == null)  throw new System.ArgumentNullException("format");
			string      output = format;
			System.Type myType = o.GetType();
			System.Reflection.PropertyInfo[] props =
				myType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (System.Reflection.PropertyInfo prop in props)  {
				string key = "[" + prop.Name + "]";
				object obj = prop.GetValue(o, null);
				string val;
				if (obj==null)  val = string.Empty;
				else            val = obj as string;
				if (val==null)  val = obj.ToString();
				//val = AppNandestak.ConvertToHtml(val);
				output = output.Replace(key, val);
			}
			return output;
		}
		
		/// <summary>Takes an IDictionary and returns a string containing its
		/// keys and values (one item per line).</summary>
		static public string Listed(System.Collections.IDictionary dictionary,
		                            string separator)  {
			StringBuilder sb = new StringBuilder(2048);
			System.Collections.IDictionaryEnumerator e = dictionary.GetEnumerator();
				while (e.MoveNext())  {
					sb.Append(e.Key);
					sb.Append(separator);
					sb.Append(e.Value);
					sb.Append(System.Environment.NewLine);
				}
				return sb.ToString();
		}
		/// <summary>Takes an IDictionary and returns a string containing its
		/// keys and values (one item per line).</summary>
		static public string Listed(System.Collections.IDictionary dictionary)  {
			return Listed(dictionary, ": ");
		}
		
		static public string Repeat(string text, uint times)  {
			StringBuilder sb = new StringBuilder();
			for (uint u=0; u<times; u++)  {
				sb.Append(text);
			}
			return sb.ToString();
		}
		
	}
	
	
	/// <summary>Why write dates in the ISO-8601 format (YYYY-MM-DD)?
	/// http://www.quarl.org/notes/yyyymmdd.html
	/// </summary>
	public class IsoDate  {
		static public string   ToDateTimeString(DateTime d)  {
			return d.ToString("yyyy-MM-dd HH:mm:ss");
		}
		
		static public string   ToDateString    (DateTime d)  {
			return d.ToString("yyyy-MM-dd");
		}
		
		static public DateTime Parse           (string   s)  {
			if (s==null)  throw new ArgumentNullException(s);
			string r = @"^(?<Year>\d{4})-(?<Month>\d{1,2})-(?<Day>\d{1,2})" +
				@"(\s(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Second>\d{1,2}))?$";
			Regex reg = new Regex(r);
			Match m   = reg.Match(s.Trim());
			if (!m.Success)  throw new System.ArgumentException
				("Invalid NiceDateTime: " + s.Substring(0,19));
			int year   = int.Parse(m.Groups["Year"] .Value);
			int month  = int.Parse(m.Groups["Month"].Value);
			int day    = int.Parse(m.Groups["Day"]  .Value);
			int hour   = 0;
			int minute = 0;
			int second = 0;
			if (m.Groups["Hour"].Success)  {
				hour   = int.Parse(m.Groups["Hour"]  .Value);
				minute = int.Parse(m.Groups["Minute"].Value);
				second = int.Parse(m.Groups["Second"].Value);
			}
			return new DateTime(year,month,day,hour,minute,second);
		}
		
	}
}
