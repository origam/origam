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
 * 09/16/06   Keith Kikta     Inital release.
 *--------------------------------------------------------------------------*/
#endregion

#region Refrences
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents an Imap Mailbox.
    /// </summary>
    #endregion
    public partial class ImapMailbox
    {
        #region private variables
        private string _mailbox;
        private int _exist;
        private int _recent;
        private bool _readwrite;
        private ImapMessageFlags _flags;
        private List<ImapMailboxMessage> _messages;
        #endregion

        #region public properties
        /// <summary>
        /// Gets a string value representing the current mailbox.
        /// </summary>
        public string Mailbox
        {
            get { return _mailbox; }
            internal set { _mailbox = value; }
        }
        /// <summary>
        /// Gets a integer value representing the number of messages in the mailbox.
        /// </summary>
        public int Exist
        {
            get { return _exist; }
            internal set { _exist = value; }
        }
        /// <summary>
        /// Gets a integer value representing the number of "Recent" messages in the mailbox.
        /// </summary>
        public int Recent
        {
            get { return _recent; }
            internal set { _recent = value; }
        }
        /// <summary>
        /// Gets a object of type <see cref="ImapMessageFlags" /> that contains the flags in the mailbox.
        /// </summary>
        public ImapMessageFlags Flags
        {
            get { return _flags; }
            internal set { _flags = value; }
        }
        /// <summary>
        /// Gets or sets the list object that contains a collection of ImapMailboxMessage.
        /// </summary>
        public List<ImapMailboxMessage> Messages
        {
            get { return (List<ImapMailboxMessage>)_messages; }
            set { _messages = (List<ImapMailboxMessage>)value; }
        }
        /// <summary>
        /// Gets a boolean value representing if the mailbox is writable and readable.
        /// </summary>
        /// 
        public bool ReadWrite
        {
            get { return _readwrite; }
            internal set { _readwrite = value; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// Initalizes an instance of the mailbox object.
        /// </summary>
        public ImapMailbox() { }
        /// <summary>
        /// Initalizes an instance of the mailbox object specifying the mailbox name.
        /// </summary>
        /// <param name="mailbox">A string representing the mailbox name.</param>
        public ImapMailbox(string mailbox)
        {
            this.Mailbox = mailbox;
        }
        /// <summary>
        /// Initalizes an instance of the mailbox object specifiying the mailbox name and how man messages it contains.
        /// </summary>
        /// <param name="mailbox">A string representing the mailbox name.</param>
        /// <param name="exist">An integer value representing the number of messages that exist in the mailbox.</param>
        public ImapMailbox(string mailbox, int exist)
        {
            this.Mailbox = mailbox;
            this.Exist = exist;
        }
        /// <summary>
        /// Initalizes an instance of the mailbox object specifiying the mailbox name and how man messages it contains.
        /// </summary>
        /// <param name="mailbox">A string representing the mailbox name.</param>
        /// <param name="exist">An integer value representing the number of messages that exist in the mailbox.</param>
        /// <param name="recent">A integer value representing the number of messages that are marked recent in the mailbox.</param>
        public ImapMailbox(string mailbox, int exist, int recent)
        {
            this.Mailbox = mailbox;
            this.Exist = exist;
            this.Recent = recent;
        }
        #endregion
    }
}
