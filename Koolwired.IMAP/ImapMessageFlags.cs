#region Copyright (c) Koolwired Solutions, LLC.
/*--------------------------------------------------------------------------
 * Copyright (c) 2006, Koolwired Solutions, LLC.
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
 * 09/29/06   Keith Kikta     Inital release.
 * 07/24/07	  Scott A. Braithwaite	Altered ParseFlags to allow correct splitting
 *            and to ignore any unknown flags.  Flags can be added at anytime
 *            according to the RFC 3501 so we'll just disregard any we don't know
 *--------------------------------------------------------------------------*/
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents IMAP message flags.
    /// </summary>
    #endregion
    public class ImapMessageFlags
    {
        #region private variables
        private bool _answered;
        private bool _deleted;
        private bool _draft;
        private bool _flagged;
        private bool _recent;
        private bool _seen;
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets a boolean value representing if the message has been seen.
        /// </summary>
        public bool Seen
        {
            get { return _seen; }
            set { _seen = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value representing if the message is deleted.
        /// </summary>
        public bool Deleted
        {
            get { return _deleted; }
            set { _deleted = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value representing if the message is a draft.
        /// </summary>
        public bool Draft
        {
            get { return _draft; }
            set { _draft = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value representing if the message was answered.
        /// </summary>
        public bool Answered
        {
            get { return _answered; }
            set { _answered = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value representing if the message is flagged.
        /// </summary>
        public bool Flagged
        {
            get { return _flagged; }
            set { _flagged = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value representing if the message is recent.
        /// </summary>
        public bool Recent
        {
            get { return _recent; }
            set { _recent = value; }
        }
        #endregion
        
        #region public constructors
        /// <summary>
        /// Initalizes a instance of the ImapMailbox
        /// </summary>
        public ImapMessageFlags() { }
        /// <summary>
        /// Initalizes a instance of the ImapMailbox
        /// </summary>
        /// <param name="draft">A boolean value representing the draft flag.</param>
        /// <param name="answered">A boolean value representing the answered flag.</param>
        /// <param name="flagged">A boolean value representing the flagged flag.</param>
        /// <param name="deleted">A boolean value representing the deleted flag.</param>
        /// <param name="seen">A boolean value representing the seen flag.</param>
        /// <param name="recent">A boolean value representing the recent flag.</param>
        public ImapMessageFlags(bool draft, bool answered, bool flagged, bool deleted, bool seen, bool recent)
        {
            this.Draft = draft;
            this.Answered = answered;
            this.Flagged = flagged;
            this.Deleted = deleted;
            this.Seen = seen;
            this.Recent = recent;
        }
        /// <summary>
        /// Initalizes a instance of the ImapMailbox
        /// </summary>
        /// <param name="flags">A string value containing a list of flags.</param>
        public ImapMessageFlags(string flags)
        {
            ParseFlags(flags);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Parses the flags of the message and sets boolean fields.
        /// </summary>
        /// <param name="flags">A string containing the list of flags.</param>
        public void ParseFlags(string flags)
        {
            string[] key;
             
            //Split on spaces instead.  Not all flags will start with \
            //key = flags.Split('\\');
            key = flags.Split();
            
            if (key.Length > 0)
            {
                for (int i = 0; i < key.Length; i++)
                    switch (key[i].Trim())
                    {
                        case "\\Draft":
                            this.Draft = true;
                            break;
                        case "\\Answered":
                            this.Answered = true;
                            break;
                        case "\\Flagged":
                            this.Flagged = true;
                            break;
                        case "\\Deleted":
                            this.Deleted = true;
                            break;
                        case "\\Seen":
                            this.Seen = true;
                            break;
                        case "\\Recent":
                            this.Recent = true;
                            break;
                        /*default:
                            throw new Exception(key[i].ToString());*/
                    }
            }
        }
        #endregion

    }
}
