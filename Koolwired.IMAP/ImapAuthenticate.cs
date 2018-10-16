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
 * 07/24/07	  Scott A. Braithwaite	Added CRAM-MD5
 * 08/27/07   Keith Kikta           Added LOGIN (Outlook Base64 user/pass)
 *--------------------------------------------------------------------------*/
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents the ImapAuthenticate object.
    /// </summary>
    #endregion
    public class ImapAuthenticate : IDisposable
    {
        #region private variables
        string _username;
        string _password;
        ImapConnect _connection;
        #endregion

        #region protected properties
        /// <summary>
        /// Sets the instance of ImapConnection to use for Authentication.
        /// </summary>
        public ImapConnect Connection
        {
            set { _connection = value; }
            private get { return _connection; }
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the username to use for authentication.
        /// </summary>
        public string Username
        {
            set { _username = value; }
            get { return _username; }
        }
        /// <summary>
        /// Gets or sets the password to use for authentication.
        /// </summary>
        public string Password
        {
            set { _password = value; }
            get { return _password; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// Initalizes an instance of the ImapAuthenticate object using the specified connection, username and password.
        /// </summary>
        /// <param name="connection">An instance of the ImapConnect object to be used for authentication.</param>
        /// <param name="username">A string value of the username to use for authentication.</param>
        /// <param name="password">A string value of the password to use for authentication.</param>
        public ImapAuthenticate(ImapConnect connection, string username, string password)
        {
            Connection = connection;
            Username = username;
            Password = password;
        }
        /// <summary>
        /// Initalizes an instance of the ImapAuthenticate object using the specified connection.
        /// </summary>
        /// <param name="connection">An instance of the ImapConnect object to be used for authentication.</param>
        public ImapAuthenticate(ImapConnect connection)
        {
            Connection = connection;
        }
        /// <summary>
        /// Initalizes an instance of the ImapAuthenticate object.
        /// </summary>
        public ImapAuthenticate()
        {
            // Default Constructor
        }
        #endregion

        #region public methods
        /// <summary>
        /// Authenticates the ImapConnection using the username and password previously specified.
        /// </summary>
        /// <exception cref="ImapAuthenticationException" />
        /// <exception cref="ImapAuthenticationNotSupportedException" />
        public void Login()
        {
            Login(Username, Password);
        }
        /// <summary>
        /// Authenticates the ImapConnection using the username and password specified.
        /// </summary>
        /// <param name="username">A string value representing the username to authenticate.</param>
        /// <param name="password">A string value representing the password to use for authentication.</param>
        /// <exception cref="ImapAuthenticationException" />
        /// <exception cref="ImapAuthenticationNotSupportedException" />
        public void Login(string username, string password)
        {
            string command;
            string result;
            if (!(Connection.ConnectionState == ConnectionState.Connected))
                throw new ImapConnectionException("Connection must be connected before authentication");
            Connection.ConnectionState = ConnectionState.Authenticating;

            //Moved from ImapConnect.Open because not all login methods will be advertised at the 
            //opening of the connection (Plain authentication usually won't be available until after
            //a STARTTLS command has been sent for example. --Scott

            //Not all servers advertise their capabilities on the OK response
            //So ask for them seperatley.
            string read;
            string serverResponse;
            _connection.Write("CAPABILITY\r\n");
            read = _connection.Read();

            //Check the Capabilities for available types
            _connection.LoginType = ParseLoginType(read);
            _connection.Read(); //Grab the extra OK line at the end
            switch (Connection.LoginType)
            {
                case LoginType.PLAIN:
                    command = "LOGIN " + username + " " + password + "\r\n";
                    Connection.Write(command);
                    break;
                case LoginType.LOGIN:
                    Connection.Write("AUTHENTICATE LOGIN\r\n");
                    serverResponse = Connection.Read();
                    Connection.UntaggedWrite(string.Format("{0}\r\n", base64Encode(username)));
                    serverResponse = Connection.Read();
                    Connection.UntaggedWrite(string.Format("{0}\r\n", base64Encode(password)));
                    break;
                case LoginType.CRAM_MD5:
                   //Let the server know to use CRAM-MD5 and read it's response (Base64 encoded)
                    Connection.Write("AUTHENTICATE CRAM-MD5\r\n");
                    serverResponse = Connection.Read();

                    string hmac = HmacMd5(username, password, serverResponse);       
                    Connection.UntaggedWrite(hmac + "\r\n");

                    break;
                default:
                    throw new ImapAuthenticationNotSupportedException("Authentication type unsupported:" + Connection.LoginType.ToString());
            }
            result = Connection.Read();
            if (result.StartsWith("* CAPABILITY")) // Gmail seems to send capabilities after login
            {
                result = Connection.Read();
            }
            if (result.StartsWith("* OK") || result.Substring(7, 2) == "OK")
                Connection.ConnectionState = ConnectionState.Open;
            else
            {
                Connection.ConnectionState = ConnectionState.Broken;
                throw new ImapAuthenticationException("Authentication failed: " + result);
            }
        }
        /// <summary>
        /// Sends a logout command to the ImapConnection.
        /// </summary>
        /// <exception cref="ImapAuthenticationException" />
        public void Logout()
        {
            string response;
            if (Connection.ConnectionState == ConnectionState.Closed)
                throw new ImapConnectionException("Connection already closed can not logout.");
            try
            {
                Connection.Write("LOGOUT\r\n");
                response = Connection.Read();
                Connection.ConnectionState = ConnectionState.Closed;
            }
            catch (Exception ex)
            {
                Connection.ConnectionState = ConnectionState.Broken;
                throw new ImapAuthenticationException("Error on connection can not logout: " + Connection.Read(), ex);
            }
        }

        /// <summary>
        /// Computes the correct HmacMd5 sum to return to the server
        /// </summary>
        /// <param name="username">Which username to compute with</param>
        /// <param name="password">Which password to compute with</param>
        /// <param name="serverResponse">The server response to hash against</param>
        /// <returns>The computed sum that can be returned to the server</returns>
        public static string HmacMd5(string username, string password, string serverResponse)
        {
            //Convert the password into usable bytes
            byte[] passwordBytes = System.Text.ASCIIEncoding.Default.GetBytes(password);

            //Setup the Keyed vectors by XORing the password with the given vectors
            byte[] ipad = new byte[64];
            byte[] opad = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                if (i < passwordBytes.Length)
                {
                    ipad[i] = (byte)(0x36 ^ passwordBytes[i]);
                    opad[i] = (byte)(0x5c ^ passwordBytes[i]);
                }
                else
                {
                    ipad[i] = 0x36 ^ 0x00;
                    opad[i] = 0x5C ^ 0x00;
                }
            }

            //Get rid of the "+ " at the beginning and then convert it into bytes
            string serverResponseT = serverResponse.TrimStart(new char[] { '+', ' ' });
            byte[] serverResponseBytes = System.Convert.FromBase64String(serverResponseT);

            //Setup the MD5 hash for the first round (ipad XOR Password + serverResponseT)
            MD5 md5 = new MD5CryptoServiceProvider();
            MemoryStream ms = new MemoryStream(1024);
            ms.Write(ipad, 0, ipad.Length);
            ms.Write(serverResponseBytes, 0, serverResponseBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] resultIpad = md5.ComputeHash(ms);

            //Setup the MD5 hash for the second round (opad XOR password + first round response)
            ms = new MemoryStream(1024);
            ms.Write(opad, 0, opad.Length);
            ms.Write(resultIpad, 0, resultIpad.Length);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] resultOpad = md5.ComputeHash(ms);

            //Append the username and the result and then convert it to base64
            string response = username + " " + ToHexString(resultOpad);
            response = Convert.ToBase64String(System.Text.ASCIIEncoding.Default.GetBytes(response));

            return response;
        }
        /// <summary>
        /// Releases all resources used by the ImapAuthenticate object and calls the logout method.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Logout();
            }
            catch (Exception) { }
        }
        #endregion
        
        #region private methods
        /// <summary>
        /// Converts an array of bytes into a usable hex string
        /// </summary>
        /// <param name="bytes">The array of bytes to convert into hex</param>
        /// <returns>A string of hexadecimal numbers</returns>
        private static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {
                '0', '1', '2', '3', '4', '5', '6', '7',
                '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
            };

            char[] chars = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }
        private string base64Encode(string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }
        private string base64Decode(string data)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }
        private LoginType ParseLoginType(string message)
        {
            //Get all the matches and loop through them
            Regex regex = new Regex("AUTH\\S*");
            MatchCollection matches;
            if (!String.IsNullOrEmpty(message))
            {
                matches = regex.Matches(message);
            }
            else
            {
                matches = null;
            }
            //gmail will have no matches so default to PLAIN.
            //If the login type hasn't been set, then
            if (_connection.LoginType == LoginType.NONE || matches.Count == 0)
            {
                LoginType ret = LoginType.NONE;

                if (matches != null)
                {
                    foreach (Match match in matches)
                    {
                        string[] vals = match.Value.Split('=');
                        if (vals.Length > 1)
                        {
                            switch (vals[1]) //This enum is by default an int and they're
                            //ordered in level of desirability.  Choose the best by
                            //using the highest one available.
                            {
                                case "NONE":
                                    if (ret <= LoginType.NONE)
                                        ret = LoginType.NONE;
                                    break;
                                case "PLAIN":
                                case "LOGIN-REFFERALS":
                                    if (ret <= LoginType.PLAIN)
                                        ret = LoginType.PLAIN;
                                    break;
                                case "LOGIN":
                                    if (ret <= LoginType.LOGIN)
                                        ret = LoginType.LOGIN;
                                    break;
                                case "CRAM-MD5":
                                    if (ret <= LoginType.CRAM_MD5)
                                        ret = LoginType.CRAM_MD5;
                                    break;
                            }
                        }
                    }
                }

                //Use this as a fallback if nothing else can be found
                if (ret == LoginType.NONE)
                {
                    ret = LoginType.PLAIN;
                }

                return ret;
            }
            //Else the login type has been chosen, if you can't find it then throw
            //an exception
            else
            {
                bool found = false;
                if (matches != null)
                {
                    foreach (Match match in matches)
                    {
                        string[] vals = match.Value.Split('=');
                        if (vals.Length > 1)
                        {
                            //Cast to a string.  If found, then the login 
                            //type is acceptable.
                            string loginTypeStr = _connection.LoginType.ToString().Replace('_', '-');
                            if (String.Compare(loginTypeStr, vals[1], true) == 0)
                            {
                                found = true;
                            }
                        }
                    }
                }
                if (!found)
                {
                    throw new ImapConnectionException("Authentication type not accepted");
                }

                return _connection.LoginType;
            }
        }
        #endregion
    }
}
