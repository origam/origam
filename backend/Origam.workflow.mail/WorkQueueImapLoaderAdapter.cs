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
using MimeKit;
using MimeTypes;
using Origam.Mail;
using Origam.Workbench.Services;
using Origam.Workflow.WorkQueue;

namespace Origam.workflow.mail;

class WorkQueueImapLoaderAdapter : WorkQueueLoaderAdapter
{
    private static readonly Regex InvalidXmlCharacterRegex = new(
        pattern: @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD\uD800-\uDBFF\uDC00-\uDFFF]",
        options: RegexOptions.Compiled
    );

    private static readonly ILog log = LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );

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
        string transactionId
    )
    {
        string url = "";
        int port = 143;
        bool ssl = false;
        string[] cnParts = connection.Split(separator: ";".ToCharArray());
        foreach (string part in cnParts)
        {
            string[] pair = part.Split(separator: "=".ToCharArray());
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
                        port = int.Parse(s: pair[1]);
                        break;
                    }
                    case "ssl":
                    {
                        ssl = bool.Parse(value: pair[1]);
                        break;
                    }
                    case "dropbox":
                    {
                        dropbox = pair[1];
                        break;
                    }
                    case "attachHtml":
                    {
                        attachHtml = bool.Parse(value: pair[1]);
                        break;
                    }
                    case "badmail":
                    {
                        badMail = pair[1];
                        break;
                    }
                    case "sanitize":
                    {
                        sanitize = bool.Parse(value: pair[1]);
                        break;
                    }
                }
            }
        }
        client = new ImapClient();
        client.Connect(host: url, port: port, useSsl: ssl);
        client.Authenticate(userName: userName, password: password);
        client.Inbox.GetSubfolders();
        inbox = client.Inbox;
        inbox.Open(access: FolderAccess.ReadWrite);
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
        mailData.Mail.AddMailRow(row: mailRow);
        var adapterResult = new WorkQueueAdapterResult(
            document: DataDocumentFactory.New(dataSet: mailData)
        );
        string finalFolder = dropbox;
        MimeMessage lastMessage = inbox.GetMessage(index: inbox.Count - 1);
        try
        {
            mailRow.Sender = lastMessage.From.First().ToString();
            mailRow.Recipient = lastMessage
                .To.Select(selector: x => x.ToString())
                .Aggregate(func: (x, y) => x + ";" + y);
            mailRow.Subject = GetSubject(lastMessage: lastMessage, mailData: mailData);
            mailRow.DateSent = lastMessage.Date.DateTime;
            mailRow.DateReceived = lastMessage.Date.DateTime;
            mailRow.MessageBody = GetMessageBody(lastMessage: lastMessage);
            if (!IsMessageBodyValidForXml(body: mailRow.MessageBody))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        message: $"Invalid characters in the mail body:\n\n{mailRow.MessageBody}"
                    );
                }
                if (sanitize)
                {
                    mailRow.MessageBody = RemoveInvalidXmlCharacters(input: mailRow.MessageBody);
                }
                else
                {
                    throw new Exception(
                        message: "Message body contains invalid characters preventing XML serialization"
                    );
                }
            }
            byte[] htmlAttachment = GetHtmlAttachment(lastMessage: lastMessage);
            adapterResult.State = lastMessage.References.LastOrDefault();
            List<WorkQueueAttachment> attachments = GetAttachments(
                lastMessage: lastMessage,
                htmlAttachment: htmlAttachment
            );
            adapterResult.Attachments = new WorkQueueAttachment[attachments.Count];
            attachments.CopyTo(array: adapterResult.Attachments);
        }
        catch (Exception ex)
        {
            mailRow.MessageBody = ex.Message;
            finalFolder = badMail;
        }
        UniqueId lastMessageId = inbox
            .Fetch(min: 0, max: -1, items: MessageSummaryItems.UniqueId)
            .Last()
            .UniqueId;
        FlagAsSeen(messageId: lastMessageId);
        IMailFolder finalMailFolder =
            FindFolder(name: finalFolder)
            ?? throw new Exception(message: $"Could not find folder called \"{finalFolder}\"");
        inbox.MoveTo(uid: lastMessageId, destination: finalMailFolder);
        lastMessageIndex++;
        return adapterResult;
    }

    private static bool IsMessageBodyValidForXml(string body)
    {
        if (string.IsNullOrEmpty(value: body))
        {
            return true;
        }
        return !InvalidXmlCharacterRegex.IsMatch(input: body);
    }

    private static string GetMessageBody(MimeMessage lastMessage)
    {
        if (!string.IsNullOrEmpty(value: lastMessage.TextBody))
        {
            return lastMessage.TextBody;
        }
        if (!string.IsNullOrEmpty(value: lastMessage.HtmlBody))
        {
            string html = lastMessage.HtmlBody;
            if (string.IsNullOrEmpty(value: lastMessage.TextBody)) // only if we do not have a text yet
            {
                return AbstractMailService.HtmlToText(source: html);
            }
        }
        return null;
    }

    private byte[] GetHtmlAttachment(MimeMessage lastMessage)
    {
        if (!string.IsNullOrEmpty(value: lastMessage.HtmlBody) && attachHtml)
        {
            return new UTF8Encoding().GetBytes(s: lastMessage.HtmlBody);
        }
        return null;
    }

    private void FlagAsSeen(UniqueId messageId)
    {
        inbox.AddFlags(
            uids: new List<UniqueId> { messageId },
            flags: MessageFlags.Seen,
            silent: true
        );
    }

    private static string GetSubject(MimeMessage lastMessage, MailData mailData)
    {
        return lastMessage.Subject.Length > mailData.Mail.SubjectColumn.MaxLength
            ? $"{lastMessage.Subject.Substring(startIndex: 0, length: mailData.Mail.SubjectColumn.MaxLength - 4)} ..."
            : lastMessage.Subject;
    }

    private List<WorkQueueAttachment> GetAttachments(MimeMessage lastMessage, byte[] htmlAttachment)
    {
        var attachments = new List<WorkQueueAttachment>();
        int nonameCount = 1;
        foreach (MimeEntity attachment in lastMessage.Attachments)
        {
            string fileName = attachment.ContentDisposition.FileName;
            if (fileName == null)
            {
                fileName = "noname" + nonameCount++;
                string extension = GetDefaultExtension(mimeType: attachment.ContentType.MediaType);
                if (extension != "")
                {
                    fileName += extension;
                }
            }
            attachments.Add(
                item: new WorkQueueAttachment
                {
                    Data = GetAttachmentData(attachment: attachment),
                    Name = fileName,
                }
            );
        }
        if (htmlAttachment != null)
        {
            attachments.Add(
                item: new WorkQueueAttachment { Data = htmlAttachment, Name = "original.html" }
            );
        }
        return attachments;
    }

    private void DeleteAllMessages()
    {
        List<UniqueId> messageIds = inbox
            .Fetch(min: 0, max: -1, items: MessageSummaryItems.UniqueId)
            .Select(selector: x => x.UniqueId)
            .ToList();
        if (messageIds.Count > 0)
        {
            inbox.AddFlags(uids: messageIds, flags: MessageFlags.Deleted, silent: true);
        }
    }

    IMailFolder FindFolder(string name)
    {
        IMailFolder topLevelFolder = client.GetFolder(
            @namespace: client.PersonalNamespaces[index: 0]
        );
        return FindSubFolder(toplevel: topLevelFolder, name: name);
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
            var folder = FindSubFolder(toplevel: subfolder, name: name);
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
            messagePart.Message.WriteTo(stream: stream);
        }
        else
        {
            var part = (MimePart)attachment;
            part.Content.DecodeTo(stream: stream);
        }
        return stream.ToArray();
    }

    private static string GetDefaultExtension(string mimeType)
    {
        string value = MimeTypeMap.GetExtension(mimeType: mimeType);
        string result = value ?? string.Empty;
        return result;
    }

    private static string RemoveInvalidXmlCharacters(string input)
    {
        return InvalidXmlCharacterRegex.Replace(input: input, replacement: string.Empty);
    }
}
