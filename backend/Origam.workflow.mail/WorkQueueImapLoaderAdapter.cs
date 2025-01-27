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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using MailKit;
using MailKit.Net.Imap;
using Origam.Mail;
using Origam.Workflow.WorkQueue;
using Origam.Workbench.Services;
using MimeKit;
using MimeTypes;

namespace Origam.workflow.mail;
class WorkQueueImapLoaderAdapter : WorkQueueLoaderAdapter
{
    private static readonly Regex InvalidXmlCharacterRegex = new(
        @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD\uD800-\uDBFF\uDC00-\uDFFF]",
        RegexOptions.Compiled);
    
    private static readonly ILog log = LogManager
        .GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
    
    private ImapClient client;
    private IMailFolder inbox;
    private string dropbox = "DROPBOX";
    private int totalMessages = 0;
    private int lastMessageIndex = 1;
    private bool attachHtml = false;
    private string badMail = "BADMAIL";
    private bool sanitize = false;
    public override void Connect(
        IWorkQueueService service, 
        Guid queueId, 
        string workQueueClass, 
        string connection, 
        string userName, 
        string password, 
        string transactionId)
    {
        string url = "";
        int port = 143;
        bool ssl = false;
        string[] cnParts = connection.Split(";".ToCharArray());
        foreach (string part in cnParts)
        {
            string[] pair = part.Split("=".ToCharArray());
            if (pair.Length == 2)
            {
                switch (pair[0])
                {
                    case "url":
                    {
                        url = pair[1];
                        break;
                    }
                    case "port":
                    {
                        port = int.Parse(pair[1]);
                        break;
                    }
                    case "ssl":
                    {
                        ssl = bool.Parse(pair[1]);
                        break;
                    }
                    case "dropbox":
                    {
                        dropbox = pair[1];
                        break;
                    }
                    case "attachHtml":
                    {
                        attachHtml = bool.Parse(pair[1]);
                        break;
                    }
                    case "badmail":
                    {
                        badMail = pair[1];
                        break;
                    }
                    case "sanitize":
                    {
                        sanitize = bool.Parse(pair[1]);
                        break;
                    }
                }
            }
        }
        client = new ImapClient();
        // accept all SSL certificates!
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        client.Connect(url, port, ssl);
        client.Authenticate(userName, password);
        client.Inbox.GetSubfolders();
        inbox = client.Inbox;
        inbox.Open(FolderAccess.ReadWrite);
        totalMessages = inbox.Count;
    }
    public override void Disconnect()
    {
        client.Disconnect(quit: true);
    }
    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if (totalMessages == 0)
        {
            return null;
        }
        if (totalMessages < lastMessageIndex)
        {
            DeleteAllMessages();
            return null;
        }
        MailData mailData = new MailData();
        mailData.DataSetName = "ROOT";
        MailData.MailRow mailRow = mailData.Mail.NewMailRow();
        mailRow.Id = Guid.NewGuid();
        mailRow.DateSent = DateTime.Now;
        mailRow.Recipient = "";
        mailRow.Sender = "";
        mailData.Mail.AddMailRow(mailRow);
        var adapterResult = new WorkQueueAdapterResult(DataDocumentFactory.New(mailData));
        string finalFolder = dropbox;
        MimeMessage lastMessage = inbox.GetMessage(inbox.Count - 1);
        try
        {
            mailRow.Sender = lastMessage.From.First().ToString();
            mailRow.Recipient = lastMessage.To
                .Select(x => x.ToString())
                .Aggregate((x, y) => x + ";" + y);
            mailRow.Subject = GetSubject(lastMessage, mailData);
            mailRow.DateSent = lastMessage.Date.DateTime;
            mailRow.DateReceived = lastMessage.Date.DateTime;
            mailRow.MessageBody = GetMessageBody(lastMessage);
            if (!IsMessageBodyValidForXml(mailRow.MessageBody))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug($"Invalid characters in the mail body:\n\n{mailRow.MessageBody}");
                }
                if (sanitize)
                {
                    mailRow.MessageBody
                        = RemoveInvalidXmlCharacters(mailRow.MessageBody);
                }
                else
                {
                    throw new Exception(
                        "Message body contains invalid characters preventing XML serialization");
                }
            }
            byte[] htmlAttachment = GetHtmlAttachment(lastMessage);
            adapterResult.State = lastMessage.References.LastOrDefault();
            List<WorkQueueAttachment> attachments = GetAttachments(lastMessage, htmlAttachment);
            adapterResult.Attachments = new WorkQueueAttachment[attachments.Count];
            attachments.CopyTo(adapterResult.Attachments);
        }
        catch (Exception ex)
        {
            mailRow.MessageBody = ex.Message;
            finalFolder = badMail;
        }
        UniqueId lastMessageId = inbox
            .Fetch(0, -1, MessageSummaryItems.UniqueId)
            .Last()
            .UniqueId;
        FlagAsSeen(lastMessageId);
        IMailFolder finalMailFolder 
            = FindFolder(finalFolder) 
              ?? throw new Exception($"Could not find folder called \"{finalFolder}\"");
        inbox.MoveTo(lastMessageId, finalMailFolder);
        lastMessageIndex++;
        return adapterResult;
    }

    private static bool IsMessageBodyValidForXml(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return true;
        }
        return !InvalidXmlCharacterRegex.IsMatch(body);
    }
    
    private static string GetMessageBody(MimeMessage lastMessage)
    {
        if (!string.IsNullOrEmpty(lastMessage.TextBody))
        {
            return lastMessage.TextBody;
        }
        if (!string.IsNullOrEmpty(lastMessage.HtmlBody))
        {
            string html = lastMessage.HtmlBody;
            if (string.IsNullOrEmpty(lastMessage.TextBody)) // only if we do not have a text yet
            {
                return AbstractMailService.HtmlToText(html);
            }
        }
        return null;
    }
    private byte[] GetHtmlAttachment(MimeMessage lastMessage)
    {
        if (!string.IsNullOrEmpty(lastMessage.HtmlBody) && attachHtml)
        {
            return new UTF8Encoding().GetBytes(lastMessage.HtmlBody);
        }
        return null;
    }
    private void FlagAsSeen(UniqueId messageId)
    {
        inbox.AddFlags(
            uids: new List<UniqueId> { messageId }, 
            flags: MessageFlags.Seen, 
            silent: true);
    }
    private static string GetSubject(MimeMessage lastMessage, MailData mailData)
    {
        return lastMessage.Subject.Length > mailData.Mail.SubjectColumn.MaxLength 
            ? $"{lastMessage.Subject.Substring(0, mailData.Mail.SubjectColumn.MaxLength - 4)} ..." 
            : lastMessage.Subject;
    }
    private List<WorkQueueAttachment> GetAttachments(
        MimeMessage lastMessage, byte[] htmlAttachment)
    {
        var attachments = new List<WorkQueueAttachment>();
        int nonameCount = 1;
        foreach (MimeEntity attachment in lastMessage.Attachments)
        {
            string fileName = attachment.ContentDisposition.FileName;
            if (fileName == null)
            {
                fileName = "noname" + nonameCount++;
                string extension = GetDefaultExtension(
                    attachment.ContentType.MediaType);
                if (extension != "")
                {
                    fileName += extension;
                }
            }
            AppendAttachment(
                attachments, fileName, GetAttachmentData(attachment));
        }
        if (htmlAttachment != null)
        {
            AppendAttachment(attachments, "original.html", htmlAttachment);
        }
        return attachments;
    }
    private void DeleteAllMessages()
    {
        List<UniqueId> messageIds = inbox.Fetch(
                0, -1, MessageSummaryItems.UniqueId)
            .Select(x => x.UniqueId)
            .ToList();
        if (messageIds.Count > 0)
        {
            inbox.AddFlags(messageIds, MessageFlags.Deleted, silent: true);
        }
    }
    IMailFolder FindFolder(string name)
    {
        IMailFolder topLevelFolder = client.GetFolder(
            client.PersonalNamespaces[0]);
        return FindSubFolder(topLevelFolder, name);
    }
    IMailFolder FindSubFolder(IMailFolder toplevel, string name)
    {
        List<IMailFolder> subfolders = toplevel.GetSubfolders().ToList();
        foreach (IMailFolder subfolder in subfolders)
        {
            if (subfolder.Name == name)
            {
                return subfolder;
            }
        }
        foreach (IMailFolder subfolder in subfolders)
        {
            var folder = FindSubFolder(subfolder, name);
            if (folder != null)
            {
                return folder;
            }
        }
        return null;
    }
    private byte[] GetAttachmentData(MimeEntity attachment)
    {
        using var stream = new MemoryStream();
        if (attachment is MessagePart messagePart)
        {
            messagePart.Message.WriteTo(stream);
        }
        else
        {
            var part = (MimePart)attachment;
            part.Content.DecodeTo(stream);
        }
        return stream.ToArray();
    }
    private static void AppendAttachment(
        List<WorkQueueAttachment> attachments, 
        string fileName, 
        byte[] data)
    {
        var attachment = new WorkQueueAttachment
        {
            Data = data,
            Name = fileName
        };
        attachments.Add(attachment);
    }
    private static string GetDefaultExtension(string mimeType)
    {
        string value = MimeTypeMap.GetExtension(mimeType);
        string result = value ?? string.Empty;
        return result;
    }
    private static string RemoveInvalidXmlCharacters(string input)
    {
        return InvalidXmlCharacterRegex.Replace(input, string.Empty);
    }
}
