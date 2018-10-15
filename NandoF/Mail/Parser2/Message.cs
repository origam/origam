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
using ArgumentNullException       = System.ArgumentNullException;
using Exception                   = System.Exception;
using System.Collections;
using System.IO;
using System.Text;
using NandoF.Data;

namespace NandoF.Mail.Parser2
{
	/// <summary>Represents an e-mail message.</summary>
	public class Message : IMimePart
	{
		/// <summary>Details any problems that may have occurred when parsing
		/// the message.</summary>
		public   NameVal[]      Warnings {
			get { return warnings;  }
		}
		internal NameVal[]      warnings;
		
		public   MessageHeaders Headers {
			get  { return headers;  }
		}
		internal MessageHeaders headers = new MessageHeaders();
		
		/// <summary>Placeholder for all message parts and content(s).
		/// Can contain a tree of MIME objects -- even another Message.</summary>
		public   IMimePart      MimeTree {
			get  { return mimeTree; }
		}
		internal IMimePart      mimeTree;
		
		#region Constructors
		/// <param name="message">The message stream to be parsed.
		/// It is closed when finished.</param>
		/// <param name="parseBody">If false, only the message headers will be
		/// parsed. The default is true.</param>
		public Message(TextReader source, bool parseBody, bool closeSource)  {
			if (source==null)  throw new ArgumentNullException("source");
			// Just get a Parser started, it will take care of everything!
			try  { Parser p = new Parser(this, source, parseBody); }
			catch (Exception x)  {
				throw new ApplicationException("Parse error", x);
			}
			finally       { if (closeSource)  source.Close(); }
			source = null;  // TODO: Is "source = null;" useful?
		}
		public Message(TextReader source, bool parseBody)
			: this(source, parseBody, true)  {}
		public Message(TextReader source) : this(source, true, true)  {}
		
		public Message(string source, bool parseBody)
			: this(new StringReader(source), parseBody, true) {}
		
		public Message(bool parseBody, string fileName)
			: this(new StreamReader(fileName), parseBody, true)  {}
		
		#endregion
		
		/// <returns>An array of MimeParts containing every leaf of the MIME tree.
		/// In other words, all parts that have content (text, HTML, binaries) and
		/// NOT parts that contain other parts.</returns>
		/// <remarks>If you want the *hierarchy* of MIME parts, use the
		/// MimeTree property instead.</remarks>
		public    MimePart[] Leaves  {
			get  {
				if (leaves != null)  return leaves;
				ArrayList a = new ArrayList(8);
				getLeaves(MimeTree, a, true);
				leaves = (MimePart[]) a.ToArray(typeof(MimePart));
				return leaves;
			}
		}
		protected MimePart[] leaves;
		
		private void getLeaves(IMimePart ip, ArrayList a, bool enterMessages)  {
			if (ip is MimePart)  {
				MimePartMulti m = ip as MimePartMulti;
				if (m != null)  {
					// Recursively get subparts
					foreach(IMimePart part in m.Children)
						getLeaves(part, a, enterMessages);
					return;
				}
				MimePartLeaf leaf = ip as MimePartLeaf;
				if (leaf != null)  {
					a.Add(leaf);
					return;
				}
			}
			else if (enterMessages && ip is Message)  {
				Message msg = ip as Message;
				getLeaves(msg.MimeTree, a, enterMessages);
			}
		}
		
		public    MimePartTextPlain BodyText  {
			get  {
				if (bodyText != null)  return bodyText;
				ArrayList a = new ArrayList(8);
				getLeaves(this.MimeTree, a, false);
				foreach (MimePart part in a)  {
					if (part is MimePartTextPlain)  {
						bodyText = part as MimePartTextPlain;
						break;
					}
				}
				return bodyText;
			}
		}
		protected MimePartTextPlain bodyText;
		
		public    MimePartTextHtml  BodyHtml  {
			get  {
				if (bodyHtml != null)  return bodyHtml;
				ArrayList a = new ArrayList(8);
				getLeaves(this.MimeTree, a, false);
				foreach (MimePart part in a)  {
					if (part is MimePartTextHtml)  {
						bodyHtml = part as MimePartTextHtml;
						break;
					}
				}
				return bodyHtml;
			}
		}
		protected MimePartTextHtml  bodyHtml;
		
		/// <summary>Writes the string "message" to a file named "path",
		/// replacing it if it exists, in a well chosen encoding.</summary>
		static public void ToFile(string message, string path)  {
			Filez.WriteTextToFile(message, path, false, messageToFileEncoding);
			// All messages should come in ASCII encoding, but if they don't, we
			// might lose characters if we used ASCII. So we use UTF-8 without BOM,
			// which includes ASCII but won't lose characters.
		}
		
		static private Encoding messageToFileEncoding = new UTF8Encoding(false);
	}
}
