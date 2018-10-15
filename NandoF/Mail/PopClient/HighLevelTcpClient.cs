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
	using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
	using DateTime                    = System.DateTime;
	using EventArgs                   = System.EventArgs;
	using EventHandler                = System.EventHandler;
	using TimeSpan                    = System.TimeSpan;
	using System.Net.Sockets;
	using System.IO;
	using System.Threading;
	using System.Text;

	public class HighLevelTcpClient
	{
		#region Events
		public delegate void WarningEventHandler(object sender, string warning);
		public event WarningEventHandler WarningEvent;
		protected       void warn(string msg)  {
			if (WarningEvent != null)  WarningEvent(this, msg);
		}
		
		/// <summary>Event that fires just before connection is attempted.</summary>
		public event EventHandler CommunicationBegan;
		private void            OnCommunicationBegan(EventArgs e)  {
			if (CommunicationBegan != null)  CommunicationBegan(this, e);
		}
		
		/// <summary>Event that fires when disconnected from POP3 server.</summary>
		public event EventHandler CommunicationLost;
		private void            OnCommunicationLost (EventArgs e)  {
			if (CommunicationLost != null)  CommunicationLost(this, e);
		}
		
		
		#endregion
		
		public  int ReceiveBufferSize  {
			get  { return receiveBufferSize;  }
			set  {
				if (value < 1)  throw new ArgumentOutOfRangeException("ReceiveBufferSize");
				receiveBufferSize = value;
			}
		}
		private int receiveBufferSize = 4090;
		
		public  int SendBufferSize  {
			get  { return sendBufferSize;  }
			set  {
				if (value < 1)  throw new ArgumentOutOfRangeException("SendBufferSize");
				sendBufferSize = value;
			}
		}
		private int sendBufferSize    = 4090;
		
		/// <summary>Timeout, in milliseconds, for reception of information from the
		/// POP server. The default value is 60000 milliseconds.</summary>
		public  int  ReceiveTimeOut  {
			get  { return receiveTimeOut;  }
			set  {
				if (value < 1)  throw new ArgumentOutOfRangeException("ReceiveTimeOut");
				receiveTimeOut = value; }
		}
		private int  receiveTimeOut   = 60000;
		
		/// <summary>Timeout, in milliseconds, for sending information to the
		/// POP server. The default value is 60000 milliseconds.</summary>
		public  int SendTimeOut {
			get  { return sendTimeOut;  }
			set  {
				if (value < 1)  throw new ArgumentOutOfRangeException("SendTimeOut");
				sendTimeOut = value;
			}
		}
		private int sendTimeOut       = 60000;
		
		protected TcpClient     client = new TcpClient();
		protected NetworkStream stream;
		protected StreamReader  reader;
		protected StreamWriter  writer;
		
		/// <summary>Connects to the TCP server.</summary>
		protected void   connect(string host, int port)  {
			OnCommunicationBegan(EventArgs.Empty);
			client.ReceiveTimeout    = ReceiveTimeOut;
			client.ReceiveBufferSize = ReceiveBufferSize;
			client.SendTimeout       = SendTimeOut;
			client.SendBufferSize    = SendBufferSize;
			try  { client.Connect(host, port); }
			catch (SocketException)  {
				disconnect();
				throw; // new PopServerNotFoundException(e.Message);
			}
			stream = client.GetStream();
			reader = new StreamReader(stream, Encoding.Default, true);
			writer = new StreamWriter(stream);
			writer.AutoFlush = true;
		}
		
		protected void   disconnect()  {
			try  {
				if (client != null)  client.Close();
				if (writer != null)  writer.Close();
				if (reader != null)  reader.Close();
//				stream().Close();
			}
			finally  {
				reader = null;
				writer = null;
				stream = null;
//				client = null;
			}
			OnCommunicationLost(EventArgs.Empty);
		}
		
		/// <summary>Time, in milliseconds, to sleep before checking if we
		/// have received information from the POP server.</summary>
		public    int    WaitForResponseInterval  {
			get  { return waitForResponseInterval;  }
			set  {
				if (value < 1)  throw new ArgumentOutOfRangeException();
				waitForResponseInterval = value;
			}
		}
		private   int    waitForResponseInterval = 200;
		
		private   void   awaitReaderResponse(int timeOut)  {
			DateTime start = DateTime.Now;
			TimeSpan patience;
			while(!this.reader.BaseStream.CanRead) {
				patience = DateTime.Now.Subtract(start);
				if (patience.Milliseconds > timeOut)  break;
				Thread.Sleep(WaitForResponseInterval);
			}
		}
		private   void   awaitReaderResponse()  {
			awaitReaderResponse(this.ReceiveTimeOut);
		}
		
		protected string readLine(int timeOut)  {
			awaitReaderResponse(timeOut);
			string response = reader.ReadLine();
			if (response==null)  response = string.Empty;  // added by Nando
			return response;
		}
		protected string readLine()  {
			awaitReaderResponse();
			string response = reader.ReadLine();
			if (response==null)  response = string.Empty;  // added by Nando
			return response;
		}
		
		protected string receiveUntilLine(string expectedLine)  {
			StringBuilder builder = new StringBuilder(4000);
			string        line    = readLine();
			int intLines = 0;
			while (line != expectedLine)  {
				builder.Append(line + "\r\n");
				++intLines;
				line = readLine(1);
			}
			// TODO: Include last line?
			builder.Append(line + "\r\n");
			return builder.ToString();
		}
		
		protected void   awaitReadyWriter() {
			while (!stream.CanWrite)  Thread.Sleep(WaitForResponseInterval);
		}
		
	}
}
