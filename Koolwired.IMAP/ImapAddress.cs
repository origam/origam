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
using System.Text;
using System.Net.Mail;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents the address of an electronic mail sender or recipient.
    /// </summary>
    #endregion
    public class ImapAddress
    {
        #region private variables
        private string _address;
        private string _displayName;
        private Encoding _displayNameEncoding;
        #endregion

        #region public properties
        /// <summary>
        /// Gets the email address specified when the instance was created.
        /// </summary>
        public string Address
        {
            get { return _address; }
            internal set { _address = new System.Net.Mail.MailAddress(value).Address; }
        }
        /// <summary>
        /// Gets the display name composed from the display name and address specified when the instance was created.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            internal set { _displayName = value; }
        }
        /// <summary>
        /// The <see cref="System.Text.Encoding"/> that defines the character set used for displayName.
        /// </summary>
        internal Encoding DisplayNameEncoding {
            get { return _displayNameEncoding; }
            set { _displayNameEncoding = value; }
        }

        #endregion

        #region public methods
        /// <summary>
        /// Initalizes a new instance of the ImapAddress object using the specified address.
        /// </summary>
        /// <param name="address">A string value representing the email address.</param>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.FormatException" />
        public ImapAddress(string address)
        {
            new System.Net.Mail.MailAddress(address);
            Address = address;
        }
        /// <summary>
        /// Initalizes a new instance of the ImapAddress object using the specified address and display name.
        /// </summary>
        /// <param name="address">A string value representing the email address.</param>
        /// <param name="displayName">A string value representing the display name.</param>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.FormatException" />
        public ImapAddress(string address, string displayName)
        {
            new System.Net.Mail.MailAddress(address, displayName);
            Address = address;
            DisplayName = displayName;
        }
        /// <summary>
        /// Initalizes a new instance of the ImapAddress object using the specified address, display name and encoding.
        /// </summary>
        /// <param name="address">A string value representing the email address.</param>
        /// <param name="displayName">A string value representing the display name.</param>
        /// <param name="displayNameEncoding">A Encoding value representing the type of encoding used in the display name.</param>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.FormatException" />
        public ImapAddress(string address, string displayName, Encoding displayNameEncoding)
        {
            new System.Net.Mail.MailAddress(address, displayName, displayNameEncoding);
            Address = address;
            DisplayName = displayName;
            DisplayNameEncoding = displayNameEncoding;
        }
        /// <summary>
        /// Converts an <see cref="Koolwired.Imap.ImapAddress"/> to a <see cref="System.Net.Mail.MailAddress"/>.
        /// </summary>
        /// <returns></returns>
        public System.Net.Mail.MailAddress ToMailAddress()
        {
            return new System.Net.Mail.MailAddress(Address, DisplayName, DisplayNameEncoding);
        }
        /// <summary>
        /// Converts a <see cref="Koolwired.Imap.ImapAddress"/> to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (DisplayName != null && DisplayName.Length > 0)
                return string.Format("\"{0}\" <{1}>", DisplayName, Address);
            else
                return string.Format(Address);
        }
        #endregion
    }
}
