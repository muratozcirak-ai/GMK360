using System.Threading.Tasks;

namespace GMK360.Core.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendTemplateSmsAsync(string phoneNumber, string templateCode, object parameters);
    }
}
