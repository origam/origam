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

using ApplicationException  = System.ApplicationException;
using ArgumentNullException = System.ArgumentNullException;
using SerializableAttribute = System.SerializableAttribute;

namespace NandoF.Data
{
	[Serializable] public class MailSmtpAccount : ServerAccount
	{
		protected NamedEmail sender;
		public    NamedEmail Sender  {
			get  { return sender; }
			set  {
				if (value==null)     throw new ArgumentNullException("Sender");
				if (!value.IsValid)  throw new ApplicationException
					("Invalid e-mail address for sender");
				sender = value;
			}
		}
		
		protected NamedEmail replyTo;
		public    NamedEmail ReplyTo  {
			get  { return replyTo;  }
			set  { replyTo = value; }
		}
		
		// Constructor
		public MailSmtpAccount()  {
			this.Port = 25;
		}
	}

}
