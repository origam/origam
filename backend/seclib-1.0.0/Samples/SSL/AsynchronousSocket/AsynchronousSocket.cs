using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Org.Mentalis.Security.Ssl;

/// <summary>
/// This example explains how to use the SecureSocket in asynchronous mode.
/// This class will fetch the URL http://www.microsoft.com/.
/// </summary>
class AsynchronousSocket {
	static void Main(string[] args) {
		Console.WriteLine("This example explains how to use the SecureSocket in asynchronous mode. This class will fetch the index page of www.microsoft.com.");
		AsynchronousSocket asc = new AsynchronousSocket();
		asc.Start();
	}
	public void Start() {
		// create a new ManualResetEvent. This will be used to make the main application
		// thread wait until the full server reply has been received.
		m_ResetEvent = new ManualResetEvent(false);
		// initialize the security options
		SecurityOptions options = new SecurityOptions(
			SecureProtocol.Ssl3 | SecureProtocol.Tls1,	// use SSL3 or TLS1
			null,										// do not use client authentication
			ConnectionEnd.Client,						// this is the client side
			CredentialVerification.None,				// do not check the certificate -- this should not be used in a real-life application :-)
			null,										// not used with automatic certificate verification
			"www.microsoft.com",						// this is the common name of the Microsoft web server
			SecurityFlags.Default,						// use the default security flags
			SslAlgorithms.SECURE_CIPHERS,				// only use secure ciphers
			null);										// do not process certificate requests.
		try {
			// create the securesocket with the specified security options
			m_Socket = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
			// resolve www.microsoft.com
			IPEndPoint endpoint = new IPEndPoint(Dns.Resolve("www.microsoft.com").AddressList[0], 443);
			// start connecting to www.microsoft.com
			m_Socket.BeginConnect(endpoint, new AsyncCallback(this.OnConnect), null);
			// wait until the entire web page has been received
			m_ResetEvent.WaitOne();
			// close the SecureSocket
			m_Socket.Close();
		} catch {
			OnError("Could not connect to the website");
		}
	}
	// called when the SecureSocket connects to the www.microsoft.com website
	private void OnConnect(IAsyncResult ar) {
		try {
			// end the connection request
			m_Socket.EndConnect(ar);
		} catch {
			OnError("the connection to the website failed");
		}
		Console.WriteLine("Connected to www.microsoft.com; sending HTTP request");
		try {
			// send the HTTP request to the server
			m_DataBuffer = Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\nHost: www.microsoft.com\r\n\r\n");
			m_Socket.BeginSend(m_DataBuffer, 0, m_DataBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnSent), null);
		} catch {
			OnError("unable to send the HTTP request");
		}
	}
	// called when the HTTP request is sent
	private void OnSent(IAsyncResult ar) {
		try {
			// end the send request
			int sent = m_Socket.EndSend(ar);
		} catch {
			OnError("unable to send the HTTP request");
		}
		Console.WriteLine("HTTP request sent; waiting for reply");
		try {
			// wait for the reply of the server
			m_DataBuffer = new byte[4096];
			m_Socket.BeginReceive(m_DataBuffer, 0, m_DataBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceive), null);
		} catch {
			OnError("unable to receive the HTTP reply");
		}
	}
	// called when the SecureSocket received a reply from the server
	private void OnReceive(IAsyncResult ar) {
		try {
			// get the received data from the server
			int received = m_Socket.EndReceive(ar);
			if (received > 0) {
				// write the received data in the console..
				Console.Write(Encoding.ASCII.GetString(m_DataBuffer, 0, received));
				// .. and start listening again
				m_Socket.BeginReceive(m_DataBuffer, 0, m_DataBuffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceive), null);
			} else {
				// the connection was shut down, so notify the main thread that the program can be stopped
				m_ResetEvent.Set();
			}
		} catch {
			OnError("unable to receive the HTTP reply");
		}
	}
	// prints an error message to the console and notifies the main thread that the program can be stopped
	private void OnError(string description) {
		Console.WriteLine("An error occurred: " + description);
		m_ResetEvent.Set();
	}
	private SecureSocket m_Socket;
	private ManualResetEvent m_ResetEvent;
	private byte[] m_DataBuffer;
}