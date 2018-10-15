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

using ArgumentNullException       = System.ArgumentNullException;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NandoF.Mail.Parser2
{
	public abstract class MimePartText : MimePartLeaf
	{
		public        string ContentCharset  {
			get  {
				if (contentCharset != null)  return contentCharset;
				// If we don't know it yet, find charset.
				// Like:  Content-Type: text/plain; charset="iso-8859-1"
				Match m = rxCharSet1.Match(this.contentProps);
				if (!m.Success)  m = rxCharSet2.Match(contentType);
				if (m.Success)   contentCharset = m.Groups[1].Value.ToLower();
				else             contentCharset = string.Empty;
				return contentCharset;
			}
		}
		private       string contentCharset;
		static private Regex rxCharSet1 = new
			Regex(@"charset=\""?(?<CharSet>[^\""]*)\""?$", RegexOptions.IgnoreCase);
			//Regex(@"charset=""(?<CharSet>[\w\d-]+?)""", RegexOptions.IgnoreCase);
		static private Regex rxCharSet2 = new
			Regex(@"charset=(?<CharSet>[\w\d-]+)", RegexOptions.IgnoreCase);
		
		virtual public string ContentDecoded  {
			get {
				if (contentDecoded != null)  return contentDecoded;
				contentDecoded = ContentRaw;
				if (ContentTransferEncoding=="quoted-printable")
					contentDecoded = DecodeQP.ConvertHexContent(contentDecoded);
				if (!Text.IsNullOrEmpty(ContentCharset))  {
					byte[] bytes = Encoding.Default.GetBytes(contentDecoded);
					Encoding enc = Encoding.GetEncoding(ContentCharset);
					contentDecoded = enc.GetString(bytes);
				}
				return contentDecoded;
			}
		}
		protected      string contentDecoded;
		
		public void ToFile(string path, Encoding enc)  {
			if (Text.IsNullOrEmpty(path))  throw new ArgumentNullException("path");
			using (StreamWriter sw = new StreamWriter(path, false, enc)) {
				sw.Write(ContentDecoded);
			}
		}
		
	}
	
	
	
	public class MimePartTextPlain : MimePartText
	{
		// Constructor
		internal MimePartTextPlain(string    contentType, string contentProps,
		                           ArrayList partHeaders, Parser p,
		                           string    boundary)  {
			// We do not check if boundary is null because
			// it CAN be null, meaning this text/html part is NOT inside a multipart.
			this.contentType  = contentType;
			this.contentProps = contentProps;
			this.headers      = partHeaders;
			// We already have the headers, so read content until the boundary.
			this.contentRaw   = p.ReadContentUntil(boundary);
		}
		
		public void ToFile()  {
			ToFile(DefaultFileName, Encoding.Default);
		}
		static public string DefaultFileName = "MimePartText.txt";
	}
	
	
	
	public class MimePartTextHtml : MimePartText
	{
		// Constructor
		internal MimePartTextHtml (string    contentType, string contentProps,
		                           ArrayList partHeaders, Parser p,
		                           string    boundary)  {
			// We do not check if boundary is null because
			// it CAN be null, meaning this text/html part is NOT inside a multipart.
			this.contentType  = contentType;
			this.contentProps = contentProps;
			this.headers      = partHeaders;
			// We already have the headers, so read content until the boundary.
			this.contentRaw   = p.ReadContentUntil(boundary);
		}
		
		override public string ContentDecoded  {
			get  {
				// Return cached string if it already exists
				if (contentDecoded != null)  return contentDecoded;
				// Worst scenario is decoded = raw
				contentDecoded   = ContentRaw;
				// Decode quoted-printable first
				if (ContentTransferEncoding=="quoted-printable")
					contentDecoded = DecodeQP.ConvertHexContent(contentDecoded);
				// Decode from MIME charset, into a byte array
				Encoding mimeEnc;
				if (!Text.IsNullOrEmpty(ContentCharset))
					mimeEnc = Encoding.GetEncoding(ContentCharset);
				else mimeEnc = Encoding.Default;
				byte[] bytes = Encoding.Default.GetBytes(contentDecoded);
				// Try to find the encoding specified in the <head> of the HTML file
				// and decode the byte array
				//Encoding htmlEnc = NandoF.Web.HtmlHelper.FindEncodingIn(contentDecoded);
				//if (htmlEnc==null)  contentDecoded = Encoding.Default.GetString(bytes);
				//else                contentDecoded = htmlEnc         .GetString(bytes);
				contentDecoded = new string(mimeEnc.GetChars(bytes));
				return contentDecoded;
			}
		}
		
		public          void ToFile()  {
			ToFile(DefaultFileName, Encoding.Default);
		}
		static public string DefaultFileName = "MimePartHtml.htm";
	}
}
