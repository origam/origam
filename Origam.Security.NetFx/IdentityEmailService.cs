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
