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

using ApplicationException        = System.ApplicationException;
using Exception                   = System.Exception;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NandoF.Data;

namespace NandoF.Mail.Parser2
{
	internal class Parser
	{
		private const string CRLF = "\r\n"; // never use Environment.NewLine for email
		protected Message    m;
		protected TextReader rd;
		protected int        currentLine = 0;
		protected ArrayList  warns       = new ArrayList(8);
		//        Accumulates all message headers
		protected StringBuilder rawHeaders = new StringBuilder(4000);
		
		// Constructor and workflow manager
		public Parser(Message m, TextReader rd, bool parseBody)  {
			this.m  = m;
			this.rd = rd;
			// Parse headers
			parseHeaders();
			// Maybe even parse content
			if (parseBody)  {
				parseMsgContents();
				// TODO: If the message had no "content length" header, we can calculate it
//				if (Headers.ContentLength==0)
//					Headers.ContentLength = RawMessage.Length; // message.Length; // _rawMessageBody.Length;
			}
			// Finally, populate the message warnings
			m.warnings = (NameVal[])warns.ToArray(typeof(NameVal));
		}
		
		protected void   parseHeaders()  {
			// Read message headers until the first empty line or the boundary
			ArrayList headers = readHeaders();
			// Now rd is at the point where message content starts.
			// Populate MessageHeaders properties, parsing each header
			foreach (NameVal header in headers) m.headers.parseHeader(header, this);
			// Store all headers in a string field
			m.headers.rawHeader = rawHeaders.ToString();
			// Fill empty headers so the user doesn't get null objects
			m.headers.fillEmptyHeaders();
		}
		
		/// <summary>Reads message headers until the first empty line.</summary>
		/// <returns>An ArrayList of NameVal objects. In each of these, Name
		/// holds the header name, and Val, the value.</returns>
		/// <remarks><para>Some headers have more than one line.
		/// These can be distinguished because "continuation" lines always start
		/// with a space or a tab character.
		/// This method joins such lines.</para>
		/// <para>No decoding is done in this step.</para>
		/// <para>If there are no headers, an ArrayList of length zero
		/// will be returned. This happens when this method is called after a
		/// boundary that is final and not the beginning of a new part.</para>
		/// </remarks>
		public    ArrayList readHeaders()  {
//			string boundary2 = boundary + "--";
			ArrayList    a = new ArrayList(20);
			string  header = readLine();
			while (true)  {
				// I tried finding the boundary here too, but it resulted in
				// message parts not being seen. To solve this, we would need to
				// inform the caller that the header has already been read.
				if (Text.IsNullOrEmpty(header)
//				    || header == boundary || header == boundary2
				   )           break;
				string line = readLine();
				while (line.StartsWith(" ") || line.StartsWith("\t"))  {
					// Accumulate "continuation" lines in header.
					header += CRLF + line;
					line = readLine();
				}
				NameVal nv;
				// Divide the header at the first colon
				try   {
					nv = NameVal.Parse(header, ":");
					nv.Name = nv.Name.Trim().ToLower();
					nv.Val  = nv.Val .Trim();
					a.Add(nv);
				}
				catch {
					if (warns != null)  this.AddWarning("Header without colon", header);
				}
				header = line;       // Loop to next line
			}
			return a;
		}
		
		protected string    readLine   ()  {
			// This method helps readHeaders().
			string line = rd.ReadLine();
			currentLine++;
			if (rawHeaders != null)  rawHeaders.Append(line + CRLF);
			return line;
		}
		
		public    void      AddWarning(string name, string val)  {
			this.warns.Add(new NameVal("Line " + currentLine.ToString() +
			                           ": " + name, val));
		}
		
		protected void      parseMsgContents()  {
			// This method is called from the constructor.
			// At this point, all message headers have been parsed.
			// Create a MimePart according to them and put it in MimeTree
			m.mimeTree = ProducePart(m.Headers.ContentHeaders, null);
		}
		
		/// <summary>Parses content-related headers which define a MIME part and
		/// returns an object that represents that MIME part.</summary>
		public   IMimePart  ProducePart(ArrayList partHeaders, string boundary)  {
			// Join lines in headers
			foreach (NameVal nv in partHeaders)  {
				nv.Val = MessageHeaders.DecodeLines(nv.Val);
			}
			// From headers get Content-Type and other properties in the same line
			string contentType  = null;
			string contentProps = null;
			foreach (NameVal nv in partHeaders)  {
				if (nv.Name.ToUpper()=="CONTENT-TYPE")  {
					// For example, the header "Content-Type: multipart/mixed;(...)"
					// val already contains "multipart/mixed;", so just keep what is
					// to the left of the semicolon ;
					contentType  = Text.KeepLeft (nv.Val, ";").ToLower();
					contentProps = Text.KeepRight(nv.Val, ";");
					break;
				}
			}
			if (contentType==null)  {
				string catHeaders = "";
				foreach (NameVal nv in partHeaders)
					catHeaders += nv.ToString() + System.Environment.NewLine;
				this.AddWarning("Mime headers do not specify a Content-Type.",
				                catHeaders);
				return null;
			}
			try  {
				// Knowing the Content-Type, we produce the correct MimePart
				if (contentType.StartsWith("multipart"))  {
					return new MimePartMulti(contentType, contentProps,
					                         partHeaders, this);
				}
				else if (contentType=="text/plain" ||
				         contentType=="message/delivery-status")  {
					return new MimePartTextPlain(contentType, contentProps, partHeaders,
					                             this, boundary);
				}
				else if (contentType=="text/html")  {
					return new MimePartTextHtml (contentType, contentProps, partHeaders,
					                             this, boundary);
				}
				else if (contentType=="message/rfc822")  {
					return new Message(this.rd, true, false);
				}
				else  return new MimePartBinary(contentType, contentProps, partHeaders,
				                                this, boundary);
//				throw new ApplicationException("Unknown Content-Type: " + contentType);
			}
			catch (Exception x)  {
				this.AddWarning(x.Message, x.ToString());
				return null;
			}
		}
		
		public   string    ReadContentUntil(string boundary)  {
			// We do not check if boundary is null because
			// it CAN be null, meaning this part is NOT inside a multipart.
			// All boundary lines have a "--" prefix added to the
			// boundary declared in the multipart headers. (RFC 2046, line 1022)
			// When there is also a "--" suffix, it is the end of current multipart.
//			string boundaryFinal;
//			if (Text.IsNullOrEmpty(boundary))  boundaryFinal = null;
//			else                               boundaryFinal = boundary + "--";
			StringBuilder sb = new StringBuilder(4000);
			while (true)  {
				string line = rd.ReadLine();
				currentLine++;
				if (line == null)  {  // Stop if end of file
					boundaryIsFinal = true;
					break;
				}
				if (boundary != null && line.StartsWith(boundary))  {
					if (line == boundary + "--")  boundaryIsFinal = true;
					else                          boundaryIsFinal = false;
					break;
				}
				else  sb.Append(line + CRLF);
			}
			return sb.ToString();
		}
		
		/// <summary>Whether the boundary (that was read last) ended with "--"
		/// </summary>
		public    bool BoundaryIsFinal  {
			get  { return boundaryIsFinal; }
		}
		protected bool boundaryIsFinal;
		
		// TODO: Use this where?
		static private string getReportTypeFrom(string contentType)  {
			// Find "report-type=X"
			Match m = rxReportType.Match(contentType);
			if (m.Success)  return m.Groups[1].Value;
			else throw new ApplicationException("No report-type found.");
		}
		static private Regex rxReportType =
			new Regex(@"report-type=(?<rtype>.+?);", RegexOptions.IgnoreCase);
		
	}
}
