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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CSharpFunctionalExtensions;
using MailKit.Net.Pop3;
using MailKit.Security;
using MimeKit;
using Origam.Service.Core;

namespace Origam.Mail;

/// <summary>
/// MailAgent sends mails based on AsMail.xsd schema
/// </summary>
public abstract class AbstractMailService : IMailService
{
    public AbstractMailService() { }

    public static readonly Regex HrefRegex = new Regex(
        pattern: @"<A[^>]*?HREF\s*=\s*[""']?([^'"" >]+?)[ '""]?>",
        options: RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnoreCase
            | RegexOptions.Compiled
    );
    public static readonly Regex URLRegex = new Regex(
        pattern: @"\b(http\://|https\://|ftp\://|mailto\:|www\.)([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.[a-zA-Z]+)(\:[0-9]+)?([a-zA-Z0-9\.\,\?\'\\/\+&%\$#\=~_\-@]*)*\b",
        options: RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnoreCase
            | RegexOptions.Compiled
    );

    static string CapHref(Match m)
    {
        // Get the matched string.
        string x = m.ToString();
        Match url = URLRegex.Match(input: x);
        return "(" + url.ToString() + ") ";
    }

    public static string HtmlToText(string source)
    {
        string result;
        // Remove HTML Development formatting
        // Replace line breaks with space
        // because browsers inserts space
        result = source.Replace(oldValue: "\r", newValue: " ");
        // Replace line breaks with space
        // because browsers inserts space
        result = result.Replace(oldValue: "\n", newValue: " ");
        // Remove step-formatting
        result = result.Replace(oldValue: "\t", newValue: string.Empty);
        // Remove repeating speces becuase browsers ignore them
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"( )+",
            replacement: " "
        );
        // Remove the header (prepare first by clearing attributes)
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*head([^>])*>",
            replacement: "<head>",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"(<( )*(/)( )*head( )*>)",
            replacement: "</head>",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(<head>).*(</head>)",
            replacement: string.Empty,
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // remove all scripts (prepare first by clearing attributes)
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*script([^>])*>",
            replacement: "<script>",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"(<( )*(/)( )*script( )*>)",
            replacement: "</script>",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        //result = System.Text.RegularExpressions.Regex.Replace(result,
        //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
        //         string.Empty,
        //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"(<script>).*(</script>)",
            replacement: string.Empty,
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // remove all styles (prepare first by clearing attributes)
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*style([^>])*>",
            replacement: "<style>",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"(<( )*(/)( )*style( )*>)",
            replacement: "</style>",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(<style>).*(</style>)",
            replacement: string.Empty,
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // insert --- instead of <hr> tags
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*hr([^>])*>",
            replacement: "\r____________________\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // insert tabs in spaces of <td> tags
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*td([^>])*>",
            replacement: "\t",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // insert line breaks in places of <BR> and <LI> tags
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*br( )*>",
            replacement: "\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*li( )*>",
            replacement: "\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // insert line paragraphs (double line breaks) in place
        // if <P>, <DIV> and <TR> tags
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*div([^>])*>",
            replacement: "\r\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*tr([^>])*>",
            replacement: "\r\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<( )*p([^>])*>",
            replacement: "\r\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = HrefRegex.Replace(input: result, evaluator: new MatchEvaluator(CapHref));
        // Remove remaining tags like <a>, links, images,
        // comments etc - anything thats enclosed inside < >
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<[^>]*>",
            replacement: string.Empty,
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // replace special characters:
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&nbsp;",
            replacement: " ",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&bull;",
            replacement: " * ",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&lsaquo;",
            replacement: "<",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&rsaquo;",
            replacement: ">",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&trade;",
            replacement: "(tm)",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&frasl;",
            replacement: "/",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"<",
            replacement: "<",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @">",
            replacement: ">",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&copy;",
            replacement: "(c)",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&reg;",
            replacement: "(r)",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // Remove all others. More can be added, see
        // http://hotwired.lycos.com/webmonkey/reference/special_characters/
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: @"&(.{2,6});",
            replacement: string.Empty,
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // for testng
        //System.Text.RegularExpressions.Regex.Replace(result,
        //       this.txtRegex.Text,string.Empty,
        //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // make line breaking consistent
        result = result.Replace(oldValue: "\n", newValue: "\r");
        // Remove extra line breaks and tabs:
        // replace over 2 breaks with 2 and over 4 tabs with 4.
        // Prepare first to remove any whitespaces inbetween
        // the escaped characters and remove redundant tabs inbetween linebreaks
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(\r)( )+(\r)",
            replacement: "\r\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(\t)( )+(\t)",
            replacement: "\t\t",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(\t)( )+(\r)",
            replacement: "\t\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(\r)( )+(\t)",
            replacement: "\r\t",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // Remove redundant tabs
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(\r)(\t)+(\r)",
            replacement: "\r\r",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // Remove multible tabs followind a linebreak with just one tab
        result = System.Text.RegularExpressions.Regex.Replace(
            input: result,
            pattern: "(\r)(\t)+",
            replacement: "\r\t",
            options: System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        // Initial replacement target string for linebreaks
        string breaks = "\r\r\r";
        // Initial replacement target string for tabs
        string tabs = "\t\t\t\t\t";
        for (int index = 0; index < result.Length; index++)
        {
            result = result.Replace(oldValue: breaks, newValue: "\r\r");
            result = result.Replace(oldValue: tabs, newValue: "\t\t\t\t");
            breaks = breaks + "\r";
            tabs = tabs + "\t";
        }
        result = result.Replace(oldValue: "\r", newValue: Environment.NewLine);
        result = result.Replace(oldValue: "\t", newValue: ((char)9).ToString());
        // Thats it.
        return result;
    }

    public static IDataDocument GetMails(
        string mailServer,
        int port,
        string userName,
        string password,
        string transactionId,
        int maxCount
    )
    {
        Pop3Client popClient = new Pop3Client();
        MailData mailData = new MailData();
        try
        {
            popClient = GetPopClient(
                mailServer: mailServer,
                port: port,
                userName: userName,
                password: password,
                transactionId: transactionId,
                useSsl: true
            );
            int count = popClient.Count;
            if (maxCount > 0 && maxCount < count)
            {
                count = maxCount;
            }

            for (int i = 1; i <= count; i++)
            {
                RetrieveMail(
                    mailData: mailData,
                    popClient: popClient,
                    messageNumber: i,
                    delete: true
                );
            }
        }
        finally
        {
            if (transactionId == null && popClient != null)
            {
                popClient.Disconnect(quit: true);
            }
        }
        return DataDocumentFactory.New(dataSet: mailData);
    }

    public static void RetrieveMailNext(MailData mailData, Pop3Client popClient, bool delete)
    {
        int? mayBeIndex = getNextMessageIndex(popClient: popClient);
        if (mayBeIndex.HasValue)
        {
            RetrieveMail(
                mailData: mailData,
                popClient: popClient,
                messageNumber: mayBeIndex.Value,
                delete: delete
            );
        }
    }

    public static void RetrieveMail(
        MailData mailData,
        Pop3Client popClient,
        int messageNumber,
        bool delete
    )
    {
        if (popClient.Count == 0)
        {
            return;
        }

        MimeMessage message = popClient.GetMessage(index: messageNumber);
        MailData.MailRow mailrow = mailData.Mail.NewMailRow();
        mailrow.Id = Guid.NewGuid();
        MailboxAddress from = message.From.Mailboxes.First();
        mailrow.Sender = from.Name + "<" + from.Address + ">";
        mailrow.Recipient = "";
        mailrow.Recipient = message
            .To.Select(selector: x => x.ToString())
            .Aggregate(func: (x, y) => x + "; " + y);
        mailrow.Subject = message.Subject;
        mailrow.DateSent = message.Date.DateTime;
        mailrow.DateReceived = DateTime.Now;
        mailrow.MessageId = message.MessageId;
        mailrow.MessageBody = GetMessageBody(message: message);
        mailData.Mail.AddMailRow(row: mailrow);
        foreach (MimeEntity attachment in message.Attachments)
        {
            MailData.MailAttachmentRow attrow = mailData.MailAttachment.NewMailAttachmentRow();
            attrow.Id = Guid.NewGuid();
            attrow.MailRow = mailrow;
            attrow.Data = GetAttachmentData(attachment: attachment);
            attrow.FileName = attachment.ContentDisposition.FileName;
            mailData.MailAttachment.AddMailAttachmentRow(row: attrow);
        }
        if (delete)
        {
            popClient.DeleteMessage(index: messageNumber);
        }
    }

    private static string GetMessageBody(MimeMessage message)
    {
        string body = null;
        if (message.TextBody == null)
        {
            if (message.HtmlBody != null)
            {
                string bodyDecoded = message.HtmlBody;
                if (bodyDecoded.IndexOf(value: "<") >= 0)
                {
                    body = HtmlToText(source: bodyDecoded);
                }
                else
                {
                    body = bodyDecoded;
                }
            }
        }
        else
        {
            body = message.TextBody;
        }
        return body;
    }

    private static int? getNextMessageIndex(Pop3Client popClient)
    {
        for (int i = 0; i < popClient.Count; i++)
        {
            var message = GetMessageByIndex(popClient: popClient, index: i);
            if (message != null)
            {
                return i;
            }
        }
        return null;
    }

    private static MimeMessage GetMessageByIndex(Pop3Client popClient, int index)
    {
        try
        {
            return popClient.GetMessage(index: index);
        }
        catch (Pop3CommandException)
        {
            return null; // message with this number was already deleted
        }
    }

    private static byte[] GetAttachmentData(MimeEntity attachment)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            if (attachment is MessagePart)
            {
                var part = (MessagePart)attachment;
                part.Message.WriteTo(stream: stream);
            }
            else
            {
                var part = (MimePart)attachment;
                part.Content.DecodeTo(stream: stream);
            }
            return stream.ToArray();
        }
    }

    public int SendMail(IXmlContainer mailDocument, string server, int port)
    {
        if (mailDocument is IDataDocument)
        {
            MailData mailData = new MailData();
            mailData.Merge(dataSet: (mailDocument as IDataDocument).DataSet);
            return SendMail2(mailData: mailData, server: server, port: port);
        }

        return SendMail1(mailDocument: mailDocument, server: server, port: port);
    }

    public abstract int SendMail1(IXmlContainer mailDocument, string server, int port);

    public abstract int SendMail2(MailData mailData, string server, int port);

    public static string GetValue(XmlNode mailRoot, XmlNamespaceManager nsmgr, string where)
    {
        return mailRoot.SelectSingleNode(xpath: where, nsmgr: nsmgr).InnerXml;
    }

    private static Maybe<Pop3Client> TryFindInActiveClient(string connString, string transactionId)
    {
        if (transactionId == null)
        {
            return null;
        }

        Pop3Transaction pop3Transaction =
            ResourceMonitor.GetTransaction(
                transactionId: transactionId,
                resourceManagerId: connString
            ) as Pop3Transaction;
        return Maybe<Pop3Client>.From(obj: pop3Transaction?.PopClient);
    }

    public static Pop3Client GetPopClient(
        string mailServer,
        int port,
        string userName,
        string password,
        string transactionId,
        bool useSsl
    )
    {
        string connString =
            "Server="
            + mailServer
            + "; Port="
            + port
            + "; UserName="
            + userName
            + "; Password="
            + password;
        Maybe<Pop3Client> maybeClient = TryFindInActiveClient(
            connString: connString,
            transactionId: transactionId
        );
        if (maybeClient.HasValue)
        {
            return maybeClient.Value;
        }

        Pop3Client popClient = new Pop3Client();
        // accept all SSL certificates (in case the server supports STARTTLS)!
        popClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        TryConnect(
            mailServer: mailServer,
            port: port,
            userName: userName,
            password: password,
            popClient: popClient,
            useSsl: useSsl
        );
        ResourceMonitor.RegisterTransaction(
            transactionId: transactionId,
            resourceManagerId: connString,
            transaction: new Pop3Transaction(client: popClient)
        );
        return popClient;
    }

    private static void TryConnect(
        string mailServer,
        int port,
        string userName,
        string password,
        Pop3Client popClient,
        bool useSsl
    )
    {
        try
        {
            popClient.Connect(host: mailServer, port: port, useSsl: useSsl);
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorConnectToServer", args: ex.Message)
            );
        }
        try
        {
            popClient.Authenticate(userName: userName, password: password);
        }
        catch (AuthenticationException ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorInvalidUsername"),
                innerException: ex
            );
        }
    }
}
