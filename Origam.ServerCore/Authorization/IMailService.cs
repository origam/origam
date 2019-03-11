using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Origam.DA;
using Origam.Security.Common;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public interface IMailService
    {
        void Send(MailMessage email);
    }

    class MailService : IMailService
    {
        public void Send(MailMessage email)
        {
            // send mail - by a workflow located at root package			
            QueryParameterCollection pms = new QueryParameterCollection();
            pms.Add(new QueryParameter("subject", email.Subject));
            pms.Add(new QueryParameter("body", email.Body));
            pms.Add(new QueryParameter("recipientEmail", email.To.First().Address));
            pms.Add(new QueryParameter("senderEmail", email.From.Address));
            if (!string.IsNullOrWhiteSpace(email.From.DisplayName))
            {
                pms.Add(new QueryParameter("senderName", email.From.DisplayName));
            }
            WorkflowService.ExecuteWorkflow(new Guid("6e6d4e02-812a-4c95-afd1-eb2428802e2b"), pms, null);
        }
    }
}