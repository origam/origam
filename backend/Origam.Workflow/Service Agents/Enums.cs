#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

namespace GsmLink;

/// <summary>
/// Specifies the method to use in encoding the data contained in the 
/// <see cref="SmsMessage.Body"/> property of an <see cref="SmsMessage"/> 
/// instance.
/// </summary>
public enum EncodingScheme
{
	/// <summary>
	/// Specifies that the data is encoded as text, using the GSM 7-bit 
	/// default character set.
	/// </summary>
	Text,		
	/// <summary>
	/// Specifies that the data is encoded as 8-bit binary data.
	/// </summary>
	Data,
	/// <summary>
	/// Specifies that the type of encoding is unknown.
	/// </summary>
	Unknown
}

/// <summary>
/// Specifies the type of the SMS message.
/// </summary>
public enum MessageType
{
	/// <summary>
	/// Specifies that the SMS message originated from the SMSC 
	/// and delivered to the GSM device.
	/// </summary>
	SmsDeliver, 
	/// <summary>
	/// Specifies that the SMS message originated from the GSM device 
	/// and submitted to the SMSC for delivery to another GSM device.
	/// </summary>
	SmsSubmit, 
//		/// <summary>
//		/// The SMS message is a SMS command message.
//		/// </summary>		
//		SmsCommand,
	/// <summary>
	/// Specifies that the SMS message is of unknown type.
	/// </summary>
	Unknown
}
	
/// <summary>
/// Specifies the synchronization options used by the <see cref="GsmDevice"/>
/// class.
/// </summary>
/// <remarks>
/// This enumeration is a bit field.
/// </remarks>
[Flags]
public enum SyncOptions
{
	/// <summary>
	/// Specifies that synchronization is turned off.
	/// </summary>
	None = 0x00,
	/// <summary>
	/// Specifies that the <see cref="GsmDevice"/> will continuously poll 
	/// the physical GSM device for new messages, send queued messages,
	/// and check the status of the GSM device.
	/// </summary>
	AutoSendReceive = 0x01,
	/// <summary>
	/// Specifies that when <see cref="GsmDevice"/> copies new SMS messages 
	/// from the physical GSM device to its internal message caches (i.e. 
	/// Inbox, Outbox, Sent), the original SMS messages left in the GSM device 
	/// are deleted.
	/// </summary>
	AutoDelete = 0x02,
	/// <summary>
	/// Specifies that the <see cref="GsmDevice"/> will raise events.
	/// </summary>
	AutoNotify = 0x04,
	/// <summary>
	/// Specifies that the <see cref="GsmDevice"/> will automatically 
	/// save the contents of its internal message caches (i.e. Inbox, Outbox, 
	/// Sent) using a <see cref="GsmLink.Persistence.PersistenceProvider"/>.
	/// </summary>
	AutoPersist = 0x08,
	/// <summary>
	/// Specifies that all options are turned on.
	/// </summary>
	All = AutoSendReceive | AutoDelete | AutoNotify | AutoPersist
}

public enum MessageStorage
{		
	Both,
	Device,
	Sim
}

public enum MessageStatus
{
	Unread,
	Read,
	Unsent,
	Sent,
	All,
	Unknown
}
	
internal enum DataBits
{
	Five = 5,
	Six = 6,
	Seven = 7,
	Eight = 8
}

	
internal enum Parity 
{
	None,			
	Odd,						
	Even,			
	Mark,			
	Space
}

	
internal enum StopBits
{			
	One, 
	OnePointFive,			
	Two
}

	
internal enum FlowControl 
{
	None, 
	XonXoff, 
	CtsRts,
	CtsDtr,		
	DsrRts,
	DsrDtr
}