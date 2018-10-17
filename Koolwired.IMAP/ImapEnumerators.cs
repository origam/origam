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
 * Date       Programmer      		Description
 * 09/16/06   Keith Kikta     		Inital release.
 * 07/23/07	  Scott A. Braithwaite	Added NONE enum to LoginType
 * 09/27/07   Keith Kikta           Added LOGIN enum to LoginType
 *--------------------------------------------------------------------------*/
#endregion

#region Refrences

#endregion

namespace Koolwired.Imap
{
    #region public enumerators
    /// <summary>
    /// Contains a list of possible values for the connection state of the current instance.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Indicates the connection is being established.
        /// </summary>
        Connecting, 
        /// <summary>
        /// Indicates the connection has been established.
        /// </summary>
        Connected, 
        /// <summary>
        /// Indicates that the connection is attempting to authenticate.
        /// </summary>
        Authenticating, 
        /// <summary>
        /// Indicates the connection is open and has been authenticated.
        /// </summary>
        Open, 
        /// <summary>
        /// Indicates that something in the connection has failed.
        /// </summary>
        Broken, 
        /// <summary>
        /// Indicates the connection has been closed.
        /// </summary>
        Closed
    }
    /// <summary>
    /// Contains a list of possible values for the login type on the specified server.
    /// </summary>
    public enum LoginType
    {
    	/// <summary>
    	/// NONE.  Indicates that the LoginType has yet to be set
    	/// </summary>
    	NONE = 0,
    	/// <summary>
        /// Plain Text Authentication.
        /// </summary>
        PLAIN,
        /// <summary>
        /// Login Authentication (Outlook authentication similar to SASL)
        /// </summary>
        LOGIN,
        /// <summary>
        /// CRAM-MD5 Authentication.
        /// </summary>
        CRAM_MD5
        
    }
    /// <summary>
    /// Type of encoding used in message part
    /// </summary>
    public enum BodyPartEncoding
    {
        /// <summary>
        /// 7BIT Encoding
        /// </summary>
        UTF7,
        /// <summary>
        /// 8BIT Encoding (no encoding)
        /// </summary>
        UTF8,
        /// <summary>
        /// Base64 Encoding
        /// </summary>
        BASE64,
        /// <summary>
        /// Quoted Printable (ASCII)
        /// </summary>
        QUOTEDPRINTABLE,
        /// <summary>
        /// Unknown Encoding
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// Binary Encoding
        /// </summary>
        NONE,
        EIGHT_BIT
    }
    #endregion
}
