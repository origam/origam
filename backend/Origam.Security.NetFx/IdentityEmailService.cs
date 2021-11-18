#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.Identity;

namespace Origam.Security.Identity
{
    public class IdentityEmailService : IIdentityMessageService
    {
        protected static readonly ILog log 
            = LogManager.GetLogger(typeof(IdentityEmailService));

        private string sender;

        public IdentityEmailService(string sender)
        {
            this.sender = sender;
        }

        public Task SendAsync(IdentityMessage message)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Sending email...");
            }
            MailMessage email = new MailMessage(sender, message.Destination);
            email.Subject = message.Subject;
            email.Body = message.Body;
            email.IsBodyHtml = true;
            SmtpClient smtpClient = new SmtpClient();
            return smtpClient.SendMailAsync(email);
        }
    }
}
