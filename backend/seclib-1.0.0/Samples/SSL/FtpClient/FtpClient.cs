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
using System.Net.Sockets;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;

class FtpClient {
	[STAThread]
	static void Main(string[] args) {
		Console.WriteLine("This test class shows how to log on to an FTP server over a secure connection.\r\n");
		FtpClient c = new FtpClient();
		try {
			c.Start();
		} catch (Exception e) {
			Console.WriteLine(e);
		}
		Console.WriteLine("\nPress ENTER to continue...");
		Console.ReadLine();
	}
	public void Start() {
		Console.WriteLine("Please enter the URL of the document you wish to download: [only ftp://]");
		Console.WriteLine("   [for instance:  ftp://anonymous:pass@ftp.ipswitch.com:21/ ]");
		string ret = Console.ReadLine();
		Url url;
		int choice;
		// get the url
		try {
			url = new Url(ret);
		} catch {
			Console.WriteLine("Invalid URL.");
			return;
		}
		if (!url.Protocol.ToLower().Equals("ftp")) {
			Console.WriteLine("Protocol invalid; only the FTP protocol is allowed.");
			return;
		}
		if (url.Username == "")
			url.Username = "anonymous";
		if (url.Password == "")
			url.Password = "empty";
		// get the secure protocol
		Console.WriteLine("Please enter the connection method you wish to use:\r\n  [1] Normal unsecure connection\r\n  [2] SSL connection using the AUTH command");
		Console.Write("Your choice: ");
		ret = Console.ReadLine();
		try {
			choice = int.Parse(ret);
		} catch {
			Console.WriteLine("Not a number.");
			return;
		}
		if (choice < 1 || choice > 2) {
			Console.WriteLine("Invalid input.");
			return;
		}
		// get the webpage
		DownloadFile(url, choice);
	}
	private void DownloadFile(Url url, int choice) {
		SecurityOptions options = new SecurityOptions(SecureProtocol.None);;
		m_Socket = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
		// connect to the FTP server using a normal TCP connection
		m_Socket.Connect(new IPEndPoint(Dns.Resolve(url.Host).AddressList[0], url.Port));
		// wait for the server hello
		ReceiveReply();
		// if the user selected to use the AUTH command..
		if (choice == 2) {
			// ..send the command to the server and start the SSL handshake
			DoAuthCommand(url.Host);
		}
		// log on and quit
		if (!SendCommand("USER " + url.Username))
			return;
		if (!SendCommand("PASS " + url.Password))
			return;
		if (!SendCommand("QUIT"))
			return;
		// clean up
		m_Socket.Shutdown(SocketShutdown.Both);
		m_Socket.Close();
	}
	private void DoAuthCommand(string cn) {
		// send the AUTH command
		if (!SendCommand("AUTH TLS")) {
			Console.WriteLine("The server does not support SSL/TLS authentication -- exiting.");
			return;
		}
		// if the server accepted our command, start the SSL/TLs connection
		SecurityOptions options = new SecurityOptions(SecureProtocol.Ssl3 | SecureProtocol.Tls1);
		options.AllowedAlgorithms = SslAlgorithms.SECURE_CIPHERS;
		options.CommonName = cn;
		options.VerificationType = CredentialVerification.Manual;
		options.Verifier = new CertVerifyEventHandler(this.OnVerify);
		m_Socket.ChangeSecurityProtocol(options);
	}
	public void OnVerify(SecureSocket socket, Certificate remote, CertificateChain chain, VerifyEventArgs e) {
		Console.WriteLine("\r\nThe certificate of the FTP server:");
		Console.WriteLine(remote.ToString(true) + "\r\n");
		// certificate chain verification can be placed here
	}
	private bool SendCommand(string command) {
		byte[] buffer = Encoding.ASCII.GetBytes(command + "\r\n");
		m_Socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
		Console.WriteLine(command);
		return ReceiveReply();;
	}
	private bool ReceiveReply() {
		byte[] buffer = new byte[1024];
		string ret = "";
		int r = m_Socket.Receive(buffer);
		while(r > 0) {
			ret += Encoding.ASCII.GetString(buffer, 0, r);
			if (IsValidReply(ret))
				break;
			r = m_Socket.Receive(buffer);
		}
		Console.Write(ret);
		if (int.Parse(ret.Substring(0, 1)) < 4)
			return true;
		else
			return false;
	}
	private bool IsValidReply(string input) {
		string [] lines = input.Split('\n');
		if (lines.Length > 1) {
			try {
				if (lines[lines.Length - 2].Replace("\r", "").Substring(3, 1).Equals(" "))
					return true;
			} catch {
				return false;
			}
		}
		return false;
	}
	private SecureSocket m_Socket;
}