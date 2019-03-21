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
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;


namespace Origam.Server.Handlers
{
    public class ReportProblemHandler : IHttpHandler
    {
        public void ProcessRequest (HttpContext context) 
        {
            if (context.Request.Form.Get("description") == null)
            {
                throw new Exception("There is no message to be sent.");
            }
            string fromMail = ((NameValueCollection)System.Configuration.ConfigurationManager
                .GetSection("problemReportingSettings")).Get("fromMail");
            string toMail = ((NameValueCollection)System.Configuration.ConfigurationManager
                .GetSection("problemReportingSettings")).Get("toMail");
            MailMessage problemMailReport = new MailMessage(fromMail, toMail);
            string relatedForm = context.Request.Form.Get("relatedForm");
            if (String.IsNullOrEmpty(relatedForm))
            {
                problemMailReport.Subject = context.Request.Form.Get("description");
            }
            else
            {
                problemMailReport.Subject = String.Format(
                Resources.ReportProblemMailSubject,
                context.Request.Form.Get("relatedForm"),
                context.Request.Form.Get("description"),
                context.Request.Form.Get("build"));
            }
            problemMailReport.SubjectEncoding = Encoding.UTF8;
            string user = context.User.Identity.Name;
            if (String.IsNullOrEmpty(user))
            {
                user = "GenericUser";
            }
            string mailOpening = String.Format(
                Resources.ReportProblemMailOpening, user);
            string mailBody = String.Format(
                "{0}\n\n{1}", mailOpening, context.Request.Form.Get("commentByUser"));
            problemMailReport.Body = mailBody;
            problemMailReport.BodyEncoding = Encoding.UTF8;
            if (context.Request.Form.Get("stackTrace") != null)
            {
                AttachStackTrace(problemMailReport, context.Request.Form.Get("stackTrace"));
            }
            if (context.Request.Form.Get("screenShot") != null) 
            {
                AttachScreenShot(problemMailReport, context.Request.Form.Get("screenShot"));
            }
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Send(problemMailReport);
            problemMailReport.Dispose();
        }

        public void AttachStackTrace(MailMessage message, String stackTrace)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(stackTrace);
            streamWriter.Flush();
            memoryStream.Position = 0;
            ContentType contentType = new ContentType(MediaTypeNames.Text.Plain);
            contentType.CharSet = "utf-8";
            Attachment traceInfo = new Attachment(memoryStream, contentType);
            traceInfo.ContentDisposition.FileName = "trace.txt";
            message.Attachments.Add(traceInfo);
        }

        public void AttachScreenShot(MailMessage message, String encodedScreenShotData)
        {
            byte[] decodedScreenShotData = System.Convert.FromBase64String(encodedScreenShotData);
            MemoryStream memoryStream = new MemoryStream(decodedScreenShotData);
            ContentType contentType = new ContentType("image/png");
            Attachment screenShot = new Attachment(memoryStream, contentType);
            screenShot.ContentDisposition.FileName = "screenShot.png";
            message.Attachments.Add(screenShot);
        }
     
        public bool IsReusable 
        {
            get 
            {
                return false;
            }
        }
    }
}
