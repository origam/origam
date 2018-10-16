#region Copyright (c) Koolwired Solutions, LLC.
/*--------------------------------------------------------------------------
 * Copyright (c) 2007, Koolwired Solutions, LLC.
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
 * 09/16/06   Keith Kikta     Inital release.
 * 06/05/07   Keith Kikta     Applied patch for attachments that do not contain charset.
 * 01/07/08   Keith Kikta     Added complex regular expressions to parse headers.
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
    #region Header
    /// <summary>
    /// Represents an Imap Body Part
    /// </summary>
    #endregion
    public class ImapMessageBodyPart
    {
        #region constants
        const string non_attach = "^\\((?<type>(\"[^\"]*\"|NIL))\\s(?<subtype>(\"[^\"]*\"|NIL))\\s(?<attr>(\\([^\\)]*\\)|NIL))\\s(?<id>(\"[^\"]*\"|NIL))\\s(?<desc>(\"[^\"]*\"|NIL))\\s(?<encoding>(\"[^\"]*\"|NIL))\\s(?<size>(\\d+|NIL))\\s(?<lines>(\\d+|NIL))\\s(?<md5>(\"[^\"]*\"|NIL))\\s(?<disposition>(\\([^\\)]*\\)|NIL))\\s(?<lang>(\"[^\"]*\"|NIL))\\)$";
        const string attachment = "^\\((?<type>(\"[^\"]*\"|NIL))\\s(?<subtype>(\"[^\"]*\"|NIL))\\s(?<attr>(\\(\"[^\"]*\"(\\s+\"[^\"]*\")*\\)|NIL))\\s(?<id>(\"[^\"]*\"|NIL))\\s(?<desc>(\"[^\"]*\"|NIL))\\s(?<encoding>(\"[^\"]*\"|NIL))\\s(?<size>(\\d+|NIL))\\s((?<data>(.*))\\s|)(?<lines>(\"[^\"]*\"|NIL))\\s(?<disposition>((?>\\((?<LEVEL>)|\\)(?<-LEVEL>)|(?!\\(|\\)).)+(?(LEVEL)(?!))|NIL))\\s(?<lang>(\"[^\"]*\"|NIL))\\)$";
        #endregion
        
        #region private variables
        System.Net.Mime.ContentType _ContentType = new System.Net.Mime.ContentType();
        bool _attachment;
        string _contentid;
        string _contentdescription;
        BodyPartEncoding _encoding;
        long _size;
        long _lines;
        string _hash;
        int index = 0;
        string _data;
        string _language;
        string _bodytype;
        string _filename;
        string _bodypart;
        string _disposition;
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets a boolean value representing if the body part is an attachment.
        /// </summary>
        public bool Attachment
        {
            set { _attachment = value; }
            get { return _attachment; }
        }
        /// <summary>
        /// Gets or sets the message data (Encoded)
        /// </summary>
        public string Data
        {
            set { _data = value; }
            get { return _data; }
        }
        /// <summary>
        /// Gets the message data in binary format.
        /// </summary>
        public byte[] DataBinary
        {
            get { return Encoding.GetBytes(_data); /*Convert.FromBase64String(_data);*/ }
        }
        /// <summary>
        /// Gets the text that identfies the body part of the message for retevial from the server.
        /// </summary>
        public string BodyPart
        {
            internal set { _bodypart = value; }
            get { return _bodypart; }
        }
        /// <summary>
        /// Gets or sets the file name of an attachment.
        /// </summary>
        public string FileName 
        {
            set { _filename = value; }
            get { return _filename; }
        }
        /// <summary>
        /// Gets or sets the content type of an body type.
        /// </summary>
        public System.Net.Mime.ContentType ContentType
        {
            set { _ContentType = value; }
            get { return _ContentType; }
        }
        /// <summary>
        /// Gets or sets the content ID.
        /// </summary>
        public string ContentID
        {
            set { _contentid = value; }
            get { return _contentid; }
        }
        /// <summary>
        /// Gets or sets the content description.
        /// </summary>
        public string ContentDescription
        {
            set { _contentdescription = value; }
            get { return _contentdescription; }
        }
        /// <summary>
        /// Gets or sets the encoding of body part.
        /// </summary>
        public BodyPartEncoding ContentEncoding
        {
            set { _encoding = value; }
            get { return _encoding; }
        }
        /// <summary>
        /// Gets the encoding type of a message.
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                try
                {
                    if (this.ContentType.CharSet == null)
                    {
                        return Encoding.Default;
                    }
                    else
                    {
                        string encoding = this.ContentType.CharSet.Split('/')[1];
                        return System.Text.Encoding.GetEncoding(encoding);
                    }
                }
                catch
                {
                    return Encoding.Default;
                }
            }
        }
        /// <summary>
        /// Gets or sets the size of body part.
        /// </summary>
        public long Size
        {
            set { _size = value; }
            get { return _size; }
        }
        /// <summary>
        /// Gets or sets the number of lines in a body part.
        /// </summary>
        public long Lines
        {
            set { _lines = value; }
            get { return _lines; }
        }
        /// <summary>
        /// Gets or sets the MD5 hash of a body part.
        /// </summary>
        public string ContentMD5
        {
            set { _hash = value; }
            get { return _hash; }
        }
        /// <summary>
        /// Gets or sets the content language of a body part.
        /// </summary>
        public string ContentLanguage
        {
            set { _language = value; }
            get { return _language; }
        }
        /// <summary>
        /// Gets or sets the body type of a body part.
        /// </summary>
        public string BodyType 
        { 
            set { _bodytype = value; }
            get { return _bodytype; }
        }
        /// <summary>
        /// Gets or sets the content disposition.
        /// </summary>
        public string Disposition {
            set { _disposition = value; }
            get { return _disposition; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the IampMessageBodyPart class.
        /// </summary>
        /// <param name="data">A string containing the message headers for a body part.</param>
        public ImapMessageBodyPart(string data)
        {
            Match match = Regex.Match(data, non_attach, RegexOptions.ExplicitCapture);
            bool isAttachment = false;
            if (MatchFileName(match.Groups["attr"].Value).Success)
            {
                isAttachment = true;
            }

            if (match.Success && ! isAttachment)
            {
                this.Attachment = false;
                this.ContentType.MediaType = string.Format("{0}/{1}", match.Groups["type"].Value.Replace("\"", ""), match.Groups["subtype"].Value.Replace("\"", ""));
                ParseCharacterSet(ParseNIL(match.Groups["attr"].Value));
                this.ContentID = ParseNIL(match.Groups["id"].Value);
                this.ContentDescription = ParseNIL(match.Groups["desc"].Value);
                this.ContentEncoding = ParseEncoding(ParseNIL(match.Groups["encoding"].Value));
                this.Size = Convert.ToInt64(ParseNIL(match.Groups["size"].Value));
                this.Lines = Convert.ToInt64(ParseNIL(match.Groups["lines"].Value));
                this.ContentMD5 = ParseNIL(match.Groups["md5"].Value);
                this.Disposition = ParseNIL(match.Groups["disposition"].Value);
                this.ContentLanguage = ParseNIL(match.Groups["lang"].Value);
            }
            else if ((match = Regex.Match(data, attachment, RegexOptions.ExplicitCapture)).Success)
            {
                this.Attachment = true;
                this.ContentType.MediaType = string.Format("{0}/{1}", match.Groups["type"].Value.Replace("\"", ""), match.Groups["subtype"].Value.Replace("\"", ""));
                ParseCharacterSet(ParseNIL(match.Groups["attr"].Value));
                ParseFileName(ParseNIL(match.Groups["attr"].Value));
                this.ContentID = ParseNIL(match.Groups["id"].Value);
                this.ContentDescription = ParseNIL(match.Groups["desc"].Value);
                this.ContentEncoding = ParseEncoding(ParseNIL(match.Groups["encoding"].Value));
                this.Size = Convert.ToInt64(ParseNIL(match.Groups["size"].Value));
                this.Lines = Convert.ToInt64(ParseNIL(match.Groups["lines"].Value));
                this.Disposition = ParseNIL(match.Groups["disposition"].Value);

                if (isAttachment == false && (this.Disposition == null || !(this.Disposition.ToLower().Contains("attachment") || this.Disposition.ToLower().Contains("inline"))))
                {
                    this.Attachment = false;
                }

                this.ContentLanguage = ParseNIL(match.Groups["lang"].Value);
            }
            else
                throw new Exception("Invalid format could not parse body part headers.");
        }
        #endregion

        #region private methods
        private void ParseContentType(string data)
        {
            string[] part = new string[2];
            part[0] = data.Substring(data.IndexOf("\"") + 1, data.IndexOf("\"", data.IndexOf("\"") + 1) - (data.IndexOf("\"") + 1));
            part[1] = data.Substring(data.IndexOf("\"", data.IndexOf("\"", data.IndexOf("\"") + 1) + 1) + 1, data.IndexOf("\"", data.IndexOf("\"", data.IndexOf("\"", data.IndexOf("\"") + 1) + 1) + 1) - (data.IndexOf("\"", data.IndexOf("\"", data.IndexOf("\"") + 1) + 1) + 1));
            this.ContentType.MediaType = string.Format("{0}/{1}", part[0], part[1]);
            index = data.IndexOf("\"", data.IndexOf("\"", data.IndexOf("\"", data.IndexOf("\"") + 1) + 1) + 1) + 1;
        }
        private void ParseCharacterSet(string data)
        {
            if (data != null)
            {

                Match match = Regex.Match(data, "\"charset\"\\s\"(?<set>([^\"]*))\"", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                if (match.Success)
                    this.ContentType.CharSet = string.Format("charset/{0}", match.Groups["set"].Value);
            }
        }
        private void ParseFileName(string data)
        {
            if (data != null)
            {
                Match match = MatchFileName(data);
                if (match.Success)
                {
                    this.FileName = ImapDecode.Decode(match.Groups["file"].Value);

                    //bugfix: remove TAB from a file name (in case the file name was longer
                    // than one line, there could be a TAB on the second line
                    this.FileName = this.FileName.Replace("\t", "");
                }
            }
        }
        private Match MatchFileName(string data)
        {
            return Regex.Match(data, "\"name\"\\s\"(?<file>([^\"]*))\"", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }

        private string ParseNIL(string data)
        {
            if (data.Trim() == "NIL")
                return null;
            return data;
        }
        private BodyPartEncoding ParseEncoding(string data)
        {
            if (data == null)
                return BodyPartEncoding.UNKNOWN;
            data = data.Replace("\"", "").ToUpper();
            switch (data.Substring(0,1))
            {
                case "7": return BodyPartEncoding.UTF7;
                case "8":
                    if (data.Substring(1).CompareTo("BIT") == 0)
                        return BodyPartEncoding.EIGHT_BIT;
                    return BodyPartEncoding.UTF8;
                case "B":
                    if (data.CompareTo("BASE64") == 0)
                        return BodyPartEncoding.BASE64;
                    else if (data.CompareTo("BINARY") == 0)
                        return BodyPartEncoding.NONE;
                    else
                        return BodyPartEncoding.UNKNOWN;
                case "Q":
                    if (data.CompareTo("QUOTED-PRINTABLE") == 0)
                        return BodyPartEncoding.QUOTEDPRINTABLE;
                    return BodyPartEncoding.UNKNOWN;
                default:
                    return BodyPartEncoding.UNKNOWN;
            }
        }
        #endregion
    }
}
