#region NANDO FLORESTAN LIBRARY - SendMail
// Copyright (C) 2005  Nando Florestan
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License VERSION 2
// as published by the Free Software Foundation.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, read it on the web:
// http://www.gnu.org/copyleft/gpl.html

/*
 * Created in SharpDevelop: http://www.icsharpcode.net/OpenSource/SD/
 * Author: Nando Florestan: http://oui.com.br/n/
 */
#endregion

#region using
using            Exception  = System.Exception;
using ApplicationException  = System.ApplicationException;
using ArgumentNullException = System.ArgumentNullException;
using Encoding              = System.Text.Encoding;
using Serializable          = System.SerializableAttribute;
using StringBuilder         = System.Text.StringBuilder;
using MailMessage           = System.Web.Mail.MailMessage;
using SmtpMail              = System.Web.Mail.SmtpMail;
using NandoF.Data;
#endregion

namespace NandoF.Mail
{
	/// <summary>DEPRECATED on 2005-10-06. TODO: Remove after 6 months</summary>
	[Serializable] public class SendMailAccount
	{
		protected string     smtpServer   = string.Empty;
		public    string     SmtpServer  {
			get  { return smtpServer;  }
			set  { smtpServer = value; }
		}
		
		protected string     smtpUser     = string.Empty;
		public    string     SmtpUser {
			get { return smtpUser;  }
			set { smtpUser = value; }
		}
		
		protected string     smtpPassword = string.Empty;
		public    string     SmtpPassword {
			get  { return smtpPassword;  }
			set  { smtpPassword = value; }
		}
		
		protected NamedEmail sender;
		public    NamedEmail Sender  {
			get  { return sender; }
			set  {
				if (value==null)     throw new ArgumentNullException("Sender");
				if (!value.IsValid)  throw new ApplicationException
					("Invalid e-mail address for sender");
				sender = value;
			}
		}
		
		protected NamedEmail replyTo;
		public    NamedEmail ReplyTo  {
			get  { return replyTo;  }
			set  { replyTo = value; }
		}
	}
	
	
	
	public class SendMail : System.ICloneable
	{
		const string WEIRD = "http://schemas.microsoft.com/cdo/configuration/";
		
		protected MailSmtpAccount account = new MailSmtpAccount();
		public    MailSmtpAccount Account  {
			get { return account;  }
			set { account = value; }
		}
		
		protected MailMessage message = new System.Web.Mail.MailMessage();
		
		protected NamedEmail  recipient;
		public    NamedEmail  Recipient  {
			get { return recipient;  }
			set { recipient = value; }
		}
		
		protected NamedEmail  carbonCopyRecipient;
		public    NamedEmail  CarbonCopyRecipient {
			get  { return carbonCopyRecipient;  }
			set  { carbonCopyRecipient = value; }
		}
		
		public    string      Subject  {
			get { return message.Subject;  }
			set { message.Subject = value; }
		}
		
		public    Encoding    ContentEncoding  {
			get  { return message.BodyEncoding;  }
			set  { message.BodyEncoding = value; }
		}
		
		public    void        SetContent(string content, bool htmlFormat)  {
			if (content==null || content==string.Empty)
				throw new ApplicationException("Content cannot be empty.");
			message.Body = content;
			if (htmlFormat) message.BodyFormat = System.Web.Mail.MailFormat.Html;
			else            message.BodyFormat = System.Web.Mail.MailFormat.Text;
		}
		
		protected void        SetItUp()  {
			if (Account.Sender==null) throw new
				ApplicationException("Sender cannot be blank.");
			if (Recipient==null) throw new
				ApplicationException("Recipient cannot be blank.");
			SmtpMail.SmtpServer = Account.Host;
			message .From       = Account.Sender.ToString();
			message .To         = Recipient     .ToString();
			if (CarbonCopyRecipient != null)
				message.Cc = CarbonCopyRecipient.ToString();
			if (Account.ReplyTo != null)  {
				message.Headers.Clear();
				message.Headers.Add("Reply-To", Account.ReplyTo.ToString());
			}
			if (Account.User != null && Account.User != string.Empty)  {
				message.Fields.Clear();
				// basic authentication
				message.Fields.Add(WEIRD + "smtpauthenticate", "1");
				message.Fields.Add(WEIRD + "sendusername", Account.User);
				message.Fields.Add(WEIRD + "sendpassword", Account.Password);
			}
		}
		
		public    void        Zend(NamedEmail recipient)  {
			if (recipient==null)     throw new ArgumentNullException("Recipient");
			if (!recipient.IsValid)  throw new ApplicationException
				("Invalid e-mail address for recipient: " + recipient.ToString());
			Recipient = recipient;
			SetItUp();
			SmtpMail.Send(message);
		}
		
		// ICloneable implementation
		public    object      Clone()  {
			// It is important to "new SendMail()" because a new MailMessage is
			// created. And the MailMessage contains the recipient. This way
			// we can send to multiple recipients at the same time.
			SendMail clone            = new SendMail();
			clone.Account             = this.Account;
			clone.Recipient           = this.Recipient;
			clone.CarbonCopyRecipient = this.CarbonCopyRecipient;
			clone.Subject             = this.Subject;
			clone.ContentEncoding     = this.ContentEncoding;
			clone.SetContent(this.message.Body,
			                 (message.BodyFormat==System.Web.Mail.MailFormat.Html));
			return clone;
		}
	}
}
