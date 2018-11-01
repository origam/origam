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
using System.Xml;

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for MobilePhoneService.
	/// </summary>
	public class MobilePhoneServiceAgent : AbstractServiceAgent
	{
		public MobilePhoneServiceAgent()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public MobilePhoneShortMessages DecodeShortMessagesFromBuffer(SerialCommunicationBuffer buffer)
		{
			MobilePhoneShortMessages messages = new MobilePhoneShortMessages();

			foreach(SerialCommunicationBuffer.CommunicationEntryRow row in buffer.CommunicationEntry.Rows)
			{
				string[] lines = ((string)row.Buffer).Split(Environment.NewLine.ToCharArray());

				for(int i = 0; i < lines.Length; i++)
				{
					string line = lines[i];

					if(line.StartsWith("+CMGL:"))
					{
						string[] lineParams = line.Substring(7).Split(",".ToCharArray());

						string smsLine = null;

						for(int j = i + 1; j < lines.Length; j++)
						{
							if(lines[j] != null && lines[j] != "")
							{
								smsLine = lines[j];
								break;
							}
						}
					
						if(smsLine == null) throw new Exception(ResourceUtils.GetString("ErrorSMSDataNotBuffer"));

						//PDUDecoder.BaseInfo sms = PDUDecoder.Decode(smsLine);
						GsmLink.SmsMessage sms = new GsmLink.SmsMessage(smsLine);
					
						MobilePhoneShortMessages.MessageRow message = messages.Message.NewMessageRow();
						message.Id = Guid.NewGuid();
//						message.DestinationPhoneNumber = sms.DestinationNumber;
//						message.EMSCurrentPiece = sms.EMSCurrentPiece;
//						message.EMSTotalPiece = sms.EMSTotolPiece;
//						message.ReceivedDate = sms.ReceivedDate;
//						message.SourcePhoneNumber = sms.SourceNumber;
//						message.StatusReportState = (int)sms.StatusFromReport;
//						message.Text = sms.Text;

						try
						{
							int messagePosition = Convert.ToInt32(lineParams[0]);
							message.PositionInPhone = messagePosition;
						}
						catch
						{
							throw new Exception(ResourceUtils.GetString("ErrorNoPosition"));
						}

						message.DestinationPhoneNumber = sms.Destination;
						message.EMSCurrentPiece = 0;
						message.EMSTotalPiece = 0;
						message.ReceivedDate = sms.TimeStamp;
						message.SourcePhoneNumber = sms.Sender;
						message.StatusReportState = 0;
						message.Text = (string)sms.Body;

						messages.Message.AddMessageRow(message);
					}
				}
			}

			return messages;
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
				case "DecodeShortMessagesFromBuffer":
					// Check input parameters
					if(! (this.Parameters["Buffer"] is IDataDocument))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorBufferNotXmlDataDocument"));
					
					SerialCommunicationBuffer buffer = new SerialCommunicationBuffer();
					buffer.Merge((this.Parameters["Buffer"] as IDataDocument).DataSet);

					_result = this.DecodeShortMessagesFromBuffer(buffer);

					break;

				default:
					throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
			}
		}

		#endregion
	}
}
