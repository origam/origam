#region Copyright (c) Koolwired Solutions, LLC.
/*--------------------------------------------------------------------------
 * Copyright (c) 2006, Koolwired Solutions, LLC.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution. 
 * Neither the name of Koolwired Solutions, LLC. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission. 
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
 * AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 * THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
 * OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
 * WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 *--------------------------------------------------------------------------*/
#endregion

#region History
/*--------------------------------------------------------------------------
 * Modification History: 
 * Date       Programmer      		Description
 * 09/16/06   Keith Kikta     		Inital release.
 * 07/23/07	  Scott A. Braithwaite	Changed Connect and ParseLoginType to allow 
 *            either the user to choose the needed login type or default to the 
 *            strongest available.
 *--------------------------------------------------------------------------*/
#endregion

#region Refrences
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents the ImapConnect object.
    /// </summary>
    #endregion
    public class ImapConnect : IDisposable
    {

        #region private variables
        string _hostname = null;
        int _port = 143;
        TcpClient _connection;
        Stream _stream;
        bool _ssl = false;
        SslStream _sslstream;
        StreamReader _streamReader;
        ConnectionState _connectionState;
        int _receiveTimeout = 1000000;
        int _sendTimeout = 1000000;
        LoginType _loginType = LoginType.NONE;
        int _tag = 0;
        #endregion

        #region internal properties
        internal TcpClient Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        internal Stream Stream
        {
            get { return _stream; }
            set { _stream = value; }
        }
        internal SslStream SslStream
        {
            get { return _sslstream; }
            set { _sslstream = value; }
        }
        internal StreamReader StreamReader
        {
            get { return _streamReader; }
            set { _streamReader = value; }
        }
        internal ConnectionState ConnectionState
        {
            set { _connectionState = value; }
            get { return _connectionState; }
        }
        internal string tag
        {
            get
            {
                return "kw" + ((int)_tag++).ToString().PadLeft(4, '0') + " ";
            }
        }
        internal string CurrentTag
        {
            get
            {
                return "kw" + ((int)_tag - 1).ToString().PadLeft(4, '0') + " ";
            }
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the login type of the IMAP server.
        /// </summary>
        public LoginType LoginType
        {
            get { return _loginType; }
            set { _loginType = value; }
        }
        /// <summary>
        /// Gets or sets the hostname of the IMAP server to connect to.
        /// </summary>
        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }
        /// <summary>
        /// Gets or sets the port to connect on.
        /// </summary>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value indicating if a ssl stream should be used.
        /// </summary>
        public bool SSL
        {
            get { return _ssl; }
            set { _ssl = value; }
        }
        /// <summary>
        /// Gets or sets the timeout for receiving commands.
        /// </summary>
        public int ReceiveTimeout
        {
            get { return _receiveTimeout; }
            set { _receiveTimeout = value; }
        }
        /// <summary>
        /// Gets or sets the timeout for sending commands.
        /// </summary>
        public int SendTimeout
        {
            get { return _sendTimeout; }
            set { _sendTimeout = value; }
        }
        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public ConnectionState State
        {
            get { return _connectionState; }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initalizes an instance of the ImapConnect object using hostname and port.
        /// </summary>
        /// <param name="hostname">A string value representing the hostname of the mail server to connect to.</param>
        /// <param name="port">A integer value representing the port number on which imap connects on the specified mail server.</param>
        /// <param name="ssl">A boolean value indicating if a ssl stream should be used.</param>
        public ImapConnect(string hostname, int port, bool ssl)
        {
            _hostname = hostname;
            _port = port;
            _ssl = ssl;
        }
        /// <summary>
        /// Initalizes an instance of the ImapConnect object using hostname and port.
        /// </summary>
        /// <param name="hostname">A string value representing the hostname of the mail server to connect to.</param>
        /// <param name="port">A integer value representing the port number on which imap connects on the specified mail server.</param>
        public ImapConnect(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }
        /// <summary>
        /// Initalizes an instance of the ImapConnect object using hostname.
        /// </summary>
        /// <param name="hostname">A string value representing the hostname of the mail server to connect to.</param>
        public ImapConnect(string hostname)
        {
            _hostname = hostname;
            // Port is default
        }
        /// <summary>
        /// Initalizes an instance of the ImapConnect object
        /// </summary>
        public ImapConnect()
        {
            _hostname = "127.0.0.1";
        }
        #endregion

        #region public methods
        /// <summary>
        /// Opens a connection to the IMAP server.
        /// </summary>
        /// <returns>Returns a boolean value of true if the connection succeded.</returns>
        public bool Open()
        {
            string read;
            _connectionState = ConnectionState.Connecting;
            _connection = new TcpClient();
            _connection.ReceiveTimeout = ReceiveTimeout;
            _connection.SendTimeout = SendTimeout;
            try
            {
                _connection.Connect(_hostname, _port);
                if (!_ssl)
                {
                    _stream = _connection.GetStream();
                    _streamReader = new StreamReader(_stream, System.Text.Encoding.Default);
                }
                else
                {
                    _sslstream = new SslStream(_connection.GetStream(), false, new RemoteCertificateValidationCallback(CertificateValidationCallback));
                    _sslstream.AuthenticateAsClient(_hostname);
                    _streamReader = new StreamReader(_sslstream, System.Text.Encoding.Default);
                }
                //read = StreamReader.ReadLine();
                read = Read();
                if (read.StartsWith("* OK "))
                {
                    _connectionState = ConnectionState.Connected;
                    return true;
                }
                else
                    throw new ImapConnectionException(read);
            }
            catch (Exception ex)
            {
                _connectionState = ConnectionState.Closed;
                throw new ImapConnectionException("Connection Failed", ex);
            }
        }

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        /// <returns>Returns a boolean value indicating if the connection was closed.</returns>
        /// <exception cref="ImapConnectionException" />
        public bool Close()
        {
            try
            {
                _connection.Close();
                _connectionState = ConnectionState.Closed;
                return true;
            }
            catch (Exception ex)
            {
                throw new ImapConnectionException("Error Closing Connection", ex);
            }
        }

        /// <summary>
        /// Releases all resources used by the ImapConnect object.
        /// </summary>
        public void Dispose()
        {
            if (_connectionState != ConnectionState.Closed)
                try
                {
                    Close();
                }
                catch (Exception) { }
            _connection = null;
            _streamReader = null;
            _stream = null;
            _sslstream = null;
        }
        #endregion

        #region internal methods
        internal void Write(string message)
        {
            message = tag + message;
            byte[] command = System.Text.Encoding.ASCII.GetBytes(message.ToCharArray());
            try
            {
                //System.Web.HttpContext.Current.Response.Write(message + "<BR>");
                if (!_ssl)
                    _stream.Write(command, 0, command.Length);
                else
                    _sslstream.Write(command, 0, command.Length);
            }
            catch (Exception e)
            {
                throw new Exception("Write error :" + e.Message);
            }
        }

        internal void UntaggedWrite(string message)
        {
            byte[] command = System.Text.Encoding.ASCII.GetBytes(message.ToCharArray());
            try
            {
                if (!_ssl)
                    _stream.Write(command, 0, command.Length);
                else
                    _sslstream.Write(command, 0, command.Length);
            }
            catch (Exception e)
            {
                throw new Exception("Write error :" + e.Message);
            }
        }

        internal string Read()
        {
            string response = StreamReader.ReadLine();
            //System.Web.HttpContext.Current.Response.Write(response + "<BR>");
            return response;
        }
        #endregion

        #region Private Methods
        bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                //Console.WriteLine("SSL Certificate Validation Error!");
                //Console.WriteLine(sslPolicyErrors.ToString());
                return false;
            }
            else
                return true;
        }
        #endregion
    }
}
