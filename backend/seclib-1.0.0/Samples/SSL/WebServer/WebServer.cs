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
using System.Text;
using System.Net.Sockets;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This test class implements a small HTTP server that supports HTTP and HTTPS. It always returns the same static page.
/// </summary>
class WebServer {
	/// <summary>
	/// Starts a new server session.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	static void Main(string[] args) {
		Console.WriteLine("This test class implements a small HTTP server that supports HTTP and HTTPS. It always returns the same static page.\r\n");
		WebServer s = new WebServer();
		s.Start();
		Console.WriteLine("\r\nPress enter to exit...");
		Console.ReadLine();
	}
	/// <summary>
	/// Starts a new server session.
	/// </summary>
	public void Start() {
		Console.WriteLine("Please enter the IP address of the network adapter to listen on [use 0.0.0.0 to listen on all installed network adapters]:");
		IPAddress bindip;
		try {
			bindip = IPAddress.Parse(Console.ReadLine());
		} catch {
			Console.WriteLine("Invalid IP address!");
			return;
		}
		Console.WriteLine("Please enter the port to listen on [or 0 to automatically select one]:");
		int bindport;
		try {
			bindport = int.Parse(Console.ReadLine());
			if (bindport < IPEndPoint.MinPort || bindport > IPEndPoint.MaxPort)
				throw new Exception();
		} catch {
			Console.WriteLine("Invalid port!");
			return;
		}
		Console.WriteLine("Please enter the security protocol you wish to use for incoming connections:\r\n  [1] None\r\n  [2] SSL3\r\n  [3] TLS1\r\n  [4] SSL3 and TLS1");
		SecureProtocol sp;
		switch(int.Parse(Console.ReadLine())) {
			case 1:
				sp = SecureProtocol.None;
				break;
			case 2:
				sp = SecureProtocol.Ssl3;
				break;
			case 3:
				sp = SecureProtocol.Tls1;
				break;
			case 4:
				sp = SecureProtocol.Ssl3 | SecureProtocol.Tls1;
				break;
			default:
				Console.WriteLine("Invalid protocol!");
				return;
		}
		Certificate cert = null;
		if (sp != SecureProtocol.None) {
			cert = GetCertificate();
			if (cert == null) // something went wrong
				return;
			Console.WriteLine("Using the following certificate:\r\n" + cert.ToString(true));
		}
		Console.WriteLine("Press CTRL-BREAK to kill the server.");
		StartServer(new IPEndPoint(bindip, bindport), sp, cert);
	}
	/// <summary>
	/// Asks the user to specify a server certificate.
	/// </summary>
	/// <returns>A server certificate.</returns>
	private Certificate GetCertificate() {
		Console.WriteLine("How do you want to load the certificate?\r\n  1 = from certificate store\r\n  2 = from PFX/P12 file\r\n  3 = from .cer file");
		string choice = Console.ReadLine();
		try {
			switch(choice.Trim()) {
				case "1":
					return GetStoreCert();
				case "2":
					return GetFileCert(true);
				case "3":
					return GetFileCert(false);
				default:
					Console.WriteLine("Invalid option selected");
					return null;
			}
		} catch {
			Console.WriteLine("An error occurred while opening the certificate.");
			return null;
		}
	}
	/// <summary>
	/// Asks the user to specify a server certificate, stored in a certificate store.
	/// </summary>
	/// <returns>A server certificate.</returns>
	private Certificate GetStoreCert() {
		try {
			Console.WriteLine("Enter the name of the store (for instance \"MY\", without the quotes):");
			// open the certificate store specified by the user
			CertificateStore cs = new CertificateStore(Console.ReadLine());
			// find a certificate that is suited for server authentication
			Certificate ret = cs.FindCertificateByUsage(new string[] {"1.3.6.1.5.5.7.3.1"});
			if (ret == null)
				Console.WriteLine("The certificate file does not contain a server authentication certificate.");
			return ret;
		} catch {
			Console.WriteLine("An error occurs while opening the specified store.");
			return null;
		}
	}
	/// <summary>
	/// Asks the user to specify a server certificate, stored on the hard disk.
	/// </summary>
	/// <param name="pfxFile"><b>true</b> if the file is a PFX/P12 file, <b>false</b> if it is a DER encoded certificate.</param>
	/// <returns>A server certificate.</returns>
	private Certificate GetFileCert(bool pfxFile) {
		string file = null, pass = null;
		Console.WriteLine("Enter the full path of the certificate file:");
		file = Console.ReadLine();
		if (!File.Exists(file)) {
			Console.WriteLine("The specified file could not be found!");
			return null;
		}
		// open the certificate specified by the user
		CertificateStore cs;
		if (pfxFile) {
			Console.WriteLine("Enter the password of the PFX/P12 file:");
			pass = Console.ReadLine();
			cs = CertificateStore.CreateFromPfxFile(file, pass);
		} else {
			cs = CertificateStore.CreateFromCerFile(file);
		}
		// find a certificate that is suited for server authentication
		Certificate ret = cs.FindCertificateByUsage( new string[] {"1.3.6.1.5.5.7.3.1"});
		if (ret == null)
			Console.WriteLine("The certificate file does not contain a server authentication certificate.");
		// make sure the certificate is associated with a private key
		if (!ret.HasPrivateKey()) {
			Console.WriteLine("The certificate is not associated with a private key. Please enter the path to a PVK file you want to associate it with:");
			file = Console.ReadLine();
			if (!File.Exists(file)) {
				Console.WriteLine("The specified file could not be found!");
				return null;
			}
			// try to associate the certificate with the specified PVK file
			// first try opening the PVK file without a password
			try {
				ret.AssociateWithPrivateKey(file, null, true);
			} catch {
				// AssociateWithPrivateKey failed; probably because the certificate is password protected
				Console.WriteLine("The PVK file appears to be password protected. Please enter the password:");
				pass = Console.ReadLine();
				try {
					ret.AssociateWithPrivateKey(file, pass, true);
				} catch {
					Console.WriteLine("The PVK file could not be read.");
					return null;
				}
			}
		}
		return ret;
	}
	/// <summary>
	/// Starts listening for incoming server connections.
	/// </summary>
	/// <param name="ep">The EndPoint on which to listen.</param>
	/// <param name="sp">The protocol to use.</param>
	/// <param name="pfxfile">An optional PFX file.</param>
	/// <param name="password">An optional PFX password.</param>
	public void StartServer(IPEndPoint ep, SecureProtocol sp, Certificate cert) {
		// initialize a SecurityOptions instance
		SecurityOptions options = new SecurityOptions(sp, cert, ConnectionEnd.Server);
		// create a new SecureSocket with the above security options
		SecureSocket s = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
		// from here on, act as if the SecureSocket is a normal Socket
		s.Bind(ep);
		s.Listen(10);
		Console.WriteLine("Listening on " + s.LocalEndPoint.ToString());
		SecureSocket ss;
		string query = "";
		byte[] buffer = new byte[1024];
		int ret;
		while(true) {
			ss = (SecureSocket)s.Accept();
			Console.WriteLine("Incoming socket accepted.");
			// receive HTTP query
			Console.WriteLine("Receiving HTTP request...");
			ret = 0;
			query = "";
			while(!IsComplete(query)) { // wait until we've received the entire HTTP query
				try {
					ret = ss.Receive(buffer, 0, buffer.Length, SocketFlags.None);
				} catch (Exception e) {
					Console.WriteLine("Error while receiving data from client [" + e.Message + "].");
					Console.WriteLine(e);
					break;
				}
				if (ret == 0) {
					Console.WriteLine("Client closed connection too soon.");
					ss.Close();
					break;
				}
				query += Encoding.ASCII.GetString(buffer, 0, ret);
			}
			if (IsComplete(query)) {
				// Send HTTP reply
				Console.WriteLine("Sending reply...");
				ret = 0;
				try {
					while(ret != MentalisPage.Length) {
						ret += ss.Send(Encoding.ASCII.GetBytes(MentalisPage), ret, MentalisPage.Length - ret, SocketFlags.None);
					}
					ss.Shutdown(SocketShutdown.Both);
					ss.Close();
				} catch (Exception e) {
					Console.WriteLine("Error while sending data to the client [" + e.Message + "].");
					Console.WriteLine(e);
				}
			}
			Console.WriteLine("Waiting for another connection...");
		}
	}
	/// <summary>
	/// Returns <b>true</b> if the HTTP request is complete, <b>false</b> otherwise.
	/// </summary>
	/// <param name="query">The query to check.</param>
	/// <returns>A boolean value.</returns>
	private bool IsComplete(string query) {
		return query.IndexOf("\r\n\r\n") >= 0;
	}
	/// <summary>
	/// The static page to return when a client connects to our server.
	/// </summary>
	private readonly string MentalisPage = "HTTP/1.1 200 OK\r\nServer: Mentalis.org SecureSocket class\r\nContent-Type: text/html\r\n\r\n<html><head><title>Mentalis.org test page</title></head><body><p><font face=\"Verdana\" size=\"2\">This page has been sent from a <a href=\"http://www.mentalis.org/soft/projects/ssocket/\">SecureSocket</a> test server. Visit <a href=\"http://www.mentalis.org/\">Mentalis.org</a> for more information.</font></p></body></html>";
}
