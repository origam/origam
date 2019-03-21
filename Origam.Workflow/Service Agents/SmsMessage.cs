#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
//
//  
//	GSM-Link - a free SMS messaging library for .NET
//	Copyright (C) 2003 Cris Almodovar
//
//	This library is free software; you can redistribute it and/or
//	modify it under the terms of the GNU Lesser General Public
//	License as published by the Free Software Foundation; either
//	version 2.1 of the License, or (at your option) any later version.
//
//	This library is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//	Lesser General Public License for more details.
//
//	You should have received a copy of the GNU Lesser General Public
//	License along with this library; if not, write to the Free Software
//	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Origam.Workflow;

namespace GsmLink
{	
	/// <summary>
	/// Represents an SMS message.
	/// </summary>
	/// <remarks>
	/// The <b>SmsMessage</b> class encapsulates the data and algorithms 
	/// for decoding and encoding <b>Protocol Data Unit (PDU)</b> packets -  
	/// representing SMS messages - that are received from and sent to 
	/// a GSM device.
	/// <para>
	/// The contents and layout of <b>PDU</b> data packets and the algorithm
	/// for encoding/decoding text messages using the GSM default character 
	/// set are defined in the <b>GSM 03.40</b> and <b>GSM 03.38</b> technical
	/// specifiacations.
	/// </para>
	/// </remarks>
	public class SmsMessage 
	{
		#region Fields
		
		private MessageType type = MessageType.Unknown;
		private MessageStatus status = MessageStatus.Unknown;
		private EncodingScheme encodingScheme = EncodingScheme.Unknown;
		private int index = 0;
		private string sender = "";
		private string destination = "";
		private DateTime timeStamp = DateTime.MinValue;
		private object body = null;		
		private object tag = null;
		
		#endregion Fields

		#region Constructors
		
		// NOTE: Using this constructor implies that the 
		//		 message is of type SmsSubmit, and that
		//       the text message will be encoded using
		//       the GSM default alphabet.
		public SmsMessage(string phoneNumber, string text) 
		{			
			this.type = MessageType.SmsSubmit;
			this.encodingScheme = EncodingScheme.Text;
			this.status = MessageStatus.Unsent;			
			this.destination = FormatPhoneNumber(phoneNumber, true);
			if (this.destination == "")
			{
				string errorMsg = ResourceUtils.GetString("ErrorInvalidPhoneNumber");
				throw new ApplicationException(errorMsg);
			}
			this.timeStamp = DateTime.Now;			
			if (text.Length >= 0 && text.Length <= 160)
				this.body = text;
			else
			{
				string errorMsg = ResourceUtils.GetString("ErrorMessageTooLong");
				throw new ApplicationException(errorMsg);
			}
//			this.pduString = CreateSmsSubmitPduString(this.destination, text);           
//			this.pdu = ConvertToByteArray(this.pduString);									
			this.status = MessageStatus.Unsent;
		}

		// NOTE: Using this constructor implies that the 
		//		 message is of type SmsSubmit, and that 
		//       the data is 8-bit data.
		public SmsMessage(string phoneNumber, byte[] data)
		{		
			this.type = MessageType.SmsSubmit;
			this.encodingScheme = EncodingScheme.Data;
			this.status = MessageStatus.Unsent;						
			this.destination = FormatPhoneNumber(phoneNumber, true);
			if (this.destination == "")
			{
				string errorMsg = ResourceUtils.GetString("ErrorInvalidPhoneNumber");
				throw new ApplicationException(errorMsg);
			}			
			this.timeStamp = DateTime.Now;			
			if (data != null && data.GetLength(0) >= 0 && data.GetLength(0) <= 140)
				this.body = data;
			else
			{
				string errorMsg = ResourceUtils.GetString("ErrorDataTooBig");
				throw new ApplicationException(errorMsg);
			}			
//			this.pduString = CreateSmsSubmitPduString(this.destination, data);           
//			this.pdu = ConvertToByteArray(this.pduString);
			this.status = MessageStatus.Unsent;
		}

		// NOTE: Using this constructor implies that the 
		//		 message is of type SmsDeliver.
		public SmsMessage(string pduString) 
		{		
			this.type = MessageType.SmsDeliver;
			object[] messageContents = ParseSmsDeliverPduString(pduString);
			this.sender = (string)messageContents[0];
			this.timeStamp = (DateTime)messageContents[1];
			this.encodingScheme = (EncodingScheme)messageContents[2];
			this.body = messageContents[3];			
//			this.pduString = pduString;	
//			this.pdu = ConvertToByteArray(this.pduString);
			this.status = MessageStatus.Unread;
		}			

		// NOTE: Using this constructor implies that the 
		//		 message is of type SmsDeliver.
		public SmsMessage(byte[] pdu)
		{			
			this.type = MessageType.SmsDeliver;
			object[] messageContents = ParseSmsDeliverPdu(pdu);
			this.sender = (string)messageContents[0];
			this.timeStamp = (DateTime)messageContents[1];
			this.encodingScheme = (EncodingScheme)messageContents[2];
			this.body = messageContents[3];			
//			this.pduString = ConvertToHexString(pdu);	
//			this.pdu = pdu;
			this.status = MessageStatus.Unread;
		}		

		#endregion Constructors
		
		#region Properties
		
		public MessageType Type
		{
			get { return this.type; }			
		}
		
		public MessageStatus Status
		{
			get { return this.status; }			
		}

		public EncodingScheme EncodingScheme
		{
			get { return this.encodingScheme; }			
		}

		internal int Index 
		{
			get { return this.index; }
			set 
			{ 
				if (value > 0)
				{
					this.index = value;
				}
				else
				{
					string errorMsg = ResourceUtils.GetString("ErrorIndexProperty");
					throw new ApplicationException(errorMsg);
				}
			}
		}		

		public string Sender
		{
			get { return this.sender; }			
		}

		public string Destination
		{
			get { return this.destination; }			
		}

		public DateTime TimeStamp
		{
			get { return this.timeStamp; }
		}		

		public object Body
		{
			get { return this.body; }
		}		

		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets the value of the <see cref="Body"/> property and 
		/// changes the value of the <see cref="Status"/> property
		/// to <b>MessageStatus.Read</b>.
		/// </summary>
		/// <returns>
		/// The value of the <see cref="Body"/> property.
		/// </returns>
		public object Read()
		{			
			if (this.status == MessageStatus.Unread)			
				this.status = MessageStatus.Read;				
			
			return this.body;
		}

		internal void SetMessageStatus(MessageStatus status)
		{
			if (status == MessageStatus.Unread || status == MessageStatus.Read ||
				status == MessageStatus.Unsent || status == MessageStatus.Sent)
			{
				if (status != this.status)
					this.status = status;
			}
			else
			{
				string errorMsg = ResourceUtils.GetString("ErrorInvalidStatus");
				throw new ApplicationException(errorMsg);
			}
		}

		public byte[] GetPdu()
		{
			byte[] pdu = new byte[0];

			if (this.type == MessageType.SmsSubmit)
			{
				pdu = CreateSmsSubmitPdu(this.destination, this.body);
			}

			// TODO: Implement processing for SMS-DELIVER PDU.
			
			return pdu;
		}

		public string GetPduString()
		{
			string pduString = "";

			if (this.type == MessageType.SmsSubmit)
			{
				pduString = CreateSmsSubmitPduString(this.destination, this.body);
			}

			// TODO: Implement processsing for SMS-DELIVER PDU.

			return pduString; 			
		}


		#region Methods for creating an SMS-SUBMIT PDU

		/// <summary>
		/// Creates an SMS-SUBMIT PDU using the supplied destination phone number 
		/// and message.
		/// </summary>
		/// <param name="phoneNumber">
		/// The destination phone number, in international format.
		/// </param>
		/// <param name="message">
		/// The message to send, can be either a <see cref="System.String"/>
		/// or a <see cref="System.Byte"/> array.
		/// </param>
		/// <returns>
		/// A <see cref="Byte"/> array containing the SMS-SUBMIT PDU if successful; 
		/// an empty <see cref="Byte"/> array otherwise.
		/// </returns>
		private static byte[] CreateSmsSubmitPdu(string phoneNumber, object message)
		{
			byte[] outputBytes = new byte[0];			

			if (phoneNumber != null && phoneNumber.Length > 0)
			{
				// Determine the encoding scheme to use for 
				// the message.
				EncodingScheme encodingScheme = EncodingScheme.Unknown;
				string messageString = "";
				byte[] messageBytes = null;
				System.Type messageType = message.GetType();

				if (messageType == typeof(System.String) || message == null)				
				{
					encodingScheme = EncodingScheme.Text;				
					if (message != null)
						messageString = (string)message;					
				}
				else if (messageType.IsArray && messageType.GetElementType() == typeof(System.Byte))				
				{
					encodingScheme = EncodingScheme.Data;				
					messageBytes = (byte[])message;
				}

				if (encodingScheme == EncodingScheme.Unknown)
				{
					string errorMsg = ResourceUtils.GetString("ErrorInvalidMessageType");
					throw new ApplicationException(errorMsg);
				}				

				// Remove whitespace and any non-number character
				// from the phoneNumber.
				phoneNumber = FormatPhoneNumber(phoneNumber, false);
				
				// Create the PDU, one byte at a time.

				ArrayList buffer = new ArrayList();

				// Add the SMSC-INFO-LENGTH field. 
				// The value 0x00 denotes that there 
				// is be no SMSC-INFO included.
				buffer.Add((byte)0x00);

				// Add the first octet of the SMS-SUBMIT message.
				// The value 0x01 denotes that: 
				// a. The message validity period is unspecified.
				// b. The message is of type SMS-SUBMIT.
				buffer.Add((byte)0x01);

				// Add the TP-MESSAGE-REFERENCE field.
				// The value 0x00 denotes that the GSM device
				// should set the message reference itself.
				buffer.Add((byte)0x00);

				// Add the ADDRESS-LENGTH field (length of the 
				// destination phone number)
				buffer.Add(Convert.ToByte(phoneNumber.Length));

				// Add the TYPE-OF-ADDRESS field.
				// The value 0x91 denotes the international
				// format for telephone numbers.
				buffer.Add((byte)0x91);

				// Add the destination phone number -
				// encoded as decimal semi-octets.
				byte[] phoneNumberBytes = EncodePhoneNumber(phoneNumber);
				for (int i = 0; i < phoneNumberBytes.GetLength(0); i++)
				{
					buffer.Add(phoneNumberBytes[i]);
				}

				// Add the TP-PID (Protocol Identifier) field.
				// The value 0x00 denotes that the protocol
				// is implicit.
				buffer.Add((byte)0x00);

				// Add the TP-DCS (Data Coding Scheme) field.
				// The value 0x00 denotes that the GSM default 
				// 7-bit alphabet is used. The value 0x04
				// denotes that the message is 8-bit.
				if (encodingScheme == EncodingScheme.Text)
					buffer.Add((byte)0x00);
				else if (encodingScheme == EncodingScheme.Data)
					buffer.Add((byte)0x04);

				// Add the TP-USER-DATA-LENGTH field.
				// This is the length of the message,
				// after properly casting it to either
				// string or byte array.
				if (encodingScheme == EncodingScheme.Text)
				{
					// Encode the text message using 
					// the GSM encoding scheme.
					int dataLength = 0;
					messageBytes = EncodeTextMessage(messageString, ref dataLength);				
					buffer.Add(Convert.ToByte(dataLength));
				}
				else if (encodingScheme == EncodingScheme.Data)
				{
					// NOTE: If the messages is a data message 
					// then no further conversion is needed.
					buffer.Add(Convert.ToByte(messageBytes.GetLength(0)));
				}

				// Add the TP-USER-DATA field.				
								
				for (int i = 0; i < messageBytes.GetLength(0); i++)
				{
					buffer.Add(messageBytes[i]);
				}   
            
				// Transfer the contents of the ArrayList
				// into the output byte array.
				outputBytes = new byte[buffer.Count];				
				buffer.CopyTo(outputBytes); 
			}
			
			return outputBytes;
		}
		
		
		/// <summary>
		/// Creates an SMS-SUBMIT PDU converted to a <see cref="System.String"/>, 
		/// using the supplied message and destination phone number.
		/// </summary>
		/// <param name="phoneNumber">
		/// The destination phone number, in international format.
		/// </param>
		/// <param name="message">
		/// The message to send, can be either a <see cref="System.String"/> 
		/// or a <see cref="System.Byte"/> array.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> representing the SMS-SUBMIT PDU 
		/// if successful; an empty <see cref="System.String"/> otherwise.
		/// </returns>
		private static string CreateSmsSubmitPduString(string phoneNumber, object message)
		{
			byte[] pdu = CreateSmsSubmitPdu(phoneNumber, message);
			return ConvertToHexString(pdu);
		}


		/// <summary>
		/// Converts a text message into an octet sequence 
		/// encoded using the GSM 7-bit default character set.
		/// </summary>
		/// <remarks>
		/// The algorithm for encoding a text message using the 
		/// GSM 7-bit default character set is described in the 
		/// <b>GSM 03.40</b> and <b>GSM 03.38</b> technical 
		/// specifications.
		/// </remarks>
		/// <param name="textMessage">
		/// The text message to encode.
		/// </param>
		/// <returns>
		/// A <see cref="System.Byte"/> array representing the resulting 
		/// octet sequence if successful; an empty <see cref="System.Byte"/> 
		/// array otherwise.
		/// </returns>
		private static byte[] EncodeTextMessage(string textMessage, ref int dataLength)
		{			
			byte[] outputBytes = new byte[0];			
			
			if (textMessage != null && textMessage.Length > 0)
			{
				try
				{
					// Convert the textMessage string into a byte array.										
					// But first, convert each Unicode character to its 
					// equivalent GSM character.				

					byte[] inputBytes = null;
					ArrayList buffer = new ArrayList();

					for (int index = 0; index < textMessage.Length; index++)
					{
						byte[] b = CharSetConverter.UnicodeToGsm(textMessage[index]);						
						if (b != null && b.GetLength(0) > 0)
						{
							buffer.Add(b[0]);														
							if (b.GetLength(0) == 2)
							{
								buffer.Add(b[1]);
							}							
						}
					}										
					inputBytes = new byte[buffer.Count];
					dataLength = buffer.Count;
					buffer.CopyTo(inputBytes, 0);						
				
					// Allocate space for the output byte array. 
					// The output byte array uses up a lesser number 
					// of bytes if the length of the input array length
					// is greater than 8.
					int outputLength = inputBytes.GetLength(0) - (inputBytes.GetLength(0) / 8);				
					outputBytes = new byte[outputLength];
				
					int i = 0, j = 0;
					int shrCounter = 0, shlCounter = 7;

					while (i < inputBytes.Length && j < outputBytes.GetLength(0))
					{
						// Copy the ith byte from the inputBytes array. 
						byte currentByte = inputBytes[i];
					
						// Get rid of the low bits because these
						// have already been extracted during the 
						// previous iteration (and prepended to 
						// the previous byte). Note: This step 
						// has no effect on the first byte.					
						currentByte = (byte)((int)currentByte >> shrCounter);
                    
						// Extract low bits from the next byte's bit sequence,
						// and prepend it to the current byte's bit sequence.					
						if ((i + 1) < inputBytes.GetLength(0))
						{						
							byte nextByte = inputBytes[i + 1];
							byte extraBits = (byte)((int)nextByte << shlCounter);
						
							// Prepend the bits to the left (high bits)
							// of the current byte's bit sequence.
							currentByte = (byte)(currentByte | extraBits);
						}

						// Copy the processed byte to the outputBytes array.
						outputBytes[j] = currentByte;

						// Note: Every 8th character can be skipped 
						// because the bit sequence representing 
						// the character have already been encoded 
						// in the preceding 7 bytes.

						if ((i + 1) % 8 != 0)
						{	
							// The next character's position is not 
							// a multiple of 8, so continue with 
							// normal processing.
							i += 1;
							j += 1;
							shrCounter += 1;
							shlCounter -= 1;												
						}
						else 
						{	
							// Skip the next character.
							i += 1;

							// Reset shift counters.
							shrCounter = 0;
							shlCounter = 7;										
						}					
					}
				}
				catch(Exception ex)
				{
					string errorMsg = ResourceUtils.GetString("ErrorEncode");
					throw new Exception(errorMsg, ex);
				}
			}
			return outputBytes;
		}
		
		
		private static byte[] EncodePhoneNumber(string phoneNumber)
		{
			byte[] outputBytes = null;

			if (phoneNumber != null && phoneNumber.Length > 0)
			{
				int byteCount = (phoneNumber.Length % 2) == 0 ? phoneNumber.Length / 2 : 
					(phoneNumber.Length / 2) + 1;                				
				
				outputBytes = new byte[byteCount];

				int byteIndex = 0;

				for (int i = 0; i < phoneNumber.Length; i += 2)
				{
					// Work with 2 digits at a time.
					byte digit1 = Byte.Parse(phoneNumber[i].ToString());
					
					byte digit2 = 0xFF;
					if ((i + 1) < phoneNumber.Length)
						digit2 = Byte.Parse(phoneNumber[i + 1].ToString());

					// Move the second digit to the high bit positions.
					digit2 = (byte)(digit2 << 4);

					// Combine the two digits in one byte.
					byte currentByte = (byte)(digit1 | digit2);
					
					outputBytes[byteIndex] = currentByte;
					byteIndex += 1;					
				}				
			}
			
			return outputBytes;
		}        
		

		#endregion

		#region Methods for extracting the contents of an SMS-DELIVER PDU
		
		private static object[] ParseSmsDeliverPdu(byte[] pdu)
		{	
			object[] messageContents = new object[0];

			if (pdu != null && pdu.GetLength(0) > 0)
			{
				try
				{
					// Track the current position in the PDU byte array.
					int currentPosition = 0;

					// Determine the length of the SMSC info.
					int smscInfoLength = Convert.ToInt32(pdu[currentPosition++]);				
                    
					if (smscInfoLength <= 0)
					{
						string errorMsg = ResourceUtils.GetString("ErrorInvalidLenghtValue");
						throw new Exception(errorMsg);
					}

					// Extract the SMSC information.								
					byte[] smscInfo = new byte[smscInfoLength];
					for (int i = 0; i < smscInfo.GetLength(0); i++)
					{
						smscInfo[i] = pdu[currentPosition++]; 
					}

					// Determine the TYPE-OF-ADDRESS of the SMSC.
					// This is the first byte of the SMSC info.
					// Note: Assumed to be 0x91 for international
					// telephone number format.
					byte smscAddressType = smscInfo[0];
					if (smscAddressType != (byte)0x91)
					{
						string errorMsg = ResourceUtils.GetString("ErrorUnexpectedFormat");
						throw new ApplicationException(errorMsg);
					}

					// Extract the SMSC number from the SMSC information. 
					// The number is embedded in the remaining bytes of 
					// the smscInfo array.
					int smscNumberLength = smscInfo.GetLength(0) - 1;
					byte[] smscNumberBytes = new byte[smscNumberLength];
					Array.Copy(smscInfo, 1, smscNumberBytes, 0, smscNumberLength);
					string smscNumber = FormatPhoneNumber(DecodePhoneNumber(smscNumberBytes), true);

					// Extract the first octet of the SMS-DELIVER
					// message. The two least significant bits
					// (bit 1 and bit 0) must both be set to 0 
					// for the PDU to be interpreted as an
					// SMS-DELIVER message.
					byte firstOctet = pdu[currentPosition++];
					if ((firstOctet & (byte)0x03) != (byte)0x00)
					{
						string errorMsg = ResourceUtils.GetString("ErrorPduParameter");
						throw new Exception(errorMsg);
					}

					// Determine the length of the sending party's 
					// telephone number.
					int senderNumberLength = Convert.ToInt32(pdu[currentPosition++]);

					// Determine the address type of the sending party's
					// telephone number. Note: Assumed to be 0x91 for
					// international telephone number format.
					byte senderAddressType = pdu[currentPosition++];
//					if (senderAddressType != (byte)0x91)
//					{
//						string errorMsg = "Unexpected format for sender number.";
//						throw new ApplicationException(errorMsg);
//					}

					// Determine the number of bytes used to represent
					// the sending party's telephone number.
					int senderNumberByteCount = (senderNumberLength % 2) == 0 ? senderNumberLength / 2 : 
						(senderNumberLength / 2) + 1;  

					// Extract the sending party's telephone number,
					// (encoded as a sequence of decimal semi-octets)
					// from the PDU.
					byte[] senderNumberBytes = new byte[senderNumberByteCount];
					for (int i = 0; i < senderNumberByteCount; i++)
					{
						senderNumberBytes[i] = pdu[currentPosition++];
					}

					// Decode the sequence of decimal semi-octets 
					// to get the telephone number as a string.
					string senderNumber = FormatPhoneNumber(DecodePhoneNumber(senderNumberBytes), true);

					// Determine the Protocol ID from the TP-PID field.
					// Note: This field is currently not used; it is 
					// assumed to have the value 0x00, indicating that
					// the protocol is Implicit/SC-specific.
					byte protocolID = pdu[currentPosition++];
					if (protocolID != (byte)0x00)
					{
						string errorMsg = ResourceUtils.GetString("ErrorTpPid");
						throw new Exception(errorMsg);
					}
 
					// Determine the Data Coding Scheme from the TP-DCS field.
					// Note: This field is currently not used; it is assumed 
					// to have either the value 0x00, indicating that the 7-bit 
					// GSM alphabet encoding is used, or 0x04, indicating that
					// the 8-bit encoding is used. No other encoding schemes
					// are supported.
					byte dataCodingScheme = pdu[currentPosition++];
					EncodingScheme encodingScheme = EncodingScheme.Unknown;
					if (dataCodingScheme == (byte)0x00)
					{
						encodingScheme = EncodingScheme.Text;
					}
					else if (dataCodingScheme == (byte)0x04)
					{
						encodingScheme = EncodingScheme.Data;
					}
					else
					{
						// TODO: Handle other data coding schemes,
						// such as V-Card, Flash SMS, etc.
					}

					// Extract the 7 bytes that contain	the time stamp of the message;
					// the time stamp is encoded as decimal semi-octets. 
					// Note: The number of bytes may vary depending on the format used 
					// for the VALIDITY-PERIOD field.
					byte[] timeStampBytes = new byte[7];
					for (int i = 0; i < 7; i++)
					{
						timeStampBytes[i] = pdu[currentPosition++];
					}

					// Determine the actual time stamp of the message.
					DateTime timeStamp = DecodeTimeStamp(timeStampBytes);
				
					// Determine the length of the user data field.
					int userDataLength = pdu[currentPosition++];

					// Extract the USER-DATA field.
					// The actual text message is 
					// contained in this field.								
					ArrayList buffer = new ArrayList();
					while(currentPosition < pdu.GetLength(0))
					{
						buffer.Add(pdu[currentPosition++]);
					}					

					// Transfer the contents of the ArrayList
					// to a byte array.
					byte[] userData = new byte[buffer.Count];
					buffer.CopyTo(userData);								

					// Decode the user data.
					string messageString = "";
					byte[] messageBytes = null;
					if (encodingScheme == EncodingScheme.Text)
						messageString = DecodeTextMessage(userData);
					else if (encodingScheme == EncodingScheme.Data)
						messageBytes = userData;

					// Check if the number of characters or bytes in the 
					// message (string or byte array) does not match userDataLength.
					// This is not a fatal error condition if the message is a
					// string (i.e. the encoding scheme is EncodingScheme.Text)
					// because even though the text message may be truncated 
					// it may still be readable. However if the message is a byte 
					// array and the length of the byte array does not match 
					// userDataLength then this is a fatal error condition.
					if (encodingScheme == EncodingScheme.Data && messageBytes.GetLength(0) != userDataLength)
					{
						string errorMsg = ResourceUtils.GetString("ErrorLenghtMatch");

						throw new ApplicationException(errorMsg);
					}				

					messageContents = new object[4];
					messageContents[0] = (object)senderNumber;					
					messageContents[1] = (object)timeStamp;	
					messageContents[2] = (object)encodingScheme;
					messageContents[3] = (encodingScheme == EncodingScheme.Text) ? (object)messageString : 
						(object)messageBytes;

				}
				catch (Exception ex)
				{
					string errorMsg = ResourceUtils.GetString("ErrorParsePdu");
					throw new Exception(errorMsg, ex);										
				}
			}

			return messageContents;
		}

		
		private static object[] ParseSmsDeliverPduString(string pduString)
		{
			byte[] pdu = ConvertToByteArray(pduString);
			return SmsMessage.ParseSmsDeliverPdu(pdu);
		}

		
		/// <summary>
		/// Converts an an octet sequence encoded using the GSM default
		/// alphabet into a text message string.
		/// </summary>
		/// <param name="octets">
		/// A byte array representing the octet sequence.
		/// </param>
		/// <returns>
		/// The text message if successful; an empty
		/// string otherwise.
		/// </returns>
		private static string DecodeTextMessage(byte[] octets)
		{
			string outputString = String.Empty;

			if (octets != null & octets.GetLength(0) > 0)
			{
				try
				{
					// Allocate space for the output byte string.
					// The output array uses up a greater number 
					// of bytes if the length of the input array 
					// is greater than 7.								
					int outputLength = octets.GetLength(0) + (octets.GetLength(0) / 7);
					byte[] outputBytes = new byte[outputLength];

					int i = 0, j = 0;
					int shlCounter = 1;
					int shrCounter = 7;				
					byte extraBits = (byte)0x00;

					while(i < octets.GetLength(0) && j < outputBytes.GetLength(0))
					{
						// Copy the ith byte from the octet sequence.
						byte currentByte = 0;
						if (i < octets.GetLength(0))					
							currentByte = octets[i];

						// Copy the current byte, this will be used
						// in extracting the hight bits.					
						byte currentByteCopy = currentByte;				

						// Remove bits from the left (high bits)
						// of the current byte's bit sequence. 
						// Note: this can also be accomplished 
						// by masking out the high bits with 0.
						currentByte = (byte)((int)currentByte << shlCounter);
						currentByte = (byte)((int)currentByte >> shlCounter);

						// Append the extra bits (determined from 
						// the previous iteration) to the right 
						// (low bit positions) of the current byte's
						// bit sequence.
						currentByte = (byte)((int)currentByte << (shlCounter - 1));
						currentByte = (byte)(currentByte | extraBits);

						// Extract the extra bits, to be used 
						// in the next iteration.
						extraBits = (byte)((int)currentByteCopy >> shrCounter);

						// Copy the processed byte to the output byte array.
						outputBytes[j] = currentByte;

						// Note: After processing every 7 bytes in the 
						// octet sequence, the bits accumulated in the 
						// "extraBits" variable will form an extra 
						// character.

						if ((i + 1) % 7 != 0)
						{			
							// The next character's position is not 
							// a multiple of 7, continue processing
							// normally.
							shlCounter += 1;
							shrCounter -= 1;
						}
						else 
						{				
							// An extra character is accumulated in the
							// "extra bits" variable. Append it to the
							// output byte array. This will effectively 
							// skip a count in the outputBytes index 
							// counter.
							j += 1;
							outputBytes[j] = extraBits;

							// Clear extra bits. 
							extraBits = 0;

							// Reset shift counters.
							shlCounter = 1;
							shrCounter = 7;																
						}	

						// Move on to the next byte.
						i += 1;
						j += 1;
					}

					// Convert the output byte array into a string.									

					if (outputLength > 0)										
					{
						// Before converting to string, first convert each byte 
						// of outputBytes to Unicode because the bytes are encoded 
						// using the GSM character set.
					
						byte[] b = null;
						StringBuilder buffer = new StringBuilder();
						int index = 0;

						while (index < outputBytes.GetLength(0))
						{						
							if (outputBytes[index] == 27)
							{
								b = new byte[]{outputBytes[index], outputBytes[index + 1]};
								index += 1;
							}
							else
							{
								b = new byte[]{outputBytes[index]};
							}

							char c = CharSetConverter.GsmToUnicode(b);
							buffer.Append(c);

							index += 1;
						}
						
						outputString = buffer.ToString();
					}
					
				}
				catch (Exception ex)
				{
					string errorMsg = ResourceUtils.GetString("ErrorConvertOctet");					
					throw new Exception(errorMsg, ex); 
				}
			}
							
			return outputString;
		}
		
		private static string DecodePhoneNumber(byte[] semiOctets)
		{
			string outputString = String.Empty;

			if (semiOctets != null && semiOctets.GetLength(0) > 0)
			{
				try
				{
					StringBuilder sb = new StringBuilder();

					for (int i = 0; i < semiOctets.GetLength(0); i++)
					{
						byte currentByte = semiOctets[i];
					
						// Each byte holds two decimal digits: 
						// one in bits 7 through 4, and another
						// in bits 3 through 0.
						byte digit1 = (byte)(currentByte & 0x0F);			
						sb.Append(digit1.ToString());
					
						byte digit2 = (byte)(currentByte >> 4);					
						if (digit2 != 0x0F)
							sb.Append(digit2.ToString());
					}
					if (sb.Length > 0)
						outputString = sb.ToString();
				}
				catch (Exception ex)
				{
					string errorMsg = "Cannot decode phone number from semi-octet sequence.";
					throw new ApplicationException(errorMsg, ex);
				}
			}
            
			return outputString;
		}	
	
		
		/// <summary>
		/// Extracts time stamp information from a decimal semi-octet sequence.
		/// </summary>
		/// <param name="semiOctets">
		/// A <see cref="System.Byte"/> array containing the decimal semi-octets 
		/// representing the time stamp.
		/// </param>
		/// <returns>
		/// A <see cref="System.DateTime"/> instance representing the correct 
		/// time stamp if successful; returns <b>DateTime.MinValue</b> otherwise.
		/// </returns>
		private static DateTime DecodeTimeStamp(byte[] semiOctets)
		{
			DateTime timeStamp = DateTime.MinValue;

			if (semiOctets != null && semiOctets.GetLength(0) == 7)
			{
				try
				{
					StringBuilder sb = new StringBuilder();

					// Decode the month.
					byte digit1 = (byte)(semiOctets[1] & 0x0F); 				
					byte digit2 = (byte)(semiOctets[1] >> 4);
					sb.Append(digit1.ToString());
					sb.Append(digit2.ToString());

					sb.Append("/");

					// Decode the day.
					digit1 = (byte)(semiOctets[2] & 0x0F); 				
					digit2 = (byte)(semiOctets[2] >> 4);
					sb.Append(digit1.ToString());
					sb.Append(digit2.ToString());

					sb.Append("/");
				
					// Decode the year.
					digit1 = (byte)(semiOctets[0] & 0x0F); 				
					digit2 = (byte)(semiOctets[0] >> 4);
					sb.Append(digit1.ToString());
					sb.Append(digit2.ToString());

					sb.Append(" ");

					// Decode the hour.
					digit1 = (byte)(semiOctets[3] & 0x0F); 				
					digit2 = (byte)(semiOctets[3] >> 4);
					sb.Append(digit1.ToString());
					sb.Append(digit2.ToString());

					sb.Append(":");

					// Decode the minutes.
					digit1 = (byte)(semiOctets[4] & 0x0F); 				
					digit2 = (byte)(semiOctets[4] >> 4);
					sb.Append(digit1.ToString());
					sb.Append(digit2.ToString());

					sb.Append(":");

					// Decode the seconds.
					digit1 = (byte)(semiOctets[5] & 0x0F); 				
					digit2 = (byte)(semiOctets[5] >> 4);
					sb.Append(digit1.ToString());
					sb.Append(digit2.ToString());				

					// TODO: Add code to process the time zone field.
					// The time zone is contained in semiOctets[6].

					timeStamp = DateTime.Parse(sb.ToString(), new System.Globalization.CultureInfo( "en-US", false ));				
				}
				catch (Exception ex)
				{
					string errorMsg = ResourceUtils.GetString("ErrorDecodeTimeStamp");
					throw new ApplicationException(errorMsg, ex);
				}
			}	
		
			return timeStamp;
		}

		
		#endregion Methods for extracting the contents of an SMS-DELIVER PDU

		#region Utility methods

		private static string FormatPhoneNumber(string phoneNumber, bool prependPlus)
		{
			string phoneNumber1 = "";
			
			if (phoneNumber != null && phoneNumber.Length > 0)
				phoneNumber1 = Regex.Replace(phoneNumber, @"[^\d]", "");

			if (phoneNumber1 != "" && prependPlus)
				phoneNumber1 = "+" + phoneNumber1;
			
			return phoneNumber1;

		}
		
		private static byte[] ConvertToByteArray(string hexString)
		{
			byte[] outputBytes = new byte[0];
			
			if (hexString != null && hexString.Length > 0)
			{
				try
				{
					if (hexString.Length % 2 != 0)
					{
						string errorMsg = ResourceUtils.GetString("ErrorEvenNumber");
						
						throw new Exception(errorMsg);
					}				
				
					outputBytes = new byte[hexString.Length / 2];
					int index = 0;
					for (int i = 0; i < hexString.Length; i += 2)
					{
						string byteString = hexString[i].ToString() + hexString[i + 1].ToString();
						outputBytes[index++] = Byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);                                         
					}
				}
				catch
				{
					string errorMsg = ResourceUtils.GetString("ErrorConvertHexString");
					throw new ApplicationException(errorMsg);
				}
			}

			return outputBytes;
		}

		private static string ConvertToHexString(byte[] data)
		{
			string hexString = String.Empty;

			if (data != null && data.GetLength(0) > 0)
			{
				try
				{
					StringBuilder buffer = new StringBuilder();
					for (int i = 0; i < data.GetLength(0); i++)
						buffer.Append(data[i].ToString("X2"));					
					
					hexString = buffer.ToString();		
				}
				catch
				{
					string errorMsg = ResourceUtils.GetString("ErrorConvertArray");
					throw new ApplicationException(errorMsg);
				}
			}

			return hexString;
		}
		
		
		#endregion Utitility methods
		
		#endregion Methods		
	}

	/// <summary>  
	/// Represents a specialized collection for storing <see cref="SmsMessage"/> objects.
	/// </summary>		
	public class SmsMessageCollection : IEnumerable, ICollection
	{		
		#region Fields

		private ArrayList list = ArrayList.Synchronized(new ArrayList());

		#endregion

		#region Constructor

		/// <summary>		
		/// Initializes a new instance of the <see cref="SmsMessageCollection"/> class.		
		/// </summary>
		public SmsMessageCollection() 
		{
		}		

		#endregion
		
		#region Properties

		/// <summary>
		/// Represents the <see cref="SmsMessage"/> at the specified index.
		/// <para>
		/// In C#, this property is the indexer for the <see cref="SmsMessageCollection"/> 
		/// class.
		/// </para>
		/// </summary>
		/// <param name="index">
		/// The zero-based index of the <see cref="SmsMessage"/> to locate in the collection.
		/// </param>
		/// <value>
		/// The <see cref="SmsMessage"/> at the specified index of the collection.
		/// </value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is outside the valid range of indexes for the collection.
		/// </exception>
		public SmsMessage this[int index] 
		{
			get 
			{
				return ((SmsMessage)(this.list[index]));
			}			
		}

		/// <summary>
		/// Gets the number of <see cref="SmsMessage"/> objects contained in the 
		/// <see cref="SmsMessageCollection"/>.
		/// </summary>
		public int Count
		{
			get { return this.list.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="SmsMessageCollection"/> 
		/// is synchronized (thread-safe).
		/// </summary>
		public bool IsSynchronized
		{
			get { return this.list.IsSynchronized; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="SmsMessageCollection"/>
		/// instance.
		/// </summary>
		public object SyncRoot
		{
			get { return this.list.SyncRoot; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Counts the number of <see cref="SmsMessage"/> objects in the collection
		/// whose <see cref="SmsMessage.Status"/> property matches the specified 
		/// <see cref="MessageStatus"/> value.
		/// </summary>
		/// <param name="status">
		/// The <see cref="MessageStatus"/> of the <see cref="SmsMessage"/> objects
		/// to count.
		/// </param>
		/// <returns>
		/// The number of <see cref="SmsMessage"/> objects in the collection with
		/// the specified <see cref="MessageStatus"/>.
		/// </returns>
		public int CountMessages(MessageStatus status)
		{
			int count = 0;
			
			for (int i = 0; i < this.list.Count; i++)
			{
				if (this[i].Status == status)
					count += 1;
			}

			return count;
		}

		/// <summary>
		/// Gets the <see cref="SmsMessage"/> object located at the specified index 
		/// of the <see cref="SmsMessageCollection"/> instance, and removes it from 
		/// the collection.
		/// </summary>
		public SmsMessage GetMessage(int index)
		{
			SmsMessage message = this[index];
			if (message != null)
				this.Remove(message);

			return message;
		}		

		/// <summary>
		/// Inserts a <see cref="SmsMessage"/> into the <see cref="SmsMessageCollection"/> instance 
		/// at the specified index.
		/// </summary>
		/// <param name="index">
		/// The zero-based index where <paramref name="value"/> should be inserted.
		/// </param>
		/// <param name="message">
		/// The <see cref="SmsMessage"/> to insert.
		/// </param>		
		internal void Insert(int index, SmsMessage message) 
		{
			this.list.Insert(index, message);
		}
		
		/// <summary>
		/// Adds a <see cref="SmsMessage"/> to the end of the <see cref="SmsMessageCollection"/> instance.
		/// </summary>
		/// <param name="message">
		/// The <see cref="SmsMessage"/> instance to add.</param>
		/// <returns>
		/// The index at which the <see cref="SmsMessage"/> was inserted.
		/// </returns>		
		internal int Add(SmsMessage message) 
		{
			return this.list.Add(message);
		}
		
		/// <summary>
		/// Copies the elements of an array of <see cref="SmsMessage"/> objects 
		/// to the end of the <see cref="SmsMessageCollection"/> instance.
		/// </summary>
		/// <param name="messages">
		/// An array of <see cref="SmsMessage"/> objects to add to the collection.
		/// </param>		
		internal void AddRange(SmsMessage[] messages) 
		{
			for (int i = 0; i < messages.Length; i++) 
			{
				this.list.Add(messages[i]);
			}
		}		

		/// <summary>
		/// Removes the specified <see cref="SmsMessage"/> from the <see cref="SmsMessageCollection"/> 
		/// instance.
		/// </summary>
		/// <param name="message">
		/// The <see cref="SmsMessage"/> to remove from the <see cref="SmsMessageCollection"/> instance.
		/// </param>		
		/// <exception cref="System.ArgumentException">
		/// <paramref name="message"/> is not found in the Collection. 
		/// </exception>
		public void Remove(SmsMessage message) 
		{
			this.list.Remove(message);
		}	

		/// <summary>
		/// Removes the <see cref="SmsMessage"/> at the specified index from the 
		/// <see cref="SmsMessageCollection"/> instance.
		/// </summary>
		/// <param name="index">
		/// The index of the <see cref="SmsMessage"/> to remove from the 
		/// <see cref="SmsMessageCollection"/> instance.
		/// </param>		
		/// <exception cref="System.ArgumentException">
		/// There is no <see cref="SmsMessage"/> object at the specified index 
		/// of the collection. 
		/// </exception>
		public void RemoveAt(int index) 
		{
			this.list.RemoveAt(index);
		}
	
		/// <summary>
		/// Removes all <see cref="SmsMessage"/> objects contained in the <see cref="SmsMessageCollection"/>
		/// instance.
		/// </summary>
		public void Clear()
		{
			this.list.Clear();
		}
		
		/// <summary>
		/// Gets a value indicating whether the <see cref="SmsMessageCollection"/> instance contains 
		/// the specified <see cref="SmsMessage"/>.
		/// </summary>
		/// <param name="message">
		/// The <see cref="SmsMessage"/> to locate.
		/// </param>
		/// <returns>
		/// <b>true</b> if the <see cref="SmsMessage"/> is contained in the collection; 
		/// <b>false</b> otherwise.
		/// </returns>		
		public bool Contains(SmsMessage message) 
		{
			return this.list.Contains(message);
		}
		
		/// <summary>
		/// Copies the elements of <see cref="SmsMessageCollection"/> instance to a one-dimensional 
		/// <see cref="System.Array"/> starting at the specified index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="System.Array"/> that is the destination of the elements 
		/// copied from <see cref="SmsMessageCollection"/>.
		/// </param>
		/// <param name="index">
		/// The index in <paramref name="array"/> where copying begins.
		/// </param>		
		/// <exception cref="System.ArgumentException">
		/// <paramref name="array"/> is multidimensional, or the number of elements in the 
		/// <see cref="SmsMessageCollection"/> is greater than the available space between 
		/// <paramref name="index"/> and the end of <paramref name="array"/>.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <b>null</b>. 
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than <paramref name="array"/>'s lower bound.
		/// </exception>		
		public void CopyTo(Array array, int index) 
		{
			this.list.CopyTo(array, index);
		}		
		
		/// <summary>
		/// Returns the index of the specified <see cref="SmsMessage"/> object in the 
		/// <see cref="SmsMessageCollection"/> instance.
		/// </summary>
		/// <param name="message">
		/// The <see cref="SmsMessage"/> to locate.
		/// </param>
		/// <returns>
		/// The index of the specified <paramref name="message"/> in the <see cref="SmsMessageCollection"/>, 
		/// if found; otherwise, -1.
		/// </returns>		
		public int IndexOf(SmsMessage message) 
		{
			return this.list.IndexOf(message);
		}		
		
		/// <summary>
		/// Returns a generic enumerator that can be used to iterate through the elements of the
		/// <see cref="SmsMessageCollection"/> instance.
		/// </summary>		
		IEnumerator IEnumerable.GetEnumerator() 
		{			
			return this.list.GetEnumerator();
		}		
		
		#endregion
	}
	
	internal class CharSetConverter
	{
		#region Fields

		private static ushort[] gsmToUnicodeTable = new ushort[]{
													 64, 163,  36, 165, 232, 233, 249, 236, 242, 199,
                                                     10, 216, 248,  13, 197, 229, 916,  95, 934, 915,
													923, 937, 928, 936, 931, 920, 926,  27, 198, 230,
													223, 201,  32,  33,  34,  35, 164,  37,  38,  39,
													 40,  41,  42,  43,  44,  45,  46,  47,  48,  49,
													 50,  51,  52,  53,  54,  55,  56,  57,  58,  59,
													 60,  61,  62,  63, 161,  65,  66,  67,  68,  69, 
													 70,  71,  72,  73,  74,  75,  76,  77,  78,  79,
													 80,  81,  82,  83,  84,  85,  86,  87,  88,  89,
													 90, 196, 214, 209, 220, 167, 191,  97,  98,  99,
													100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
													110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
													120, 121, 122, 228, 246, 241, 252, 224};


		#endregion

		#region Constructor

		// Do not allow the creation of this class.
		private CharSetConverter()
		{		
		}

		#endregion

		#region Methods

		/// <summary>
		/// Converts a Unicode character to its corresponding representation 
		/// in the GSM character set.
		/// </summary>
		/// <param name="c">
		/// The Unicode character to convert. Note: the escape character
		/// (U+001B) is "illegal" because it has a special meaning in the
		/// GSM character set; it is automatically converted to the space 
		/// character (U+0020).
		/// </param>
		/// <returns>
		/// A <see cref="Byte"/> array containing the GSM representation of 
		/// the Unicode character.
		/// </returns>
		/// <remarks>
		/// Depending on the Unicode character to be converted, the 
		/// <b>UnicodeToGsm</b> method may return either a single-element 
		/// or a two-element byte array. For "normal" characters such as 
		/// A...Z, a...z, 0...9 the <b>UnicodeToGsm</b> returns a single-element
		/// byte array; for "extended" characters such as Greek letters, 
		/// currency symbols, etc. the <b>UnicodeToGsm</b> method returns a 
		/// two-element byte array.
		/// </remarks>
		public static byte[] UnicodeToGsm(char c)
		{
			byte[] outputBytes = new byte[0];			

			ushort charValue = Convert.ToUInt16(c);
			
			if ((charValue >= 32 && charValue <= 63) ||
				(charValue >= 65 && charValue <= 90) ||
				(charValue >= 97 && charValue <= 122))
			{
				// If the Unicode character's numeric value falls in 
				// this range then the corresponding GSM char set 
				// value is the same as the Unicode value, converted 
				// to byte. 
				// Note: The character's numeric value also coincides 
				// with its ASCII value.
				outputBytes = new byte[]{Convert.ToByte(charValue)};
			}
			else
			{
				switch (charValue)
				{
					// Unicode escape character (0x001B) not allowed
					// -- replace with space character.
					case 27:
						outputBytes = new byte[]{32};
						break;

					// Inverted exclamation mark.
					case 161:
						outputBytes = new byte[]{64};
						break;

					case 12:
						outputBytes = new byte[]{27, 10};
						break;

					case 94:
						outputBytes = new byte[]{27, 20};						
						break;

					case 123:
						outputBytes = new byte[]{27, 40};						
						break;

					case 125:
						outputBytes = new byte[]{27, 41};						
						break;

					case 92:
						outputBytes = new byte[]{27, 47};						
						break;

					case 91:
						outputBytes = new byte[]{27, 60};						
						break;

					case 126:
						outputBytes = new byte[]{27, 61};						
						break;

					case 93:
						outputBytes = new byte[]{27, 62};												
						break;

					case 124:
						outputBytes = new byte[]{27, 64};												
						break;

					case 8364:
						outputBytes = new byte[]{27, 101};												
						break;
				
					default:
						
						bool found = false;					
						for (int i = 0; i <= 31; i++)
						{						
							if (gsmToUnicodeTable[i] == charValue)
							{
								outputBytes = new byte[]{Convert.ToByte(i)};
								found = true;
								break;
							}
						}

						if (!found)
						{
							for (int i = 91; i <= 96; i++)
							{
								if (gsmToUnicodeTable[i] == charValue)
								{
									outputBytes = new byte[]{Convert.ToByte(i)};
									found = true;
									break;
								}
							}
						}

						if (!found)
						{
							for (int i = 123; i <= 127; i++)
							{
								if (gsmToUnicodeTable[i] == charValue)
								{
									outputBytes = new byte[]{Convert.ToByte(i)};
									found = true;
									break;
								}
							}
						}

						if (!found)
						{
							// The character cannot be converted,
							// replace with space character.
							outputBytes = new byte[]{32};
						}

						break;
				}
			}

			return outputBytes;
		}

		/// <summary>
		/// Converts a byte sequence representing a GSM character into 
		/// its equivalent Unicode character.
		/// </summary>
		/// <param name="b">
		/// A <see cref="Byte"/> array containing the GSM character to 
		/// convert. Note: A GSM character is represented by either one 
		/// or two bytes. "Normal" GSM characters such as A...Z, a...z, 
		/// 0...9 occupy a single byte; "extended" characters such as 
		/// Greek letters, currency symbols, etc. occupy two bytes.	
		/// </param>
		/// <returns>
		/// The Unicode character equivalent of the supplied GSM character.
		/// </returns>
		public static char GsmToUnicode(byte[] b)
		{
			char outputChar = ' ';

			if (b != null && b.GetLength(0) > 0)
			{
				if (b.GetLength(0) == 1)
				{
					byte firstByte = b[0];
					outputChar = Convert.ToChar(gsmToUnicodeTable[(int)firstByte]);					
				}
				else if (b.GetLength(0) == 2 && b[0] == 27)
				{
					byte secondByte = b[1];
					switch (secondByte)
					{
						case 10:
							outputChar = (char)12;
							break;
						
						case 20:
							outputChar = (char)94;
							break;
						
						case 40:
							outputChar = (char)123;
							break;

						case 41:
							outputChar = (char)125;
							break;

						case 47:
							outputChar = (char)92;
							break;

						case 60:
							outputChar = (char)91;
							break;

						case 61:
							outputChar = (char)126;
							break;

						case 62:
							outputChar = (char)93;
							break;

						case 64:
							outputChar = (char)124;
							break;

						case 101:
							outputChar = (char)8364;
							break;
					}
				}

			}
			
			return outputChar;
		}

		#endregion
	}
}
