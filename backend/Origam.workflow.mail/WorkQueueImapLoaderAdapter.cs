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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
    private ImapClient client;
    private IMailFolder inbox;
    private string _dropbox = "DROPBOX";
    private int _totalMessages = 0;
    private int _lastMessage = 1;
    private bool _attachHtml = false;
    private string _badmail = "BADMAIL";

    public override void Connect(IWorkQueueService service, Guid queueId, string workQueueClass, string connection, string userName, string password, string transactionId)
    {
            string url = "";
            int port = 143;
            string mailbox = "INBOX";
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
                            url = pair[1];
                            break;
                        case "port":
                            port = int.Parse(pair[1]);
                            break;
                        case "mailbox":
                            mailbox = pair[1];
                            break;
                        case "ssl":
                            ssl = bool.Parse(pair[1]);
                            break;
                        case "dropbox":
                            _dropbox = pair[1];
                            break;
                        case "attachHtml":
                            _attachHtml = bool.Parse(pair[1]);
                            break;
                        case "badmail":
                            _badmail = pair[1];
                            break;
                        default:
                            break;
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
            _totalMessages = inbox.Count;
        }

    public override void Disconnect()
    {
            client.Disconnect(true);
        }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
            if (_totalMessages == 0) return null;
            if (_totalMessages < _lastMessage)
            {
                DeleteAllMessages();
                return null;
            }

            MailData mailData = new MailData();
            mailData.DataSetName = "ROOT";
            MailData.MailRow mailrow = mailData.Mail.NewMailRow();
            mailrow.Id = Guid.NewGuid();
            mailrow.DateSent = DateTime.Now;
            mailrow.Recipient = "";
            mailrow.Sender = "";
            mailData.Mail.AddMailRow(mailrow);
            WorkQueueAdapterResult result = new WorkQueueAdapterResult(DataDocumentFactory.New(mailData));
            string finalFolder = _dropbox;

            MimeMessage lastMessage = inbox.GetMessage(inbox.Count - 1);

            try
            {
                mailrow.Sender = lastMessage.From.First().ToString();
                mailrow.Recipient = lastMessage.To
                    .Select(x => x.ToString())
                    .Aggregate((x, y) => x + ";" + y);

                mailrow.Subject = GetSubject(lastMessage, mailData);
                mailrow.DateSent = lastMessage.Date.DateTime;
                mailrow.DateReceived = lastMessage.Date.DateTime;
                mailrow.MessageBody = GetMessageBody(lastMessage);

                byte[] htmlAttachment = GetHtmlAttachment(lastMessage);

                result.State = lastMessage.References.LastOrDefault();

                List<WorkQueueAttachment> attachments = GetAttachments(lastMessage, htmlAttachment);
                result.Attachments = new WorkQueueAttachment[attachments.Count];
                attachments.CopyTo(result.Attachments);
            }
            catch (Exception ex)
            {
                mailrow.MessageBody = ex.Message;
                finalFolder = _badmail;
            }

            MailKit.UniqueId lastMessageId = inbox
                .Fetch(0, -1, MessageSummaryItems.UniqueId)
                .Last()
                .UniqueId;

            FlagAsSeen(lastMessageId);

            IMailFolder finalMailFolder =
                FindFolder(finalFolder)
                ?? throw new Exception("Could not find folder called \""+ finalFolder+"\"");
            inbox.MoveTo(lastMessageId, finalMailFolder);

            _lastMessage++;

            return result;
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
            if (!string.IsNullOrEmpty(lastMessage.HtmlBody) && _attachHtml)
            {
                return new UTF8Encoding().GetBytes(lastMessage.HtmlBody);
            }
            else
            {
                return null;
            }
        }

    private void FlagAsSeen(MailKit.UniqueId messageid)
    {
            inbox.AddFlags(new List<MailKit.UniqueId> { messageid }, MessageFlags.Seen, true);
        }

    private static string GetSubject(MimeMessage lastMessage, MailData mailData)
    {
            if (lastMessage.Subject.Length > mailData.Mail.SubjectColumn.MaxLength)
            {
                return String.Format("{0} ...",
                    lastMessage.Subject.Substring(0,
                        mailData.Mail.SubjectColumn.MaxLength - 4));
            }
            else
            {
                return lastMessage.Subject;
            }
        }

    private List<WorkQueueAttachment> GetAttachments(MimeMessage lastMessage, byte[] htmlAttachment)
    {
            List<WorkQueueAttachment> attachments = new List<WorkQueueAttachment>();

            int nonameCount = 1;
            foreach (MimeEntity attachment in lastMessage.Attachments)
            {
                string fileName = attachment.ContentDisposition.FileName;
                if (fileName == null)
                {
                    fileName = "noname" + nonameCount++;

                    string extension = GetDefaultExtension(attachment.ContentType.MediaType);

                    if (extension != "") fileName += extension;
                }

                AppendAttachment(attachments, fileName, GetAttachmentData(attachment));
            }

            if (htmlAttachment != null)
            {
                AppendAttachment(attachments, "original.html", htmlAttachment);
            }

            return attachments;
        }

    private void DeleteAllMessages()
    {
            var allMsgSummaries = inbox.Fetch(0, -1, MessageSummaryItems.UniqueId)
                .Select(x => x.UniqueId)
                .ToList();
            if (allMsgSummaries.Count > 0)
            {
                inbox.AddFlags(allMsgSummaries, MessageFlags.Deleted, true);
            }
        }

    IMailFolder FindFolder(string name)
    {
            var toplevel = client.GetFolder(client.PersonalNamespaces[0]);
            return FindSubFolder(toplevel, name);
        }

    IMailFolder FindSubFolder(IMailFolder toplevel, string name)
    {
            var subfolders = toplevel.GetSubfolders().ToList();

            foreach (var subfolder in subfolders)
            {
                if (subfolder.Name == name)
                    return subfolder;
            }

            foreach (var subfolder in subfolders)
            {
                var folder = FindSubFolder(subfolder, name);

                if (folder != null)
                    return folder;
            }
            return null;
        }

    private byte[] GetAttachmentData(MimeEntity attachment)
    {
            using (MemoryStream stream = new MemoryStream())
            {
                if (attachment is MessagePart)
                {
                    var part = (MessagePart)attachment;
                    part.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)attachment;
                    part.Content.DecodeTo(stream);
                }
                return stream.ToArray();
            }
        }

    private static void AppendAttachment(List<WorkQueueAttachment> attachments, string fileName, byte[] data)
    {
            WorkQueueAttachment att = new WorkQueueAttachment();
            att.Data = data;
            att.Name = fileName;

            attachments.Add(att);
        }

    private static string GetDefaultExtension(string mimeType)
    {
            var value = MimeTypeMap.GetExtension(mimeType);
            var result = value != null ? value.ToString() : string.Empty;
            return result;
        }
}