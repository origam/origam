using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This example program show how to send email via an SMTP server. Apart from the normal
/// unencrypted SMTP connections, it also supports TLS encrypted connections.
/// </summary>
class SmtpClient {
	static void Main(string[] args) {
		Console.WriteLine("This example program show how to send email via an SMTP server. Apart from the normal unencrypted SMTP connections, it also supports TLS encrypted connections.\r\n");
		SmtpClient s = new SmtpClient();
		s.Start();
		Console.WriteLine("\nPress ENTER to continue...");
		Console.ReadLine();
	}
	public void Start() {
		try {
			// ask the user for SMTP server, port and the type of connection
			Console.Write("Host to connect to: ");
			string server = Console.ReadLine().Trim();
			Console.Write("Port: ");
			string port = Console.ReadLine().Trim();
			Console.Write("Connection type [0 = normal connection, 1 = direct TLS connection, 2 = indirect TLS connection (using STARTTLS command)]: ");
			string type = Console.ReadLine().Trim();
			if (Array.IndexOf(new string[]{"0", "1", "2"}, type) == -1) {
				Console.WriteLine("Invalid connection type.");
				return;
			}
			Console.WriteLine("Please enter the email address you wish to send an email to:");
			string address = Console.ReadLine().Trim();
			// create a new SecurityOptions instance
			SecurityOptions options = new SecurityOptions(SecureProtocol.None);
			// allow only secure ciphers to be used. Currently, the seure ciphers are:
			// the AES algorithm [128 or 256 bit keys], the RC4 algorithm [128 bit keys]
			// or TipleDES [168 bit keys]
			options.AllowedAlgorithms = SslAlgorithms.SECURE_CIPHERS;
			// the SecureSocket should be a client socket
			options.Entity = ConnectionEnd.Client;
			// we will use manual verification of the server certificate
			options.VerificationType = CredentialVerification.Manual;
			// when the server certificate is receives, the SecureSocket should call
			// the OnVerify method
			options.Verifier = new CertVerifyEventHandler(OnVerify);
			// use the default flags
			options.Flags = SecurityFlags.Default;
			// the common name is the domain name or IP address of the server
			options.CommonName = server;
			// if the user chose a direct TLS connection, set the protocol to TLS1
			if (type == "1") {
				options.Protocol = SecureProtocol.Tls1;
			}
			// create the new secure socket
			Connection = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
			// connect to the SMTP server
			Connection.Connect(new IPEndPoint(Dns.Resolve(server).AddressList[0], int.Parse(port)));
			// wait for the server hello message
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("Server disallowed connection.");
				return;
			}
			// if the user selected an indirect TLS connection, issue
			// a EHLO command. Otherwise, stick to the standard HELO command.
			if (type == "2") // STARTTLS connection
				Send("EHLO SmtpClient.Mentalis.org\r\n");
			else
				Send("HELO SmtpClient.Mentalis.org\r\n");
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("HELLO failed.");
				return;
			}
			// if the user selected an indirect TLS connection, issue
			// the STARTTLS command
			if (type == "2") {
				// send the STARTTLS command to the server
				Send("STARTTLS\r\n");
				// make sure the server supports the STARTTLS command
				if (!IsReplyOK(Receive())) {
					Console.WriteLine("STARTTLS failed.");
					return;
				}
				// change the protocol from SecureProtocol.None
				// to SecureProtocol.Tls1
				options.Protocol = SecureProtocol.Tls1;
				// start the TLS handshake
				Connection.ChangeSecurityProtocol(options);
				// after this line, we're using an encrypted TLS connection
			}
			// from here on, act as if the SecureSocket is a normal Socket
			Send("MAIL FROM: secure.socket@mentalis.org\r\n"); // secure.socket@mentalis.org is not a real email address
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("MAIL FROM failed.");
				return;
			}
			Send("RCPT TO:" + address + "\r\n");
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("RCPT TO failed.");
				return;
			}
			Send("DATA\r\n");
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("DATA failed.");
				return;
			}
			Send(TestMail.Replace("#ADDRESS#", address) + "\r\n.\r\n");
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("Sending of e-mail failed.");
				return;
			}
			Send("QUIT\r\n");
			if (!IsReplyOK(Receive())) {
				Console.WriteLine("QUIT failed.");
			}
			Connection.Shutdown(SocketShutdown.Both);
		} catch (Exception e) {
			Console.WriteLine("An error occurred [" + e.Message + "]");
			Console.WriteLine(e);
		} finally {
			if (Connection != null) {
				Connection.Close();
			}
		}
	}
	/// <summary>
	/// This method is called when the SecureSocket received the remote
	/// certificate and when the certificate validation type is set to Manual.
	/// </summary>
	/// <param name="socket">The <see cref="SecureSocket"/> that received the certificate to verify.</param>
	/// <param name="remote">The <see cref="Certificate"/> of the remote party to verify.</param>
	/// <param name="chain">The <see cref="CertificateChain"/> associated with the remote certificate.</param>
	/// <param name="e">A <see cref="VerifyEventArgs"/> instance used to (in)validate the certificate.</param>
	/// <remarks>If an error is thrown by the code in the delegate, the SecureSocket will close the connection.</remarks>
	protected void OnVerify(SecureSocket socket, Certificate remote, CertificateChain chain, VerifyEventArgs e) {
		// get all the certificates from the certificate chain ..
		Certificate[] certs = chain.GetCertificates();
		// .. and print them out in the console
		for(int i = 0; i < certs.Length; i++) {
			Console.WriteLine(certs[i].ToString(true));
		}
		// print out the result of the chain verification
		Console.WriteLine(chain.VerifyChain(socket.CommonName, AuthType.Server));
	}
	// send data to the SMTP server
	protected void Send(string data) {
		Console.WriteLine(data.TrimEnd());
		byte[] toSend = Encoding.ASCII.GetBytes(data);
		int sent = Connection.Send(toSend);
		while(sent != toSend.Length) {
			sent += Connection.Send(toSend, sent, toSend.Length - sent, SocketFlags.None);
		}
	}
	// receive a reply from the SMTP server
	protected string Receive() {
		string reply = "";
		byte[] buffer = new byte[1024];
		int ret = Connection.Receive(buffer);
		while(ret > 0) {
			reply += Encoding.ASCII.GetString(buffer, 0, ret);
			if (IsComplete(reply))
				break;
			ret = Connection.Receive(buffer);
		}
		return reply;
	}
	// check whether the reply of the server is complete
	protected bool IsComplete(string reply) {
		string[] parts = reply.Replace("\r\n", "\n").Split('\n');
		if (parts.Length > 1 && ((parts[parts.Length - 2].Length > 3 && parts[parts.Length - 2].Substring(3, 1).Equals(" ")) || (parts[parts.Length - 2].Length == 3)))
			return true;
		else
			return false;
	}
	// check whether the reply of the server is positive
	protected bool IsReplyOK(string reply) {
		Console.Write(reply);
		try {
			int replyNumber = Int32.Parse(reply.Substring(0, 3)) / 100;
			if (replyNumber == 2 || replyNumber == 3)
				return true;
		} catch {}
		return false;
	}
	// the connection with the SMTP server
	protected SecureSocket Connection {
		get {
			return m_Connection;
		}
		set {
			m_Connection = value;
		}
	}
	private SecureSocket m_Connection;
	public static string TestMail = "From: \"Mentalis.org Secure Socket\" <secure.socket@mentalis.org>\r\nTo: \"You\" <#ADDRESS#>\r\nSubject: SecureSocket test\r\n\r\nThis is a test message sent through a Mentalis.org SecureSocket.";
}