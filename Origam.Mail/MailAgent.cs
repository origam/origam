using System;
using OpenSmtp.Mail;
using System.Xml;
using System.Xml.XPath;
using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Origam.Mail
{
	/// <summary>
	/// MailAgent sends mails based on AsMail.xsd schema
	/// </summary>
	public class MailAgent
	{
		public MailAgent()
		{
			//load configuration
			ConfigurationManager.Read("MailAgentConfig");
		}

		public int SendMail(XmlDocument mailDocument)
		{
			//return Value positive number (include 0zero) indicates OK result, negative -1 means error
			int retVal=0;
            
			//local variables
			OpenSmtp.Mail.Smtp s = new Smtp();
			OpenSmtp.Mail.MailMessage m = new MailMessage();

			//configure smtp server parameters
			s.Host = (string)ConfigurationManager.Items["SmtpHost"];
			s.Port = Convert.ToInt32(ConfigurationManager.Items["SmtpPort"]);

			//get root (Mails) element
			XmlElement root = mailDocument.DocumentElement;

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
				//put mail header info
				m.Subject = GetValue(mailRoot, nsmgr, "m:Subject");
				m.Charset = "utf-8";
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
	
		private string GetValue(XmlNode mailRoot, XmlNamespaceManager nsmgr, string where)
		{
			return mailRoot.SelectSingleNode(where, nsmgr).InnerXml;
		}
	}
}
	