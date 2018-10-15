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
	using System.Security.Cryptography;
	using System.Text;
	
	public class MyMD5
	{
		public static string GetMD5Hash(string input)  {
			MD5    md5  = new MD5CryptoServiceProvider();
			byte[] res  = md5.ComputeHash(Encoding.Default.GetBytes(input),
			                              0, input.Length);
			char[] temp = new char[res.Length];
			//copy to a char array which can be passed to a string constructor
			System.Array.Copy(res, temp, res.Length);
			//return the result as a string
			return new string(temp);
		}
		
		public static string GetMD5HashHex(string input) {
			MD5 md5 = new MD5CryptoServiceProvider();
//			DES des = new DESCryptoServiceProvider();
			byte[] res = md5.ComputeHash(Encoding.Default.GetBytes(input),
			                             0, input.Length);
			string returnThis = "";
			for(int i=0; i < res.Length; i++)  {
				returnThis += System.Uri.HexEscape((char)res[i]);
			}
			returnThis = returnThis.Replace("%","");
			returnThis = returnThis.ToLower();
			return returnThis;
		}
	}
}
