/*
 *   Mentalis.org Security Library -- Test Application
 * 
 *     Copyright © 2002-2003, The KPD-Team
 *     All rights reserved.
 *     http://www.mentalis.org/
 *
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
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This test class implements a small HTTP client that supports HTTP and HTTPS.
/// </summary>
class WebClient {
	/// <summary>
	/// Starts a new client session.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	static void Main(string[] args) {
		Console.WriteLine("This test class implements a small HTTP client that supports HTTP and HTTPS.\r\n");
		WebClient c = new WebClient();
		try {
			c.Start();
		} catch (Exception e) {
			Console.WriteLine(e);
		}
		Console.WriteLine("\nPress ENTER to continue...");
		Console.ReadLine();
	}
	/// <summary>
	/// Starts a new client session.
	/// </summary>
	public void Start() {
		Console.WriteLine("Please enter the URL of the document you wish to download: [only HTTP/HTTPS]");
		string ret = Console.ReadLine();
		Url url;
		SecureProtocol sp;
		// get the url
		try {
			url = new Url(ret);
		} catch {
			Console.WriteLine("Invalid URL.");
			return;
		}
		if (!url.Protocol.ToLower().Equals("http") && !url.Protocol.ToLower().Equals("https")) {
			Console.WriteLine("Protocol invalid; only the HTTP and HTTPS protocols are allowed.");
			return;
		}
		// get the secure protocol
		if (url.Protocol.ToLower().Equals("https")) {
			Console.WriteLine("Please enter the name of the secure protocol you wish to use:\r\n  [1] SSL and TLS\r\n  [2] SSL only\r\n  [3] TLS only");
			Console.Write("Your choice: ");
			ret = Console.ReadLine();
			switch(int.Parse(ret)) {
				case 1:
					sp = SecureProtocol.Tls1 | SecureProtocol.Ssl3;
					break;
				case 2:
					sp = SecureProtocol.Ssl3;
					break;
				case 3:
					sp = SecureProtocol.Tls1;
					break;
				default:
					Console.WriteLine("Secure protocol name not understood; you should enter either 1, 2 or 3.");
					return;
			}
		} else {
			sp = SecureProtocol.None;
		}
		// get the webpage
		DownloadFile(url, sp);
	}
	/// <summary>
	/// Converts a Url object into a HTTP request string.
	/// </summary>
	/// <param name="url">The Url to request.</param>
	/// <returns>The HTTP query string.</returns>
	protected string GetHttpRequest(Url url) {
		string request = "GET " + (url.Path.Length == 0 ? "/" : url.Path) + (url.Query.Length == 0 ? "" : ("?" + url.Query)) + " HTTP/1.0\r\n";
		request += "Accept: */*\r\nUser-Agent: Mentalis.org SecureSocket\r\nHost: " + url.Host + "\r\n";
		if (url.Username.Length != 0)
			request += "Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(url.Username + ":" + url.Password)) + "\r\n";
		return request + "\r\n";
	}
	/// <summary>
	/// Download a file using the synchronous Socket methods.
	/// </summary>
	/// <param name="url">The URL to download.
	/// </param>
	/// <param name="sp">The protocol to use.</param>
	protected void DownloadFile(Url url, SecureProtocol sp) {
		string request = GetHttpRequest(url); // holds the HTTP request for the given URL
		SecureSocket s;
		try {
			// First we create a new SecurityOptions instance
			// SecurityOptions objects hold information about the security
			// protocols the SecureSocket should use and how the SecureSocket
			// should react in certain situations.
			SecurityOptions options = new SecurityOptions(sp);
			// The Certificate field holds a certificate associated with
			// a client [you, for instance]. Because client certificates
			// are not often used for HTTP over SSL or TLS, we'll simply
			// set this field to a null reference.
			options.Certificate = null;
			// The Entity specifies whether the SecureSocket should act
			// as a client socket or as a server socket. In this case,
			// it should act as a client socket.
			options.Entity = ConnectionEnd.Client;
			// The CommonName field specifies the name of the remote
			// party we're connecting to. This is usually the domain name
			// of the remote host.
			options.CommonName = url.Host;
			// The VerificationType field specifies how we intend to verify
			// the certificate. In this case, we tell the SecureSocket that
			// we will manually verify the certificate. Look in the documentation
			// for other options.
			options.VerificationType = CredentialVerification.Manual;
			// When specifying the CredentialVerification.Manual option, we
			// must also specify a CertVerifyEventHandler delegate that will
			// be called when the SecureSocket receives the remote certificate.
			// The Verifier field holds this delegate. If no manual verification
			// is done, this field can be set to a null reference.
			options.Verifier = new CertVerifyEventHandler(OnVerify);
			// The Flags field specifies which flags should be used for the
			// connection. In this case, we will simply use the default behavior.
			options.Flags = SecurityFlags.Default;
			// Allow only secure ciphers to be used. If the server only supports
			// weak encryption, the connections will be shut down.
			options.AllowedAlgorithms = SslAlgorithms.SECURE_CIPHERS;
			// create a new SecureSocket instance and initialize it with
			// the security options we specified above.
			s = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
			// connect to the remote host
			s.Connect(new IPEndPoint(Dns.Resolve(url.Host).AddressList[0], url.Port));
		} catch (Exception e) {
			Console.WriteLine("Exception occurred while connecting: " + e.ToString());
			return;
		}
		// send the HTTP request to the remote host
		Console.Write("HTTP Query string:\r\n------------------\r\n" + request);
		try {
			byte[] reqBytes = Encoding.ASCII.GetBytes(request);
			int sent = s.Send(reqBytes, 0, reqBytes.Length, SocketFlags.None);
			while(sent != reqBytes.Length) {
				sent += s.Send(reqBytes, sent, reqBytes.Length - sent, SocketFlags.None);
			}
		} catch (Exception e) {
			Console.WriteLine("Exception occurred while sending: " + e.ToString());
			return;
		}
		// receive the reply
		Console.WriteLine("HTTP Server reply:\r\n------------------");
		try {
			byte[] buffer = new byte[4096];
			int ret = s.Receive(buffer);
			while(ret != 0) {
				Console.Write(Encoding.ASCII.GetString(buffer, 0, ret));
				ret = s.Receive(buffer);
			}
		} catch (Exception e) {
			Console.WriteLine("Exception occurred while receiving: " + e.ToString());
			return;
		}
		try {
			s.Shutdown(SocketShutdown.Both); // shut down the TCP connection
		} catch (Exception e) {
			Console.WriteLine("Exception occurred while shutting the connection down: " + e.ToString());
			return;
		}
		s.Close();
	}
	/// <summary>
	/// Verifies a certificate received from the remote host.
	/// </summary>
	/// <param name="socket">The SecureSocket that received the certificate.</param>
	/// <param name="remote">The received certificate.</param>
	/// <param name="e">The event parameters.</param>
	protected void OnVerify(SecureSocket socket, Certificate remote, CertificateChain chain, VerifyEventArgs e) {
		CertificateChain cc = new CertificateChain(remote);
		Console.WriteLine("\r\nServer Certificate:\r\n-------------------");
		Console.WriteLine(remote.ToString(true));
		Console.Write("\r\nServer Certificate Verification:\r\n--------------------------------\r\n    -> ");
		Console.WriteLine(cc.VerifyChain(socket.CommonName, AuthType.Server).ToString() + "\r\n");
	}
}