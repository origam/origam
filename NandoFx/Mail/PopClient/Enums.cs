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

	/// <summary>Possible responses received from the server when performing
	/// authentication</summary>
	public enum  AuthenticationResponses  {
		SUCCESS               = 0,
		INVALIDUSER           = 1,
		INVALIDPASSWORD       = 2,
		INVALIDUSERORPASSWORD = 3
	}		

	/// <summary>Authentication method to use</summary>
	/// <remarks>TRYBOTH means code will first attempt to use the APOP method
	/// as it is more secure. In case of failure we fall back to the USERPASS
	/// method.</remarks>
	public enum  AuthenticationMethods  {
		USERPASS = 0,
		APOP     = 1,
		TRYBOTH  = 2
	}

}
