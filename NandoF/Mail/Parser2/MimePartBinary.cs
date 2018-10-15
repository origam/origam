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

using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NandoF.Data;

namespace NandoF.Mail.Parser2
{
	public class MimePartBinary : MimePartLeaf
	{
		public    string ContentDisposition  {
			get  {
				if (contentDisposition != null)  return contentDisposition;
				foreach (NameVal header in this.headers)  {
					if (header.Name=="content-disposition")
						contentDisposition = header.Val;
				}
				if (contentDisposition==null)  contentDisposition = "";
				return contentDisposition;
			}
		}
		protected string contentDisposition;
		
		public    string FileName  {
			get  {
				if (fileName != null)  return fileName;
				// If we don't know it yet, find file name.
				// Like:  Content-Type: image/jpeg; name="MyImage.jpg"
				Match m = rxFileName1.Match(this.contentProps);
				// File name could also be in the Content-Disposition header.
				// Like:  Content-Disposition: attachment; filename="MyImage.jpg"
				if (!m.Success)  m = rxFileName1.Match(ContentDisposition);
				if (!m.Success)  m = rxFileName2.Match(contentProps);
				if (!m.Success)  m = rxFileName2.Match(ContentDisposition);
				if (m.Success)   fileName = m.Groups[1].Value.ToLower();
				else             fileName = string.Empty;
				fileName = fileName.Replace("%20", " ");
				return fileName;
			}
			set  {
				fileName = value;
			}
		}
		protected string fileName;
		static private Regex rxFileName1 = new
			Regex(@"name=""(?<FileName>.+?)""", RegexOptions.IgnoreCase);
		static private Regex rxFileName2 = new
			Regex(@"name=(?<FileName>.+)", RegexOptions.IgnoreCase);
		
		/*
		static private byte[] decodeB64(string text)  {
			if (text == null)  throw new ArgumentNullException("text");
			// A message had a base64-encoded attachment that was only one blank line
			// and System.Convert.FromBase64String(text) throwed a
			// "System.FormatException: Invalid length for a Base-64 string". So...
			//if (text=="" || text=="\r\n")  return Encoding.Default.GetBytes("\0");
			if (text=="" || text=="\r\n")  return Encoding.Default.GetBytes("");
			return System.Convert.FromBase64String(text);
		}
		*/
		static private byte[] decodeB64(string text)  {
			if (Text.IsNullOrEmpty(text) || text=="\r\n")
				return Encoding.Default.GetBytes(""); //Encoding.Default.GetBytes("\0")
			try  { return System.Convert.FromBase64String(text); }
			catch (System.FormatException)  {
				// "System.FormatException: Invalid character in a Base-64 string."
				// To remedy this, we remove any characters not allowed in base-64:
				StringBuilder sb = new StringBuilder(text.Length);
				StringReader  sr = new StringReader(text);
				while (sr.Peek() != -1)  {
					char c = (char)sr.Read();
					if (rxBase64.IsMatch(c.ToString()))  sb.Append(c);
				}
				string sbs = sb.ToString();
//				System.Windows.Forms.MessageBox.Show
//					("Reduction from " + text.Length + " to " + sbs.Length);
				return System.Convert.FromBase64String(sbs);
			}
		}
		static Regex rxBase64 = new Regex(@"^[A-Za-z\d\+/=]$");
		
		public    byte[] ContentDecoded  {
			get  {
				if (contentDecoded != null)  return contentDecoded;
				if (ContentTransferEncoding=="base64")  {
					// contentDecoded = decodeB64(ContentRaw);
					// The above implementation sometimes generates a
					// "System.FormatException: Invalid character in a Base-64 string."
					// To remedy this, we remove null characters:
					contentDecoded = decodeB64(ContentRaw.Replace("\0", ""));
				}
				else contentDecoded = Encoding.Default.GetBytes(ContentRaw);
				return contentDecoded;
			}
		}
		protected byte[] contentDecoded;
		
		public void ToFile(string path)  {
			using (FileStream fs = File.OpenWrite(path))  {
				fs.Write(ContentDecoded, 0, ContentDecoded.Length);
			}
		}
		
		// Constructor
		internal MimePartBinary(string    contentType, string contentProps,
		                        ArrayList partHeaders, Parser p,
		                        string    boundary)  {
			// We do not check if boundary is null because
			// it CAN be null, meaning this part is NOT inside a multipart.
			this.contentType  = contentType;
			this.contentProps = contentProps;
			this.headers      = partHeaders;
			//this.p = p;
			// We already have the headers, so read content until the boundary.
			this.contentRaw   = p.ReadContentUntil(boundary);
		}
		
		//internal Parser p;
	}
}
