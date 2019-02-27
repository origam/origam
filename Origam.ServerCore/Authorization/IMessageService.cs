using System.Threading.Tasks;

namespace Origam.ServerCore
{
    public interface IMessageService
    {
        Task Send(string email, string subject, string message);
    }
}