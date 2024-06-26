using System.Net.Mail;

namespace Origam.Mail;
class NetFxMailService: NetStandardMailService
{
    public NetFxMailService() 
    {
        
    }
    protected override void SetConfigValues(SmtpClient smtpClient)
    {
        // this is handled automatically by the framework
    }
}
