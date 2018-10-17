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
    #region public exceptions
    /// <summary>
    /// Represents an exception for the IMAP library.
    /// </summary>
    public class ImapException : Exception
    {
        /// <summary>
        /// Initalizes an Exception for the IMAP library.
        /// </summary>
        public ImapException() : base() { }
        /// <summary>
        /// Initalizes an Exception for the IMAP library using a message.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        public ImapException(string message) : base(message) { }
        /// <summary>
        /// Initalizes an Exception for the IMAP library using a message and inner exception.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        /// <param name="inner">An exception that this exception is includes.</param>
        public ImapException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Represents an exception of type ImapException.ImapConnectionException.
    /// </summary>
    public class ImapConnectionException : ImapException
    {
        /// <summary>
        /// Initalizes an Exception in the connect object.
        /// </summary>
        public ImapConnectionException() : base() { }
        /// <summary>
        /// Initalizes an Exception in the connect object using a message.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        public ImapConnectionException(string message) : base(message) { }
        /// <summary>
        /// Initalizes an Exception in the connect object using a message and inner exception.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        /// <param name="inner">An exception that this exception is includes.</param>
        public ImapConnectionException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Represents an exception in the authentication object.
    /// </summary>
    public class ImapAuthenticationException : ImapException
    {
        /// <summary>
        /// Initalizes an exception in the authentication object.
        /// </summary>
        public ImapAuthenticationException() : base() { }
        /// <summary>
        /// Initalizes an exception in the authentication object with a message.
        /// </summary>
        /// <param name="message"></param>
        public ImapAuthenticationException(string message) : base(message) { }
        /// <summary>
        /// Initalizes an exception in the authentication object with a message and inner exception.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        /// <param name="inner">An exception that this exception is includes.</param>
        public ImapAuthenticationException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Represents an exception in the authentication object for a unsupported authentication type.
    /// </summary>
    public class ImapAuthenticationNotSupportedException : ImapAuthenticationException
    {
        /// <summary>
        /// Initalizes an exception in the authentication object for a unsupported authentication type.
        /// </summary>
        public ImapAuthenticationNotSupportedException() : base() { }
        /// <summary>
        /// Initalizes an exception in the authentication object for a unsupported authentication type with a message.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        public ImapAuthenticationNotSupportedException(string message) : base(message) { }
        /// <summary>
        /// Initalizes an exception in the authentication object for a unsupported authentication type with a message and inner exception.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        /// <param name="inner">An exception that this exception is includes.</param>
        public ImapAuthenticationNotSupportedException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Represents an exception in the command object.
    /// </summary>
    public class ImapCommandException : ImapException
    {
        /// <summary>
        /// Initalizes an exception in the command object.
        /// </summary>
        public ImapCommandException() : base() { }
        /// <summary>
        /// Initalizes an exception in the command object with a message.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        public ImapCommandException(string message) : base(message) { }
        /// <summary>
        /// Initalizes an exception in the command object with a message and inner exception.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        /// <param name="inner">An exception that this exception is includes.</param>
        public ImapCommandException(string message, Exception inner) : base(message, inner) { }

    }
    /// <summary>
    /// Represents an exception in the command object for an invalid message number.
    /// </summary>
    public class ImapCommandInvalidMessageNumber : ImapCommandException
    {
        /// <summary>
        /// Initalizes an exception in the command object for an invalid message number.
        /// </summary>
        public ImapCommandInvalidMessageNumber() : base() { }
        /// <summary>
        /// Initalizes an exception in the command object for an invalid message number with a message.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        public ImapCommandInvalidMessageNumber(string message) : base(message) { }
        /// <summary>
        /// Initalizes an exception in the command object for an invalid message number with a message and inner exception.
        /// </summary>
        /// <param name="message">A string containing some text to describe the error.</param>
        /// <param name="inner">An exception that this exception is includes.</param>
        public ImapCommandInvalidMessageNumber(string message, Exception inner) : base(message, inner) { }
    }
    #endregion
}