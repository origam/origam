#region Copyright (c) Koolwired Solutions, LLC.
/*--------------------------------------------------------------------------
 * Copyright (c) 2006-2007, Koolwired Solutions, LLC.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution. 
 * Neither the name of Koolwired Solutions, LLC. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission. 
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
 * AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 * THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
 * OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
 * WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 *--------------------------------------------------------------------------*/
#endregion

#region History
/*--------------------------------------------------------------------------
 * Modification History: 
 * Date       Programmer      Description
 * 12/27/2007 Keith Kikta     Inital release. Decoding created by who ever created
 *                            GITSmail. Contact me if this is yours so I can give
 *                            you credit.
 * 04/22/2009 Michal Ziemski  Rewrite of decode method that uses encoding.
 * 04/22/2009 Stefano         Removed space between ?= and =? if it exists.
 *--------------------------------------------------------------------------*/
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Koolwired.Imap
{
    internal class ImapDecode
    {
        /// <summary>
        /// Decodes from ASCII representations of characters in the given encoding
        /// </summary>
        /// <param name="input">The input, can be many lines long, but only in one encoding.</param>
        /// <param name="enc">The encoding the input is coming from</param>
        /// <param name="replaceUnderscores">If true underscore are changed to spaces</param>
        /// <returns>The decoded string, suitable for .Net use</returns>
        internal static string Decode(string input, Encoding enc, bool replaceUnderscores)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            char[] chars = input.ToCharArray();
            byte[] bytes = new byte[chars.Length];

            int j = 0;
            for (int i = 0; i < chars.Length; i++, j++)
            {
                if (chars[i] == '=')
                {
                    i++;
                    if (chars.Length >= i + 2 &&
                    byte.TryParse(new string(chars, i, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out bytes[j]))
                        i++;
                    else
                        j--;
                }
                else if ((chars[i] == '_') && replaceUnderscores)
                    bytes[j] = (byte)' ';
                else
                    bytes[j] = (byte)chars[i];
            }
            return new string(enc.GetChars(bytes, 0, j)); 
        }

        /// <summary>
        /// Decodes a string to it's native encoding 
        /// Pulls from the string correcting multiple =?ENC?METHOD?TEXT?=
        /// </summary>
        /// <param name="input">The string with embedded encoding(s)</param>
        /// <returns>The decoded string, suitable for .Net use</returns>
        internal static string Decode(string input)
        {
            try
            {
                if (input == "" || input == null)
                    return "";
                Regex regex = new Regex(@"=\?(?<Encoding>[^\?]+)\?(?<Method>[^\?]+)\?(?<Text>[^\?]+)\?=");
                MatchCollection matches = regex.Matches(input);
                string ret = input;
                if (matches.Count > 1)
                    ret = ret.Replace("?= =?", "?==?");
                foreach (Match match in matches)
                {
                    string encoding = match.Groups["Encoding"].Value;
                    encoding = FixEncoding(encoding);
                    string method = match.Groups["Method"].Value;
                    string text = match.Groups["Text"].Value;
                    string decoded;
                    if (method.ToUpper() == "B")
                    {
                        byte[] bytes = Convert.FromBase64String(text);
                        Encoding enc = Encoding.GetEncoding(encoding);
                        decoded = enc.GetString(bytes);
                    }
                    else
                        decoded = Decode(text, Encoding.GetEncoding(encoding), true);
                    ret = ret.Replace(match.Groups[0].Value, decoded);
                }
                return ret;
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        internal static string FixEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "utf8": encoding = "UTF-8";
                    break;
                case "utf7": encoding = "UTF-7";
                    break;
            }
            return encoding;
        }
    }
}