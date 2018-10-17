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
 *--------------------------------------------------------------------------*/
#endregion

#region Refrences
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents a IMAP mail message.
    /// </summary>
    #endregion
    public class ImapMailboxMessage
    {
        #region private variables
        private ImapMessageFlags _flags;
        private ImapAddressCollection _addresses;
        private ImapMessageBodyPartList _bodyparts;
        int _id;
        string _uid;
        DateTime _received;
        int _size;
        DateTime _sent;
        string _timezone;
        string _messageid;
        string _reference;
        string _subject;
        string _errors;
        bool _hashtml;
        bool _hastext;
        int _html;
        int _text;
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the ImapMessageFlags for a ImapMessage.
        /// </summary>
        public ImapMessageFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }
        /// <summary>
        /// Gets or sets the ImapAddressCollection for a ImapMessage.
        /// </summary>
        public ImapAddressCollection Addresses
        {
            get { return _addresses; }
            set { _addresses = value; }
        }
        /// <summary>
        /// Gets the from address from the ImapAddressCollection.
        /// </summary>
        public ImapAddress From
        {
            get { return Addresses.From; }
        }
        /// <summary>
        /// Gets the reply to address from the ImapAddressCollection.
        /// </summary>
        public ImapAddress ReplyTo
        {
            get { return Addresses.ReplyTo; }
        }
        /// <summary>
        /// Gets the sender address from the ImapAddressCollection.
        /// </summary>
        public ImapAddress Sender
        {
            get { return Addresses.Sender; }
        }
        /// <summary>
        /// Gets the to addresses from the ImapAddressCollection.
        /// </summary>
        public ImapAddressList To
        {
            get { return Addresses.To; }
        }
        /// <summary>
        /// Gets the CC addresses from the ImapAddressCollection.
        /// </summary>
        public ImapAddressList CC
        {
            get { return Addresses.CC; }
        }
        /// <summary>
        /// Gets the BCC addresses from the ImapAddressCollection.
        /// </summary>
        public ImapAddressList BCC
        {
            get { return Addresses.BCC; }
        }
        /// <summary>
        /// Get message number
        /// </summary>
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// Gets or sets the UID of the message.
        /// </summary>
        public string UID
        {
            get { return _uid; }
            set { _uid = value; }
        }
        /// <summary>
        /// Get date message was received.
        /// </summary>
        public DateTime Received
        {
            get { return _received; }
            set { _received = value; }
        }
        /// <summary>
        /// Get integer value indicating the size of the message.
        /// </summary>
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
        /// <summary>
        /// Get date time value indicating the date and time the message was sent.
        /// </summary>
        public DateTime Sent
        {
            get { return _sent; }
            internal set { _sent = value; }
        }
        /// <summary>
        /// Get string value indicating the time zone.
        /// </summary>
        public string TimeZone
        {
            get { return _timezone; }
            internal set { _timezone = value; }
        }
        /// <summary>
        /// Get string value indicating the Message ID
        /// </summary>
        public string MessageID
        {
            get { return _messageid; }
            set { _messageid = value; }
        }
        /// <summary>
        /// Get string value indicating what Message ID the Message refrences.
        /// </summary>
        public string Reference
        {
            get { return _reference; }
            set { _reference = value; }
        }
        /// <summary>
        /// Gets a string value containing the message subject.
        /// </summary>
        public string Subject
        {
            get { return _subject; }
            internal set { _subject = value; }
        }
        /// <summary>
        /// Gets errors so they can be displayed in a data grid.
        /// </summary>
        public string Errors
        {
            get { return _errors; }
            internal set { _errors = value; }
        }
        /// <summary>
        /// Gets message body parts that have been parsed.
        /// </summary>
        public ImapMessageBodyPartList BodyParts
        {
            get { return _bodyparts; }
            internal set { _bodyparts = value; }
        }
        /// <summary>
        /// Gets a boolean value indicating if the message contains a text part.
        /// </summary>
        public bool HasText
        {
            get { return _hastext; }
            internal set { _hastext = value; }
        }
        /// <summary>
        /// Gets a boolean value indicating if the message contains an html part.
        /// </summary>
        public bool HasHTML
        {
            get { return _hashtml; }
            internal set { _hashtml = value; }
        }
        /// <summary>
        /// Gets the index of the HTML part in the bodypart list.
        /// </summary>
        public int HTML
        {
            get { return _html; }
            internal set { _html = value; }
        }
        /// <summary>
        /// Gets the index of the text part in the bodypart list.
        /// </summary>
        public int Text
        {
            get { return _text; }
            internal set { _text = value; }
        }
        #endregion
    }
}
