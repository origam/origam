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
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Mail;

public class NetStandardMailService : AbstractMailService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    private readonly string username;
    private readonly string password;
    private readonly bool useSsl;
    private readonly string defaultServer;
    private readonly int defaultPort;
    private readonly string pickupDirectoryLocation;

    protected NetStandardMailService() { }

    public NetStandardMailService(
        string server,
        int port,
        string pickupDirectoryLocation,
        string username = null,
        string password = null,
        bool useSsl = true
    )
    {
        if (
            !string.IsNullOrWhiteSpace(value: pickupDirectoryLocation)
            && !string.IsNullOrWhiteSpace(value: server)
        )
        {
            throw new ArgumentException(
                message: "It is not possible to specify both server and pickup directory."
            );
        }
        if (string.IsNullOrWhiteSpace(value: pickupDirectoryLocation))
        {
            if (
                string.IsNullOrWhiteSpace(value: password)
                && !string.IsNullOrWhiteSpace(value: username)
            )
            {
                throw new ArgumentException(
                    message: nameof(password) + " cannot be empty if fromAddress is not empty"
                );
            }
            if (string.IsNullOrWhiteSpace(value: server))
            {
                throw new ArgumentException(message: nameof(server) + " cannot be empty");
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
        var smtpClient = BuildSmtpClient(server: server, port: port);
        //get root (Mails) element
        XmlElement root = mailDocument.Xml.DocumentElement;
        //configure xsd namespace
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(
            nameTable: ((XPathNavigator)root.CreateNavigator()).NameTable
        );
        //all tags have to begin with m: prefix
        nsmgr.AddNamespace(prefix: "m", uri: "http://schema.advantages.cz/AsMail.xsd"); //default namespace
        //get Mail nodes
        XmlNodeList mailList;
        mailList = root.SelectNodes(xpath: "/m:Mails/m:Mail", nsmgr: nsmgr);
        //send one mail per Mail section
        foreach (XmlNode mailRoot in mailList)
        {
            MailMessage m = new MailMessage();

            //put mail header info
            m.BodyEncoding = Encoding.UTF8;
            m.Subject = GetValue(mailRoot: mailRoot, nsmgr: nsmgr, where: "m:Subject");
            m.From = new MailAddress(
                address: GetValue(mailRoot: mailRoot, nsmgr: nsmgr, where: "m:From/m:Address"),
                displayName: GetValue(mailRoot: mailRoot, nsmgr: nsmgr, where: "m:From/m:Name")
            );
            m.Headers.Add(
                name: "X-OrigamEmailIdentifier",
                value: GetValue(mailRoot: mailRoot, nsmgr: nsmgr, where: "m:MessageIdentifier")
            );

            //load recipient list
            XmlNodeList recipientList;
            recipientList = mailRoot.SelectNodes(xpath: "m:To/m:EmailAddress", nsmgr: nsmgr);
            foreach (XmlNode recipientRoot in recipientList)
            {
                m.To.Add(
                    item: new MailAddress(
                        address: GetValue(
                            mailRoot: recipientRoot,
                            nsmgr: nsmgr,
                            where: "m:Address"
                        ),
                        displayName: GetValue(
                            mailRoot: recipientRoot,
                            nsmgr: nsmgr,
                            where: "m:Name"
                        )
                    )
                );
            }
            //put html body inside
            m.Body = GetValue(mailRoot: mailRoot, nsmgr: nsmgr, where: "m:Body");
            m.IsBodyHtml = true;
            try
            {
                MailLogUtils.SendMessageAndLog(client: smtpClient, message: m);
                retVal++;
            }
            catch (Exception ex)
            {
                log.LogOrigamError(ex: ex);
                throw;
            }
        }
        return retVal;
    }

    public override int SendMail2(MailData mailData, string server, int port)
    {
        //return Value positive number (include 0zero) indicates OK result, negative -1 means error
        int retVal = 0;
        var smtpClient = BuildSmtpClient(server: server, port: port);
        //send one mail per Mail section
        foreach (MailData.MailRow mailrow in mailData.Mail.Rows)
        {
            MailMessage m = new MailMessage();
            //put mail header info
            m.Subject = mailrow.Subject;
            m.BodyEncoding = Encoding.UTF8;
            string fromName = "";
            string fromAddress = "";
            OpenPOP.MIMEParser.Utility.ParseEmailAddress(
                strEmailAddress: mailrow.Sender,
                strUser: ref fromName,
                strAddress: ref fromAddress
            );
            m.From = new MailAddress(address: fromAddress, displayName: fromName);
            m.Headers.Add(name: "X-OrigamEmailIdentifier", value: mailrow.Id.ToString());
            string[] to = mailrow.Recipient.Split(separator: ";".ToCharArray());
            foreach (string recipient in to)
            {
                string toName = "";
                string toAddress = "";
                OpenPOP.MIMEParser.Utility.ParseEmailAddress(
                    strEmailAddress: recipient,
                    strUser: ref toName,
                    strAddress: ref toAddress
                );
                if (!string.IsNullOrEmpty(value: recipient))
                {
                    m.To.Add(item: new MailAddress(address: toAddress, displayName: toName));
                }
            }
            if (!mailrow.IsCCNull())
            {
                var cc = mailrow.CC.Split(separator: ";".ToCharArray());
                foreach (string recipient in cc)
                {
                    string ccName = "";
                    string ccAddress = "";
                    OpenPOP.MIMEParser.Utility.ParseEmailAddress(
                        strEmailAddress: recipient,
                        strUser: ref ccName,
                        strAddress: ref ccAddress
                    );
                    if (!string.IsNullOrEmpty(value: recipient))
                    {
                        m.CC.Add(item: new MailAddress(address: ccAddress, displayName: ccName));
                    }
                }
            }
            if (!mailrow.IsBCCNull())
            {
                var bcc = mailrow.BCC.Split(separator: ";".ToCharArray());
                foreach (string recipient in bcc)
                {
                    string bccName = "";
                    string bccAddress = "";
                    OpenPOP.MIMEParser.Utility.ParseEmailAddress(
                        strEmailAddress: recipient,
                        strUser: ref bccName,
                        strAddress: ref bccAddress
                    );
                    if (!string.IsNullOrEmpty(value: recipient))
                    {
                        m.Bcc.Add(item: new MailAddress(address: bccAddress, displayName: bccName));
                    }
                }
            }
            if (!mailrow.IsMessageBodyNull())
            {
                //put html body inside
                if (mailrow.MessageBody.StartsWith(value: "<"))
                {
                    AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(
                        content: HtmlToText(source: mailrow.MessageBody),
                        contentEncoding: null,
                        mediaType: MediaTypeNames.Text.Plain
                    );
                    m.AlternateViews.Add(item: plainTextView);
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                        content: WebUtility.HtmlDecode(value: mailrow.MessageBody),
                        contentEncoding: null,
                        mediaType: MediaTypeNames.Text.Html
                    );
                    m.AlternateViews.Add(item: htmlView);
                }
                else
                {
                    m.Body = mailrow.MessageBody;
                    m.IsBodyHtml = false;
                }
            }
            foreach (MailData.MailAttachmentRow attachment in mailrow.GetMailAttachmentRows())
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream(
                    buffer: attachment.Data,
                    writable: false
                );
                Attachment att = new Attachment(contentStream: stream, name: attachment.FileName);
                m.Attachments.Add(item: att);
            }
            try
            {
                MailLogUtils.SendMessageAndLog(client: smtpClient, message: m);
                retVal++;
            }
            catch (Exception ex)
            {
                log.LogOrigamError(ex: ex);
                throw;
            }
        }
        return retVal;
    }

    private SmtpClient BuildSmtpClient(string server, int port)
    {
        var smtpClient = new SmtpClient();
        if (!string.IsNullOrEmpty(value: server))
        {
            smtpClient.Host = server;
            smtpClient.Port = port;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        }
        else if (!string.IsNullOrWhiteSpace(value: pickupDirectoryLocation))
        {
            smtpClient.PickupDirectoryLocation = pickupDirectoryLocation;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            smtpClient.EnableSsl = false;
        }
        else
        {
            SetConfigValues(smtpClient: smtpClient);
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
        smtpClient.Credentials = new NetworkCredential(userName: username, password: password);
    }
}
