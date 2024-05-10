using Origam.Service.Core;

namespace Origam.Mail
{
    public interface IMailService
    {
        int SendMail(IXmlContainer mailDocument, string server, int port);
        int SendMail1(IXmlContainer mailDocument, string server, int port);
        int SendMail2(MailData mailData, string server, int port);
    }
}