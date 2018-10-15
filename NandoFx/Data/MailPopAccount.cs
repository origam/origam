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

namespace NandoF.Data
{
	public class MailPopAccount : ServerAccount
	{
		// Constructors
		public MailPopAccount()  {
			this.Port = 110;
		}
		public MailPopAccount(string host, string user, string password)
			: this() {
			Host     = host;
			User     = user;
			Password = password;
		}
		
	}
	
}
