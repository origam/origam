#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using OpenSmtp.Mail;
using System.Xml;
using System.Xml.XPath;
//using OpenPOP.POP3;

//using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Origam.Mail
{
	/// <summary>
	/// MailAgent sends mails based on AsMail.xsd schema
	/// </summary>
	public class OpenSmtpMailService : AbstractMailService
	{
		public OpenSmtpMailService()
		{
		}		

		public override int SendMail1(IXmlContainer mailDocument, string server, int port)
		{
            if (server == null)
            {
                throw new ArgumentException(ResourceUtils.GetString("ErrorSmtpServerMissing"));
            }

			//return Value positive number (include 0zero) indicates OK result, negative -1 means error
			int retVal=0;
            
			//local variables
			OpenSmtp.Mail.Smtp s = new Smtp();

			//configure smtp server parameters
			s.Host = server;
			s.Port = port;

			//get root (Mails) element
			XmlElement root = mailDocument.Xml.DocumentElement;

			//configure xsd namespace
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(((XPathNavigator)root.CreateNavigator()).NameTable);
			//all tags have to begin with m: prefix
			nsmgr.AddNamespace("m", "http://schema.advantages.cz/AsMail.xsd"); //default namespace

			//get Mail nodes
			XmlNodeList mailList;
			mailList = root.SelectNodes("/m:Mails/m:Mail", nsmgr);

			//send one mail per Mail section
			foreach (XmlNode mailRoot in mailList)
			{
				OpenSmtp.Mail.MailMessage m = new MailMessage();

				//put mail header info
				m.Charset = "utf-8";
				m.Subject = GetValue(mailRoot, nsmgr, "m:Subject");
				m.From = new EmailAddress(GetValue(mailRoot, nsmgr, "m:From/m:Address"), GetValue(mailRoot, nsmgr, "m:From/m:Name"));
				m.AddCustomHeader("X-OrigamEmailIdentifier", GetValue(mailRoot, nsmgr, "m:MessageIdentifier"));

				//load recipient list
				XmlNodeList recipientList;
				recipientList = mailRoot.SelectNodes("m:To/m:EmailAddress", nsmgr);
				foreach (XmlNode recipientRoot in recipientList)
				{
					m.To.Add(new EmailAddress(GetValue(recipientRoot, nsmgr, "m:Address"), GetValue(recipientRoot, nsmgr, "m:Name")));
				}

				//put html body inside
				m.HtmlBody = GetValue(mailRoot, nsmgr, "m:Body");
	
				try
				{
					s.SendMail(m);
					retVal++;
				}
				catch
				{
					retVal = -1;
				}

				/// po uspesnem odeslani mailu posleme zpet domluveny fragment s klicem,
				/// podle ktereho bude proveden update logu se statusem a casem odeslaneho mailu
				/// .
				/// Temito vysledky bude naplnen dataset, na nej dan data adapter a bude
				/// proveden update.
			}
			return retVal;
		}
		public override int SendMail2(MailData mailData, string server, int port)
		{
            // for opensmtp client the server name is mandatory.
            if (server == null)
            {
                throw new ArgumentException(ResourceUtils.GetString("ErrorSmtpServerMissing"));
            }

			//return Value positive number (include 0zero) indicates OK result, negative -1 means error
			int retVal=0;
            
			//local variables
			OpenSmtp.Mail.Smtp s = new Smtp();

			//configure smtp server parameters
			s.Host = server;
			s.Port = port;

			//send one mail per Mail section
			foreach (MailData.MailRow mailrow in mailData.Mail.Rows)
			{
				OpenSmtp.Mail.MailMessage m = new MailMessage();

				//put mail header info
				m.Subject = mailrow.Subject;
				m.Charset = "utf-8";
				string fromName = "";
				string fromAddress = "";
				OpenPOP.MIMEParser.Utility.ParseEmailAddress(mailrow.Sender, ref fromName, ref fromAddress);
				m.From = new EmailAddress(fromAddress, fromName);
				m.AddCustomHeader("X-OrigamEmailIdentifier", mailrow.Id.ToString());

				string[] to = mailrow.Recipient.Split(";".ToCharArray());

				foreach(string recipient in to)
				{
					string toName = "";
					string toAddress = "";
					OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref toName, ref toAddress);

					if(recipient != null & recipient != String.Empty) m.To.Add(new EmailAddress(toAddress, toName));
				}
				
				string[] cc = null; 
				if(!mailrow.IsCCNull())
				{
					cc = mailrow.CC.Split(";".ToCharArray());

					foreach(string recipient in cc)
					{
						string ccName = "";
						string ccAddress = "";
						OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref ccName, ref ccAddress);

						if(recipient != null & recipient != String.Empty) m.CC.Add(new EmailAddress(ccAddress, ccName));
					}
				}

				string[] bcc = null;
				if(!mailrow.IsBCCNull())
				{
					bcc = mailrow.BCC.Split(";".ToCharArray());

					foreach(string recipient in bcc)
					{
						string bccName = "";
						string bccAddress = "";
						OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref bccName, ref bccAddress);

						if(recipient != null & recipient != String.Empty) m.BCC.Add(new EmailAddress(bccAddress, bccName));
					}
				}

				//put html body inside
				if(mailrow.MessageBody.StartsWith("<"))
				{
					m.HtmlBody = mailrow.MessageBody;
					m.Body = HtmlToText(mailrow.MessageBody);
				}
				else
				{
					m.Body = mailrow.MessageBody;
				}
				
	
				foreach(MailData.MailAttachmentRow attachment in mailrow.GetMailAttachmentRows())
				{
					System.IO.MemoryStream stream = new System.IO.MemoryStream(attachment.Data, false);
					
					OpenSmtp.Mail.Attachment att = new Attachment(stream, attachment.FileName);

					m.AddAttachment(att);
				}

				s.SendMail(m);
				retVal++;

				/// po uspesnem odeslani mailu posleme zpet domluveny fragment s klicem,
				/// podle ktereho bude proveden update logu se statusem a casem odeslaneho mailu
				/// .
				/// Temito vysledky bude naplnen dataset, na nej dan data adapter a bude
				/// proveden update.
			}
			return retVal;
		} 
	}
}
	