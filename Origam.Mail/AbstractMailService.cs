#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Xml;
//using OpenPOP.POP3;
using NandoF.Mail.PopClient;
using System.Text.RegularExpressions;


namespace Origam.Mail
{
    /// <summary>
    /// MailAgent sends mails based on AsMail.xsd schema
    /// </summary>
    public abstract class AbstractMailService
    {
        public AbstractMailService()
        {
        }

        public static readonly Regex HrefRegex = new Regex(@"<A[^>]*?HREF\s*=\s*[""']?([^'"" >]+?)[ '""]?>", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex URLRegex = new Regex(@"\b(http\://|https\://|ftp\://|mailto\:|www\.)([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.[a-zA-Z]+)(\:[0-9]+)?([a-zA-Z0-9\.\,\?\'\\/\+&%\$#\=~_\-@]*)*\b", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static string CapHref(Match m)
        {
            // Get the matched string.
            string x = m.ToString();

            Match url = URLRegex.Match(x);

            return "(" + url.ToString() + ") ";
        }

        public static string HtmlToText(string source)
        {
            string result;
            // Remove HTML Development formatting
            // Replace line breaks with space
            // because browsers inserts space
            result = source.Replace("\r", " ");
            // Replace line breaks with space
            // because browsers inserts space
            result = result.Replace("\n", " ");
            // Remove step-formatting
            result = result.Replace("\t", string.Empty);
            // Remove repeating speces becuase browsers ignore them
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"( )+", " ");

            // Remove the header (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*head([^>])*>", "<head>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"(<( )*(/)( )*head( )*>)", "</head>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(<head>).*(</head>)", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // remove all scripts (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*script([^>])*>", "<script>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"(<( )*(/)( )*script( )*>)", "</script>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //result = System.Text.RegularExpressions.Regex.Replace(result, 
            //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
            //         string.Empty, 
            //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"(<script>).*(</script>)", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // remove all styles (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*style([^>])*>", "<style>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"(<( )*(/)( )*style( )*>)", "</style>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(<style>).*(</style>)", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert --- instead of <hr> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*hr([^>])*>", "\r____________________\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert tabs in spaces of <td> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*td([^>])*>", "\t",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert line breaks in places of <BR> and <LI> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*br( )*>", "\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*li( )*>", "\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert line paragraphs (double line breaks) in place
            // if <P>, <DIV> and <TR> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*div([^>])*>", "\r\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*tr([^>])*>", "\r\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<( )*p([^>])*>", "\r\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            result = HrefRegex.Replace(result, new MatchEvaluator(CapHref));

            // Remove remaining tags like <a>, links, images,
            // comments etc - anything thats enclosed inside < >
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<[^>]*>", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // replace special characters:
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&nbsp;", " ",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&bull;", " * ",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&lsaquo;", "<",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&rsaquo;", ">",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&trade;", "(tm)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&frasl;", "/",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"<", "<",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @">", ">",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&copy;", "(c)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&reg;", "(r)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove all others. More can be added, see
            // http://hotwired.lycos.com/webmonkey/reference/special_characters/
            result = System.Text.RegularExpressions.Regex.Replace(result,
                @"&(.{2,6});", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // for testng
            //System.Text.RegularExpressions.Regex.Replace(result, 
            //       this.txtRegex.Text,string.Empty, 
            //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // make line breaking consistent
            result = result.Replace("\n", "\r");

            // Remove extra line breaks and tabs:
            // replace over 2 breaks with 2 and over 4 tabs with 4. 
            // Prepare first to remove any whitespaces inbetween
            // the escaped characters and remove redundant tabs inbetween linebreaks
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(\r)( )+(\r)", "\r\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(\t)( )+(\t)", "\t\t",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(\t)( )+(\r)", "\t\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(\r)( )+(\t)", "\r\t",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove redundant tabs
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(\r)(\t)+(\r)", "\r\r",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove multible tabs followind a linebreak with just one tab
            result = System.Text.RegularExpressions.Regex.Replace(result,
                "(\r)(\t)+", "\r\t",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Initial replacement target string for linebreaks
            string breaks = "\r\r\r";
            // Initial replacement target string for tabs
            string tabs = "\t\t\t\t\t";
            for (int index = 0; index < result.Length; index++)
            {
                result = result.Replace(breaks, "\r\r");
                result = result.Replace(tabs, "\t\t\t\t");
                breaks = breaks + "\r";
                tabs = tabs + "\t";
            }

            result = result.Replace("\r", Environment.NewLine);
            result = result.Replace("\t", ((char)9).ToString());

            // Thats it.
            return result;
        }



        public static IDataDocument GetMails(string mailServer, int port, string userName, string password, string transactionId, int maxCount)
        {
            PopClient popClient = null;
            MailData mailData = new MailData();

            try
            {
                popClient = GetPopClient(mailServer, port, userName, password, transactionId);

                int count = popClient.GetMessageCount();

                if (maxCount > 0 && maxCount < count) count = maxCount;

                for (int i = 1; i <= count; i++)
                {
                    RetrieveMail(mailData, popClient, i, true);
                }
            }
            finally
            {
                if (transactionId == null && popClient != null)
                {
                    popClient.Disconnect();
                }
            }

            return DataDocumentFactory.New(mailData);
        }

        public static void RetrieveMail(MailData mailData, PopClient popClient, int messageNumber, bool delete)
        {
            //OpenPOP.MIMEParser.Message msg = popClient.GetMessageHeader(i);
            //OpenPOP.MIMEParser.Message msg = popClient.FetchMessage(messageNumber);
            NandoF.Mail.Parser2.Message msg = new NandoF.Mail.Parser2.Message(popClient.FetchMessage(messageNumber), true);

            if (msg == null) return;

            MailData.MailRow mailrow = mailData.Mail.NewMailRow();
            mailrow.Id = Guid.NewGuid();
            //mailrow.Sender = msg.FromEmail;
            mailrow.Sender = msg.Headers.From.Name + "<" + msg.Headers.From.Email + ">";
            mailrow.Recipient = "";

            foreach (string s in msg.Headers.To) // msg.TO)
            {
                if (mailrow.Recipient.Length > 0) mailrow.Recipient += "; ";

                mailrow.Recipient += s;
            }

            mailrow.Subject = msg.Headers.Subject; //msg.Subject;

            try
            {
                mailrow.DateSent = Convert.ToDateTime(msg.Headers.Date); // msg.Date);
            }
            catch
            {
                mailrow.DateSent = DateTime.MinValue;
            }

            try
            {
                mailrow.DateReceived = DateTime.Now; // msg.Received);
            }
            catch
            {
                mailrow.DateReceived = DateTime.Now;
            }

            mailrow.MessageId = msg.Headers.MessageID;

            string body = null;
            if (msg.BodyText == null)
            {
                if (msg.BodyHtml != null)
                {
                    string bodyDecoded = msg.BodyHtml.ContentDecoded;
                    if (bodyDecoded.IndexOf("<") >= 0)
                    {
                        body = HtmlToText(bodyDecoded);
                    }
                    else
                    {
                        body = bodyDecoded;
                    }
                }
            }
            else
            {
                body = msg.BodyText.ContentDecoded;
            }

            if (body != null)
            {
                mailrow.MessageBody = body;
            }

            //			string body = msg.MessageBody[0].ToString();
            //			if(msg.HTML & body.ToUpper().IndexOf("<") >= 0)
            //			{
            //				System.Web.Mail.MailMessage mm = new System.Web.Mail.MailMessage();
            //				mm.BodyFormat = System.Web.Mail.MailFormat.Html;
            //				mm.Body = body;
            //							
            //
            //				mailrow.MessageBody = HtmlToText(body);
            //			}
            //			else
            //			{
            //				mailrow.MessageBody = body;
            //			}

            mailData.Mail.AddMailRow(mailrow);

            foreach (NandoF.Mail.Parser2.MimePart mp in msg.Leaves)
            {
                NandoF.Mail.Parser2.MimePartBinary att = mp as NandoF.Mail.Parser2.MimePartBinary;
                if (att != null)
                {
                    MailData.MailAttachmentRow attrow = mailData.MailAttachment.NewMailAttachmentRow();
                    attrow.Id = Guid.NewGuid();
                    attrow.MailRow = mailrow;
                    attrow.Data = att.ContentDecoded;
                    attrow.FileName = att.FileName;

                    mailData.MailAttachment.AddMailAttachmentRow(attrow);
                }
            }

            //			foreach(OpenPOP.MIMEParser.Attachment att in msg.Attachments)
            //			{
            //				if(att.DecodedAsBytes() != null)
            //				{
            //					MailData.MailAttachmentRow attrow = mailData.MailAttachment.NewMailAttachmentRow();
            //					attrow.Id = Guid.NewGuid();
            //					attrow.MailRow = mailrow;
            //					attrow.Data = att.DecodedAsBytes();
            //					attrow.FileName = att.ContentFileName;
            //					attrow.Note = att.ContentDescription;
            //
            //					mailData.MailAttachment.AddMailAttachmentRow(attrow);
            //				}
            //			}
            //
            if (delete)
            {
                popClient.DeleteMessage(messageNumber);
            }
        }

        public int SendMail(IDataDocument mailDocument, string server, int port)
        {
//            if (mailDocument is IDataDocument)
//            {
//                MailData mailData = new MailData();
//                mailData.Merge((mailDocument as IDataDocument).DataSet);
//
//                return SendMail2(mailData, server, port);
//            }
//            else
//            {
                return SendMail1(mailDocument, server, port);
//            }
        }

        public abstract int SendMail2(MailData mailData, string server, int port);

        public abstract int SendMail1(IDataDocument mailDocument, string server, int port);

        public static string GetValue(XmlNode mailRoot, XmlNamespaceManager nsmgr, string where)
        {
            return mailRoot.SelectSingleNode(where, nsmgr).InnerXml;
        }

        public static PopClient GetPopClient(string mailServer, int port, string userName, string password, string transactionId)
        {
            PopClient popClient;

            if (transactionId == null)
            {
                //				popClient = new PopClient(mailServer, port, userName, password, OpenPOP.POP3.AuthenticationMethod.USERPASS);
                popClient = new PopClient(new NandoF.Data.MailPopAccount(mailServer, port, userName, password));
            }
            else
            {
                string connString = "Server=" + mailServer + "; Port=" + port.ToString() + "; UserName=" + userName + "; Password=" + password;

                Pop3Transaction pop3transaction = ResourceMonitor.GetTransaction(transactionId, connString) as Pop3Transaction;
                if (pop3transaction == null)
                {
                    //					popClient = new POPClient(mailServer, port, userName, password, OpenPOP.POP3.AuthenticationMethod.USERPASS);
                    popClient = new PopClient(new NandoF.Data.MailPopAccount(mailServer, port, userName, password));

                    try
                    {
                        popClient.Connect();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorConnectToServer", ex.Message));
                    }

                    try
                    {
                        popClient.Authenticate(NandoF.Mail.PopClient.AuthenticationMethods.USERPASS);
                    }
                    catch (InvalidPasswordException ex)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorInvalidUsername"), ex);
                    }
                    catch (InvalidLoginException ex)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorInvalidUsername"), ex);
                    }
                    catch (InvalidLoginOrPasswordException ex)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorInvalidUsername"), ex);
                    }
                    catch (PopServerLockException ex)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorPOP3Locked"), ex);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    ResourceMonitor.RegisterTransaction(transactionId, connString, new Pop3Transaction(popClient));
                }
                else
                {
                    popClient = pop3transaction.PopClient;
                }
            }

            return popClient;
        }
    }
}
