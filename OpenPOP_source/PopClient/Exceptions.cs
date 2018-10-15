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

namespace NandoF.Mail.PopClient
{
	
	using ApplicationException = System.ApplicationException;
	using Exception            = System.Exception;
	
	/// <summary>Thrown when the specified POP3 server cannot be found or
	/// connected to.</summary>	
	public class PopServerNotFoundException : ApplicationException
	{
		// Constructors
		public PopServerNotFoundException()  {}
		public PopServerNotFoundException(string msg) : base(msg)  {}
		public PopServerNotFoundException(string msg, Exception inner) :
			base(msg, inner)  {}
	}
	
	/// <summary>Thrown when the POP3 Server sends an error (-ERR) during
	/// initial handshake (HELO).</summary>
	public class PopServerNotAvailableException : ApplicationException
	{
		// Constructors
		public PopServerNotAvailableException()  {}
		public PopServerNotAvailableException(string msg) : base(msg)  {}
		public PopServerNotAvailableException(string msg, Exception inner) :
			base(msg, inner)  {}
	}
	
	/// <summary>Thrown when the attachment is not in a supported format.</summary>
	/// <remarks>Supported attachment encodings are Base64 and Quoted-Printable
	/// </remarks>
	public class AttachmentEncodingNotSupportedException : ApplicationException
	{
		// Constructors
		public AttachmentEncodingNotSupportedException()  {}
		public AttachmentEncodingNotSupportedException(string msg) : base(msg)  {}
		public AttachmentEncodingNotSupportedException(string msg, Exception inner)
			: base(msg, inner)  {}
	}
	
	
	/// <summary>Thrown when the supplied login doesn't exist on the server</summary>
	/// <remarks>Should only be used with the USER/PASS
	/// authentication method.</remarks>
	public class InvalidLoginException : ApplicationException
	{
		// Constructors
		public InvalidLoginException()  {}
		public InvalidLoginException(string msg) : base(msg)  {}
		public InvalidLoginException(string msg, Exception inner) :
			base(msg, inner)  {}
	}
	

	/// <summary>Thrown when the password supplied for the login is invalid
	/// </summary>
	/// <remarks>Should only be used with the USER/PASS authentication method.
	/// </remarks>
	public class InvalidPasswordException : ApplicationException
	{
		// Constructors
		public InvalidPasswordException()  {}
		public InvalidPasswordException(string msg) : base(msg)  {}
		public InvalidPasswordException(string msg, Exception inner) :
			base(msg, inner)  {}
	}
	

	/// <summary>Thrown when either the login or the password is invalid.</summary>
	/// /// <remarks>Should only be used with the APOP authentication method.</remarks>
	public class InvalidLoginOrPasswordException : ApplicationException
	{
		// Constructors
		public InvalidLoginOrPasswordException()  {}
		public InvalidLoginOrPasswordException(string msg) : base(msg)  {}
		public InvalidLoginOrPasswordException(string msg, Exception inner) :
			base(msg, inner)  {}
	}
	

	/// <summary>Thrown when the user mailbox is in a locked state.</summary>
	/// <remarks>The mail boxes are locked when an existing session is open
	/// on the mail server. This can occur in case of aborted sessions.</remarks>
	public class PopServerLockException : ApplicationException
	{
		// Constructors
		public PopServerLockException()  {}
		public PopServerLockException(string msg) : base(msg)  {}
		public PopServerLockException(string msg, Exception inner) :
			base(msg, inner)  {}
	}
	
}

