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

using System;
using System.Text;
using System.Xml;
using System.Net.Mail;
using System.Xml.XPath;
using System.Net.Mime;
using System.Net;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Mail;

public class NetStandardMailService : AbstractMailService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
    private readonly string username;
    private readonly string password;
    private readonly bool useSsl;
    private readonly string defaultServer;
    private readonly int defaultPort;
    private readonly string pickupDirectoryLocation;

    protected NetStandardMailService()
    {
        }

    public NetStandardMailService(
        string server, int port, string pickupDirectoryLocation,
        string username = null, string password = null, bool useSsl = true)
    {
            if(!string.IsNullOrWhiteSpace(pickupDirectoryLocation)
            && !string.IsNullOrWhiteSpace(server))
            {
                throw new ArgumentException(
                    "It is not possible to specify both server and pickup directory.");
            }
            if (string.IsNullOrWhiteSpace(pickupDirectoryLocation))
            {
                if (string.IsNullOrWhiteSpace(password) 
                    && !string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException(nameof(password) 
                        + " cannot be empty if fromAddress is not empty");
                }
                if (string.IsNullOrWhiteSpace(server))
                {
                    throw new ArgumentException(nameof(server)
                        + " cannot be empty");
                }
            }
            this.username = username;
            this.password = password;
            this.useSsl = useSsl;
            defaultServer = server;
            defaultPort = port;
            this.pickupDirectoryLocation = pickupDirectoryLocation;
        }

    public override int SendMail1(IXmlContainer mailDocument, string server, int port)
    {
            //return Value positive number (include 0zero) indicates OK result, negative -1 means error
            int retVal = 0;

            var smtpClient = BuildSmtpClient(server, port);

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
                    MailLogUtils.SendMessageAndLog(smtpClient, m);
                    retVal++;
                }
                catch(Exception ex)
                {
                    log.LogOrigamError(ex);
                    throw;
                }
            }
            return retVal;
        }

    public override int SendMail2(MailData mailData, string server, int port)
    {
            //return Value positive number (include 0zero) indicates OK result, negative -1 means error
            int retVal = 0;

            var smtpClient = BuildSmtpClient(server, port);

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
                    if (!string.IsNullOrEmpty(recipient))
                    {
                        m.To.Add(new MailAddress(toAddress, toName));
                    }
                }

                if (!mailrow.IsCCNull())
                {
                    var cc = mailrow.CC.Split(";".ToCharArray());
                    foreach (string recipient in cc)
                    {
                        string ccName = "";
                        string ccAddress = "";
                        OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref ccName, ref ccAddress);
                        if (!string.IsNullOrEmpty(recipient))
                        {
                            m.CC.Add(new MailAddress(ccAddress, ccName));
                        }
                    }
                }

                if (!mailrow.IsBCCNull())
                {
                    var bcc = mailrow.BCC.Split(";".ToCharArray());
                    foreach (string recipient in bcc)
                    {
                        string bccName = "";
                        string bccAddress = "";
                        OpenPOP.MIMEParser.Utility.ParseEmailAddress(recipient, ref bccName, ref bccAddress);
                        if (!string.IsNullOrEmpty(recipient))
                        {
                            m.Bcc.Add(new MailAddress(bccAddress, bccName));
                        }
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
                        WebUtility.HtmlDecode(mailrow.MessageBody), null, MediaTypeNames.Text.Html);
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

                try
                {
                    MailLogUtils.SendMessageAndLog(smtpClient, m);
                    retVal++;
                }
                catch (Exception ex)
                {
                    log.LogOrigamError(ex);
                    throw;
                }
            }
            return retVal;
        }

    private SmtpClient BuildSmtpClient(string server, int port)
    {
            var smtpClient = new SmtpClient();
            if (!string.IsNullOrEmpty(server))
            {
                smtpClient.Host = server;
                smtpClient.Port = port;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            }
            else if (!string.IsNullOrWhiteSpace(pickupDirectoryLocation))
            {
                smtpClient.PickupDirectoryLocation = pickupDirectoryLocation;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtpClient.EnableSsl = false;
            }
            else
            {
                SetConfigValues(smtpClient);
            }
            return smtpClient;
        }

    protected virtual void SetConfigValues(SmtpClient smtpClient)
    {
            smtpClient.Host = defaultServer;
            smtpClient.Port = defaultPort;
            smtpClient.EnableSsl = useSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(username, password);
        }
}