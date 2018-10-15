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
	public abstract class AccountBase
	{
		private  string user     = string.Empty;
		public   string User  {
			get  { return user;  }
			set  {
				if (value==null)  throw new ArgumentNullException("user");
				user = value;
			}
		}
		
		private  string password = string.Empty;
		public   string Password  {
			get  { return password;  }
			set  {
				if (value==null)  throw new ArgumentNullException("password");
				password = value;
			}
		}
		
		override public string ToString()  { return User; }
	}
	
	
	public abstract class ServerAccount : AccountBase
	{
		private  string host     = string.Empty;
		public   string Host  {
			get  { return host;  }
			set  {
				if (value==null)  throw new ArgumentNullException("host");
				host = value;
			}
		}
		
		private  int    port;
		public   int    Port  {
			get  { return port;  }
			set  {
				if (value < 1)
					throw new ApplicationException("Port should be a positive integer.");
				else  port = value;
			}
		}
		
		override public string ToString()  {
			if (Text.IsNullOrEmpty(User))  return Host;
			if (User.IndexOf('@') != -1)   return User;
			return User + "@" + Host;
		}
	}
}
