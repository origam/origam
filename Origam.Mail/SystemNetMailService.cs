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

using System;
using System.Text;
using System.Xml;
using System.Net.Mail;
using System.Xml.XPath;
using System.Net.Mime;

namespace Origam.Mail
{
    public class SystemNetMailService : AbstractMailService
    {
        public SystemNetMailService()
        {
        }

        public override int SendMail1(IXmlContainer mailDocument, string server, int port)
        {
            //return Value positive number (include 0zero) indicates OK result, negative -1 means error
            int retVal = 0;

            //local variables
            SmtpClient s = new SmtpClient();

            //configure smtp server parameters if given, otherwise use web config settings
            if (server != null)
            {
                s.Host = server;
                s.Port = port;
                s.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            }

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
                MailMessage m = new MailMessage();                
                     
                //put mail header info
                m.BodyEncoding = Encoding.UTF8;
                m.Subject = GetValue(mailRoot, nsmgr, "m:Subject");
                m.From = new MailAddress(GetValue(mailRoot, nsmgr, "m:From/m:Address"), GetValue(mailRoot, nsmgr, "m:From/m:Name"));
                m.Headers.Add("X-OrigamEmailIdentifier", GetValue(mailRoot, nsmgr, "m:MessageIdentifier"));
                

                //load recipient list
                XmlNodeList recipientList;
                recipientList = mailRoot.SelectNodes("m:To/m:EmailAddress", nsmgr);
                foreach (XmlNode recipientRoot in recipientList)
                {
                    m.To.Add(new MailAddress(GetValue(recipientRoot, nsmgr, "m:Address"), GetValue(recipientRoot, nsmgr, "m:Name")));
                }

                //put html body inside
                m.Body = GetValue(mailRoot, nsmgr, "m:Body");
                m.IsBodyHtml = true;


                try
                {
                    s.Send(m);
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
            //return Value positive number (include 0zero) indicates OK result, negative -1 means error
            int retVal = 0;

            //local variables
            SmtpClient s = new SmtpClient();

            //configure smtp server parameters if given, otherwise use web config settings
            if (! string.IsNullOrEmpty(server))
            {
                s.Host = server;
                s.Port = port;
                s.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            }

            //send one mail per Mail section
            foreach (MailData.MailRow mailrow in mailData.Mail.Rows)
            {
                MailMessage m = new MailMessage();

                //put mail header info
                m.Subject = mailrow.Subject;
                m.BodyEncoding = Encoding.UTF8;
                string fromName = "";
                string fromAddress = "";
                OpenPOP.MIMEParser.Utility.ParseEmailAddress(mailrow.Sender, ref fromName, ref fromAddress);
                m.From = new MailAddress(fromAddress, fromName);
                m.Headers.Add("X-OrigamEmailIdentifier", mailrow.Id.ToString());

                string[] to = mailrow.Recipient.Split(";".ToCharArray());

                foreach (string recipient in to)
                {
                    string toName = "";
                    string toAddress = "";
                    OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref toName, ref toAddress);

                    if (recipient != null & recipient != String.Empty) m.To.Add(new MailAddress(toAddress, toName));
                }

                string[] cc = null;
                if (!mailrow.IsCCNull())
                {
                    cc = mailrow.CC.Split(";".ToCharArray());

                    foreach (string recipient in cc)
                    {
                        string ccName = "";
                        string ccAddress = "";
                        OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref ccName, ref ccAddress);

                        if (recipient != null & recipient != String.Empty) m.CC.Add(new MailAddress(ccAddress, ccName));
                    }
                }

                string[] bcc = null;
                if (!mailrow.IsBCCNull())
                {
                    bcc = mailrow.BCC.Split(";".ToCharArray());

                    foreach (string recipient in bcc)
                    {
                        string bccName = "";
                        string bccAddress = "";
                        OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref bccName, ref bccAddress);

                        if (recipient != null & recipient != String.Empty) m.Bcc.Add(new MailAddress(bccAddress, bccName));
                    }
                }

                //put html body inside
                if (mailrow.MessageBody.StartsWith("<"))
                {
                    AlternateView plainTextView =
                        AlternateView.CreateAlternateViewFromString(
                            HtmlToText(mailrow.MessageBody), null, MediaTypeNames.Text.Plain);
                    m.AlternateViews.Add(plainTextView);
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                        mailrow.MessageBody, null, MediaTypeNames.Text.Html);
                    m.AlternateViews.Add(htmlView);                    
                }
                else
                {
                    m.Body = mailrow.MessageBody;
                    m.IsBodyHtml = false;
                }


                foreach (MailData.MailAttachmentRow attachment in mailrow.GetMailAttachmentRows())
                {
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(attachment.Data, false);
                    Attachment att = new Attachment(stream, attachment.FileName);
                    m.Attachments.Add(att);
                }

                s.Send(m);
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
