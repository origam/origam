using System.Net.Mail;
using System.Threading.Tasks;
using Origam.Security.Common;

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
            var mailMan = new MailMan("","");
            mailMan.SendMailByAWorkflow(email);
        }
    }
}