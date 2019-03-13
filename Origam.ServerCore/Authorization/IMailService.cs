using System.Net.Mail;
using System.Threading.Tasks;
using Origam.Security.Common;

namespace Origam.ServerCore
{
    public interface IMailService
    {
        void SendPasswordResetToken(IOrigamUser user, string token, int tokenValidityHours);
        void SendNewUserToken(IOrigamUser user, string token);
    }
}