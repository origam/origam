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
	using ArgumentNullException       = System.ArgumentNullException;
	using Array                       = System.Array;
	using DateTime                    = System.DateTime;
	using EventArgs                   = System.EventArgs;
	using EventHandler                = System.EventHandler;
	using Exception                   = System.Exception;
	using TimeSpan                    = System.TimeSpan;
	using System.Net.Sockets;
	using System.IO;
	using System.Threading;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections;
	using NandoF.Data;
	using NandoF.Mail.Parser2;
	
	public class PopClient : HighLevelTcpClient, System.IDisposable
	{
		#region Events
		
		/// <summary>Event that fires when connected with POP3 server.</summary>
		public event EventHandler CommunicationOccured;
		
		/// <summary>Event that fires when authentication begins.</summary>
		public event EventHandler AuthenticationBegan;
		
		/// <summary>Event that fires when authentication is finished.</summary>
		public event EventHandler AuthenticationFinished;
		
		/// <summary>Event that fires when message transfer has begun.</summary>
		public event EventHandler MessageTransferBegan;
		
		/// <summary>Event that fires when message transfer has finished.</summary>
		public event EventHandler MessageTransferFinished;
		
		internal void OnCommunicationOccured   (EventArgs e)  {
			if (CommunicationOccured != null)  CommunicationOccured(this, e);
		}
		
		internal void OnAuthenticationBegan    (EventArgs e)  {
			if (AuthenticationBegan != null)  AuthenticationBegan(this, e);
		}
		
		internal void OnAuthenticationFinished (EventArgs e)  {
			if (AuthenticationFinished != null)  AuthenticationFinished(this, e);
		}
		
		internal void OnMessageTransferBegan   (EventArgs e)  {
			if (MessageTransferBegan != null)  MessageTransferBegan(this, e);
		}
		
		internal void OnMessageTransferFinished(EventArgs e)  {
			if (MessageTransferFinished != null)  MessageTransferFinished(this, e);
		}
		#endregion
		
		// Constructor
		public PopClient(MailPopAccount account)  {
			if (account==null)  throw new ArgumentNullException("account");
			this.account = account;
		}
		
		public    MailPopAccount Account {
			get { return account; }
		}
		protected MailPopAccount account;
		
		#region Connect(), Disconnect(), Dispose(), IsConnected
		/// <summary>Connects to the POP3 server.</summary>
		public  void Connect()  {
			connect(account.Host, account.Port);
			string response  = readLine();
			if (isOkResponse(response))  {
				ExtractTimeStamp(response);
				isConnected = true;
				OnCommunicationOccured(EventArgs.Empty);
			} else {
				Disconnect();
				throw new PopServerNotAvailableException
					("Connect() error: The response was: " + response);
			}
		}
		
		public  bool IsConnected  {
			get  { return isConnected; }
		}
		private bool isConnected = false;
		
		public  void Disconnect()  {
			isConnected = false;
			try  {
				client.ReceiveTimeout = 500;
				client.SendTimeout    = 500;
				SendCommand("QUIT", true);
			}
			finally  {
				client.ReceiveTimeout = ReceiveTimeOut;
				client.SendTimeout    = SendTimeOut;
			}
			disconnect();
		}
		
		public  void Dispose()  {  // IDisposable implementation
			Disconnect();
		}
		#endregion
		
		private const string RESPONSE_OK = "+OK";
		//private const string RESPONSE_ERR="-ERR";
		private string _lastCommandResponse;
		
		public  string TimeStamp  {
			get  { return timeStamp; }
		}
		private string timeStamp;
		
//		public  int ReceiveContentSleepInterval  {
//			get  { return receiveContentSleepInterval;  }
//			set  { receiveContentSleepInterval = value; }
//		}
//		private int receiveContentSleepInterval = 100;
		
		/// <summary>Examines a string to see if it contains a timestamp
		/// to use with the APOP command.
		/// If it does, sets the TimeStamp property to this value.</summary>
		private void   ExtractTimeStamp(string response)  {
			Match match = Regex.Match(response, "<.+>");
			if (match.Success)  {
				timeStamp = match.Value;
			}
		}
		
		/// <summary> Returns whether a string starts with "+OK".</summary>
		private bool   isOkResponse(string response)  {
			return (response.Substring(0, 3) == RESPONSE_OK);
		}
		
		private string getResponsePrefix()  {
			return _lastCommandResponse.Substring(3);
		}
		
		/// <summary>Sends a command to the POP server.</summary>
		/// <param name="strCommand">command to send to server</param>
		/// <param name="blnSilent">Do not give error</param>
		/// <returns>true if server responded "+OK"</returns>
		private bool SendCommand(string strCommand, bool blnSilent)  {
			_lastCommandResponse = "";
			try  {
				if (stream.CanWrite)  {
					writer.WriteLine(strCommand);
					writer.Flush();
					_lastCommandResponse = this.readLine();
					return isOkResponse(_lastCommandResponse);
				}
				else  return false;
			}
			catch (Exception e)  {
				if (!blnSilent)  {
					warn(strCommand + ": " + e.Message);
				}
				return false;
			}
		}
		
		/// <summary>Sends a command to the POP server.</summary>
		/// <param name="strCommand">command to send to server</param>
		/// <returns>true if server responds "+OK"</returns>
		private bool SendCommand(string strCommand)  {
			return SendCommand(strCommand, false);
		}
		
		/// <summary>Sends a command to the POP server and expects an
		/// integer reply in the response.</summary>
		/// <param name="strCommand">command to send to server</param>
		/// <returns>integer value in the reply</returns>
		private int SendCommandIntResponse(string strCommand)  {
			int retVal = 0;
			if (SendCommand(strCommand))  {
				try  {
					retVal = int.Parse(_lastCommandResponse.Split(' ')[1]);
				}
				catch (Exception e)  {
					warn(strCommand + ": " + e.Message);
				}
			}
			return retVal;
		}
		
		/// <summary>Verifies user and password, trying both USER and APOP methods.
		/// </summary>
		/// <remarks>If not yet connected to the server, tries to connect before
		/// authenticating.</remarks>
		public void      Authenticate()  {
			Authenticate(AuthenticationMethods.TRYBOTH);
		}
		
		/// <summary>Verifies user and password.</summary>
		/// <param name="authenticationMethod">Verification mode.</param>
		/// <remarks>If not yet connected to the server, tries to connect before
		/// authenticating.</remarks>
		public  void     Authenticate(AuthenticationMethods authenticationMethod)  {
			if (authenticationMethod==AuthenticationMethods.USERPASS)
				AuthenticateUsingUSER();
			else if (authenticationMethod==AuthenticationMethods.APOP)
				AuthenticateUsingAPOP();
			else if (authenticationMethod==AuthenticationMethods.TRYBOTH)  {
				try  { AuthenticateUsingUSER(); }
				catch(InvalidLoginException e) {
					warn("Authenticate(): " + e.Message);
				}
				catch(InvalidPasswordException e) {
					warn("Authenticate(): " + e.Message);
				}
				catch(Exception e) {
					warn("Authenticate(): " + e.Message);
					AuthenticateUsingAPOP();
				}
			}
		}
		
		private void     AuthenticateUsingUSER()  {
			if (!this.isConnected)  Connect();
			OnAuthenticationBegan(EventArgs.Empty);
			if (!SendCommand("USER " + account.User))  {
				warn("AuthenticateUsingUSER(): wrong user");
				throw new InvalidLoginException();
			}
			awaitReadyWriter();
			if(!SendCommand("PASS " + account.Password))  {
				if(_lastCommandResponse.ToLower().IndexOf("lock") != -1)  {
					warn("AuthenticateUsingUSER(): maildrop is locked");
					throw new PopServerLockException();
				}
				else  {
					warn("AuthenticateUsingUSER(): " + getResponsePrefix());
					throw new InvalidPasswordException();
				}
			}
			
			OnAuthenticationFinished(EventArgs.Empty);
		}

		/// <summary>Verifies user and password using APOP.</summary>
		private void     AuthenticateUsingAPOP()  {
			if (!this.isConnected)  Connect();
			OnAuthenticationBegan(EventArgs.Empty);
			if (!SendCommand("APOP " + account.User + " " +
			                 MyMD5.GetMD5HashHex(account.Password)))  {
				warn("AuthenticateUsingAPOP(): wrong user or password");
				throw new InvalidLoginOrPasswordException();
			}
			OnAuthenticationFinished(EventArgs.Empty);
		}
		
		private string[] getParameters(string input)  {
			string []temp=input.Split(' ');
			string []retStringArray=new string[temp.Length-1];
			Array.Copy(temp,1,retStringArray,0,temp.Length-1);

			return retStringArray;
		}
		
		public int       GetMessageCount()  {
			return SendCommandIntResponse("STAT");
		}
		
		/// <summary>Deletes message with given index when Close() is called.
		/// </summary>
		public bool      DeleteMessage(int intMessageIndex)  {
			return SendCommand("DELE " + intMessageIndex.ToString());
		}
		
		public bool      DeleteAllMessages() {
			int messageCount = GetMessageCount();
			for (int messageItem = messageCount; messageItem>0; messageItem--) {
				if (!DeleteMessage(messageItem))
					return false;
			}
			return true;
		}
		
		/// <summary>Sends a QUIT command to the POP3 server.</summary>
		public bool QUIT() {
			return SendCommand("QUIT");
		}
		
		/// <summary>Keeps the server active.</summary>
		public bool NOOP()  {
			return SendCommand("NOOP");
		}
		
		/// <summary>Keeps the server active.</summary>
		public bool RSET()  {
			return SendCommand("RSET");
		}
		
		/// <summary>Gets the headers of one message.</summary>
		/// <param name="intMessageNumber">message number</param>
		/// <returns>A Message object, with null content (only headers).</returns>
		public Message   GetMessageHeader(int intMessageNumber)  {
			OnMessageTransferBegan(EventArgs.Empty);
			string s = fetchMessage("TOP " + intMessageNumber.ToString() + " 0");
			OnMessageTransferFinished(EventArgs.Empty);
			return new Message(s, false);
		}
		
		public string    GetMessageUID(int intMessageNumber)
		{
			string[] strValues=null;
			if(SendCommand("UIDL " + intMessageNumber.ToString()))
			{
				strValues = getParameters(_lastCommandResponse);
			}
			return strValues[1];
		}
		
		public ArrayList GetMessageUIDs()
		{
			ArrayList uids=new ArrayList();
			if(SendCommand("UIDL"))
			{
				string strResponse=reader.ReadLine();
				while (strResponse!=".")
				{
					uids.Add(strResponse.Split(' ')[1]);
					strResponse=reader.ReadLine();
				}
				return uids;
			}
			else
			{
				return null;
			}
		}
		
		/// <summary>Gets the sizes of all the messages.
		/// CAUTION: Assumes no messages have been deleted.</summary>
		/// <returns>An ArrayList of ints, with the size of each message.</returns>
		public ArrayList LIST()  {
			ArrayList sizes = new ArrayList();
			if(SendCommand("LIST"))  {
				string strResponse=reader.ReadLine();
				while (strResponse!=".")  {
					sizes.Add(int.Parse(strResponse.Split(' ')[1]));
					strResponse=reader.ReadLine();
				}
				return sizes;
			}
			else return null;
		}
		
		/// <summary>Gets the size of a message.</summary>
		public int       LIST(int intMessageNumber)  {
			return SendCommandIntResponse("LIST " + intMessageNumber.ToString());
		}
		
		/// <summary>Fetches a message (as a string).</summary>
		/// <param name="number">Message number on the server.</param>
		public  string   FetchMessage(int number)  {
			OnMessageTransferBegan(EventArgs.Empty);
			string s = fetchMessage("RETR " + number.ToString());
			OnMessageTransferFinished(EventArgs.Empty);
			return s;
		}
		
		/// <summary>Fetches a message (as a string).</summary>
		/// <param name="strCommand">Command to send to Pop server</param>
		private string   fetchMessage(string strCommand)  {
//			_receiveFinish = false;
			if (!SendCommand(strCommand))  return null;
			try  {
				return receiveUntilLine(".");
//				string receivedContent = ReceiveUntilLine(".");
//				Message msg = new Message((ref _receiveFinish, _basePath,
//				                          _autoDecodeMSTNEF, receivedContent,
//				                          blnOnlyHeader);
//				WaitForResponse(_receiveFinish, WaitForResponseInterval);
//				return msg;
			}
			catch (Exception e)  {
				warn("FetchMessage(): " + e.Message);
				return null;
			}
		}
		
	}
}

