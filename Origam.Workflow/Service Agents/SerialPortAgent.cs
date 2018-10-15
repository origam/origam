#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Timers;
using System.Text.RegularExpressions;

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for SerialPortAgent.
	/// </summary>
	public class SerialPortAgent : AbstractServiceAgent
	{
//		private class CommClass
//		{
//			private bool _communicationFinished = false;
//
//			public string Command = "";
//			public MSCommLib.MSCommClass comPort;
//			public string ReceiveBuffer = "";
//
//			CommClass()
//			{
//			}
//
//			public void Communicate()
//			{
//				comPort.PortOpen = true;
//				comPort.Output = Command + Environment.NewLine;
//			
//				while(! _communicationFinished) 
//				{
//					//System.Threading.Thread.Sleep(10);
//					System.Windows.Forms.Application.DoEvents();
//
//					// check for timeout
//					if(_isTimeoutExpired)
//					{
//						throw new Exception("Timeout expired while waiting for the serial port communication");
//					}
//				}
//			}
//
//		}

		private const int OUTPUT_END_TYPE_NOTHING = 0;
		private const int OUTPUT_END_TYPE_NEWLINE = 1;
		private const int OUTPUT_END_TYPE_EOF = 2;


		private MSCommLib.MSCommClass comPort = new MSCommLib.MSCommClass();
		private string _receiveBuffer = "";
		private bool _communicationFinished = false;
		private bool _isTimeoutExpired = false;
		private Timer _timer = new Timer(30000);

		public SerialPortAgent()
		{
			_timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);

			comPort.RThreshold = 0;
			comPort.RTSEnable = true;
			comPort.InBufferSize = 10000;
			comPort.OutBufferSize = 2048;
			comPort.DTREnable = true;
			comPort.Handshaking = MSCommLib.HandshakeConstants.comRTS;
			comPort.InputMode = MSCommLib.InputModeConstants.comInputModeText;
			comPort.InputLen = 0;
			comPort.NullDiscard = false;
			//comPort.OnComm += new MSCommLib.DMSCommEvents_OnCommEventHandler(comPort_OnComm);
		}

		public SerialCommunicationBuffer ExecuteCommandWithResults(string command, int port, string settings, int outputEndType)
		{
			_receiveBuffer = "";
			_communicationFinished = false;
			_isTimeoutExpired = false;

			comPort.CommPort = Convert.ToInt16(port);
			comPort.Settings = settings; //"2400,n,8,1";
			try
			{
				comPort.PortOpen = true;
			}
			catch(Exception ex)
			{
				comPort = null;
				throw new Exception(ResourceUtils.GetString("ErrorOpenPort", port.ToString(), Environment.NewLine + ex.Message));
			}

			_timer.Start();
			_isTimeoutExpired = false;
			while(! comPort.PortOpen)
			{
				System.Windows.Forms.Application.DoEvents();

				// check for timeout
				if(_isTimeoutExpired)
				{
					comPort.PortOpen = false;
					comPort = null;
					throw new Exception(ResourceUtils.GetString("ErrorSPConnectTimeout"));
				}
			}
			_timer.Stop();
			
			try
			{
				_isTimeoutExpired = false;
				_timer.Start();

				string[] commands = command.Split("\r".ToCharArray());

				for(int i = 0; i < commands.Length; i++)
				{
					string cmd = commands[i];
					if(cmd.StartsWith("\n"))
					{
						cmd = cmd.Substring(1, cmd.Length -1);
					}

					comPort.Output = cmd;
					if(i < commands.Length - 1)
					{
						comPort.Output = Environment.NewLine;
					}
					System.Threading.Thread.Sleep(100);
				}

				switch(outputEndType)
				{
					case OUTPUT_END_TYPE_NOTHING:
						break;

					case OUTPUT_END_TYPE_NEWLINE:
						comPort.Output = Environment.NewLine;
						break;

					case OUTPUT_END_TYPE_EOF:
						comPort.Output = Regex.Unescape(@"\cZ");	//((char)0x1A).ToString();
						break;

					default:
						comPort.PortOpen = false;
						comPort = null;
						throw new ArgumentOutOfRangeException("outputEndType", outputEndType, ResourceUtils.GetString("ErrorUnknownOutputEndType"));
				}

				while(! _communicationFinished) 
				{
					System.Windows.Forms.Application.DoEvents();
					//System.Diagnostics.Debug.WriteLine(comPort.InBufferCount);

//					object input = null;
					
//					try
//					{
//						input = comPort.Input;
//					} 
//					catch {}
//
//					if(input != null)
					if(comPort.InBufferCount > 0)
					{
//						_receiveBuffer += input;
						_receiveBuffer += comPort.Input;

						if(IsEndOfCommunication(_receiveBuffer))
						{
							_communicationFinished = true;
						}
					}

					// check for timeout
					if(_isTimeoutExpired)
					{
						comPort.PortOpen = false;
						comPort = null;
						throw new Exception(ResourceUtils.GetString("ErrorSPComTimeout"));
					}

					//System.Threading.Thread.Sleep(10);
				}
			}
			catch(Exception ex)
			{
				//comPort.PortOpen = false;
				comPort = null;
				throw;
			}
			finally
			{
				//comPort.PortOpen = false;
				_timer.Stop();
			}

			comPort.PortOpen = false;
			comPort = null;
			SerialCommunicationBuffer buffer = new SerialCommunicationBuffer();
			
			SerialCommunicationBuffer.CommunicationEntryRow row = buffer.CommunicationEntry.NewCommunicationEntryRow();
			row.Id = Guid.NewGuid();
			row.Buffer = _receiveBuffer;
			buffer.CommunicationEntry.AddCommunicationEntryRow(row);

			return buffer;
		}

		private bool IsEndOfCommunication(string buffer)
		{
			if(buffer.EndsWith("OK" + Environment.NewLine) | buffer.EndsWith("> ")) return true;
//			string[] lines = buffer.Split(Environment.NewLine.ToCharArray());
//
//			foreach(string line in lines)
//			{
//				if(line == "OK" | line == "> ")
//				{
//					return true;
//				}
//			}

			return false;
		}

		#region IServiceAgent Members
		private object _result;
		public override object Result
		{
			get
			{
				return _result;
			}
		}

		public override void Run()
		{
			switch(this.MethodName)
			{
				case "ExecuteCommandWithResults":
					// Check input parameters
					if(! (this.Parameters["Command"] is string))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorCommandNotString"));
					
					if(! (this.Parameters["Port"] is int))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorPortNotInt"));

					if(! (this.Parameters["Settings"] is string))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorSettingsNotString"));

					if(! (this.Parameters["OutputEndType"] is int))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorOutputEndTypeNotInt"));

					_result = this.ExecuteCommandWithResults((string)this.Parameters["Command"],
						(int)this.Parameters["Port"],
						(string)this.Parameters["Settings"],
						(int)this.Parameters["OutputEndType"]);

					break;

				default:
					throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
			}
		}

		#endregion

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			_isTimeoutExpired = true;
		}
	}
}