'
'     Copyright © 2002 [/4], The KPD-Team
'     All rights reserved.
'     http://www.mentalis.org/
'
'   Redistribution and use in source and binary forms, with or without
'   modification, are permitted provided that the following conditions
'   are met:
'
'     - Redistributions of source code must retain the above copyright
'        notice, this list of conditions and the following disclaimer. 
'
'     - Neither the name of the KPD-Team, nor the names of its contributors
'        may be used to endorse or promote products derived from this
'        software without specific prior written permission. 
'
'   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
'   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
'   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
'   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
'   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
'   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
'   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
'   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
'   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
'   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
'   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
'   OF THE POSSIBILITY OF SUCH DAMAGE.
'

imports System
imports System.IO
imports System.Net
imports System.Security.Cryptography.X509Certificates
imports Org.Mentalis.Security.Certificates
imports Microsoft.VisualBasic

' <summary>
' This example project shows how you can use the Security Library together with
' classes from the .NET base class library. In this example, we will use the Security
' Library to open a PKCS#12 encoded certificate file, and we will pass the certificate
' to an instance of the HttpWebRequest class.
' </summary>
public class WebRequestInterop
	public shared sub Main(args as string())
		Console.WriteLine("This example project shows how you can use the Security Library together with classes from the .NET base class library. In this example, we will use the Security Library to open a PKCS#12 encoded certificate file, and we will pass the certificate to an instance of the HttpWebRequest class." + ControlChars.CrLf + "The HttpWebRequest will get the index page of www.mcirosoft.com." + ControlChars.CrLf)
		' Load the PFX certificate
		Console.WriteLine("Please enter the path to the PFX file you wish to load:")
		dim filename as string = Console.ReadLine()
		if (not File.Exists(filename))
			Console.WriteLine("The specified file does not exist.")
			return
		end if
		Console.WriteLine("Please enter the password of the PFX file:")
		dim pass as string = Console.ReadLine()
		dim cert as Certificate = Certificate.CreateFromPfxFile(filename, pass, true)
		' Converts the Certificate instance to an ordinary X509Certificate instance
		dim cert2 as X509Certificate = cert.ToX509()
		' create the HttpWebRequest as usual
		' and pass the X509Certificate instance to it
		dim req as HttpWebRequest = CType(WebRequest.Create("https://www.microsoft.com/"), HttpWebRequest)
		dim c as WebClient = new WebClient()
		req.ClientCertificates.Add(cert2)
		dim result as WebResponse = req.GetResponse()
		dim strm as Stream = result.GetResponseStream()
		dim buffer(1024) as byte
		dim len as integer = 0
		do
			try 
				len = strm.Read(buffer, 0, buffer.Length)
				Console.Write(System.Text.Encoding.ASCII.GetString(buffer, 0, len))
			catch
            end try
		loop until len <= 0
		strm.Close()
		Console.WriteLine(ControlChars.CrLf + ControlChars.CrLf + "Press ENTER to continue...")
		Console.ReadLine()
	end sub
end class