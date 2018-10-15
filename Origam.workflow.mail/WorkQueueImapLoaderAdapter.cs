using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using Koolwired.Imap;

using Origam.Mail;
using Origam.Workflow.WorkQueue;
using Origam.Workbench.Services;
using Microsoft.Win32;

namespace Origam.workflow.mail
{
    class WorkQueueImapLoaderAdapter : WorkQueueLoaderAdapter
    {
        private ImapConnect _imapConnection;
        private ImapCommand _command;
        private ImapAuthenticate _authenticate;
        private ImapMailbox _imapBox;
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

            _imapConnection = new ImapConnect(url, port, ssl);
            _command = new ImapCommand(_imapConnection);
            _authenticate = new ImapAuthenticate(_imapConnection, userName, password);
            _imapConnection.Open();
            _authenticate.Login();
            _imapBox = _command.Select(mailbox);
            _totalMessages = _imapBox.Exist;
        }

        public override void Disconnect()
        {
            if (_imapConnection.State == ConnectionState.Open)
            {
                _authenticate.Logout();
                _imapConnection.Close();
            }
        }

        public override WorkQueueAdapterResult GetItem(string lastState)
        {
            if (_totalMessages < _lastMessage)
            {
                int[] uids = new int[_totalMessages];
                for (int i = 0; i < _totalMessages; i++)
                {
                    uids[i] = _command.FetchUID(i + 1);
                }
                for (int i = 0; i < uids.Length; i++)
                {
                    _command.SetFlag(uids[i], @"\Deleted", true);
                }
                _command.Expunge();
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
            WorkQueueAdapterResult result = new WorkQueueAdapterResult(new XmlDataDocument(mailData));
            string finalFolder = _dropbox;

            try
            {
                ImapMailbox imapBox = _command.Fetch(_lastMessage, _lastMessage);
                if (imapBox.Messages.Count == 0)
                {
                    return null;
                }

                ImapMailboxMessage msg = imapBox.Messages[0];
                _command.FetchBodyStructure(msg);

                mailrow.Sender = msg.From.Address;

                foreach (ImapAddress addr in msg.To)
                {
                    if (mailrow.Recipient.Length > 0) mailrow.Recipient += "; ";

                    mailrow.Recipient += addr.Address;
                }

                if (msg.Subject.Length > mailData.Mail.SubjectColumn.MaxLength)
                {
                    mailrow.Subject = String.Format("{0} ...",
                        msg.Subject.Substring(0,
                        mailData.Mail.SubjectColumn.MaxLength - 4));
                }
                else
                {
                    mailrow.Subject = msg.Subject;
                }
                
                if (msg.Sent == new DateTime())
                {
                    // no date set
                    mailrow.DateSent = msg.Received;
                }
                else
                {
                    mailrow.DateSent = msg.Sent;
                }
                mailrow.DateReceived = msg.Received;

                for (int i = 0; i < msg.BodyParts.Count; i++)
                {
                    ImapMessageBodyPart bodyPart = msg.BodyParts[i];
                    _command.FetchBodyPart(msg, i);
                }

                byte[] htmlAttachment = null;

                if (msg.HasText)
                {
                    mailrow.MessageBody = msg.BodyParts[msg.Text].Data;
                }

                if (msg.HasHTML)
                {
                    string html = msg.BodyParts[msg.HTML].Data;

                    if (!msg.HasText)    // only if we do not have a text yet
                    {
                        mailrow.MessageBody = AbstractMailService.HtmlToText(html);
                    }

                    if (_attachHtml)
                    {
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        htmlAttachment = encoding.GetBytes(html);
                    }
                }

                result.State = msg.Reference;

                // set attachments
                List<WorkQueueAttachment> attachments = new List<WorkQueueAttachment>();

                int nonameCount = 1;

                for (int i = 0; i < msg.BodyParts.Count; i++)
                {
                    ImapMessageBodyPart bodyPart = msg.BodyParts[i];
                    if (bodyPart.Attachment)
                    {
                        string fileName = bodyPart.FileName;

                        if (fileName == null)
                        {
                            fileName = "noname" + nonameCount++;

                            string extension = GetDefaultExtension(bodyPart.ContentType.MediaType);

                            if (extension != "") fileName += extension;
                        }

                        AppendAttachment(attachments, fileName, bodyPart.DataBinary);
                    }
                }

                if (htmlAttachment != null)
                {
                    AppendAttachment(attachments, "original.html", htmlAttachment);
                }

                result.Attachments = new WorkQueueAttachment[attachments.Count];
                attachments.CopyTo(result.Attachments);
            }
            catch (Exception ex)
            {
                mailrow.MessageBody = ex.Message;
                finalFolder = _badmail;
            }

            _command.SetSeen(_lastMessage, true);
            _command.Copy(finalFolder, _lastMessage, _lastMessage);

            _lastMessage++;

            return result;
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
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : string.Empty;

            return result;
        }
    }
}
