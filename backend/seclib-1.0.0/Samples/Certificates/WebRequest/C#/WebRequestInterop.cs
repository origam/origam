/*
 *     Copyright © 2002 [/4], The KPD-Team
 *     All rights reserved.
 *     http://www.mentalis.org/
 *
 *   Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions
 *   are met:
 *
 *     - Redistributions of source code must retain the above copyright
 *        notice, this list of conditions and the following disclaimer. 
 *
 *     - Neither the name of the KPD-Team, nor the names of its contributors
 *        may be used to endorse or promote products derived from this
 *        software without specific prior written permission. 
 *
 *   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 *   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 *   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 *   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 *   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 *   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 *   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 *   OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This example project shows how you can use the Security Library together with
/// classes from the .NET base class library. In this example, we will use the Security
/// Library to open a PKCS#12 encoded certificate file, and we will pass the certificate
/// to an instance of the HttpWebRequest class.
/// </summary>
public class WebRequestInterop {
	static void Main(string[] args) {
		Console.WriteLine("This example project shows how you can use the Security Library together with classes from the .NET base class library. In this example, we will use the Security Library to open a PKCS#12 encoded certificate file, and we will pass the certificate to an instance of the HttpWebRequest class.\r\nThe HttpWebRequest will get the index page of www.mcirosoft.com.\r\n");
		//Load the PFX certificate
		Console.WriteLine("Please enter the path to the PFX file you wish to load:");
		string file = Console.ReadLine();
		if (!File.Exists(file)) {
			Console.WriteLine("The specified file does not exist.");
			return;
		}
		Console.WriteLine("Please enter the password of the PFX file:");
		string pass = Console.ReadLine();
		Certificate cert = Certificate.CreateFromPfxFile(file, pass, true);
		//Converts the Certificate instance to an ordinary X509Certificate instance
		X509Certificate cert2 = cert.ToX509();
		// create the HttpWebRequest as usual
		// and pass the X509Certificate instance to it
		HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.microsoft.com/");
		WebClient c = new WebClient();
		req.ClientCertificates.Add(cert2);
		WebResponse result = req.GetResponse();
		Stream strm = result.GetResponseStream();
		byte[] buffer = new byte[1024];
		int len = 0;
		do {
			try {
				len = strm.Read(buffer, 0, 1024);
				Console.Write(System.Text.Encoding.ASCII.GetString(buffer, 0, len));
			} catch {}
		} while(len > 0);
		strm.Close();
		Console.WriteLine("\r\n\r\nPress ENTER to continue...");
		Console.ReadLine();
	}
}