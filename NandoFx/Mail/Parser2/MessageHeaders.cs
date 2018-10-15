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

This file is based on OpenPOP.Net (2004/07) -- http://sf.net/projects/hpop/
                      Copyright 2003-2004 Hamid Qureshi and Unruled Boy
 */
#endregion

#region using
using ApplicationException        = System.ApplicationException;
using ArgumentNullException       = System.ArgumentNullException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using Convert                     = System.Convert;
using DateTime                    = System.DateTime;
using TimeSpan                    = System.TimeSpan;
using StringCollection            = System.Collections.Specialized.StringCollection;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NandoF.Data;
#endregion

namespace NandoF.Mail.Parser2
{
	/// <summary>Represents the headers of an e-mail message.</summary>
	public class MessageHeaders
	{
		public   string RawHeader  {
			get { return rawHeader; }
		}
		internal string rawHeader;
		
		internal void   parseHeader    (NameVal nv, Parser p)  {
			// Decode each header, store the value in a property
			switch (nv.Name.ToUpper())  {
				case "TO":
					// Store all recipients in the "_to" string array, decoding them
					_to  = decodeAddresses(nv.Val);
					break;
					
				case "BCC":
					_bcc = decodeAddresses(nv.Val);
					break;
					
				case "CC":
					_cc  = decodeAddresses(nv.Val);
					break;
					
				case "CONTENT-LENGTH":
					try   { _contentLength = Convert.ToInt32(nv.Val); }
					catch { p.AddWarning("Cannot parse content-length header", nv.Val); }
					break;
					
				case "DATE":
					_dateTimeInfo = nv.Val;
					string date = TrimEmailDate(_dateTimeInfo);
					try   {
						// _date = DateTime.Parse(date);
						_date = ParseEmailDate(date);
					}
					catch { p.AddWarning("Cannot parse date header", nv.Val); }
					break;
					
				case "DISPOSITION-NOTIFICATION-TO":
					_dispositionNotificationTo = nv.Val;
					break;
					
				case "FROM":
					string f = DecodeLines(nv.Val);
					try   { _from = NamedEmail.Parse(f); }
					catch { p.AddWarning("Cannot parse 'From:' header", f); }
					break;
					
				case "IMPORTANCE":
					_importance = nv.Val;
					break;
					
				case "RECEIVED":
					this._received.Add(DecodeLines(nv.Val));
					break;
					
				case "REPLY-TO":
					this._replyTo = NamedEmail.Parse(DecodeLines(nv.Val));
					break;
					
				case "RETURN-PATH":  // TODO: Use NamedEmail here?
					_returnPath = nv.Val.Trim('>').Trim('<');
					break;
					
				case "SUBJECT":
				case "THREAD-TOPIC":
					this._subject = DecodeLines(nv.Val);
					break;
					
				case "MIME-VERSION":
					_mimeVersion = nv.Val;
					break;
					
				case "MESSAGE-ID":  // TODO: Use NamedEmail here?
					_messageID = nv.Val.Trim('>').Trim('<');
					break;
					
				case "CONTENT-TYPE":
				case "CONTENT-TRANSFER-ENCODING":
					_contentHeaders.Add(nv);
					break;
					
				default: // Here we parse all custom headers
					// OpenPOP says: - Every custom header starts with "X"
					// Nando: - OK, but should we really drop all non-compliant headers?
//					if (headerName.ToUpper().StartsWith("X"))  {
					nv.Val = DecodeLines(nv.Val);
					_customHeaders.Add(nv);
//						}
					break;
			}
		}
		
		internal void   fillEmptyHeaders()  {
			// After parsing the headers, fill in the empty ones
			// so the user doesn't get null objects.
			if (this._from==null)   _from = new NamedEmail(string.Empty);
			if (_replyTo==null)  _replyTo = new NamedEmail(string.Empty);
		}
		
		#region static utility methods
		static private string[] decodeAddresses(string emails)  {
			string[] array = emails.Split(',');
			for (int i=0; i < array.Length; i++)
				array[i] = DecodeLines(array[i].Trim());
			return array;
		}
		
		// TODO: The example when decoded still keeps the underlines, is this normal?
		/// <summary>Decodes encoded text in an e-mail header.</summary>
		/// <param name="header">One header of an e-mail message,
		/// possibly occupying more than one line.</param>
		/// <remarks>Consider this example:
		/// <c>Subject: =?iso-8859-1?Q?Mat=E9ria_no_Estadao?=
		/// =?iso-8859-1?Q?_sobre_as_Lagoas_de_Bonsucesso?=</c>
		/// Here we have a subject header with 2 lines. Of course, they mean
		/// a single subject line. (There exist no multiline subjects.)
		/// The lines are decoded, then joined.</remarks>
		static public string DecodeLines(string header)  {
			try  {
				string   ret   = "";
				string[] lines = Regex.Split(header, CRLF);
				Regex r = new
					Regex(@"\=\?"              + // =?                (encoding start)
					      @"(?<Charset>\S+)\?" + // iso-8859-1
					      @"(?<Encoding>\w)\?" + // ?Q?
					      @"(?<Content>\S+)"   + // Nando
					      @"\?\=");              // ?=               (end of encoding)
				foreach (string oneLine in lines)  {
					string line = oneLine; // (makes line writable)
					// If line starts with a tab, substitute it for a space
					if (line.StartsWith("\t"))  line = " " + line.Substring(1);
					// Decode any encoded parts of line
					Match m = r.Match(line);
					while (m.Success)  {
						string body     = m.Groups["Content"].Value;
						switch (m.Groups["Encoding"].Value.ToUpper())  {
							case "B":
								body = DecodeB64s(body, m.Groups["Charset"].Value);
								break;
							case "Q":
								// TODO: Charset is being ignored, is this OK?
								body = DecodeQP.ConvertHexContent(body);//, m.Groups["Charset"].Value);
								break;
							default:
								break;
						}
						string original = m.Value;
						line = line.Replace(original, body);
						m = r.Match(line);
					}
					ret += line;
//					else  {
//						if (!isValidMime(lines[i]))  ret += lines[i].Replace('\t', ' ');
//					}
				}
				return ret;
			}
			catch  { return header; }
		}
		
		/// <summary>DO NOT use Environment.NewLine in a MIME parser.
		/// E-mail messages always use CRLF as the line separator.
		/// See RFC 2046, section 4.1.1.</summary>
		private const string CRLF = "\r\n";
		
		static public string DecodeB64s(string text, string encodingName)  {
			try  {
				if (encodingName.ToLower()=="ISO-8859-1".ToLower())
					return DecodeB64s(text);
				else
					return Encoding.GetEncoding(encodingName).GetString(decodeB64(text));
			}
			catch  { return DecodeB64s(text); }
		}
		
		static public string DecodeB64s(string text)  {
			return Encoding.Default.GetString(decodeB64(text));
		}
		
		static private byte[] decodeB64(string text)  {
			if (text == null)  throw new ArgumentNullException("text");
			if (text == "")    return Encoding.Default.GetBytes("\0");
			return Convert.FromBase64String(text);
			//text=Encoding.Default.GetString(by);
		}
		
		/// <summary>Determines whether the text is valid MIME text.</summary>
		/// <param name="text">Text to be verified</param>
		/// <returns>True if MIME text, false if not</returns>
//		static private bool  isValidMime   (string text)  {
//			int pos = text.IndexOf("=?");
//			return (pos != -1 && text.IndexOf("?=", pos+6) != -1 && text.Length > 7);
//		}
		
		/// <summary>Strips some info from an e-mail date-and-time string such as...
		/// <code>Thu, 20 Oct 2005 13:22:31 -0200</code>
		/// ... turning it into...
		/// <code>20 Oct 2005 13:22:31</code>
		/// ...which can be sent to DateTime.Parse().</summary>
		static public string TrimEmailDate(string date)  {
			// string  strRet = strDate.Trim();
			// int indexOfTag = strRet.IndexOf(",");
			// if (indexOfTag != -1)  strRet = strRet.Substring(indexOfTag + 1);
//			string strRet = Text.KeepRight(date, ",");
//			strRet = Text.KeepLeft(strRet, "+");
//			strRet = Text.KeepLeft(strRet, "-");
//			strRet = Text.KeepLeft(strRet, "GMT");
//			strRet = Text.KeepLeft(strRet, "CST");
//			return strRet.Trim();
			// NEW IMPLEMENTATION
//			Regex r = new Regex(@"(?<WeekDay>\w{0,3})"              + // Thu
//			                    @",?\s+"                            + // ,<space>
			Regex r = new Regex(
			                    @"(?<Date>\d{1,2}\s\w{3}\s\d{2,4})" + // 20 Oct 2005
			                    @"\s"                               + // <space>
			                    @"(?<Time>\d{1,2}:\d{1,2}:\d{1,2})"   // 13:22:31
			                   );
			Match m = r.Match(date);
			if (m.Success)  return m.Groups[0].Value;
			else            return date;
		}
		
		static public DateTime ParseEmailDate(string date)  {
			if (Text.IsNullOrEmpty(date))  throw new ArgumentNullException("date");
			// In .NET 1.1 you would just...
			// return DateTime.Parse(date);
			// However, that does not work in Mono 1.1.13, so...
			Regex r = new Regex(@"(?<Day>\d{1,2})\s"  + // 20
			                    @"(?<Month>\w{3})\s"  + // Oct
			                    @"(?<Year>\d{2,4})\s" + // 2005
			                    @"(?<Time>\d{1,2}:\d{1,2}:\d{1,2})"   // 13:22:31
			                   );
			Match m = r.Match(date);
			if (!m.Success) throw new ApplicationException
				("Unable to parse date: " + date);
			string lower = m.Groups["Month"].Value.ToLower();
			int    month;
			switch (lower)  {
					case "jan": month = 1;  break;
					case "feb": month = 2;  break;
					case "mar": month = 3;  break;
					case "apr": month = 4;  break;
					case "may": month = 5;  break;
					case "jun": month = 6;  break;
					case "jul": month = 7;  break;
					case "aug": month = 8;  break;
					case "sep": month = 9;  break;
					case "oct": month = 10; break;
					case "nov": month = 11; break;
					case "dec": month = 12; break;
				default:
					throw new ApplicationException("Unable to parse month: " + lower);
			}
			DateTime d = new DateTime(Convert.ToInt32(m.Groups["Year"].Value),
			                          Convert.ToInt32(month),
			                          Convert.ToInt32(m.Groups["Day"].Value)
			                         );
			TimeSpan time = TimeSpan.Parse(m.Groups["Time"].Value);
			d = d.Add(time);
			return d;
		}
		
		#endregion
		
		#region Properties
		public  string[]  BCC {
			get  { return _bcc; }
		}
		private string[] _bcc  = new string[0];
		
		public  string[]  CC  {
			get  { return _cc; }
		}
		private string[] _cc   = new string[0];
		
		public  long      ContentLength  {
			get  { return _contentLength; }
		}
		private long     _contentLength  = 0;
		
		/// <summary>ArrayList that holds all headers about the main MIME content
		/// container: Content-Type, Charset, Content-Transfer-Encoding...
		/// </summary>
		public  ArrayList  ContentHeaders  {
			get  { return _contentHeaders; }
		}
		private ArrayList _contentHeaders = new ArrayList(5);
		
		/// <summary>This ArrayList holds all "the other headers"
		/// as NameVal objects.</summary>
		public  ArrayList  CustomHeaders  {
			get  { return _customHeaders; }
			set  { _customHeaders=value;  }
		}
		private ArrayList _customHeaders = new ArrayList(20);
		
		public  DateTime  Date  {
			get  { return _date; }
		}
		private DateTime _date;
		
		public  string    DateTimeInfo  {
			get  { return _dateTimeInfo; }
		}
		private string   _dateTimeInfo              = string.Empty;
		
		public  string    DispositionNotificationTo  {
			get  { return _dispositionNotificationTo; }
		}
		private string   _dispositionNotificationTo = string.Empty;
		
		public  NamedEmail  From  {
			get  { return _from; }
		}
		private NamedEmail _from;
		
		/// <summary>Importance level</summary>
		public  string    Importance  {
			get  { return _importance; }
		}
		private string   _importance  = string.Empty;
		
		/// <summary>Importance level type</summary>
		public MessageImportanceType ImportanceType  {
			get  {
				// HACK: Nando added null check. Is this really needed before switch?
				if (_importance==null)  return MessageImportanceType.NORMAL;
				switch (_importance.ToUpper())  {
					case "5":
						case "HIGH":   return MessageImportanceType.HIGH;
					case "3":
						case "NORMAL": return MessageImportanceType.NORMAL;
					case "1":
						case "LOW":    return MessageImportanceType.LOW;
						default:       return MessageImportanceType.NORMAL;
				}
			}
		}
		
		// TODO: Keywords not implemented, are they important?
//		/// <summary>Message keywords</summary>
//		public  ArrayList  Keywords  {
//			get  { return _keywords; }
//		}
//		private ArrayList _keywords    = new ArrayList();
		
		public  string    MessageID  {
			get  { return _messageID; }
		}
		private string   _messageID    = string.Empty;
		
		public  string    MimeVersion  {
			get  { return _mimeVersion; }
		}
		private string   _mimeVersion  = string.Empty;
		
		public  StringCollection  Received  {
			get  { return _received; }
		}
		private StringCollection _received = new StringCollection();
		
		public  NamedEmail  ReplyTo  {
			get  { return _replyTo; }
		}
		private NamedEmail _replyTo;
		
		public  string    ReportType  {
			get  { return _reportType; }
		}
		private string   _reportType   = string.Empty;
		
		public  string    ReturnPath  {
			get  { return _returnPath; }
		}
		private string   _returnPath   = string.Empty;
		
		public  string    Subject  {
			get  { return _subject; }
		}
		private string   _subject      = string.Empty;
		
		public  string[]  To  {
			get  { return _to; }
		}
		private string[] _to           = new string[0];
		#endregion
		
		override public string ToString()  {
			StringBuilder sb = new StringBuilder(2000);
			string NL = System.Environment.NewLine;
			sb.Append("Subject:                   " + Subject          + NL);
			sb.Append("From:                      " + From              + NL);
			sb.Append("To:                        " + Text.FromArray(To) + NL);
			sb.Append("ReplyTo:                   " + ReplyTo             + NL);
			sb.Append("BCC:                       " + Text.FromArray(BCC)  + NL);
			sb.Append("CC:                        " + Text.FromArray(CC)    + NL);
			sb.Append("Date:                      " + Date                   + NL);
			sb.Append("DateTimeInfo:              " + DateTimeInfo            + NL);
			sb.Append("ContentLength:             " + ContentLength.ToString() + NL);
			sb.Append("DispositionNotificationTo: " + DispositionNotificationTo + NL);
			sb.Append("Importance:                " + Importance                + NL);
			sb.Append("ImportanceType:            " + ImportanceType.ToString() + NL);
			sb.Append("MessageID:                 " + MessageID                + NL);
			sb.Append("MimeVersion:               " + MimeVersion             + NL);
			sb.Append("ReportType:                " + ReportType              + NL);
			sb.Append("ReturnPath:                " + ReturnPath              + NL);
			sb.Append("Received: ********************************************" + NL);
			foreach(string s in this.Received)              sb.Append("  " + s + NL);
			sb.Append("Content headers: *************************************" + NL);
			foreach(object o in this._contentHeaders)
				sb.Append("  " + o.ToString() + NL);
			sb.Append("CustomHeaders: ***************************************" + NL);
			foreach(NameVal nv in CustomHeaders)  {
				sb.Append("  " + nv.ToString(": ") + NL);
			}
			return sb.ToString();
		}
		
		override public int GetHashCode()  {
//			int ihash = this.msg.Headers.ContentLength.GetHashCode() ^
//				DateTime.Now.Millisecond ^ subject.GetHashCode();
//			string    msgId = msg.MessageID;
//			if (msgId   != null)  ihash = ihash ^ msgId.GetHashCode();
//			string  msgDate = msg.DateTimeInfo;
//			if (msgDate != null)  ihash = ihash ^ msgDate.GetHashCode();
//			string     hash = ihash.ToString();
			return rawHeader.GetHashCode();
		}
		
		/// <summary>Generates a unique file name for this message, using
		/// the first 20 characters of the subject, the sender e-mail address,
		/// and a hash code combined with the current millisecond.
		/// The extension is .eml.</summary>
		public string SuggestFileName()  {
			int   ihash = ContentLength.GetHashCode() ^ Subject.GetHashCode() ^
				this.MessageID.GetHashCode() ^ DateTimeInfo.GetHashCode() ^
				DateTime.Now.Millisecond;
			string hash = ihash.ToString();
			int len = 20;  // Get at most 20 chars from subject
			if (Subject.Length < len)  len = Subject.Length;
			return Filez.ToValidName(Subject.Substring(0,len) + "-" +
			                         From.Email + "-" + hash + ".eml");
		}
	}
	
	
	/// <summary>3 message importance types defined by RFC</summary>
	public enum MessageImportanceType  { HIGH=5, NORMAL=3, LOW=1 }
	
}
