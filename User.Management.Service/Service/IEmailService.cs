

using User.Management.Service.Models;

namespace User.Management.Service.Service
{
    public interface IEmailService
    {
        void SendEmail(Message message);
    }
}
