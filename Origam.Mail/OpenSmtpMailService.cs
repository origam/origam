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

		public override int SendMail(IXmlContainer mailDocument, string server, int port)
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
	}
}
	