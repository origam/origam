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

using  ApplicationException = System.ApplicationException;
using ArgumentNullException = System.ArgumentNullException;
using Environment           = System.Environment;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace NandoF.Web
{
	public class HtmlHelper
	{
		#region static public Encoding FindEncodingIn(string html)
		/// <summary>Tries to find the encoding name in the headers of an HTML
		/// string.</summary>
		/// <param name="html">The string to be searched.</param>
		/// <returns>The Encoding if found, else null.</returns>
		/// <remarks>This method searches only the top 1200 characters.</remarks>
		static public Encoding FindEncodingIn(string html)  {
			if (html==null)  throw new ArgumentNullException("html");
			// Search only the top of the file
			int len = html.Length;
			if (len > 1200)  len = 1200;
			Match m = rxHtmlEncoding1.Match(html, 0, len);
			if (!m.Success)  m = rxHtmlEncoding2.Match(html, 0, len);
			if (!m.Success)  return null;
			string encName = m.Groups["encoding"].Value;
			return Encoding.GetEncoding(encName);
		}
		const string ENCODING_START = @"<meta\s+";
		const string ENCODING_KEY   = "http-equiv=\"Content-Type\"";
		const string ENCODING_VAL   = "content=\"text/html;" +
			@"\s+charset=(?<encoding>\S{3,20}?)" + "\"";
		const string ENCODING_END   = @"\s*/?>";
		static public Regex    rxHtmlEncoding1 =
			new Regex(ENCODING_START + ENCODING_KEY + @"\s+" + ENCODING_VAL +
			          ENCODING_END,
			          RegexOptions.IgnoreCase | RegexOptions.Singleline);
		static public Regex    rxHtmlEncoding2 =
			new Regex(ENCODING_START + ENCODING_VAL + @"\s+" + ENCODING_KEY +
			          ENCODING_END,
			          RegexOptions.IgnoreCase | RegexOptions.Singleline);
		#endregion
		
		static public int PositionOfTag(string tagName, string html,
		                                out int positionAfterTag)  {
			Regex rx = new Regex("<" + tagName + ".*?>");
			Match m  = rx.Match(html);
			if (!m.Success)  throw new ApplicationException
				("Could not find tag: " + tagName);
			positionAfterTag = m.Index + m.Length;
			return m.Index;
		}
		static public int PositionOfTag(string tagName, string html)  {
			int p;
			return PositionOfTag(tagName, html, out p);
		}
		
		/// <param name="html">An HTML string.</param>
		/// <returns>Only what is inside the &lt;body&gt; tag.</returns>
		static public  string   KeepBody  (string html)  {
			if (html==null)  throw new ArgumentNullException("html");
			Match m = rxKeepBody.Match(html);
			if (!m.Success)  throw new
				ApplicationException("Unable to find body of HTML.");
			else  {
				return m.Groups["Content"].Value;
			}
		}
		static private Regex  rxKeepBody =
			new Regex(@"<body[^><]*>(?<Content>.+?)</body>",
			          RegexOptions.IgnoreCase | RegexOptions.Singleline);
		
		/// <param name="html">Usually only contents of the &lt;body&gt; tag.</param>
		/// <returns>A string array. The split is based on a
		/// "page-break-before: always" attribute of any paragraph or heading tag.
		/// </returns>
		static public  string[] SplitPages(string html)  {
			if (html==null)  throw new ArgumentNullException("html");
			if (html=="")   return new string[0];
			MatchCollection mm = rxSplitPages.Matches(html);
			int numberOfParts  = mm.Count + 1;
			ArrayList    parts = new ArrayList(numberOfParts);
			int pos = 0;
			string part;
			string previousMatch = "";
			foreach(Match m in mm)  {
				part = html.Substring(pos, m.Index - pos);
				part = removePageBreakIn(part, previousMatch);
				if (part != "") parts.Add(part);
				pos = m.Index;
				previousMatch = m.Value;
			}
			//if (mm.Count > 0)  {
				
				// Add the last part (in case there were page breaks)
				// or  the only part (in case there weren't)
				part = html.Substring(pos, html.Length - pos);
				part = removePageBreakIn(part, previousMatch);
				if (part != "") parts.Add(part);
				
			//}
			return (string[]) parts.ToArray(typeof(string));
		}
		static private Regex  rxSplitPages = new Regex
			(@"<[ph]{1}[^><]*style=""[^><]*page-break-before: always[^><]*""[^><]*>",
			 RegexOptions.IgnoreCase | RegexOptions.Singleline);
		static private string removePageBreakIn(string part, string firstTag)  {
			if (firstTag.Length < 3)  return part;
			// If second page or later, throw away the page break,
			// keeping only the tag -- e.g. "<p>", "<h1>", "<h4>"...
			string tag = firstTag.Substring(0, 2).ToLower();
			if (tag=="<h") tag += firstTag.Substring(2, 1);
			tag += ">";
			return part.Replace(firstTag, tag);
		}
		
//		static public  string   RemoveClasses(string html)  {
//
//		}
//		static private Regex  rxRemoveClasses = new Regex
//			(@"<\w+ [^><]*class=""[^><]*""[^><]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		
		// TODO: static public string AddAnchors(string html)  {
		// Loop
		// find a link
		// See if it is inside an anchor already
		// If not, add the link
//		}
		
		public string Massage(string html)  {
			string current = html;
			if (RemoveAttribs)     current = removeAttribs(current);
			if (this.RemoveSpans)  current = removeSpans  (current);
			current = removeAllTagInstances( "font", current);
			current = removeAllTagInstances("/font", current);
			// Make these tags lower-case
			current = current.Replace( "<P>",  "<p>");
			current = current.Replace("</P>", "</p>");
			current = current.Replace("<BR>", "<br />");
			current = current.Replace( "<OL>",  "<ol>");
			current = current.Replace("</OL>", "</ol>");
			current = current.Replace( "<UL>",  "<ul>");
			current = current.Replace("</UL>", "</ul>");
			current = current.Replace( "<LI>", "<li>");
			current = current.Replace("</LI>", "</li>");
			current = current.Replace("<STRONG>" , "<strong>" );
			current = current.Replace("</STRONG>", "</strong>");
			current = current.Replace("<TITLE>"  , "</title>");
			// Update deprecated HTML tags
			current = current.Replace("<I>" , "<em>");
			current = current.Replace("</I>", "</em>");
			current = current.Replace("<B>" , "<strong>");
			current = current.Replace("</B>", "</strong>");
			if (UpdateUnderline)  current = updateUnderLine(current);
			
			// Convert end-heading tags to lower case
			Regex r = new Regex(@"</H(?<Level>\d).*?>");
			Match m = r.Match(current);
			while (m.Success)  {
				current = current.Replace(m.Value, m.Result("</h$1>"));
				m = m.NextMatch();
			}
			// Configurable actions (the order is important)
			if (this.RemoveEmptyStrong)  current = removeEmptyStrong(current);
			if (RemoveFormatInHeaders)   current = removeFormatInHeaders(current);
			if (this.FixBROrder)         current = fixBROrder (current);
			if (ChangeBRToP)             current = changeBRToP(current);
			// Remove weird that remains: <p><br /></p>
			if (RemoveEmptyParagraphs)   current = removeEmptyParagraphs(current);
			// Substitute any line feeds that are NOT just before or after a tag
			// with a space (improves HTML legibility)
			if (LimitLineFeeds)          current = limitLineFeeds(current);
			return current;
		}
		
		/// <summary>Removes attributes from paragraphs and headings.</summary>
		public bool RemoveAttribs = true;
		string removeAttribs(string html)  {
			if (html==null)  throw new ArgumentNullException("html");
			// Remove attributes from <p>
			Regex rp = new Regex(@"<P(?<attribs>[^><]+)>",
			                     RegexOptions.IgnoreCase |
			                     RegexOptions.Singleline); // novidade
			string current = rp.Replace(html,"<p>");
			// Remove attributes from headings
			Regex r = new Regex(@"<H(?<Level>\d)[^><]+>", RegexOptions.IgnoreCase);
			Match m  = r.Match(current);
			while (m.Success)  {
				current = current.Replace(m.Value, m.Result("<h$1>"));
				m = m.NextMatch();
			}
			return current;
		}
		
		public bool RemoveSpans = true;
		string removeSpans(string html)  {
			if (html==null)  throw new ArgumentNullException("html");
			string s = html.Replace("</span>", "");
			s        = html.Replace("</SPAN>", "");
			MatchCollection mm = rxRemoveSpans.Matches(html);
			foreach (Match m in mm)  {
				s = s.Replace(m.Value, "");
			}
			return s;
		}
		static Regex rxRemoveSpans = new Regex
			(@"<span[^><]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		
		/// <summary>Updates the deprecated &lt;U&gt; tag.</summary>
		public bool UpdateUnderline = true;
		string updateUnderLine(string text)  {
			string current = text;
			// OpenOffice strangely puts every anchor inside an underline
			current = Regex.Replace(current, "<U><A ", "<a ",
			                        RegexOptions.IgnoreCase);
			current = Regex.Replace(current, "</A></U>", "</a>",
			                        RegexOptions.IgnoreCase);
			// Now replace the rest of the <U> tags
			current = Regex.Replace(current, "<U>",
			                        "<span style='text-decoration:underline;'>",
			                        RegexOptions.IgnoreCase);
			current = Regex.Replace(current, "</U>", "</span>",
			                        RegexOptions.IgnoreCase);
			return current;
		}
		
		/// <summary>Simplifies &lt;h1&gt;, &lt;h2&gt; etc.</summary>
		public bool RemoveFormatInHeaders = true;
		string removeFormatInHeaders(string html)  {
			// Turn "<h1><strong>" into just "<h1>"
			Regex r = new Regex(@"<h(\d)>\s*<strong>\s*",
			                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
			Match m = r.Match(html);
			string s = html;
			while (m.Success)  {
				s = s.Replace(m.Value, m.Result("<h$1>"));
				m = m.NextMatch();
			}
			
			// Turn "</strong></h1>" into just "</h1>"
			r = new Regex(@"\s*</strong>\s*</h(\d)>",
			              RegexOptions.IgnoreCase | RegexOptions.Singleline);
			m = r.Match(s);
			while (m.Success)  {
				s = s.Replace(m.Value, m.Result("</h$1>"));
				m = m.NextMatch();
			}
			return s;
		}
		
		public bool FixBROrder = true;
		string fixBROrder(string html)  {
			string s = html;
			s = Regex.Replace(s, @"<br ?/?>\s*</span>", "</span><br />",
			                  RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s = Regex.Replace(s, @"<br ?/?>\s*</strong>", "</strong><br />",
			                  RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s = Regex.Replace(s, @"<br ?/?>\s*</em>", "</em><br />",
			                  RegexOptions.IgnoreCase | RegexOptions.Singleline);
			return s;
		}
		
		/// <summary>Removes empty &lt;strong&gt;&lt;/strong&gt; and variations
		/// </summary>
		public bool RemoveEmptyStrong = true;
		string removeEmptyStrong(string html)  {
			string s = Regex.Replace(html, @"</strong>\s+<strong>", " ",
			                         RegexOptions.IgnoreCase);
			s = Regex.Replace(s, @"<strong>\s+</strong>", " ",
			                  RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s = Regex.Replace(s, @"<strong></strong>"   , "",
			                  RegexOptions.IgnoreCase);
			s = Regex.Replace(s, @"</strong><strong>"   , "",
			                  RegexOptions.IgnoreCase);
			s = Regex.Replace(s, @"<strong>\s*<br />\s*</strong>", "<br />",
			                  RegexOptions.IgnoreCase | RegexOptions.Singleline);
			return s;
		}
		
		public bool ChangeBRToP     = true;
		string changeBRToP(string html)  {
			return Regex.Replace(html, @"\s*<br\s*/?>\s*",
			                     "</p>" + Text.Repeat(Environment.NewLine, 2) + "<p>",
			                     RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		
		public bool RemoveEmptyParagraphs = true;
		string removeEmptyParagraphs(string text)  {
			string current =
				Regex.Replace(text, @"[ ]*<br />\s*<br />[ ]*", "<br />",
				              RegexOptions.IgnoreCase | RegexOptions.Singleline);
			current = Regex.Replace(current, @"[ ]*<p>\s*</p>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			current = Regex.Replace(current, @"[ ]*<h\d>\s*</h\d>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			current = Regex.Replace(current,
			                        @"[ ]*<p>\s*<br />\s*</p>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			current = Regex.Replace(current,
			                        @"[ ]*<h\d>\s*<br />\s*</h\d>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			current = Regex.Replace(current, @"[ ]*<p>\s*&nbsp;\s*</p>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			current = Regex.Replace(current, @"[ ]*<h\d>\s*&nbsp;\s*</h\d>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			current = Regex.Replace(current,
			                        @"[ ]*</UL>\s*<UL>[ ]*", "",
			                        RegexOptions.IgnoreCase |
			                        RegexOptions.Singleline);
			return current;
		}
		
		public bool LimitLineFeeds = true;
		/// <summary>Removes line feeds (except those after a few important tags)
		/// in order to improve HTML legibility.</summary>
		string limitLineFeeds(string text)  {
			string current = text;
			StringCollection lines = Text.LinesInString(current);
			StringBuilder    sb    = new StringBuilder(current.Length);
			string[] importantTags = new string[]  {
				"<div>", "</div>",   "<table>", "</table>",   "<tr>", "</tr>",
				"<ol>", "</ol>",   "<ul>", "</ul>",   "<html>", "</html>",
				"<body>", "</body>",
				"</td>", "</th>", "</p>", "</head>", "</title>", "</style>", "</li>",
				"</h1>", "</h2>", "</h3>", "</h4>", "</h5>", "</h6>"
			} ;
			foreach (string line in lines)  {
				sb.Append(line.Replace("  ", " "));
				// See if line ends with an "important" tag
				bool breakLine = false;
				foreach (string tag in importantTags)  {
					if (!line.EndsWith(tag))  continue;
					breakLine = true;
					break;
				}
				if      (breakLine)            sb.Append(Environment.NewLine +
				                                         Environment.NewLine);
				else if (!line.EndsWith(" "))  sb.Append(" ");
			}
			current = sb.ToString();
			// After that, don't leave spaces at the start of a paragraph
			current = Regex.Replace(current, @"<p>\s*", "<p>");
			
			current = ensureEnterAfterTag("!DOCTYPE", current, 1);
			current = ensureEnterAfterTag("html"    , current, 1);
			current = ensureEnterAfterTag("head"    , current, 1);
			current = ensureEnterAfterTag("meta"    , current, 1);
			current = ensureEnterAfterTag("/title"  , current, 1);
			current = ensureEnterAfterTag("/style"  , current, 1);
			current = ensureEnterAfterTag("/head"   , current, 1);
			current = ensureEnterAfterTag("body"    , current);
			return current;
		}
		
		/// <summary>Returns a regular expression that matches the given tag.
		/// Its attributes are available through the "attribs" group.</summary>
		Regex tagWithAttribs(string tag)  {
			return new Regex("<" + tag + @"(?<attribs>.*?)>",
			                 RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		
		/// <summary>Ensures there is a line feed after the given tag</summary>
		string ensureEnterAfterTag(string tag, string html, byte numberOfEnters)  {
			string current = html;
			string enter = Text.Repeat(Environment.NewLine, numberOfEnters);
			Regex rx = tagWithAttribs(tag);
			Match m  = rx.Match(current);
			while (m.Success)  {
				string replacement = "<" + tag + m.Groups["attribs"] + ">";
				current = current.Replace(m.Value, replacement + enter);
				m = m.NextMatch();
			}
			return current;
		}
		string ensureEnterAfterTag(string tag, string html)  {
			return ensureEnterAfterTag(tag, html, 2);
		}
		
		string removeAllTagInstances(string tag, string html)  {
			Regex  rx      = tagWithAttribs(tag);
			return rx.Replace(html, "");
		}
		
		
	}
}
